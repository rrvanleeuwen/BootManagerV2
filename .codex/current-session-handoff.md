# Current Codex Handoff

Updated: 2026-06-19.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `feature/pilot-loc-03-qr-tag-print-png`.
- `master` is bijgewerkt tot en met mergecommit `fd18442` voor `PILOT-LOC-02`.
- De worktree bevat de gecontroleerde en handmatig geaccepteerde wijzigingen voor
  `PILOT-LOC-03`; commit/push/PR staan als eerstvolgende administratieve stap open.

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
- `PILOT-LOC-04` blijft documentair uitgesplitst voor QR-tokenvervanging/tagoverzicht;
- tijdens acceptatie zijn twee smalle fixes door Codex toegevoegd: `/_framework`
  toestaan in de Crew-PCR-gate en open Blazor-sessies periodiek valideren tegen
  `CredentialVersion`.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

Controleer status en diff, werk de pilotdocumentatie af, commit en push
`feature/pilot-loc-03-qr-tag-print-png`, maak de PR aan en pak daarna
`PILOT-LOC-04` op vanaf een schone actuele `master`.

Laatste verificatie op 2026-06-19:

- handmatige acceptatie van `PILOT-LOC-03` geslaagd: browserprint werkt, compacte
  5x5 cm tagweergave is bruikbaar, PNG-download levert een bestand met locatienaam op
  en zowel zichtbare QR als gedownloade PNG openen dezelfde locatie;
- gerichte storage/tag unit-tests: 36/36;
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
