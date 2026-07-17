# ClaudeStatus — PILOT-INV-06

Story: Productoverzicht herontwerpen naar dezelfde responsieve zoek- en resultaatstijl.
Branch: `codex/pilot-inv-06-products-overview` (geverifieerd actief, niet `master`).

## 1. Gewijzigde bestanden en geïmplementeerd gedrag

### `BootManager.Web/Components/Pages/Inventory/Products.razor`

- De niet-bewerkende lijstweergave is herbouwd van een generieke beheertabel naar een
  mockup-geleide, responsieve productzoek- en resultaatpagina volgens
  `.docs/analysis/stitch_responsive_bootstrap_process_design/producten_overzicht/`.
- **Kop**: eyebrow `Voorraadbeheer`, titel `Producten` en de bestaande actie
  `Nieuw product` (altijd zichtbaar, ook op mobiel).
- **Direct beschikbare zoekinvoer** boven de resultaten. Zoekt catalogusbreed op naam en
  omschrijving via `IProductService.SearchByNameOrDescriptionAsync` (hoofdletterongevoelig,
  zoals de bestaande productzoekflow). Enter voert de zoekopdracht uit; het legen van het
  veld herstelt het catalogusoverzicht.
- **Unified resultaatset**: initieel is de resultaatset de catalogus (`GetAllAsync`),
  gestuurd door de bestaande actief/gearchiveerd-toggle; een zoekopdracht vervangt de set
  door de zoekresultaten. Beide sets gebruiken exact dezelfde resultaatopbouw als de
  home-widget.
- **Per resultaat**: productnaam; totale actieve voorraad = som van de actieve
  voorraadlocaties (geladen via `IStockService.GetActiveStocksByProductAsync`, exact zoals
  home); standaardeenheid (`DefaultUnitName`); alle actieve locaties als locatiechips
  `gebied - locatie`; bij geen actieve voorraad een rustige `Geen actieve voorraad`-status
  in plaats van een ontbrekende hoeveelheid/locatie; gekoppelde productcode als secundaire
  info (`Code: <waarde>`) wanneer aanwezig — zonder nieuwe velden of codeformats.
- **Paginering per exact 10** voor zowel de initiële catalogus als gefilterde
  zoekresultaten. Een nieuwe zoekopdracht en een wijziging van de archiefstand beginnen op
  pagina 1. Het pagineringscontrol verschijnt alleen bij meer dan één pagina.
  Voorraad wordt per zichtbare pagina geladen (10 tegelijk).
- **Behouden interacties**: de primaire klik op een resultaat gebruikt de bestaande
  finding flow (`SelectProductFromSearch`) met ongewijzigd gedrag bij één locatie
  (directe navigatie), meerdere locaties (locatielijst) en geen actieve voorraad
  (no-stockmelding + verwachte locatie + `Voorraad toevoegen`). De aparte acties
  `Productdetails` (popup) en `Bewerken/code` (deep-link naar de bewerkform) blijven losse,
  van de primaire klik gescheiden knoppen. Archiveren, reactiveren, nieuw product, bewerken
  en het tonen van gearchiveerde producten blijven bereikbaar per resultaat/kop.
- **Responsief (page-local CSS-isolatie, geen JavaScript voor viewport)**: mobiel toont
  cards, desktop vanaf `768px` een compacte horizontale lijstregel; beide tonen dezelfde
  inhoud via één markup met CSS-grid-reflow.
- **Alleen op mobiel verborgen**: `Gearchiveerd/Actieve weergeven` en
  `Voorraadbijzonderheid` staan in een `.desktop-only`-container (blijven in de DOM voor
  desktopgebruik). `Nieuw product` blijft bewust wel zichtbaar op mobiel.
- **Mutatiefallback ontkoppeld**: de Voorraadbijzonderheid-modal gebruikt nu eigen
  zoekvelden (`_fallbackSearchTerm`/`_fallbackSearchResults`) zodat de nieuwe, altijd
  zichtbare hoofdzoekinvoer en de modal elkaars state niet meer overschrijven. Het
  mutatiegedrag zelf is ongewijzigd.
- Verwijderd: de generieke tabel, de expand/collapse-rij met `GetStocksByProductAsync` en
  de aparte zoekmodus-toggle (`_searchMode`/`OpenSearchMode`/`CloseSearchMode`); de
  locatie-inhoud daarvan is nu direct als chips zichtbaar.

### `BootManager.Web/Components/Pages/Inventory/Products.razor.css` (nieuw)

- Page-local CSS-isolatie voor kop, zoekbalk, resultaatcards/lijstregels, locatiechips,
  no-stockstatus, paginering en de `.desktop-only`-zichtbaarheid. Hergebruikt dezelfde
  CSS-variabelen als de home-widget voor herkenbare familiegelijkenis. Media-query op
  `768px` schakelt tussen card- en lijstregelweergave; media-query onder `768px` verbergt
  `.desktop-only`.

### `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`

- Aangepast aan de nieuwe markup (directe zoekinvoer + Enter i.p.v. zoekmodus-toggle) en
  uitgebreid met de verplichte nieuwe dekking. Reflectie-test voor de mutatiefallback
  volgt de hernoemde velden (`_fallbackSearchTerm`).

## 2. Uitgevoerde tests/checks en resultaten

- `dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductsComponentTests"`
  → **Passed! Failed: 0, Passed: 19, Skipped: 0** (geen xUnit-analyzerwaarschuwingen meer).
- `dotnet build BootManager.sln --no-restore` → **Build succeeded, 0 Error(s)** (alleen
  reeds bestaande, ongerelateerde warnings elders in de solution).
- `git diff --check` → geen whitespace-fouten. Alleen informatieve `LF will be replaced by
  CRLF`-filtermeldingen (normaal op Windows; worden bij commit door git genormaliseerd).

## 3. Nieuwe/gewijzigde testnamen en het productiegedrag dat ze uitvoeren

Nieuw:

- `Overview_InitialCatalogueWith11ActiveProducts_RendersOnly10AndPaginatesToEleventh` —
  rendert een initiële catalogus van 11 actieve producten, bewijst dat exact 10
  resultaten op pagina 1 staan met correcte naam/totaal/eenheid/locatie-inhoud, en dat het
  elfde product via `Volgende` op pagina 2 verschijnt.
- `Overview_Search_StartsOnFirstPage_LoadsStockThroughStockService` — navigeert eerst naar
  catalogus-pagina 2, voert daarna een zoekopdracht (12 resultaten) uit, en bewijst dat de
  zoekresultaten op pagina 1 beginnen (`Pagina 1 van 2`), dezelfde resultaatinhoud tonen en
  dat de actieve voorraad van een zoekresultaat via `IStockService.GetActiveStocksByProductAsync`
  is geladen.
- `Overview_ProductWithoutActiveStock_RendersNoStockState` — bewijst dat een product zonder
  actieve voorraad de bewuste `Geen actieve voorraad`-status toont, zonder locatiechips en
  zonder hoeveelheidswaarde.
- `Overview_DetailAndEditActions_AreDistinctFromPrimaryResultClick` — bewijst in het
  initiële overzicht dat de primaire klik (`button.product-main`) een ander element is dan
  de losse `Productdetails`- en `Bewerken/code`-acties.
- `DesktopOnlyControls_AreGroupedUnderDesktopOnlyContainer` — bewijst het responsieve
  zichtbaarheidscontract: `Voorraadbijzonderheid` en de archieftoggle staan onder
  `.desktop-only`, terwijl `Nieuw product` daar bewust buiten valt. (Feitelijke verberging
  is handmatige viewportcontrole, zie sectie 5.)

Gewijzigd (gedrag behouden, interactie aangepast aan de directe zoekinvoer / nieuwe markup):

- `ManualSearch_FindsProductByName_CaseInsensitive` — zoeken op naam levert het product op
  en roept `SearchByNameOrDescriptionAsync` aan.
- `ManualSearch_WithOneActiveLocation_NavigatesDirectlyToLocation` — primaire klik met één
  actieve locatie navigeert direct naar die locatie.
- `ManualSearch_WithMultipleActiveLocations_ShowsLocationListWithoutNavigating` — primaire
  klik met meerdere locaties toont de locatielijst zonder te navigeren.
- `ManualSearch_WithNoActiveStock_ShowsNoActiveStockMessage` — primaire klik zonder actieve
  voorraad toont de no-stockmelding en verwachte locatie.
- `ManualSearch_WithNoActiveStock_OpensAddStockModal` — `Voorraad toevoegen` opent de
  add-stock-modal met productselectie en locatiekeuze.
- `AdministrativeMutationFallback_ModalCanBeOpened` — de Voorraadbijzonderheid-modal opent.
- `AdministrativeMutationFallback_CallsMutateStockAsync_WhenSaved` — bewijst dat opslaan
  `IStockService.MutateStockAsync` met de juiste argumenten aanroept (velden hernoemd naar
  `_fallbackSearchTerm`).
- `ProductSearchResult_ExposesSeparateDetailAction_DistinctFromMainClick`,
  `ProductDetailAction_OpensPopupWithoutNavigating_ShowsUnitCodeAndStock`,
  `ProductDetailAction_WithNoActiveStock_ShowsNoStockWithoutCrash`,
  `ProductSearchResult_RendersDetailsAndEditActionsAsSeparateButtons`,
  `EditProductAction_NavigatesToDeepLinkWithProductId` — bewijzen dat detail-popup en
  edit-deep-link losstaan van de primaire klik en hun gedrag behouden.
- `Products_WithEditProductIdQuery_OpensEditFormWithCodeSection`,
  `Products_WithUnknownEditProductIdQuery_ShowsErrorWithoutCrash` — deep-link opent de
  bewerkform met codesectie; onbekend id toont een nette fout en laat de pagina bruikbaar
  (zoekveld aanwezig).

Red-green: deze slice is een nieuwe UX-herbouw; er is geen bestaand defect gefixt, dus
formeel red-green-bewijs is niet vereist en niet van toepassing.

## 4. Migratie-/configuratie-impact

- Geen. Geen wijzigingen aan servicecontracten, DTO's, EF Core, migraties, routes,
  autorisatie of configuratie. `IProductService`/`IStockService` blijven de bron van
  waarheid; geen nieuwe query of client-side substituut voor actieve voorraad.

## 5. Resterende risico's en noodzakelijke handmatige test

- **Handmatige viewportcontrole vereist** (niet volledig door bUnit gedekt, CSS-isolatie
  wordt in bUnit niet toegepast):
  1. Desktop (`>= 768px`): `Voorraadbeheer > Producten` toont een compacte horizontale
     lijstregel; `Gearchiveerd/Actieve weergeven` en `Voorraadbijzonderheid` zijn zichtbaar
     en bruikbaar; paginering en locatie-informatie kloppen bij meerdere producten en na
     zoeken.
  2. Mobiel (`< 768px`): resultaten tonen als cards; `Gearchiveerd/Actieve weergeven` en
     `Voorraadbijzonderheid` zijn niet zichtbaar; `Nieuw product` en de zoekinvoer blijven
     bruikbaar.
  3. Vergelijk de pagina globaal met de aangeleverde mockup en controleer dat de
     taakhiërarchie (kop + actie, zoekbalk, resultaatcards met naam/totaal/eenheid/locaties)
     herkenbaar overeenkomt en familie is van de home-widget.
- **Prestatie-observatie**: bij zeer grote catalogi worden per pagina 10 losse
  `GetActiveStocksByProductAsync`-aanroepen gedaan (zelfde patroon als de home-widget).
  Binnen de pilotomvang acceptabel; een batchquery valt buiten de scope van deze story.
- **Zoektrigger**: zoeken gebeurt op Enter (zoals home), niet live per toetsaanslag; het
  legen van het veld herstelt de catalogus. Bevestig dat dit de gewenste interactie is bij
  de handmatige acceptatie.

## 6. Eindstatus

**ready for Codex review** — alle scope- en acceptatie-items zijn technisch geïmplementeerd:
de generieke tabel is vervangen door de mockup-geleide card-/lijsthiërarchie, 10-item
paginering werkt op zowel het initiële overzicht als na zoeken, elk resultaat toont
naam/totaal/eenheid/locaties of de bewuste no-stockstatus, en de bestaande primaire klik-,
detail-, edit-, archiveer-, reactiveer- en no-stockflows zijn behouden. Alle 19 gerichte
tests slagen, de build slaagt en `git diff --check` is schoon. Er zijn geen wijzigingen
buiten de verwachte write-set.

---

# Review Fix 01 — no-stock toont hoeveelheid en eenheid (PILOT-INV-06-review-fix-packet-01)

Branch: `codex/pilot-inv-06-products-overview` (geverifieerd actief, niet `master`).

## 1. Gewijzigde bestanden en geïmplementeerd gedrag

- `BootManager.Web/Components/Pages/Inventory/Products.razor`: de `.product-stock`-tak
  rendert de totale hoeveelheid en `DefaultUnitName` nu zodra de voorraad geladen is
  (`stockLoaded`) in plaats van alleen bij aanwezige voorraad (`hasStock`). Hierdoor toont
  een product zonder actieve voorraad in het resultaat altijd de totale hoeveelheid `0`
  (som van geen locaties) en zijn standaardeenheid. De expliciete status
  `Geen actieve voorraad` blijft behouden en er worden geen locatiechips getoond, want er
  zijn geen actieve locaties. Alleen tijdens het laden (`!stockLoaded`) blijft het
  hoeveelheidsblok leeg (de locatiekolom toont dan `Laden…`).
- Geen wijziging aan voorraadberekening, services, DTO's, mutaties, zoeken, paginering,
  responsieve CSS, routes of andere productinteractie.

## 2. Gewijzigde test, uitgevoerd productiegedrag en red-green-bewijs

- Gewijzigde test: `Overview_ProductWithoutActiveStock_RendersNoStockState`
  (`BootManager.UnitTests/Inventory/ProductsComponentTests.cs`).
- Productiegedrag: rendert via echte bUnit-rendering één actief product met bekende
  standaardeenheid `stuk` en zonder actieve voorraad (`GetActiveStocksByProductAsync` →
  lege lijst), en eist nu alle vier: zichtbare totale hoeveelheid `0` (`.stock-value` == "0"),
  de standaardeenheid `stuk`, de status `Geen actieve voorraad`, en de afwezigheid van
  locatiechips (`.location-chip` leeg). De eerdere assertie dat de hoeveelheid ontbrak
  (`Assert.Empty(.stock-value)`) is verwijderd.
- Red-green-bewijs:
  - **Red**: met de oude markup (`@if (hasStock)`) draait de aangepaste test rood met
    `Bunit.ElementNotFoundException : No elements were found that matches the selector
    '.product-result .stock-value'` — de no-stock-tak rendert dan geen hoeveelheid of
    eenheid. (Bevestigd door de fix tijdelijk terug te draaien en de test te draaien:
    Failed 1, Passed 0.)
  - **Green**: na de correctie (`@if (stockLoaded)`) toont het no-stock-resultaat `0` +
    `stuk` + `Geen actieve voorraad` zonder chips en slaagt de test.

## 3. Uitgevoerde checks en resultaten

- `dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductsComponentTests"`
  → **Passed! Failed: 0, Passed: 19, Skipped: 0**.
- `dotnet build BootManager.sln --no-restore` → **Build succeeded, 0 Error(s)**.
- `git diff --check` → geen whitespace-fouten (alleen informatieve LF→CRLF-filtermeldingen,
  normaal op Windows).

## 4. Resterende handmatige acceptatie

- De handmatige viewport- en mockupcontroles uit sectie 5 hierboven blijven gelden.
- Aanvullend: controleer visueel dat een product zonder actieve voorraad `0` met de
  eenheid toont naast de status `Geen actieve voorraad`, zowel in de mobiele card- als de
  desktop-lijstweergave.

## 5. Eindstatus

**ready for Codex review** — de no-stock-correctie is doorgevoerd: elk resultaat, ook
zonder actieve voorraad, toont productnaam, hoeveelheid (`0`), eenheid en locatiestatus.
De gewijzigde test dekt het gedrag met echte rendering en meetbare assertions en heeft
red-green-bewijs. Alle 19 gerichte tests slagen, de build slaagt en `git diff --check` is
schoon. Er zijn geen wijzigingen buiten de verwachte write-set.

---

# Review Fix 02 — opnieuw kunnen bewerken na Annuleren (PILOT-INV-06-review-fix-packet-02)

Branch: `codex/pilot-inv-06-products-overview` (geverifieerd actief, niet `master`).

## 1. Gewijzigde bestanden en geïmplementeerd gedrag

- `BootManager.Web/Components/Pages/Inventory/Products.razor` — `CancelForm`: wanneer de
  bewerkform via de deep-link (`?editProductId=<id>`) is geopend (`EditProductId.HasValue`),
  navigeert Annuleren nu naar `/inventory/products` en verwijdert daarmee de
  `editProductId`-queryparameter. De daaropvolgende parametercyclus (`OnParametersSet` met
  `EditProductId == null`) zet `_handledEditProductId` terug op `null`, zodat een tweede klik
  op `Bewerken` voor hetzelfde product de deep-link opnieuw activeert en de bestaande form
  weer opent. Voor een nieuw, nog niet opgeslagen product (geen queryparameter) blijft de
  bestaande terugkeer naar het overzicht behouden zonder onnodige routewijziging.
- Alleen de annuleerroute van de deeplink-bewerkflow is aangepast. Geen wijziging aan
  productgegevens, opslaan, voorraad, zoeken, paginering, responsieve presentatie, services,
  DTO's of andere routes.

## 2. Nieuwe regressietest, uitgevoerd productiegedrag en red-green-bewijs

- Nieuwe test: `DeepLinkEdit_AfterCancel_CanReopenEditForSameProduct`
  (`BootManager.UnitTests/Inventory/ProductsComponentTests.cs`).
- Productiegedrag (echte bUnit-rendering + knopinteracties, geen reflectie): de pagina wordt
  via `?editProductId=<id>` gerenderd zodat de bewerkform opent; daarna klikt de test in de
  form op `Annuleren` en verifieert dat het overzicht weer verschijnt (zoekveld aanwezig, geen
  naam-invoer) én dat `editProductId` uit de URL is verdwenen; vervolgens klikt de test in het
  overzicht opnieuw op de `Bewerken/code`-actie van hetzelfde product en verifieert dat de
  deep-link (`?editProductId=<id>`) opnieuw actief is en de bewerkform weer geopend is.
- Red-green-bewijs:
  - **Red**: met de oude `CancelForm` (`_editMode = false;` zonder navigatie) faalt de test met
    `Assert.DoesNotContain() Failure: Sub-string found — "/inventory/products?editProductId=…"`:
    de queryparameter blijft na Annuleren staan, waardoor een tweede `Bewerken` op hetzelfde
    product de form niet heropent. (Bevestigd door de fix tijdelijk terug te draaien: Failed 1,
    Passed 0.)
  - **Green**: na de correctie verwijdert Annuleren de query, keert terug naar het overzicht en
    heropent een tweede `Bewerken` de form; de test slaagt (Passed 1).
- De bestaande tests voor de onbekende-deeplinkfout
  (`Products_WithUnknownEditProductIdQuery_ShowsErrorWithoutCrash`) en de normale
  bewerknavigatie (`EditProductAction_NavigatesToDeepLinkWithProductId`,
  `Products_WithEditProductIdQuery_OpensEditFormWithCodeSection`) blijven ongewijzigd en slagen.

## 3. Uitgevoerde checks en resultaten

- `dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductsComponentTests"`
  → **Passed! Failed: 0, Passed: 20, Skipped: 0**.
- `dotnet build BootManager.sln --no-restore` → **Build succeeded, 0 Error(s)**.
- `git diff --check` → geen whitespace-fouten (alleen informatieve LF→CRLF-filtermeldingen,
  normaal op Windows).

## 4. Resterende handmatige acceptatie

- De handmatige viewport- en mockupcontroles uit de eerdere secties blijven gelden.
- Aanvullend: open op desktop een product via `Bewerken`, klik `Annuleren`, en controleer dat
  de URL geen `editProductId` meer bevat en dat opnieuw `Bewerken` op hetzelfde product de
  form weer opent. Controleer tevens dat `Annuleren` bij een nieuw product (via `Nieuw product`)
  gewoon terugkeert naar het overzicht.

## 5. Eindstatus

**ready for Codex review** — de deeplink-annuleerregressie is opgelost: na `Bewerken` →
`Annuleren` verdwijnt `editProductId` uit de URL en kan hetzelfde product direct opnieuw
bewerkt worden. De nieuwe regressietest dekt de volledige zichtbare volgorde met echte
rendering/knopinteracties en heeft red-green-bewijs; bestaande deeplink- en bewerknavigatie-
tests blijven slagen. Alle 20 gerichte tests slagen, de build slaagt en `git diff --check` is
schoon. Er zijn geen wijzigingen buiten de verwachte write-set.

Done: 2026-07-17 13:11
