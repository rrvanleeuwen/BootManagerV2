# Claude Code Instructions

Claude Code is the current implementation agent for this repository. Codex
owns project context, scope, approved stories, review, documentation, manual
test coordination, and git/PR flow.

## Start Small

Read only:

1. `AGENTS.md`
2. the implementation packet supplied with the task
3. the approved story section named by that packet
4. source files explicitly named by the packet

Use targeted `rg` and small file reads when more context is required. Do not
load the full roadmap, legacy analysis, handoff, or unrelated epics unless the
task is blocked and the packet explicitly permits it.

## Execution

- The story is already approved and stored. Do not restate it or request
  approval again.
- Give a short plan, then implement directly.
- Stay within scope and the expected write-set.
- Do not perform unrelated refactors, formatting, dependency upgrades, or
  documentation cleanup.
- Preserve existing user changes and local repository patterns.
- Keep domain and application rules out of Razor components where practical.
- Add focused tests proportional to the behavior changed.
- Run targeted tests first, then the required build/check commands.
- Do not commit, push, open PRs, or update project status unless the packet
  explicitly assigns that work.

## Completion Format

Report only:

- changed files and behavior;
- tests/checks run and their results;
- migrations or configuration impact;
- remaining risks or manual test needs.

If blocked, report the concrete blocker and the smallest missing decision. Do
not broaden repository exploration by default.
