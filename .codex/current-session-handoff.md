# Current Codex Handoff

Updated: 2026-06-21.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `codex/pilot-scan-04-location-scan-context`.
- Branch bevat nu de nieuwe locatie-scanwerkcontext uit `PILOT-SCAN-04`, inclusief
  locatiecontext, muteren op bestaand product en `ander product toevoegen` binnen
  dezelfde vaste locatiecontext.
- Volgende gitstap na deze handoff: committen, pushen en PR openen voor
  `PILOT-SCAN-04`.

## Actieve release

De leidende release is de **BootManager Holiday Pilot 2026**.

- bron: `.docs/releases/holiday-pilot-2026.md`;
- status: actief en leidend voor de eerstvolgende ontwikkelperiode;
- afgerond: `PILOT-SCAN-01`, `PILOT-AUTH-01`, `PILOT-LOC-01`, `PILOT-LOC-02`,
  `PILOT-LOC-03`, `PILOT-LOC-04`, `PILOT-INV-01`, `PILOT-INV-02`, `PILOT-INV-03`,
  `PILOT-INV-04`, `PILOT-INV-05`, `PILOT-SCAN-02`, `PILOT-SCAN-03`,
  `PILOT-SCAN-03A`, `PILOT-SCAN-04`;
- eerstvolgende focus: `PILOT-SCAN-05` voor de onbekende-code-flow en resterende
  scanvervolgstappen, opnieuw zonder zichtbare terugval naar oude scanflow-schermen.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

Na merge van deze branch de volgende sessie starten met `PILOT-SCAN-05`, met als
expliciete hoofdfocus de onbekende-code-flow binnen de nieuwe scanervaring. Die route
mag niet eindigen in legacy, generieke beheerpagina's of doodlopende schermen, maar
moet een helder nieuw beslispad bieden voor:

- bestaand product koppelen of hervatten waar logisch;
- nieuw product aanmaken waar nodig;
- terug naar scanstart wanneer de gebruiker bewust annuleert.

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
- vanaf nu geldt voor alle scanstories expliciet dat flow en UI samen beoordeeld worden;
  een technisch werkende route zonder duidelijke UI-vertaling naar de aangeleverde
  designs is niet acceptabel.
- `PILOT-SCAN-03A` is handmatig geaccepteerd:
  - bekende productscans landen nu in nieuwe scanroutes;
  - muteren en voorraad toevoegen werken binnen nieuwe productflow-schermen;
  - locatie-QR scannen en handmatige locatiecode-invoer werken in de add-stock-flow;
  - er is geen zichtbare terugval naar `/scan/old` binnen het bekende-product-pad.
- `PILOT-SCAN-04` is handmatig geaccepteerd:
  - bekende locatie-QR's landen nu in een nieuwe locatie-werkcontext;
  - muteren op bestaand product blijft binnen nieuwe scanroutes;
  - `ander product toevoegen` houdt de locatiecontext vast;
  - productselectie ondersteunt nu deelzoeking en zichtbare barcode-scan binnen de
    nieuwe scanstijl;
  - er is geen zichtbare terugval naar oude scanflow-pagina's of de generieke
    locatiepagina als scan-eindervaring.
- Voor `PILOT-SCAN-05` geldt dezelfde harde regel:
  - geen gebruik van oude scanflow-pagina's;
  - geen gebruik van oude scanflow-componenten;
  - geen gebruik van generieke beheerpagina's als scan-eindervaring;
  - onbekende codes krijgen een volledig nieuwe zichtbare afhandeling.

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
