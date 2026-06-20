# PILOT-INV-01

Bron: `.docs/releases/holiday-pilot-2026.md`

### PILOT-INV-01 — Productcategorieen, producten en productbarcodes

**Storyzin**  
Als Owner of Crew wil ik productcategorieen, eenheden en producten met basisgegevens en
een gekoppelde code kunnen vastleggen en beheren, zodat een lokale productcatalogus
ontstaat die klaar is voor latere voorraad- en scanflows.

**Waarom deze slice nu**  
Deze story levert de minimale catalogusbasis voor inventory zonder al voorraadregels,
locatiekoppelingen of scanrouting te introduceren. Daarmee wordt eerst productidentiteit
stabiel gemaakt; `PILOT-INV-02` kan daarna de taakgerichte voorraadbasis per product en
locatie toevoegen, en `PILOT-INV-03` kan vervolgens de scan-gestuurde inruimflow
uitwerken.

**Scope**

- Owner en Crew kunnen via inventory-beheerflows productcategorieen, eenheden en
  producten aanmaken, bewerken, archiveren en heractiveren.
- Een categorie heeft minimaal een unieke naam, een optionele korte omschrijving en een
  verplichte icoonkeuze uit een beperkte ingebouwde set.
- Een eenheid is een aparte herbruikbare referentie-entiteit met een unieke naam; de
  applicatie start met een kleine seed/defaultset.
- Een product heeft minimaal naam, standaard eenheid en optionele omschrijving.
- Een product krijgt in de UI exact een categorie, maar de modellering wordt voorbereid
  als product-categorie-koppeling; in deze story blijft maximaal een actieve
  categoriekoppeling per product toegestaan.
- Productaanmaak kan inline een nieuwe categorie of nieuwe eenheid opslaan en daarna de
  productflow vervolgen.
- Per product kan nul of een gekoppelde code worden vastgelegd als aparte entiteit.
- Een gekoppelde code heeft minimaal een waarde en een formaat/type, mag via scan of
  handmatige invoer worden toegevoegd, en mag zowel een standaardbarcode als een vrije
  tekstcode zijn.
- De hoofdnavigatie krijgt een inventory-/voorraadbeheermenu met submenu-items
  `Producten`, `Categorieen` en `Eenheden`.
- `Producten` gebruikt een apart formulier of scherm voor aanmaken en bewerken.
- `Categorieen` en `Eenheden` gebruiken eenvoudige beheerlijsten met simpele modals voor
  aanmaken en bewerken.
- Vanuit het productformulier kan de gebruiker in een kleine modal direct een nieuwe
  categorie of eenheid toevoegen; na opslaan keert de flow terug naar het
  productformulier met de nieuwe keuze beschikbaar.
- De gekoppelde code wordt beheerd als onderdeel van het productformulier en niet via
  een apart submenu of apart detailscherm.
- Product-, categorie- en eenheidslijsten zijn leesbaar genoeg om de vastgelegde
  catalogus te controleren; gearchiveerde records zijn standaard verborgen maar via een
  archiefweergave of filter terug te vinden.

**Buiten scope**

- Voorraadregels, hoeveelheden, mutaties en historie.
- Product aan opslaglocatie koppelen.
- Product op meerdere locaties tonen of beheren.
- Product aanmaken vanuit een reeds gescande locatie of verplichte locatie-scan in de
  flow.
- Barcode scannen om een product terug te vinden.
- Onbekende barcode-afhandeling tijdens scannen.
- Meerdere actieve categorieen per product in de UI.
- Meerdere gekoppelde codes per product, product-QR-codes, foto/label, minimumvoorraad,
  filteren en dashboardintegratie.
- Vrij uploadbare categorie-iconen; deze story gebruikt alleen een kleine vaste set.
- Eenheidsclassificaties, merk/fabrikant en interne SKU's.

**Acceptatiecriteria**

1. Owner en Crew kunnen categorieen beheren met unieke naam, optionele omschrijving en
   verplicht icoon uit een vaste set.
2. Owner en Crew kunnen eenheden beheren vanuit een aparte beheerflow; namen zijn uniek
   en de applicatie levert een kleine defaultset.
3. Owner en Crew kunnen tijdens productaanmaak inline een nieuwe categorie of eenheid
   toevoegen zonder eerst naar een aparte beheerpagina terug te hoeven.
4. Owner en Crew kunnen een product opslaan met naam, exact een gekozen categorie in de
   UI, exact een verplichte standaard eenheid en optionele omschrijving.
5. Een product kan zonder gekoppelde code worden opgeslagen; een gekoppelde code kan bij
   aanmaak of later via scan of handmatige invoer worden toegevoegd, vervangen of
   ontkoppeld.
6. Een gekoppelde code wordt als aparte entiteit opgeslagen, bewaart waarde en
   formaat/type, wordt genormaliseerd en case-onafhankelijk gevalideerd, en blijft uniek
   binnen de volledige catalogus, ook wanneer het gekoppelde product gearchiveerd is.
7. Producten, categorieen en eenheden ondersteunen soft delete met heractiveren;
   gearchiveerde records zijn standaard verborgen uit normale lijsten en keuzelijsten.
8. Een categorie of eenheid kan niet worden gearchiveerd zolang er nog actieve
   producten naar verwijzen.
9. Een gekoppelde code van een gearchiveerd product blijft aan dat product gekoppeld en
   behoudt zijn unieke claim ook zolang het product gearchiveerd is.
10. De gebruiker bereikt deze catalogusfuncties via een inventory-/voorraadbeheermenu
    met submenu-items `Producten`, `Categorieen` en `Eenheden`; productbeheer gebruikt
    een apart formulier/scherm en categorie-/eenheidbeheer gebruikt eenvoudige lijsten
    met simpele modals.
11. De catalogus werkt volledig lokaal/offline-first binnen de bestaande BootManager
    database.

**Legacy-impact**

- Dekt primair `US2.1` categorieen beheren, `US2.3` product aanmaken, `US2.4` product
  bewerken of verwijderen en het niet-scanende deel van `US2.5` barcodes koppelen aan
  producten.
- Levert een eerste invulling voor `US2.2` met een beperkte vaste iconenset, maar niet
  voor upload of een aparte iconbibliotheek.
- Levert voor `US2.3` bewust alleen de catalogusbasis: naam, categorie, eenheid,
  optionele omschrijving en een optionele gekoppelde code; minimumvoorraad,
  locatiekoppeling, merk/fabrikant, foto/label en voorraadgedrag blijven latere scope.
- Modelleert gekoppelde codes als aparte entiteit, maar beperkt de functionele UI-scope
  van deze story bewust tot maximaal een code per product.
- Laat `US2.6`, `US2.8`, `US2.9`, `US2.10`, `US2.11`, `US2.12`, `US2.13`, `US2.14`,
  `US2.19` en `US2.20` bewust open voor latere inventory-slices.

**Handmatige acceptatietest**

1. Log in als Owner of Crew.
2. Open de inventory-beheerpagina en controleer dat categorie-, eenheid- en
   productbeheer beschikbaar zijn.
3. Maak twee categorieen aan met verschillende iconen, bijvoorbeeld `Drinken` en
   `Onderdelen`, en bewerk daarna de omschrijving van een categorie.
4. Controleer dat een categorie met een dubbele naam wordt geblokkeerd.
5. Controleer dat de standaardset met eenheden zichtbaar is en voeg indien nodig een
   extra eenheid toe.
6. Start productaanmaak en maak desgewenst inline een nieuwe categorie of een nieuwe
   eenheid aan; rond daarna de productaanmaak af met naam, categorie, eenheid en
   optionele omschrijving.
7. Voeg tijdens of na productaanmaak een gekoppelde code toe via handmatige invoer of
   bestaande scanflow en controleer dat opslaan lukt.
8. Probeer dezelfde gekoppelde code aan een tweede product te koppelen; verwacht een
   duidelijke validatiefout of blokkade.
9. Ontkoppel de code weer en controleer dat het product zonder code kan blijven bestaan.
10. Archiveer een product en controleer dat het in de standaardlijst verdwijnt maar via
    archiefweergave terug te vinden en te heractiveren is.
11. Probeer een categorie of eenheid te archiveren terwijl er nog een actief product aan
    hangt; verwacht een blokkade.
12. Controleer dat `Producten`, `Categorieen` en `Eenheden` bereikbaar zijn via het
    inventory-/voorraadbeheermenu en dat categorie-/eenheidaanmaak vanuit het
    productformulier in een modal terugkeert naar dezelfde productflow.
