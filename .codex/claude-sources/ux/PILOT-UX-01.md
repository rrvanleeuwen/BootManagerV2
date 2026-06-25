# PILOT-UX-01

Bron: `.docs/releases/holiday-pilot-2026.md`

### PILOT-UX-01 — Home optimaliseren als snelle pilot-hub

**Storyzin**
Als Owner of Crew wil ik op de homepagina direct naar `Logboek`, `Dashboard` en
`Scannen` kunnen gaan en meteen producten kunnen zoeken, zodat de meest gebruikte
pilotacties zonder omwegen bereikbaar zijn.

**Waarom deze slice nu**
Tijdens de pilot zijn snelheid, duidelijkheid en minimale navigatie belangrijker dan
extra functionele diepte. Deze slice maakt de homepagina een echte dagelijkse
startplek voor Roelof en Carla.

**Scope**

- De homepagina krijgt drie duidelijke primaire tegels:
  - `Logboek`;
  - `Dashboard`;
  - `Scannen`.
- De homepagina krijgt een productzoekwidget als directe actie zonder extra navigatie.
- De zoekwidget toont per resultaat:
  - productnaam;
  - totale hoeveelheid;
  - eenheid;
  - locaties waar het product te vinden is.
- Resultaten worden per 10 items gepagineerd.
- Desktop en groter:
  - resultaten als compacte lijst;
  - visuele hiërarchie in de richting van
    `.docs/analysis/stitch_responsive_bootstrap_process_design/home_desktop/code.html`.
- Mobiel:
  - resultaten als cards;
  - visuele hiërarchie in de richting van
    `.docs/analysis/stitch_responsive_bootstrap_process_design/home/code.html`.

**Buiten scope**

- Nieuwe dashboardinhoud of extra dashboardwidgets buiten de snelle doorsteek.
- Nieuwe logboekfunctionaliteit.
- Wijzigingen aan scanflows.
- Uitgebreide statistiek- of beheerblokken op home die niet direct bijdragen aan de
  snelle pilotstart.

**Ontwerprichting is verplicht**

- De mockups in
  `.docs/analysis/stitch_responsive_bootstrap_process_design/home/` en
  `.docs/analysis/stitch_responsive_bootstrap_process_design/home_desktop/` zijn voor
  deze slice leidend, niet slechts inspirerend.
- Claude mag de layout technisch vertalen naar de bestaande app, maar niet vrij
  terugvallen naar een generieke CRUD-, tabel- of standaard-bootstrap-home.
- Afwijkingen van de mockup zijn alleen acceptabel wanneer een bestaand
  design-system- of autorisatiepatroon dat aantoonbaar vereist; zulke afwijkingen
  moeten dan expliciet in de reviewnotities worden benoemd.

**Acceptatiecriteria**

1. Home toont direct zichtbare primaire tegels voor `Logboek`, `Dashboard` en
   `Scannen`.
2. De productzoekwidget is op home bruikbaar zonder extra navigatie.
3. Elk resultaat toont productnaam, hoeveelheid, eenheid en locaties.
4. Resultaten zijn gepagineerd per 10 items.
5. Desktop toont resultaten als lijst; mobiel toont resultaten als cards.
6. De resulterende UI volgt aantoonbaar de aangeleverde mockup-hiërarchie en voelt
   niet als generieke bootstrap-lijst.

**Handmatige acceptatietest**

1. Open home op desktop en controleer dat `Logboek`, `Dashboard` en `Scannen` als
   duidelijke primaire tegels zichtbaar zijn.
2. Zoek een product dat op meerdere locaties ligt en controleer dat productnaam,
   hoeveelheid, eenheid en alle locaties zichtbaar zijn.
3. Controleer paginering na meer dan 10 resultaten.
4. Herhaal op mobiel en controleer dat dezelfde resultaten als cards verschijnen.
5. Vergelijk desktop en mobiel globaal met de aangeleverde mockups en controleer dat
   de taakhiërarchie herkenbaar overeenkomt.
