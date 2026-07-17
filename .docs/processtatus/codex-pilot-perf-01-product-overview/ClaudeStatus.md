# PILOT-PERF-01 — Productoverzicht met vast querybudget — ClaudeStatus

## 1. Gewijzigde bestanden en geïmplementeerd gedrag

**Nieuw (Application):**
- `BootManager.Application/Inventory/DTOs/ProductOverviewItemDto.cs` — één zichtbaar
  overzichtsitem: `Product` (ProductDto), `ActiveLocations` (StockDto-lijst) en `TotalQuantity`.
- `BootManager.Application/Inventory/DTOs/ProductOverviewPageDto.cs` — `TotalCount` plus
  uitsluitend de zichtbare producten van de gevraagde pagina.
- `BootManager.Application/Inventory/Contracts/IProductOverviewReadQuery.cs` — contract met
  `GetPageAsync(searchTerm, showArchived, pageNumber, pageSize, ct)`. Application blijft vrij van
  EF Core-/Infrastructure-afhankelijkheden.

**Nieuw (Infrastructure):**
- `BootManager.Infrastructure/Inventory/ProductOverviewReadQuery.cs` — EF Core-implementatie via
  de bestaande `IDbContextFactory<BootManagerDbContext>` met `AsNoTracking`. Filtert database-zijdig
  op archiefstand (`ArchivedAt`) en op hoofdletterongevoelige deelmatches in naam of omschrijving
  (`ToLower().Contains(...)`), sorteert stabiel op productnaam en daarna product-id, telt matches in
  de database, haalt daarna alleen de gevraagde pagina op met directe DTO-projectie van product,
  eenheid, optionele code en actieve categoriekoppeling, en haalt de actieve voorraadregels
  (`Quantity > 0`) inclusief locatie en gebied in één gebatchte query voor de zichtbare product-id's.
  Er worden geen `Product`-entiteiten of alle productrelaties in geheugen opgebouwd; geen actieve
  voorraad geeft totaal `0` en geen locaties.

**Gewijzigd (Infrastructure):**
- `BootManager.Infrastructure/DependencyInjection.cs` — registreert
  `IProductOverviewReadQuery` → `ProductOverviewReadQuery` (scoped) bij de bestaande DI-registratie.

**Gewijzigd (Web):**
- `BootManager.Web/Components/Pages/Inventory/Products.razor` — het overzicht gebruikt nu de
  gepagineerde reader in plaats van de volledige catalogus + client-side filteren/pagineren + de
  per-product `GetActiveStocksByProductAsync`-lus. Zoeken, wissen van de zoekterm, wisselen van
  archiefstand en vorige/volgende pagina vragen de juiste server-side pagina op; `TotalCount` stuurt
  de bestaande pagineringstekst; 10 items per pagina behouden. Categorieën, eenheden,
  default-eenheden en icoonsleutels worden niet meer bij het openen/verversen van het overzicht
  geladen, maar lazy via `EnsureFormLookupsLoadedAsync()` bij het openen van een aanmaak-/bewerkform,
  en gericht vernieuwd na het toevoegen van een categorie of eenheid. De deep-link naar bewerken
  (`?editProductId=`) haalt via `OnParametersSetAsync` het ene doelproduct op aanvraag op met
  `IProductService.GetByIdAsync` in plaats van de volledige catalogus te laden. Hoofdklik
  (finding flow), detailpopup, bewerken, archiveren/heractiveren, no-stockpresentatie en de
  voorraadbijzonderheid blijven functioneel gelijk; de vervolgflow na een hoofdklik raadpleegt nog
  steeds `IStockService`.

## 2. Uitgevoerde tests/checks en resultaten

- `dotnet test BootManager.IntegrationTests --filter "FullyQualifiedName~ProductOverviewReadQueryIntegrationTests"`
  → **Passed: 3, Failed: 0**.
- `dotnet test BootManager.UnitTests --filter "FullyQualifiedName~ProductsComponentTests"`
  → **Passed: 21, Failed: 0**.
- `dotnet build BootManager.sln` → **Build succeeded, 0 Errors**.
- `git diff --check` → schoon (alleen een informatieve LF→CRLF regeleinde-normalisatiemelding op
  `ProductsComponentTests.cs`, geen whitespace-/conflictfouten).

## 3. Nieuwe/gewijzigde testnamen en het uitgevoerde productiegedrag

**Integratietests (`BootManager.IntegrationTests/Inventory/ProductOverviewReadQueryIntegrationTests.cs`)** —
roepen de via DI geregistreerde production reader (`IProductOverviewReadQuery`) aan op echte SQLite;
een `DbCommandInterceptor` (`CommandCountingInterceptor`, PRAGMA-huishouding uitgesloten) op de
test-`DbContextFactory` telt de uitgevoerde SQL-commando's:
- `GetPage_FiltersArchiveAndSearch_ProjectsData_AndExcludesZeroStock` — bewijst met
  representatieve data dat de reader actief/gearchiveerd filtert, hoofdletterongevoelig deelmatcht op
  naam én omschrijving (zoekterm `"SaP"` matcht `Appelsap` via naam en `Bronwater` via omschrijving
  `"Verse SAPPEN…"`), nulvoorraad (`Plank` qty 0) uitsluit terwijl actieve voorraad (`Kast` qty 5)
  wel telt, product/eenheid/optionele code/actieve categorie correct projecteert, een gedeactiveerde
  categoriekoppeling niet toont, en dat de gearchiveerde stand met dezelfde term alleen het
  archiefproduct oplevert.
- `GetPage_OrdersByNameThenId_AndPagesInTensStably` — 12 producten in willekeurige invoegvolgorde;
  bewijst stabiele sortering op naam en paginering per tien (pagina 1 = `Prod 01..10`, pagina 2 =
  `Prod 11..12`) met correcte `TotalCount`.
- `GetPage_UsesFixedQueryBudget_AndDoesNotGrowWithMoreProducts` — meet het gemeten queryaantal voor
  pagina 1 (met 12 producten, 10 zichtbaar met actieve voorraad), bewijst `<= 5` databasecommando's,
  voegt daarna 48 extra niet-zichtbare producten met voorraad toe en bewijst dat het aantal voor
  dezelfde pagina gelijk blijft (`<= 5` en exact gelijk aan de eerste meting). Dit faalt aantoonbaar
  wanneer de reader weer per-product voorraad- of DTO-queries zou uitvoeren (10 zichtbare producten
  zouden het aantal ver boven vijf duwen).

**bUnit-tests (`BootManager.UnitTests/Inventory/ProductsComponentTests.cs`)** — renderen het echte
`Products`-component en asserten concrete reader-argumenten, gerenderde uitkomsten en interactie:
- `Overview_InitialPageWith11Products_RendersOnly10_AndRequestsSecondServerSidePage` — reader levert
  pagina 1 (10 items) en pagina 2 (het elfde); asserteert 10 gerenderde resultaten, naam/totaal/
  eenheid/locatie-inhoud, pagineringstekst, en verifieert dat "Volgende" de reader server-side voor
  pagina 2 aanroept (`GetPageAsync(null, false, 2, 10)`).
- `Overview_Search_ReturnsToFirstPage_AndRequestsServerSideSearchPage` — vanaf pagina 2 van de
  catalogus terug naar pagina 1 bij zoeken; verifieert `GetPageAsync("zoek", false, 1, 10)` en de
  gerenderde resultaatinhoud.
- `Overview_ProductWithoutActiveStock_RendersNoStockState` — reader-item zonder locaties rendert
  totaal `0`, de eenheid, de no-stockstatus en geen locatiechips.
- `ArchiveToggle_RequestsArchivedFirstPageFromReader` — de archieftoggle vraagt
  `GetPageAsync(null, true, 1, 10)` op en toont het gearchiveerde product met de reactiveer-actie.
- `ManualSearch_RequestsReaderWithSearchTerm` — zoekinvoer roept de reader met de zoekterm aan.
- Behouden interactietests op reader-geleverde data: `ManualSearch_WithOneActiveLocation…`,
  `…WithMultipleActiveLocations…`, `…WithNoActiveStock_ShowsNoActiveStockMessage`,
  `…WithNoActiveStock_OpensAddStockModal` (hoofdklik-finding flow via `IStockService`),
  `ProductDetailAction_OpensPopupWithoutNavigating…`, `…WithNoActiveStock_ShowsNoStockWithoutCrash`,
  `ProductSearchResult_ExposesSeparateDetailAction…`, `…RendersDetailsAndEditActionsAsSeparateButtons`,
  `Overview_DetailAndEditActions_AreDistinctFromPrimaryResultClick`,
  `DesktopOnlyControls_AreGroupedUnderDesktopOnlyContainer`,
  `AdministrativeMutationFallback_ModalCanBeOpened`,
  `AdministrativeMutationFallback_CallsMutateStockAsync_WhenSaved`.
- Deep-link/bewerken op aanvraag: `EditProductAction_NavigatesToDeepLinkWithProductId`,
  `Products_WithEditProductIdQuery_FetchesTargetProductOnDemand_OpensEditFormWithCodeSection`
  (verifieert `IProductService.GetByIdAsync`), `Products_WithUnknownEditProductIdQuery_ShowsErrorWithoutCrash`,
  `DeepLinkEdit_AfterCancel_CanReopenEditForSameProduct`.

## 4. Migratie-/configuratie-impact

- Geen. Geen schemawijziging, migratie, nieuwe package, configuratie- of loggingbeleid. Uitsluitend
  één nieuwe scoped DI-registratie in `BootManager.Infrastructure/DependencyInjection.cs`.

## 5. Resterende risico's en noodzakelijke handmatige test

- De hoofdletterongevoelige deelmatch leunt op SQLite `lower()`/`instr()` (ASCII); dit dekt de
  bestaande gegevens. Niet-ASCII casing valt buiten deze story-scope (gelijk aan de eerdere
  `ToLowerInvariant`-aanpak).
- Sorteervolgorde is nu database-gestuurd (naam, dan id) in plaats van de eerdere
  repository-invoegvolgorde; dit is de door de story vereiste stabiele sortering.
- Handmatige acceptatie (aanbevolen): open `Voorraadbeheer > Producten` op **desktop en mobiel**,
  zoek op een deel van naam **en** omschrijving, blader **vooruit en terug**, wissel naar
  **gearchiveerd** en terug, open **detail** en **bewerken** (ook via de deep-link), en controleer in
  de EF-log dat het queryaantal per pagina begrensd blijft en dat aantallen, eenheden en locaties
  gelijk zijn aan de bestaande gegevens.

## 6. Eindstatus

**ready for Codex review** — De volledige overzichtsflow gebruikt de gerichte database-gepagineerde
reader zonder alle producten te laden en zonder één voorraadquery per zichtbaar product; filtering,
archiefstand, stabiele 10-item-paginering en zichtbare data blijven correct; de SQLite-test bewijst
een queryaantal van hoogstens vijf zonder groei bij meer producten; lazy form-lookups en deep-link
bewerken behouden het bestaande productbeheer; alle vereiste gerichte tests, de solution-build en
`git diff --check` slagen; er zijn geen onverklaarde wijzigingen buiten de verwachte write-set.

Done: 2026-07-17 14:32
