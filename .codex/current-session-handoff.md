# Current Codex Handoff

Updated: 2026-07-17.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `codex/pilot-perf-01-product-overview`.
- `PILOT-PERF-01` is op 2026-07-17 technisch gecontroleerd en door de gebruiker
  geaccepteerd. Het productoverzicht gebruikt databasegestuurde paginering en een vast
  querybudget.

## Actieve release

De leidende release is de **BootManager Holiday Pilot 2026**.

- bron: `.docs/releases/holiday-pilot-2026.md`;
- status: actief en leidend voor de eerstvolgende ontwikkelperiode;
- afgerond: `PILOT-SCAN-01`, `PILOT-AUTH-01`, `PILOT-LOC-01`, `PILOT-LOC-02`,
  `PILOT-LOC-03`, `PILOT-LOC-04`, `PILOT-INV-01`, `PILOT-INV-02`, `PILOT-INV-03`,
  `PILOT-INV-04`, `PILOT-INV-05`, `PILOT-SCAN-02`, `PILOT-SCAN-03`,
  `PILOT-SCAN-03A`, `PILOT-SCAN-04`, `PILOT-SCAN-05`, `PILOT-UX-01`,
  `PILOT-INV-07`, `PILOT-INV-08`, `PILOT-LOG-01`, `PILOT-LOG-02`, `PILOT-INV-06`,
  `PILOT-PERF-01`;
- eerstvolgende stap: `PILOT-PERF-02` voor gebatchte home- en gedeelde productzoeking;
- daarna: `PILOT-E2E-01` voor de end-to-end gebruikstest.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

De eerstvolgende sessie blijft gericht op snelle vakantie-ingebruikname en lost eerst
de aangetoonde inventoryperformanceproblemen op:

- `PILOT-PERF-02` voor home- en gedeelde productzoeking zonder per-product
  voorraadqueries;
- daarna `PILOT-E2E-01`, gevolgd door de bredere performancevervolgstories en
  `PILOT-OPS-01`.

Historische scanrework-details, acceptatie-uitkomsten en de expliciete
herprioriteringen rond scan/home/productoverzicht staan in
`.codex/current-session-deferred-context.md`.

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
