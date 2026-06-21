# Current Codex Handoff

Updated: 2026-06-21.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `codex/scanflow-herdefinitie`.
- Branch bevat documentatievoorbereiding voor de herdefinitie van de scanflows,
  inclusief nieuwe analyse- en UI-richtlijnendocumenten.
- Lokale status bevat alleen documentatiewijzigingen voor het nieuwe scan-reworkspoor.

## Actieve release

De leidende release is de **BootManager Holiday Pilot 2026**.

- bron: `.docs/releases/holiday-pilot-2026.md`;
- status: actief en leidend voor de eerstvolgende ontwikkelperiode;
- afgerond: `PILOT-SCAN-01`, `PILOT-AUTH-01`, `PILOT-LOC-01`, `PILOT-LOC-02`,
  `PILOT-LOC-03`, `PILOT-LOC-04`, `PILOT-INV-01`, `PILOT-INV-02`, `PILOT-INV-03`,
  `PILOT-INV-04`, `PILOT-INV-05`, `PILOT-SCAN-02`;
- eerstvolgende focus na afronding van deze documentatiebranch: `PILOT-SCAN-03`.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

Bij hervatten na merge de implementation packet-voorbereiding voor `PILOT-SCAN-03`
starten, gevolgd door `PILOT-SCAN-04` en `PILOT-SCAN-05`.

Technisch bevestigd in deze sessie:

- de gebruiker heeft expliciet gekozen om de scanflows vóór de vakantie opnieuw op te
  bouwen en daarmee tijdelijk voorrang te geven boven `PILOT-LOG-01`;
- de nieuwe basis is vastgelegd in:
  - `.docs/analysis/ScannenFlow/scanflow-herdefinitie.md`;
  - `.docs/analysis/ScannenFlow/scanflow-ui-richtlijnen.md`;
- de afgesproken overgangsstrategie is dat de huidige scanimplementatie tijdelijk
  functioneel blijft bestaan maar technisch en qua routing als `old` geïsoleerd wordt,
  zodat de nieuwe implementatie de definitieve naamgeving kan krijgen;
- `PILOT-SCAN-02` is handmatig geaccepteerd:
  - oude flow staat nu op `/scan/old`;
  - `/scan` blijft de canonieke route en redirect tijdelijk naar `/scan/old`;
  - navigatie blijft naar `/scan` wijzen;
- `PILOT-INV-05` blijft de actuele oude scan/mutatiebasis totdat de nieuwe flow
  handmatig geaccepteerd is.

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
