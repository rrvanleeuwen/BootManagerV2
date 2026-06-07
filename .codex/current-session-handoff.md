# Current Codex Handoff

Updated: 2026-06-07.

## Rollen

- Claude Code is de programmeur en maakt uitsluitend code vanuit een goedgekeurd implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Laatst bekende mastercommit vóór deze documentatiebranch: `842bed306abaa1fb2603cf2d2afb2e8022a55fcc`.
- Actieve documentatiebranch: `codex/token-efficient-agent-context`.
- Deze branch optimaliseert alleen agentinstructies en sessiecontext; geen applicatiecode of functionele scope wordt gewijzigd.

## Actuele taak

Contextgebruik voor Codex en Claude Code verkleinen en verantwoordelijkheden scherper scheiden.

Beoogde uitkomst:

- compacte root-instructies;
- één gezaghebbende Codex-procesafspraak;
- taakgerichte contextselectie;
- korte handoff met uitsluitend actuele status;
- Claude krijgt alleen codecontext uit het implementation packet en voert geen proces-, documentatie- of gitregie uit.

## Laatst afgeronde productwijziging

`DSH-BUG-DBCTX-1: Gelijktijdige dashboard- en logboekqueries isoleren` is gemerged via PR #85.

Bevestigd:

- dashboardmetingen gebruiken een context per laadoperatie;
- polling is sequentieel en annuleerbaar;
- navigatie-evaluaties worden geïsoleerd;
- gerichte tests en build slaagden;
- de gebruiker accepteerde de runtime-test;
- Raspberry Pi is bijgewerkt en de concurrencyfout trad niet opnieuw op.

## Eerstvolgende functionele kandidaat

`DSH-LIVE-3: Logboekactiviteit en snelle doorsteek vanaf dashboard` is inhoudelijk akkoord, maar bewust uitgesteld. Kies deze alleen als de gebruiker geen andere prioriteit aangeeft.

## Relevante actuele documenten

- `.codex/working-agreement.md` — Codex-proces en verantwoordelijkheden;
- `.codex/task-context-map.md` — contextkeuze per taaktype;
- `CLAUDE.md` — minimale instructies voor de programmeur;
- `.docs/epics/dashboard-overview.md` — dashboardstories;
- `.docs/epics/digital-logbook.md` — logboekstatus;
- `.docs/TODO.md` — actuele backlog, alleen gericht lezen.

## Handoffregel

Houd dit bestand bij volgende sessies kort. Bewaar alleen actuele branch, taak, blokkades, laatste relevante productwijziging en eerstvolgende actie. Historische details blijven vindbaar in git, PR's en epicdocumenten en worden hier niet herhaald.
