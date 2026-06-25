# Current Codex Deferred Context

Dit bestand bevat context die niet iedere Codex-sessie nodig heeft, maar later wel
snel terugvindbaar moet zijn. Lees dit niet standaard bij sessiestart.

## Wanneer wel lezen

Lees dit bestand alleen bij concrete noodzaak, bijvoorbeeld voor:

- historische pilotbeslissingen of afgeronde storysamenvattingen;
- scan-, QR-, auth- of storage-grondslagen die opnieuw relevant worden;
- bekende baseline-teststatus of laatste verificatiehistorie;
- Raspberry Pi-, runtime-, acceptatie- of post-vakantie vervolgvragen.

## Afgeronde pilotbasis tot en met 2026-06-19

- `PILOT-SCAN-01` is handmatig geaccepteerd op de Raspberry Pi en beide Samsung-telefoons.
- `PILOT-AUTH-01` is technisch gecontroleerd en handmatig geaccepteerd op 2026-06-17.
- `PILOT-LOC-01` tot en met `PILOT-LOC-04` zijn technisch gecontroleerd en handmatig
  geaccepteerd op 2026-06-18 en 2026-06-19.
- De eerstvolgende geplande story blijft `PILOT-INV-01`.

## Samenvatting scan, auth en storage

- Scanbasis: QR- en productbarcodescan is voor de pilot bewezen op de goedgekeurde
  telefoons; de functionele details staan in het release-archief.
- Authbasis: lokaal Owner/Crew-model met aparte login voor Carla is gereed.
- Storagebasis: opslaggebieden, locaties, stabiele locatie-QR's, print/export,
  tokenvervanging en tagoverzicht zijn gereed.
- Navigatiebasis: opslag loopt nu via het Owner-only hoofdmenu `Opslag` met
  `Locaties` en `Tagoverzicht`.

## Smalle aanvullende fixes tijdens acceptatie

- `/_framework` is toegestaan in de Crew-PCR-gate.
- Open Blazor-sessies worden periodiek gevalideerd tegen `CredentialVersion`.

## Scanrework en pilotherprioritering tot en met 2026-06-25

- De gebruiker heeft expliciet gekozen om de scanflows vóór de vakantie opnieuw op te
  bouwen en daarmee tijdelijk voorrang te geven boven `PILOT-LOG-01`.
- De nieuwe scanbasis is vastgelegd in:
  - `.docs/analysis/ScannenFlow/scanflow-herdefinitie.md`;
  - `.docs/analysis/ScannenFlow/scanflow-ui-richtlijnen.md`.
- Overgangsstrategie tijdens de rework:
  - de bestaande scanimplementatie bleef tijdelijk functioneel bestaan;
  - de oude flow werd technisch en qua routing als `old` geïsoleerd;
  - de nieuwe implementatie hield de definitieve naamgeving op `/scan`.
- `PILOT-SCAN-02` is handmatig geaccepteerd:
  - oude flow staat op `/scan/old`;
  - `/scan` bleef de canonieke route en redirectte tijdelijk naar `/scan/old`;
  - navigatie bleef naar `/scan` wijzen.
- `PILOT-INV-05` bleef de oude scan/mutatiebasis totdat de nieuwe scanflow
  handmatig geaccepteerd was.
- Voor alle scanstories geldt sindsdien expliciet dat flow en UI samen beoordeeld
  worden; een technisch werkende route zonder duidelijke UI-vertaling naar de
  aangeleverde designs is niet acceptabel.
- `PILOT-SCAN-03A` is handmatig geaccepteerd:
  - bekende productscans landen in nieuwe scanroutes;
  - muteren en voorraad toevoegen werken binnen nieuwe productflow-schermen;
  - locatie-QR scannen en handmatige locatiecode-invoer werken in de add-stock-flow;
  - er is geen zichtbare terugval naar `/scan/old` binnen het bekende-product-pad.
- `PILOT-SCAN-04` is handmatig geaccepteerd:
  - bekende locatie-QR's landen in een nieuwe locatie-werkcontext;
  - muteren op bestaand product blijft binnen nieuwe scanroutes;
  - `ander product toevoegen` houdt de locatiecontext vast;
  - productselectie ondersteunt deelzoeking en zichtbare barcode-scan in de nieuwe
    scanstijl;
  - er is geen zichtbare terugval naar oude scanflow-pagina's of de generieke
    locatiepagina als scan-eindervaring.
- `PILOT-SCAN-05` is technisch gecontroleerd en handmatig geaccepteerd:
  - onbekende codes gaan vanuit `/scan` naar een nieuw onbekende-code-scherm;
  - de gebruiker kan binnen nieuwe scanroutes kiezen voor nieuw product, koppelen of
    annuleren;
  - de nieuwe productaanmaakroute vereist expliciete keuze van standaardeenheid;
  - er is geen zichtbare terugval naar oude scanflow-pagina's of generieke
    beheerpagina's als eindervaring.
- Follow-upfix op 2026-06-25, inmiddels gemerged en handmatig bevestigd:
  - locatie-QR's in `ScanProductAddStock` na nieuw product vanuit onbekende code
    gebruikten niet hetzelfde callback- en resolvepad als `/scan`;
  - technische fix en regressietests zijn lokaal groen;
  - handmatige bevestiging op mobiel/Raspberry Pi is akkoord.
- Extra expliciete herprioritering op 2026-06-25:
  - vóór de logboekvervolgstories eerst extra pilotgebruiksgemak op home en in
    `Voorraadbeheer > Producten`;
  - `.docs/analysis/stitch_responsive_bootstrap_process_design/` is leidend voor
    deze slices;
  - implementaties moeten die mockups als verplichte ontwerprichting volgen en mogen
    niet terugvallen naar generieke bootstrap- of CRUD-layouts.
- `PILOT-UX-01` is technisch gecontroleerd en handmatig geaccepteerd:
  - home is nu de standaard startpagina in plaats van directe doorstuur naar
    dashboard;
  - home volgt de mockup-geleide pilot-hub met snelle tegels naar `Logboek`,
    `Dashboard` en `Scannen`;
  - home-productzoekresultaten tonen productnaam, hoeveelheid, eenheid en locaties;
  - een klik op een home-product opent eerst productinformatie en biedt daarna direct
    de verbruiksroute met vooraf geselecteerd product.

## Laatste verificatiehistorie

Laatste volledige handoffverificatie: 2026-06-19.

- Handmatige acceptatie van `PILOT-LOC-04` is geslaagd.
- Gerichte storage/navigation unit-tests voor de laatste navigatiecheck: 20/20.
- Eerdere bredere storage/tag unit-suite: 138/138.
- Gerichte storage-integratietests voor tokenvervanging en migratie: geslaagd.
- `dotnet build BootManager.sln --no-restore`: geslaagd.
- `git diff --check`: geslaagd.
- Bestaande repositorybrede baseline warnings buiten deze story blijven aanwezig.

## Verwijzingen

- Actuele release: `.docs/releases/holiday-pilot-2026.md`
- Archief afgeronde pilotstories:
  `.docs/releases/holiday-pilot-2026-archive-completed-stories.md`
- Huidige compacte sessiestart: `.codex/current-session-handoff.md`
