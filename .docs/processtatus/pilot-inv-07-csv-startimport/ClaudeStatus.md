# Completion Notes — PILOT-INV-07 (review-fix ronde)

Story: Owner-only CSV-startimport voor echte vakantievoorraad, locatie-mapping en QR-tags
Branch: `codex/pilot-inv-07-csv-startimport`
Context: follow-up review-fix na de eerste implementatieronde — uitsluitend de twee geaccepteerde bevindingen.

## 1. Changed files and implemented behavior

- `BootManager.Web/Components/Pages/Inventory/InventoryImport.razor`
  - **Review-bevinding 1 opgelost:** het locatieveld in de mappingstap is nu gebied-bewust selecteerbaar. Per mapping-rij wordt een `<datalist id="existing-locations-{index}">` gerenderd met de bestaande locatienamen die horen bij het in die rij gekozen gebied (hoofdletterongevoelig gefilterd). De Owner kan zo een bestaande locatie kiezen óf een nieuwe naam typen (vrije-tekst blijft mogelijk).
  - De keuzelijst reageert op het gekozen gebied doordat het gebiedveld met `@bind:event="oninput"` bindt en de rij-datalist bij elke render opnieuw wordt berekend via de nieuwe helper `ExistingLocationsForArea(areaName)`.
  - Databron is de bestaande `IStorageService.GetAllLocationsOverviewAsync()` (AreaName + LocationName); er is **geen applicatieservice gewijzigd** en **geen tweede locatiemodel** geïntroduceerd. Bestaande gebied-datalist, destructieve waarschuwing, verplichte bevestigingscheckbox, importsemantiek en resultaatoverzicht zijn ongewijzigd.
  - Toegevoegd: `@using BootManager.Application.Storage.DTOs`, veld `_existingLocations`, laden van `_existingLocations` bij bestandskeuze, helper `ExistingLocationsForArea`.

- `BootManager.UnitTests/Inventory/InventoryImportComponentTests.cs`
  - Constructor voorziet nu ook een standaard-mock voor `GetAllLocationsOverviewAsync`; helper `SetupExistingLocations(...)` toegevoegd.
  - Twee nieuwe echte bUnit-tests toegevoegd (zie §3).

- `.docs/processtatus/pilot-inv-07-csv-startimport/ClaudeStatus.md`
  - **Review-bevinding 2 opgelost:** dit bestand herschreven en afgesloten met exact het vereiste `Done: yyyy-MM-dd HH:mm`-format.

## 2. Tests/checks and results
- Targeted unit (`FullyQualifiedName~InventoryImportComponentTests`): **5 passed, 0 failed** (3 bestaand + 2 nieuw).
- `dotnet build BootManager.sln --no-restore`: **0 errors** (alleen pre-existing warnings).
- `git diff --check`: schoon (alleen informatieve LF→CRLF-meldingen op niet door mij gewijzigde `.codex`-bestanden).
- Volledige unit-run (extra controle): **511 passed, 5 failed, 1 skipped**. De 5 failures zijn identiek aan de eerdere ronde en **pre-existing** (`ProductCategoryServiceTests` ×3, `StorageServiceTests.GetLocationDetailAsync_ReturnsDetail_WithValidId`, `StorageServiceTagStatusTests.GetLocationDetail_IncludesTagStatus`) — niet gerelateerd aan de importpagina. Geen nieuwe failures.

## 3. Nieuwe/gewijzigde testnamen en welk productiegedrag ze uitvoeren

**InventoryImportComponentTests** (echte bUnit-render + InputFile-upload):
- `Mapping_ExistingLocationsSelectable_AndReactToChosenArea` — na upload en het kiezen van gebied "Kombuis" bevat de rij-keuzelijst `#existing-locations-0` alleen "Kruidenkast" (niet "Rugleuning"); na wisselen naar "Salon" bevat dezelfde keuzelijst "Rugleuning" (niet "Kruidenkast"). Bewijst zowel selecteerbaarheid van bestaande locaties als de gebied-afhankelijke filtering.
- `Mapping_AllowsFreeTextNewLocation_NotInExistingList` — met bestaande locaties aanwezig typt de Owner een nieuwe naam "Nieuwe plank" die niet in de keuzelijst staat, bevestigt en importeert; verifieert dat `ExecuteImportAsync` wordt aangeroepen met `LocationName == "Nieuwe plank"` en `AreaName == "Kombuis"`. Bewijst dat vrije-tekst nieuwe-locatie-invoer behouden blijft.
- Ongewijzigd behouden en nog steeds groen: `Upload_ShowsMappingStepWithDistinctLocations`, `Confirm_ExecutesImportWithFilledMappings`, `ImportButton_DisabledUntilConfirmedAndMapped` (bewijst dat de destructieve gating ongewijzigd werkt: importknop geblokkeerd tot alle mappings volledig zijn én de bevestiging is aangevinkt).

## 4. Migratie/configuratie-impact
Geen. Geen migratie, geen DI-wijziging, geen nieuwe dependency; alleen UI + UI-tests.

## 5. Resterende risico's en handmatige test
- **Handmatige acceptatie:** login als Owner → `/inventory/import`; upload CSV; kies per rij een gebied en controleer dat de locatie-keuzelijst uitsluitend bestaande locaties van dát gebied toont en meebeweegt bij het wisselen van gebied; kies een bestaande locatie in één rij en typ een nieuwe locatienaam in een andere rij; controleer dat de importknop geblokkeerd blijft tot alle mappings volledig zijn en de bevestiging is aangevinkt; voer de import uit en controleer het resultaatoverzicht.
- **Ongewijzigd residueel risico (uit vorige ronde, buiten deze fix-scope):** de import kent geen expliciete DB-transactieseam; mitigatie blijft validate-first-then-execute.
- **Categorieën:** blijven bewust behouden (expliciete gebruikersbeslissing); geen code toegevoegd om ze te wissen.

## 6. Eindstatus
**ready for Codex review** — beide review-bevindingen opgelost: (1) gebied-bewuste selectie van bestaande locaties met behoud van vrije-tekst nieuwe-locatie-invoer; (2) processtatusbestand met het exacte `Done:`-format. Targeted tests groen, build en `git diff --check` slagen, destructieve gating ongewijzigd, geen wijzigingen buiten de toegestane write-set (`InventoryImport.razor`, `InventoryImportComponentTests.cs`, deze processtatusfile).

Done: 2026-07-16 14:51
