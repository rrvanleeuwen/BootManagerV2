# Current Codex Handoff

Updated: 2026-06-19.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `master`.
- `master` en `origin/master` staan op mergecommit `4a610da` voor `PILOT-LOC-04`.
- De worktree is schoon na merge van PR #93.

## Actieve release

De leidende release is de **BootManager Holiday Pilot 2026**.

- bron: `.docs/releases/holiday-pilot-2026.md`;
- status: actief en leidend voor de eerstvolgende ontwikkelperiode;
- afgerond: `PILOT-SCAN-01`, `PILOT-AUTH-01`, `PILOT-LOC-01`, `PILOT-LOC-02`,
  `PILOT-LOC-03`, `PILOT-LOC-04`;
- actuele focus: inventory-vervolg vanaf `PILOT-INV-01`.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

De inventory-stories `PILOT-INV-01` tot en met `PILOT-INV-05` zijn nu functioneel
uitgewerkt in de holiday-release. Pak als eerstvolgende implementatiestap
`PILOT-INV-01` op vanaf deze schone actuele `master`: maak eerst een nieuwe
featurebranch en laad alleen de minimale pilotcontext voor producten,
productcategorieën, eenheden en gekoppelde codes.

Voor Claude-gerichte startsessies zijn losse storybronbestanden vastgelegd onder
`.codex/claude-sources/inventory/`: `PILOT-INV-01.md`, `PILOT-INV-02.md`,
`PILOT-INV-03.md`, `PILOT-INV-04.md` en `PILOT-INV-05.md`.
Gebruik in de eerstvolgende sessie direct `PILOT-INV-01.md` als expliciete storybron;
de gebruiker start daarna met nul-één.

Laatste verificatie op 2026-06-19:

- handmatige acceptatie van `PILOT-LOC-04`: geslaagd;
- gerichte storychecks, build en diffcheck: geslaagd;
- `git diff --check`: geslaagd.
- PR #93 is op 2026-06-19 gemerged; lokale `master` is fast-forward bijgewerkt tot
  mergecommit `4a610da`.

## Niet-standaard context

Lees `.codex/current-session-deferred-context.md` alleen wanneer historische
pilotdetails, scan-/QR-grondslagen, auth-/storage-samenvattingen, baseline-teststatus,
Raspberry Pi-/runtimecontext of post-vakantie vervolgvragen concreet relevant zijn.

## Documentatie

Bij iedere pilotstory blijven minimaal deze documenten onderling consistent:

- `README.md`;
- `.docs/releases/holiday-pilot-2026.md`;
- relevante actuele epic/userstory en `.docs/TODO.md`;
- geraakte legacy-userstories en `.docs/legacy-analysis/legacy-coverage-register.md`;
- deze handoff.

Storystatus, pilotvoortgang en eerstvolgende story moeten overeenkomen.
Documentatiewijzigingen worden na controle automatisch gecommit en naar de actuele
remote branch gepusht, tenzij de gebruiker expliciet anders vraagt of dit onveilig is.

## Handoffregel

Houd dit bestand als actuele momentopname. Bewaar alleen branchstatus, actieve release,
blockers, relevante productstatus en eerstvolgende actie. Historische implementatie-,
test-, PR- en acceptatiedetails horen in git, de release- of epicdocumentatie of
`.codex/current-session-deferred-context.md`.
