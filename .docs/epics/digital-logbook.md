# Epic: Digitaal Logboek

**Datum:** 2026-05-23  
**Status:** Voorgesteld, klaar voor eerste implementatie-slice

---

## Aanleiding

BootManager verwerkt inmiddels echte YDEN-03 UDP-data en simulator-data voor de ondersteunde NMEA 0183 sentence-types. Daarmee is er genoeg betrouwbare meetdata om richting een eindgebruikersinterface te bewegen.

Het bestaande voorbeeldlogboek in `.docs/extraInfo/LogboekVoorbeeld.png` is leidend voor de eerste UI-richting. Dat logboek bestaat uit:

- een reis-header
- een reis-samenvatting
- chronologische logboekregels met tijd, navigatiegegevens, wind en opmerkingen

De UI moet geen technisch datadashboard worden als eerste stap. Het primaire doel is een logisch digitaal logboek voor een schipper/eindgebruiker.

---

## Functioneel Doel

BootManager toont automatisch verzamelde bootdata in een logboekvorm die lijkt op het bestaande papieren/PDF-logboek, met ruimte voor handmatige aanvulling door de gebruiker.

Automatische sensorwaarden blijven gekoppeld aan de onderliggende measurements. Handmatige invoer wordt apart opgeslagen en mag niet stilzwijgend door nieuwe sensorwaarden worden overschreven.

---

## Referentie: Voorbeeldlogboek

Bronbestand:

- `.docs/extraInfo/LogboekVoorbeeld.png`
- `.docs/extraInfo/VoorbeeldLogboek.pdf`

### Reis-header

Velden uit het voorbeeld:

- Reis
- Datum
- Boot
- Van
- Naar
- Bemanning

### Reis-samenvatting

Velden uit het voorbeeld:

- Vertrek
- Aankomst
- Logstand
- Gelogde mijlen (nm)
- Motor urenstand start
- Motor urenstand eind
- Brandstof (L)
- Totaal vaaruren

### Logboekregels

Kolommen uit het voorbeeld:

- Tijd
- Baro
- Log
- Koers
- Positie, zeilvoering, opmerkingen
- Bijlagen
- Wind
- GPS
- Lat.
- Long.

---

## Ontwerpkeuzes

### Uurregels als hoofdweergave

De hoofdweergave toont bij voorkeur een logboekregel per uur of per handmatig aangemaakt event. De gebruiker moet niet elke 10 seconden een invulregel krijgen.

### Detaildata blijft beschikbaar

Onder een uurregel kan de gebruiker details openen. Die detailweergave toont automatische samples binnen het tijdvak, bijvoorbeeld elke 10 seconden of elke minuut.

### Automatisch versus handmatig

Automatische waarden worden uit measurements voorgesteld. De gebruiker kan waarden overschrijven of aanvullen. Handmatige waarden blijven leidend zodra ze zijn ingevuld.

### Eerste versie zonder perfecte PDF-export

Print/PDF-layout is belangrijk, maar niet nodig voor de eerste implementatie-slice. Eerst moet het datamodel en de Blazor-pagina functioneel kloppen.

---

## User Stories

### Story 1 - Reis aanmaken en beheren

**Als** eigenaar  
**wil ik** een reis kunnen aanmaken met vertrek-, aankomst- en bootgegevens  
**zodat** logboekregels aan een duidelijke vaartocht gekoppeld zijn.

**Acceptatiecriteria**

- Gebruiker kan `Reis`, `Datum`, `Van`, `Naar`, `Boot` en `Bemanning` invullen.
- Gebruiker kan `Vertrek`, `Aankomst`, `Motoruren start/eind`, `Brandstof`, `Logstand` en `Gelogde mijlen` invullen.
- Reisgegevens worden opgeslagen en later opnieuw geladen.
- Een reis kan meerdere logboekregels bevatten.

### Story 2 - Logboekoverzicht per reis

**Als** gebruiker  
**wil ik** per reis een overzicht zien in tabelvorm zoals het voorbeeldlogboek  
**zodat** ik snel het verloop van de tocht kan lezen.

**Acceptatiecriteria**

- Pagina `/logbook` toont een geselecteerde reis.
- Tabel bevat kolommen: `Tijd`, `Baro`, `Log`, `Koers`, `Positie, zeilvoering, opmerkingen`, `Bijlagen`, `Wind`, `GPS`, `Lat.`, `Long.`
- Logregels staan chronologisch.
- De layout is compact en geschikt voor laptop/tablet.

### Story 3 - Automatische uurregels uit meetdata

**Als** gebruiker  
**wil ik** dat BootManager automatisch per uur een logboekregel voorstelt  
**zodat** het logboek niet volledig handmatig ingevuld hoeft te worden.

**Acceptatiecriteria**

- Voor elk uur met meetdata wordt een logboekregel gegenereerd of voorgesteld.
- Automatische velden gebruiken beschikbare measurements:
  - koers uit heading/motion
  - wind uit windmetingen
  - GPS/lat/long uit positie
  - snelheid/log indien beschikbaar
- Ontbrekende data blijft leeg.
- Gebruiker kan automatische waarden overschrijven.

### Story 4 - Handmatige logboeknotities

**Als** gebruiker  
**wil ik** per logboekregel opmerkingen en zeilvoering kunnen invullen  
**zodat** context wordt vastgelegd die sensoren niet meten.

**Acceptatiecriteria**

- Gebruiker kan tekst invoeren bij `Positie, zeilvoering, opmerkingen`.
- Gebruiker kan `Baro`, `Log`, `Koers`, `Wind`, `GPS`, `Lat.` en `Long.` handmatig aanpassen.
- Handmatige invoer blijft bewaard.
- Automatische data overschrijft handmatige invoer niet zonder bevestiging.

### Story 5 - Detailweergave per logboekregel

**Als** gebruiker  
**wil ik** een logboekregel kunnen openklappen  
**zodat** ik de onderliggende meetdata van dat uur kan bekijken.

**Acceptatiecriteria**

- Elke logboekregel kan worden opengeklapt.
- Detail toont samples binnen het tijdvak, bijvoorbeeld elke 10 seconden.
- Detail toont minimaal tijd, positie, COG/SOG, heading, wind, diepte en watertemperatuur indien beschikbaar.
- Detail toont ook een samenvatting per waarde: eerste, laatste, gemiddelde of laatste bekende waarde.

### Story 6 - Automatisch samenvatten per tijdvak

**Als** gebruiker  
**wil ik** dat BootManager per uur een compacte samenvatting maakt  
**zodat** het logboek leesbaar blijft ondanks veel sensorregels.

**Acceptatiecriteria**

- Logboekregel gebruikt representatieve waarden per tijdvak.
- Voor positie: begin/eind of laatste bekende positie.
- Voor wind/snelheid/koers: laatste bekende waarde of gemiddelde, duidelijk gekozen.
- Voor diepte/watertemperatuur: laatste bekende waarde.
- Samenvattingslogica is consistent en gedocumenteerd.

### Story 7 - Bijlagen bij logboekregels

**Als** gebruiker  
**wil ik** bijlagen kunnen koppelen aan een logboekregel  
**zodat** foto's, documenten of notities bij een moment in de tocht bewaard blijven.

**Acceptatiecriteria**

- Logboekregel heeft een `Bijlagen` veld.
- Gebruiker kan later een of meerdere bijlagen koppelen.
- In eerste versie mag dit een placeholder/teller zijn zonder uploadfunctionaliteit.
- UI laat zien of er bijlagen aanwezig zijn.

### Story 8 - Logboek print/PDF-layout

**Als** gebruiker  
**wil ik** het digitale logboek kunnen bekijken of exporteren in een layout die lijkt op het bestaande logboek  
**zodat** het bruikbaar blijft als officieel of persoonlijk vaartverslag.

**Acceptatiecriteria**

- Reis-header en samenvatting staan bovenaan.
- Logregels staan in tabelvorm met dezelfde kolommen als het voorbeeld.
- Layout is geschikt voor print/PDF.
- Niet alle detaildata wordt standaard geprint; detaildata is optioneel.

---

## Aanbevolen Eerste Implementatie-Slice

Start met Story 1, Story 2 en een eenvoudige basis van Story 3.

### Scope

- Nieuwe `LogbookTrip` entity voor de reisgegevens.
- Nieuwe `LogbookEntry` entity voor handmatige logboekregels per tijdvak.
- EF Core configuratie en migratie.
- Application-service voor aanmaken/ophalen/bijwerken van reizen en regels.
- Blazor-pagina `/logbook`.
- Navigatielink naar Logboek voor ingelogde eigenaar.
- Eerste automatische uurweergave op basis van bestaande measurements waar beschikbaar.

### Bewust buiten scope voor de eerste slice

- Bijlagen uploaden.
- Print/PDF-export.
- Volledige 10-seconden detailweergave.
- Volledige reis-samenvattingvelden uit het voorbeeldlogboek, zoals motoruren, brandstof, gelogde mijlen en totaal vaaruren.
- Automatische vulling uit bestaande measurements; de eerste foundation mag handmatige velden en placeholders leveren.
- Deduplicatie of conflictbeleid tussen meerdere meetbronnen.
- AIS-semantiek.

### Acceptatiecriteria Eerste Slice

- Gebruiker kan een reis aanmaken en opnieuw openen.
- Gebruiker ziet een logboektabel met de kolommen uit het voorbeeld.
- Gebruiker kan per regel opmerkingen/zeilvoering invullen en bewaren.
- Pagina is voorbereid om bestaande meetdata te gebruiken; daadwerkelijke automatische vulling mag in een vervolgslice.
- Ontbrekende meetdata veroorzaakt lege cellen, geen foutmelding.
- `dotnet build` slaagt.

---

## Databronnen Voor Automatische Velden

| UI-veld | Primaire bron | Opmerking |
|---------|---------------|-----------|
| Tijd | `LogbookEntry.StartUtc` of measurement-tijdvak | Hoofdregel per uur/event |
| Baro | Handmatig | Barometer-slice bestaat nog niet |
| Log | Handmatig of latere log/speed-integratie | Geen betrouwbare totale logstand beschikbaar |
| Koers | `HeadingMeasurement` of `MotionMeasurement.CourseOverGroundDegrees` | Keuze expliciet maken in service |
| Positie/opmerkingen | Handmatig, aangevuld met positie-samenvatting | Vrij tekstveld blijft centraal |
| Bijlagen | Placeholder in eerste slice | Upload later |
| Wind | `WindMeasurement` | Richting/snelheid compact tonen |
| GPS | `PositionMeasurement` aanwezig ja/nee of laatste fix | Eventueel "OK" wanneer positie beschikbaar is |
| Lat. | `PositionMeasurement.Latitude` | Laatste of begin/eind binnen tijdvak |
| Long. | `PositionMeasurement.Longitude` | Laatste of begin/eind binnen tijdvak |

