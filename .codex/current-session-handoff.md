# Current Codex Handoff

Updated: 2026-06-09.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `feature/pilot-auth-01-local-users`.
- De featurebranch volgt `origin/feature/pilot-auth-01-local-users`.
- De worktree bevat de nog niet gecommitte applicatiecode van Claude voor
  `PILOT-AUTH-01`; deze wijzigingen niet resetten, overschrijven of naar `master`
  verplaatsen.

## Actieve release

De leidende release is de **BootManager Holiday Pilot 2026** voor drie weken praktisch
gebruik op Linde door Roelof en Carla:

- bron: `.docs/releases/holiday-pilot-2026.md`;
- `PILOT-SCAN-01` is Done en op de Raspberry Pi en beide Samsung-telefoons geaccepteerd;
- `PILOT-AUTH-01` is goedgekeurd en wordt door Claude geïmplementeerd;
- de bekende reviewblockers staan in
  `.codex/PILOT-AUTH-01-review-fix-2-packet.md` en zijn nog niet opnieuw geverifieerd.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

Claude werkt aan de tweede reviewherstelronde vanuit
`.codex/PILOT-AUTH-01-review-fix-2-packet.md`.

Wanneer Claude klaar meldt:

1. controleer dat de branch `feature/pilot-auth-01-local-users` actief is;
2. review de volledige on-gecommitte diff tegen het oorspronkelijke packet en beide
   review-fix-packets;
3. voer `git diff --check`, volledige unit-tests en solution-build sequentieel uit;
4. test de EF-migratie op een tijdelijke kopie van een bestaande SQLite-database;
5. controleer minimaal Crew-wachtwoordgate, Owner/Crew-routeautorisatie,
   sessievernieuwing en beheer van actieve én inactieve Crew;
6. voer pas daarna `dotnet run` uit op de lokale database wanneer migratie en review
   geen blocker tonen.

De vorige onafhankelijke review vond onder meer een verouderde EF-modelsnapshot,
Owner-only dagelijkse routes, ontbrekende Crew-wachtwoordgate, onbruikbare
cookievernieuwing en een onvolledige Crew-beheerlijst. De build slaagde met 13 bestaande
waarschuwingen; unit-tests waren 136/137 met alleen de bekende recoverybaseline rood.
De tijdelijke migratieproef faalde met `PendingModelChangesWarning`. Deze resultaten
moeten na Claude's herstel opnieuw onafhankelijk worden vastgesteld.

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
