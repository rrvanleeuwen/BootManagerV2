# Agent Instructions

This repository separates the implementation-agent role from the Codex role.
Claude Code is currently the primary implementation agent. Codex handles
project context, planning, implementation packets, review, documentation, and
git-flow support.

Use task-driven context loading. Do not preload the full documentation set.

At the start of a Codex session, read only:

- `.codex/working-agreement.md`
- `.codex/current-session-handoff.md`
- `.codex/task-context-map.md`

Then load additional documents only when the task actually needs them. The
context map defines which docs are relevant for common task types. Prefer
targeted `Select-String`, `rg`, or small file sections over reading large files
in full.

At the start of a Claude Code implementation session, follow `CLAUDE.md` and
the supplied implementation packet. Do not load the Codex start set unless the
packet explicitly requires a specific file from it.

Only read the old full documentation set when the task explicitly requires a
broad roadmap/scope audit, legacy mapping, or project-wide planning decision.
The old full set is:

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

The `.codex/working-agreement.md` file is authoritative for the
Codex/implementation-agent workflow. In particular:

- Codex must not change application code unless the user explicitly asks Codex
  to implement or fix something directly.
- This restriction also applies to small review fixes, warnings, whitespace,
  build errors, and quick corrections in application code. Codex must not make
  those application-code edits itself unless the user explicitly asks Codex to
  do so.
- If Codex finds that implementation-agent output needs a code fix, Codex
  should provide focused review instructions or explicitly ask whether Codex
  may edit the code.
- Codex should normally create scoped implementation packets, review
  implementation-agent output, formulate acceptance tests, and guide the
  branch/PR flow.
- If Codex finds a bug during review, Codex should first provide review advice
  or implementation-agent instructions instead of directly editing code.
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

Before commit/push/PR, Codex must also proactively check which related project
documents now need status updates because of the completed story. This includes
the relevant `.docs/epics/*.md` file, `.docs/TODO.md`, handoff documents and
other directly affected status-tracking docs. Codex should update every
reasonably afvinkbare/admin-complete status on its own; the user should not
need to ask for this cleanup explicitly.

For implementation work, Codex must not jump directly from branch creation to
an implementation packet. After creating or selecting the feature branch,
Codex must first formulate a concise user story together with scope,
out-of-scope items,
acceptance criteria, legacy coverage impact, and required manual test notes.
Codex must ask the user whether that user story is correct. After user approval,
Codex must automatically save the approved user story in the relevant
`.docs/epics/*.md` file before generating the implementation packet. The saved
story must include the story sentence, scope, out-of-scope items, acceptance
criteria, legacy coverage impact, and required manual test notes. If no
suitable epic file exists yet, Codex should create a small appropriate epic
document or first propose the documentation location when the choice is
ambiguous. Only after the approved user story is stored in the epic file may
Codex generate the implementation packet.

Implementation packets should follow
`.codex/implementation-packet-template.md`. They must keep implementation-agent
context minimal by naming the exact story section, expected write-set,
necessary source files, targeted checks, and completion format. Claude Code
also follows the compact root `CLAUDE.md`.

For the legacy BootManager Word-source inventory, continue strictly one source
file at a time. After each processed file, stop and ask the user for approval
before processing the next file. Do not commit or push between files unless the
user explicitly asks for it.
