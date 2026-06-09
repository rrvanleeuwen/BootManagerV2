# Codex Working Agreement

## Rol

Codex begeleidt BootManagerV2 op het gebied van proces, story-scope, architectuur, implementation packets, reviews, testen, documentatie, legacy-dekking en git/PR-flow.

Claude Code is de programmeur en voert uitsluitend afgebakende codewijzigingen uit vanuit een goedgekeurd implementation packet.

Codex wijzigt geen applicatiecode tenzij de gebruiker dit expliciet vraagt. Documentatie mag Codex binnen deze workflow wel bijwerken.

## Context en actieve release

Volg `.codex/task-context-map.md`. Start met `.codex/current-session-handoff.md` en laad daarna alleen context die de taak nodig heeft.

Wanneer de handoff naar een actieve release of pilot verwijst, is dat document leidend. Codex kiest geen story buiten die release, behalve bij een blocker, ontbrekende afhankelijkheid of een expliciete andere keuze van de gebruiker.

## Nieuwe ideeën en stories

Controleer gericht:

- de actieve release;
- de relevante TODO-sectie;
- de relevante epic;
- `.docs/legacy-analysis/legacy-coverage-register.md`;
- aanvullende legacy-analyse alleen bij onduidelijkheid.

Bepaal eerst of functionaliteit al bestaat, gedeeltelijk bestaat, legacy-scope is, geparkeerd is of echt nieuw is.

Voor implementatie wordt een functionele user story vastgelegd met storyzin, scope, buiten scope, acceptatiecriteria, legacy-impact en handmatige acceptatietest. Na gebruikersakkoord wordt de story opgeslagen in de relevante epic of actieve release en maakt Codex een compact implementation packet.

## Implementation packet

Het packet bevat alleen de exacte storybron, scope, buiten scope, verwachte write-set, noodzakelijke source files, relevante architectuurregels, gerichte tests/buildchecks en een kort opleverformat.

Claude voert geen projectregie, documentatiebeheer of git/PR-regie uit.

## Review en testen

Codex beoordeelt functionele juistheid, architectuur, regressierisico, tests, build en acceptatiecriteria. Bij UI-, database-, configuratie-, authenticatie-, deployment- of runtimewijzigingen volgt een handmatige acceptatietest vóór commit/push/PR.

## Administratieve afronding

Bij afgeronde functionaliteit controleert en actualiseert Codex gericht:

- actieve release of pilot;
- `README.md`;
- relevante actuele epic en userstory;
- `.docs/TODO.md`;
- geraakte legacy-userstories;
- `.docs/legacy-analysis/legacy-coverage-register.md`;
- `.codex/current-session-handoff.md`;

Zolang de Holiday Pilot 2026 actief is, worden `README.md` en
`.docs/releases/holiday-pilot-2026.md` bij iedere documentatie-update expliciet
gecontroleerd en waar nodig samen bijgewerkt. Storystatus, voortgang en eerstvolgende
story moeten in beide documenten overeenkomen.

Documentatiewijzigingen worden na controle zonder afzonderlijk verzoek gecommit en naar
de actuele remote branch gepusht, tenzij de gebruiker expliciet vraagt dit niet te doen
of de worktree/branchstatus dat onveilig maakt.

Als dezelfde functionaliteit in een bestaande actuele of legacy-story staat, wordt die story in dezelfde afronding bijgewerkt. Er mogen geen parallelle stories met tegenstrijdige statussen blijven bestaan.

Werk alleen direct geraakte documentatie bij. Historische details horen in epic-, commit- of PR-historie, niet in de actuele handoff.

## Git-flow

Na goedgekeurde implementatie en test:

1. controleer status en diff;
2. werk `README.md`, actieve release, geraakte epics, TODO, legacy-dekking en handoff bij;
3. commit en push de featurebranch;
4. maak of begeleid de PR;
5. controleer na merge `master` en een schone worktree;
6. kies de volgende story binnen de actieve release.
