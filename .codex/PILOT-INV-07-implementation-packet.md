# Implementation Packet

## Task

- Story ID: `PILOT-INV-07`
- Approved story: Owner-only CSV-startimport voor echte vakantievoorraad, locatie-mapping en QR-tags
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-INV-07`
- Goal: lever een Owner-only vakantie-startimport waarmee een concreet CSV-bestand de
  bestaande voorraadbeheerdata vervangt, de Owner per nieuwe CSV-locatie een
  gebied-/locatie-mapping laat bevestigen, daarna producten/voorraad/QR-tokens opbouwt
  en direct een printvriendelijk A4-overzicht voor de nieuwe locatietags oplevert,
  zonder de bestaande scan-, tag-, inventory- en autorisatieflows regressief te raken.
- Required branch: `codex/pilot-inv-07-csv-startimport`

The story is already approved. Do not restate it or ask for approval. Give a short
plan, implement directly, run the checks, and provide completion notes.

## Scope

- Add one Owner-only CSV import entry point for inventory bootstrap on the web side.
- Support the concrete CSV structure from
  `.docs/extraInfo/voorraadoverzicht_boot_zomervakantie.csv` with exactly these
  columns: `Aantal`, `Eenheid`, `Product`, `Locatie`.
- Before the actual import commit step, remove all existing inventory-management data
  except units:
  - storage areas;
  - storage locations;
  - location QR tokens/tag state;
  - products;
  - product codes;
  - active stock rows;
  - stock mutation history;
  - expected-location rows if they exist separately from active stock.
- Keep existing units intact and create missing units from the CSV when needed.
- Because the CSV only contains free-text `Locatie`, add an explicit Owner mapping step
  for each distinct CSV location value where the Owner can:
  - select an existing area or create a new area;
  - select an existing location in that area or create a new location.
- Reuse that confirmed mapping automatically for all remaining CSV rows with the exact
  same source location text.
- Import products without a required category. Keep the existing optional-category
  product model intact.
- Imported products start without product codes. The existing unknown-code flow must
  still be able to link a later scanned unknown barcode to an imported existing
  product.
- Generate a BootManager QR token for every storage location that is used by the final
  mapping and expose those locations in a print-friendly A4 overview that shows at
  least QR image, area name and location name per tag.
- Keep the change vertically focused on the Owner import flow and the resulting tag
  print output. Reuse existing storage, token and product services where practical.

## Outside Scope

- No generic import/export engine for arbitrary CSV formats.
- No Crew access to the import flow or destructive reset action.
- No automatic area inference from free-text location names without Owner
  confirmation.
- No mandatory category assignment during import.
- No redesign of `PILOT-INV-06` product overview in this story.
- No new barcode-scanning architecture; only preserve the existing code-linking flow.
- No reset of units.
- No changes to logbook, dashboard, auth model, Raspberry Pi deployment or non-inventory
  domains.
- No documentation edits, commits, pushes, branches, PRs, merges, releases or
  deployments.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Application/Inventory/` for a focused import contract/service and any
  small DTOs/results needed for parsing, preview, mapping and execution;
- `BootManager.Application/Storage/Services/StorageService.cs` and/or
  `BootManager.Application/Inventory/Services/` only where existing create/delete/token
  operations must be composed safely for the import flow;
- `BootManager.Application/DependencyInjection.cs`;
- `BootManager.Web/Components/Pages/` for one new Owner-only import page and, only if
  needed, one new Owner-only print page or a small extension of the existing tag
  overview page;
- `BootManager.Web/Components/Layout/NavMenu.razor` only if a direct Owner-only
  navigation entry is required for this flow;
- `BootManager.Web/Components/Pages/StorageLocationTagOverview.razor` only if it is the
  chosen A4 output surface;
- `BootManager.Web/Components/Pages/ScanUnknownCodeLinkProduct.razor` only if a small
  compatibility adjustment is required to keep linking imported uncategorized products
  working;
- `BootManager.UnitTests/Inventory/`, `BootManager.UnitTests/Storage/` and
  `BootManager.UnitTests/Web/` for focused bUnit/service tests;
- `BootManager.IntegrationTests/Inventory/` and/or `BootManager.IntegrationTests/Storage/`
  for destructive import behavior and relational proof on real SQLite.

Do not add a migration by default. This story should prefer the existing schema unless a
minimal schema change is truly unavoidable. Before changing an additional area, explain
why it is required.

## Execution Boundaries

- Implement only application code, migrations, configuration and tests explicitly
  required by this packet.
- Before editing, verify that the active branch matches `Required branch` and is not
  `master`.
- Do not change story, release, TODO, legacy, README, handoff or other project
  documentation.
- Do not modify `.docs/extraInfo/voorraadoverzicht_boot_zomervakantie.csv`; it is input
  evidence, not implementation output.
- Do not create commits, pushes, branches, PRs, merges, releases or deployments.
- Do not change scope, acceptance criteria or architectural direction. Stop and report
  the smallest missing decision when an approved direction cannot be followed.
- Do not weaken destructive-reset safety. The UI must clearly gate the reset/import
  action behind an explicit Owner flow instead of silently replacing data.
- Never declare the story `Done`, accepted or production-ready. Only report
  `ready for Codex review` after satisfying the technical completion definition.

## Minimal Context

Read:

- `CLAUDE.md`;
- `.codex/PILOT-INV-07-implementation-packet.md`;
- the section `PILOT-INV-07` in `.docs/releases/holiday-pilot-2026.md`;
- `.docs/extraInfo/voorraadoverzicht_boot_zomervakantie.csv`;
- `BootManager.Application/Inventory/Contracts/IProductService.cs`;
- `BootManager.Application/Inventory/Services/ProductService.cs`;
- `BootManager.Application/Inventory/Services/StockService.cs`;
- `BootManager.Application/Inventory/Services/UnitService.cs`;
- `BootManager.Application/Storage/Services/IStorageService.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Web/Components/Pages/StorageLocations.razor`;
- `BootManager.Web/Components/Pages/StorageLocationTagOverview.razor`;
- `BootManager.Web/Components/Pages/ScanUnknownCodeLinkProduct.razor`;
- `BootManager.Web/Components/Layout/NavMenu.razor`;
- `BootManager.UnitTests/Inventory/ProductServiceTests.cs`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- `BootManager.UnitTests/Storage/StorageLocationTagOverviewComponentTests.cs`;
- `BootManager.UnitTests/Web/RouteAuthorizationTests.cs`;
- `BootManager.IntegrationTests/Inventory/InventoryMigrationAndConstraintsTests.cs`;
- `BootManager.IntegrationTests/Storage/StorageMigrationAndConstraintsTests.cs`.

Do not load by default:

- full `.docs/TODO.md`;
- unrelated epic documents;
- `.docs/legacy-analysis/`;
- `.docs/legacy-input/`;
- `.codex/current-session-handoff.md`;
- repository-wide source trees.

## Existing Constraints

- Follow .NET 8 and the repository architecture rules in `CLAUDE.md`.
- `IProductService.CreateAsync` already supports `categoryId = null`; preserve that
  optional-category behavior instead of introducing a fake import category.
- Existing QR-token generation and formatting are already live in the storage flow;
  reuse the current BootManager token/value format and do not invent a second QR model.
- Existing unknown-code linking is accepted pilot behavior and must keep working for
  imported products that start without codes.
- Existing units must survive the import reset. Missing units from the CSV may be added
  additively.
- Existing storage-area and storage-location uniqueness rules remain in force. The
  import mapping UI must work with those rules instead of bypassing them.
- Prefer a transactionally safe import execution path on the application side. Avoid a
  half-imported database where some rows are deleted and the replacement import has only
  partly succeeded.
- Keep the UI practical and compact for the pilot. This is an operator-only bootstrap
  flow, not a broad management redesign.

## Acceptance Focus

- Only Owner can open and execute the import flow.
- The Owner can upload the CSV, see each distinct unknown CSV location, map it to area
  + location, and reuse that mapping for all matching rows.
- On final import, old inventory-management data is gone except units.
- Imported products, locations, stock quantities and QR tokens match the confirmed CSV
  mapping.
- Imported products can exist without category and without code.
- The existing unknown-barcode-to-existing-product link path still works afterward.
- The resulting A4 tag output is usable for printing and shows area + location per QR.
- No regressions to storage routing, tag overview authorization or existing scan flows.

## Test Evidence Requirements

- Name the production behavior or defect each new test executes.
- This is a new slice, so formal red-green bugfix proof is not required unless you fix a
  discovered pre-existing defect. If you do fix one, record equivalent defect-sensitive
  evidence.
- Require real product-code or component execution and concrete assertions on calls,
- arguments, state and outcomes.
- Forbid placeholder or documentary tests, including `Assert.True(true)`, empty test
  methods, source-shape assertions used instead of behavior, and `async` tests without
  relevant awaited behavior.
- Identify existing success and error paths that the change must preserve and require
  regression checks for them.
- For UI tests, require actual component rendering and user interaction through the
  repository's component-test framework.

Required new or changed test coverage must prove at least:

- parsing/validation of the supported CSV structure, including decimal quantities such as
  `1,5`;
- importing with preserved existing units and additive creation of missing units;
- destructive removal of old inventory data while leaving units intact;
- one-time mapping reuse for repeated source location names;
- creation of new areas/locations/products/stock rows from confirmed mappings;
- QR token generation for every imported location;
- Owner-only route authorization for the import page and any new print page;
- print overview rendering of QR tags with area + location labels;
- post-import compatibility of linking an unknown scanned code to an existing imported
  product if that path is touched.

Inspect every new or changed test and confirm that it can fail for the behavior it
claims to cover.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductServiceTests|FullyQualifiedName~ProductsComponentTests|FullyQualifiedName~StorageLocationTagOverviewComponentTests|FullyQualifiedName~RouteAuthorizationTests|FullyQualifiedName~InventoryImport"
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~Inventory|FullyQualifiedName~Storage"
```

Then:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

Before accepting the command results, inspect every new or changed test and confirm
that it can fail for the defect it claims to cover. A green test suite is not evidence
when its tests only document intended behavior.

## Definition of Technical Completion

Report `ready for Codex review` only when:

- every scope item and acceptance criterion is technically implemented;
- the import flow is Owner-only and the destructive replace behavior is explicitly
  gated;
- all targeted tests pass;
- every new or changed test executes real product behavior and contains meaningful
  assertions;
- the full required test run contains no new failure;
- build and `git diff --check` pass;
- imported locations have real QR tokens and the print output is technically available;
- products can remain uncategorized after import and still participate in the existing
  unknown-code link flow;
- no unexplained change exists outside the expected write-set;
- remaining manual acceptance steps are listed explicitly.

Report `not ready` when any scope item is incomplete, import safety is unproven, unit
preservation is unproven, a test is documentary or cannot detect the claimed behavior,
a new or changed test fails, build/diffcheck fails, a required decision is missing, or
an additional write area cannot be justified. Do not downgrade failures to warnings or
weaken tests or acceptance criteria to claim completion.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names and the production behavior they execute;
4. migration/configuration impact;
5. remaining risks and manual test requirements;
6. final status: `ready for Codex review` or `not ready`, with the concrete reason.
