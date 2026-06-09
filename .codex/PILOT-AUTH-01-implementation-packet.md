# Implementation Packet

## Task

- Story ID: `PILOT-AUTH-01`
- Approved story: lokale Owner- en Crew-accounts
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-AUTH-01`
- Goal: migreer de bestaande single-Owner-authenticatie naar een uniform lokaal
  Owner/Crew-model en lever de goedgekeurde login-, account- en beheerflow.

De story is al goedgekeurd. Formuleer haar niet opnieuw en vraag geen nieuw akkoord.
Geef een kort uitvoeringsplan, implementeer direct, voer de checks uit en rapporteer
volgens `Completion Notes`.

## Scope

- Introduceer `LocalUser` als uniforme authenticatie-entiteit en `LocalUserRole` met
  uitsluitend `Owner` en `Crew`.
- Behoud bij migratie de bestaande Owner-id, wachtwoordhash, versleutelde
  profielpayload, encryption version, timestamps en onboardingflags.
- Voeg aan de gebruiker een leesbare unieke `DisplayName`, genormaliseerde naam,
  `IsActive` en oplopende `CredentialVersion` toe.
- Migreer `OwnerProfiles` naar de nieuwe tabel/entiteit. Het gemigreerde record krijgt
  rol `Owner`, actief `true` en credentialversie `1`.
- Backfill de gemigreerde Owner-accountnaam vanuit `Name` in de bestaande versleutelde
  payload via een gerichte startup/application-service na de EF-migratie. Gebruik
  `Owner` als veilige fallback wanneer decryptie of naam ontbreekt. Werk de
  genormaliseerde naam en unieke index consistent bij.
- Generaliseer login naar `UserId + Password + RememberMe`. Lever een anonieme,
  read-only lijst van uitsluitend actieve gebruikers met `Id` en `DisplayName` voor de
  accountselector.
- Laat loginresultaat en cookie-/JWT-claims de werkelijke user-id, displaynaam, rol en
  credentialversie bevatten. Verwijder hardcoded Owner-claims.
- Valideer bij ieder cookie- en JWT-gebruik dat de gebruiker bestaat, actief is en dat
  de credentialversie overeenkomt. Behoud de bestaande persistente cookie en
  niet-persistente `IAuthSessionStore`-semantiek.
- Voeg een gedeelde Owner/Crew-pagina `/account` toe voor eigen wachtwoordwijziging.
  Gebruik een browser-endpoint zodat na succes de huidige cookie met de nieuwe
  credentialversie wordt vernieuwd; alle andere cookies/tokens blijven ongeldig.
- Wachtwoorden zijn minimaal acht tekens. Nieuw wachtwoord en bevestiging moeten
  overeenkomen en het nieuwe wachtwoord moet verschillen van het huidige of tijdelijke
  wachtwoord.
- Nieuwe en geresette Crew krijgt `PasswordChangeRequired=true`. Alleen `/account` en
  logout zijn toegestaan totdat de wijziging is voltooid.
- Behoud Owner-onboarding als afzonderlijke flow: een onvoltooide Owner gaat naar
  `/onboarding`; Crew doorloopt nooit boot-onboarding.
- Voeg Owner-only gebruikersbeheer toe in `Instellingen > Account > Lokale gebruikers`:
  Crew aanmaken, tijdelijk wachtwoord resetten, uitschakelen en opnieuw activeren.
- Sta technisch meerdere Crew-accounts toe. Accountnamen zijn getrimd,
  hoofdletterongevoelig uniek en maximaal 100 tekens.
- Owner kan geen tweede Owner of andere rol aanmaken en kan zichzelf niet uitschakelen.
  Definitief verwijderen en rolwijziging bestaan niet.
- Reset verhoogt `CredentialVersion`, vervangt het wachtwoord en zet
  `PasswordChangeRequired=true`. Uitschakelen verhoogt `CredentialVersion` en zet
  `IsActive=false`. Opnieuw activeren wijzigt de bestaande wachtwoordwijzigingsstatus
  niet.
- Autoriseer dashboard, scan, logboek, logboekdetails, logboekprint en
  logboekbijlagen voor `Owner,Crew`. Houd Settings, Analysis/Beheerder,
  SystemController en shutdown Owner-only.
- Toon in navigatie aan Crew geen Settings- of Beheerderlinks. Toon de werkelijke
  displaynaam en bied iedere gebruiker `Mijn account`.
- Synchroniseer een gewijzigde Owner-naam vanuit Settings naar `DisplayName` en
  genormaliseerde naam, inclusief uniqueness-validatie.

## Outside Scope

- Geen inventory-, logboek- of andere domeinentiteiten uitbreiden met user-id.
- Geen uitnodigingen, e-mailflow, externe identity provider of cloudauthenticatie.
- Geen rollenmatrix, role editor, tweede Owner, definitief verwijderen of multi-vessel.
- Geen pincode-, recovery- of master-keyflow opnieuw zichtbaar maken.
- Geen ongerelateerde authrefactor, dependency-upgrade, UI-frameworkwijziging,
  documentatie, commit, push of PR.

## Expected Write-Set

Wijzig alleen deze modules, plus noodzakelijke compile-time bestanden binnen dezelfde
feature:

- `BootManager.Core`: vervang `OwnerProfile` door `LocalUser`; voeg de rol-enum toe.
- `BootManager.Application/Authentication` en
  `BootManager.Application/OwnerRegistration`: login, account, gebruikersbeheer,
  setupstatus, bootstrap, onboarding, DTO's en DI.
- `BootManager.Infrastructure/Persistence`, configuratie en migrations: uniforme
  gebruiker, datamigratie, index en model snapshot.
- `BootManager.Web`: auth endpoints/claims, cookie- en JWT-validatie, gates,
  Login/Account/Settings, navigatie en autorisatieattributen.
- `BootManager.UnitTests`: gerichte auth-, onboarding-, account- en
  gebruikersbeheertests.

Verwijder of pas bestaande Owner-gerichte types alleen aan wanneer ze door het uniforme
model zijn vervangen. Leg vóór wijziging buiten deze modules uit waarom die
compile-time of functioneel noodzakelijk is.

## Minimal Context

Lees:

- `CLAUDE.md`;
- de goedgekeurde storysectie in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Core/Entities/OwnerProfile.cs`;
- `BootManager.Application/Authentication/` en alleen de gebruikte
  OwnerRegistration-services;
- `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs`,
  `Configurations/OwnerProfileConfiguration.cs` en de actuele model snapshot;
- `BootManager.Web/Program.cs`, `Controllers/AuthController.cs`,
  `Components/OnboardingGate.razor`, `Components/Routes.razor`,
  `Components/Pages/Login.razor`, `Components/Pages/Settings.razor` en de twee
  layoutcomponenten;
- bestaande gerichte auth-, bootstrap-, onboarding- en settings-unit-tests.

Gebruik gerichte zoekopdrachten voor resterende `OwnerProfile`-referenties en
`Authorize(Roles = "Owner")`. Lees geen brede source trees.

Lees niet standaard:

- volledige `.docs/TODO.md`;
- andere epics of releaseverhalen;
- `.docs/legacy-analysis/` en `.docs/legacy-input/`;
- `.codex/current-session-handoff.md`;
- scan-, dashboard-, logboek- of NMEA-implementaties buiten de expliciet geraakte
  autorisatieattributen.

## Existing Constraints

- Target framework en architectuur volgen `CLAUDE.md`.
- De actuele SQLite database op de Raspberry Pi moet in-place kunnen migreren; een
  database-reset is geen acceptabele implementatiestrategie.
- Bestaande Owner-cookieclaims zijn hardcoded in zowel `Program.cs` als
  `AuthController`; beide routes moeten hetzelfde claimmodel gebruiken.
- `OnboardingGate` leest nu singleton Owner-status; vervang dit door setupstatus voor
  de actuele user-id zonder Application afhankelijk te maken van `HttpContext`.
- Een directe servicecall vanuit Blazor kan de browsercookie niet vernieuwen. Gebruik
  daarom voor eigen wachtwoordwijziging een geauthenticeerd browserendpoint en de
  bestaande `authClient.js`-fetchroute of een gelijkwaardige bestaande browserflow.
- De anonieme accountlijst retourneert geen rol, e-mail, hashes, setupflags of
  inactieve accounts.
- Gebruik geen nieuw authframework of externe package.
- Laat bestaande legacy pin/recovery-kolommen alleen bestaan voor
  migratiecompatibiliteit; bouw er geen nieuwe flow omheen.

## Acceptance Focus

- In-place migratie en behoud van bestaande Owner-login/onboarding.
- Accountselector, unieke namen en correcte werkelijke claims.
- Gescheiden Owner-onboarding en Crew-wachtwoordgate zonder redirectloop.
- Directe sessie-/tokenintrekking bij reset en uitschakelen.
- Crew-toegang tot dagelijkse pilotflows en harde weigering van Owner-beheer.
- Geen mogelijkheid om de enige Owner uit te schakelen of een extra Owner te maken.

## Required Checks

Voeg gerichte tests toe voor minimaal:

- migratie/backfill van een bestaand Owner-record;
- bootstrap bij lege database;
- actieve en inactieve user-login, fout wachtwoord en onbekende user-id;
- hoofdletterongevoelige unieke accountnaam;
- Crew aanmaken, resetten, uitschakelen en activeren;
- eigen wachtwoordwijziging en credentialversie;
- setupstatus voor Owner en Crew;
- cookie-/JWT-claimopbouw en versievalidatie waar praktisch geïsoleerd testbaar;
- autorisatieconstanten/policies of betrokken endpoints.

Voer eerst de gerichte testfilters voor de gewijzigde testklassen uit. Voer daarna uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

De bestaande
`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`-failure mag
alleen als niet-gerelateerde baseline worden gemeld wanneer exact dezelfde test als
enige bestaande failure overblijft.

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden en geïmplementeerd gedrag;
2. tests/checks en resultaten;
3. migratie- en configuratie-impact;
4. resterende risico's en exacte handmatige testvereisten.
