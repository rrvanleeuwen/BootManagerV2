# BootManager – Epic 8: Systeembeheer & Configuratie

Dit document bevat de volledige beschrijving van Epic 8 van het BootManager-project. De Epic behandelt het systeembeheer en de configuratie, waarmee de eigenaar of beheerder instellingen kan beheren zoals eenheden, taal, sensorkoppelingen, gebruikersrechten, back-up en synchronisatie.

## Doel

De eigenaar (of beheerder) moet het systeem kunnen instellen en configureren: eenheden, taal, sensorkoppelingen, gebruikersrechten, back-up- en synchronisatie-instellingen. Dit zorgt ervoor dat BootManager zich aanpast aan verschillende boten, gebruikers en gebruiksomstandigheden (offline of online).

## Belangrijkste functionaliteiten

- Instellingen voor eenheden (afstand, gewicht, volume, temperatuur, snelheid, druk)

- Taal- en regionale instellingen

- Gebruikersrechten (rolbeheer: eigenaar, bemanning, gast)

- Sensorintegratie (NMEA, GPS, barometer, motordata, tankniveaus, weerstation)

- Beheer van lokale en cloud-accounts

- Back-up en herstel van data

- Systeeminformatie (hardwarestatus, opslag, netwerk, Raspberry Pi-configuratie)

- Logboek van systeemacties en synchronisaties

- Offline modusbeheer

## User Stories + Acceptatiecriteria

### US8.1 – Instellingenpagina openen

Als eigenaar wil ik via het menu een centrale instellingenpagina kunnen openen, zodat ik systeemvoorkeuren kan beheren.

Given dat de gebruiker eigenaar is, When hij de instellingenpagina opent, Then toont BootManager alle configuratiecategorieën (eenheden, taal, sensoren, gebruikers, back-up, enz.).

### US8.2 – Eenheden configureren

Als eigenaar wil ik de gebruikte eenheden (liter, kilo, knopen, °C/°F, bar, NM) kunnen instellen, zodat metingen consistent worden weergegeven.

Given dat er standaardwaarden bestaan, When de gebruiker eenheden aanpast, Then past BootManager alle modules automatisch aan de nieuwe instellingen aan.

### US8.3 – Taal en regio instellen

Als gebruiker wil ik de taal van de interface en regionale voorkeuren (datum-/tijdnotatie) kunnen wijzigen, zodat de interface aansluit bij mijn voorkeur.

Given dat meerdere talen beschikbaar zijn, When de gebruiker een andere taal kiest, Then wordt de interface opnieuw geladen in de gekozen taal en opgeslagen als voorkeur.

### US8.4 – Gebruikersrollen beheren

Als eigenaar wil ik verschillende gebruikersrollen kunnen toekennen (bijv. eigenaar, bemanning, gast), zodat ik controle heb over wie welke functies mag gebruiken.

Given dat meerdere gebruikers zijn geregistreerd, When de eigenaar een rol wijzigt, Then worden de toegangsrechten voor die gebruiker direct bijgewerkt.

### US8.5 – Sensorintegratie configureren

Als eigenaar wil ik sensoren kunnen koppelen (GPS, NMEA, tankniveaus, motordata, barometer, windmeter), zodat BootManager automatisch gegevens ontvangt.

Given dat de boot beschikt over sensoren, When de gebruiker verbindingen toevoegt of test, Then worden deze opgeslagen en automatisch gebruikt in logboek en dashboard.

### US8.6 – Raspberry Pi-configuratie beheren

Als eigenaar wil ik systeeminformatie kunnen bekijken van de Raspberry Pi (CPU, geheugen, opslag, netwerk), zodat ik de status kan monitoren.

Given dat BootManager draait op een Pi, When de eigenaar de systeeminfo bekijkt, Then toont het systeem een overzicht met de actuele hardwarestatus.

### US8.7 – Gebruikersbeheer (toevoegen/verwijderen)

Als eigenaar wil ik gebruikers kunnen toevoegen of verwijderen, zodat ik de toegang tot het systeem kan beheren.

Given dat de eigenaar ingelogd is, When hij een nieuwe gebruiker toevoegt of verwijdert, Then worden de wijzigingen opgeslagen en doorgevoerd in de gebruikerslijst.

### US8.8 – Back-up maken en herstellen

Als eigenaar wil ik handmatig of automatisch een back-up kunnen maken van alle gegevens en deze kunnen herstellen, zodat ik geen data verlies.

Given dat BootManager lokaal data opslaat, When de gebruiker kiest voor back-up of herstel, Then wordt een exportbestand (.zip of .json) aangemaakt of ingelezen.

### US8.9 – Cloudinstellingen beheren

Als eigenaar wil ik mijn cloudaccount kunnen koppelen of ontkoppelen, zodat ik zelf bepaal of mijn data gesynchroniseerd wordt.

Given dat BootManager cloud-functionaliteit ondersteunt, When de gebruiker de koppeling wijzigt, Then worden de cloudinstellingen bijgewerkt en de synchronisatie-status weergegeven.

### US8.10 – Automatische synchronisatie plannen

Als eigenaar wil ik de frequentie van automatische synchronisatie kunnen instellen, zodat ik controle heb over netwerkverkeer en batterijgebruik.

Given dat cloud-synchronisatie actief is, When de gebruiker een tijdsinterval kiest, Then gebruikt BootManager dat interval voor automatische updates.

### US8.11 – Logboek van systeemacties bekijken

Als eigenaar wil ik een logboek kunnen zien van systeemacties (zoals back-ups, updates, synchronisaties), zodat ik inzicht heb in recente activiteiten.

Given dat systeemacties worden gelogd, When de gebruiker het systeemlogboek opent, Then toont BootManager datum, type actie, resultaat en eventuele foutmeldingen.

### US8.12 – Offline modus beheren

Als eigenaar wil ik handmatig de offline modus kunnen activeren of deactiveren, zodat ik controle heb over netwerkgebruik.

Given dat BootManager netwerktoegang heeft, When de gebruiker offline modus activeert, Then stopt BootManager alle automatische synchronisatieprocessen tot de modus wordt uitgeschakeld.

### US8.13 – Systeeminstellingen exporteren/importeren

Als eigenaar wil ik mijn configuratie kunnen exporteren of importeren, zodat ik dezelfde instellingen kan gebruiken op een ander apparaat of na herinstallatie.

Given dat instellingen beschikbaar zijn, When de gebruiker exporteert of importeert, Then maakt BootManager een configuratiebestand aan of leest het in.

### US8.14 – Standaardinstellingen herstellen

Als eigenaar wil ik alle instellingen kunnen terugzetten naar fabriekswaarden, zodat ik kan herbeginnen met een schone configuratie.

Given dat BootManager actief is, When de gebruiker kiest voor ‘Herstel standaardinstellingen’, Then worden alle configuraties gewist en teruggezet naar de standaardwaarden.
