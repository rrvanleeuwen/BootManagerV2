# Epic: First-Run Onboarding & Auth Simplification

Status: ontwerp vastgesteld. US1, US2, US3, US4, US5 gereed (2026-05-24). US6 gereed (2026-05-25). US7 vastgelegd als bugfix-story op 2026-05-26.

Doel: BootManager krijgt een simpele, robuuste single-owner eerste-start flow. Bij een lege database maakt de applicatie automatisch één bootstrap owner aan. De gebruiker logt in met een geconfigureerd bootstrap wachtwoord, wordt daarna verplicht door onboarding geleid, vult eigenaar- en bootgegevens in, en wijzigt direct het wachtwoord.

## Uitgangspunten

- Er is voorlopig maximaal één gebruiker/eigenaar.
- Meerdere gebruikers, rollenbeheer en externe identity providers zijn buiten scope.
- De huidige database mag bij deze herinrichting weg; backward-compatible migratie van bestaande owner/recovery/pincode-data is niet nodig.
- De UI moet eenvoudig worden: wachtwoord-only login, geen pincode, geen recovery/master-key flow.
- Bootgegevens worden tijdens onboarding vastgelegd, maar later wijzigen van bootgegevens is een aparte toekomstige story.
- Implementatie gebeurt later per user story via Copilot-prompts. Dit document is de bron voor die prompts.

## Niet-Doelen

- Meerdere gebruikers.
- E-mailgebaseerde wachtwoordreset.
- Recovery-code UI.
- Master-key import.
- Shutdown-knop.
- Internet-exposure of remote beheer.
- Bootgegevens wijzigen na onboarding.

## Huidige Implementatie Samenvatting

Relevante huidige onderdelen:

- `BootManager.Core/Entities/OwnerProfile.cs`
  - single-owner entity;
  - wachtwoordhash/salt;
  - optionele pincode;
  - optionele recovery code;
  - encrypted payload met naam/e-mail.
- `BootManager.Application/OwnerRegistration/Services/OwnerRegistrationService.cs`
  - detecteert first-run op basis van ontbreken van owner;
  - registreert eerste owner via vrije registratie.
- `BootManager.Application/Authentication/Services/OwnerLoginService.cs`
  - login met wachtwoord of pincode.
- `BootManager.Application/Authentication/Services/OwnerRecoveryService.cs`
  - recovery via backup-code;
  - master-key import is placeholder-achtig en moet niet in de nieuwe flow blijven.
- `BootManager.Web/Components/StartupGate.razor`
  - redirect bij first-run naar `/register-owner`;
  - redirect root naar login/dashboard.
- `BootManager.Web/Components/Pages/Login.razor`
  - toont wachtwoord, pincode en recovery-link.
- `BootManager.Web/Components/Pages/RegisterOwner.razor`
  - vrije eerste-owner registratie.
- `BootManager.Web/Components/Pages/RecoverAccess.razor`
  - recovery/master-key UI.
- `BootManager.Web/Components/Pages/Settings.razor`
  - wachtwoord wijzigen;
  - pincode instellen/verwijderen;
  - operationele instellingen.

## Gewenste Nieuwe Flow

1. Applicatie start.
2. Database wordt gemigreerd.
3. Als er geen `OwnerProfile` bestaat:
   - app maakt één bootstrap owner aan;
   - naam: `BootManager Owner`;
   - e-mail: `owner@bootmanager.local`;
   - wachtwoord: uit configuratie;
   - `PasswordChangeRequired = true`;
   - `OnboardingCompleted = false`.
4. Gebruiker logt in via `/login` met bootstrap wachtwoord.
5. Zolang setup verplicht is, redirect de app hard naar `/onboarding`.
6. Gebruiker vult onboarding in:
   - eigenaargegevens;
   - bootgegevens;
   - nieuw wachtwoord.
7. Na succesvolle onboarding:
   - wachtwoord is gewijzigd;
   - owner payload is bijgewerkt;
   - vessel profile is opgeslagen;
   - `PasswordChangeRequired = false`;
   - `OnboardingCompleted = true`;
   - redirect naar `/dashboard`.

## User Stories

### US1: Auth UI Vereenvoudigen Naar Wachtwoord-Only ✅ (2026-05-24)

**Status:** Gereed.

Realisatie:

- Pincode verdwijnt uit de UI.
- Recovery/master-key verdwijnt volledig uit de gebruikersflow.
- Login is alleen met wachtwoord.
- Settings toont geen pincodeblok.
- `/recover` is niet meer bereikbaar via normale navigatie.

Implementatie:

- **Login.razor:** pincode-veld, hint en recovery-link verwijderd. Pagina toont nu alleen wachtwoord, "ingelogd blijven" en login-knop.
- **Login.razor:** al ingelogde gebruikers worden doorgestuurd naar het dashboard.
- **Settings.razor:** pincode-card volledig verwijderd. Code-methoden `SetPin`, `HandlePinSubmit`, `ClearPin` en gerelateerde state verwijderd.
- **Settings.razor:** pagina is expliciet beschermd met owner-autorisatie.
- **Auth cookies:** niet-persistente logins krijgen een in-memory sessie-id en worden ongeldig na applicatieherstart; "ingelogd blijven" blijft persistent.
- **Acceptatiecriteria:** alle vervuld. `dotnet build` slaagt. Geen EF migration, geen bootstrap, geen onboarding.
- **Opmerkingen:** OwnerRecoveryService, RecoverAccess.razor en pincode-properties in LoginRequestDto en services blijven als technische legacy-code; deze cleanup valt buiten deze story.

Niet in deze story:

- Bootstrap owner.
- Onboarding.
- Vessel profile.
- Deployment-config.
- Opschoning services/database.

Aanbevolen implementatie-aanpak voor vervolgstories:

- Begin klein: verwijder eerst UI en navigatie naar pincode/recovery.
- Services/kolommen mogen pas later worden opgeruimd als dat de slice kleiner en veiliger houdt.

### US2: Bootstrap Owner Bij Lege Database ✅ (2026-05-24)

**Status:** Gereed.

Realisatie:

- `OwnerProfile` entiteit uitgebreid met `PasswordChangeRequired` en `OnboardingCompleted` flags.
- `IBootstrapOwnerService` implementatie zorgt voor automatische bootstrap owner aanmaak.
- Bootstrap owner aangemaakt met naam `BootManager Owner`, e-mail `owner@bootmanager.local`.
- Bootstrap wachtwoord uit configuratie `Bootstrap:DefaultPassword`.
- `PasswordChangeRequired = true`, `OnboardingCompleted = false` voor bootstrap owner.
- EF Core migration toegevoegd: `20260524183942_AddOwnerSetupFlags.cs`.
- Program.cs startup-flow aangepast: bootstrap service aangeroepen na database migratie.
- Production modus: startup faalt duidelijk als geen owner en geen `Bootstrap:DefaultPassword`.
- Development modus: fallback naar `BootManagerDev123!` if niet geconfigureerd.
- **appsettings.json:** Bevat geen `Bootstrap:DefaultPassword` (moet expliciet via environment variable, secret of deployment-config ingesteld worden).
- **appsettings.Development.json:** Mag optioneel `Bootstrap:DefaultPassword` bevatten voor development/testing.

Implementatie:

- **BootManager.Core/Entities/OwnerProfile.cs:**
  - Twee nieuwe properties: `PasswordChangeRequired` en `OnboardingCompleted` (default false).
  - Constructor en Create-methode bijgewerkt met optionele parameters.
  - Methods: `SetPasswordChangeRequired()` en `SetOnboardingCompleted()`.

- **BootManager.Application/OwnerRegistration/Services:**
  - `IBootstrapOwnerService` interface: `Task<bool> EnsureBootstrapOwnerAsync(string? bootstrapPassword, bool isProduction, CancellationToken ct)`.
  - `BootstrapOwnerService` implementatie met veilige behandeling van configuratie.
  - Production-validatie: exception if geen password.
  - Development-fallback: dev wachtwoord.

- **BootManager.Web/Program.cs:**
  - Startup-blok aangepast: bootstrap service aangeroepen na database migratie.
  - Foutafhandeling met duidelijke logging.

- **appsettings.json:**
  - Geen `Bootstrap.DefaultPassword` configuratie (Production moet dit expliciet instellen).
  - `DevAdmin` config verwijderd (vervangen door bootstrap service).

- **appsettings.Development.json:**
  - `Bootstrap.DefaultPassword` configuratie toegevoegd voor development/testing.

- **Unit tests:**
  - 6 tests in `BootManager.UnitTests/OwnerRegistration/BootstrapOwnerServiceTests.cs`.
  - Scenario's: lege DB, bestaande owner, production validatie, development fallback, flags.

Acceptatiecriteria: allemaal vervuld.
- ✅ Bij lege database ontstaat automatisch één owner.
- ✅ Owner kan inloggen met `Bootstrap:DefaultPassword`.
- ✅ Owner krijgt `PasswordChangeRequired = true`.
- ✅ Owner krijgt `OnboardingCompleted = false`.
- ✅ Geen tweede owner aangemaakt bij herstart.
- ✅ Production zonder config faalt duidelijk.
- ✅ EF migration aanwezig.
- ✅ `dotnet build` slaagt.
- ✅ Unit tests slagen.

Handmatige validatie (2026-05-24):

- Development lege database maakte precies één bootstrap owner aan.
- Login met development bootstrap-wachtwoord werkte en landde op dashboard.
- Flags na aanmaak: `PasswordChangeRequired = 1`, `OnboardingCompleted = 0`.
- Tweede start maakte geen tweede owner aan.
- Production-test moet lokaal met `dotnet run --no-launch-profile` worden uitgevoerd, omdat launch profiles Development forceren.
- Production zonder `Bootstrap__DefaultPassword` en zonder owner faalde duidelijk met ontbrekende `Bootstrap:DefaultPassword`.
- Production met expliciete `Bootstrap__DefaultPassword` startte en login werkte.
- Production met bestaande owner en zonder bootstrap password startte en maakte geen tweede owner.

Aandachtspunt buiten US2:

- Tijdens Production-login op dashboard gaf `BootManager.Web.styles.css` een 404 en Blazor toonde "An unhandled error has occurred". Behandel dit later als apart Production/static asset issue als het opnieuw relevant wordt.

Niet in deze story:

- Onboarding UI.
- Vessel profile.
- Pincode/recovery cleanup buiten wat nodig is voor bootstrap.

Configuratie-afspraak:

```json
"Bootstrap": {
  "DefaultPassword": "..."
}
```

- **Production**: Moet expliciet ingesteld worden via environment variable, Docker secret of Azure Key Vault (niet in appsettings.json).
- **Development**: Fallback naar `BootManagerDev123!` in BootstrapOwnerService als niet geconfigureerd; optioneel in appsettings.Development.json.
- Startup faalt duidelijk als Production mode en geen owner en geen DefaultPassword geconfigureerd.

Docker/deployment gebruikt later:

```bash
# Environment variable
export BOOTMANAGER_BOOTSTRAP_PASSWORD=your-secure-password

# Of via Docker secret/Azure Key Vault
```

### US3: Onboarding-Gate Afdwingen ✅ (2026-05-24)

**Status:** Gereed.

Realisatie:

- `IOwnerSetupStateService` interface met methode `GetSetupStateAsync()`.
- `OwnerSetupStateDto` DTO met properties: `HasOwner`, `PasswordChangeRequired`, `OnboardingCompleted`, `SetupRequired`.
- `OwnerSetupStateService` implementatie leest OwnerProfile via repository en vult DTO.
  - `SetupRequired` is true als `!HasOwner || PasswordChangeRequired || !OnboardingCompleted`.
- `OnboardingGate.razor` component controleert setup-status na authenticatie.
  - Redirect naar `/onboarding` als `SetupRequired=true` en huidige route niet in whitelist.
  - Redirect naar `/dashboard` als `SetupRequired=false` en huidige route == `/onboarding`.
  - Whitelist: `/login`, `/logout`, `/onboarding`, `/health`.
  - Anonieme gebruikers passeren gate; AuthorizeRouteView handelt ingang af.
- Routes.razor aangepast: OnboardingGate toegevoegd na StartupGate in `<Found>` block.
- `/onboarding` minimale placeholder-pagina aangemaakt met `@attribute [Authorize(Roles="Owner")]`.
  - Toont korte tekst dat onboarding moet worden voltooid.
  - Geen formulier (wordt US4).
- Dependency injection: `IOwnerSetupStateService` geregistreerd in `BootManager.Application/DependencyInjection.cs`.
- Unit tests: 5 tests in `BootManager.UnitTests/OwnerRegistration/OwnerSetupStateServiceTests.cs`.
  - Scenario's: no owner, SetupRequired when PasswordChangeRequired, SetupRequired when OnboardingCompleted=false, SetupNotRequired when both flags true.
  - Alle 5 tests slagen.

**Acceptatiecriteria:** allemaal vervuld.
- ✅ Er is een setup-state service `IOwnerSetupStateService`.
- ✅ Service retourneert `HasOwner`, `PasswordChangeRequired`, `OnboardingCompleted`, `SetupRequired`.
- ✅ Ingelogde user met `SetupRequired=true` wordt naar `/onboarding` gestuurd.
- ✅ Dashboard, settings, logboek niet bereikbaar vóór setup klaar is.
- ✅ `/onboarding` bereikbaar voor ingelogde owner met setup required.
- ✅ Anonieme user krijgt login-flow.
- ✅ Setup-klaar user wordt van `/onboarding` naar `/dashboard` geleid.
- ✅ Geen redirect-loop.
- ✅ `dotnet build` slaagt.
- ✅ Unit tests slagen.

Handmatige validatie (2026-05-24):

- Bestaande development-owner met `OnboardingCompleted = false` werd na login naar `/onboarding` geleid.
- Handmatig openen van `/dashboard`, `/settings` en `/logbook` stuurde terug naar `/onboarding`.
- Anoniem openen van `/onboarding` leidde naar login.
- Na tijdelijk zetten van `PasswordChangeRequired = false` en `OnboardingCompleted = true` stuurde `/onboarding` door naar `/dashboard`.
- Met setup-klaar flags bleven `/settings` en `/logbook` normaal bereikbaar.
- Opmerking: bij client-side navigatie naar geblokkeerde routes kan de doelpagina kort zichtbaar zijn voordat de gate terugstuurt. Functioneel blokkeert de gate de route; UX/security-hardening kan later server-side of route-level worden aangescherpt.
- Testdatabase-notitie: lokale development-database is na validatie setup-klaar gezet (`PasswordChangeRequired = 0`, `OnboardingCompleted = 1`). Voor US4-test kan tijdelijk teruggezet worden naar `1,0` of een verse bootstrap-database worden gebruikt.

Implementatie details:

- **BootManager.Application/OwnerRegistration/DTOs/OwnerSetupStateDto.cs** - nieuw
- **BootManager.Application/OwnerRegistration/Services/IOwnerSetupStateService.cs** - nieuw
- **BootManager.Application/OwnerRegistration/Services/OwnerSetupStateService.cs** - nieuw
- **BootManager.Web/Components/OnboardingGate.razor** - nieuw
- **BootManager.Web/Components/Pages/Onboarding.razor** - nieuw
- **BootManager.Web/Components/Routes.razor** - aangepast (OnboardingGate toevoegen)
- **BootManager.Application/DependencyInjection.cs** - aangepast (service registratie)
- **BootManager.UnitTests/OwnerRegistration/OwnerSetupStateServiceTests.cs** - 5 new tests

Routes vóór onboarding toegestaan:
- `/login` - anoniem/ingelogd
- `/logout` - anoniem/ingelogd
- `/onboarding` - alleen ingelogd
- `/health` - anoniem/ingelogd

Volgende stap: US4 implementeert het volledige onboardingformulier voor eigenaar-, boot- en wachtwoordgegevens.

### US4: Onboardingformulier Voor Eigenaar En Boot ✅ (2026-05-24)

**Status:** Gereed.

Realisatie:

- `/onboarding` pagina vervangen door volledig formulier met drie secties
- Eigenaargegevens sectie: Naam (verplicht), E-mail (optioneel)
- Bootgegevens sectie: Bootnaam (verplicht), Thuishaven (optioneel), Roepnaam (optioneel), MMSI (optioneel)
- Wachtwoordwijziging sectie: Huidig (verplicht), Nieuw (verplicht, 8+ chars), Bevestiging (verplicht, moet gelijk)
- `IOnboardingService` interface met methode `CompleteInitialOnboardingAsync(CompleteOnboardingRequestDto request, CancellationToken ct)`
- `OnboardingService` implementatie met volledige validatie logica:
  - Verplichte velden: eigenaarsnaam, bootnaam, huidig/nieuw wachtwoord
  - Wachtwoord minimaal 8 tekens
  - Nieuw wachtwoord ≠ huidig wachtwoord
  - Wachtwoord en bevestiging moeten gelijk zijn
  - Huidig wachtwoord verificatie tegen OwnerProfile hash
- Serviceflow: password verify → vessel get-or-create via `IVesselProfileService.GetOrCreateVesselProfileAsync()` → vessel update via `IVesselProfileService.UpdateVesselProfileAsync()` → owner payload encrypt → password update → flags zetten → redirect
- DTOs:
  - `CompleteOnboardingRequestDto`: alle velden (owner/vessel/password)
  - `CompleteOnboardingResponseDto`: success, error message, updated vessel profile, updated owner name/email
- Dependency injection: `IOnboardingService` geregistreerd als Scoped in `BootManager.Application/DependencyInjection.cs`
- Blazor component `/onboarding` met:
  - Three card sections (Eigenaar, Boot, Wachtwoord) met labels en hints
  - Form submission handler met loading state
  - Error message display
  - Success message met auto-redirect naar `/dashboard`
  - Logout button als fallback
  - Responsive layout (col-md-8 form, col-md-4 info panel)
- Error handling: exceptions caught, returned as failure response met bericht
- Unit tests: 9 tests in `BootManager.UnitTests/OwnerRegistration/OnboardingServiceTests.cs`
  - Scenario's: valid submission, missing owner name, missing vessel name, password too short, password mismatch, incorrect current password, new password same as old, no owner found, optional fields empty
  - Alle 9 tests slagen
- Build slaagt; gerichte onboardingtests slagen

Aanvullende validatie en fix (2026-05-25):

- Verse-database runtime-test uitgevoerd door bestaande `BootManager.Web\bootmanager.db` tijdelijk te hernoemen en de app een nieuwe SQLite database te laten maken.
- Bootstrap login met `BootManager123!` leidde correct naar `/onboarding`.
- Formulier succesvol ingevuld en opgeslagen; gebruiker werd naar `/dashboard` geleid.
- Oud bootstrap-wachtwoord werd ongeldig; nieuw gekozen wachtwoord werkte.
- Handmatig navigeren naar `/onboarding` na afronding redirectte terug naar `/dashboard`.
- SQLite-controle bevestigde `PasswordChangeRequired = 0`, `OnboardingCompleted = 1` en zichtbare bootgegevens in `VesselProfiles`.
- Opgeloste opslagbug: onboarding faalde in een verse database als nog geen `VesselProfile` bestond, omdat `UpdateVesselProfileAsync()` een bestaand profiel verwacht. `OnboardingService` roept nu eerst `GetOrCreateVesselProfileAsync()` aan en werkt daarna het profiel bij.

**Implementatie details:**

- **BootManager.Application/OwnerRegistration/DTOs/CompleteOnboardingRequestDto.cs** - nieuw
  - Properties: OwnerName, OwnerEmail, VesselName, HomePort, CallSign, Mmsi, CurrentPassword, NewPassword, ConfirmNewPassword
  - Dutch XML-documentatie

- **BootManager.Application/OwnerRegistration/DTOs/CompleteOnboardingResponseDto.cs** - nieuw
  - Properties: Success, ErrorMessage, UpdatedVesselProfile, UpdatedOwnerName, UpdatedOwnerEmail
  - Dutch XML-documentatie

- **BootManager.Application/OwnerRegistration/Services/IOnboardingService.cs** - nieuw
  - Interface met methode `CompleteInitialOnboardingAsync(request, ct)`
  - Dutch XML-documentatie

- **BootManager.Application/OwnerRegistration/Services/OnboardingService.cs** - nieuw
  - Service implementatie met validatie en serviceflow
  - Afhankelijkheden: `IRepository<OwnerProfile>`, `IPasswordHasher`, `IEncryptionService`, `ISystemClock`, `IVesselProfileService`, `ILogger`
  - Maakt het singleton bootprofiel aan als dit nog niet bestaat voordat de vessel update wordt uitgevoerd
  - Catch en handle exceptions (ArgumentException, UnauthorizedAccessException, InvalidOperationException)
  - Dutch logging en error messages
  - Private helper `ValidateRequest()` voor invoervalidatie

- **BootManager.Web/Components/Pages/Onboarding.razor** - aangepast
  - Replaced placeholder met volledige formulier
  - Three card sections met form fields en hints
  - Submit button met loading state en spinner
  - Form submit gebruikt Blazor `preventDefault`, zodat de submit volledig via de componenthandler loopt
  - Error/success message display
  - Logout button
  - Form field labels met required markers (`*`) en optional markers
  - Responsive Bootstrap layout
  - Injected `IOnboardingService` en `NavigationManager`
  - Code-behind met `HandleSubmit()` en `HandleLogout()` methods
  - `@using` directives voor DTOs

- **BootManager.Application/DependencyInjection.cs** - aangepast
  - `services.AddScoped<IOnboardingService, OnboardingService>();` toegevoegd

**Acceptatiecriteria:** allemaal vervuld.
- ✅ `/onboarding` toont volledige formulier voor ingelogde owner met setup required
- ✅ Opslaan faalt bij ontbrekende eigenaarsnaam
- ✅ Opslaan faalt bij ontbrekende bootnaam
- ✅ Opslaan faalt bij onjuist huidig wachtwoord
- ✅ Opslaan faalt bij nieuw wachtwoord korter dan 8 tekens
- ✅ Opslaan faalt bij mismatch nieuw wachtwoord en bevestiging
- ✅ Opslaan faalt bij nieuw wachtwoord gelijk aan huidig
- ✅ Bij succes: owner naam/e-mail bijgewerkt, wachtwoord gewijzigd, vessel profile opgeslagen, flags gezet, redirect naar `/dashboard`
- ✅ Dashboard/settings/logbook bereikbaar na onboarding compleet
- ✅ `dotnet build` slaagt
- ✅ 9 unit tests slagen

### US5: VesselProfile Introduceren ✅ (2026-05-24)

**Status:** Gereed.

Realisatie:

- Nieuwe singleton entity `VesselProfile` voor bootgegevens per installatie.
- EF Core configuratie met tabel `VesselProfiles`, constraints en index.
- DTOs:
  - `VesselProfileDto` (immutable record, alle velden, lees-output).
  - `UpdateVesselProfileRequestDto` (immutable record, voor updates).
- Service interface `IVesselProfileService`:
  - `GetOrCreateVesselProfileAsync()`: haalt bestaand profiel op of maakt leeg profiel aan met standaard bootnaam "Unnamed Vessel".
  - `UpdateVesselProfileAsync(UpdateVesselProfileRequestDto)`: werkt profiel bij met validatie.
- Service implementatie `VesselProfileService`:
  - Validates VesselName: verplicht, max 128 tekens.
  - Validates HomePort: optioneel, max 128 tekens.
  - Validates CallSign: optioneel, max 64 tekens.
  - Validates Mmsi: optioneel, max 32 tekens.
  - Singleton semantiek: maximaal 1 record per installatie (gehandhaafd via service logica, geen database constraint).
  - Gebruikt `IRepository<VesselProfile>`, `ISystemClock` en logging.
- Dependency injection: `IVesselProfileService` geregistreerd als Scoped in `BootManager.Application/DependencyInjection.cs`.
- EF Core migration: `20260524201623_AddVesselProfile.cs` met VesselProfiles table, alle velden, maxLength constraints en index.
- Unit tests: 11 tests in `BootManager.UnitTests/VesselProfile/VesselProfileServiceTests.cs`.
  - Scenario's: auto-create lege profiel, fetch bestaand profiel, update met validatie, optional fields, max length violations, errors.
  - Alle 11 tests slagen.

**Implementatie details:**

- **BootManager.Core/Entities/VesselProfile.cs** - nieuw
  - Entity met properties: Id, VesselName, HomePort, CallSign, Mmsi, CreatedUtc, UpdatedUtc.
  - Factory method `Create()` en `Update()` method.
  - Volledig Nederlands XML-commentaar.

- **BootManager.Infrastructure/Persistence/Configurations/VesselProfileConfiguration.cs** - nieuw
  - EF IEntityTypeConfiguration implementatie.
  - Tabel, keys, property constraints, index.

- **BootManager.Infrastructure/Persistence/BootManagerDbContext.cs** - aangepast
  - `DbSet<VesselProfile> VesselProfiles` toegevoegd.
  - VesselProfileConfiguration geregistreerd in OnModelCreating.

- **BootManager.Application/VesselProfile/DTOs/VesselProfileDto.cs** - nieuw
- **BootManager.Application/VesselProfile/DTOs/UpdateVesselProfileRequestDto.cs** - nieuw
- **BootManager.Application/VesselProfile/Services/IVesselProfileService.cs** - nieuw
- **BootManager.Application/VesselProfile/Services/VesselProfileService.cs** - nieuw

- **BootManager.Application/DependencyInjection.cs** - aangepast
  - `IVesselProfileService` registratie toegevoegd.

- **BootManager.Infrastructure/Migrations/20260524201623_AddVesselProfile.cs** - nieuw
- **BootManager.Infrastructure/Migrations/20260524201623_AddVesselProfile.Designer.cs** - nieuw (auto-gegenereerd)

- **BootManager.UnitTests/VesselProfile/VesselProfileServiceTests.cs** - nieuw
  - 11 comprehensive tests: auto-create, fetch, update, validation, optional fields, max lengths, errors.

**Acceptatiecriteria:** allemaal vervuld.
- ✅ Nieuwe entity `VesselProfile` met alle vereiste velden (Id, VesselName, HomePort, CallSign, Mmsi, CreatedUtc, UpdatedUtc).
- ✅ Nieuwe EF configuratie met verplichting en max length constraints.
- ✅ `DbSet<VesselProfile>` in BootManagerDbContext.
- ✅ EF migration gegenereerd.
- ✅ DTOs en service toegevoegd.
- ✅ Service kan bootprofiel ophalen (GetOrCreateVesselProfileAsync).
- ✅ Service maakt leeg profiel aan als geen bestaat (singleton-eerste-load).
- ✅ Service werkt bestaand profiel bij (UpdateVesselProfileAsync).
- ✅ Singleton semantiek bewaard via service logica.
- ✅ Validatie faalt bij lege/null VesselName.
- ✅ Validatie faalt bij te lange velden (VesselName 128, HomePort 128, CallSign 64, Mmsi 32).
- ✅ Optionele velden mogen null/leeg zijn.
- ✅ Geen UI-wijzigingen.
- ✅ `dotnet build` slaagt.
- ✅ Alle 11 unit tests slagen.

Handmatige minimale validatie (2026-05-24):

- Development-start van `BootManager.Web` verliep zonder fout.
- EF migration werd toegepast op de lokale SQLite database.
- `sqlite3 BootManager.Web\bootmanager.db ".tables"` toonde de nieuwe tabel `VesselProfiles`.

**Notities voor US4:**

- US4 kan nu `IVesselProfileService` injecteren in het onboardingformulier.
- US4 roept `GetOrCreateVesselProfileAsync()` aan bij pagina-load.
- US4 roept `UpdateVesselProfileAsync(request)` aan bij formulieropslag met de bootgegevens van de gebruiker.
- Service handelt singleton-semantiek en validatie af; Razor-pagina kan dun blijven.
- Geen wijziging aan `/onboarding` placeholder in deze story; UI volgt in US4.

Geen wijzigingen aan:
- Owner flags (PasswordChangeRequired, OnboardingCompleted): dat gebeurt in US4 als geheel opboarding compleet is.
- Settings-pagina (bootgegevens wijzigen na onboarding): aparte toekomstige story.
- Meerdere boten: niet ondersteund; singleton per installatie.
- NMEA, ingest, logboek, Docker, Raspberry Pi: onveranderd.

### US6: Documentatie En Deployment-Config Bijwerken ✅ (2026-05-25)

**Status:** Gereed.

Te wijzigen:

- `.env.example` ✅
- `docker-compose.yml` ✅
- `.docs/raspberry-pi-deployment.md` ✅
- `.docs/docker-deployment.md` ✅
- `.docs/pi-first-install-runbook.md` ✅
- `.docs/TODO.md` ✅

Docker config:

```text
BOOTMANAGER_BOOTSTRAP_PASSWORD=replace-with-first-login-password
```

Compose:

```yaml
- Bootstrap__DefaultPassword=${BOOTMANAGER_BOOTSTRAP_PASSWORD:?Set BOOTMANAGER_BOOTSTRAP_PASSWORD in .env}
```

Docs moeten uitleggen:

- Eerste start maakt bootstrap owner. ✅
- Eerste login gebruikt bootstrap wachtwoord. ✅
- Onboarding is verplicht. ✅
- Gebruiker kiest direct nieuw wachtwoord. ✅
- Geen pincode/recovery/master-key UI. ✅
- Als gebruiker niet meer kan inloggen: operationele factory-reset procedure via database backup/hernoemen/verwijderen. ✅
- Bootgegevens wijzigen is later. ✅

Acceptatiecriteria:

- ✅ Deployment-config bevat bootstrap password env var.
- ✅ Development-config ondersteunt lokale flow.
- ✅ Runbook beschrijft eerste login en onboarding.
- ✅ Docs beschrijven reset bij vergeten wachtwoord.
- ✅ `dotnet build` slaagt.

### US7: Legacy Register Owner Route En Menu Verwijderen

**Status:** Vastgelegd op 2026-05-26 naar aanleiding van Raspberry Pi test vóór eerste login/onboarding. Nog niet geïmplementeerd.

**User story:** Als eigenaar die BootManager voor het eerst opstart wil ik geen oude "Register Owner"-route of menuoptie meer zien, zodat de eerste-start flow uitsluitend via bootstrap login en verplichte onboarding loopt.

**Aanleiding:**

Tijdens de Raspberry Pi test, voordat er was ingelogd en voordat onboarding was uitgevoerd, stond in het menu nog een item **Register Owner**. Klikken daarop navigeerde naar `/register-owner`. Dat hoort niet meer bij de huidige BootManagerV2-flow.

Legacy/context:

- Legacy `US0.2 Registratie eerste eigenaar` is in BootManagerV2 vervangen door bootstrap owner + verplichte onboarding.
- Vrije eerste-owner registratie via `/register-owner` hoort niet meer beschikbaar te zijn.
- Multi-user/rollenregistratie blijft geparkeerd.

**Scope:**

- Verwijder de zichtbare menuoptie "Register Owner" uit de navigatie.
- Zorg dat `/register-owner` niet meer bereikbaar is als normale gebruikersroute.
- Verwijder of neutraliseer de legacy `RegisterOwner.razor` pagina.
- Verwijder de oude `StartupGate` redirect naar `/register-owner`; first-run hoort via bootstrap owner + `/login` + `/onboarding` te lopen.
- Controleer dat de bestaande bootstrap/onboarding flow intact blijft.
- Voeg of actualiseer tests waar passend, bijvoorbeeld voor route-/gate-gedrag of menuverwachting.
- Werk documentatie bij als codeverwijzingen naar `/register-owner` wijzigen.

**Buiten scope:**

- Geen wijziging aan bootstrap owner aanmaak.
- Geen wijziging aan `BOOTMANAGER_BOOTSTRAP_PASSWORD`.
- Geen nieuwe registratieflow.
- Geen multi-user, rollen of crew accounts.
- Geen opruiming van alle technische legacy services/DTOs tenzij strikt nodig om de route veilig te verwijderen.
- Geen wijziging aan Pi database reset story.

**Acceptatiecriteria:**

- Het menu toont geen "Register Owner" item meer.
- Handmatig openen van `/register-owner` toont geen oude registratiepagina meer.
- Bij lege database maakt BootManager nog steeds automatisch één bootstrap owner aan.
- Login met bootstrap wachtwoord leidt nog steeds naar verplichte `/onboarding`.
- Onboarding afronden leidt nog steeds naar `/dashboard`.
- Auth/login routes blijven werken.
- `dotnet build` slaagt.
- Relevante unit/componenttests slagen of worden bijgewerkt.

**Legacy coverage impact:**

- `US0.2 Registratie eerste eigenaar` blijft `Replaced`; deze story verwijdert resterende UI/route-restanten van de oude registratieaanpak.
- `US0.3 Inloggen als eigenaar` blijft `Done`; login blijft wachtwoord-only.
- `US1.3` en verdere multi-user stories blijven `Parked`.

**Handmatige testnotities:**

- Test bij voorkeur op een verse of geresette database.
- Vóór login: controleer dat "Register Owner" niet in het menu staat.
- Open `/register-owner` handmatig en controleer dat de oude registratiepagina niet bruikbaar is.
- Log in met `BOOTMANAGER_BOOTSTRAP_PASSWORD`.
- Controleer dat `/onboarding` verplicht opent.
- Rond onboarding af en controleer dat dashboard/settings/logboek normaal bereikbaar zijn.

## Operationele Resetprocedure Bij Vergeten Wachtwoord

Voor deze epic bouwen we geen in-app recovery.

Als de enige gebruiker niet meer kan inloggen:

1. Zorg voor fysieke/admin toegang tot de Pi.
2. Stop containers/app.
3. Maak eventueel backup van de SQLite database.
4. Hernoem of verwijder de SQLite database.
5. Start app opnieuw.
6. App komt opnieuw in bootstrap/onboarding flow.

Later kan een aparte story een nettere factory-reset of owner-reset command toevoegen.

## Aanbevolen Implementatievolgorde

1. ✅ US1: auth UI vereenvoudigen naar wachtwoord-only. (2026-05-24)
2. ✅ US2: bootstrap owner + owner setup flags. (2026-05-24)
3. ✅ US3: onboarding gate. (2026-05-24)
4. ✅ US5: vessel profile datalaag. (2026-05-24)
5. ✅ US4: onboardingformulier dat owner + vessel + wachtwoord afrondt. (2026-05-24)
6. ✅ US6: docs/deployment-config. (2026-05-25)
7. 🔜 US7: legacy Register Owner route en menu verwijderen. (vastgelegd 2026-05-26)

US4 hangt af van US5 voor opslag van bootgegevens. Daarom is het praktisch om US5 vóór of samen met US4 te implementeren, maar de user-facing flow blijft US4.

Alle core user stories zijn nu voltooid. De onboarding-flow is operationeel en helpt de eindgebruiker door de eerste-start setup. US6 heeft de documentatie en deployment-config bijgewerkt voor Docker, Raspberry Pi en eerste installatie. US7 is een kleine bugfix-story om resterende legacy registratie-UI te verwijderen.

## Volgende Keer Hier Starten

De First-Run Onboarding & Auth Simplification epic is deploymentklaar. De eerste Raspberry Pi 4 Docker Compose deployment-smoke-test is op 2026-05-26 geslaagd met lokale `.env`, ARM64 Docker build, Web healthcheck, draaiende Ingest-container en geslaagde reboot-test.

Open voor deze epic: US7 uitvoeren, omdat de Raspberry Pi test aantoonde dat de oude `/register-owner` flow nog via het menu zichtbaar is. Daarna ligt een logische vervolgstap buiten deze epic, bijvoorbeeld `SYS-RESET-1`, de echte boot UDP-broadcasttest met YDEN-03 of een volgende roadmap-story.
