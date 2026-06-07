# Current Codex Handoff

Updated: 2026-06-07.

## Rollen

- Claude Code is de programmeur en maakt uitsluitend code vanuit een goedgekeurd implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- De actuele productprioriteit is rechtstreeks op `master` vastgelegd.
- Er is geen actieve functionele featurebranch vanuit deze handoff.

## Actuele productdoelstelling

De leidende productrelease is de **BootManager Holiday Pilot 2026** voor drie weken praktisch gebruik op Linde door Roelof en Carla.

Bron:

- `.docs/releases/holiday-pilot-2026.md`

Tot deze pilot gereed is kiest Codex geen story buiten deze release, tenzij:

- een blocker eerst opgelost moet worden;
- een noodzakelijke afhankelijkheid ontbreekt;
- de gebruiker expliciet een andere prioriteit vaststelt.

## Eerstvolgende story

`PILOT-SCAN-01: Camera-, QR- en barcode-proof-of-concept`

Doel:

- vroeg bewijzen dat QR- en productbarcodescannen in de lokaal gehoste Blazor-app werkt;
- testen op de telefoons van Roelof en Carla;
- noodzakelijke HTTPS-, browser- en netwerkvoorwaarden vastleggen;
- nog geen product-, locatie- of databasefunctionaliteit implementeren.

De volledige story, scope, buiten-scope en acceptatietest staan in `.docs/releases/holiday-pilot-2026.md`.

## Documentatieregel

Bij iedere pilotstory controleert en actualiseert Codex ook:

- relevante bestaande actuele userstories/epics;
- `.docs/TODO.md`;
- geraakte legacy-userstories en `legacy-coverage-register.md`;
- deze handoff;
- README-status wanneer cijfers wijzigen.

Als dezelfde functionaliteit al in een bestaande of legacy-story staat, wordt die status bijgewerkt en wordt geen los tegenstrijdig verhaal achtergelaten.

## Laatst afgeronde productwijziging

`DSH-BUG-DBCTX-1: Gelijktijdige dashboard- en logboekqueries isoleren` is gemerged via PR #85 en op de Raspberry Pi gevalideerd.

## Relevante actuele documenten

- `.docs/releases/holiday-pilot-2026.md` — leidende release-scope en prioriteitsvolgorde;
- `.codex/working-agreement.md` — proces en administratieve afronding;
- `.codex/task-context-map.md` — contextkeuze per taaktype;
- `.docs/epics/digital-logbook.md` — bestaande logboekstatus;
- `.docs/legacy-analysis/legacy-coverage-register.md` — legacy-dekking;
- `.docs/TODO.md` — algemene backlog, ondergeschikt aan de actieve pilot.

## Handoffregel

Houd dit bestand kort. Bewaar alleen actuele branch, release-doel, blokkades, laatste relevante productwijziging en eerstvolgende actie. Historische details blijven in git, PR's, epics en release-documentatie.
