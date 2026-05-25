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

---

## Slice: Verbeterde Missing Moments en Delete-functionaliteit (2026-05-24)

**Datum:** 2026-05-24
**Branch:** feature/logbook-missed-moments-list
**Status:** Geïmplementeerd

### Aanleiding
In de vorige implementatie toonde de banner slechts het eerstvolgende gemiste logmoment. Voor langere reizen zonder regelmatige logboekinvoer kunnen meerdere logmomenten achterlopen, waardoor het nuttig is een overzicht van meerdere gemiste momenten en gecontroleerde batch-aanmaak van conceptregels toe te voegen.

Daarnaast ontbrak de mogelijkheid om logboekregels te verwijderen, wat nuttig is voor corrigeren van fouten.

### Functionaliteit: Gemiste Logmomenten Overzicht

#### 1. Service-methoden
`ILogbookService` uitgebreid met:

**`GetMissedLogMomentsAsync(int tripId)`**
- Retourneert alle logmomenten die verstreken zijn sinds de vorige logboekregel (of DepartureUtc) + LogIntervalMinutes.
- Berekent tot en met `DateTime.UtcNow`.
- Retourneert `MissedLogMomentsDto` met `TotalCount` en geordende lijst van `MissedMomentDto`.

**`CreateMultipleDraftEntriesAsync(int tripId, int maxCount = 24)`**
- Maakt Draft-regels aan voor de N eerste gemiste logmomenten (max 24).
- Defensief: voorkomt duplicaten door te controleren of een regel voor dat moment al bestaat.
- Herberekent automatisch na de batch.

**`DeleteEntryAsync(int entryId)`**
- Hard-delete van logboekregel.
- Werkt voor zowel Draft als Confirmed regels.

#### 2. DTO's
Twee nieuwe DTO's in `BootManager.Application.Logbook.DTOs`:

- `MissedMomentDto`: één gemist moment (`EntryTimeUtc`).
- `MissedLogMomentsDto`: totaal aantal en lijst van momenten.

#### 3. UI-banner
`/logbook` toont verbeterde banner:

- **Telling:** "⚠ Gemiste logmomenten: N"
- **Compacte tijdlijst:** eerste 5 momenten in lokale boordtijd, of "HH:mm, HH:mm, ..." format.
- **Overflow:** "+ M meer" als meer dan 5 gemiste momenten bestaan.
- **Knop:** "Conceptregels aanmaken" (in plaats van "Conceptregel maken" voor enkelvoudig).
- **Sluit banner:** mogelijk via ✕, maar herbereken gebeurt na regel-aanmaak.

#### 4. Bulk-aanmaak
- Klik op "Conceptregels aanmaken" maakt tot 24 Draft-regels aan in één beurt.
- Na aanmaak herbereken banner automatisch.
- Als meer dan 24 gemist zijn, blijven de resterende momenten zichtbaar na herberekening.
- Geen enkel moment resulteert in automatische Confirmed; alle blijven Draft.

#### 5. Verwijderingsknop
Elke logboekregel in `/logbook` krijgt verwijderknop (🗑):

- Positie: in actiecel naast Details, Bewerken, Accorderen.
- Bevestigingsdialoog: "Weet u zeker dat u de logboekregel van HH:mm wilt verwijderen?"
- Na verwijdering: regel verwijderd uit database en UI, banner herberekend.
- Werkt voor Draft en Confirmed.

#### 6. Migratie
Geen nieuwe database-migratie nodig; existing `LogbookEntry` tabel ondersteunt al hard-delete.

### Acceptatiecriteria
- ✓ `dotnet build` slaagt.
- ✓ Als een reis 1 gemist logmoment heeft, toont banner dat moment en kan daar één Draft-regel voor worden aangemaakt.
- ✓ Als meerdere gemiste logmomenten bestaan, toont banner telling + compacte tijdlijst (max 5 zichtbaar + "+N meer").
- ✓ "Conceptregels aanmaken" maakt maximaal 24 Draft-regels aan per klik.
- ✓ Resterende gemiste momenten (>24) blijven zichtbaar na herberekening.
- ✓ Geen dubbele regels voor hetzelfde logboektijdstip.
- ✓ Draft-regels gebruiken veilige periode-data (ongewijzigd van vorige slice).
- ✓ Na aanmaak blijft `/logbook` lijsten in nieuw→oud sortering.
- ✓ Printweergave blijft ongewijzigd: Confirmed-only, oud→nieuw sortering.
- ✓ Handmatige "+ Nieuwe regel" maakt nog steeds Confirmed-regels.
- ✓ Elke regel heeft verwijderknop met bevestigingsdialoog.
- ✓ Na verwijdering herbereken banner.
- ✓ Verwijdering werkt voor Draft en Confirmed.
- ✓ Print toont verwijderde Confirmed-regels niet meer (automatisch doordat hard-delete uit database is).

### Implementatiedetails
- **Berekening gemiste momenten:** basis = `EntryTimeUtc` van laatste regel of `DepartureUtc` van reis. Interval = `trip.LogIntervalMinutes` (fallback 60 als ongeldig). Loop tot alle momenten < `DateTime.UtcNow` zijn berekend.
- **Defensive checks:** Bij bulk-aanmaak controleren of regel voor dat moment al bestaat, om races bij gelijktijdige requests te voorkomen.
- **Delete**: `EfRepository.DeleteAsync` is aangepast zodat een al getrackte entity met dezelfde primary key wordt gebruikt bij verwijderen. Dit voorkomt EF Core tracking-conflicten in Blazor Server-scenario's.
- **Logging:** Service logt alle operaties (aanmaken, verwijderen, fouten).
- **UI-state:** Verwijderknop en modal-overlay blijven eenvoudig (Bootstrap 5 classes, geen externe libraries).

---

## Slice: Detailpagina Herontwerp als Accordeerhulpmiddel (2026-05-24)

**Datum:** 2026-05-24
**Branch:** feature/logbook-detail-approval-view
**Status:** Geïmplementeerd

### Aanleiding
De bestaande detailpagina (`/logbook/entries/{entryId:int}/details`) was ontworpen als read-only samenvattingsscherm met oude bronmetingen die vóór het logtijdvak lagen. Dit conflicteerde met de nieuwe domeinregel dat Draft-regels voor gemiste logmomenten **alleen meetdata binnen hun eigen logtijdvak** gebruiken, geen oude "laatst bekende" waarden.

De pagina is nu herontworpen als een **accordeerhulpmiddel** voor gebruikers om snel te bepalen:
1. Of een logregel compleet is ingevuld.
2. Welke meetdata beschikbaar is in het logtijdvak.
3. Of handmatige aanvulling nodig is voordat accordering kan plaatsvinden.

### Functionaliteit

#### 1. Context-header (compact)
Bovenaan de pagina:
- Terug-knop naar `/logbook`
- Titel: "Logboekregel accordering"
- Reisnaam
- Status badge: "Concept" (geel) of "Akkoord" (groen)
- Logboektijd (lokaal, dd-MM-yyyy HH:mm)
- **Logtijdvak (lokaal):** bereik getoond als "HH:mm — HH:mm" (lokaal boordtijd)

#### 2. Waarschuwingen
Twee waarschuwingen kunnen verschijnen:

**Geen geldige startperiode:**
Getoond als alert wanneer `PeriodStartUtc` null is. Tekst: "Waarschuwing: Geen geldige startperiode beschikbaar. Controleer de logboekgegevens."

**Geen meetdata in logtijdvak:**
Getoond als alert-info wanneer `PeriodStartUtc` beschikbaar is maar alle samplecounts (Positie, Beweging, Heading, Wind, Diepte, WaterTemperatuur) gelijk zijn aan 0. Tekst: "Geen meetdata in dit logtijdvak. Vul deze logregel handmatig aan voordat u deze accordeert."

#### 3. Logregelwaarden (vervangen "Opgeslagen logregelwaarden")
Nieuwe compacte layout toont **alle** velden inclusief lege waarden:

- **Barometer:** waarde of "Nog niet ingevuld"
- **Log:** waarde of "Nog niet ingevuld"
- **Koers:** waarde of "Nog niet ingevuld"
- **Gem. SOG:** waarde of "Nog niet ingevuld"
- **Wind:** waarde of "Nog niet ingevuld"
- **GPS-status:** waarde of "Nog niet ingevuld"
- **Breedtegraad:** waarde of "Nog niet ingevuld"
- **Lengtegraad:** waarde of "Nog niet ingevuld"
- **Opmerkingen:** enkel als ingevuld

Doel: gebruiker ziet onmiddellijk wat er is ingevuld en wat nog ontbreekt.

#### 4. Meetdata in dit logtijdvak (overzicht per type)
Compacte 6-kolom grid met samplecounts:

- **Positie-samples:** aantal
- **COG/SOG-samples:** aantal
- **Heading-samples:** aantal
- **Wind-samples:** aantal
- **Diepte-samples:** aantal
- **Watertemperatuur-samples:** aantal

Tekst en getal grote font voor snelle scan.

#### 5. Sampletabellen (secundair)
Onder kopje "Samples", alleen getoond als samples beschikbaar zijn:

Per meettype een tabel (als `Samples.Count > 0`):
- **Positie:** Tijd (lokaal), Lat., Long.
- **COG/SOG:** Tijd (lokaal), COG, SOG
- **Heading:** Tijd (lokaal), Heading
- **Wind:** Tijd (lokaal), Hoek, Snelheid
- **Diepte:** Tijd (lokaal), Diepte
- **Watertemperatuur:** Tijd (lokaal), Temperatuur

#### 6. Verwijderde elementen
**Bronmetingen voor automatische suggesties** sectie volledig verwijderd. Dit betrof `CourseBron`, `WindBron`, `PositieBron` DTOs en bijbehorende service-logica:
- `LogbookSourceMeasurementDto` klasse
- `LogbookEntryDetailDto.CourseBron`, `.WindBron`, `.PositieBron` properties
- Service-methodes: `BepaalCourseSourceAsync()`, `BepaalWindSourceAsync()`, `BepaalPositieSourceAsync()`

Reden: Deze toonden uitsluitend punt-in-tijd metingen vóór/op het logmoment, veelal buiten het logtijdvak. De nieuwe domeinregel schrijft voor dat Draft-regels **geen** waarden buiten hun eigen logtijdvak kunnen gebruiken.

### Implementatiewijzigingen

#### DTO Cleanup
**BootManager.Application/Logbook/DTOs/LogbookEntryDetailDto.cs:**
- Verwijderde properties: `CourseBron`, `WindBron`, `PositieBron`
- Verwijderde klasse: `LogbookSourceMeasurementDto`
- Behouden: `SavedValues`, `PeriodStartUtc`, `PeriodEndUtc`, sample-samenvattingen (`Positie`, `Beweging`, `Heading`, `Wind`, `Diepte`, `WaterTemperatuur`)

#### Service Cleanup
**BootManager.Application/Logbook/Services/LogbookEntryDetailService.cs:**
- Verwijderde private methods: `BepaalCourseSourceAsync()`, `BepaalWindSourceAsync()`, `BepaalPositieSourceAsync()`
- `GetEntryDetailAsync()` veroorzaakt geen aanroepen meer naar deze methodes; de DTO-properties kunnen niet meer worden gevuld

#### UI Herontwerp
**BootManager.Web/Components/Pages/LogbookEntryDetails.razor:**
- Gecompleteerde layout: context-header, waarschuwingen, logregelwaarden, meetdata-overzicht, sampletabellen
- Helper-methods:
  - `HasNoMeasurementData(LogbookEntryDetailDto detail)` – controleer of alle samplecounts 0 zijn
  - `GetSampleCount<T>(LogbookDetailSummaryDto<T>? summary)` – retourneer samplecount of 0
- Geen verwijzingen meer naar `CourseBron`, `WindBron` of `PositieBron`

### Acceptatiecriteria
- ✓ `dotnet build` slaagt.
- ✓ Detailpagina toont context-header met reisnaam, logboektijd, logtijdvak lokaal.
- ✓ Detailpagina toont status badge (Concept/Akkoord).
- ✓ Waarschuwing "Geen geldige startperiode" verschijnt wanneer `PeriodStartUtc` null is.
- ✓ Waarschuwing "Geen meetdata in dit logtijdvak" verschijnt wanneer alle samplecounts 0 zijn.
- ✓ Logregelwaarden sectie toont alle velden (Barometer, Log, Koers, Gem. SOG, Wind, GPS-status, Breedtegraad, Lengtegraad, Opmerkingen).
- ✓ Lege velden getoond als "Nog niet ingevuld" of "-".
- ✓ Meetdata-overzicht toont samplecounts per meettype in 6-kolom grid.
- ✓ Sampletabellen enkel getoond als samples beschikbaar zijn.
- ✓ Geen referenties meer naar `CourseBron`, `WindBron`, `PositieBron` in DTO, Service of UI.
- ✓ Geen "Bronmetingen voor automatische suggesties" sectie meer zichtbaar.
- ✓ Terug-knop en koppelingen naar `/logbook` blijven werkend.
- ✓ Print-weergave ongewijzigd.
- ✓ Delete-functionaliteit ongewijzigd.
- ✓ Draft-aanmaaklogica ongewijzigd; nieuwe regels gebruiken nog steeds alleen periode-data.

### Rationale
Dit herontwerp verduidelijkt de rol van de detailpagina: ondersteuning bij accordering door:
1. Snel inzicht in wat is ingevuld (waarden-sectie).
2. Duidelijke indicatie of meetdata beschikbaar is (overzicht + waarschuwing).
3. Toegang tot onderliggende samples voor verificatie.

De verwijdering van oude bronmetingen versterkt consistentie: als het logtijdvak geen meetdata bevat, mogen opgeslagen waarden niet worden onderbouwd met metingen buiten het vak.

---

## Slice 6: Bijlagen per Logboekregel

**Datum:** 2026-05-24
**Status:** Geïmplementeerd

### Aanleiding

Een schipper wil tijdens/na een reis notities, foto's, documenten of handgeschreven schetsen kunnen koppelen aan individuele logboekregels. Deze bijlagen moeten:
- Configureerbaar worden opgeslagen (voor flexibiliteit op Raspberry Pi / Docker persistent volumes)
- Via logboek-detailpagina beheerd kunnen worden (upload/download/delete voor concept- en definitieve regels)
- Vanuit `/logbook` direct kunnen worden toegevoegd via een compacte uploadmodal
- In `/logbook` als teller zichtbaar zijn

### Functioneel Doel

Gebruiker kan per logboekregel bijlagen toevoegen en beheren, onafhankelijk van de akkoordstatus van de regel. Bijlagen tellen verschijnen in de logboek-lijstweergave.

### Implementatie

#### Core Entity
**BootManager.Core/Entities/LogbookAttachment.cs**
- `Id` (int, PK)
- `LogbookEntryId` (int, FK → LogbookEntry)
- `OriginalFileName` (string, bestandsnaam zoals geüpload)
- `StoredFileName` (string, gegenereerd bestandsnaam met GUID ter voorkoming van conflicten)
- `ContentType` (string, MIME-type)
- `SizeBytes` (long, bestandsgrootte)
- `UploadedAtUtc` (DateTime, systeemtijd)
- `Description` (string?, optionele omschrijving/type van de bijlage)
- Navigation `Entry` → LogbookEntry

#### EF Core Mapping
**BootManager.Infrastructure/Persistence/Configurations/LogbookAttachmentConfiguration.cs**
- Table `LogbookAttachments`
- Cascade delete op `LogbookEntry`
- Unieke index op `LogbookEntryId` niet nodig; 1:many-verhouding toestaat meerdere bijlagen per entry

#### Operationele Instellingen
**BootManager.Core/Entities/OperationalSettings.cs**
- Nieuw veld: `LogbookAttachmentsDirectory` (default: `"data/logbook-attachments"`)
- Ondersteunt absoluut pad (bijv. `/mnt/data/attachments` op Raspberry Pi) en relatief pad

**BootManager.Application/OperationalSettings/DTOs/OperationalSettingsDto.cs**
- Nieuw veld: `LogbookAttachmentsDirectory` met `[Required]` en `[MaxLength(1024)]`

#### Application Service
**BootManager.Application/Logbook/Services/ILogbookAttachmentService.cs**
- `UploadAsync(entryId, stream, fileName, contentType, description, ct)` → AttachmentUploadResultDto
- `DownloadAsync(attachmentId, ct)` → (stream, fileName, contentType)
- `DeleteAsync(attachmentId, ct)` → bool (true = verwijderd)
- `GetAttachmentsAsync(entryId, ct)` → IEnumerable<LogbookAttachmentDto>
- `GetAttachmentCountAsync(entryId, ct)` → int

**BootManager.Application/Logbook/Services/LogbookAttachmentService.cs**
- Implementatie met:
  - 10 MB bestand-limiet
  - Allowlist-filtering op bestandstype (PDF, Office, afbeeldingen, tekst)
  - Gegenereerde opslag-bestandsnaam via GUID + extensie ter voorkoming van padtraversalfouten
  - Path-safety checks via `Path.GetFullPath()`
  - Logging op fouten
  - Bij delete: probeert fysieke bestand te verwijderen, verwijdert altijd metadatarecord uit database

#### DTOs
**BootManager.Application/Logbook/DTOs/LogbookAttachmentDto.cs**
- `Id`, `OriginalFileName`, `ContentType`, `SizeBytes`, `UploadedAtUtc`, `Description`
- `FormattedSize` (computed property met human-readable grootte)

**BootManager.Application/Logbook/DTOs/AttachmentUploadResultDto.cs**
- Enum `AttachmentUploadStatus` (Success, FileTooLarge, InvalidFileType, StorageError, DatabaseError, UnknownError)
- Velden: `Status`, `Message`, `Attachment`
- Computed property `Success` (true als Status = Success)
- Statische factories: `SuccessResult(attachment)`, `Error(status, message)`

#### Web API Controller
**BootManager.Web/Controllers/LogbookAttachmentsController.cs**
- `POST api/LogbookAttachments/upload/{entryId:int}` → AttachmentUploadResultDto
- `GET api/LogbookAttachments/download/{attachmentId:int}` → File (stream)
- `DELETE api/LogbookAttachments/{attachmentId:int}` → 204 NoContent of 404
- `GET api/LogbookAttachments/entry/{entryId:int}` → IEnumerable<LogbookAttachmentDto>
- `GET api/LogbookAttachments/count/{entryId:int}` → int

#### Blazor UI Wijzigingen

**BootManager.Web/Components/Pages/Logbook.razor**
- Logboektabel: kolom "Bijlagen" toont badge met aantal bijlagen (als > 0)
- Per logboekregel is er een compacte uploadknop die een modal opent.
- Uploadmodal bevat bestandselectie en optionele omschrijving/type.
- Na succesvolle upload wordt de bijlagenteller direct bijgewerkt.

**BootManager.Web/Components/Pages/LogbookEntryDetails.razor**
- Sectie "Bijlagen" staat direct bovenaan na de context-header.
- Toont lijst met bijlagen (naam, omschrijving, grootte, datum)
- Downloaden per bijlage (via client-side download helper)
- Delete-knop (met bevestiging) is beschikbaar voor concept- en definitieve regels.
- Upload-formulier (InputFile + omschrijving + Upload-knop) is beschikbaar voor concept- en definitieve regels.
- Feedback-berichten voor upload-fouten en -succes

**BootManager.Web/Components/Pages/LogbookPrint.razor**
- Printweergave toont geen bijlagenkolom. Bijlagenbeheer hoort bij de werk-UI en detailpagina, niet bij de compacte printlayout.

**BootManager.Web/wwwroot/app.js**
- Helper `downloadFileFromStream()` om bijlagen client-side af te downloaden

#### Logbook Service Aanpassingen
**BootManager.Application/Logbook/Services/LogbookService.cs**
- Injectie van `ILogbookAttachmentService`
- `GetEntriesAsync()`: async mapping via `MapEntryAsync()` die `AttachmentCount` populeer
- `CreateEntryAsync()`, `CreateDraftEntryAsync()`: gebruiken `MapEntryAsync()`

**BootManager.Application/Logbook/DTOs/LogbookEntryDto.cs**
- Nieuw veld: `AttachmentCount` (int)

#### Database Migratie
- Tabel `LogbookAttachments` met kolommen en foreign key
- `OperationalSettings.LogbookAttachmentsDirectory` kolom (nvarchar, 1024)

#### DI Registratie
**BootManager.Application/DependencyInjection.cs**
- Registreer `ILogbookAttachmentService, LogbookAttachmentService` als scoped

#### Settings UI
**BootManager.Web/Components/Pages/Settings.razor**
- Nieuwe sectie "Logboekbijlagen" onder "Operationele instellingen"
- Input-veld voor `LogbookAttachmentsDirectory` met hulptext

### Acceptatiecriteria

- ✓ `dotnet build` slaagt.
- ✓ EF Core migratie maakt tabel `LogbookAttachments` en veld in `OperationalSettings` aan.
- ✓ Bijlagen kunnen worden geüpload via detailpagina.
- ✓ Bijlagen kunnen rechtstreeks vanuit het logboekoverzicht via modal worden geüpload.
- ✓ Bijlagen verschijnen in downloadbare lijst op detailpagina.
- ✓ Bijlagen kunnen per ID worden gedownload (met originele bestandsnaam).
- ✓ Bijlagen kunnen (met bevestiging) worden verwijderd.
- ✓ Bijlageomschrijving/type wordt opgeslagen en getoond.
- ✓ Bijlagen-teller verschijnt in logboek-lijstweergave als > 0.
- ✓ Printweergave bevat geen bijlagenkolom.
- ✓ Upload-limit (10 MB) wordt afgedwongen.
- ✓ Bestandstypering wordt afgedwongen (PDF, Office, afbeeldingen, tekst).
- ✓ Opslag-directory kan via settings worden ingesteld.
- ✓ Fysieke bestanden worden veilig opgeslagen (gegenereerde naam, geen padtraversal).
- ✓ Verwijdering van bestand en metadata werken correct.
- ✓ Logboekregels kunnen conform oude logica worden aangemaakt/bewerkt (attachment-slice is orthogonaal).
- ✓ Persistentie werkt op Linux/Docker volumekoppelingen (relatieve paden toegestaan).

### Notities voor Implementatie en Deployment

#### Opslag Directory Setup
- **Lokale ontwikkeling:** standaard `data/logbook-attachments` wordt automatisch aangemaakt bij eerste upload
- **Raspberry Pi / Docker:** mount persistent volume, bijvoorbeeld:
  ```bash
  docker run -v /mnt/external/logbook-attachments:/app/data/logbook-attachments \
             -e "OperationalSettings__LogbookAttachmentsDirectory=/app/data/logbook-attachments" \
             bootmanager-web
  ```
- **appsettings.json fallback:** kan nodig zijn voor initiële boot; zie OperationalSettingsService

#### Filesystem Rechten
- Container/Linux-user moet schrijfrechten hebben op geselecteerde directory
- Bestanden worden per upload aangemaakt; geen periodieke cleanup geïmplementeerd (toekomstig)

#### Backwards Compatibility
- Oude logboeken zonder bijlagen blijven ongewijzigd werken
- `AttachmentCount` in `LogbookEntryDto` defaultt naar 0 als geen bijlagen aanwezig zijn
- Bestaande bijlagen zonder omschrijving blijven geldig.

#### Verwijderen van logboekregels met bijlagen
- Bij verwijderen van een volledige logboekregel verwijdert EF de gekoppelde bijlage-records via cascade delete.
- Vooraf worden de veilige bestandspaden van gekoppelde bijlagen verzameld.
- De logboekregel en bijlage-metadata worden binnen een database-transactie verwijderd.
- Fysieke bestanden worden pas na succesvolle database-commit opgeruimd.
- Als de databaseverwijdering faalt, blijven fysieke bestanden behouden.
- Als fysieke cleanup na commit faalt, wordt dit gelogd zonder de databaseverwijdering terug te draaien.

### Rationale
Deze slice voegt een praktisch feature toe zonder logica voor Draft/Confirmed of missed-logmoment te wijzigen. Bijlagen zijn orthogonaal aan bestaande logboek-workflows en voegen alleen UI en API-layer toe. De configureerbaarheid zorgt voor flexibiliteit op embedded systemen waar storage-paden kunnen verschillen per deployment.

---

## Slice 6b: Bijlagen-UI Verbetering (2026-05-24)

**Status:** UI/UX Geoptimaliseerd
**Wijzigingen:** Minimaal, alleen UI-laag

### Aanpassingen

#### Logbook.razor - Bijlagen-kolom
- Bij `AttachmentCount == 0`: toon "—"
- Bij `AttachmentCount > 0`: toon compacte klikbare badge "📎 N" (paperclip + aantal)
- Badge navigeert naar `/logbook/entries/{entryId:int}/details` voor bijlagenbeheer
- Naast de badge staat een uploadknop per regel voor direct toevoegen via modal.
- Geen bestandsnamen in /logbook tabel; compact blijven

#### Logbook.razor - Uploadmodal
- Modal bevat bestandselectie en optionele omschrijving/type.
- Upload gebruikt `ILogbookAttachmentService.UploadAsync()` direct vanuit Blazor Server, niet `HttpClient`.
- Bij succes sluit de modal en wordt de teller in de tabel ververst.
- Bij fout blijft de modal open en toont de foutmelding.

#### LogbookEntryDetails.razor - Bijlagen-sectie
- **Plaatsing:** Direct bovenaan na de context-header, zodat de gebruiker niet langs meetdata hoeft te scrollen.
- **Upload:** Altijd beschikbaar (niet meer beperkt tot Draft-regels)
- **Omschrijving/type:** Optioneel tekstveld bij upload; bestaande bijlagen tonen omschrijving wanneer ingevuld.
- **Delete:** Altijd beschikbaar (niet meer beperkt tot Draft-regels)
- **Download:** Ongewijzigd, voor alle bijlagen
- **Helpertekst:** Toegevoegd: "Bijlagen kunnen ook na accorderen nog worden toegevoegd of verwijderd."
- **Entry status:** Upload/delete wijzigen entry-status niet (orthogonale operaties)

### Acceptatiecriteria

- ✓ `/logbook` toont "—" bij 0 bijlagen.
- ✓ `/logbook` toont badge "📎 N" bij N > 0.
- ✓ Badge is klikbare link naar detailpagina.
- ✓ Per regel kan direct een bijlage worden toegevoegd via uploadmodal.
- ✓ Detailpagina toont upload-formulier altijd (Draft en Confirmed).
- ✓ Detailpagina toont bijlagenblok bovenaan.
- ✓ Detailpagina toont delete-knoppen altijd (Draft en Confirmed).
- ✓ Omschrijving/type wordt opgeslagen en getoond.
- ✓ Download werkt voor alle bijlagen.
- ✓ Upload/delete wijzigt entry-status niet.
- ✓ Geen bestandsnamen in `/logbook` tabel.
- ✓ Helpertekst verduidelijkt management ook na accordering.
- ✓ Printweergave heeft geen bijlagenkolom.
- ✓ Missing-moments flow ongewijzigd.
- ✓ Draft/Confirmed akkoordflow ongewijzigd.

### Rationale
Deze UI-verbetering maakt bijlagenbeheer intuïtiever en toegankelijker:
1. **Logbook tabel:** Compact en begrijpelijk; badge signaleert aanwezigheid van bijlagen.
2. **Detailpagina:** Volledige controle over bijlagen, onabhankelijk van entry-status (accordering beperkt entry-wijzigingen, niet bijlagenbeheer).
3. **Helpertekst:** Verduidelijkt gebruiker dat bijlagen altijd kunnen worden beheerd.

---

## Slice 7: Responsieve UI & Filterfunctionaliteit (2026-05-24)

**Status:** Gerealiseerd
**Wijzigingen:** Filters client-side, responsive layout voor mobiel/tablet

### Problematiek
- Logboektabel heeft veel kolommen, waardoor pagina op mobiel/tablet slecht bruikbaar wordt
- Bij lange reizen met veel logmomenten is het moeilijk snel bepaalde regels te vinden
- Gebruiker moet horizontaal scrollen of zwaar zoomen op mobiel

### Aanpassingen

#### Logbook.razor - Filters
Compacte filterbalk boven de logboektabel met:
- **Statusfilter:** Alle / Concept / Definitief
- **Bijlagenfilter:** Alle / Met bijlagen / Zonder bijlagen
- **Zoekveld:** Doorzoekt Remarks, WindDescription en GpsStatus client-side
- **Datumfilter:** Optioneel; desktop inline (md-breakpoint), mobiel via modal
- **Reset-knop:** Stelt alle filters terug naar standaard

**Implementatie:**
- Filters zijn volledig client-side (`_statusFilter`, `_attachmentFilter`, `_searchFilter`, `_dateFilterStart`, `_dateFilterEnd`)
- Gefilterde entries via `GetFilteredAndSortedEntries()` methode
- Sortering: nieuw → oud (descending) behouden
- Filter UI wordt weergegeven in compacte card boven de tabel
- State-management: filters persisteren zolang pagina geopend is (niet sessie-opslag)

#### Logbook.razor - Responsive Layout
- **Desktop (≥992px):** Tabelweergave, alle kolommen zichtbaar, filterbalk compact
- **Tablet/Mobiel (<992px):**
  - Tabel verborgen via CSS (`display: none`)
  - Alternatieve kaart-weergave aktief
  - Per logboekregel een aparte kaart met:
    - **Header:** Tijd (lokaal), statusbadge, bijlagenbadge
    - **Body:** Opmerkingen prominent, technische waarden in responsive grid (Baro, Log, Koers, Wind, GPS, Lat, Long, Gem. SOG)
    - **Acties:** Details, Bewerken, Accorderen (als concept), Verwijderen, Bijlage toevoegen
  - Acties niet overlappend, tekst niet buiten containers
  - Compact padding/margins op <576px

#### Logbook.razor.css - Responsive Styles
- CSS media queries voor breakpoints: xs (<576px), sm (≥576px), md (≥768px), lg (≥992px)
- Tabelweergave dynamisch tonen/verbergen
- Card-grid layouts voor data-velden
- Responsieve buttongroepen met flex-wrap
- Small-screen optimalisaties: kleinere tekst, compact padding, responsive input-breedtes

#### Bestaande Flows
- Nieuwe regel toevoegen: UI aangepast per breakpoint (tabel vs. kaart)
- Bewerken: In-line editing in tabel (desktop), kaart-formulier (mobiel)
- Accorderen: Knop beschikbaar in tabel en kaart
- Verwijderen: Bevestigingsdialoog werkt op beide layouts
- Bijlage toevoegen: Modal ongewijzigd, werkend op desktop en mobiel
- Gemiste logmomenten: Banner behouden, acties zichtbaar op beide layouts
- Detailpagina: Ongewijzigd, navigatie werkt via Details-knop in kaart en tabel

#### Printweergave
- Ongewijzigd; `/logbook/trips/{tripId}/print` blijft compact tabelformat

### Acceptatiecriteria

- ✓ `dotnet build` slaagt zonder fouten
- ✓ `git diff --check` geeft geen whitespace errors
- ✓ **Desktop (≥992px):** Bestaande tabelweergave werkend, filters compact boven tabel
- ✓ **Tablet/Mobiel (<992px):** Kaart-weergave zichtbaar, tabel verborgen
- ✓ **Filters werken client-side:**
  - Status filter (Alle/Concept/Definitief) filtert correct
  - Bijlage filter (Alle/Met/Zonder) filtert correct
  - Zoekfilter (opmerkingen, wind, GPS) werkt case-insensitively
  - Datumfilter (optioneel) filtert per dag-range
- ✓ **Sortering:** Nieuw → oud behouden na filteren
- ✓ **Responsive tekst/buttons:** Geen overlaps, geen horizontaal scrollen op mobiel als primaire workflow
- ✓ **Bestaande acties beschikbaar:** Details, Bewerken, Accorderen, Verwijderen, Bijlage toevoegen op desktop en mobiel
- ✓ **Flows ongewijzigd:** Nieuwe regel, bewerken, accorderen, verwijderen, bijlage toevoegen, details openen, gemiste logmomenten allen werkend
- ✓ **Printweergave:** Ongewijzigd
- ✓ **Bijlagendetails:** Ongewijzigd

### Rationale
Deze slice verbetert gebruikbaarheid op mobiel/tablet aanzienlijk:
1. **Filters:** Gebruiker kan snel relevante regels vinden zonder lange scroll
2. **Responsive layout:** Geen compulsief zoomen/horizontaal scrollen op kleine schermen
3. **Client-side filters:** Snelle interactie zonder server round-trips
4. **Consistent design:** Desktop tabel behouden voor power-users, mobiel-optimized kaarten voor onderweg

---

## Slice 8: Mobile/Tablet Infinite Scroll en Dynamische Paging

**Datum:** 2026-05-24
**Status:** Geïmplementeerd

### Aanleiding

De kaart-weergave voor mobiel/tablet kon alle gefilterde logboekregels tegelijk renderen, wat voor langere reizen (>100 regels) tot vertragen op kleine apparaten kon leiden. Dit slice implementeert **dynamische paging** met infinite-scroll-trigger op mobiel/tablet, terwijl desktop-tabel ongewijzigd blijft.

### Functionaliteit

#### Card-Paging State
**BootManager.Web/Components/Pages/Logbook.razor** uitgebreid met paging-state:
- `_displayedEntries` (List<LogbookEntryDto>): zichtbare kaarten in het huidige venster
- `_cardPageSize` (int): 8 items per pagina (aanpasbaar)
- `_cardPageIndex` (int): huidig pagina-index (0-based)
- `_hasMoreEntries` (bool): zijn er meer gefilterde entries beschikbaar?
- `_isLoadingMore` (bool): wordt momenteel volgende batch geladen?

#### Reset bij Filter-Verandering
Nieuwe helper-methode `ResetCardPaging()` stelt paging-state terug:
- `_cardPageIndex = 0`
- `_displayedEntries.Clear()`
- Laadt eerste batch via `LoadMoreCards()`
- Aangeroepen vanuit alle filter-change handlers: `OnStatusFilterChanged()`, `OnAttachmentFilterChanged()`, `OnSearchFilterChanged()`, `OnDateFilterStartChanged()`, `OnDateFilterEndChanged()`
- Ook aangeroepen in: `SelecteerReis()`, `OpslaanNieuweRegel()`, `BevestigVerwijdering()`, `MaakMultipleDraftRegels()`

#### Load-More Methode
**`LoadMoreCards()`** (private):
- Haalt volgende `_cardPageSize` items uit gefilterde en gesorteerde entries
- Voegt toe aan `_displayedEntries`
- Increment `_cardPageIndex`
- Update `_hasMoreEntries` op basis van totale gefilterde count
- Defensive: check `_isLoadingMore` om dubbele loads te voorkomen

#### Filter Change Handlers
Vijf nieuwe event handlers vervangen combinatie van `@bind` + `@onchange`:
- `OnStatusFilterChanged(ChangeEventArgs e)`
- `OnAttachmentFilterChanged(ChangeEventArgs e)`
- `OnSearchFilterChanged(ChangeEventArgs e)`
- `OnDateFilterStartChanged(ChangeEventArgs e)`
- `OnDateFilterEndChanged(ChangeEventArgs e)`

Elk handler:
- Parset waarde uit `ChangeEventArgs`
- Update filter state (`_statusFilter`, enz.)
- Roept `ResetCardPaging()` aan

Rationale: Blazor laat `@bind` en `@onchange` niet samen toe op hetzelfde element. Expliciete handlers geven meer controle en voorkomen binding-conflicten.

#### Infinite-Scroll Trigger
**Nieuwe JavaScript-module:** `BootManager.Web/Components/Pages/logbook-infinite-scroll.js`

Exports `initInfiniteScroll()`:
- Zoekt HTML-element `#cardScrollContainer` (de kaart-container)
- Monitort met `IntersectionObserver` de "Meer laden" knop
- Wanneer knop in viewport komt (100px vóór het scrollen): auto-click
- Callback aan Blazor niet nodig; JavaScript luistert naar native HTML-events

Gebruikersvoordeel: Scrollen naar beneden laadt automatisch meer kaarten, zonder handmatige knopklik (optioneel gebruikersgemak).

#### Blazor Component Changes

**Markup-wijzigingen:**
- Filter-inputs/selects gewijzigd van `@bind="_var" @onchange="() => ResetCardPaging()"` naar:
  ```razor
  @onchange="OnStatusFilterChanged" value="@_statusFilter"
  ```
  (voor select; vergelijkbaar voor input-velden)
- Datepicker-inputs: `@onchange="OnDateFilterStartChanged" value="@_dateFilterStart?.ToString("yyyy-MM-dd")"`
- Kaart-weergave: loopt over `_displayedEntries` in plaats van volledige gefilterde lijst
- "Meer laden" knop: zichtbaar als `_hasMoreEntries`; disabled tijdens `_isLoadingMore`

**Code-behind:**
- Injectie van `IJSRuntime` toegevoegd
- Velden `_infiniteScrollModule` (IJSObjectReference?) en `_infiniteScrollController` (dynamic?)
- Methode `OnAfterRenderAsync(bool firstRender)` importeert JS-module en initialiseert infinite scroll
- IAsyncDisposable interface geïmplementeerd: `DisposeAsync()` ruimt JS-resources op

#### Desktop Ongewijzigd
- Tabelweergave toont **alle** gefilterde entries in één tabel
- Geen paging op desktop (≥992px breakpoint)
- Filter-reset werkt ook op desktop; paging-state wordt beheerd maar niet zichtbaar

#### Printweergave Ongewijzigd
- `/logbook/trips/{tripId}/print` toont alleen `Confirmed` entries, ongewijzigd

### Implementatiedetails

#### Card Markup (Blazor)
```razor
<div class="logbook-cards-wrapper px-2 py-2" id="cardScrollContainer">
  @foreach (var entry in _displayedEntries)
  {
    <div class="logbook-card">
      <!-- Header: Tijd, Status, Bijlage-teller -->
      <!-- Body: Opmerkingen, technische velden -->
      <!-- Acties: Details, Bewerken, Accorderen, Verwijderen, Bijlage -->
    </div>
  }
</div>

@if (_hasMoreEntries)
{
  <button class="btn btn-sm btn-outline-primary"
          @onclick="LoadMoreCards"
          disabled="@_isLoadingMore">
    @if (_isLoadingMore)
    {
      <span class="spinner-border spinner-border-sm me-2"></span>Laden...
    }
    else
    {
      <span>Meer laden</span>
    }
  </button>
}
```

#### CSS (@media query)
```css
/* Mobiel/tablet (<992px): kaarten zichtbaar, paging actief */
@media (max-width: 991px) {
  .logbook-cards-wrapper {
    display: block;
  }
  .logbook-card {
    /* styled */
  }
}

/* Desktop (≥992px): tabel zichtbaar, kaarten verborgen */
@media (min-width: 992px) {
  .logbook-cards-wrapper {
    display: none;
  }
  .logbook-table-wrapper {
    display: block;
  }
}
```

#### JavaScript (`logbook-infinite-scroll.js`)
```javascript
let observer;
let container;
let loadMoreBtn;

export function initInfiniteScroll() {
  container = document.getElementById('cardScrollContainer');
  if (!container) return;

  observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting && loadMoreBtn && !loadMoreBtn.disabled) {
        loadMoreBtn.click();
      }
    });
  }, {
    root: null,
    rootMargin: '100px',
    threshold: 0.01
  });

  refreshInfiniteScroll();
}

export function refreshInfiniteScroll() {
  if (!observer || !container) return;
  if (loadMoreBtn) {
    observer.unobserve(loadMoreBtn);
  }

  loadMoreBtn = findLoadMoreButton();
  if (loadMoreBtn) {
    observer.observe(loadMoreBtn);
  }
}

export function disconnectInfiniteScroll() {
  observer?.disconnect();
  observer = undefined;
  container = undefined;
  loadMoreBtn = undefined;
}
```

### Acceptatiecriteria

- ✓ `dotnet build` slaagt zonder fouten
- ✓ `git diff --check` geeft geen whitespace errors
- ✓ **Paging state:** `_displayedEntries`, `_cardPageSize`, `_hasMoreEntries`, `_cardPageIndex` geïnitialiseerd
- ✓ **Filter handlers:** `OnStatusFilterChanged`, `OnAttachmentFilterChanged`, `OnSearchFilterChanged`, `OnDateFilterStartChanged`, `OnDateFilterEndChanged` werken zonder `@bind`/`@onchange` conflicten
- ✓ **Reset paging:** Filter-verandering triggert `ResetCardPaging()` → kaarten worden opnieuw geladen van pagina 0
- ✓ **Lazy loading:** Bij laadtijd tonen slechts eerste `_cardPageSize` (8) kaarten
- ✓ **"Meer laden" knop:** Zichtbaar als meer entries beschikbaar; disabled tijdens laadtijd
- ✓ **Infinite scroll:** JavaScript-module laadt automatisch volgende batch wanneer knop in zicht komt
- ✓ **Mobiel/tablet (<992px):** Kaarten zichtbaar, paging werkend
- ✓ **Desktop (≥992px):** Tabel zichtbaar, paging niet zichtbaar (alle entries in tabel)
- ✓ **Filters werken:** Gefilterde entries correct in paging opgenomen
- ✓ **Sortering:** Nieuw → oud behouden binnen pagina's
- ✓ **Bijlagen, accordering, verwijdering:** Acties beschikbaar per kaart
- ✓ **Details openen:** Navigatie naar detailpagina werkt
- ✓ **Printweergave:** Ongewijzigd, alleen Confirmed entries
- ✓ **Gemiste logmomenten banner:** Werkt met nieuwe paging-state
- ✓ **IAsyncDisposable cleanup:** JavaScript-resources correct opgeruimd

### Rationale

1. **Performantie:** Kleine apparaten renderen slechts 8 kaarten tegelijk → snellere DOM-updates, minder geheugengebruik
2. **UX:** Infinite scroll geeft seamless scrolling-ervaring; gebruiker voelt zich niet "tot X items per pagina beperkt"
3. **Desktop ongewijzigd:** Power-users op laptop kunnen alle regels in één tabel zien
4. **Client-side filters:** Gefilterde lijsten dynamisch opnieuw gepagineerd zonder server round-trip
5. **Blazor binding fix:** Expliciete handlers vermijden RZ10008 compile-fouten

---

## Slice 8b: Responsive Logbook Hardening (2026-05-24)

**Status:** Geïmplementeerd

### Aanpassingen
- Desktop en mobiel tonen nu een aparte lege-state als actieve filters geen regels opleveren.
- Mobiele kaartweergave ondersteunt bestaande regels bewerken met een inline kaartformulier.
- Card-paging wordt opnieuw opgebouwd na bewerken, accorderen en bijlage-upload, zodat filters en bijlagentellers actueel blijven.
- `AttachmentCount` blijft behouden bij lokale DTO-updates na bewerken of accorderen.
- Infinite-scroll interop gebruikt expliciete modulefuncties (`initInfiniteScroll`, `refreshInfiniteScroll`, `disconnectInfiniteScroll`) in plaats van een dynamisch teruggegeven controller-object.
- De observer wordt na paging-wijzigingen opnieuw gekoppeld aan de actuele "Meer laden"-knop.

### Acceptatiecriteria
- ✓ `dotnet build` slaagt.
- ✓ `git diff --check` geeft geen whitespace errors.
- ✓ Kaartweergave blijft consistent na filteren, bewerken, accorderen, uploaden en verwijderen.
- ✓ Infinite-scroll cleanup loopt via de JS-module en laat geen controller-state achter in .NET.

---

## Slice 9: Confirm-knop op Detailpagina (2026-05-25)

**Status:** Geïmplementeerd

### Doel
Gebruikers kunnen een Draft-logboekregel direct accorderen vanuit de detailpagina (`/logbook/entries/{entryId:int}/details`), zonder terug naar het overzicht te hoeven gaan.

### Aanpassingen
- **Detailpagina context-header:** "✓ Accorderen" knop toegevoegd rechts naast statusbadge.
  - Zichtbaar: alleen voor Draft-regels.
  - Disabled: voor Confirmed-regels.
- **Knopgedrag:**
  - Loading-state: spinner + "Bezig met accorderen..." tijdens de operatie.
  - Foutafhandeling: InvalidOperationException en andere fouten tonen in rode alert boven context-header.
  - Gebruiker: gebruiker blijft op detailpagina na succes.
- **Status-update:**
  - Na bevestiging roept `ILogbookService.ConfirmEntryAsync` op.
  - Detail herlaadt automatisch via `ILogbookEntryDetailService.GetEntryDetailAsync`.
  - Statusbadge wisselt van "Concept" naar "Akkoord"; knop verdwijnt.
- **Service:**
  - Geen wijzigingen; `ConfirmEntryAsync` bestond al.
  - Component injekt `ILogbookService` voor de bevestiging.
- **Printweergave:**
  - Ongewijzigd: Confirmed-only filtering blijft intact in `/logbook/trips/{tripId:int}/print`.

### Acceptatiecriteria
- ✓ Draft-detailpagina toont "✓ Accorderen"-knop in context-header.
- ✓ Confirmed-detailpagina toont geen knop.
- ✓ Bevestigen werkt: roept `ConfirmEntryAsync`, status wijzigt, pagina vernieuwt.
- ✓ Foutafhandeling: meldingen in rode alert, geen crash.
- ✓ Gebruiker blijft op pagina met bijgewerkte status.
- ✓ Printweergave ongewijzigd.
- ✓ `dotnet build` slaagt.

### Technische Details
- **File:** `BootManager.Web\Components\Pages\LogbookEntryDetails.razor`
  - Geïnjectioneerd: `ILogbookService LogbookService`.
  - State: `_confirming` (bool), `_confirmationError` (string?).
  - Method: `OnConfirmEntry()` – aanroepen service, herladen detail, fout afhandelen.

---

## Technische Schuld & Toekomstige Slices

### Voor Verdere Optimalisatie
- Virtual scrolling op extreem lange lijsten (>1000 entries in één reis)
- Serverside filters en paging als client-side filters bottleneck wordt
- Caching van gefilterde resultaten per trip
- Responsive grid-layouts voor tabellen (bijv. Griddle of Syncfusion)

### Potentiële Features
- Batch-acties (selecteer meerdere kaarten, accordeer/verwijder tegelijk)
- Drag-and-drop reordering van regels (voor nooddocumenten)
- In-kaart-bewerking in plaats van modal (prototype)
- Offline-support via IndexedDB voor draft-regels
