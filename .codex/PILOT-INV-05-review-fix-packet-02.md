# Review Fix Packet

## Task

- Story ID: `PILOT-INV-05`
- Base packet: `.codex/PILOT-INV-05-implementation-packet.md`
- Previous failed correction packet: `.codex/PILOT-INV-05-review-fix-packet-01.md`
- Required branch: `codex/pilot-inv-05-mutaties-historie`
- Goal: voltooi `PILOT-INV-05` nu zodanig dat Claude niet alleen servicewerk doet,
  maar verplicht ook de fysieke scanroute, de administratieve fallback, de historie-UI
  en de vereiste componenttests daadwerkelijk aansluit.

Dit packet vervangt niet de story, maar **vernauwt** de correctie tot een harde
uitvoeringsset. Als een verplichte bestandswijziging of verplichte test ontbreekt, is
de eindstatus automatisch `not ready`.

## Hard Gate

Claude mag deze ronde alleen `ready for Codex review` noemen als aan **alle** onderstaande
mechanische voorwaarden is voldaan:

1. `BootManager.Web/Components/Pages/Scan.razor` is inhoudelijk gewijzigd.
2. `BootManager.Web/Components/Pages/Inventory/Products.razor` is inhoudelijk gewijzigd.
3. `BootManager.UnitTests/Storage/ScanComponentTests.cs` is inhoudelijk gewijzigd.
4. `BootManager.UnitTests/Inventory/ProductsComponentTests.cs` is inhoudelijk gewijzigd.
5. `BootManager.Web/Components/Pages/Inventory/StockMutationHistory.razor` bevat geen
   dode terugknop meer.
6. De fysieke scanroute bestaat echt in productiecode.
7. De administratieve fallback bestaat echt in productiecode.
8. Beide routes hebben defectgevoelige componenttests.

Ontbreekt één van deze acht punten, rapporteer dan `not ready` zonder discussie.

## Mandatory Start Check

Controleer vóór iedere wijziging:

1. de actieve branch is exact `codex/pilot-inv-05-mutaties-historie` en niet `master`;
2. de huidige worktreewijzigingen van `PILOT-INV-05` zijn nog aanwezig;
3. dit packet, het basispacket en packet `01` zijn gelezen;
4. de index bevat geen onverwachte staged wijzigingen.

Stop en rapporteer `not ready` wanneer een van deze checks faalt.

## Exact Remaining Defects

Deze ronde lost exact deze restpunten op, niet minder en niet meer:

1. Verwachte locatie na mutatie naar `0` moet betrouwbaar blijven werken.
2. De fysieke scanroute ontbreekt nog in `Scan.razor`.
3. De administratieve fallback ontbreekt nog in `Products.razor` of een direct daaraan
   gekoppelde inventory-flow.
4. De historiepagina heeft nog een dode terugknop.
5. De componenttests voor scanroute en fallback ontbreken nog.

## Required File-Level Outcome

Claude moet de volgende productie-bestanden inhoudelijk wijzigen:

- `BootManager.Web/Components/Pages/Scan.razor`
- `BootManager.Web/Components/Pages/Inventory/Products.razor`
- `BootManager.Web/Components/Pages/Inventory/StockMutationHistory.razor`

Claude moet de volgende testbestanden inhoudelijk wijzigen:

- `BootManager.UnitTests/Storage/ScanComponentTests.cs`
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`

Daarnaast mogen alleen de minimaal noodzakelijke service-, entity-, migration- en
ondersteunende inventory-bestanden worden aangepast om dit werkend te krijgen.

Als Claude eindigt zonder wijzigingen in een van de vijf verplichte bestanden hierboven,
dan is de status `not ready`.

## Allowed Write-Set

Wijzig uitsluitend:

- `BootManager.Application/Inventory/Contracts/IStockService.cs`;
- `BootManager.Application/Inventory/Services/StockService.cs`;
- optioneel `BootManager.Application/Inventory/Services/ProductService.cs` als dat
  aantoonbaar nodig is voor de fallback;
- kleine inventory DTO's/result-types onder
  `BootManager.Application/Inventory/DTOs/` of `.../Results/`;
- kleine inventory-entiteiten/enums onder `BootManager.Core/Entities/`;
- `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs`;
- inventory-configuraties onder
  `BootManager.Infrastructure/Persistence/Configurations/`;
- de bestaande of vervangende `PILOT-INV-05`-migraties plus snapshot;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Web/Components/Pages/Inventory/StockMutationHistory.razor`;
- `BootManager.Web/Components/Pages/Inventory/StockMutations.razor` als die pagina
  de fallback daadwerkelijk huisvest;
- `BootManager.Web/Components/Inventory/StockMutationModal.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- `BootManager.Web/Components/Layout/NavMenu.razor`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- optioneel `BootManager.UnitTests/Storage/StorageLocationDetailsWithStockComponentTests.cs`;
- optioneel gerichte inventory integration tests als migratiebewijs nodig is.

## Forbidden Moves

Niet toegestaan:

- alleen nieuwe losse pagina's bouwen zonder aansluiting in `Scan.razor` en
  `Products.razor`;
- alleen service- of entityfixes doen;
- alleen unit-tests toevoegen zonder componenttests;
- een “handmatige route” toevoegen via alleen `NavMenu.razor` zonder koppeling in
  `Products.razor`;
- documentatie wijzigen buiten dit packet;
- commit, push, branch, PR, merge, release of deployment.

## Exact Behavioral Requirements

### A. Verwachte locatie na `0`

- `GetExpectedLocationForProductAsync` moet blijven werken nadat een mutatie de actieve
  voorraadregel naar `0` heeft gebracht en die regel verdwenen is.
- Dit mag niet afhankelijk zijn van een zojuist verwijderde `Stock`.

### B. Fysieke scanroute in `Scan.razor`

Claude moet in `Scan.razor` een echte mutatie-/verbruikroute aansluiten die voortbouwt
op de bestaande scanflow.

Minimaal vereist in productiegedrag:

1. De gebruiker komt vanuit de bestaande scan/terugvindcontext in een verbruikroute.
2. Een locatiecode wordt in die route betekenisvol verwerkt als locatiecontext.
3. Een productcode wordt daarna betekenisvol verwerkt als productcontext.
4. De gebruiker kan daarna `Verbruik` vastleggen.
5. Na succesvolle opslag keert de route terug naar het begin van die fysieke flow.

Een losse nieuwe pagina of losse modal zonder aansluiting in `Scan.razor` telt niet.

### C. Administratieve fallback in `Products.razor`

Claude moet in `Products.razor` een echte fallback aansluiten voor muteren zonder
scannen.

Minimaal vereist in productiegedrag:

1. De gebruiker kiest eerst een product.
2. Daarna kiest de gebruiker een locatie, behalve bij exact één actieve locatie.
3. Bij exact één actieve locatie wordt die locatie automatisch gekozen.
4. Daarna kan `Verbruik`, `Correctie` of `Telling` met optionele notitie worden
   opgeslagen.

Een losse route via alleen `NavMenu.razor` telt niet als vervanging voor deze eis.

### D. Historiepagina

- De historiepagina toont de verplichte kolommen.
- De terugknop werkt echt of wordt verwijderd; een lege handler is niet toegestaan.

## Minimal Context

Lees alleen:

- `CLAUDE.md`;
- `.codex/PILOT-INV-05-implementation-packet.md`;
- `.codex/PILOT-INV-05-review-fix-packet-01.md`;
- dit packet;
- alleen de sectie `PILOT-INV-05` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Application/Inventory/Contracts/IStockService.cs`;
- `BootManager.Application/Inventory/Services/StockService.cs`;
- relevante nieuwe inventory DTO/entity/configuratie/migratiebestanden van deze story;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Web/Components/Pages/Inventory/StockMutationHistory.razor`;
- `BootManager.Web/Components/Pages/Inventory/StockMutations.razor`;
- `BootManager.Web/Components/Inventory/StockMutationModal.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- `BootManager.Web/Components/Layout/NavMenu.razor`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- `BootManager.UnitTests/Inventory/StockServiceTests.cs`;
- optioneel `BootManager.UnitTests/Storage/StorageLocationDetailsWithStockComponentTests.cs`.

## Required Test Evidence

### Service tests

Behoud en verbeter defectgevoelig bewijs voor:

1. `Verbruik` verlaagt voorraad correct.
2. oververbruik wordt geblokkeerd.
3. `Correctie` zet de nieuwe hoeveelheid.
4. `Telling` zet de nieuwe hoeveelheid.
5. mutatie naar `0` verwijdert actieve voorraadregel.
6. verwachte locatie blijft daarna opvraagbaar.
7. historie retourneert nieuwste eerst.

### Component tests in `ScanComponentTests.cs`

Voeg of wijzig tests die **echt** bewijzen:

1. de fysieke verbruikroute in `Scan.razor` bestaat;
2. locatiecontext in die route wordt gezet;
3. productcontext daarna wordt gezet;
4. een verbruikactie naar de mutatieservice leidt;
5. de route na succes terugkeert naar het begin van de flow.

Als deze vijf punten niet in `ScanComponentTests.cs` bewezen worden, status `not ready`.

### Component tests in `ProductsComponentTests.cs`

Voeg of wijzig tests die **echt** bewijzen:

1. de administratieve fallback vanuit `Products.razor` bestaat;
2. productselectie werkt;
3. auto-locatiekeuze werkt bij exact één actieve locatie;
4. handmatige locatiekeuze werkt bij meerdere actieve locaties;
5. een mutatieactie naar de mutatieservice leidt.

Als deze vijf punten niet in `ProductsComponentTests.cs` bewezen worden, status
`not ready`.

### Historie-UI bewijs

Voeg waar nodig componentbewijs toe dat:

1. de historiepagina de verplichte kolommen toont;
2. de terugactie niet dood is.

## Not Accepted As Evidence

- Alleen `rg`/markup assertions op tekststrings.
- Alleen aantonen dat een knop bestaat.
- Alleen aantonen dat een losse nieuwe route compileert.
- Alleen service-tests zonder componenttests.

## Required Checks

Voer minimaal uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockServiceTests|FullyQualifiedName~ProductsComponentTests|FullyQualifiedName~ScanComponentTests|FullyQualifiedName~StorageLocationDetailsWithStockComponentTests"
dotnet build BootManager.sln --no-restore
git diff --check
git diff --name-only
git status --short
```

Controleer daarna expliciet in de completion notes:

1. dat `Scan.razor` gewijzigd is;
2. dat `Products.razor` gewijzigd is;
3. dat `ScanComponentTests.cs` gewijzigd is;
4. dat `ProductsComponentTests.cs` gewijzigd is;
5. dat de historie-terugactie niet meer dood is.

## Definition Of Technical Completion

Rapporteer alleen `ready for Codex review` wanneer:

- alle verplichte bestandswijzigingen aanwezig zijn;
- de fysieke scanroute echt in `Scan.razor` is aangesloten;
- de administratieve fallback echt in `Products.razor` is aangesloten;
- de historiepagina geen dode terugactie meer heeft;
- service- én componenttests defectgevoelig zijn en slagen;
- build en `git diff --check` slagen;
- geen onverklaarde wijziging buiten de write-set aanwezig is.

Rapporteer `not ready` wanneer een van deze voorwaarden niet gehaald is.

## Completion Notes

Retourneer uitsluitend:

1. exacte gewijzigde bestanden;
2. bevestiging per verplichte file dat die inhoudelijk is gewijzigd;
3. per defectcluster wat concreet is hersteld;
4. exacte nieuwe/gewijzigde testnamen en wat ze echt bewijzen;
5. alle checkresultaten;
6. eindstatus `ready for Codex review` of `not ready`, met concrete reden.
