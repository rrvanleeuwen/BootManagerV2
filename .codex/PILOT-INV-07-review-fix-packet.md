# Review Fix Packet

## Task

- Story ID: `PILOT-INV-07`
- Context: follow-up review fixes after the first Claude implementation round
- Goal: correct the two accepted review findings only:
  1. the import mapping UI must support selecting an existing location, not only typing a
     free-text location name;
  2. the Claude process-status handoff file must use the required `Done:` line format.
- Required branch: `codex/pilot-inv-07-csv-startimport`

The story and the original implementation are already approved as a basis. Do not
restate the story and do not broaden scope. Give a short plan, implement only these
review fixes, run the checks, and report according to `Completion Notes`.

## Scope

- Extend the Owner-only import mapping step so the user can explicitly select an
  existing location within the chosen area or type a new one.
- Keep “create new location by typing” available.
- Reuse existing storage data for the selectable locations; do not invent a second
  location model.
- Ensure the UI behavior is area-aware: existing-location suggestions/options must match
  the currently chosen area.
- Update the Claude process-status output so
  `.docs/processtatus/<branch-map>/ClaudeStatus.md` ends with exactly:
  `Done: yyyy-MM-dd HH:mm`
- Rewrite the current branch status file for this branch using that required format when
  finishing this review-fix round.

## Outside Scope

- No redesign of the import flow beyond the minimal area/location selection fix.
- No change to category preservation; categories staying intact is now an explicit user
  decision.
- No new import semantics, no transaction refactor, no new migration.
- No changes to unrelated inventory, scan, tag, auth, or documentation behavior.
- No commits, pushes, branches, PRs, merges, releases or deployments.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Web/Components/Pages/Inventory/InventoryImport.razor`;
- any minimal supporting DTO/viewmodel/helper already directly tied to that import page;
- focused tests under `BootManager.UnitTests/Inventory/`;
- `.docs/processtatus/pilot-inv-07-csv-startimport/ClaudeStatus.md`.

Do not modify application services unless the UI fix truly requires a minimal existing
location lookup seam that cannot be obtained from already available storage contracts.
If that happens, explain why before widening the write-set.

## Execution Boundaries

- Before editing, verify that the active branch matches `codex/pilot-inv-07-csv-startimport`
  and is not `master`.
- Do not change story, release, TODO, legacy, README, handoff or other project
  documentation.
- Do not create commits, pushes, branches, PRs, merges, releases or deployments.
- Do not broaden scope beyond the two review findings above.
- Before finishing, create or update
  `.docs/processtatus/pilot-inv-07-csv-startimport/ClaudeStatus.md`.
- Put the full `Completion Notes` content in that file and end it with a separate line:
  `Done: yyyy-MM-dd HH:mm`
- Treat that `Done:` line only as a Codex-review handoff signal, never as acceptance or
  production-readiness.
- Never declare the story `Done`, accepted or production-ready. Only report
  `ready for Codex review` after satisfying the technical completion definition.

## Minimal Context

Read:

- `CLAUDE.md`;
- `.codex/PILOT-INV-07-review-fix-packet.md`;
- `.codex/PILOT-INV-07-implementation-packet.md`;
- `BootManager.Web/Components/Pages/Inventory/InventoryImport.razor`;
- `BootManager.UnitTests/Inventory/InventoryImportComponentTests.cs`;
- `.docs/processtatus/README.md`;
- `.docs/processtatus/pilot-inv-07-csv-startimport/ClaudeStatus.md`.

Do not load by default:

- full `.docs/TODO.md`;
- unrelated epic documents;
- `.docs/legacy-analysis/`;
- `.docs/legacy-input/`;
- `.codex/current-session-handoff.md`;
- repository-wide source trees.

## Existing Constraints

- Keep the import page Owner-only.
- Preserve the existing destructive-confirmation gate.
- Preserve the existing import semantics and result summary.
- Categories remain preserved by explicit user decision; do not add code to wipe them.
- The process-status rule now requires `Done:` with a colon and `yyyy-MM-dd HH:mm`
  formatting. Follow it exactly.

## Acceptance Focus

- For each mapping row, the Owner can either select an existing location in the chosen
  area or enter a new location name.
- Existing-location choices are filtered to the chosen area.
- The import button still stays blocked until all mappings are complete and the
  destructive confirmation is checked.
- The rewritten ClaudeStatus file for this branch ends with the exact required `Done:`
  line format.

## Test Evidence Requirements

- Require real component rendering and user interaction through the repository’s
  component-test framework.
- Forbid placeholder or documentary tests, including `Assert.True(true)`, empty test
  methods, source-shape assertions used instead of behavior, and `async` tests without
  relevant awaited behavior.
- Each new or changed test must prove at least:
  - existing locations become selectable in the mapping step;
  - the selectable existing locations react to the chosen area;
  - free-text new-location entry remains possible;
  - destructive gating still works as before.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~InventoryImportComponentTests"
```

Then:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

If additional touched tests are needed, run them too and report them explicitly.

## Definition of Technical Completion

Report `ready for Codex review` only when:

- both review findings are fixed;
- all targeted tests pass;
- every new or changed test executes real product behavior and contains meaningful
  assertions;
- build and `git diff --check` pass;
- `.docs/processtatus/pilot-inv-07-csv-startimport/ClaudeStatus.md` is updated and ends
  with the exact required `Done:` format;
- no unexplained change exists outside the expected write-set;
- remaining manual acceptance steps are listed explicitly.

Report `not ready` when either review finding remains open, a new or changed test fails,
build/diffcheck fails, the status file format is wrong, or an extra write-area cannot be
justified.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names and the production behavior they execute;
4. migration/configuration impact;
5. remaining risks and manual test requirements;
6. final status: `ready for Codex review` or `not ready`, with the concrete reason.

Additionally, write the same completion content to
`.docs/processtatus/pilot-inv-07-csv-startimport/ClaudeStatus.md` and end that file
with:

`Done: yyyy-MM-dd HH:mm`
