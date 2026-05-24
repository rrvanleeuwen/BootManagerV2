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

### US1: Auth UI Vereenvoudigen Naar Wachtwoord-Only

Besluit:

- Pincode verdwijnt uit de UI.
- Recovery/master-key verdwijnt volledig uit de gebruikersflow.
- Login is alleen met wachtwoord.
- Settings toont geen pincodeblok.
- `/recover` mag verwijderd worden of minstens niet meer bereikbaar/gelinkt zijn.
- Oude database hoeft niet behouden te blijven; pincode/recovery backward compatibility is niet nodig.

Acceptatiecriteria:

- Loginpagina toont alleen wachtwoord, "ingelogd blijven" en login-knop.
- Link "Wachtwoord vergeten / Herstel toegang" is weg.
- Settingspagina toont geen pincode instellen/verwijderen meer.
- Recovery/master-key pagina is niet meer zichtbaar of bereikbaar in normale flow.
- `OwnerLoginService` wordt in de UI-flow alleen met wachtwoord gebruikt.
- `dotnet build` slaagt.

Niet in deze story:

- Bootstrap owner.
- Onboarding.
- Vessel profile.
- Deployment-config.

Aanbevolen implementatie-aanpak:

- Begin klein: verwijder eerst UI en navigatie naar pincode/recovery.
- Services/kolommen mogen pas later worden opgeruimd als dat de slice kleiner en veiliger houdt.

### US2: Bootstrap Owner Bij Lege Database

Besluit:

- Als er geen owner bestaat, maakt de app automatisch één bootstrap owner aan.
- Bootstrap naam: `BootManager Owner`.
- Bootstrap e-mail: `owner@bootmanager.local`.
- Bootstrap wachtwoord komt uit configuratie: `Bootstrap:DefaultPassword`.
- In Production faalt startup duidelijk als er geen owner bestaat en `Bootstrap:DefaultPassword` ontbreekt.
- Development mag een lokale fallback/configwaarde hebben.
- Nieuwe velden op `OwnerProfile`:
  - `PasswordChangeRequired bool`;
  - `OnboardingCompleted bool`.

Acceptatiecriteria:

- Bij lege database ontstaat automatisch één owner.
- Owner kan inloggen met het geconfigureerde bootstrap wachtwoord.
- Owner krijgt `PasswordChangeRequired = true`.
- Owner krijgt `OnboardingCompleted = false`.
- Er wordt nooit een tweede owner aangemaakt.
- Production zonder bootstrap wachtwoord faalt duidelijk als er nog geen owner bestaat.
- EF migration toegevoegd voor de nieuwe owner flags.
- `dotnet build` slaagt.

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

Docker/deployment gebruikt later:

```text
BOOTMANAGER_BOOTSTRAP_PASSWORD=...
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
