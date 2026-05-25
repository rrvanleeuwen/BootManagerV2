# Agent Instructions

This repository uses Copilot for most implementation work and Codex for project
context, planning, prompt creation, review, documentation, and git-flow support.

Before doing work in this repository, read these files:

- `.codex/working-agreement.md`
- `docs/bootmanager_codex_handoff.md`
- `.docs/TODO.md`
- `.docs/epics/first-run-onboarding.md`
- `.docs/epics/digital-logbook.md`

The `.codex/working-agreement.md` file is authoritative for the Codex/Copilot
workflow. In particular:

- Codex must not change application code unless the user explicitly asks Codex
  to implement or fix something directly.
- Codex should normally create scoped Copilot prompts, review Copilot output,
  formulate acceptance tests, and guide the branch/PR flow.
- If Codex finds a bug during review, Codex should first provide review advice
  or a Copilot prompt instead of directly editing code.
- For UI changes, onboarding/auth flow, deployment/configuration, database
  behavior, or other runtime-sensitive changes, Codex must give the user a
  short manual test step before commit/push/PR and wait for the user's feedback.
- When a PR is merged, Codex should verify the PR merge, switch local checkout
  back to `master`, fast-forward pull from `origin/master`, verify a clean
  worktree, and then propose the next logical story.

Use the `.docs` directory as the primary source of current project status and
roadmap decisions. If documentation and code appear to disagree, establish the
actual code/build/test state first and then propose a documentation correction.
