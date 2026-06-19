# Implementation Packet

## Task

- Story ID: `PILOT-LOC-04`
- Approved story: QR-token vervangen en tagoverzicht
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-LOC-04`
- Goal: laat alleen Owner bestaande locatie-QR's beheren via een compact
  tagoverzicht, handmatige tagstatus per locatie opslaan en een QR-token kunnen
  vervangen zodat het oude token ongeldig wordt en het nieuwe token direct werkt,
  zonder scan-, print/export- of autorisatiegedrag uit `PILOT-LOC-02` en
  `PILOT-LOC-03` te breken.
- Required branch: `feature/pilot-loc-04-token-replacement-tag-overview`

De story is al goedgekeurd. Formuleer haar niet opnieuw en vraag geen nieuw akkoord.
Geef een kort uitvoeringsplan, implementeer direct, voer de checks uit en rapporteer
volgens `Completion Notes`.

## Scope

- Voeg een expliciet handmatig tagstatusveld toe voor opslaglocaties met precies deze
  toestanden: `Niet geprint`, `Geprint`, `Gekoppeld`, `Vervangen`.
- Persist deze tagstatus in de bestaande storage-opslaglaag; een bestaande database
  moet additief kunnen upgraden zonder verlies van gebieden, locaties of bestaande
  QR-tokens.
- Voeg in de application-laag een gerichte vervangactie toe waarmee Owner voor één
  bestaande locatie een nieuw BootManager-token activeert.
- Tokenvervanging moet het oude token ongeldig maken en het nieuwe token direct
  bruikbaar maken via de bestaande scan-/resolveflow.
- Bouw een Owner-only tagoverzichtspagina waarop alle locaties zichtbaar zijn met
  minimaal gebied, locatienaam, huidig `QrValue` of duidelijke afwezigheid daarvan,
  en de handmatige tagstatus.
- Laat Owner in dat overzicht de tagstatus aanpassen en voor locaties met bestaande
  token een vervangactie uitvoeren.
- Zorg dat Crew het overzicht en de vervangactie niet kan beheren, ook niet via een
  directe URL.
- Behoud de bestaande locatie-detailpagina en tagpagina uit `PILOT-LOC-03`; waar
  nodig mogen daar kleine gerichte links of refreshes worden toegevoegd, maar de
  kern van deze story is het overzicht en de vervangactie.

## Outside Scope

- Geen automatische afleiding van status uit print-, download- of scanacties.
- Geen auditlog van tokenvervangingen.
- Geen printerintegratie, labelprinterprofielen, batchprint of extra exportformaten.
- Geen wijziging van QR-value-format, scanroutering of barcodegedrag buiten wat nodig
  is om oud token ongeldig en nieuw token geldig te maken.
- Geen producten, voorraad, mutatiehistorie of andere inventory-functionaliteit.
- Geen documentatie-, commit-, push-, branch-, PR-, merge-, release- of
  deploymentacties.

## Expected Write-Set

Wijzig alleen deze bestanden of modules, tenzij een noodzakelijke compile-time
dependency wordt ontdekt:

- `BootManager.Core/Entities/StorageLocation.cs`;
- optioneel één nieuwe enum of waardeobject onder `BootManager.Core/Enums/` of een
  vergelijkbare bestaande Core-map voor de handmatige tagstatus;
- `BootManager.Application/Storage/DTOs/StorageLocationDto.cs`;
- `BootManager.Application/Storage/DTOs/StorageLocationDetailDto.cs`;
- `BootManager.Application/Storage/Services/IStorageService.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- optioneel één kleine result- of request-DTO onder `BootManager.Application/Storage/`;
- `BootManager.Infrastructure/Persistence/Configurations/StorageLocationConfiguration.cs`;
- één nieuwe EF-migratie plus bijbehorende designer en
  `BootManager.Infrastructure/Migrations/BootManagerDbContextModelSnapshot.cs`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor` alleen voor kleine
  navigatie-/refreshaanpassingen;
- `BootManager.Web/Components/Pages/StorageLocationTag.razor` alleen als een kleine
  refresh- of vervanglink nodig is;
- één nieuwe Owner-only tagoverzichtspagina onder `BootManager.Web/Components/Pages/`;
- gerichte tests onder `BootManager.UnitTests/Storage/`,
  `BootManager.UnitTests/Web/` en `BootManager.IntegrationTests/Storage/`.

Voeg geen brede refactors toe in settings, scan, layout of auth. Wijzig geen andere
storage-entiteiten zonder vooraf uit te leggen waarom dat nodig is.

## Execution Boundaries

- Implementeer alleen applicatiecode, migraties, presentatiecode en tests die dit
  packet expliciet vereist.
- Controleer vóór bewerken dat de actieve branch exact
  `feature/pilot-loc-04-token-replacement-tag-overview` is en niet `master`.
  Rapporteer `not ready` als dat niet zo is.
- Wijzig geen story-, release-, TODO-, legacy-, README-, handoff- of andere
  projectdocumentatie.
- Maak geen commit, push, branch, PR, merge, release of deployment.
- Verander de goedgekeurde storystatusset niet. Gebruik exact de vier afgesproken
  handmatige toestanden of motiveer een compile-time noodzakelijke representatie die
  daar 1-op-1 aan correspondeert.
- Houd tokenvervanging beperkt tot het actief maken van een nieuw token en het
  ongeldig maken van het vorige token. Voeg geen tokenhistorie of soft-delete-model
  toe.
- Noem de story nooit `Done`, geaccepteerd of productierijp. Meld alleen
  `ready for Codex review` wanneer de technische completion definition volledig is
  gehaald.

## Minimal Context

Lees:

- `CLAUDE.md`;
- `.codex/PILOT-LOC-04-implementation-packet.md`;
- alleen de sectie `PILOT-LOC-04` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Core/Entities/StorageLocation.cs`;
- `BootManager.Application/Storage/Services/IStorageService.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- `BootManager.Application/Storage/DTOs/StorageLocationDto.cs`;
- `BootManager.Application/Storage/DTOs/StorageLocationDetailDto.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/StorageLocationConfiguration.cs`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- `BootManager.Web/Components/Pages/StorageLocationTag.razor`;
- `BootManager.UnitTests/Storage/StorageServiceQrTokenTests.cs`;
- `BootManager.UnitTests/Storage/StorageLocationDetailsComponentTests.cs`;
- `BootManager.UnitTests/Storage/StorageLocationTagComponentTests.cs`;
- `BootManager.UnitTests/Web/RouteAuthorizationTests.cs`;
- `BootManager.IntegrationTests/Storage/StorageQrTokenIntegrationTests.cs`;
- `BootManager.IntegrationTests/Storage/StorageMigrationAndConstraintsTests.cs`.

Gebruik gerichte zoekopdrachten en kleine bestandssecties. Lees niet standaard:

- de volledige `.docs/TODO.md` of andere releaseverhalen;
- `.docs/legacy-analysis/` of `.docs/legacy-input/`;
- `.codex/current-session-handoff.md` of `.codex/working-agreement.md`;
- repositorybrede source trees.

## Existing Constraints

- Volg .NET 8 en de Clean Architecture-regels in `CLAUDE.md`.
- `StorageLocation.SetQrToken` weigert nu overschrijven; deze story mag die
  domeinbeperking gericht aanpassen, maar alleen voor expliciete tokenvervanging en
  zonder andere invarianten rond naam, gebied of uniqueness te verzwakken.
- De bestaande unieke SQLite-index op `QrToken` moet effectief blijven bewijzen dat
  één actief token maar aan één locatie gekoppeld kan zijn.
- De bestaande `ResolveQrValueAsync`-flow is leidend bewijs dat oude tokens niet meer
  resolven en nieuwe tokens wel; introduceer geen parallelle QR-resolvepaden.
- De bestaande tagpagina en renderer uit `PILOT-LOC-03` blijven het print/exportpad;
  deze story mag dat pad niet opnieuw ontwerpen.
- Houd de UI functioneel en klein. Een eenvoudig Owner-overzicht is voldoende; geen
  brede visuele redesign.

## Acceptance Focus

- Tokenvervanging moet aantoonbaar invalideren: hetzelfde oude `QrValue` resolveert na
  vervanging niet meer naar de locatie.
- Het nieuwe `QrValue` moet direct na vervanging wel naar dezelfde locatie resolven.
- Tagstatus is expliciet handmatig, persistent en zichtbaar in het overzicht.
- Owner kan beheren; Crew niet.
- Bestaande print/export (`PILOT-LOC-03`) en bestaande scanrouting (`PILOT-LOC-02`)
  blijven functioneel op basis van de actuele token.

## Test Evidence Requirements

Voeg defectgevoelige tests toe die echte productcode/componenten uitvoeren en concreet
bewijzen:

- een bestaande locatie met token kan via de service een nieuw token krijgen, waarbij
  het oude token niet behouden blijft;
- `ResolveQrValueAsync` geeft na vervanging voor het oude `QrValue` geen gekoppelde
  locatie meer terug en voor het nieuwe `QrValue` wel;
- tagstatus kan via de service worden bijgewerkt en wordt correct teruggelezen in de
  relevante DTO's;
- de migratie voegt het tagstatusveld additief toe en behoudt bestaande locaties en
  bestaande `QrToken`-waarden;
- het nieuwe overzicht rendert alle relevante kolommen en laat Owner status wijzigen
  en vervanging starten;
- Crew krijgt geen beheer-UI en de nieuwe route is via autorisatietests `Owner` only;
- bestaande detail- en tagpagina's blijven compatibel met locaties zonder token en met
  locaties waarvan het token net vervangen is.

Inspecteer iedere nieuwe of gewijzigde test: geen `Assert.True(true)`, lege test,
bronvormtest als vervanging van gedrag of `async` test zonder relevante `await`.

Deze story is geen bugfix, dus formeel red-green-bewijs is niet verplicht. Als je een
bestaand defect tegenkomt en meeneemt, lever daarvoor alsnog expliciet red-green of
gelijkwaardig bewijs.

## Required Checks

Voer eerst gerichte checks uit, bijvoorbeeld:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StorageServiceQrTokenTests|FullyQualifiedName~StorageLocationDetailsComponentTests|FullyQualifiedName~StorageLocationTagComponentTests|FullyQualifiedName~StorageLocationTagOverview|FullyQualifiedName~RouteAuthorizationTests"
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~StorageQrTokenIntegrationTests|FullyQualifiedName~StorageMigrationAndConstraintsTests"
```

Voer daarna uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

De bekende
`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`-failure mag
alleen als ongerelateerde baseline worden gemeld wanneer exact die ene bestaande
unit-testfailure overblijft. Alle nieuwe of gewijzigde tests moeten slagen.

## Definition of Technical Completion

Meld uitsluitend `ready for Codex review` wanneer:

- ieder scopepunt en acceptatiecriterium technisch is geïmplementeerd;
- tagstatus persistent is toegevoegd met bewezen upgradepad;
- tokenvervanging oude tokens invalideert en nieuwe tokens direct activeert;
- het overzicht Owner-only is en Crew geen beheeractie of route-toegang krijgt;
- alle gerichte tests slagen en alle nieuwe of gewijzigde tests echte productcode
  uitvoeren;
- volledige unit- en integratietestruns geen nieuwe failure bevatten;
- build en `git diff --check` slagen;
- geen onverklaarde wijziging buiten de verwachte write-set staat;
- resterende handmatige acceptatiestappen expliciet zijn vermeld.

Meld `not ready` wanneer scope onvolledig is, de migratie/compatibiliteit onbewezen
blijft, oude tokens nog resolven, tagstatus niet persistent is, Crew-toegang
onvoldoende afgeschermd is, een nieuwe of gewijzigde test faalt, build/diffcheck
faalt, een vereiste beslissing ontbreekt of extra write-area niet kan worden
verantwoord.

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden en geïmplementeerd gedrag;
2. tests/checks en resultaten;
3. exacte nieuwe/gewijzigde testnamen en welk productiegedrag zij uitvoeren;
4. migratie-, package- of configuratie-impact;
5. resterende risico's en exacte handmatige testvereisten;
6. eindstatus: `ready for Codex review` of `not ready`, met concrete reden.
