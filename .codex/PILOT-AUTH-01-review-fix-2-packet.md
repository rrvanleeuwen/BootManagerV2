# Review Fix Packet 2

## Task

- Story ID: `PILOT-AUTH-01`
- Required branch: `feature/pilot-auth-01-local-users`
- Source packets:
  - `.codex/PILOT-AUTH-01-implementation-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-packet.md`
- Status: niet gereed; start de applicatie nog niet.

Werk uitsluitend de onderstaande resterende reviewblockers af. Alle oorspronkelijke
scope-, uitvoerings- en oplevergrenzen blijven gelden.

## Blocking Findings

1. **EF-migratie is niet uitvoerbaar.**

   `BootManagerDbContextModelSnapshot.cs` bevat nog `OwnerProfile`/`OwnerProfiles`.
   Een migratieproef op een tijdelijke kopie stopt met
   `PendingModelChangesWarning`. Genereer een consistente migratie, designer en
   modelsnapshot voor `LocalUser`, en bewijs daarna de in-place migratie op een
   tijdelijke databasekopie.

2. **Crew kan dagelijkse pilotflows niet openen.**

   `Dashboard.razor`, `Logbook.razor`, `LogbookEntryDetails.razor`,
   `LogbookPrint.razor` en `LogbookAttachmentsController` zijn nog Owner-only.
   Autoriseer deze expliciet voor `Owner,Crew`. Houd Analysis, Settings, System en
   shutdown Owner-only.

3. **Verplichte Crew-wachtwoordwijziging wordt niet afgedwongen.**

   `OnboardingGate` retourneert direct voor iedere Crew-user. Een nieuwe of geresette
   Crew moet uitsluitend `/account` en logout kunnen gebruiken totdat
   `PasswordChangeRequired=false`; Owner-onboarding blijft naar `/onboarding` gaan.

4. **Sessie vernieuwen na wachtwoordwijziging kan niet betrouwbaar werken.**

   `AccountService` verhoogt eerst `CredentialVersion`; de daaropvolgende browser-POST
   naar het geautoriseerde `/auth/renew-session` gebruikt nog de oude cookie, die door
   `OnValidatePrincipal` wordt ingetrokken voordat het endpoint kan vernieuwen.
   Ontwerp één browserendpoint dat het huidige wachtwoord verifieert, het wachtwoord
   wijzigt en in dezelfde request de cookie met de nieuwe versie uitgeeft. Laat
   `Account.razor` uitsluitend dat endpoint gebruiken.

5. **Crew-beheer toont de verkeerde en onvolledige lijst.**

   Settings gebruikt `GetActiveUsersAsync()`, waardoor:
   - de Owner als Crew-regel verschijnt;
   - inactieve Crew ontbreekt;
   - opnieuw activeren onmogelijk is;
   - iedere getoonde gebruiker kunstmatig `IsActive=true` krijgt.

   Voeg een afzonderlijk Owner-only beheercontract toe dat uitsluitend alle
   Crew-accounts met echte actieve status retourneert. Houd de anonieme loginselector
   beperkt tot actieve id en displaynaam.

6. **Navigatie is inconsistent.**

   Dashboard staat in `NavMenu` ten onrechte onder Owner-only. `MainLayout` toont Crew
   nog steeds een link naar `/settings`. Toon dashboard voor Owner en Crew, toon
   Settings/Beheerder alleen voor Owner en toon `Mijn account` voor beide rollen.

7. **Nieuwe kernservices zijn niet gericht getest.**

   Voeg tests toe voor minimaal:
   - Crew aanmaken, naamuniciteit en wachtwoordminimum;
   - reset, uitschakelen, heractiveren en credentialversie;
   - Crew-beheerlijst inclusief inactieve Crew en zonder Owner;
   - eigen wachtwoordwijziging plus cookievernieuwing;
   - Owner-onboarding versus Crew-wachtwoordgate;
   - Owner/Crew-autorisatie van dagelijkse routes;
   - migratiebehoud van id, hashes, payload, legacyvelden, timestamps en setupflags.

8. **Opleverrapport was feitelijk onjuist.**

   Meld geen `0 warnings`: de onafhankelijke build bevat 13 bestaande waarschuwingen.
   Rapporteer exacte aantallen en onderscheid bestaande waarschuwingen van nieuwe
   fouten.

## Required Checks

Voer sequentieel uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

Voer daarna een EF-migratie uit op een tijdelijke kopie van een bestaande SQLite
database en controleer de bewaarde Owner-velden. Gebruik nooit
`BootManager.Web/bootmanager.db` als migratiedoel.

## Completion Rule

Meld alleen `gereed voor Codex-review` als alle acht blockers aantoonbaar zijn opgelost,
de migratieproef slaagt en er geen nieuwe testfailure is. De bekende recoverytest mag
als enige baselinefailure overblijven.

Wijzig geen documentatie, commit/push/PR of deployment.
