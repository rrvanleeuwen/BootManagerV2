# Codex Task Context Map

Doel: laad per taak alleen de minimaal noodzakelijke context.

## Start van iedere Codex-sessie

Lees:

- `.codex/current-session-handoff.md`;
- dit bestand.

Lees `.codex/working-agreement.md` alleen als proces, storyflow, review, documentatie of git/PR aan de orde is.

Gebruik `rg`, `Select-String`, gerichte diffs en kleine bestandssecties. Lees grote bestanden niet volledig tenzij dat aantoonbaar nodig is.

## Taaktypen

### Status, overdracht of volgende stap

Lees:

- actuele handoff;
- `git status --short --branch`;
- alleen de relevante TODO- of epicsectie.

### Nieuwe feature of volgende story

Lees gericht:

- relevante TODO-sectie;
- relevante epicsectie;
- `legacy-coverage-register.md` voor geraakte scope;
- `mapped-epics.md` of `proposed-backlog.md` alleen bij onduidelijkheid;
- `scope-inventory.md` alleen als de andere bronnen onvoldoende zijn.

### Implementation packet voor goedgekeurde story

Lees:

- goedgekeurde storysectie;
- relevante architectuur- of beslisnotitie indien nodig;
- alleen source files die nodig zijn om het packet precies te maken;
- `.codex/implementation-packet-template.md`.

Lees geen volledige legacy-analyse wanneer de scope al vaststaat.

### Review van Claude Code-output

Lees/check:

- `git status --short --branch`;
- `git diff --stat`;
- gerichte diff van gewijzigde bestanden;
- goedgekeurde storysectie;
- relevante tests of architectuurafspraken.

### Bug in bestaande functionaliteit

Lees:

- relevante story- of epicsectie;
- betrokken source files;
- relevante logs of foutmelding;
- handoff alleen voor actuele branch- en teststatus.

Legacy-analyse is alleen nodig als de bug functionele scope verandert.

### Commit, push, PR of merge

Controleer:

- branch en worktree;
- gewijzigde bestanden en tests;
- direct geraakte epic/TODO/coverage/handoff;
- README-status alleen bij gewijzigde story- of epiccijfers.

### Raspberry Pi, deployment of runtimeconfiguratie

Lees:

- actuele handoff;
- relevante sectie van `system-operations.md`;
- `docker-compose.yml` en alleen relevante configuratiebestanden;
- runbook alleen wanneer commando's nodig zijn.

### Legacy Word-bron verwerken

Lees uitsluitend:

- `word-source-progress.md`;
- één bronbestand;
- direct relevante analyse-uitvoer.

Verwerk één bronbestand per keer.

## Claude Code-context

Claude Code leest uitsluitend:

- `CLAUDE.md`;
- het implementation packet;
- de exact genoemde storysectie;
- expliciet genoemde source files.

Claude leest niet standaard:

- `.codex/current-session-handoff.md`;
- `.codex/working-agreement.md`;
- volledige TODO of roadmap;
- legacy-analyse;
- ongerelateerde epics;
- brede source trees.

## Niet standaard laden

- `.docs/legacy-input/`;
- `.docs/extraInfo/`;
- `veldtests/`;
- volledige `scope-inventory.md`;
- volledige `.docs/TODO.md`;
- volledige ongerelateerde epics;
- historische handoff- of sessieverslagen.
