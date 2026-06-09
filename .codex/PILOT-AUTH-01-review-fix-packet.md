# Review Fix Packet

## Task

- Story ID: `PILOT-AUTH-01`
- Source packet: `.codex/PILOT-AUTH-01-implementation-packet.md`
- Status: huidige implementatie is niet gereed voor Codex-review.

Werk de bestaande implementatie af en herstel alle onderstaande reviewbevindingen.
De oorspronkelijke scope, outside scope, execution boundaries en definition of
technical completion blijven volledig gelden.

## Blocking Findings

1. De solution build faalt met 16 compile-errors in bestaande auth-, bootstrap- en
   setuptests. Werk bestaande tests om naar `LocalUser` en voeg alle in het bronpacket
   verplichte nieuwe tests toe.
2. De login-UI vult `LoginRequestDto.UserId` niet. Implementeer de anonieme actieve
   accountlijst en accountselector; zonder geselecteerde user kan niemand inloggen.
3. `/account`, het browserendpoint voor eigen wachtwoordwijziging en cookievernieuwing
   ontbreken volledig.
4. Owner-only gebruikersbeheer in Settings ontbreekt volledig.
5. Navigatie, displaynaam en Owner/Crew-autorisatie van dashboard/logboek/bijlagen zijn
   niet aangepast.
6. `OnboardingGate` stuurt Crew met `PasswordChangeRequired=true` naar `/onboarding`.
   Splits Owner-onboarding en Crew-wachtwoordwijziging en voorkom redirectloops.
7. `OwnerSettingsService` en `OnboardingService` gebruiken nog
   `IRepository<OwnerProfile>`. Na de migratie is `OwnerProfiles` verwijderd, waardoor
   Owner-instellingen en onboarding runtime breken. Migreer alle actuele Ownerflows
   naar `LocalUser`. Behandel legacy recovery/registration alleen voor
   compile-/migratiecompatibiliteit en maak geen nieuwe UI.
8. De EF modelsnapshot bevat nog `OwnerProfile`/`OwnerProfiles`. Genereer of corrigeer
   de migratie en snapshot als één consistent EF-model.
9. JWT's bevatten wel een credentialversieclaim, maar JWT-validatie controleert niet
   of de gebruiker actief is en de versie nog overeenkomt. Voeg dezelfde
   databasevalidatie toe als voor cookies.
10. De migratie gebruikt losse SQL met `suppressTransaction: true` en verwijdert daarna
    `OwnerProfiles`. Maak de in-place migratie transactioneel en bewijs op een tijdelijke
    SQLite-database dat id, hashes, payload, legacyvelden, timestamps en setupflags
    behouden blijven.
11. Valideer tijdelijke wachtwoorden bij Crew-aanmaak en reset op minimaal acht tekens.
12. Synchroniseer een Owner-naamswijziging in Settings atomair met `DisplayName` en
    `NormalizedName`, inclusief uniqueness-validatie.

## Required Verification

Voer na herstel uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

Lever daarnaast expliciet bewijs van een tijdelijke SQLite-migratietest met een
bestaand Owner-record. Start geen production- of Raspberry Pi-database.

## Completion Rule

Meld alleen `gereed voor Codex-review` wanneer alle twaalf bevindingen zijn opgelost,
de volledige oorspronkelijke packetscope aanwezig is en alle checks voldoen aan de
definition of technical completion. Anders meld je `niet gereed` met de concrete
resterende blokkade.

Wijzig geen documentatie, maak geen commit/push/PR en start geen deployment.
