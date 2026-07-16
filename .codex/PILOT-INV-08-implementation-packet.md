# Implementation Packet

## Task

- Story ID: `PILOT-INV-08`
- Approved story: Product-zoekdetails en A4-tagbatchprint vanuit bestaande beheerflows
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-INV-08`
- Goal: verwijder twee resterende bedieningsomwegen in de vakantiepilot door in
  `Voorraadbeheer > Producten` naast het bestaande zoekklikgedrag een expliciete
  detailactie met compacte productpopup toe te voegen, en in `Opslag > Tagoverzicht`
  een directe batchprintactie naar de bestaande A4-tagweergave beschikbaar te maken,
  zonder de huidige navigatie-, voorraad- of taglogica regressief te raken.
- Required branch: `codex/pilot-inv-08-product-details-tagbatchprint`

The story is already approved. Do not restate it or ask for approval. Give a short
plan, implement directly, run the checks, and provide completion notes.

## Scope

- Keep the existing primary click behavior of a product search result in
  `Voorraadbeheer > Producten` unchanged.
- Add a second explicit visible detail action per search result, such as an info
  button, that opens a compact product-properties popup in place without direct
  navigation.
- The popup must show at least:
  - product name;
  - standard unit;
  - linked product code when available;
  - relevant active stock summary for the selected product.
- Reuse the existing home/product detail presentation pattern where practical instead
  of inventing a second unrelated detail modal.
- Add a direct batchprint action in `Opslag > Tagoverzicht` that opens the existing A4
  tag print route for all currently available location tags.
- Keep the existing A4 tag layout and ensure the print route still supports multiple
  pages automatically when many tags are present.

## Outside Scope

- No redesign of the broader `Voorraadbeheer > Producten` page from `PILOT-INV-06`.
- No change to the existing main click result behavior in product search.
- No new stock mutation logic, inventory calculations or scan flows.
- No new QR formats, label formats, exports or PDF engines beyond the existing A4 print
  route.
- No broad product details redesign outside the compact popup needed for this story.
- No documentation edits, commits, pushes, branches, PRs, merges, releases or
  deployments.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Web/Components/Inventory/ProductDetailsFromHome.razor` or one small new
  neighboring inventory detail component if reuse by parameterization is cleaner;
- `BootManager.Web/Components/Pages/StorageLocationTagOverview.razor`;
- `BootManager.Web/Components/Pages/StorageLocationTagPrintOverview.razor` only if a
  small route/input adjustment is required for explicit batchprint entry;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- `BootManager.UnitTests/Storage/StorageLocationTagOverviewComponentTests.cs`;
- `BootManager.UnitTests/Storage/StorageLocationTagPrintOverviewComponentTests.cs`;
- `BootManager.UnitTests/Web/RouteAuthorizationTests.cs` only if a route or
  authorization surface changes.

Do not introduce migrations or service-layer redesign by default. Before changing an
additional area, explain why it is required.

## Execution Boundaries

- Implement only application code, configuration and tests explicitly required by this
  packet.
- Before editing, verify that the active branch matches `Required branch` and is not
  `master`.
- Do not change story, release, TODO, legacy, README, handoff or other project
  documentation.
- Before finishing, create or update
  `.docs/processtatus/codex-pilot-inv-08-product-details-tagbatchprint/ClaudeStatus.md`.
- Put the full `Completion Notes` content in that `ClaudeStatus.md` file and end the
  file with a separate line `Done: yyyy-MM-dd HH:mm`.
- Treat that `Done:` line only as a handoff signal for Codex review, never as a claim
  that the story is accepted or production-ready.
- Do not create commits, pushes, branches, PRs, merges, releases or deployments.
- Do not change scope, acceptance criteria or architectural direction. Stop and report
  the smallest missing decision when an approved direction cannot be followed.
- Never declare the story `Done`, accepted or production-ready. Only report
  `ready for Codex review` after satisfying the technical completion definition.

## Minimal Context

Read:

- `CLAUDE.md`;
- `.codex/PILOT-INV-08-implementation-packet.md`;
- the section `PILOT-INV-08` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Web/Components/Inventory/ProductDetailsFromHome.razor`;
- `BootManager.Web/Components/Pages/StorageLocationTagOverview.razor`;
- `BootManager.Web/Components/Pages/StorageLocationTagPrintOverview.razor`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- `BootManager.UnitTests/Storage/StorageLocationTagOverviewComponentTests.cs`;
- `BootManager.UnitTests/Storage/StorageLocationTagPrintOverviewComponentTests.cs`.

Do not load by default:

- full `.docs/TODO.md`;
- unrelated epic documents;
- `.docs/legacy-analysis/`;
- `.docs/legacy-input/`;
- `.codex/current-session-handoff.md`;
- repository-wide source trees.

## Existing Constraints

- Follow .NET 8 and the repository architecture rules in `CLAUDE.md`.
- `Products.razor` already has an accepted manual search flow whose result click may
  navigate directly to one location, show multiple locations, or open the add-stock
  fallback; preserve that behavior exactly.
- `ProductDetailsFromHome.razor` already establishes a product detail modal pattern
  with stock lookup; prefer reuse or a narrowly aligned variant over a second unrelated
  popup style.
- `StorageLocationTagOverview.razor` is currently Owner-only and already lists the
  location/tag management rows; the new batchprint entry should feel like a direct
  management action, not a separate hidden workflow.
- `StorageLocationTagPrintOverview.razor` already renders all available QR-tagged
  locations and uses a print layout; keep the same rendering and let CSS/browser page
  flow continue handling multi-page output.
- Keep the change compact and pilot-practical. This is a UX shortcut story, not a
  structural inventory rewrite.

## Acceptance Focus

- Product search results in `Voorraadbeheer > Producten` still perform the current main
  click behavior unchanged.
- Each search result also exposes a separate visible detail action.
- Activating that detail action opens a compact popup without direct navigation.
- The popup shows product name, standard unit, linked code when present and a relevant
  stock/location summary.
- `Opslag > Tagoverzicht` exposes a direct action for batchprinting all available tags.
- The batchprint action opens the existing A4 print surface and remains usable when the
  tag set spans multiple printed pages.

## Test Evidence Requirements

- Name the production behavior or defect each new test executes.
- This is a new slice, so formal red-green bugfix proof is not required unless you fix a
  discovered pre-existing defect. If you do fix one, record equivalent defect-sensitive
  evidence.
- Require real product-code or component execution and concrete assertions on calls,
  arguments, state and outcomes.
- Forbid placeholder or documentary tests, including `Assert.True(true)`, empty test
  methods, source-shape assertions used instead of behavior, and `async` tests without
  relevant awaited behavior.
- Identify existing success and error paths that the change must preserve and require
  regression checks for them.
- For UI tests, require actual component rendering and user interaction through the
  repository's component-test framework.

Required new or changed test coverage must prove at least:

- the new detail action is separately clickable from the main search result click;
- clicking the detail action opens a compact product popup without navigating away;
- the popup shows linked code when available and handles the no-active-stock case
  without crashing;
- the existing main search result click still keeps its single-location direct
  navigation and multi-location/no-stock behaviors;
- the tag overview renders a direct batchprint action;
- the batchprint action targets the existing print route for all tags, not a new export
  surface;
- the print overview still renders all QR-tagged locations with area and location labels
  after any route/input change.

Inspect every new or changed test and confirm that it can fail for the behavior it
claims to cover.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductsComponentTests|FullyQualifiedName~StorageLocationTagOverviewComponentTests|FullyQualifiedName~StorageLocationTagPrintOverviewComponentTests"
```

Then:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

Before accepting the command results, inspect every new or changed test and confirm
that it can fail for the defect it claims to cover. A green test suite is not evidence
when its tests only document intended behavior.

## Definition of Technical Completion

Report `ready for Codex review` only when:

- every scope item and acceptance criterion is technically implemented;
- the detail action is visibly distinct from the main search result click;
- the existing main search result behavior is preserved;
- the tag overview exposes the new batchprint shortcut and the existing print page still
  renders all available tags;
- all targeted tests pass;
- every new or changed test executes real product behavior and contains meaningful
  assertions;
- the full required test run contains no new failure;
- build and `git diff --check` pass;
- no unexplained change exists outside the expected write-set;
- remaining manual acceptance steps are listed explicitly.

Report `not ready` when any scope item is incomplete, the main search click behavior has
changed, the batchprint route is unproven, a test is documentary or cannot detect the
claimed behavior, a new or changed test fails, build/diffcheck fails, a required
decision is missing, or an additional write area cannot be justified. Do not downgrade
failures to warnings or weaken tests or acceptance criteria to claim completion.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names and the production behavior they execute;
4. migration/configuration impact;
5. remaining risks and manual test requirements;
6. final status: `ready for Codex review` or `not ready`, with the concrete reason.
