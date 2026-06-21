# Current Codex Handoff

Updated: 2026-06-21.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `codex/pilot-inv-05-mutaties-historie`.
- Branch bevat de afgeronde werkset voor `PILOT-INV-05`, inclusief mutatiehistorie,
  scan-gestuurde voorraadbijzonderheden en administratieve fallback.
- Lokale status is gereed voor commit, push en PR vanaf deze branch.

## Actieve release

De leidende release is de **BootManager Holiday Pilot 2026**.

- bron: `.docs/releases/holiday-pilot-2026.md`;
- status: actief en leidend voor de eerstvolgende ontwikkelperiode;
- afgerond: `PILOT-SCAN-01`, `PILOT-AUTH-01`, `PILOT-LOC-01`, `PILOT-LOC-02`,
  `PILOT-LOC-03`, `PILOT-LOC-04`, `PILOT-INV-01`, `PILOT-INV-02`, `PILOT-INV-03`,
  `PILOT-INV-04`, `PILOT-INV-05`;
- eerstvolgende focus na afronding van deze branch: `PILOT-LOG-01`.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

Bij hervatten na merge de volgende story `PILOT-LOG-01` voorbereiden, tenzij de
gebruiker eerst nog een nieuwe branch of aanvullende correctie wil.

Technisch bevestigd in deze sessie:

- `PILOT-INV-05` levert de mutatietypes `Verbruik`, `Correctie` en `Telling` op;
- `Scannen` ondersteunt nu de route `product -> Voorraadbijzonderheid -> locatie ->
  mutatieformulier -> opslaan`, zonder verplichte herhaalscan van hetzelfde product;
- na locatiekeuze gaat de flow ook direct door wanneer de gekozen locatie nog geen
  actieve voorraadregel voor dat product heeft; de huidige hoeveelheid start dan op `0`;
- de administratieve fallback in `Voorraadbeheer > Producten` gebruikt dezelfde
  gebruikersclaim-afhandeling als de echte loginflow;
- aparte pagina's `StockMutations` en `StockMutationHistory` zijn lokaal aanwezig op
  deze branch;
- gerichte regressies zijn groen:
  `dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ScanComponentTests|FullyQualifiedName~ProductsComponentTests"`;
- solution-build is groen:
  `dotnet build BootManager.sln --no-restore`.

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
