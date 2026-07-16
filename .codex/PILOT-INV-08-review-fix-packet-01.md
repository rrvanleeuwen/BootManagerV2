# Review Fix Packet 01

## Task

- Story ID: `PILOT-INV-08`
- Fix scope: voeg naast de bestaande productzoekresultaat-acties een directe
  productbewerkactie toe, zodat de gebruiker vanuit een zoekresultaat meteen het juiste
  product kan openen om een barcode/code toe te voegen of te wijzigen.
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-INV-08`
- Required branch: `codex/pilot-inv-08-product-details-tagbatchprint`

The current implementation is ready for Codex review except for this manual
acceptance follow-up. Implement this small fix directly on the current branch and then
report back for Codex review. Do not ask for story approval.

## User Feedback

- De batchprintactie is goed en hoeft niet aangepast te worden.
- Het nieuwe detailicoon is goed en moet blijven.
- Een tweede actieknop in hetzelfde productzoekresultaat is wenselijk om snel naar het
  specifieke product in `Voorraadbeheer > Producten` te gaan en daar direct een barcode
  toe te voegen of te wijzigen.
- Reden: de productlijst is inmiddels te lang om handmatig naar het betreffende product
  te scrollen en daarna pas te bewerken.

## Scope

- Add a second explicit action button next to the existing `Details` action in each
  search result.
- The new action must open the existing product edit form for that specific product.
- Use a URL that includes the product id so the edit target is deep-linkable, for
  example `/inventory/products?editProductId=<productId>`.
- When the products page is opened with that id, load the normal products data and open
  the existing edit form for the matching product.
- The edit form must be the existing product form with the existing `Gekoppelde code`
  section, so the user can add, replace or remove a code without scrolling the full
  products list.
- Preserve:
  - the existing main click behavior of a search result;
  - the existing `Details` popup behavior;
  - the accepted batchprint behavior.

## Outside Scope

- No redesign of the full product overview.
- No new barcode scanning flow.
- No new product edit page outside `Products.razor`.
- No changes to stock, storage, QR tag printing, import, dashboard or logbook logic.
- No documentation status updates, commits, pushes, PRs, merges, releases or
  deployments.

## Expected Write-Set

Only change these files unless a compile-time dependency proves otherwise:

- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- `.docs/processtatus/codex-pilot-inv-08-product-details-tagbatchprint/ClaudeStatus.md`.

Do not change the tag overview or print overview for this fix unless a regression is
discovered.

## Execution Boundaries

- Verify that the active branch is exactly
  `codex/pilot-inv-08-product-details-tagbatchprint` and is not `master`.
- Do not change story, release, TODO, legacy, README, handoff or other project
  documentation.
- Update
  `.docs/processtatus/codex-pilot-inv-08-product-details-tagbatchprint/ClaudeStatus.md`
  with fresh completion notes for this fix and end the file with a new separate line
  `Done: yyyy-MM-dd HH:mm`.
- Do not create commits, pushes, branches, PRs, merges, releases or deployments.
- Report `ready for Codex review` only if the fix and required checks are complete.

## Minimal Context

Read:

- `CLAUDE.md`;
- `.codex/PILOT-INV-08-review-fix-packet-01.md`;
- `.codex/PILOT-INV-08-implementation-packet.md` only for the original boundaries;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`.

Do not load by default:

- unrelated docs;
- full legacy analysis;
- repository-wide source trees.

## Implementation Notes

- Prefer `[SupplyParameterFromQuery]` for the deep link if it fits the existing Blazor
  component style.
- Ensure the page handles both first load and same-page navigation to a different
  `editProductId` cleanly.
- If the id does not match a loaded product, keep the page usable and show a small
  error message instead of crashing.
- Avoid opening the add-code subform automatically unless this is already a local
  pattern. Opening the edit form with the visible `Gekoppelde code` section is enough.

## Required Test Coverage

Add or update bUnit tests that prove:

- each search result renders the existing `Details` action and the new edit/code action
  as separate buttons;
- clicking the new edit/code action navigates to `/inventory/products?editProductId=...`
  or otherwise updates the URL with the selected product id;
- opening `Products.razor` with a valid `editProductId` loads the matching product and
  opens the existing edit form with the `Gekoppelde code` section visible;
- an unknown `editProductId` does not crash and leaves a clear error or usable fallback;
- the existing main result click behavior and `Details` popup behavior remain covered
  by the current tests.

Tests must execute real component behavior through bUnit and contain concrete
assertions. Do not add placeholder or source-shape tests.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductsComponentTests"
```

Then:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names and the production behavior they execute;
4. migration/configuration impact;
5. remaining risks and manual test requirements;
6. final status: `ready for Codex review` or `not ready`, with the concrete reason.
