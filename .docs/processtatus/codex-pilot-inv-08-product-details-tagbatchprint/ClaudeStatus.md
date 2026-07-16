# ClaudeStatus — PILOT-INV-08

Story: Product-zoekdetails en A4-tagbatchprint vanuit bestaande beheerflows.
Branch: `codex/pilot-inv-08-product-details-tagbatchprint`.

## 1. Gewijzigde bestanden en geïmplementeerd gedrag

- `BootManager.Web/Components/Pages/Inventory/Products.razor`
  - Zoekresultaat is nu een flex-rij met twee losse knoppen: de bestaande hoofdklik
    (`SelectProductFromSearch`, ongewijzigd gedrag: directe navigatie bij één locatie,
    meerdere-locatieslijst, of add-stock/geen-voorraad fallback) én een aparte
    zichtbare detailknop ("Details", `title="Productdetails"`).
  - De detailknop roept `ShowProductDetailsPopup(product)` aan en opent de compacte
    producteigenschappen-popup in context, zonder navigatie. `CloseProductDetailsPopup`
    sluit de popup. Nieuwe state: `_showProductDetailsPopup`, `_detailsPopupProduct`.
  - Popup wordt gerenderd door hergebruik van het bestaande `ProductDetailsFromHome`.

- `BootManager.Web/Components/Inventory/ProductDetailsFromHome.razor`
  - Toont nu ook de gekoppelde code (waarde + formaat) wanneer aanwezig. Naam,
    standaardeenheid en actieve voorraad-/locatiesamenvatting waren al aanwezig.
    Wijziging is optioneel/conditioneel en laat het bestaande home-gebruik ongemoeid.

- `BootManager.Web/Components/Pages/StorageLocationTagOverview.razor`
  - Directe batchprint-beheeractie toegevoegd ("Alle tags afdrukken") in de header,
    die via `NavigationManager` naar de bestaande route `/storage/tag-print-overview`
    navigeert. `NavigationManager` geïnjecteerd; `OpenBatchPrint()` toegevoegd.

- `BootManager.Web/Components/Pages/StorageLocationTagPrintOverview.razor`
  - Geen wijziging nodig: deze route rendert al alle QR-getagde locaties en de
    bestaande CSS (`page-break-inside: avoid`) plus browser-pageflow verzorgen
    automatisch meerdere pagina's. Geen route-/input-aanpassing vereist.

Testbestanden:
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`
- `BootManager.UnitTests/Storage/StorageLocationTagOverviewComponentTests.cs`
- `BootManager.UnitTests/Storage/StorageLocationTagPrintOverviewComponentTests.cs`

`RouteAuthorizationTests.cs` niet aangepast: geen nieuwe route of autorisatiegrens.

## 2. Tests/checks en resultaten

- Gerichte tests:
  `dotnet test ... --filter "FullyQualifiedName~ProductsComponentTests|...TagOverviewComponentTests|...TagPrintOverviewComponentTests"`
  → Passed: 23, Failed: 0.
- `dotnet build BootManager.sln --no-restore` → 0 Errors (alleen bestaande warnings).
- `git diff --check` → schoon (exit 0).

Defect-gevoeligheid geverifieerd: met de detailknop tijdelijk onherkenbaar gemaakt
(`title` gewijzigd) faalden de drie nieuwe Products-detailtests rood (Failed: 3);
na herstel weer groen. Overige nieuwe tests sturen echt gedrag (navigatie-URI,
gerenderde QR-kaarten) en falen zonder de betreffende productwijziging.

## 3. Nieuwe/gewijzigde testnamen en uitgevoerd productiegedrag

- `ProductsComponentTests.ProductSearchResult_ExposesSeparateDetailAction_DistinctFromMainClick`
  — bewijst dat elk zoekresultaat naast de hoofdklikknop een apart zichtbaar
  detailknop-element ("Details", `title=Productdetails`) heeft dat niet dezelfde knop is.
- `ProductsComponentTests.ProductDetailAction_OpensPopupWithoutNavigating_ShowsUnitCodeAndStock`
  — klik op de detailactie opent de popup met productnaam, eenheid, gekoppelde code en
  voorraad-/locatiesamenvatting, terwijl de URI ongewijzigd blijft ook al heeft het
  product precies één actieve locatie (waar de hoofdklik wél zou navigeren).
- `ProductsComponentTests.ProductDetailAction_WithNoActiveStock_ShowsNoStockWithoutCrash`
  — popup toont de "Geen actieve voorraad"-staat zonder crash en zonder codesectie bij
  een product zonder code en zonder voorraad.
- `StorageLocationTagOverviewComponentTests.Component_RendersBatchPrintAction`
  — het tagoverzicht rendert de zichtbare batchprint-knop.
- `StorageLocationTagOverviewComponentTests.Component_BatchPrintAction_NavigatesToExistingPrintRoute`
  — klik op de batchprint-knop navigeert naar `/storage/tag-print-overview`.
- `StorageLocationTagPrintOverviewComponentTests.RendersAllTaggedLocationsForBatchPrint_AcrossManyLocations`
  — bij 12 getagde locaties rendert de printroute alle 12 kaarten met gebied-/locatienaam
  en 12 QR-afbeeldingen (bewijst dat batchprint niet aftopt; browser pagineert de rest).

Bestaande regressiedekking behouden: `ManualSearch_WithOneActiveLocation_NavigatesDirectlyToLocation`,
`..._WithMultipleActiveLocations_ShowsLocationListWithoutNavigating`,
`..._WithNoActiveStock_ShowsNoActiveStockMessage`, `..._OpensAddStockModal` bewijzen dat
het hoofdklikgedrag ongewijzigd is; de tag-print `RendersQrTagsWithAreaAndLocation` en
`ShowsInfoMessageWhenNoTaggedLocations` blijven groen.

## 4. Migratie-/configuratie-impact

Geen. Geen EF-migraties, geen DI-/configuratiewijzigingen, geen nieuwe dependencies.

## 5. Resterende risico's en handmatige test

- De multi-pagina-uitvoer bij grote aantallen tags is inherent browser-/printgedrag
  (CSS `page-break-inside: avoid`) en is niet in een unit-test af te dwingen; handmatig
  te verifiëren via de printdialoog.
- Handmatige acceptatietest (uit story):
  1. Open `Voorraadbeheer > Producten`, zoek een bestaand product.
  2. Klik de nieuwe "Details"-actie in een zoekresultaat; controleer de compacte popup
     met naam, eenheid, code (indien aanwezig) en voorraadsamenvatting.
  3. Klik daarna het zoekresultaat zelf; controleer dat navigatie/terugvindgedrag
     ongewijzigd is.
  4. Open `Opslag > Tagoverzicht`, gebruik "Alle tags afdrukken".
  5. Controleer dat de bestaande A4-tagweergave voor alle tags opent en bij grote
     aantallen over meerdere pagina's verdeelt.

## 6. Eindstatus

`ready for Codex review` — alle scope-items en acceptatiecriteria technisch
geïmplementeerd; hoofdklikgedrag aantoonbaar behouden; detailactie zichtbaar losstaand;
batchprint-shortcut aanwezig en gericht op de bestaande printroute; alle gerichte tests
groen; build en `git diff --check` schoon; geen wijziging buiten de verwachte write-set.

Dit is uitsluitend een handoff-signaal voor Codex-review, geen acceptatie of
productieklaar-verklaring.

Done: 2026-07-16 16:52

---

# Review Fix 01 — directe productbewerkactie vanuit zoekresultaat

Packet: `.codex/PILOT-INV-08-review-fix-packet-01.md`.
Branch: `codex/pilot-inv-08-product-details-tagbatchprint`.

## 1. Gewijzigde bestanden en geïmplementeerd gedrag

- `BootManager.Web/Components/Pages/Inventory/Products.razor`
  - Naast de bestaande "Details"-knop heeft elk zoekresultaat nu een tweede
    zichtbare actieknop "Bewerken" (`title="Bewerken/code"`). Beide actieknoppen
    staan gegroepeerd rechts in de resultaatrij; de hoofdklik en de Details-popup
    blijven ongewijzigd.
  - `NavigateToEditProduct(product)` navigeert naar de deep-link
    `/inventory/products?editProductId=<id>`.
  - Nieuwe query-parameter `EditProductId` via
    `[Parameter, SupplyParameterFromQuery(Name = "editProductId")]`.
  - `OnParametersSet()` verwerkt de deep-link: bij eerste weergave (na dataload) en
    bij same-page navigatie naar een ander id wordt via `OpenEditForm` de bestaande
    productbewerkform geopend, inclusief de bestaande "Gekoppelde code"-sectie. Een
    guard (`_handledEditProductId`) voorkomt heropenen bij elke re-render. Een onbekend
    id zet een nette melding ("Product niet gevonden voor bewerken.") en laat de
    lijstweergave bruikbaar zonder crash. De add-code-subform wordt niet automatisch
    geopend; de code-sectie is zichtbaar zoals in de bestaande form.
  - Batchprint, Details-popup en `ProductDetailsFromHome` zijn niet aangeraakt.

Buiten deze fix zijn geen andere bestanden gewijzigd (tagoverzicht/printoverzicht
ongemoeid; geen migraties/DI/config).

## 2. Tests/checks en resultaten

- `dotnet test ... --filter "FullyQualifiedName~ProductsComponentTests"`
  → Passed: 14, Failed: 0.
- `dotnet build BootManager.sln --no-restore` → 0 Errors (bestaande warnings).
- `git diff --check` → schoon (exit 0).

Defect-gevoeligheid geverifieerd: met de productlookup in `OnParametersSet` tijdelijk
uitgeschakeld faalde `Products_WithEditProductIdQuery_OpensEditFormWithCodeSection` rood;
na herstel weer groen. De nieuwe knop-/navigatietests falen zonder de betreffende
productwijziging (ontbrekende knop → `Single()` faalt; ontbrekende navigatie → URI
ongewijzigd).

## 3. Nieuwe/gewijzigde testnamen en uitgevoerd productiegedrag

- `ProductsComponentTests.ProductSearchResult_RendersDetailsAndEditActionsAsSeparateButtons`
  — elk zoekresultaat rendert de bestaande Details-actie én de nieuwe Bewerken-actie als
  twee losse knoppen.
- `ProductsComponentTests.EditProductAction_NavigatesToDeepLinkWithProductId`
  — klik op de Bewerken-actie navigeert naar `/inventory/products?editProductId=<id>`.
- `ProductsComponentTests.Products_WithEditProductIdQuery_OpensEditFormWithCodeSection`
  — pagina geopend met geldige `editProductId` laadt het product en opent de bestaande
  bewerkform met naam-veld en de zichtbare "Gekoppelde code"-sectie (incl. codewaarde).
- `ProductsComponentTests.Products_WithUnknownEditProductIdQuery_ShowsErrorWithoutCrash`
  — onbekend `editProductId` toont "Product niet gevonden voor bewerken.", opent geen
  form en laat de lijstweergave bruikbaar (geen crash).

Bestaande dekking behouden en groen: `ManualSearch_*` (hoofdklikgedrag),
`ProductDetailAction_*` (Details-popup), `AdministrativeMutationFallback_*`.

## 4. Migratie-/configuratie-impact

Geen.

## 5. Resterende risico's en handmatige test

- De deep-link opent de bewerkform; het daadwerkelijk toevoegen/vervangen/verwijderen
  van een code verloopt via de al bestaande code-sectie (ongewijzigd, reeds gedekt).
- Handmatige test:
  1. Open `Voorraadbeheer > Producten`, zoek een product.
  2. Klik "Bewerken" in het zoekresultaat; controleer dat de bewerkform van juist dat
     product opent met de zichtbare "Gekoppelde code"-sectie.
  3. Voeg/wijzig een code en sla op.
  4. Controleer dat Details-popup, hoofdklik en batchprint ongewijzigd werken.
  5. Open handmatig een URL met een niet-bestaand `editProductId` en controleer de
     nette melding zonder crash.

## 6. Eindstatus

`ready for Codex review` — tweede actieknop toegevoegd met deep-link naar de bestaande
bewerkform (incl. code-sectie), deep-link-handling voor eerste load en same-page
navigatie, nette fallback bij onbekend id; Details en batchprint ongemoeid; alle
gerichte tests groen; build en `git diff --check` schoon; wijziging binnen de
verwachte write-set. Handoff-signaal voor Codex-review, geen acceptatie- of
productieklaar-verklaring.

Done: 2026-07-16 17:27
