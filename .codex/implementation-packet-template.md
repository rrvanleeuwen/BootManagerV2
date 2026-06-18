# Implementation Packet

Use this after the user story is approved and saved in its epic. Remove
irrelevant sections and keep the completed packet concise.

## Task

- Story ID:
- Approved story:
- Story source:
- Goal:
- Required branch:

The story is already approved. Do not restate it or ask for approval. Give a
short plan, implement directly, run the checks, and provide completion notes.

Codex must create and verify the required feature branch before giving this packet to
Claude. Claude must stop and report `not ready` when the active branch is `master` or
does not match the required branch.

## Scope

-

## Outside Scope

-

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

-

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

-

Do not load by default:

- full `.docs/TODO.md`;
- unrelated epic documents;
- `.docs/legacy-analysis/`;
- `.docs/legacy-input/`;
- `.codex/current-session-handoff.md`;
- repository-wide source trees.

## Existing Constraints

-

## Acceptance Focus

-

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
- For migration tests, require an explicit migration to the named previous migration,
  assertions on applied migrations before and after upgrade, insertion of existing
  data before upgrade and proof that data remains unchanged afterward. Migrating an
  empty database directly to latest is insufficient.

## Required Checks

Run targeted checks first:

```powershell
# targeted test command
```

Then:

```powershell
dotnet build BootManager.sln
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
