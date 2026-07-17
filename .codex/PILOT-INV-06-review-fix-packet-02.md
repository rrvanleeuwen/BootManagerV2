# Review Fix Packet

## Task

- Story ID: `PILOT-INV-06`
- Review correction: een product opnieuw kunnen bewerken na `Annuleren`
- Required branch: `codex/pilot-inv-06-products-overview`
- Defect: na openen via `?editProductId=<id>` sluit `CancelForm` alleen de form. De
  queryparameter blijft staan, waardoor opnieuw kiezen voor `Bewerken` op hetzelfde
  product niet opnieuw het bewerkscherm opent.

The story is already approved. Do not restate it or ask for approval. Verify that the
active branch matches `codex/pilot-inv-06-products-overview` and is not `master`, then
implement this correction directly.

## Scope

- Pas alleen de annuleerroute van de bestaande deeplink-bewerkflow aan.
- Wanneer de form via `editProductId` geopend is, moet `Annuleren`:
  - terugkeren naar het productoverzicht;
  - `editProductId` uit de URL verwijderen;
  - de deeplink-state zodanig resetten dat een tweede klik op `Bewerken` voor hetzelfde
    product de bestaande form opnieuw opent.
- Bij een nieuw, nog niet opgeslagen product moet `Annuleren` de huidige bestaande
  terugkeer naar het overzicht behouden, zonder een onnodige routewijziging.
- Voeg een echte bUnit-regressietest toe voor de volledige zichtbare volgorde:
  deeplink opent bewerken, `Annuleren` toont weer het overzicht en verwijdert de query,
  opnieuw `Bewerken` voor hetzelfde product leidt weer naar de deeplink/form. Gebruik
  waar nodig de normale bUnit-router/parametercyclus om die laatste navigatie te
  renderen; test geen private velden met reflectie.

## Outside Scope

- Geen wijziging aan productgegevens, opslaan, voorraad, zoeken, paginering,
  responsieve presentatie, services, DTO's of routes buiten het verwijderen van deze
  queryparameter bij annuleren.
- Geen documentatie-, commit-, push-, branch-, PR-, merge-, release- of
  deploymentacties, behalve de verplichte processtatus hieronder.

## Expected Write-Set

- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- `.docs/processtatus/codex-pilot-inv-06-products-overview/ClaudeStatus.md`.

Do not change another area unless a compile-time dependency makes it necessary; explain
that reason before doing so.

## Evidence Requirements

- The regression test must execute the rendered component and button interactions, not
  merely inspect source, a private field or `NavigationManager.Uri` in isolation.
- Record red-green evidence. The new test must fail on the current implementation
  because cancelling leaves the same `editProductId` query active and a repeat edit of
  the same product cannot re-open the form, then pass after the correction.
- Preserve the existing unknown-deeplink error behavior and normal product edit
  navigation tests.

## Required Checks

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductsComponentTests"
dotnet build BootManager.sln --no-restore
git diff --check
```

## Completion Notes

Update `.docs/processtatus/codex-pilot-inv-06-products-overview/ClaudeStatus.md` with:

1. changed files and implemented behavior;
2. exact new/changed regression test, production behavior it executes, and red-green evidence;
3. checks and results;
4. remaining manual acceptance steps;
5. final status: `ready for Codex review` or `not ready`.

End the file with a separate line `Done: yyyy-MM-dd HH:mm`. Do not declare the story
accepted or production-ready.
