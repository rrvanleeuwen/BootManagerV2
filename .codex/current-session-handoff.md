# Current Codex Handoff

Updated: 2026-06-07.

## Rollen

- Claude Code is de programmeur en maakt uitsluitend code vanuit een goedgekeurd implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve featurebranch: `feature/pilot-scan-01`.
- `PILOT-SCAN-01` is geïmplementeerd en lokaal op de laptop geaccepteerd.
- Branch moet na commit/push tijdelijk op de Raspberry Pi worden getest.

## Actuele productdoelstelling

De leidende productrelease is de **BootManager Holiday Pilot 2026** voor drie weken praktisch gebruik op Linde door Roelof en Carla.

Bron:

- `.docs/releases/holiday-pilot-2026.md`

Tot deze pilot gereed is kiest Codex geen story buiten deze release, tenzij:

- een blocker eerst opgelost moet worden;
- een noodzakelijke afhankelijkheid ontbreekt;
- de gebruiker expliciet een andere prioriteit vaststelt.

## Eerstvolgende actie

`PILOT-SCAN-01: Camera-, QR- en barcode-proof-of-concept`

Open voor afronding:

- de huidige ZXing-JavaScriptdecoder voor EAN-13 op Android niet verder repareren; ZXing blijft voorlopig bruikbaar voor QR;
- de geïsoleerde `/scan-quagga-test`-pagina op de Raspberry Pi uitrollen en beide vastgelegde EAN-13-codes elk tienmaal testen;
- bij geslaagde Quagga2-proef de QR- en EAN-13-scanflow in Edge en Chrome op beide telefoons accepteren;
- expliciet valideren dat ingest en de webapp sinds toevoeging van HTTPS samen nog correct werken, zowel via de bestaande HTTP-route als de aanvullende HTTPS-route;
- daarna story administratief afronden en PR maken.

De volledige story, scope, buiten-scope en acceptatietest staan in `.docs/releases/holiday-pilot-2026.md`.

## Documentatieregel

Bij iedere pilotstory controleert en actualiseert Codex ook:

- relevante bestaande actuele userstories/epics;
- `.docs/TODO.md`;
- geraakte legacy-userstories en `legacy-coverage-register.md`;
- deze handoff;
- README-status wanneer cijfers wijzigen.

Als dezelfde functionaliteit al in een bestaande of legacy-story staat, wordt die status bijgewerkt en wordt geen los tegenstrijdig verhaal achtergelaten.

## Laatste relevante productwijziging

`PILOT-SCAN-01` bevat nu naast de bestaande ZXing `/scan`-pagina een geïsoleerde `/scan-quagga-test`-pagina met lokaal meegeleverde Quagga2 1.12.1 voor EAN-13. De pagina gebruikt de op dezelfde Samsung-telefoon bewezen 800px/large/halfSample-uit-configuratie, valideert het EAN-13-controlecijfer en heeft een geserialiseerde, sessiegebonden camera-lifecycle. Build, JavaScript-syntaxcontrole en lokale routecontrole slagen; 147 van 148 unit-tests zijn groen met de bekende ongerelateerde owner-recoverytest rood. Eerstvolgend: branch op de Pi uitrollen, beide EAN-13-codes elk tienmaal testen en daarna de volledige telefoon- en ingest/webapp-regressietest uitvoeren.

## Relevante actuele documenten

- `.docs/releases/holiday-pilot-2026.md` — leidende release-scope en prioriteitsvolgorde;
- `.codex/working-agreement.md` — proces en administratieve afronding;
- `.codex/task-context-map.md` — contextkeuze per taaktype;
- `.docs/epics/digital-logbook.md` — bestaande logboekstatus;
- `.docs/legacy-analysis/legacy-coverage-register.md` — legacy-dekking;
- `.docs/TODO.md` — algemene backlog, ondergeschikt aan de actieve pilot.

## Handoffregel

Houd dit bestand kort. Bewaar alleen actuele branch, release-doel, blokkades, laatste relevante productwijziging en eerstvolgende actie. Historische details blijven in git, PR's, epics en release-documentatie.
