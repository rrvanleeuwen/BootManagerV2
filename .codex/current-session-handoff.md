# Current Codex Handoff

Updated: 2026-06-25.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `codex/pilot-ux-01-home-hub`.
- `master` bevat nu ook de follow-upfix op `PILOT-SCAN-05` voor de locatie-QR-scan
  in `ScanProductAddStock` na productaanmaak vanuit een onbekende code:
  - callbackcontract van de gedeelde scanner sluit nu aan op de component;
  - cameraresultaten resolven BootManager locatie-QR's nu via
    `ResolveQrValueAsync` in plaats van raw vergelijking met `LocationId`;
  - onbekende of niet-herkende cameraresultaten tonen nu ook zichtbaar een scanfout;
  - regressietests dekken camera-succes- en foutpaden zonder placeholder-asserties;
  - handmatige Raspberry Pi-/mobielvalidatie voor
    `onbekende code -> nieuw product -> locatie-QR scannen` is akkoord.
- Volgende gitstap na deze handoff: `PILOT-UX-01` documenteren/afronden en daarna
  verder met `PILOT-INV-06`.

## Actieve release

De leidende release is de **BootManager Holiday Pilot 2026**.

- bron: `.docs/releases/holiday-pilot-2026.md`;
- status: actief en leidend voor de eerstvolgende ontwikkelperiode;
- afgerond: `PILOT-SCAN-01`, `PILOT-AUTH-01`, `PILOT-LOC-01`, `PILOT-LOC-02`,
  `PILOT-LOC-03`, `PILOT-LOC-04`, `PILOT-INV-01`, `PILOT-INV-02`, `PILOT-INV-03`,
  `PILOT-INV-04`, `PILOT-INV-05`, `PILOT-SCAN-02`, `PILOT-SCAN-03`,
  `PILOT-SCAN-03A`, `PILOT-SCAN-04`, `PILOT-SCAN-05`, `PILOT-UX-01`;
- eerstvolgende focus: `PILOT-INV-06` voor het responsieve productoverzicht; daarna
  terug naar `PILOT-LOG-01`.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

Na afronding van de scanfixes en handmatige acceptatie van de nieuwe home is de
volgende sessie expliciet gericht op het resterende pilotgebruiksgemak:

- eerst `PILOT-INV-06` voor een redesign van `Voorraadbeheer > Producten` met
  dezelfde zoek- en resultaatpresentatie als home;
- pas daarna terug naar `PILOT-LOG-01`.

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
- `PILOT-SCAN-05` is technisch gecontroleerd en handmatig geaccepteerd:
  - onbekende codes gaan vanuit `/scan` naar een nieuw onbekende-code-scherm;
  - de gebruiker kan binnen nieuwe scanroutes kiezen voor nieuw product, koppelen of
    annuleren;
  - de nieuwe productaanmaakroute vereist expliciete keuze van standaardeenheid;
  - er is geen zichtbare terugval naar oude scanflow-pagina's of generieke
    beheerpagina's als eindervaring.
- follow-up 2026-06-25 op aparte branch, inmiddels gemerged en handmatig bevestigd:
  - locatie-QR's in `ScanProductAddStock` na nieuw product vanuit onbekende code
    gebruikten niet hetzelfde callback- en resolvepad als `/scan`;
  - technische fix en regressietests zijn lokaal groen;
  - handmatige bevestiging op mobiel/Raspberry Pi is akkoord.
- nieuwe expliciete herprioritering 2026-06-25:
  - de gebruiker wil vóór de logboekvervolgstories extra pilotgebruiksgemak op home
  en in `Voorraadbeheer > Producten`;
  - de map `.docs/analysis/stitch_responsive_bootstrap_process_design/` is leidend
  voor deze slices;
  - Claude moet die mockups bij implementatie als verplichte ontwerprichting volgen en
  mag niet terugvallen naar generieke bootstrap- of CRUD-layouts.
- `PILOT-UX-01` is technisch gecontroleerd en handmatig geaccepteerd:
  - home is nu de standaard startpagina in plaats van directe doorstuur naar
    dashboard;
  - home volgt de mockup-geleide pilot-hub met snelle tegels naar `Logboek`,
    `Dashboard` en `Scannen`;
  - home-productzoekresultaten tonen productnaam, hoeveelheid, eenheid en locaties;
  - een klik op een home-product opent eerst productinformatie en biedt daarna direct
    de verbruiksroute met vooraf geselecteerd product.

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
