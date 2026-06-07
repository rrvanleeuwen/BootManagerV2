# Implementation Packet

Use this after the user story is approved and saved in its epic. Remove
irrelevant sections and keep the completed packet concise.

## Task

- Story ID:
- Approved story:
- Story source:
- Goal:

The story is already approved. Do not restate it or ask for approval. Give a
short plan, implement directly, run the checks, and provide completion notes.

## Scope

-

## Outside Scope

-

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

-

Before changing an additional area, explain why it is required.

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

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. migration/configuration impact;
4. remaining risks and manual test requirements.
