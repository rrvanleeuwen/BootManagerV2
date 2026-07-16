# Implementation Packet

## Task

- Story ID: `PILOT-LOG-01`
- Approved story: Handmatig logboekmoment met actuele NMEA-snapshot
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-LOG-01`
- Goal: geef Owner en Crew tijdens een open reis een directe actie `Moment vastleggen`
  die een nieuwe conceptlogboekregel met een momentopname van de reeds beschikbare
  boorddata opslaat, zonder de reis af te sluiten.
- Required branch: `codex/pilot-log-01`

The story is already approved. Do not restate it or ask for approval. Give a short
plan, implement directly, run the checks, and provide completion notes.

Codex has created and verified the required feature branch. Stop and report `not ready`
when the active branch is `master` or does not match the required branch.

## Scope

- Add a clear, directly usable `Moment vastleggen` action in the active/open-trip
  header of the existing logbook page.
- On activation, create one new logbook entry at the current UTC time for the selected
  open trip and keep that trip selected and open in the UI.
- Persist the existing available onboard snapshot fields from the current logbook
  suggestion mechanism: course, wind description, GPS status, latitude, longitude and
  average SOG when available. Missing data must remain null; do not invent values.
- Save this captured moment as `Draft` and display it through the existing draft
  presentation/filtering so it can be found and edited later.
- Reuse the existing last-known-measurement behavior for a manual snapshot
  (`onlyPeriodData: false`). Do not change the stricter `CreateDraftEntryAsync`
  behavior used for automatically created missed moments.
- Retain the existing access restriction (`Owner,Crew`). The current logbook model has
  no actor field, so do not add a user/audit model in this story; capture of the local
  user is therefore not applicable within the existing registration model.

## Outside Scope

- No event selection, weather icons, weather-domain value or note flow from
  `PILOT-LOG-02`.
- No new NMEA sentence types, sensors, ingestion changes or simulator changes.
- No route map, trip planning, report/export or automatic event-detection work.
- No change to automated missed-moment creation semantics.
- No database migration, new persisted fields, DI/configuration changes, documentation
  edits, commits, pushes, branches, PRs, merges, releases or deployments.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Application/Logbook/Services/ILogbookService.cs`;
- `BootManager.Application/Logbook/Services/LogbookService.cs`;
- `BootManager.Web/Components/Pages/Logbook.razor`;
- `BootManager.UnitTests/Logbook/LogbookServiceTests.cs` (new);
- `BootManager.UnitTests/Logbook/LogbookComponentTests.cs` (new);
- `.docs/processtatus/codex-pilot-log-01/ClaudeStatus.md` (required handoff only).

Before changing an additional area, explain why it is required. Do not add an EF
migration: the existing `LogbookEntry` fields persist the required snapshot.

## Execution Boundaries

- Implement only application code and tests explicitly required by this packet.
- Before editing, verify that the active branch matches `codex/pilot-log-01` and is
  not `master`.
- Do not change story, release, TODO, legacy, README, handoff or other project
  documentation.
- Before finishing, create or update
  `.docs/processtatus/codex-pilot-log-01/ClaudeStatus.md`.
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
- `.codex/PILOT-LOG-01-implementation-packet.md`;
- the section `PILOT-LOG-01` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Application/Logbook/Services/ILogbookService.cs`;
- `BootManager.Application/Logbook/Services/LogbookService.cs`;
- `BootManager.Application/Logbook/Services/ILogbookMeasurementSuggestionService.cs`;
- `BootManager.Web/Components/Pages/Logbook.razor`;
- `BootManager.UnitTests/Logbook/LogbookTripTests.cs`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs` only as the local bUnit
  fixture pattern.

Do not load by default:

- full `.docs/TODO.md`;
- unrelated epic documents;
- `.docs/legacy-analysis/`;
- `.docs/legacy-input/`;
- `.codex/current-session-handoff.md`;
- repository-wide source trees.

## Existing Constraints

- Follow .NET 8 and repository architecture rules in `CLAUDE.md`.
- `Logbook.razor` is already authorized for `Owner,Crew` and renders the selected
  travel's action header only for `LogbookTripStatus.Open`.
- `CreateDraftEntryAsync` is for scheduled/missed automatic moments and must keep
  using `onlyPeriodData: true`; adding the manual flow by changing that meaning would
  regress existing behavior.
- `ILogbookMeasurementSuggestionService.GetSuggestionsAsync(..., onlyPeriodData:
  false)` already defines the required manual snapshot semantics: latest available
  measurements at or before the supplied log timestamp, with period SOG where data
  exists.
- The existing `LogbookEntry` stores every required snapshot field and already has
  `Draft`; do not introduce duplicate storage or entity properties.
- The current logbook registration has no executing-user property. This story must not
  introduce a new identity/audit design merely to fill an unavailable field.
- Keep error handling aligned with the page's existing `_foutmelding` pattern, and do
  not close, replace or complete `_selectedTrip` after a successful capture.

## Acceptance Focus

- During an open trip, Owner and Crew see `Moment vastleggen` as a clear action.
- One click creates exactly one new draft entry for the selected trip at capture time.
- The persisted entry contains every currently available onboard value and leaves
  unavailable values empty.
- The manual capture uses latest-known data, not the automatic missed-moment period
  filter.
- The active trip remains open and the new concept is immediately discoverable in the
  existing logbook list.
- A completed trip neither offers the action nor accepts capture through the service.

## Test Evidence Requirements

- Name the production behavior each new test executes. This is a new slice; formal
  red-green bugfix proof is not required unless a pre-existing defect is fixed.
- Require real product-code or component execution and concrete assertions on calls,
  arguments, persisted entity state and rendered outcomes.
- Forbid placeholder or documentary tests, including `Assert.True(true)`, empty test
  methods, source-shape assertions used instead of behavior, and `async` tests without
  relevant awaited behavior.
- For UI tests, render `Logbook.razor` through bUnit, configure the required services,
  and click the actual `Moment vastleggen` button. Do not invoke the private handler
  through reflection.

Required new or changed test coverage must prove at least:

- `CreateManualDraftEntryAsync` (or the equally clear public manual-capture API)
  requests suggestions with `onlyPeriodData: false`, persists their available values
  into a `Draft` entry, and returns that draft; assert the trip id, captured timestamp,
  every populated snapshot field and draft status.
- The same service rejects a completed trip before adding an entry and does not request
  suggestions.
- An open-trip component render exposes `Moment vastleggen`; clicking it calls the
  manual-capture API once for the selected trip, renders the returned draft concept,
  and keeps the active-trip controls visible.
- A completed-trip component render does not show `Moment vastleggen`.
- Existing automatic missed-moment creation remains separately executable and still
  calls its existing period-only draft flow; do not weaken or rewrite its behavior.

Inspect every new or changed test and confirm that it can fail for the behavior it
claims to cover.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~LogbookServiceTests|FullyQualifiedName~LogbookComponentTests|FullyQualifiedName~LogbookTripTests"
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
- manual capture and automatic missed-moment behavior have distinct, proven semantics;
- all targeted tests pass;
- every new or changed test executes real product behavior and contains meaningful
  assertions;
- the full required test run contains no new failure;
- build and `git diff --check` pass;
- no unexplained change exists outside the expected write-set;
- remaining manual acceptance steps are listed explicitly.

Report `not ready` when any scope item is incomplete, automatic missed-moment behavior
is changed, a completed trip can capture a moment, a test is documentary or cannot
detect the claimed behavior, a new or changed test fails, build/diffcheck fails, a
required decision is missing, or an additional write area cannot be justified. Do not
downgrade failures to warnings or weaken tests or acceptance criteria to claim
completion.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names and the production behavior they execute;
4. migration/configuration impact;
5. remaining risks and manual test requirements;
6. final status: `ready for Codex review` or `not ready`, with the concrete reason.

Additionally, write the same completion content to
`.docs/processtatus/codex-pilot-log-01/ClaudeStatus.md` and end that file with
`Done: yyyy-MM-dd HH:mm`.
