# Review Fix Packet 8

## Task

- Story ID: `PILOT-AUTH-01`
- Required branch: `feature/pilot-auth-01-local-users`
- Source packet: `.codex/PILOT-AUTH-01-review-fix-7-packet.md`
- Status: `niet gereed`; handmatige acceptatie wacht op deze technische afronding.

Werk uitsluitend de resterende `MainLayout`-initialisatie- en testfinding af. De
Owner/Crew-PCR-fix is onafhankelijk correct bevonden en mag niet worden gewijzigd.

## Independent Review

Codex heeft na packet 7 vastgesteld:

- bootstrap Owner kan na login `/onboarding` bereiken zonder redirect naar
  `/account`;
- Crew-PCR blijft rolgericht server-side actief;
- 203 unit-tests slagen; alleen de bekende recoverybaseline faalt;
- 12/12 integratietests slagen;
- solution-build en `git diff --check` slagen.

De volgende claim uit de oplevering is echter onjuist:

> Alle code-fixes zijn correct geïmplementeerd en bewezen via geautomatiseerde tests.

Packet 7 vereiste voor `MainLayout`:

- onderscheidbare diagnostiek voor module-import- en auto-collapsefouten;
- een echte componenttest of voldoende gedragsmatige test die een falende
  module-import simuleert;
- bewijs dat geen uitzondering ontsnapt, een zichtbare fout ontstaat en logout
  zonder module geen endpointaanroep doet.

De huidige implementatie bevat:

```csharp
catch (JSException) { }
```

voor `setupAutoCollapse`. Dit maskeert de fout zonder melding of logging.

De nieuwe tests lezen alleen de `.razor`-bron en zoeken tekst. Zij simuleren geen
`IJSRuntime`-fout, renderen de component niet en roepen logout niet aan. Dit is geen
gedragsbewijs.

## Required Fix

- Injecteer gerichte logging in `MainLayout`.
- Log een module-importfout en een auto-collapsefout met verschillende,
  herkenbare berichten en de gevangen exception.
- De importfout blijft daarnaast onmiddellijk zichtbaar voor de gebruiker.
- Een auto-collapsefout mag het circuit niet beëindigen. Een zichtbare melding is
  toegestaan, maar logging is minimaal verplicht.
- Laat geen lege `catch (JSException) { }` achter.
- Logout met `_authModule == null` toont een zichtbare fout en doet geen
  `postJson`-aanroep.

## Required Behavioral Test

Voeg een echte component-/gedragstest toe. Alleen bron-tekstinspectie is niet
voldoende.

De test moet met een gecontroleerde/fake `IJSRuntime` minimaal bewijzen:

1. een exception bij `import` ontsnapt niet uit de eerste render;
2. de gerenderde layout toont de importfout;
3. de importfout wordt gelogd;
4. een exception bij `setupAutoCollapse` ontsnapt niet en wordt met een ander bericht
   gelogd;
5. klikken op logout wanneer de module ontbreekt geeft een zichtbare melding;
6. in dat geval wordt `postJson` niet aangeroepen.

Gebruik bij voorkeur een bestaande testtechniek. Wanneer een componenttestpakket
nodig is, voeg uitsluitend de minimale testdependency toe en leg kort uit waarom.
Verwijder of behoud bron-teksttests naar nut, maar presenteer ze niet als
gedragsbewijs.

## Expected Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Components/Layout/MainLayout.razor`;
- gerichte `MainLayout`-tests;
- `BootManager.UnitTests/BootManager.UnitTests.csproj` alleen wanneer een minimale
  componenttestdependency nodig is.

Wijzig niet:

- `PcrGateMiddleware`;
- authservices, endpoints, migraties of domeinmodel;
- overige UI;
- documentatie;
- gitstatus buiten bovenstaande files.

## Execution Boundaries

- Behoud alle bestaande ongecommitte wijzigingen.
- Maak geen commit, push, branch, PR, merge, release of deployment.
- Gebruik geen productie- of Raspberry Pi-database.

## Required Checks

Voer eerst de gerichte componenttest uit. Voer daarna sequentieel uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

Wanneer een nieuwe package restore nodig is, voer die eenmalig gericht uit vóór de
`--no-restore`-checks.

De bekende
`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`-failure mag
alleen als bestaande baseline worden gemeld wanneer dit exact de enige
unit-testfailure blijft.

## Completion Rule

Meld `gereed voor Codex-review` wanneer:

- de lege JSException-catch verwijderd is;
- beide initialisatiefouten onderscheidbaar worden gelogd;
- de echte gedragstest alle zes punten bewijst;
- de vereiste checks acceptabel zijn.

Browseracceptatie is niet onderdeel van deze kleine herstelopdracht. Codex geeft na
review de handmatige gebruikersacceptatietest vrij.

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden;
2. logginggedrag;
3. naam en bewijs van de echte component-/gedragstest;
4. test-, build- en diffcheckresultaten;
5. eindstatus `gereed voor Codex-review` of `niet gereed`.
