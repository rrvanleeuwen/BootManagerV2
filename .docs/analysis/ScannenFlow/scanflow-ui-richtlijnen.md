# UI-richtlijnen scanflow

## Doel

Dit document vertaalt het aangeleverde designpakket naar concrete UI-richtlijnen voor de toekomstige scanflow-implementatie in BootManager.

Bronnen:

- `stitch_responsive_bootstrap_process_design.zip`;
- `UIVoorbeeld1.png`;
- `UIVoorbeeld2.png`;
- `UIVoorbeeld3.png`;
- `marine_operations_inventory/DESIGN.md`.

Niet ieder voorbeeldscherm is letterlijk toepasbaar op BootManager. De voorbeelden zijn wel leidend voor:

- schermopbouw;
- knophiërarchie;
- informatieblokken;
- verhouding tussen scanactie, contextinformatie en mutatieformulieren.

## Visuele richting

### Algemene stijl

- rustig, zakelijk en taakgericht;
- lichte achtergronden en witte kaarten;
- nadruk op leesbaarheid boven decoratie;
- duidelijke scheiding tussen primaire en secundaire acties.

### Kleurgebruik

- blauw voor primaire bevestigingsacties zoals `Scannen`, `Opslaan`, `Opzoeken`;
- groen voor toevoegen of positief voorraadresultaat;
- rood voor afname, foutstatus of onbekende scan;
- grijs voor secundaire of afsluitende acties.

### Componentlogica

- primaire actieknoppen zijn gevuld en visueel dominant;
- secundaire acties zijn outlined of rustiger gevuld;
- kaarten groeperen context per taak;
- tabellen en lijsten zijn functioneel en dun gescheiden;
- statuschips tonen type, status of context compact.

## Belangrijkste schermtypen

## 1. Scanstartscherm

### Mobiel

Leidend voorbeeld:

- `scannen_start/screen.png`

Het mobiele scanstartscherm moet bestaan uit:

- een heldere schermtitel;
- een korte instructieregel;
- een prominente scankaart met camera-actie;
- een apart blok voor handmatige invoer;
- een blok met recente scans;
- vaste bottom navigation.

### Desktop

Leidend voorbeeld:

- `scannen_start_desktop/screen.png`

Het desktop scanstartscherm moet bestaan uit:

- een grote scanzone als primaire focus;
- een rechterkolom met handmatige invoer;
- een tweede kaart met recente scans;
- optionele snelle acties onderaan of in een zijpaneel;
- consistente linker- of bovennavigatie conform bestaand BootManager-patroon.

### Implementatieregel

De scanactie moet op dit scherm altijd visueel het zwaarst wegen. Handmatige invoer en recente scans zijn ondersteunend, niet gelijkwaardig.

## 2. Locatiecontextscherm

### Mobiel

Leidend voorbeeld:

- `locatie_details/screen.png`

Op mobiel moet de locatiecontext deze blokken tonen:

- locatiekop met naam en status;
- directe acties bovenaan;
- aanwezige voorraad als hoofdblok;
- recente activiteiten onder de voorraad;
- een zwevende of vaste snelle actie voor toevoegen indien dat de gekozen mobiele patroon wordt.

### Desktop

Leidend voorbeeld:

- `locatie_details_desktop/screen.png`

Op desktop moet de locatiecontext deze blokken tonen:

- duidelijke headerkaart met locatie-identiteit;
- hoofdactieknoppen rechts of naast de titel;
- voorraadtabel als primaire inhoud;
- zijpaneel of tweede kolom voor activiteit/context;
- ruimte voor status en totalen zonder dat dit de taak verstoort.

### Implementatieregel

Een locatiepagina moet direct bruikbaar zijn als werkcontext na een scan. De gebruiker moet daar zonder extra zoeken een product kunnen muteren of een ander product kunnen toevoegen.

## 3. Productcontext met mutatiepaneel

### Mobiel

Leidend voorbeeld:

- `product_mutatie/screen.png`

Het mobiele productscherm moet tonen:

- productidentiteit bovenaan;
- compacte samenvatting van huidige voorraad;
- een kaart `Nieuwe Mutatie`;
- daaronder een lijst `Laatste Mutaties`.

De mutatiekaart bevat minimaal:

- typekeuze;
- hoeveelheid;
- locatie;
- optionele notitie;
- een dominante knop `Mutatie opslaan`.

### Desktop

Leidend voorbeeld:

- `product_mutatie_desktop/screen.png`

Het desktop productscherm moet tonen:

- productkaart met kerngegevens;
- aparte kaart of kolom `Voorraad per Locatie`;
- aparte kaart `Nieuwe Mutatie`;
- aparte kaart `Mutatie Historie`.

### Implementatieregel

Bij scanflows is het productscherm geen klassiek beheerformulier, maar een werkblad. Productinfo, locatiekeuze en mutatie-invoer moeten in een oogopslag begrijpelijk zijn.

## 4. Historie / logboek

### Mobiel

Leidend voorbeeld:

- `historie_logboek/screen.png`

Mobiele historie gebruikt:

- filterchips of tab-achtige toggles;
- kaartgebaseerde mutatieregels;
- per regel datum, gebruiker, wijziging en typebadge;
- een knop om meer historie te laden.

### Desktop

Leidend voorbeeld:

- `historie_logboek_desktop/screen.png`

Desktop historie gebruikt:

- compacte filterbalk bovenaan;
- tabelweergave voor veel records;
- kleur- en chipcodering per mutatietype;
- eventueel export of print alleen als secundaire beheeractie.

### Implementatieregel

Historie ondersteunt controle en terugvinden, maar mag de primaire scan- en mutatieflow niet blokkeren. Daarom hoort historie visueel altijd na of naast de primaire taak.

## Interactieprincipes

## Actiehiërarchie

- Per scherm maximaal één dominante primaire actie.
- `Opslaan` en `Nu scannen` zijn steeds de sterkste CTA's.
- `Terug`, `Sluiten`, `Filters wissen` en vergelijkbare acties zijn visueel rustiger.

## Context eerst, formulier daarna

- Eerst tonen we altijd waar de gebruiker is:
  - welk product;
  - welke locatie;
  - welke voorraadcontext.
- Daarna tonen we pas het mutatieformulier.

## Geen zware modalketens

- Een modal is acceptabel voor een korte zoek- of koppelactie.
- Volledige werkflows horen op een volwaardig scherm of in een duidelijke kaartlayout.
- Als een modal gebruikt wordt, moet deze één hoofdtaak hebben.

## Mobiel versus desktop

### Mobiel

- één kolom;
- grote tappable knoppen;
- scanactie hoog op de pagina;
- compacte kaarten onder elkaar;
- bottom navigation blijft zichtbaar of voorspelbaar.

### Desktop

- twee- of driekolomsopbouw waar dat de taak versnelt;
- scanvlak blijft dominant;
- context, mutatie en historie mogen naast elkaar staan;
- zijbalknavigatie werkt goed voor beheer- en contextschermen.

## Toepassing op BootManager

## Wat we moeten overnemen

- duidelijke scanstart als eigen scherm;
- kaartgebaseerde contextblokken;
- visueel onderscheid tussen toevoegen, afname en neutrale acties;
- product- en locatiecontext als werkpagina's;
- compacte, goed leesbare historieblokken.

## Wat we niet letterlijk hoeven overnemen

- generieke warehouse-beelden en illustraties;
- enterprise-termen die niet passen bij bootgebruik;
- export-, report- of managementfuncties die buiten de pilotscope vallen;
- schermonderdelen die productbreedte of administratieve complexiteit suggereren die BootManager niet nodig heeft.

## Concreet schermmapping-advies

### `Scannen`

- baseer mobiel op `scannen_start`;
- baseer desktop op `scannen_start_desktop`.

### `Locatie na scan`

- baseer mobiel op `locatie_details`;
- baseer desktop op `locatie_details_desktop`.

### `Product na scan`

- baseer mobiel op `product_mutatie`;
- baseer desktop op `product_mutatie_desktop`.

### `Mutatiehistorie`

- baseer mobiel op `historie_logboek`;
- baseer desktop op `historie_logboek_desktop`.

## Concreet schermcontract voor BootManager

Deze sectie is leidend voor alle vervolgstories in het scanspoor. Een story is pas
acceptabel wanneer zowel de functionele flow als de bedoelde UI-vertaling zichtbaar is.
Een technisch werkende route die de gebruiker laat terugvallen op een oud CRUD-achtig
scherm geldt daarom niet als voldoende eindresultaat.

## 1. `Scan start`

### Doel

De gebruiker moet in minder dan enkele seconden begrijpen:

- dit is het centrale scanwerkgebied;
- hier kan ik direct scannen of handmatig invoeren;
- het systeem onthoudt mijn recente werk;
- de hoofdactie is nu scannen, niet zoeken of beheren.

### Mobiel: boven de vouw

Toon direct:

- paginatitel `Scannen`;
- korte instructieregel van maximaal één zin;
- één dominante scankaart met duidelijke primaire knop;
- één compact blok voor handmatige invoer.

Toon nog niet boven de vouw:

- uitgebreide technische statussen;
- meerdere concurrerende primaire knoppen;
- beheerinformatie of oude resultaatschermen.

### Desktop: eerste indruk

Toon direct:

- een dominante scanzone als primaire kolom;
- een rustige rechterkolom voor handmatige invoer en recents;
- voldoende witruimte en duidelijke kaartscheiding.

Voorkom:

- een formulierwand;
- gelijkgewicht tussen scan, historie en beheer;
- tabellen of detailvelden die de scanstart verdringen.

### Informatiehiërarchie

Volgorde:

1. wat kan ik nu doen;
2. hoe kan ik fallback gebruiken;
3. wat heb ik net gedaan.

### Acties

- primaire knop: `Camera starten` of equivalent;
- secundaire actie: handmatige invoer toepassen;
- recents zijn snelle hervat-acties, geen hoofdtaak.

## 2. `Product gescand`

### Doel

Na het scannen van een product moet de gebruiker direct begrijpen:

- welk product is herkend;
- op welke locaties dit product nu aanwezig is;
- wat de eerstvolgende logische actie is;
- hoe hij snel voorraad kan muteren zonder te verdwalen.

### Harde UX-regel

Een productscan mag niet eindigen op een generieke oude locatie- of beheerpagina als
eindervaring. Zolang een tijdelijke technische handoff nodig is, moet de vervolgstap
nog steeds aanvoelen als onderdeel van de nieuwe scanflow en niet als terugval naar een
legacy CRUD-scherm.

### Mobiel

Toon in deze volgorde:

- productkaart met naam, code en compacte status;
- primaire actiekaart `Muteren op bestaande locatie`;
- secundaire actiekaart `Voorraad op andere locatie toevoegen`;
- compacte lijst `Voorraad per locatie`;
- pas daarna eventuele historie.

Beperk tot minimale informatie:

- geen brede beheerformulieren;
- geen irrelevante productmetadata;
- geen zware tabellen boven de hoofdactie.

### Desktop

Toon in duidelijke kaarten of kolommen:

- productidentiteit;
- voorraad per locatie;
- nieuwe mutatie;
- recente mutaties.

De mutatiekaart moet visueel dominanter zijn dan historische of administratieve blokken.

### Actielogica

- precies één dominante eerstvolgende actie;
- locatiekeuze of mutatiekeuze moet direct zichtbaar zijn;
- de gebruiker hoeft niet te zoeken naar de werkactie.

## 3. `Locatie gescand`

### Doel

Na het scannen van een locatie moet de gebruiker direct begrijpen:

- welke locatie actief is;
- welke producten daar nu liggen;
- hoe hij direct op bestaand product muteert;
- hoe hij snel een ander product aan deze locatie toevoegt.

### Harde UX-regel

Een locatiescan mag niet landen op een klassieke detailpagina die primair voelt als
beheer- of registratiescherm. Het moet een werkcontext zijn: overzichtelijk, actiegericht
en duidelijk scan-gerelateerd.

### Mobiel

Toon in deze volgorde:

- locatiekop met naam en status;
- primaire actie `Muteren op bestaand product`;
- secundaire actie `Ander product toevoegen`;
- compacte voorraadlijst;
- pas daarna recente activiteit of aanvullende details.

### Desktop

Toon in kaarten/kolommen:

- locatie-identiteit;
- actieblok;
- voorraad als hoofdinhoud;
- activiteit of context als tweede inhoudsblok.

### Informatiebeperking

Laat alleen zien wat nodig is om direct verder te werken:

- productnaam;
- hoeveelheid;
- relevante locatiecontext;
- duidelijke volgende actie.

Vermijd:

- overmatige detailvelden;
- administratieve metadata zonder taakwaarde;
- schermen die voelen als een database-record.

## Beslisregel voor acceptatie

Bij iedere scanstory geldt voortaan:

- flow correct, UI onduidelijk: niet acceptabel;
- UI mooi, flow onduidelijk: niet acceptabel;
- pas akkoord als de gebruiker zowel begrijpt wat er gebeurt als direct ziet wat de
  volgende logische actie is.

## Implementatie-afspraken voor de volgende fase

- Nieuwe scanflow-schermen volgen deze documentatie als referentie voor layout en interactie.
- Voorbeelden uit het zip-pakket gelden als patroonbibliotheek, niet als pixelverplichte kopie.
- Iedere update in het scanspoor bevat voortaan expliciet:
  - de functionele flowwijziging;
  - de bedoelde UI-vertaling op mobiel en desktop;
  - de reden waarom de gekozen eindschermen de gebruiker niet laten terugvallen op een
    oud beheer- of CRUD-patroon.
- Bij twijfel is de volgorde:
  1. functionele flow uit `scanflow-herdefinitie.md`;
  2. daarna de UI-principes uit dit document;
  3. pas daarna bestaande huidige schermdetails.
