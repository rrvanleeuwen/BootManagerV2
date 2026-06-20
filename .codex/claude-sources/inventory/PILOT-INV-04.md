# PILOT-INV-04

Bron: `.docs/releases/holiday-pilot-2026.md`

### PILOT-INV-04 — Product terugvinden via scan of zoeken

**Storyzin**  
Als Owner of Crew wil ik een product snel kunnen terugvinden via scannen of handmatig
zoeken, zodat ik direct zie op welke locatie of locaties het product ligt en daar
desgewenst naartoe kan navigeren.

**Waarom deze slice nu**  
Na de catalogusbasis van `PILOT-INV-01`, de locatiegebonden voorraadbasis van
`PILOT-INV-02` en de scan-gestuurde inruimflow van `PILOT-INV-03` is de volgende
praktische vraag: waar ligt iets? Deze story maakt het terugvinden van producten snel via
de voorkeursroute scannen en via een handmatige zoekfallback, zonder al voorraadmutaties
of dashboardzoekingangen te introduceren.

**Scope**

- De primaire route start vanuit het bestaande menu `Scannen`.
- Als in `Scannen` een bekende productcode wordt gescand, start direct de
  terugvindflow.
- Handmatige fallback is beschikbaar via `Voorraadbeheer > Producten`.
- Handmatig zoeken werkt op productnaam en productomschrijving.
- Handmatig zoeken is hoofdletterongevoelig en ondersteunt deelmatches.
- Als handmatig zoeken meerdere producten vindt, toont BootManager een korte
  productresultatenlijst waaruit de gebruiker een product kiest.
- Die resultatenlijst toont per product minimaal:
  - productnaam;
  - de eerste tekens van de omschrijving als die bestaat;
  - de bekende locaties van dat product als komma-gescheiden samenvatting.
- Hoeveelheden worden nog niet in die eerste resultatenlijst getoond.
- Als een gescand of gekozen product precies een actieve voorraadlocatie heeft, opent
  BootManager direct de locatiepagina van die locatie.
- Als een gescand of gekozen product meerdere actieve voorraadlocaties heeft, toont
  BootManager direct een lijst met die locaties.
- Die lijst toont minimaal gebied, locatienaam, hoeveelheid en eenheid per locatie.
- Vanuit die lijst kan de gebruiker doorklikken naar de locatiepagina van een gekozen
  locatie.
- Als een product bekend is maar momenteel geen actieve voorraadlocaties heeft, meldt
  BootManager dat duidelijk.
- Als voor dat product nog een laatst gebruikte locatie bekend is, toont BootManager
  die echte locatieverwijzing als verwachte plek waar het product normaal hoort te
  liggen, weergegeven als leesbare gebied- en locatienaam.
- In beide gevallen biedt BootManager een vervolgstap `Voorraad toevoegen`.

**Buiten scope**

- Dashboard-zoekbalk of andere nieuwe dashboardingangen.
- Verbruik, correcties, voorraadhistorie of andere voorraadmutaties vanuit de
  terugvindflow.
- Uitgebreide filters op categorie, gebied of andere velden.
- Echte typo-correctie, fuzzy matching of synoniembeheer.
- Hoeveelheden tonen in de eerste productresultatenlijst van handmatig zoeken.

**Acceptatiecriteria**

1. De gebruiker kan vanuit `Scannen` een bekende productcode scannen en direct de
   terugvindflow starten.
2. De gebruiker kan ook handmatig zoeken via `Voorraadbeheer > Producten`.
3. Handmatig zoeken doorzoekt productnaam en omschrijving, is hoofdletterongevoelig en
   ondersteunt deelmatches.
4. Als handmatig zoeken meerdere producten vindt, toont BootManager een korte lijst met
   productnaam, eerste omschrijvingstekens en locatiesamenvatting, waarna de gebruiker
   een product kiest.
5. Als een gescand of gekozen product precies een actieve voorraadlocatie heeft, opent
   direct de locatiepagina van die locatie.
6. Als een gescand of gekozen product meerdere actieve voorraadlocaties heeft, toont
   BootManager direct een lijst met gebied, locatienaam, hoeveelheid en eenheid per
   locatie.
7. Vanuit die locatielijst kan de gebruiker doorklikken naar een locatiepagina.
8. Als een product bekend is maar geen actieve voorraadlocaties heeft, meldt
   BootManager dat duidelijk.
9. Als voor dat product nog een laatst gebruikte locatie bekend is, toont BootManager
   die locatie als verwachte plek waar het product normaal hoort te liggen.
10. In beide gevallen biedt BootManager een actie `Voorraad toevoegen`.

**Legacy-impact**

- Dekt primair `US2.6` barcode scannen bij zoeken en de product-terugvindkant van
  `US2.9` voorraad bekijken per locatie.
- Bouwt voort op de gekoppelde codes uit `PILOT-INV-01` en de voorraadregels per
  locatie uit `PILOT-INV-02`.
- Laat `US2.10` voorraad aanpassen, `US2.12` breder zoeken en filteren, `US2.13`
  voorraadlogboek en `US2.20` verbruik via barcode bewust open voor latere
  inventory-slices.

**Handmatige acceptatietest**

1. Open `Scannen` en scan een bekende productcode van een product dat op precies een
   locatie ligt; controleer dat direct de juiste locatiepagina opent.
2. Scan een bekende productcode van een product dat op meerdere locaties ligt;
   controleer dat direct een locatielijst opent met gebied, locatienaam, hoeveelheid en
   eenheid.
3. Klik vanuit die lijst door naar een locatiepagina en controleer dat de juiste
   locatie wordt geopend.
4. Open `Voorraadbeheer > Producten` en zoek handmatig op een productnaam met
   hoofdletterverschil, bijvoorbeeld `rijst` versus `Rijst`; controleer dat het product
   gevonden wordt.
5. Zoek handmatig op tekst die alleen in de omschrijving voorkomt; controleer dat het
   product gevonden wordt.
6. Controleer dat meerdere zoekresultaten eerst een korte productlijst tonen met
   productnaam, omschrijvingstekst en locatiesamenvatting, zonder hoeveelheden.
7. Kies een product uit die lijst en controleer dat het vervolggedrag gelijk is aan de
   scanroute: direct locatiepagina bij een locatie, of locatielijst bij meerdere
   locaties.
8. Open een bekend product zonder actieve voorraadlocaties en controleer dat
   BootManager meldt dat het momenteel niet op voorraad is.
9. Controleer dat, als voor dit product nog een laatst gebruikte locatie bekend is,
   BootManager die als verwachte plek toont.
10. Controleer dat in beide gevallen een actie `Voorraad toevoegen` beschikbaar is.
