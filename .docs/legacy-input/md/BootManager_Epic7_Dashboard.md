# BootManager – Epic 7: Dashboard & Overzicht

Dit document bevat de volledige beschrijving van Epic 7 van het BootManager-project. De Epic behandelt het dashboard en overzicht waarmee gebruikers in één oogopslag de actuele status van de boot kunnen zien, inclusief voorraad, onderhoud, passages, documenten, logboek en waarschuwingen.

## Doel

Gebruikers (vooral de eigenaar en bemanning) zien in één oogopslag de actuele status van de boot, inclusief voorraad, onderhoud, geplande passage, logboekactiviteit, waarschuwingen en weersinformatie. Het dashboard dient als centrale startpagina en beslissingsondersteuning voor het dagelijkse bootbeheer.

## Belangrijkste functionaliteiten

- Overzicht van bootstatus (actieve boot, locatie, datum, tijd, stroomvoorziening, netwerkstatus)

- Widget-gebaseerde secties: Voorraad, Passage, Onderhoud, Documenten, Logboek, Weer

- Dynamische waarschuwingen (lage voorraad, bijna verlopen documenten, gepland onderhoud)

- Navigatie naar de bijbehorende modules (klik-door op widgets)

- Personalisatie: eigenaar kiest welke widgets zichtbaar zijn

- Automatische updates via lokale data of sensoren (NMEA / GPS)

- Offline weergave van laatste bekende data

- Synchronisatie met cloud zodra internet beschikbaar is

## User Stories + Acceptatiecriteria

### US7.1 – Dashboardweergave openen

Als bemanningslid of eigenaar wil ik een dashboard zien met een samenvatting van de belangrijkste gegevens, zodat ik direct inzicht heb in de status van de boot.

Given dat de gebruiker is ingelogd, When het dashboard wordt geopend, Then toont BootManager actuele samenvattingen van voorraad, onderhoud, passages en waarschuwingen.

### US7.2 – Actieve bootinformatie

Als eigenaar wil ik bovenaan het dashboard de actieve boot zien met naam, type, foto en locatie, zodat ik weet voor welk schip ik werk.

Given dat meerdere boten bestaan, When de gebruiker het dashboard opent, Then toont BootManager de actieve bootinformatie met eventueel de mogelijkheid om van boot te wisselen.

### US7.3 – Waarschuwingen en meldingen

Als gebruiker wil ik waarschuwingen zien over lage voorraad, bijna verlopen documenten of gepland onderhoud, zodat ik direct actie kan ondernemen.

Given dat er waarschuwingen aanwezig zijn, When het dashboard wordt geopend, Then verschijnen deze in een meldingenpaneel met type, urgentie en link naar de betreffende module.

### US7.4 – Weerinformatie en getijden

Als gebruiker wil ik de actuele weer- en getijdeninformatie zien, zodat ik navigatie- en ankerbeslissingen kan nemen.

Given dat er internet of lokale sensordata beschikbaar zijn, When het dashboard wordt geladen, Then toont BootManager windrichting, windsnelheid, luchtdruk, temperatuur en (optioneel) getijdeninformatie.

### US7.5 – Widget voor voorraadstatus

Als bemanningslid of eigenaar wil ik een overzicht van de voorraad per categorie zien, zodat ik weet of er iets moet worden aangevuld.

Given dat voorraaddata bestaat, When de gebruiker het dashboard bekijkt, Then toont BootManager per categorie het aantal producten onder de minimumvoorraad.

### US7.6 – Widget voor onderhoudsstatus

Als eigenaar wil ik de status van onderhoudstaken zien (gepland, bijna vervallen, uitgevoerd), zodat ik onderhoud kan plannen.

Given dat onderhoudstaken bestaan, When het dashboard wordt geopend, Then toont BootManager een samenvatting met kleurenindicaties per status.

### US7.7 – Widget voor documentstatus

Als eigenaar wil ik waarschuwingen zien voor documenten met vervaldatum, zodat ik op tijd kan verlengen of vervangen.

Given dat er documenten zijn met vervaldatum, When een document bijna verloopt, Then toont BootManager dit in het dashboard met een icoon en resterende dagen.

### US7.8 – Widget voor passageplanning

Als eigenaar wil ik een korte samenvatting zien van de actuele of komende passage, zodat ik voorbereid kan vertrekken.

Given dat er een geplande passage bestaat, When het dashboard wordt geopend, Then toont BootManager vertrek- en aankomstlocatie, bemanning, verwachte duur en benodigdhedenstatus.

### US7.9 – Widget voor logboekactiviteit

Als gebruiker wil ik de laatst geregistreerde logboekvermeldingen zien, zodat ik weet wat er recent is gebeurd.

Given dat logboekdata bestaat, When het dashboard wordt geladen, Then toont BootManager de laatste logregels met tijd en samenvatting.

### US7.10 – Personaliseren van widgets

Als eigenaar wil ik kunnen kiezen welke widgets op het dashboard getoond worden en in welke volgorde, zodat het aansluit bij mijn gebruik.

Given dat meerdere widgets beschikbaar zijn, When de gebruiker instellingen wijzigt, Then wordt de lay-out opgeslagen en bij volgende sessies toegepast.

### US7.11 – Interactieve navigatie

Als gebruiker wil ik op een widget kunnen klikken om naar de corresponderende module te gaan, zodat ik snel kan doorklikken naar details.

Given dat een widget gegevens toont, When de gebruiker erop klikt, Then opent BootManager de bijbehorende pagina (bijv. Voorraad of Onderhoud).

### US7.12 – Offline weergave

Als gebruiker wil ik het dashboard ook offline kunnen gebruiken, zodat ik aan boord altijd toegang heb tot de laatste bekende gegevens.

Given dat BootManager offline is, When het dashboard wordt geopend, Then toont het de laatst gesynchroniseerde data met een melding dat de informatie mogelijk verouderd is.

### US7.13 – Automatische update van gegevens

Als gebruiker wil ik dat het dashboard automatisch wordt bijgewerkt bij wijzigingen in andere modules, zodat ik altijd actuele informatie zie.

Given dat data verandert (bijv. voorraad of onderhoud), When BootManager actief is, Then ververst het dashboard automatisch de bijbehorende widgets.

### US7.14 – Cloudsynchronisatie

Als eigenaar wil ik dat het dashboard automatisch wordt bijgewerkt zodra er internet beschikbaar is, zodat ik ook op afstand de actuele status zie.

Given dat cloud-synchronisatie is ingeschakeld, When BootManager verbinding maakt met internet, Then worden alle dashboardgegevens bijgewerkt met de meest recente informatie uit de cloud.
