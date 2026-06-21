# Herdefinitie scanflows

## Doel

Dit document herdefinieert de scanflows voor BootManager op basis van:

- `Procesflow_Scannen.png`;
- `Procesflow_Scannen.drawio`;
- de UI-voorbeelden en schermsets in `stitch_responsive_bootstrap_process_design.zip`.

De bedoeling is nadrukkelijk niet om de huidige implementatie te verdedigen, maar om een nieuwe, heldere functionele basis vast te leggen voor de volgende implementatiefase.

## Ontwerpuitgangspunten

- `Scannen` is een zelfstandig werkgebied en niet alleen een camera-overlay.
- Een scan moet direct leiden tot een begrijpelijke vervolgstap: locatiecontext, productcontext of onbekende-code-afhandeling.
- De gebruiker werkt vanuit context:
  - vanaf een product;
  - vanaf een locatie;
  - of vanuit een neutrale scanstart.
- Muteren gebeurt altijd met expliciete context voor product en locatie.
- De flow moet herhaalbaar zijn zonder dat de gebruiker telkens terugvalt naar beheer- of zoekpagina's.
- Handmatige invoer blijft beschikbaar als fallback, maar is ondergeschikt aan scannen.

## Hoofdstructuur van de nieuwe scanflow

### 1. Centrale start: `Scannen`

De flow start vanuit een centraal scherm `Scannen` met drie functies:

- camera scannen;
- handmatige code-invoer;
- recente scans / snelle herstart van eerder werk.

Vanuit dit startpunt zijn er drie uitkomsten:

1. de code is een bekende locatie;
2. de code is een bekend product;
3. de code is onbekend.

## Hoofdscenario A: locatie gescand

### Verwacht gedrag

Wanneer een locatiecode wordt gescand:

- opent BootManager direct de locatiecontext;
- toont het systeem de locatie-informatie;
- toont het systeem de producten die op die locatie aanwezig zijn;
- kan de gebruiker vanuit die locatie direct muteren of een ander product toevoegen.

### Acties binnen locatiecontext

Vanuit het locatiescherm zijn er twee primaire acties:

1. `Muteren op bestaand product`
2. `Voorraad van ander product toevoegen`

### A1. Muteren op bestaand product binnen locatie

De gebruiker kiest een bestaand product binnen de locatiecontext. Daarna:

- BootManager opent een compacte mutatie-invoer;
- de locatie staat vast;
- de gebruiker voert een hoeveelheid in voor `+` of `-`;
- BootManager verwerkt de mutatie;
- daarna volgt de vraag of de gebruiker wil doorgaan met deze locatiecontext.

### A2. Ander product toevoegen aan al gescande locatie

De gebruiker kiest `Voorraad van ander product toevoegen`. Daarna:

- BootManager vraagt om een productbarcode te scannen of handmatig op te zoeken;
- bij een bekend product toont BootManager productinfo;
- de locatie blijft vast op de eerder gescande locatie;
- de gebruiker voert alleen een `+` hoeveelheid in;
- BootManager verwerkt de mutatie;
- daarna volgt opnieuw de keuze om door te gaan binnen dezelfde locatiecontext.

### A3. Onbekend product binnen al gescande locatie

Als de productbarcode binnen deze route onbekend is:

- BootManager laat de gebruiker een nieuw product vastleggen;
- productgegevens en beginhoeveelheid worden in dezelfde flow ingevoerd;
- de koppeling met de al actieve locatie wordt direct mee opgeslagen;
- daarna kan de gebruiker verder binnen dezelfde locatiecontext.

### Belangrijke ontwerpregel

Bij een reeds gescande locatie mag de gebruiker niet opnieuw om locatiecontext gevraagd worden, tenzij hij expliciet kiest om van locatie te wisselen.

## Hoofdscenario B: product gescand

### Verwacht gedrag

Wanneer een productbarcode wordt gescand:

- opent BootManager de productcontext;
- toont het systeem productinfo;
- toont het systeem de bekende locaties van dat product;
- toont het systeem de aanwezige voorraad per locatie;
- van daaruit kiest de gebruiker of hij een bestaande locatie muteert of voorraad op een andere locatie toevoegt.

### Acties binnen productcontext

Vanuit het productscherm zijn er twee primaire acties:

1. `Muteren op bestaande locatie`
2. `Voorraad op andere locatie toevoegen`

### B1. Muteren op bestaande locatie

De gebruiker kiest een bestaande locatiekaart of actie-icoon bij een locatie. Daarna:

- BootManager opent de mutatie-invoer;
- product en locatie staan vast;
- de gebruiker voert een `+` of `-` hoeveelheid in;
- BootManager verwerkt de mutatie;
- daarna volgt de vraag of de gebruiker wil doorgaan met hetzelfde product.

### B2. Voorraad op andere locatie toevoegen

De gebruiker kiest `Voorraad op andere locatie toevoegen`. Daarna:

- BootManager vraagt om een locatie te selecteren via QR-scan of lijst;
- na locatiekeuze staat het product vast en hoeft alleen de doel-locatie nog bepaald te worden;
- de gebruiker voert een `+` hoeveelheid in op de geselecteerde locatie;
- BootManager verwerkt de mutatie;
- daarna volgt de vraag of de gebruiker wil doorgaan met hetzelfde product.

### Belangrijke ontwerpregel

Bij een reeds gescand product is het logisch om door te werken op dat product. De flow moet daarom productgericht herhaalbaar zijn: eerst dit product afronden, pas daarna terug naar een generieke scanstart.

## Hoofdscenario C: onbekende code gescand

### Verwacht gedrag

Bij een onbekende scan moet BootManager niet stilvallen. De flow vraagt expliciet:

- is dit een nieuw product?

### C1. Ja, nieuw product

Bij `Ja`:

- de gebruiker vult productgegevens in;
- de gebruiker vult een beginhoeveelheid in;
- de gebruiker kiest of scant daarna de locatie;
- BootManager bewaart het product en de locatiekoppeling;
- de flow eindigt niet abrupt maar sluit logisch aan op verder werken.

### C2. Nee, geen nieuw product

Bij `Nee`:

- BootManager keert terug naar de scanstart;
- er wordt niets opgeslagen;
- de gebruiker kan direct opnieuw scannen.

### Belangrijke ontwerpregel

De flow voor een onbekende code moet expliciet, kort en veilig zijn. Geen impliciete productcreatie, geen verborgen koppelingen en geen verlies van de net gescande context.

## Beslisregels

### Codeherkenning

- Bekende locatiecode: open locatiecontext.
- Bekende productcode: open productcontext.
- Onbekende code: start onbekende-code-flow.

### Contextbehoud

- In locatiecontext blijft de locatie leidend totdat de gebruiker wisselt of stopt.
- In productcontext blijft het product leidend totdat de gebruiker wisselt of stopt.
- In onbekende-code-flow blijft de net gescande code leidend totdat de gebruiker annuleert of opslaat.

### Mutatietype

Op basis van de flowplaat is de nieuwe scanstructuur generiek bedoeld voor `+` en `-` mutaties. Voor de uitwerking in stories moet dit verder worden gespecificeerd in:

- toevoegen / ontvangst;
- verbruik / afname;
- correctie / telling waar relevant.

De hoofdregel blijft dat het scherm eerst context vastlegt en pas daarna om de mutatie vraagt.

## Navigatieprincipes

- De gebruiker blijft zoveel mogelijk binnen een taaklus.
- Scannen leidt niet direct naar brede beheerpagina's zonder duidelijke reden.
- Terugnavigatie moet begrijpelijk zijn:
  - vanuit locatie-acties terug naar locatie;
  - vanuit product-acties terug naar product;
  - vanuit scanstart terug naar reguliere navigatie.

## Functionele implicaties voor latere implementatie

### Wat leidend moet worden

- `Scannen` wordt een volwaardig startscherm.
- Product- en locatiecontext worden twee verschillende werkmodi.
- Toevoegen van nieuwe voorraad is niet overal hetzelfde:
  - vanuit locatie: kies eerst locatie, dan product;
  - vanuit product: kies eerst product, dan locatie.
- Onbekende codes krijgen een korte expliciete beslisflow.

### Wat we expliciet willen vermijden

- extra omwegen via beheerpagina's;
- herhaald opnieuw scannen van context die al vaststaat;
- schermen waar product, locatie en mutatietype tegelijk onduidelijk zijn;
- modals of formulieren zonder duidelijke hoofdactie.

## Open punten voor stories en packets

- Welke mutatietypes exact in scancontext zichtbaar zijn per route;
- wanneer `+` en `-` als aparte knoppen getoond worden versus een typeselectie;
- hoe recents/history op het scanstartscherm precies gebruikt worden;
- of onbekende codes ook aan een bestaand product gekoppeld mogen worden in deze nieuwe flow;
- welke validaties verplicht zijn bij beginhoeveelheid, nulwaarden en negatieve invoer.

## Aanbevolen opsplitsing voor implementatie

De nieuwe implementatie leent zich voor aparte stories:

1. nieuw scanstartscherm met routering op code-type;
2. locatiegerichte scanmodus;
3. productgerichte scanmodus;
4. onbekende-code-flow;
5. uniforme mutatiecomponent voor beide contexten;
6. historie- en recents-integratie.
