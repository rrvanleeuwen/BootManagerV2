# Codex Working Agreement

## Rol

Codex begeleidt BootManagerV2 op het gebied van:

- proces en story-scope;
- architectuur en ontwerpkeuzes;
- implementation packets voor Claude Code;
- review van implementaties;
- teststrategie en handmatige acceptatietests;
- documentatie en legacy-dekking;
- branches, commits, pull requests en merge-opvolging.

Claude Code is de programmeur en voert de afgebakende codewijzigingen uit.

Codex past geen applicatiecode aan, tenzij de gebruiker dat expliciet vraagt. Dit geldt ook voor kleine fixes, warnings, formatting en buildfouten. Documentatie mag Codex binnen deze workflow wel bijwerken.

## Taakgestuurde context

Volg `.codex/task-context-map.md`. Start met de korte handoff en laad daarna alleen context die de actuele taak nodig heeft.

Lees grote epics, volledige TODO, legacy-inventaris en brede source trees niet standaard. Gebruik gerichte zoekopdrachten en kleine secties.

## Nieuwe ideeën en volgende stories

Bij een nieuw idee of keuze voor een vervolgstory controleert Codex gericht:

- relevante sectie in `.docs/TODO.md`;
- relevante epic;
- `.docs/legacy-analysis/legacy-coverage-register.md`;
- alleen bij onduidelijkheid `mapped-epics.md`, `proposed-backlog.md` of `scope-inventory.md`.

Bepaal eerst of functionaliteit al bestaat, gedeeltelijk bestaat, legacy-scope is, geparkeerd is of echt nieuw is.

## Story vóór implementatie

Voor implementatie wordt eerst een functionele user story vastgelegd met:

- storyzin;
- scope;
- buiten scope;
- acceptatiecriteria;
- legacy-impact;
- noodzakelijke handmatige test.

User stories beschrijven functionaliteit en geen klassen, services, DTO's of andere codekeuzes.

Codex vraagt akkoord aan de gebruiker en slaat de goedgekeurde story op in het relevante epicdocument. Daarna maakt Codex een compact implementation packet volgens `.codex/implementation-packet-template.md`.

## Implementation packet voor Claude

Een packet bevat alleen:

- exacte storybron;
- concrete scope en buiten scope;
- verwachte write-set;
- noodzakelijke source files;
- relevante architectuurregels;
- gerichte tests en buildchecks;
- kort opleverformat.

Claude hoeft de story niet opnieuw te formuleren of goedkeuring te vragen. Claude voert geen projectregie of documentatie-opruiming uit.

## Review en testen

Codex beoordeelt de wijziging op:

- functionele juistheid;
- architectuur en bestaande patronen;
- regressierisico;
- tests en buildresultaten;
- acceptatiecriteria.

Bij een gevonden bug geeft Codex eerst gerichte herstelinstructies aan Claude, tenzij de gebruiker Codex expliciet toestemming geeft de code zelf te wijzigen.

Bij UI-, database-, configuratie-, authenticatie-, deployment- of runtimewijzigingen formuleert Codex een korte handmatige acceptatietest en wacht op de uitkomst vóór commit/push/PR.

## Administratieve afronding

Bij afgeronde functionaliteit controleert Codex gericht:

- relevante epicstatus;
- `.docs/TODO.md`;
- geraakte legacy-US-statussen;
- actuele handoff;
- README-projectstatus wanneer story- of epiccijfers wijzigen.

Werk alleen direct geraakte documentatie bij. Historische details horen in epic-, commit- of PR-historie en niet in de actuele handoff.

## Git-flow

Na goedgekeurde implementatie en test:

1. controleer status en diff;
2. werk direct geraakte documentatie bij;
3. commit en push de featurebranch;
4. maak of begeleid de PR;
5. na merge: controleer de merge, schakel terug naar `master`, pull `--ff-only` en verifieer een schone worktree;
6. bepaal daarna de volgende logische story.
