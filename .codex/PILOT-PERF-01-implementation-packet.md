# Implementation Packet

## Task

- Story ID: `PILOT-PERF-01`
- Approved story: Productoverzicht met vast querybudget
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-PERF-01`
- Goal: laad het productoverzicht, de naam-/omschrijvingzoeking, de archiefstand en paginering rechtstreeks en begrensd uit SQLite, zonder per product losse DTO- of voorraadqueries.
- Required branch: `codex/pilot-perf-01-product-overview`

The story is already approved. Do not restate it or ask for approval. Give a short plan, implement directly, run the checks, and provide completion notes.

Codex has already created and verified the required feature branch. Claude must stop and report `not ready` when the active branch is `master` or does not match the required branch.

## Scope

- Voeg een klein application-contract toe voor één gepagineerde productoverzicht-read: zoekterm, actieve/gearchiveerde stand, 1-based paginanummer en paginaomvang. Het resultaat bevat het totale aantal matches en uitsluitend de zichtbare producten, inclusief product, eenheid, eventuele code en actieve categorie, actieve voorraadlocaties en totale hoeveelheid.
- Implementeer dat contract in Infrastructure als een gespecialiseerde EF Core-query via de bestaande `IDbContextFactory<BootManagerDbContext>`. Gebruik `AsNoTracking`. Houd Application vrij van EF Core- en Infrastructure-afhankelijkheden.
- Laat de readquery in de database filteren op de bestaande actieve/gearchiveerde betekenis en op hoofdletterongevoelige deelmatches in naam of omschrijving. Sorteer stabiel op productnaam en daarna product-id. Tel matches in de database, haal daarna alleen de gevraagde pagina van maximaal tien producten op en haal actieve voorraadregels (`Quantity > 0`), locatie en gebied in één gebatchte query voor die product-id's op.
- Projecteer product-, eenheid-, code- en actieve-categoriegegevens rechtstreeks naar DTO's. Bouw geen `Product`-entiteiten of alle productrelaties in geheugen op. Geen actieve voorraad geeft totaal `0` en geen locaties; een productcode blijft optioneel; alleen een actieve categoriekoppeling wordt getoond.
- Registreer de Infrastructure-implementatie bij de bestaande DI-registratie.
- Vervang in `Products.razor` het ophalen van de gehele catalogus, client-side filteren/pagineren en de per-product `GetActiveStocksByProductAsync`-lus door de nieuwe gepagineerde read. Een zoekopdracht, het wissen van de zoekterm, wisselen van archiefstand en vorige/volgende pagina vragen de juiste server-side pagina op. Gebruik `TotalCount` voor de bestaande pagineringstekst en behoud 10 items per pagina.
- Houd zoekresultaatinhoud en UI-gedrag functioneel gelijk: productnaam, code, totaal, eenheid, locaties/no-stock, hoofdklik, detailpopup, bewerkactie, archiveren/heractiveren en voorraadbijzonderheid blijven werken. De bestaande vervolgflow na een hoofdklik mag voor actuele voorraad nog steeds `IStockService` raadplegen; die gebruikersactie telt niet mee als opbouw van de overzichtspagina.
- Laad categorieën, eenheden en default-eenheden niet meer bij het openen of verversen van het overzicht. Laad/initialiseer ze pas wanneer de gebruiker een productform voor aanmaken of bewerken opent, en vernieuw die form-lookups gericht na het toevoegen van een categorie of eenheid. Een deep-link naar bewerken haalt het ene doelproduct zo nodig op aanvraag op; laad daarvoor niet de volledige catalogus.

## Outside Scope

- Geen functionele UI-herontwerp, nieuwe voorraadregels, filters, scanroutes of homezoeking (`PILOT-PERF-02`).
- Geen wijziging van `StockService`-readpaden buiten de overzichtsopbouw (`PILOT-PERF-03`) en geen DbContext-lifetime- of write-flowrefactor (`PILOT-PERF-04`).
- Geen schemawijziging, migratie, nieuwe package, configuratie- of loggingbeleid.
- Geen story-, release-, TODO-, legacy-, README- of handoffwijzigingen, en geen git- of PR-acties, behalve de verplichte processtatus hieronder.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is discovered:

- nieuwe application DTO(s) en query-contract onder `BootManager.Application/Inventory/DTOs/` en `BootManager.Application/Inventory/Contracts/`;
- nieuwe EF Core-reader onder `BootManager.Infrastructure/Inventory/` (of de direct omliggende bestaande Inventory-map) en `BootManager.Infrastructure/DependencyInjection.cs` voor de registratie;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- nieuw `BootManager.IntegrationTests/Inventory/ProductOverviewReadQueryIntegrationTests.cs`;
- `.docs/processtatus/codex-pilot-perf-01-product-overview/ClaudeStatus.md`.

Do not change by default:

- `BootManager.Application/Inventory/Services/ProductService.cs`, de generieke repository of `StockService.cs`;
- `Home.razor`, scan-, opslag- of overige inventorypagina's;
- projectbestanden, migraties, entiteitsconfiguratie en globale styling;
- alle projectdocumentatie buiten de verplichte processtatus.

Before changing an additional area, explain why it is required.

## Execution Boundaries

- Before editing, verify the active branch is `codex/pilot-perf-01-product-overview` and is not `master`.
- Gebruik de bestaande Clean Architecture-richting: het contract en de DTO's leven in Application; de EF-query en `BootManagerDbContext` blijven in Infrastructure; de Razor-component bevat alleen presentatiestaat en interactie.
- Introduceer geen generieke repository-uitbreiding of nieuwe abstractielaag. Dit is een gerichte readmodel-boundary voor deze overview-story.
- Houd de query op maximaal drie databasecommando's voor een gewone overzichtspagina (count, zichtbare productprojectie, gebatchte actieve voorraad). Een eventuele form-openactie is een afzonderlijke interactie en valt buiten dit budget. Het geverifieerde budget mag in geen geval boven de storygrens van vijf uitkomen.
- Gebruik geen client-side fallback die eerst alle producten of voorraadregels laadt.
- Do not change story, release, TODO, legacy, README, handoff or other project documentation.
- Before finishing, create or update `.docs/processtatus/codex-pilot-perf-01-product-overview/ClaudeStatus.md` with the full Completion Notes and end it with `Done: yyyy-MM-dd HH:mm`.
- Do not create commits, pushes, branches, PRs, merges, releases or deployments.
- Report only `ready for Codex review` when the full technical completion definition is met; never call the story Done, accepted or production-ready.

## Minimal Context

Read:

- `CLAUDE.md`;
- this packet;
- `.docs/releases/holiday-pilot-2026.md`, only `PILOT-PERF-01`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Application/Inventory/DTOs/ProductDto.cs` and `StockDto.cs`;
- `BootManager.Application/Inventory/Contracts/IProductService.cs`, `IProductCategoryService.cs`, `IUnitService.cs` and `IStockService.cs`;
- `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs` and `BootManager.Infrastructure/DependencyInjection.cs`;
- `BootManager.Core/Entities/Product.cs`, `ProductCode.cs`, `ProductCategoryMapping.cs`, `Stock.cs`, `StorageLocation.cs` and `StorageArea.cs`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- `BootManager.IntegrationTests/Inventory/InventoryImportIntegrationTests.cs` as the local SQLite/WebApplicationFactory pattern.

Do not load by default:

- full `.docs/TODO.md`, legacy-analysis or legacy-input;
- unrelated pages, services or repository-wide source trees;
- `.codex/current-session-handoff.md` or `.codex/working-agreement.md`;
- home-search implementation (`PILOT-PERF-02` owns that scope).

## Acceptance Focus

- Het productoverzicht behoudt zoeken, archieftoggle, 10-item-paginering, detail, bewerken en de voorraadpresentatie zonder merkbare functionele regressie.
- Voor een overview-pagina worden alleen die tien (of minder) producten en hun benodigde overzichtsdata gelezen.
- De eerste pagina gebruikt maximaal vijf databasecommando's; bij meer opgeslagen producten blijft dit aantal gelijk.
- Lege actieve voorraad blijft zichtbaar als `0` met de bestaande no-stockpresentatie.
- Categorie- en eenheidlookups zijn niet langer deel van de normale paginaopbouw.

## Test Evidence Requirements

- Voeg een echte SQLite-integratietest toe met een `DbCommandInterceptor` of gelijkwaardig telmechanisme op de test-`DbContextFactory`. Deze test moet de geregistreerde production reader aanroepen, geen nagemaakte LINQ-query.
- Bewijs met representatieve data (minimaal een pagina en extra, niet-zichtbare producten) dat de reader correct filtert op actief/gearchiveerd en hoofdletterongevoelige deelmatches in naam én omschrijving; stabiel in pagina's van tien sorteert; productnaam, eenheid, optionele code, actieve categorie, actieve locaties en totaal juist projecteert; nulvoorraad uitsluit; en hoogstens vijf commando's uitvoert voor dezelfde pagina met hetzelfde budget wanneer extra niet-zichtbare producten worden toegevoegd.
- Actualiseer bUnit-tests zodat het echte `Products`-component de nieuwe reader met zoek-/archief-/paginaargumenten aanroept, resultaatinhoud rendert, bij zoeken naar pagina 1 teruggaat en bij vorige/volgende de juiste server-side pagina aanvraagt. Behoud de bestaande interactietests voor hoofdklik, detail, bewerken en no-stock.
- Inspecteer iedere nieuwe of gewijzigde test op defectgevoeligheid. Tests mogen niet alleen mocks of broncodevorm controleren: assert concrete readerargumenten, gerenderde uitkomsten en het gemeten queryaantal. Geen placeholdertests.
- Dit is een geplande performanceverbetering, geen losse bugfix; formeel red-green bewijs is niet vereist. De integratietest moet echter aantoonbaar falen wanneer de reader weer per-product voorraad- of DTO-queries uitvoert.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~ProductOverviewReadQueryIntegrationTests"
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductsComponentTests"
```

Then:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

Before accepting the command results, inspect every new or changed test and confirm that it can fail for the performance or UI behavior it claims to cover.

## Definition of Technical Completion

Report `ready for Codex review` only when:

- the full overview flow uses the dedicated database-paged reader, without loading all products or one stock query per visible product;
- filtering, archive state, stable 10-item pagination and visible data remain correct;
- the SQLite test proves a query count no higher than five and no growth when total product count grows;
- lazy form lookups and deep-link editing preserve existing product management;
- all required targeted tests, build and diffcheck pass;
- no unexplained change exists outside the expected write-set;
- remaining manual acceptance steps are listed, including desktop/mobile overview browsing, search, archive toggle, pagination, detail and edit.

Report `not ready` when any scope item is incomplete, the query count is unproven or grows with total products, a test is documentary, a UI flow regresses, build/diffcheck fails, a required decision is missing, or an extra changed area is not justified.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names and the production behavior they execute;
4. migration/configuration impact;
5. remaining risks and manual test requirements;
6. final status: `ready for Codex review` or `not ready`, with the concrete reason.
