# Current Codex Handoff

Updated: 2026-06-17.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `feature/pilot-auth-01-local-users`.
- De featurebranch volgt `origin/feature/pilot-auth-01-local-users`.
- De worktree bevat de afgeronde maar nog niet gecommitte applicatiecode en
  documentatie voor `PILOT-AUTH-01`; deze wijzigingen niet resetten, overschrijven
  of naar `master` verplaatsen.

## Actieve release

De leidende release is de **BootManager Holiday Pilot 2026** voor drie weken praktisch
gebruik op Linde door Roelof en Carla:

- bron: `.docs/releases/holiday-pilot-2026.md`;
- `PILOT-SCAN-01` is Done en op de Raspberry Pi en beide Samsung-telefoons geaccepteerd;
- `PILOT-AUTH-01` is op 2026-06-17 technisch gecontroleerd en handmatig
  geaccepteerd;
- tijdens acceptatie zijn twee smalle fixes door Codex toegevoegd: `/_framework`
  toestaan in de Crew-PCR-gate en open Blazor-sessies periodiek valideren tegen
  `CredentialVersion`.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

1. controleer de finale diff van `PILOT-AUTH-01`;
2. commit en push de featurebranch;
3. open of actualiseer de PR;
4. na merge is `PILOT-LOC-01` de eerstvolgende pilotstory.

Laatste verificatie op 2026-06-17:

- handmatige acceptatie met Owner en Carla geslaagd;
- unit-tests: 210/211, alleen de bekende
  `OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect` baseline rood;
- integratietests: 12/12;
- `dotnet build BootManager.sln --no-restore`: geslaagd met 0 warnings en 0 errors;
- `git diff --check`: geslaagd met alleen CRLF-waarschuwingen.

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
