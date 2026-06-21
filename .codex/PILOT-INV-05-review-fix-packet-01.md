# Review Fix Packet

## Task

- Story ID: `PILOT-INV-05`
- Base packet: `.codex/PILOT-INV-05-implementation-packet.md`
- Required branch: `codex/pilot-inv-05-mutaties-historie`
- Goal: herstel de huidige `PILOT-INV-05`-uitwerking zodat de volledige bedoelde
  storyscope in één gerichte correctieronde alsnog klopt: behoud van verwachte locatie
  na `0`-mutatie, een echte fysieke scanroute, een echte administratieve fallback en
  defectgevoelig testbewijs voor het nieuwe gedrag.

Dit is geen nieuwe story en geen brede herinterpretatie. Het is ook nadrukkelijk geen
"repareer één issue en stop". Deze correctieronde is alleen gereed als **alle**
onderstaande open punten samen zijn opgelost.

## Mandatory Start Check

Controleer vóór iedere wijziging:

1. de actieve branch is exact `codex/pilot-inv-05-mutaties-historie` en niet `master`;
2. de huidige `PILOT-INV-05`-worktreewijzigingen staan nog lokaal aanwezig;
3. er zijn geen onverwachte staged wijzigingen;
4. dit packet en het basispacket zijn beide gelezen.

Stop en rapporteer `not ready` wanneer de branch niet klopt of de bestaande
worktreewijzigingen ontbreken. Reset, checkout, stash of verwijder geen bestaande
wijzigingen.

## Current Open Defects

Deze bevindingen zijn al vastgesteld en moeten samen worden opgelost:

1. De verwachte locatie blijft functioneel niet bewaard na een mutatie naar `0`, omdat
   de terugvindlogica nog steeds alleen uit actieve of bestaande `Stock`-regels leest.
2. De storykritische fysieke route ontbreekt nog:
   - product terugvinden;
   - naar locatie gaan;
   - locatie scannen;
   - product scannen;
   - verbruik invoeren;
   - opslaan;
   - terug naar het begin van die route.
3. De storykritische administratieve fallback ontbreekt nog:
   - eerst product kiezen;
   - daarna locatie kiezen;
   - automatische locatiekeuze bij exact één actieve locatie;
   - mutatie uitvoeren zonder scan.
4. Het testbewijs is nog onvoldoende:
   - geen defectgevoelige tests voor `MutateStockAsync`;
   - geen bewijs voor behoud van verwachte locatie;
   - geen componenttests voor scanroute/fallback/historie.

## Non-Negotiable Outcome

Claude mag deze ronde **niet** als gereed melden wanneer slechts een subset is hersteld.

Met andere woorden:

- alleen servicefix zonder scanroute is onvoldoende;
- alleen scanroute zonder administratieve fallback is onvoldoende;
- alleen UI zonder defectgevoelige tests is onvoldoende;
- alleen historiepagina zonder correct behoud van verwachte locatie is onvoldoende.

Als één onderdeel nog ontbreekt, is de eindstatus `not ready`.

## Exact Scope Of This Fix Round

Herstel alleen de huidige story-uitwerking van `PILOT-INV-05` zodat zij voldoet aan het
al goedgekeurde basispacket.

Concreet moet deze ronde opleveren:

- correcte opslag en uitlezing van verwachte locatie na mutatie naar `0`;
- fysieke verbruikroute via de bestaande scan- en terugvindbasis;
- administratieve fallback zonder scannen;
- historiepagina met de al toegevoegde basis, maar nu aangesloten op echte storyflow;
- defectgevoelige tests voor service- en componentgedrag;
- migratiebewijs wanneer de gekozen technische oplossing opslag of upgradepad raakt.

## Explicitly Outside Scope

- Geen nieuwe story-uitbreidingen buiten `PILOT-INV-05`.
- Geen dashboardintegratie.
- Geen geavanceerde filters/export/rapportage voor historie.
- Geen batchmutaties, verplaatsingen of bulkacties.
- Geen documentatie-updates buiten dit packet.
- Geen commit, push, branch, PR, merge, release of deployment.

## Allowed Write-Set

Wijzig uitsluitend de al bedoelde `PILOT-INV-05`-gebieden, plus alleen wat strikt nodig
is om de huidige half-afgemaakte implementatie af te maken:

- `BootManager.Core/Entities/Stock.cs`;
- bestaande of nieuwe kleine inventory-entiteiten/enums onder
  `BootManager.Core/Entities/`;
- `BootManager.Application/Inventory/Contracts/IStockService.cs`;
- `BootManager.Application/Inventory/Services/StockService.cs`;
- optioneel `BootManager.Application/Inventory/Services/ProductService.cs` alleen als de
  fallback extra productselectie nodig heeft;
- bestaande of nieuwe kleine DTO's/result-types onder
  `BootManager.Application/Inventory/DTOs/` of `.../Results/`;
- `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/StockConfiguration.cs`;
- bestaande of nieuwe kleine inventory-configuraties onder
  `BootManager.Infrastructure/Persistence/Configurations/`;
- de al aangemaakte migratie of een vervangende migratie plus snapshot onder
  `BootManager.Infrastructure/Migrations/`, zolang uiteindelijk maar één consistente
  oplossing voor deze story overblijft;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Web/Components/Pages/Inventory/StockMutationHistory.razor`;
- `BootManager.Web/Components/Inventory/StockMutationModal.razor`;
- `BootManager.Web/Components/Layout/NavMenu.razor`;
- gerichte tests onder `BootManager.UnitTests/Inventory/` en
  `BootManager.UnitTests/Storage/`;
- indien echt nodig een gerichte migration/persistence-test onder
  `BootManager.IntegrationTests/Inventory/`.

## Forbidden Changes

Wijzig niet:

- story-, release-, TODO-, legacy-, README- of handoffdocumentatie;
- ongerelateerde domeinmodules;
- auth-architectuur;
- QR-format;
- algemene routering buiten de vereiste inventory-routes;
- projectbrede refactors;
- packages of deploymentconfiguratie;
- commit/push/branch/PR/merge/release/deployment.

## Exact Behavioral Rules

Volg deze regels letterlijk.

### A. Verwachte locatie na `0`

- Na `Verbruik`, `Correctie` of `Telling` naar exact `0` verdwijnt de actieve
  voorraadregel van de locatie.
- Daarna moet bestaand terugvindgedrag nog steeds een verwachte locatie voor dat product
  kunnen tonen.
- Dat bewijs moet niet afhankelijk zijn van een `Stock`-regel die net verwijderd is.
- `GetExpectedLocationForProductAsync` moet dus blijven werken na een mutatie naar `0`.

### B. Fysieke verbruikroute

- De fysieke route moet voortbouwen op de bestaande terugvind- en scanbasis, niet op een
  losse tweede scanfeature.
- De gebruiker moet expliciet in locatiecontext terechtkomen en daarna locatiecode en
  productcode kunnen scannen voor verbruik.
- De route moet eindigen met invoer van verbruikte hoeveelheid en opslaan.
- Na succesvol opslaan moet de gebruiker terugkeren naar het begin van dezelfde
  terugvind/verbruikroute, niet stil op een willekeurige detailpagina blijven hangen.

### C. Administratieve fallback

- Er moet een route zijn zonder scannen.
- Daar kiest de gebruiker eerst een product.
- Daarna kiest de gebruiker een locatie, behalve wanneer exact één actieve locatie
  bestaat; dan kiest BootManager die locatie automatisch.
- Vanuit die fallback moet de gebruiker minimaal `Verbruik`, `Correctie` en `Telling`
  kunnen vastleggen met optionele notitie.

### D. Historie

- De historiepagina moet alle mutaties standaard nieuwste eerst tonen.
- Elke regel toont minimaal:
  - datum/tijd;
  - type;
  - product;
  - gebied + locatie;
  - oude hoeveelheid;
  - nieuwe hoeveelheid;
  - gebruiker;
  - optionele notitie.
- De pagina mag geen dode navigatieknoppen bevatten.

### E. Huidige modal/historie-aanzet

- De al toegevoegde `StockMutationModal` en `StockMutationHistory` mogen worden
  hergebruikt, aangepast of vervangen.
- Maar laat ze niet als geïsoleerde “losse UI” bestaan; ze moeten onderdeel worden van
  de echte storystromen hierboven.

## Minimal Context

Lees alleen:

- `CLAUDE.md`;
- `.codex/PILOT-INV-05-implementation-packet.md`;
- dit packet;
- alleen de sectie `PILOT-INV-05` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Core/Entities/Stock.cs`;
- `BootManager.Core/Entities/Product.cs`;
- `BootManager.Core/Entities/StorageLocation.cs`;
- `BootManager.Core/Entities/StockMutation.cs`;
- `BootManager.Core/Entities/StockMutationType.cs`;
- `BootManager.Application/Inventory/Contracts/IStockService.cs`;
- `BootManager.Application/Inventory/Services/StockService.cs`;
- `BootManager.Application/Inventory/DTOs/StockMutationDto.cs`;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Web/Components/Pages/Inventory/StockMutationHistory.razor`;
- `BootManager.Web/Components/Inventory/StockMutationModal.razor`;
- `BootManager.Web/Components/Layout/NavMenu.razor`;
- `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/StockConfiguration.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/StockMutationConfiguration.cs`;
- de huidige `PILOT-INV-05`-migratiebestanden;
- `BootManager.UnitTests/Inventory/StockServiceTests.cs`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`;
- `BootManager.UnitTests/Storage/StorageLocationDetailsWithStockComponentTests.cs`.

Lees geen brede source trees of ongerelateerde documentatie.

## Required Test Evidence

Iedere nieuwe of gewijzigde test moet echte productcode/componentgedrag uitvoeren en
concreet kunnen falen.

### Servicebewijs

Voeg defectgevoelige tests toe die minimaal bewijzen:

1. `MutateStockAsync` verlaagt voorraad correct bij `Verbruik`.
2. `MutateStockAsync` blokkeert oververbruik en laat bestaande voorraad ongemoeid.
3. `MutateStockAsync` zet de nieuwe hoeveelheid correct bij `Correctie`.
4. `MutateStockAsync` zet de nieuwe hoeveelheid correct bij `Telling`.
5. een mutatie naar exact `0` verwijdert de actieve voorraadregel.
6. na zo'n `0`-mutatie blijft `GetExpectedLocationForProductAsync` nog steeds de
   verwachte locatie opleveren.
7. `GetStockMutationsAsync` retourneert nieuwste eerst.
8. `GetStockMutationsAsync` bevat type, oude hoeveelheid, nieuwe hoeveelheid, gebruiker
   en notitie.

### Componentbewijs

Voeg defectgevoelige componenttests toe die minimaal bewijzen:

1. de fysieke scanroute bestaat echt en gebruikt de bedoelde locatie- en productcontext;
2. na geslaagd fysiek verbruik keert `Scan.razor` terug naar het begin van die route;
3. de administratieve fallback zonder scannen bestaat echt;
4. die fallback kiest automatisch de locatie wanneer exact één actieve locatie bestaat;
5. die fallback dwingt locatiekeuze af wanneer meerdere actieve locaties bestaan;
6. de historiepagina toont de verplichte kolommen en data;
7. bestaand `PILOT-INV-04`-gedrag voor terugvinden en verwachte locatie niet regressief
   is.

### Niet toegestaan als “bewijs”

- Alleen controleren dat markuptekst voorkomt.
- Alleen controleren dat een modal zichtbaar is.
- Alleen constructor- of DI-wijzigingen zonder gedragstest.
- Alleen groene build zonder nieuwe gedragsasserties.

## Migration Proof

Als de uiteindelijke oplossing opslag wijzigt, lever dan echt migratiebewijs:

- migreer expliciet vanaf `20260620181000_AddStockUpdatedAtTimestamp`;
- controleer applied migrations vóór en na upgrade;
- voeg bestaande voorraaddata in vóór upgrade;
- bewijs na upgrade dat die data behouden blijft;
- bewijs dat de nieuwe opslag voor verwachte locatie/mutaties daarna werkt.

Als echte migration-test technisch niet haalbaar is, meld dat vóór afronding met de
concrete reden en lever gelijkwaardig bewijs. Zonder dat bewijs is de status
`not ready`.

## Required Checks

Voer minimaal uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockServiceTests|FullyQualifiedName~ProductsComponentTests|FullyQualifiedName~ScanComponentTests|FullyQualifiedName~StorageLocationDetailsWithStockComponentTests"
dotnet build BootManager.sln --no-restore
git diff --check
git status --short
git diff --stat
```

Als je een integration test toevoegt voor migratiebewijs, voer die aparte run ook uit.

## Definition Of Technical Completion

Rapporteer alleen `ready for Codex review` wanneer:

- alle vier huidige open defectclusters zijn opgelost;
- verwachte locatie na `0` aantoonbaar intact is;
- fysieke verbruikroute aantoonbaar werkt;
- administratieve fallback aantoonbaar werkt;
- historie aantoonbaar klopt en geen dode UI bevat;
- alle vereiste tests defectgevoelig zijn en slagen;
- build en `git diff --check` slagen;
- migratie- of gelijkwaardig opslagbewijs aanwezig is;
- geen onverklaarde wijziging buiten de toegestane write-set staat.

Rapporteer `not ready` wanneer ook maar één van deze onderdelen ontbreekt of niet
bewezen is.

## Completion Notes

Retourneer uitsluitend:

1. exacte gewijzigde bestanden;
2. per open defectcluster wat concreet is hersteld;
3. exacte nieuwe/gewijzigde testnamen en welk productiegedrag zij uitvoeren;
4. migratie- of gelijkwaardig opslagbewijs;
5. alle checkresultaten;
6. resterende risico's of handmatige testpunten;
7. eindstatus `ready for Codex review` of `not ready`, met concrete reden.
