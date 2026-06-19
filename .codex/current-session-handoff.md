# Current Codex Handoff

Updated: 2026-06-19.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `master`.
- `master` is bijgewerkt tot en met mergecommit `fd18442` voor `PILOT-LOC-02`.
- De worktree is schoon na de mergecontrole.

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
- `PILOT-LOC-03` en `PILOT-LOC-04` blijven documentair uitgesplitst voor QR-print/export
  en QR-tokenvervanging/tagoverzicht;
- tijdens acceptatie zijn twee smalle fixes door Codex toegevoegd: `/_framework`
  toestaan in de Crew-PCR-gate en open Blazor-sessies periodiek valideren tegen
  `CredentialVersion`.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

Start vanaf de schone actuele `master`. Maak een featurebranch en compact
implementation packet voor `PILOT-LOC-03` — QR-tag printen en PNG exporteren. Geef
Claude de opdracht pas nadat de branch is gecontroleerd en niet `master` is.

Laatste verificatie op 2026-06-19:

- handmatige acceptatie van `PILOT-LOC-02` geslaagd; een gemelde afwijking bleek een
  controle op een verkeerde dubbel voorkomende locatienaam, niet een productdefect;
- gerichte storage unit-tests: 96/96;
- volledige unit-suite: 292/293, alleen de bekende
  `OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect` baseline rood;
- gerichte storage-integratietests: 24/24;
- volledige integratiesuite: 36/36;
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
