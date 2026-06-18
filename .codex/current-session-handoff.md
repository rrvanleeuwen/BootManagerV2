# Current Codex Handoff

Updated: 2026-06-18.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `master`.
- `master` is bijgewerkt tot na de merge van `PILOT-AUTH-01`.
- De worktree was schoon na de mergecontrole.

## Actieve release

De leidende release is de **BootManager Holiday Pilot 2026** voor drie weken praktisch
gebruik op Linde door Roelof en Carla:

- bron: `.docs/releases/holiday-pilot-2026.md`;
- `PILOT-SCAN-01` is Done en op de Raspberry Pi en beide Samsung-telefoons geaccepteerd;
- `PILOT-AUTH-01` is op 2026-06-17 technisch gecontroleerd en handmatig
  geaccepteerd;
- `PILOT-LOC-01` is op 2026-06-18 documentair uitgewerkt in het holidaybestand,
  inclusief scope, acceptatiecriteria, handmatige acceptatietest en legacy-koppeling;
- tijdens acceptatie zijn twee smalle fixes door Codex toegevoegd: `/_framework`
  toestaan in de Crew-PCR-gate en open Blazor-sessies periodiek valideren tegen
  `CredentialVersion`.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

Start de volgende sessie op `master` en controleer eerst `git status --short --branch`.
De eerstvolgende uitvoeringsactie is een featurebranch en implementation packet voor
`PILOT-LOC-01` — opslaggebieden en opslaglocaties.

Aanpak voor de volgende sessie:

1. laad alleen deze handoff, `.codex/task-context-map.md` en
   `.docs/releases/holiday-pilot-2026.md`;
2. lees gericht de uitgewerkte `PILOT-LOC-01` story in het releasedocument;
3. controleer branch en worktree;
4. maak de featurebranch voor `PILOT-LOC-01`;
5. stel daarna het compacte implementation packet op voor Claude Code.

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
