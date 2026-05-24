# Epic: First-Run Onboarding & Auth Simplification

Status: ontwerp vastgesteld, nog niet geïmplementeerd.

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

### US3: Onboarding-Gate Afdwingen

Besluit:

- Hard redirect naar `/onboarding`.
- Setup is verplicht als:
  - `PasswordChangeRequired = true`; of
  - `OnboardingCompleted = false`.
- Ingelogde gebruiker mag vóór onboarding niet naar dashboard/settings/logboek/andere app-pagina's.
- Toegestaan vóór onboarding:
  - `/login`;
  - `/logout`;
  - `/onboarding`;
  - `/health`.
- Als onboarding klaar is en gebruiker opent `/onboarding`, redirect naar `/dashboard`.
- Voeg bij voorkeur een kleine setup-state service toe zodat Razor niet direct `OwnerProfile` hoeft te kennen.

Acceptatiecriteria:

- Er is een setup-state service, bijvoorbeeld `IOwnerSetupStateService`.
- Service retourneert minimaal:
  - `HasOwner`;
  - `PasswordChangeRequired`;
  - `OnboardingCompleted`;
  - `SetupRequired`.
- Ingelogde gebruiker met `SetupRequired = true` wordt naar `/onboarding` gestuurd.
- Beschermde routes zijn niet bereikbaar vóór onboarding klaar is.
- Setup-klaar gebruikers kunnen normaal navigeren.
- `dotnet build` slaagt.

Niet in deze story:

- Volledig onboardingformulier, tenzij een minimale placeholder nodig is om routing testbaar te houden.

### US4: Onboardingformulier Voor Eigenaar En Boot

Besluit:

- Eén pagina: `/onboarding`.
- Alleen bruikbaar na login.
- Verplicht zolang setup niet klaar is.

Velden:

Eigenaar:

- Naam: verplicht.
- E-mail: optioneel.

Boot:

- Bootnaam: verplicht.
- Thuishaven: optioneel.
- Roepnaam: optioneel.
- MMSI: optioneel.

Wachtwoord:

- Huidig/bootstrap wachtwoord: verplicht.
- Nieuw wachtwoord: verplicht.
- Bevestig nieuw wachtwoord: verplicht.
- Nieuw wachtwoord minimaal 8 tekens.
- Nieuw wachtwoord mag niet gelijk zijn aan huidig/bootstrap wachtwoord.

Na succesvol opslaan:

- Owner naam/e-mail opgeslagen.
- Vessel profile opgeslagen.
- Wachtwoord gewijzigd.
- `PasswordChangeRequired = false`.
- `OnboardingCompleted = true`.
- Redirect naar `/dashboard`.

Aanbevolen service:

- `IOnboardingService`.
- Methode: `CompleteInitialOnboardingAsync(CompleteOnboardingRequestDto request)`.
- Validatie in application service.
- Razor-pagina blijft dun.

Acceptatiecriteria:

- `/onboarding` bestaat.
- Opslaan faalt bij onjuist huidig wachtwoord.
- Opslaan faalt als nieuw wachtwoord gelijk is aan huidig wachtwoord.
- Opslaan faalt als verplichte velden ontbreken.
- Bij succes zijn owner, vessel profile en setup flags bijgewerkt.
- Daarna blijft gebruiker niet in onboarding hangen.
- `dotnet build` slaagt.

### US5: VesselProfile Introduceren

Besluit:

- Nieuwe singleton entity voor bootgegevens.
- Wordt aangemaakt/gevuld tijdens onboarding.
- Geen Settings-blok of editpagina voor bootgegevens in deze epic.
- Boot wijzigen komt later als aparte story.

Voorgestelde entity:

```text
VesselProfile
- Id Guid
- VesselName string required max 128
- HomePort string? max 128
- CallSign string? max 64
- Mmsi string? max 32
- CreatedUtc DateTime
- UpdatedUtc DateTime?
```

Waarom apart:

- `OperationalSettings` is technisch/operationeel.
- `LogbookTrip` is per reis.
- `VesselProfile` beschrijft de boot bij deze installatie.

Acceptatiecriteria:

- Nieuwe entity `VesselProfile`.
- Nieuwe EF configuratie.
- `DbSet<VesselProfile>`.
- EF migration.
- DTO's en service toegevoegd.
- Onboarding kan vessel profile opslaan.
- Geen ondersteuning voor meerdere boten.
- `dotnet build` slaagt.

### US6: Documentatie En Deployment-Config Bijwerken

Besluit:

- Deze story hoort aan het einde van de epic, zodra de codeflow vastligt.

Te wijzigen:

- `.env.example`;
- `docker-compose.yml`;
- `.docs/raspberry-pi-deployment.md`;
- `.docs/docker-deployment.md`;
- `.docs/pi-first-install-runbook.md`;
- eventueel `.docs/TODO.md`.

Docker config:

```text
BOOTMANAGER_BOOTSTRAP_PASSWORD=replace-with-first-login-password
```

Compose:

```yaml
- Bootstrap__DefaultPassword=${BOOTMANAGER_BOOTSTRAP_PASSWORD:?Set BOOTMANAGER_BOOTSTRAP_PASSWORD in .env}
```

Docs moeten uitleggen:

- Eerste start maakt bootstrap owner.
- Eerste login gebruikt bootstrap wachtwoord.
- Onboarding is verplicht.
- Gebruiker kiest direct nieuw wachtwoord.
- Geen pincode/recovery/master-key UI.
- Als gebruiker niet meer kan inloggen: operationele factory-reset procedure via database backup/hernoemen/verwijderen.
- Bootgegevens wijzigen is later.

Acceptatiecriteria:

- Deployment-config bevat bootstrap password env var.
- Development-config ondersteunt lokale flow.
- Runbook beschrijft eerste login en onboarding.
- Docs beschrijven reset bij vergeten wachtwoord.
- `dotnet build` slaagt.

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

1. US1: auth UI vereenvoudigen naar wachtwoord-only.
2. US2: bootstrap owner + owner setup flags.
3. US3: onboarding gate.
4. US5: vessel profile datalaag.
5. US4: onboardingformulier dat owner + vessel + wachtwoord afrondt.
6. US6: docs/deployment-config.

US4 hangt af van US5 voor opslag van bootgegevens. Daarom is het praktisch om US5 vóór of samen met US4 te implementeren, maar de user-facing flow blijft US4.

## Volgende Keer Hier Starten

Start met US1.

Voorgestelde branch:

```text
feature/auth-simplify-password-only
```

Bespreek voor de prompt nog één keer:

- Gaan we `/recover` volledig verwijderen of alleen route onbereikbaar maken?
- Laten we pincode/recovery serviceklassen tijdelijk bestaan als dode code, of ruimen we ze meteen op?

Aanbevolen Copilot-richting voor US1:

- Scope klein houden.
- Geen migration.
- Geen bootstrap.
- Geen onboarding.
- Alleen UI/navigatie vereenvoudigen naar wachtwoord-only en recovery/pincode uit de zichtbare flow verwijderen.
- `dotnet build` draaien.
