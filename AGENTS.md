# Agent Instructions

This repository uses Copilot for most implementation work and Codex for project
context, planning, prompt creation, review, documentation, and git-flow support.

Before doing work in this repository, read these files:

- `.codex/working-agreement.md`
- `docs/bootmanager_codex_handoff.md`
- `.docs/TODO.md`
- `.docs/epics/first-run-onboarding.md`
- `.docs/epics/digital-logbook.md`
- `.docs/legacy-analysis/word-source-progress.md`
- `.docs/legacy-analysis/scope-inventory.md`
- `.docs/legacy-analysis/mapped-epics.md`
- `.docs/legacy-analysis/legacy-coverage-register.md`
- `.docs/legacy-analysis/proposed-backlog.md`
- `.docs/legacy-analysis/implemented-or-obsolete.md`
- `.codex/current-session-handoff.md`

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

When the user proposes an idea, asks for a next step, or asks Codex to choose a
new story, Codex must proactively check the legacy scope analysis before
answering. Treat the files in `.docs/legacy-analysis/` as the canonical
functional scope inventory from the legacy BootManager material:

- `scope-inventory.md` for the full legacy functional scope;
- `mapped-epics.md` for what is already covered, open, replaced, or parked;
- `legacy-coverage-register.md` for story-level legacy US coverage status;
- `proposed-backlog.md` for BootManagerV2-style story slicing;
- `implemented-or-obsolete.md` for implemented, replaced, obsolete, and parked
  legacy stories.

Codex should not wait for the user to explicitly ask for this check. For new
ideas, first determine whether the idea is already defined in the legacy scope,
already implemented in BootManagerV2, partially implemented, intentionally
parked, or genuinely new. Then frame the next step in the current
BootManagerV2 architecture and roadmap.

When a BootManagerV2 feature is completed, Codex must update
`legacy-coverage-register.md` for any covered legacy US numbers before treating
the story as administratively complete. If coverage is partial, keep the status
as `Partial` and record what remains open.

For the legacy BootManager Word-source inventory, continue strictly one source
file at a time. After each processed file, stop and ask the user for approval
before processing the next file. Do not commit or push between files unless the
user explicitly asks for it.
