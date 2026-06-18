# Current Codex Handoff

Updated: 2026-06-18.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `feature/pilot-loc-01-storage-locations`.
- `master` is bijgewerkt tot na de merge van `PILOT-AUTH-01`.
- De worktree was schoon na de mergecontrole.

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
- `PILOT-LOC-02`, `PILOT-LOC-03` en `PILOT-LOC-04` zijn op 2026-06-18 documentair
  uitgesplitst voor locatie-QR-token, QR-print/export en QR-tokenvervanging/tagoverzicht;
- tijdens acceptatie zijn twee smalle fixes door Codex toegevoegd: `/_framework`
  toestaan in de Crew-PCR-gate en open Blazor-sessies periodiek valideren tegen
  `CredentialVersion`.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

Commit en push de geaccepteerde `PILOT-LOC-01`-implementatie en documentatie op
`feature/pilot-loc-01-storage-locations` en rond de PR af. Controleer na merge een
schone actuele `master`; maak daarna een featurebranch en implementation packet voor
`PILOT-LOC-02`.

Laatste verificatie op 2026-06-18:

- handmatige acceptatie van opslagbeheer en Owner/Crew-detailtoegang geslaagd;
- storage unit-tests: 40/40;
- volledige unit-suite: 248/249, alleen de bekende
  `OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect` baseline rood;
- integratietests: 22/22;
- `dotnet build BootManager.sln --no-restore`: geslaagd met 0 warnings en 0 errors;
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
