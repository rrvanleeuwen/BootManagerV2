# PILOT-INV-02

Bron: `.docs/releases/holiday-pilot-2026.md`

### PILOT-INV-02 — Taakgerichte voorraadbasis per locatie

**Storyzin**  
Als Owner of Crew wil ik vanaf een locatiepagina voorraad aan die locatie kunnen
toevoegen en aanvullen, zodat BootManager bruikbaar vastlegt wat waar ligt zonder mij
door administratieve CRUD-schermen te dwingen.

**Waarom deze slice nu**  
Deze story maakt inventory voor het eerst praktisch bruikbaar door de catalogus uit
`PILOT-INV-01` te verbinden aan echte opslaglocaties en hoeveelheden. De focus ligt op
taakgericht vastleggen en tonen van actuele voorraad per locatie. Scan-gestuurde
hoofdroutes, product-terugvinden via barcode en mutatiehistorie blijven bewust voor
latere stories.

**Scope**

- Owner en Crew kunnen vanaf een locatiepagina de actie `Voorraad toevoegen` starten.
- De primaire route start vanaf een locatiepagina; dezelfde locatie moet ook zonder scan
  handmatig bereikbaar zijn via bestaande locatienavigatie.
- Binnen een flow `Voorraad toevoegen` kiest de gebruiker een bestaand product of maakt
  direct een nieuw product aan vanuit die locatiecontext.
- Als tijdens deze flow een nieuw product wordt aangemaakt, keert de gebruiker daarna
  automatisch terug naar dezelfde locatieflow met dat product geselecteerd.
- Een voorraadregel legt functioneel alleen `product`, `locatie` en `hoeveelheid` vast.
- Hoeveelheid is een vrij numerieke waarde in de standaard eenheid van het product.
- Hetzelfde product kan op meerdere locaties tegelijk voorraad hebben.
- Per locatie bestaat voor een product maximaal een actuele voorraadregel.
- Als een product op die locatie al bestaat, wordt dezelfde voorraadregel hergebruikt en
  wordt de hoeveelheid aangevuld.
- De locatiepagina toont de actuele inhoud van die locatie met minimaal productnaam,
  hoeveelheid en eenheid.
- De productpagina toont op welke locaties het product ligt, met minimaal gebied,
  locatienaam en hoeveelheid.
- Een voorraadregel kan vanaf de locatiepagina eenvoudig worden verwijderd na
  bevestiging wanneer het product daar niet meer ligt.

**Buiten scope**

- Scan-gestuurde dashboardstart of automatische keuze van de juiste voorraadactie.
- Verplichte locatie-QR als hoofdroute voor productaanmaak of inruimen.
- Barcode scannen om een product terug te vinden.
- Verbruik, correcties, overschrijven van hoeveelheden, negatieve hoeveelheden en
  mutatiehistorie.
- Voorraad verplaatsen tussen twee locaties als samengestelde actie.
- Slimme recente lijsten, voorkeursproducten per locatie of automatische suggesties.
- Categorie-filters in de handmatige productzoekflow.
- Meerdere aparte voorraadregels voor hetzelfde product op dezelfde locatie.

**Acceptatiecriteria**

1. Owner en Crew kunnen een locatiepagina handmatig openen zonder scan en daar de actie
   `Voorraad toevoegen` starten.
2. In `Voorraad toevoegen` kan de gebruiker een bestaand product zoeken op productnaam of
   gekoppelde code.
3. In dezelfde flow kan de gebruiker ook direct een nieuw product aanmaken; na opslaan
   keert de flow terug naar dezelfde locatie met dat product geselecteerd.
4. De gebruiker kan vervolgens een vrij numerieke hoeveelheid invoeren en opslaan voor
   die locatie.
5. Als het gekozen product nog niet op die locatie ligt, ontstaat een nieuwe
   voorraadregel voor die product-locatie-combinatie.
6. Als het gekozen product al op die locatie ligt, wordt geen tweede regel aangemaakt
   maar wordt de bestaande hoeveelheid aangevuld.
7. Een actieve voorraadregel met hoeveelheid `0` of lager is niet toegestaan in deze
   story; zulke invoer wordt geblokkeerd.
8. De locatiepagina toont na opslaan de actuele producten op die locatie met minimaal
   naam, hoeveelheid en eenheid.
9. De productpagina toont voor een product alle gekoppelde locaties met minimaal gebied,
   locatienaam en hoeveelheid.
10. Een voorraadregel kan vanaf de locatiepagina na bevestiging direct verwijderd worden
    als het product daar niet meer ligt.

**Legacy-impact**

- Dekt primair `US2.8` product koppelen aan opslaglocatie en `US2.9` voorraad bekijken
  per locatie.
- Levert een eerste, bewust beperkte invulling van `US2.19` automatisch ophogen bij
  nieuwe voorraad op dezelfde locatie, maar zonder brede mutatielogica of
  aankoophistorie.
- Laat `US2.10` voorraad aanpassen, `US2.13` voorraadlogboek, `US2.14`
  QR-scanner-modus en `US2.20` verbruik via barcode bewust open voor latere
  inventory-slices.

**Handmatige acceptatietest**

1. Log in als Owner of Crew.
2. Open handmatig een bestaande locatiepagina via de locatienavigatie.
3. Controleer dat de actie `Voorraad toevoegen` beschikbaar is.
4. Start `Voorraad toevoegen`, zoek een bestaand product op naam of gekoppelde code, vul
   een hoeveelheid in en sla op.
5. Controleer dat de locatiepagina daarna het product toont met hoeveelheid en eenheid.
6. Start `Voorraad toevoegen` opnieuw voor hetzelfde product op dezelfde locatie, voer een
   extra hoeveelheid in en controleer dat de bestaande regel wordt aangevuld in plaats
   van gedupliceerd.
7. Start `Voorraad toevoegen` nogmaals en maak vanuit die flow een nieuw product aan;
   controleer dat je automatisch terugkeert naar dezelfde locatieflow en daarna een
   hoeveelheid voor dat nieuwe product kunt opslaan.
8. Open de productpagina van een opgeslagen product en controleer dat alle gekoppelde
   locaties zichtbaar zijn met gebied, locatienaam en hoeveelheid.
9. Probeer een hoeveelheid `0` of lager op te slaan; verwacht een duidelijke blokkade.
10. Verwijder een voorraadregel vanaf de locatiepagina en controleer dat deze na
    bevestiging uit de actuele locatie-inhoud verdwijnt.
