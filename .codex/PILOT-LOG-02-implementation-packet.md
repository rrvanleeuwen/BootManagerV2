# Implementation Packet

## Task

- Story ID: `PILOT-LOG-02`
- Approved story: Gebeurteniskeuze, weericonen en notitie
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-LOG-02`
- Goal: maak het handmatige logboekmoment uit `PILOT-LOG-01` praktisch bruikbaar door
  direct na `Moment vastleggen` snel een gebeurtenis, weerconditie en korte notitie te
  kunnen opslaan, terwijl de bestaande NMEA-snapshot en Draft-flow intact blijven.
- Required branch: `codex/pilot-log-02`

The story is already approved. Do not restate it or ask for approval. Give a short
plan, implement directly, run the checks, and provide completion notes.

Codex has created and verified the required feature branch. Stop and report `not ready`
when the active branch is `master` or does not match the required branch.

## Scope

- Reuse the existing `Moment vastleggen` action on an open trip and keep the
  `PILOT-LOG-01` behavior that first creates a manual Draft with the latest known
  onboard snapshot.
- After a successful manual capture, place that new Draft directly into a task-focused
  edit flow on `Logbook.razor`, so the user can immediately choose:
  - one gebeurtenis from the approved pilot list;
  - one weerconditie from the approved pilot list, shown as large understandable icons;
  - one short free notitie, reusing the existing `Remarks` field instead of adding a
    second note field.
- Persist gebeurtenis and weerconditie as stable domain values in the logbook entry
  model and database. Do not store icon names, CSS classes or rendered labels as the
  source of truth.
- Keep existing manual and generic entry editing able to read, save and display the new
  gebeurtenis- and weerwaarden for both Draft and Confirmed regels.
- Show the saved gebeurtenis, weerconditie and notitie later in the existing logbook
  views at least on:
  - `BootManager.Web/Components/Pages/Logbook.razor`;
  - `BootManager.Web/Components/Pages/LogbookEntryDetails.razor`.
- Keep the existing automatic/manual snapshot fields, Draft status and confirmation flow
  unchanged apart from carrying the new optional values.
- Add and prove the required EF Core migration for the new persisted fields, including
  upgrade-path evidence from the current latest migration.

## Outside Scope

- No changes to the event list beyond the approved pilot choices.
- No new NMEA sentence types, weather sensors, ingestion logic or automatic event
  detection.
- No new free-text rich editor, attachments redesign, route map, passage planning,
  export/reporting or print-layout redesign.
- No replacement of `Remarks`; reuse it as the short notitie field for this story.
- No broader logbook CRUD redesign outside the direct support needed for this story.
- No documentation edits, commits, pushes, branches, PRs, merges, releases or
  deployments.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Core/Entities/LogbookEntry.cs`;
- `BootManager.Core/Enums/` for new stable event/weather enum definitions;
- `BootManager.Application/Logbook/DTOs/LogbookEntryDto.cs`;
- `BootManager.Application/Logbook/DTOs/SaveLogbookEntryDto.cs`;
- `BootManager.Application/Logbook/DTOs/LogbookEntryDetailDto.cs`;
- `BootManager.Application/Logbook/Services/ILogbookService.cs` only if a public API
  change is strictly required;
- `BootManager.Application/Logbook/Services/LogbookService.cs`;
- `BootManager.Application/Logbook/Services/LogbookEntryDetailService.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/LogbookEntryConfiguration.cs`;
- the new EF migration pair plus `BootManagerDbContextModelSnapshot.cs`;
- `BootManager.Web/Components/Pages/Logbook.razor`;
- `BootManager.Web/Components/Pages/LogbookEntryDetails.razor`;
- `BootManager.UnitTests/Logbook/LogbookServiceTests.cs`;
- `BootManager.UnitTests/Logbook/LogbookComponentTests.cs`;
- a new targeted migration test under `BootManager.IntegrationTests/Logbook/`;
- `.docs/processtatus/codex-pilot-log-02/ClaudeStatus.md` (required handoff only).

Before changing an additional area, explain why it is required.

## Execution Boundaries

- Implement only application code, migrations and tests explicitly required by this
  packet.
- Before editing, verify that the active branch matches `codex/pilot-log-02` and is
  not `master`.
- Do not change story, release, TODO, legacy, README, handoff or other project
  documentation.
- Before finishing, create or update
  `.docs/processtatus/codex-pilot-log-02/ClaudeStatus.md`.
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
- `.codex/PILOT-LOG-02-implementation-packet.md`;
- the section `PILOT-LOG-02` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Core/Entities/LogbookEntry.cs`;
- `BootManager.Application/Logbook/DTOs/LogbookEntryDto.cs`;
- `BootManager.Application/Logbook/DTOs/SaveLogbookEntryDto.cs`;
- `BootManager.Application/Logbook/DTOs/LogbookEntryDetailDto.cs`;
- `BootManager.Application/Logbook/Services/ILogbookService.cs`;
- `BootManager.Application/Logbook/Services/LogbookService.cs`;
- `BootManager.Application/Logbook/Services/LogbookEntryDetailService.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/LogbookEntryConfiguration.cs`;
- `BootManager.Web/Components/Pages/Logbook.razor`;
- `BootManager.Web/Components/Pages/LogbookEntryDetails.razor`;
- `BootManager.UnitTests/Logbook/LogbookServiceTests.cs`;
- `BootManager.UnitTests/Logbook/LogbookComponentTests.cs`;
- one existing migration-upgrade integration test as local pattern reference, for
  example `BootManager.IntegrationTests/Inventory/StockMigrationTests.cs`.

Do not load by default:

- full `.docs/TODO.md`;
- unrelated epic documents;
- `.docs/legacy-analysis/`;
- `.docs/legacy-input/`;
- `.codex/current-session-handoff.md`;
- repository-wide source trees.

## Existing Constraints

- Follow .NET 8 and repository architecture rules in `CLAUDE.md`.
- `PILOT-LOG-01` already proved the manual Draft creation semantics. Do not regress the
  distinction between:
  - manual capture with latest-known snapshot data;
  - automatic missed-moment Draft creation with period-only data.
- The existing `Remarks` field already persists free text and is the required storage
  for the short notitie in this story. Do not add a second note column.
- Weather must be stored as a stable domain value, not derived from the displayed icon
  or label. An enum or equivalently stable coded value is required.
- The user-facing icon set may live in the UI layer, but the persisted weather value
  must remain presentation-independent.
- Existing logbook rows and migrations must remain backward compatible; pre-existing
  entries must survive the upgrade with null event/weather values.
- Keep the current `Owner,Crew` authorization and the existing `_foutmelding` style on
  `Logbook.razor`.

## Acceptance Focus

- One handmatig moment from an open trip can immediately capture gebeurtenis,
  weerconditie and notitie without leaving the active trip flow.
- The approved gebeurtenis list is available and saves the selected stable value.
- The approved weather choices are available as large understandable icons and save the
  selected stable value.
- The short note saves through `Remarks`.
- Existing `PILOT-LOG-01` Draft creation, NMEA snapshot persistence and confirmation
  flow stay intact.
- Saved event/weather/note remain visible later in the logbook UI and detail view.
- Existing rows without the new values remain readable after migration and render
  without errors.

## Test Evidence Requirements

- Name the production behavior each new or changed test executes. This is a new slice;
  formal red-green bugfix proof is not required unless a pre-existing defect is fixed.
- Require real product-code or component execution and concrete assertions on persisted
  state, mapped DTO values, rendered text/icon selection state and save calls.
- Forbid placeholder or documentary tests, including `Assert.True(true)`, empty test
  methods, source-shape assertions used instead of behavior, and `async` tests without
  relevant awaited behavior.
- For UI tests, render the real Blazor component through bUnit and drive the actual
  `Moment vastleggen` -> save flow. Do not invoke private handlers through reflection.
- For the migration test, migrate explicitly from
  `20260621074251_AddStockExpectedLocations`, prove the migration list before and after,
  insert an existing `LogbookEntry` before upgrade, migrate to latest and assert that:
  - the row is preserved;
  - the new event/weather columns exist;
  - both new values are null/default-safe for old rows.

Required new or changed test coverage must prove at least:

- `LogbookService.CreateEntryAsync` and `UpdateEntryAsync` persist the selected
  gebeurtenis and weather-domain values while still using `Remarks` for the note.
- Manual Draft capture followed by the task-focused save flow preserves the original
  Draft and snapshot fields and only enriches the entry with event/weather/note.
- `LogbookEntryDetailService` maps the saved event/weather/note back to the detail DTO.
- `Logbook.razor` shows the direct post-capture input flow, saves the chosen values and
  renders the saved event/weather context afterward.
- Existing entries with null event/weather still render safely in both the list and
  detail page.

Inspect every new or changed test and confirm that it can fail for the behavior it
claims to cover.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~LogbookServiceTests|FullyQualifiedName~LogbookComponentTests"
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~Logbook"
```

Then:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

Before accepting the command results, inspect every new or changed test and confirm
that it can fail for the behavior it claims to cover. A green test suite is not evidence
when its tests only document intended behavior.

## Definition of Technical Completion

Report `ready for Codex review` only when:

- every scope item and acceptance criterion is technically implemented;
- event and weather values are stored as stable domain values and not as rendered UI
  labels/icons;
- the direct post-capture edit flow works for the newly created Draft;
- the short note is persisted through `Remarks` without introducing duplicate fields;
- all targeted tests pass;
- every new or changed test executes real product behavior and contains meaningful
  assertions;
- the migration upgrade path from `20260621074251_AddStockExpectedLocations` is proven;
- the full required test run contains no new failure;
- build and `git diff --check` pass;
- no unexplained change exists outside the expected write-set;
- remaining manual acceptance steps are listed explicitly.

Report `not ready` when any scope item is incomplete, a non-stable weather storage model
is used, the direct post-capture flow is missing, `Remarks` is replaced instead of
reused, migration compatibility is unproven, a test is documentary or cannot detect the
claimed behavior, a new or changed test fails, build/diffcheck fails, a required
decision is missing, or an additional write area cannot be justified. Do not downgrade
failures to warnings or weaken tests or acceptance criteria to claim completion.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names, the production behavior they execute and migration
   proof;
4. migration/configuration impact;
5. remaining risks and manual test requirements;
6. final status: `ready for Codex review` or `not ready`, with the concrete reason.

Additionally, write the same completion content to
`.docs/processtatus/codex-pilot-log-02/ClaudeStatus.md` and end that file with
`Done: yyyy-MM-dd HH:mm`.
