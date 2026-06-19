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

Pak `PILOT-INV-01` op vanaf deze schone actuele `master`. Maak daarvoor eerst een
nieuwe featurebranch en laad alleen de minimale pilotcontext voor producten,
productcategorieën en productbarcodes.

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
- PR #93 is op 2026-06-19 gemerged; lokale `master` is fast-forward bijgewerkt tot
  mergecommit `4a610da`.

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
