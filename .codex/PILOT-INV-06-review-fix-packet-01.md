# Review Fix Packet

## Task

- Story ID: `PILOT-INV-06`
- Review correction: no-stock-resultaten moeten nog steeds hoeveelheid en eenheid tonen
- Required branch: `codex/pilot-inv-06-products-overview`
- Review finding: de story vereist dat **elk** resultaat productnaam, hoeveelheid,
  eenheid en locaties toont. De huidige no-stock-tak laat alleen `Geen actieve voorraad`
  zien en rendert geen hoeveelheid of eenheid.

The story is already approved. Do not restate it or ask for approval. Verify that the
active branch matches `codex/pilot-inv-06-products-overview` and is not `master`, then
implement this correction directly.

## Scope

- In `Products.razor`, laat een product zonder actieve voorraad in het resultaat altijd
  totale hoeveelheid `0` en zijn `DefaultUnitName` zien.
- Behoud daarbij de expliciete status `Geen actieve voorraad` en toon geen
  locatiechips, want er zijn geen actieve locaties.
- Pas de no-stock-componenttest aan zodat die de zichtbare `0`, de eenheid en de
  no-stockstatus eist. De test mag niet langer bevestigen dat de hoeveelheid ontbreekt.

## Outside Scope

- Geen wijziging aan voorraadberekening, services, DTO's, mutaties, zoeken,
  paginering, responsieve CSS, routes of andere productinteractie.
- Geen documentatie-, commit-, push-, branch-, PR-, merge-, release- of
  deploymentacties, behalve de verplichte processtatus hieronder.

## Expected Write-Set

- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- `.docs/processtatus/codex-pilot-inv-06-products-overview/ClaudeStatus.md`.

Do not change another area unless a compile-time dependency makes it necessary; explain
that reason before doing so.

## Evidence Requirements

- Use real bUnit component rendering.
- The changed no-stock test must render a product with a known standard unit and no
  active stocks, then assert all of:
  - `0` as the visible total;
  - the product's standard unit;
  - `Geen actieve voorraad`;
  - absence of location chips.
- This is a review correction for an already visible acceptance gap. Record the
  equivalent red-green evidence: explain that the changed assertion fails against the
  current no-stock markup because it has no `.stock-value` or unit, then passes after
  the correction.
- Do not weaken this test into a source-shape assertion.

## Required Checks

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductsComponentTests"
dotnet build BootManager.sln --no-restore
git diff --check
```

## Completion Notes

Update `.docs/processtatus/codex-pilot-inv-06-products-overview/ClaudeStatus.md` with:

1. changed files and implemented behavior;
2. exact changed test name, production behavior it executes, and red-green evidence;
3. checks and results;
4. remaining manual acceptance steps;
5. final status: `ready for Codex review` or `not ready`.

End the file with a separate line `Done: yyyy-MM-dd HH:mm`. Do not declare the story
accepted or production-ready.
