# Codex Task Context Map

Purpose: keep Codex sessions token-efficient by loading only the project context
needed for the current task.

Always start with:

- `.codex/working-agreement.md`
- `.codex/current-session-handoff.md`
- `.codex/task-context-map.md`

Prefer targeted searches (`rg`, `Select-String`) and small file sections over
reading large documents in full.

## Common Task Types

### Quick Status Or Handoff

Read:

- `.codex/current-session-handoff.md`
- `git status --short --branch`

Use when the user asks where we stand, what changed last, or what to do next.

### Implementation Packet For An Existing Approved Story

Read:

- `.codex/working-agreement.md`
- `.codex/current-session-handoff.md`
- the specific `.docs/epics/*.md` section for that story
- relevant source files only if needed to make the prompt precise
- `.codex/implementation-packet-template.md`

Do not reread all legacy analysis files unless the story scope is changing.

### Claude Code Implementation

Claude Code should read:

- root `CLAUDE.md`;
- the supplied implementation packet;
- the exact approved story section named in the packet;
- only the source files named in the packet.

Claude Code should not read `.codex/current-session-handoff.md`, the full TODO,
legacy analysis, unrelated epics, or broad source trees by default. Use targeted
`rg` and small file sections only when a concrete implementation dependency is
missing.

### New Feature Idea Or Next Story Choice

Read:

- `.docs/TODO.md` targeted around the relevant area
- the relevant `.docs/epics/*.md`
- `.docs/legacy-analysis/legacy-coverage-register.md`
- `.docs/legacy-analysis/mapped-epics.md` if coverage/status is unclear
- `.docs/legacy-analysis/proposed-backlog.md` if slicing is unclear

Only read `scope-inventory.md` when the idea cannot be mapped from the coverage
register or mapped epics.

### Bug Report In Existing Feature

Read:

- the relevant `.docs/epics/*.md` story section
- the affected source file(s)
- `.codex/current-session-handoff.md`

Read legacy analysis only if the bug changes functional scope or acceptance
criteria.

### Review Implementation-Agent Output

Read:

- `git status --short --branch`
- `git diff --stat`
- targeted `git diff -- <changed-files>`
- the relevant story section if needed

Focus on correctness, architecture, tests, and documented acceptance criteria.

### Commit, Push, PR, Or Merge Follow-Up

Read/check:

- `git status --short --branch`
- relevant changed docs: story epic, `.docs/TODO.md`, `.codex/current-session-handoff.md`
- `legacy-coverage-register.md` only for completed functional coverage changes

When a PR is merged, verify the PR, checkout `master`, pull `--ff-only`, and
check a clean worktree.

### Raspberry Pi / Deployment / Runtime Config

Read:

- `.codex/current-session-handoff.md`
- `.docs/epics/system-operations.md` relevant section
- `docker-compose.yml`
- relevant `.docs/*deployment*.md` or runbook only when operator commands are
  needed

### Legacy Word Source Processing

Read:

- `.docs/legacy-analysis/word-source-progress.md`
- the single legacy input/source file being processed
- relevant legacy analysis output file(s)

Process strictly one source file at a time.

## Avoid Loading By Default

Do not read these unless the task specifically needs them:

- `.docs/legacy-input/`
- `.docs/extraInfo/`
- `veldtests/`
- full `.docs/legacy-analysis/scope-inventory.md`
- full `.docs/TODO.md`
- full epic files unrelated to the current task
