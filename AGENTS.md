# Agent Instructions

BootManagerV2 gebruikt twee strikt gescheiden agentrollen:

- **Claude Code** is de programmeur. Claude implementeert uitsluitend code binnen een goedgekeurde story en implementation packet.
- **Codex** is de begeleider voor proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Context laden

Gebruik altijd taakgestuurde context. Lees geen brede documentatiesets of source trees zonder concrete noodzaak.

### Codex start

Lees alleen:

1. `.codex/current-session-handoff.md`;
2. `.codex/task-context-map.md`.

Lees `.codex/working-agreement.md` alleen wanneer procesafspraken, storyflow, review, documentatie of git/PR-flow relevant zijn.

### Claude Code start

Claude leest alleen:

1. `CLAUDE.md`;
2. het implementation packet;
3. de expliciet genoemde storysectie;
4. de expliciet genoemde source files.

Claude leest niet standaard de Codex-handoff, TODO, legacy-analyse, roadmap of ongerelateerde epics.

## Belangrijkste regels

- Codex wijzigt geen applicatiecode tenzij de gebruiker dat expliciet vraagt.
- Claude maakt alleen code en voert geen projectregie, storykeuze, documentatiebeheer, commits, pushes of PR-beheer uit tenzij dit expliciet in het packet staat.
- Gebruik gerichte zoekopdrachten en kleine bestandssecties in plaats van volledige grote bestanden.
- `.docs` is de bron voor actuele stories, roadmap en functionele status.
- Bij verschil tussen documentatie en code wordt eerst de feitelijke code-, build- en teststatus vastgesteld.
- Historische context wordt niet standaard geladen; raadpleeg die alleen wanneer de actuele taak dat vereist.

De volledige Codex-procesafspraken staan in `.codex/working-agreement.md`. De taakgerichte contextkeuze staat in `.codex/task-context-map.md`.
