# Implementation Packet

## Task

- Story ID: `PILOT-SCAN-02`
- Approved story: Parallelle scan-reworkbasis met `old`-isolatie van de huidige flow
- Story source: `.docs/releases/holiday-pilot-2026.md`
- Goal: isoleer de huidige scanimplementatie expliciet als `old`, houd die tijdelijk bruikbaar en maak de codebase vrij voor een nieuwe scanimplementatie met definitieve naamgeving
- Required branch: `codex/scanflow-herdefinitie`

The story is already approved. Do not restate it or ask for approval. Give a
short plan, implement directly, run the checks, and provide completion notes.

Codex must create and verify the required feature branch before giving this packet to
Claude. Claude must stop and report `not ready` when the active branch is `master` or
does not match the required branch.

## Scope

- Verplaats de huidige volledige `/scan`-implementatie naar een expliciete `old`-page
  of gelijkwaardige `old`-module binnen `BootManager.Web\Components\Pages`.
- Zorg dat de oude flow via een expliciete `old`-route bereikbaar blijft.
- Laat de applicatie tijdens deze overgang nog steeds een bruikbare scanroute hebben.
- Reserveer de canonieke scannaamgeving voor de nieuwe flow door de huidige oude
  implementatie niet meer als de definitieve scanmodule te positioneren.
- Werk navigatie en tests bij zodat oud versus nieuw expliciet is.

## Outside Scope

- Nieuwe scanstart-UI volgens het nieuwe design.
- Nieuwe locatiegerichte scanmodus.
- Nieuwe productgerichte scanmodus.
- Definitieve onbekende-code-flow volgens de nieuwe basis.
- Verwijderen van de oude flow.
- Aanpassingen aan release-, README-, TODO-, handoff- of analysedocumentatie.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Web\Components\Pages\Scan.razor`
- `BootManager.Web\Components\Pages\Scan.razor.css`
- nieuwe scan-gerelateerde page/componentbestanden onder
  `BootManager.Web\Components\Pages\` die nodig zijn om `old` expliciet te scheiden
- `BootManager.Web\Components\Layout\NavMenu.razor`
- eventueel `BootManager.Web\Components\Routes.razor` alleen als dat technisch nodig is
  voor de overgangsroute
- `BootManager.UnitTests\Storage\ScanComponentTests.cs`
- `BootManager.UnitTests\Web\NavMenuComponentTests.cs`

Before changing an additional area, explain why it is required.

## Execution Boundaries

- Implement only application code, migrations, configuration and tests explicitly
  required by this packet.
- Before editing, verify that the active branch matches `Required branch` and is not
  `master`.
- Do not change story, release, TODO, legacy, README, handoff or other project
  documentation.
- Do not create commits, pushes, branches, PRs, merges, releases or deployments.
- Do not change scope, acceptance criteria or architectural direction. Stop and report
  the smallest missing decision when an approved direction cannot be followed.
- Never declare the story `Done`, accepted or production-ready. Only report
  `ready for Codex review` after satisfying the technical completion definition.

## Minimal Context

Read:

- `.codex\PILOT-SCAN-02-implementation-packet.md`
- de sectie `PILOT-SCAN-02` in `.docs/releases/holiday-pilot-2026.md`
- `.docs/analysis/ScannenFlow/scanflow-herdefinitie.md`
- `.docs/analysis/ScannenFlow/scanflow-ui-richtlijnen.md`
- `BootManager.Web\Components\Pages\Scan.razor`
- `BootManager.Web\Components\Pages\Scan.razor.css`
- `BootManager.Web\Components\Layout\NavMenu.razor`
- `BootManager.UnitTests\Storage\ScanComponentTests.cs`
- `BootManager.UnitTests\Web\NavMenuComponentTests.cs`

Do not load by default:

- full `.docs/TODO.md`;
- unrelated epic documents;
- `.docs/legacy-analysis/`;
- `.docs/legacy-input/`;
- `.codex/current-session-handoff.md`;
- repository-wide source trees.

## Existing Constraints

- De huidige scanflow in `Scan.razor` bevat meerdere bestaande werkende paden:
  - locatie-QR naar locatie-detail;
  - productscan naar inventory-flow;
  - onbekende-code-afhandeling;
  - mutatieflow en handmatige fallback.
- Alleen `NavMenu.razor` verwijst op dit moment rechtstreeks naar `scan`.
- De bestaande bUnit testset rendert de componentklasse `Scan` direct; bij de `old`-
  isolatie moet de testbasis expliciet mee veranderen.
- De oude flow moet tijdelijk functioneel blijven totdat latere stories de nieuwe flow
  opleveren en handmatige acceptatie rond is.

## Acceptance Focus

- Oud versus nieuw is expliciet herkenbaar in code en routing.
- De oude flow blijft tijdens deze story bruikbaar.
- De codebase krijgt een veilige basis waarop `PILOT-SCAN-03` de nieuwe scanstart met
  canonieke naamgeving kan bouwen.
- Navigatie en tests weerspiegelen bewust de tijdelijke overgangssituatie.

## Test Evidence Requirements

- Name the production behavior or defect each new test executes.
- For a bugfix or review correction, require red-green evidence: the regression test
  fails against the existing defect and passes after the fix. If a prior red run is
  technically impossible, require the concrete reason and an equivalent proof before
  implementation continues.
- Require real product-code or component execution and concrete assertions on calls,
  arguments, state and outcomes.
- Forbid placeholder or documentary tests, including `Assert.True(true)`, empty test
  methods, source-shape assertions used instead of behavior, and `async` tests without
  relevant awaited behavior.
- Identify existing success and error paths that the change must preserve and require
  regression checks for them.
- For UI tests, require actual component rendering and user interaction through the
  repository's component-test framework.
- Existing behavior that must stay proven in this story:
  - bekende locatie-QR blijft naar de locatiepagina leiden;
  - onbekende BootManager-QR blijft role-based afhandelen;
  - handmatige scaninvoer blijft beschikbaar op de oude flow zolang die actief is;
  - scan-navigatie blijft zichtbaar voor ingelogde gebruikers via de tijdelijke route.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ScanComponentTests|FullyQualifiedName~NavMenuComponentTests"
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
- all targeted tests pass;
- required red-green or equivalent defect-sensitive evidence is recorded;
- every new or changed test executes real product behavior and contains meaningful
  assertions;
- the full required test run contains no new failure;
- build and `git diff --check` pass;
- migration or compatibility behavior is proven when relevant;
- no unexplained change exists outside the expected write-set;
- remaining manual acceptance steps are listed explicitly.

Report `not ready` when any scope item is incomplete, migration/compatibility is
unproven, red-green evidence is missing, a test is documentary or cannot detect the
claimed defect, a new or changed test fails, build/diffcheck fails, a required decision
is missing, or an additional write area cannot be justified. Do not downgrade failures
to warnings or weaken tests or acceptance criteria to claim completion.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names, the production behavior they execute and red-green
   evidence for fixes;
4. migration/configuration impact;
5. remaining risks and manual test requirements;
6. final status: `ready for Codex review` or `not ready`, with the concrete reason.
