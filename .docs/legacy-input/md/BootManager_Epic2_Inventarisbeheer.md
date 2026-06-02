# BootManager – Epic 2: Inventarisbeheer

Dit document bevat de volledige, definitieve user stories en acceptatiecriteria voor Epic 2 (Inventarisbeheer), conform het canvas.

## Doel

Gebruikers (eigenaar en bemanning) kunnen producten beheren, zien waar ze zich bevinden aan boord, bijhouden hoeveel voorraad beschikbaar is en zien wat moet worden aangevuld.

## Belangrijkste functionaliteiten

- Productcategorieën (voeding, onderdelen, gereedschap, zeilaccessoires, enz.)

- Aanmaken, wijzigen, verwijderen van producten

- Koppeling van producten aan opslaglocaties

- Automatische voorraadberekening en waarschuwingen bij lage voorraad

- Zoeken, filteren, sorteren op naam, type, locatie of categorie

- Handmatige of QR-gestuurde telling (scanner-modus)

- Export/import van voorraad

- Basislogboek voor wijzigingen

- Integratie met passage-planning (voorraadplanning bij reizen)

- Barcode/QR-ondersteuning en AI-herkenning

- Offline + (toekomst) cloud-synchronisatie

## User Stories + Acceptatiecriteria

**US2.1 – Categorieën beheren**

\*Als eigenaar wil ik productcategorieën kunnen aanmaken, bewerken en verwijderen, met een naam, korte omschrijving en icoontje, zodat ik de voorraad overzichtelijk en herkenbaar kan indelen.\*

Given dat de eigenaar is ingelogd,

When hij een nieuwe categorie aanmaakt of een bestaande bewerkt door een naam, omschrijving en icoontje te selecteren,

Then wordt de categorie opgeslagen in de lokale database en verschijnt ze met icoontje in de product- en filterlijsten.

**US2.2 – Categorie-icoontjes beheren**

\*Als eigenaar wil ik een set icoontjes kunnen koppelen aan categorieën, zodat ik visueel herkenbare symbolen kan kiezen of zelf toevoegen.\*

Given dat de eigenaar categorieën gebruikt,

When hij een icoon wil wijzigen of een nieuw icoon uploadt (vb. PNG of SVG),

Then wordt het icoon toegevoegd aan de bibliotheek en kan het worden geselecteerd bij categorieën.

**US2.3 – Product aanmaken**

\*Als eigenaar wil ik een nieuw product kunnen aanmaken met naam, omschrijving, categorie, eenheid (bijv. blik, kg, stuk), minimale voorraad, barcode(s), optioneel een foto en locatie, zodat ik alle productgegevens in één keer kan vastleggen.\*

Given dat de eigenaar de inventarispagina opent,

When hij op “Nieuw product” klikt en de velden invult (naam, omschrijving, categorie, eenheid, minimale voorraad, barcode, eventueel een foto en opslaglocatie),

Then wordt het product toegevoegd aan de database met alle ingevoerde gegevens, inclusief locatie, en is direct beschikbaar voor voorraadbeheer en scanning.

**US2.4 – Product bewerken of verwijderen**

\*Als eigenaar wil ik bestaande producten kunnen wijzigen of verwijderen, zodat de gegevens actueel blijven.\*

Given dat een product bestaat,

When de eigenaar de bewerk- of verwijderactie uitvoert,

Then worden wijzigingen opgeslagen of het product na bevestiging verwijderd.

**US2.5 – Barcodes en QR-codes koppelen aan producten**

\*Als eigenaar wil ik één of meerdere barcodes en QR-codes kunnen koppelen aan producten, zodat ik ze snel kan identificeren, opzoeken en varianten van hetzelfde product kan samenvoegen (bijv. verschillende merken van hagelslag).\*

Given dat een product bestaat,

When de eigenaar één of meerdere barcodes toevoegt of een QR-code genereert,

Then worden deze codes opgeslagen bij het product en kunnen ze gebruikt worden voor zoeken, voorraadbeheer en scanning.

**US2.6 – Barcode scannen bij zoeken**

\*Als bemanningslid of eigenaar wil ik een barcode kunnen scannen om een product snel te vinden, zodat ik niet handmatig hoef te zoeken.\*

Given dat producten barcodes hebben,

When de gebruiker een barcode scant via camera of scanner,

Then toont BootManager direct het overeenkomende productdetail.

**US2.7 – Barcodeherkenning via foto en AI**

\*Als bemanningslid of eigenaar wil ik een foto van een product kunnen maken, zodat BootManager de barcode uitleest of het product herkent met AI als de code niet leesbaar is.\*

Given dat een gebruiker een foto maakt van een product,

When BootManager de barcode niet kan scannen,

Then probeert het systeem met AI te herkennen wat het product is (bijv. merk, verpakking, tekst) en suggesties te tonen.

**US2.8 – Product koppelen aan opslaglocatie**

\*Als eigenaar wil ik een product aan een opslaglocatie kunnen koppelen, zodat ik weet waar het zich bevindt aan boord.\*

Given dat er producten en opslaglocaties bestaan,

When de eigenaar in het productdetail “Koppel locatie” kiest,

Then kan hij één of meerdere locaties selecteren en een hoeveelheid per locatie opgeven.

**US2.9 – Voorraad bekijken per locatie**

\*Als bemanningslid of eigenaar wil ik de producten kunnen zien die zich op een specifieke opslaglocatie bevinden, zodat ik snel iets kan terugvinden.\*

Given dat de bemanning of eigenaar een locatie selecteert (handmatig of via QR-scan),

When de detailpagina wordt geopend,

Then toont BootManager alle producten en hoeveelheden die daar zijn opgeslagen.

**US2.10 – Voorraad aanpassen (tellen of corrigeren)**

\*Als bemanningslid of eigenaar wil ik de actuele voorraad kunnen aanpassen, zodat de gegevens overeenkomen met de werkelijkheid.\*

Given dat een product aan een locatie is gekoppeld,

When de gebruiker een telling invoert of een correctie doet,

Then wordt de hoeveelheid bijgewerkt en wordt de wijziging gelogd in het voorraadlogboek.

**US2.11 – Minimumvoorraad & waarschuwing**

\*Als eigenaar wil ik een minimumvoorraad per product kunnen instellen, zodat ik een melding krijg wanneer iets bijna op is.\*

Given dat producten een minimumvoorraadwaarde hebben,

When de actuele hoeveelheid onder die waarde zakt,

Then toont BootManager een waarschuwing in de inventarislijst of dashboard.

**US2.12 – Zoeken en filteren**

\*Als bemanningslid of eigenaar wil ik producten kunnen zoeken en filteren, zodat ik snel iets vind.\*

Given dat de inventaris veel producten bevat,

When de gebruiker een zoekterm invoert of filters kiest,

Then toont BootManager enkel de relevante producten (met directe toegang tot de locatie).

**US2.13 – Voorraadlogboek**

\*Als eigenaar wil ik een logboek van voorraadwijzigingen kunnen bekijken, zodat ik zie wie wat heeft aangepast en wanneer.\*

Given dat er voorraadwijzigingen zijn geweest,

When de eigenaar het logboek opent,

Then toont het systeem datum, gebruiker, product, oude en nieuwe hoeveelheid.

**US2.14 – QR-scanner-modus**

\*Als bemanningslid of eigenaar wil ik een QR-scanner-modus kunnen gebruiken om snel voorraad te bekijken of te wijzigen, zodat telling aan boord eenvoudig is.\*

Given dat opslaglocaties een QR-code hebben,

When de gebruiker deze scant,

Then opent het systeem de voorraadlijst van die locatie en kan de gebruiker direct aantallen aanpassen.

**US2.15 – Bulkimport / export voorraad**

\*Als eigenaar wil ik mijn voorraad kunnen exporteren of importeren, zodat ik deze elders kan bewerken of een back-up kan maken.\*

Given dat er voorraaddata bestaat,

When de eigenaar kiest voor export of import,

Then wordt een .CSV / .JSON-bestand aangemaakt of ingelezen met alle producten, hoeveelheden en locaties.

**US2.16 – Voorraadstatus in dashboard**

\*Als bemanningslid of eigenaar wil ik een overzicht zien van totale voorraadwaarde en status, zodat ik snel inzicht krijg in de situatie aan boord.\*

Given dat de voorraad is geregistreerd,

When de gebruiker het dashboard opent,

Then toont het systeem een samenvatting per categorie, aantal producten onder minimum, en totale aantallen.

**US2.17 – Integratie met passage-planning**

\*Als eigenaar wil ik de geplande reis kunnen koppelen aan de voorraad, zodat ik weet of ik voldoende heb voor de duur van de tocht.\*

Given dat een passageplanning bestaat (duur, aantal personen),

When de eigenaar op “Bereken benodigdheden” klikt,

Then berekent BootManager wat nog moet worden aangeschaft en toont dit in een boodschappenlijst.

**US2.18 – Productfoto of label**

\*Als eigenaar wil ik een foto of label aan een product kunnen toevoegen, zodat het makkelijker te herkennen is.\*

Given dat een product bestaat,

When de eigenaar een foto uploadt of label invoert,

Then wordt deze opgeslagen en zichtbaar in de productdetails en lijsten.

**US2.19 – Voorraad automatisch ophogen bij nieuwe aankoop**

\*Als eigenaar wil ik bij het toevoegen van nieuwe producten aan de voorraad alleen de aangekochte hoeveelheid hoeven invoeren, zodat BootManager automatisch de bestaande voorraad ophoogt.\*

Given dat het product al in de voorraad bestaat,

When de eigenaar een nieuwe hoeveelheid invoert (bijv. “+1 kg” of “+3 stuks”),

Then verhoogt BootManager de bestaande voorraad automatisch met deze hoeveelheid en logt de wijziging in het voorraadlogboek.

**US2.20 – Voorraad verminderen bij verbruik via barcode**

\*Als bemanningslid of eigenaar wil ik door de barcode van een product te scannen kunnen aangeven hoeveel ik daarvan heb verbruikt, zodat BootManager automatisch de voorraad verlaagt met de juiste hoeveelheid.\*

Given dat er voorraad van een product aanwezig is,

When de gebruiker de barcode scant en invoert hoeveel van het product is verbruikt (bijv. “0,5 zak macaroni” of “1 fles water”),

Then vermindert BootManager automatisch de huidige voorraad met die hoeveelheid en logt het verbruik in het voorraadlogboek.

**US2.21 – Synchronisatie met cloud (toekomst)**

\*Als eigenaar wil ik mijn voorraad kunnen synchroniseren met de cloud, zodat ik ook buiten de boot toegang heb tot de gegevens.\*

Given dat cloud-synchronisatie is ingeschakeld,

When er wijzigingen worden gemaakt,

Then worden deze automatisch gesynchroniseerd zodra er verbinding beschikbaar is.

## Samenvatting van de Epic

| Categorie | Functionaliteit                                                      |
|-----------|----------------------------------------------------------------------|
| Beheer    | Producten, categorieën en locaties aanmaken, wijzigen en verwijderen |
| Structuur | Koppeling van producten aan opslaglocaties en categorieën            |
| Controle  | Voorraadniveaus, waarschuwingen, barcode/QR en AI-herkenning         |
| Toegang   | Offline beheer en (toekomst) cloud-synchronisatie                    |
| Overzicht | Zoeken, filteren, audit trail, export/import en passage-integratie   |
