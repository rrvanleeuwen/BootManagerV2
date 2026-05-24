# Epic: Digitaal Logboek

**Datum:** 2026-05-24 (latest: Draft-suggesties herzien voor veiligheid)
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

Vanuit een logboekregel kan de gebruiker een aparte read-only detailpagina openen. Die detailweergave toont automatische samples binnen het tijdvak van de regel, zonder de compacte logboektabel te vergroten.

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

**Datum implementatie:** 2026-05-23

**Als** gebruiker  
**wil ik** de onderliggende meetdata van een logboekregel bekijken op een aparte read-only detailpagina  
**zodat** ik inzicht heb in wat er tijdens dat tijdvak is gemeten.

**Implementatiekeuzes**

- Aparte read-only detailpagina op route `/logbook/entries/{entryId:int}/details` (geen openklap in de tabel).
- Op /logbook staat bij elke logboekregel een "Details"-knop die naar de detailpagina navigeert.
- De detailpagina is volledig alleen-lezen; opgeslagen LogbookEntry-waarden worden niet gewijzigd.
- Periode-afbakening: start = EntryTimeUtc van de vorige logboekregel in dezelfde reis; als die er niet is: DepartureUtc van de reis; einde = EntryTimeUtc van de gekozen logboekregel.
- Als geen geldige startperiode bepaald kan worden, toont de pagina een nette lege weergave zonder fout.
- Data ná de gekozen logboektijd wordt nooit getoond.
- SOG wordt getoond in knopen.
- Diepte wordt getoond in meters.
- Alle tijden in de UI zijn lokale boordtijd (via BoordtijdHelper); intern blijft alles UTC.
- Samplestrategie: maximaal 50 records per meettype, gesorteerd op tijd. Bij meer dan 50 records wordt uniform gesampleld (elke N-de record).
- Application-service: `ILogbookEntryDetailService` / `LogbookEntryDetailService` in `BootManager.Application`.
- DTOs: `LogbookEntryDetailDto`, `LogbookDetailSummaryDto<T>` en meettype-specifieke sample-DTOs.

**Acceptatiecriteria**

- Elke logboekregel heeft een Details-knop die naar de detailpagina gaat.
- Detailpagina toont reisnaam, logboektijd (lokaal), en periode start/eind (lokaal).
- Detailpagina toont samenvatting (eerste, laatste, gemiddelde waar van toepassing) voor: positie, COG/SOG, heading, wind, diepte, watertemperatuur.
- Sampletabellen tonen beschikbare meetrecords binnen het tijdvak (max 50).
- Ontbrekende meettypen geven geen crash, maar tonen "Geen data".
- "Terug naar logboek"-knop navigeert terug naar /logbook.
- `dotnet build` slaagt.

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


---

## Story 8 — Printvriendelijke Weergave (2026-05-23)

### Status
Geïmplementeerd als browser-printvriendelijke HTML/CSS weergave.

### Wat is gebouwd
- Printpagina op /logbook/trips/{tripId}/print (LogbookPrint.razor).
- Printpagina gebruikt een eigen PrintLayout zonder app-menu/topbar, zodat alleen logboekinhoud wordt geprint.
- Toont reis-header (reis, datum, boot, van, naar, bemanning), reis-samenvatting (vertrek, aankomst, logstand start, gelogde mijlen, motor uren start/eind, brandstof, totaal vaaruren) en logboekregels in compacte tabel.
- Knop 🖨 Afdrukweergave op /logbook bij geselecteerde reis (opent in nieuw tabblad).
- Terug naar logboek knop en Afdrukken knop (roept window.print() aan).
- Terug/afdruk-knoppen worden niet afgedrukt via @media print.
- Layout A4 landscape via @page CSS.
- Lokale boordtijd via BoordtijdHelper; intern UTC ongewijzigd.
- Ontbrekende velden tonen leeg of —, geen crash.

### Bewust buiten scope
- Server-side PDF-generatie (geen externe PDF-library).
- Print/PDF loopt via browser print.
- Detaildata (/logbook/entries/{id}/details) wordt niet standaard geprint.
- Bijlagen uploaden.

---

## Slice: Akkoordflow (LogbookEntryStatus)

**Datum:** 2026-05-23  
**Branch:** feature/logbook-entry-status

### Wijzigingen
- `LogbookEntryStatus` enum (`Draft`, `Confirmed`) toegevoegd in `BootManager.Core.Enums`.
- `LogbookEntry` entiteit uitgebreid met `Status` property (default `Confirmed`) en `Confirm()` methode.
- `LogbookEntryDto` uitgebreid met `Status`.
- `ILogbookService` uitgebreid met `ConfirmEntryAsync(int entryId)`.
- EF Core configuratie bijgewerkt; migratie `AddLogbookEntryStatus` toegevoegd (database default = 1 = Confirmed).
- `/logbook`: statuskolom met badge "Te accorderen" (oranje) of "Definitief" (groen); Draft-rijen lichtgeel gemarkeerd; knop "✓ Accorderen" zichtbaar voor Draft-regels.
- `/logbook/trips/{id}/print`: filtert automatisch conceptregels uit — alleen Confirmed regels worden afgedrukt.

### Productregels vastgelegd
- Alleen `Confirmed`/Definitief regels staan in de printweergave (officieel document).
- `Draft`/Te accorderen regels blijven zichtbaar in de werk-UI (`/logbook`).
- Nieuwe handmatige regels zijn standaard `Confirmed`.
- Bestaande regels krijgen na migratie database-default `Confirmed`.

### Vervolgstappen (buiten scope deze slice)
- Browser notifications bij overschreden loginterval.
- Automatisch aanmaken van `Draft`-regels via intervaldetectie.
- Gebruikersinstelling voor loginterval.

---

## Volgende slice: ontbrekende logmomenten zichtbaar maken

**Datum:** 2026-05-23  
**Status:** Voorstel voor eerstvolgende logboekstap

### Wat bestaat al
- `Draft`/`Confirmed` akkoordflow voor logboekregels.
- Badges "Te accorderen" en "Definitief" in `/logbook`.
- Draft-regels zijn zichtbaar in de werk-UI en worden niet geprint.
- Handmatige nieuwe regels zijn standaard `Confirmed`.
- Meetdatasuggesties bestaan al voor een gekozen logboektijdstip.
- Detailperiode loopt al van vorige logboekregel (of reisvertrek) tot de gekozen regel.

### Gewenste eerstvolgende stap

Maak nog geen volledige browser-notificatie of push notification. Begin met zichtbaar maken dat een logmoment ontbreekt.

Functioneel voorstel:
- Bepaal per geselecteerde/actieve reis het volgende verwachte logmoment op basis van:
  - laatste logboekregel, of
  - vertrek/reisstart als er nog geen regels zijn,
  - plus een voorlopig vast of later instelbaar loginterval.
- Als dat moment verstreken is, toon boven het logboek een duidelijke melding/banner.
- Toon een knop: "Conceptregel maken voor dit logmoment".
- De knop maakt een `Draft`/Te accorderen logboekregel aan voor dat tijdstip.
- De gebruiker vult opmerkingen/zeilvoering aan en accordeert bewust.
- Print blijft ongewijzigd: alleen `Confirmed` regels worden afgedrukt.

### Bewuste scopegrens
- Nog geen browser push notifications.
- Nog geen automatisch definitief maken.
- Nog geen stille bulk-aanmaak van meerdere regels zonder gebruiker.
- Eventueel ontbrekende meerdere logmomenten mogen eerst als lijst of één eerstvolgend logmoment worden ontworpen.

---

## Slice: Correctie van Draft-regelsuggesties (2026-05-24)

**Datum:** 2026-05-24
**Branch:** feature/logbook-missing-moments
**Status:** Geïmplementeerd

### Probleem
In de vorige implementatie kregen Draft-regels voor gemiste logmomenten "laatst bekende" meetwaarden die van veel eerder in de reis konden stammen. Dit was misleidend: als een gebruiker een gemist logmoment om 18:08 zag met koerswaarde van 090°, kon die waarde afkomstig zijn van 12:58 (geen metingen tussen 17:08–18:08). De detailtabellen waren leeg (geen samples in het logtijdvak), maar de overzichtswaarden suggereerden verkeerd dat er actuele data was.

### Domeinregel
**Draft-regels voor gemiste logmomenten gebruiken ALLEEN meetdata BINNEN het bijbehorende logtijdvak.**

- **Logtijdvak:** van vorige logboekregel (of `DepartureUtc` als geen vorige) tot `EntryTimeUtc` van de nieuwe Draft-regel.
- **Punt-in-tijd velden (Course, WindDescription, GpsStatus, Latitude, Longitude):** alleen metingen BINNEN het logtijdvak; voorkeur = meest recente beschikbare.
- **Periode-velden (AverageSogKnots):** gemiddeld over het logtijdvak (blijft ongewijzigd).
- **Geen data = leeg veld:** als geen metingen van het type BINNEN het logtijdvak, blijft het veld leeg.
- **Handmatige regels:** behouden het oude "laatst bekende vóór of op logmoment" gedrag voor gebruikergemak.

### Implementatie

#### 1. Service-contract uitgebreid
`ILogbookMeasurementSuggestionService.GetSuggestionsAsync(..., bool onlyPeriodData = false, ...)`

- `onlyPeriodData=true`: Draft-regels, alleen metingen IN het logtijdvak.
- `onlyPeriodData=false` (default): handmatige regels, "laatst bekende vóór of op logmoment" gedrag.

#### 2. Suggestie-logica herzien
`LogbookMeasurementSuggestionService.GetSuggestionsAsync()` nu met twee paden:

**Draft-pad (`onlyPeriodData=true`):**
- **Course:** Zoekt Heading/MotionMeasurement met `RecordedAtUtc >= periodStart && RecordedAtUtc <= entryTimeUtc`. Voorkeur: laatste Heading, fallback: Motion/COG. Geen data = null.
- **WindDescription:** Zoekt WindMeasurement in periode, voorkeur = meest recent. Geen data = null.
- **GpsStatus, Latitude, Longitude:** Zoekt PositionMeasurement in periode, voorkeur = meest recent. Geen data = null.
- **AverageSogKnots:** Berekend over MotionMeasurements in periode (ongewijzigd).

**Handmatig-pad (`onlyPeriodData=false`):**
- Course, WindDescription, Positie: "laatst bekende vóór of op logmoment" (origineel gedrag).

#### 3. Draft-aanmaak
`LogbookService.CreateDraftEntryAsync()` roept nu:
```
await _suggestionService.GetSuggestionsAsync(tripId, entryTimeUtc, onlyPeriodData: true, ...)
```

#### 4. Detailpagina
De detailweergave blijft ongewijzigd en werkt correct:
- Toont de opgeslagen logwaarden (die nu vaker null zijn voor lege Draft-regels).
- Periode-sampletabellen tonen alleen data IN het logtijdvak (blijft consistent).
- Als geen samples beschikbaar: "Geen data" (nu correct).

### Acceptatie
- ✓ `dotnet build` slaagt.
- ✓ Draft-regel voor gemist logmoment zonder metingen in het logtijdvak: alle automatische velden leeg.
- ✓ Draft-regel met metingen in logtijdvak: velden gevuld uit die metingen alleen.
- ✓ Handmatige regels: ongewijzigd Confirmed, kunnen nog suggesties ophalen (met oude "laatst bekende" semantiek).
- ✓ Detailpagina: toont correcte waarden, geen oude bronmetingen als automatische onderbouwing.
- ✓ Print: ongewijzigd, alleen Confirmed.

### Rationale
Veiligheid en controleerbaarheid: een lege Draft-regel is beter dan een regel met oude waarden. De gebruiker kan dan bewust kiezen of deze handmatig in te vullen op basis van andere bronnen (papieren logboek, GPS, etc.). De domeinregel zorgt voor consistentie: als de detailtabel leeg is (geen meetdata in periode), mag de overzichtsregel ook geen "oude" waarden tonen.
