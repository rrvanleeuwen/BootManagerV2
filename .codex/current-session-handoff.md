# Current Codex Handoff

Updated: 2026-06-19.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `feature/pilot-loc-04-token-replacement-tag-overview`.
- `master` en `origin/master` staan op mergecommit `9e77812` voor `PILOT-LOC-03`.
- De worktree bevat de gecontroleerde en handmatig geaccepteerde wijzigingen voor
  `PILOT-LOC-04`, inclusief de kleine navigatievervolgstap voor het Owner-only
  menu `Opslag`; commit/push/PR staan als eerstvolgende administratieve stap open.

## Actieve release

De leidende release is de **BootManager Holiday Pilot 2026** voor drie weken praktisch
gebruik op Linde door Roelof en Carla:

- bron: `.docs/releases/holiday-pilot-2026.md`;
- `PILOT-SCAN-01` is Done en op de Raspberry Pi en beide Samsung-telefoons geaccepteerd;
- `PILOT-AUTH-01` is op 2026-06-17 technisch gecontroleerd en handmatig
  geaccepteerd;
- `PILOT-LOC-01` is op 2026-06-18 technisch gecontroleerd en handmatig geaccepteerd;
  persistent gebieds- en locatiebeheer, Owner/Crew-detailtoegang en migratiebewijs zijn
  gereed;
- `PILOT-LOC-02` is op 2026-06-19 technisch gecontroleerd en handmatig geaccepteerd;
  stabiele locatie-QR-tokens, scanrouting en Owner-only koppelen van onbekende
  BootManager-QR's zijn gereed;
- `PILOT-LOC-03` is op 2026-06-19 technisch gecontroleerd en handmatig geaccepteerd;
  Owner-only QR-tagpagina's, compacte 5x5 cm printweergave, QRCoder-rendering via een
  vervangbare abstraction en scanbare PNG-download met locatienaam zijn gereed;
- `PILOT-LOC-04` is op 2026-06-19 technisch gecontroleerd en handmatig geaccepteerd;
  Owner-only tokenvervanging, tagstatusbeheer, tagoverzicht en directe navigatie via
  hoofdmenu `Opslag` met `Locaties` en `Tagoverzicht` zijn gereed;
- tijdens acceptatie zijn twee smalle fixes door Codex toegevoegd: `/_framework`
  toestaan in de Crew-PCR-gate en open Blazor-sessies periodiek valideren tegen
  `CredentialVersion`.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

Controleer status en diff, werk de pilotdocumentatie af, commit en push
`feature/pilot-loc-04-token-replacement-tag-overview`, maak de PR aan en pak daarna
`PILOT-INV-01` op vanaf een schone actuele `master`.

Laatste verificatie op 2026-06-19:

- handmatige acceptatie van `PILOT-LOC-04` geslaagd: tagoverzicht en tokenvervanging
  werken, oude tokens worden ongeldig, nieuwe tokens openen de locatie, en de
  opslagnavigatie loopt nu uitsluitend via het Owner-only hoofdmenu `Opslag`;
- gerichte storage/navigation unit-tests: 20/20 voor de laatste navigatiecheck en
  eerder 138/138 voor de bredere storage/tag suite;
- gerichte storage-integratietests voor tokenvervanging/migratie: geslaagd;
- `dotnet build BootManager.sln --no-restore`: geslaagd; bestaande repositorybrede
  baseline warnings buiten deze story blijven aanwezig;
- `git diff --check`: geslaagd.

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
test-, PR- en acceptatiedetails horen in git en de release- of epicdocumentatie.
