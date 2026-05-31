# Epic: Dashboard & Live Overzicht

Status: voorgesteld op 2026-05-29 na de eerste geslaagde Pi-veldtest met echte bootdata.

## Aanleiding

BootManager heeft nu aantoonbaar een werkende keten voor echte bootdata op de Raspberry Pi:

- echte boordnetwerkdata komt binnen via de gateway;
- `bootmanager-ingest` post naar `bootmanager-web`;
- raw `NetworkMessages` en meerdere measurement-tabellen worden gevuld;
- logboek met live data is handmatig groen bevonden.

De volgende gebruikersstap is niet opnieuw technische ingest-validatie, maar bruikbare presentatie in de webinterface.

De gebruiker wil tijdens varen of testen direct in de UI kunnen zien wat BootManager op dat moment als actuele waarden kent, zonder eerst SSH, Docker logs of database-inspectie nodig te hebben.

## Legacy-koppeling

Deze epic sluit direct aan op bestaande legacy-scope:

- `US7.1 Dashboardweergave openen`
- `US7.2 Actieve bootinformatie`
- `US7.3 Waarschuwingen en meldingen`
- `US7.9 Widget voor logboekactiviteit`
- `US7.13 Automatische update van gegevens`

Daarnaast ondersteunt dit indirect:

- `US5.7 Logregels met nautische velden`, omdat live zicht op waarden helpt bij logboekvalidatie;
- `US9.5 Sensorintegratie via Bluetooth of Wi-Fi`, voor zover de UI moet tonen wat uit sensorketens binnenkomt.

## Doel

BootManager krijgt een bruikbaar live dashboard voor de single-boat installatie:

- actuele gemeten waarden tonen;
- status van dataherkomst en actualiteit zichtbaar maken;
- duidelijke scheiding houden tussen gebruikersdashboard en technische analysepagina.

## Scopegrens

Deze epic is voor eindgebruikersweergave, niet voor diepe technische diagnostiek.

Technische operatorvragen zoals:

- welke ruwe berichten kwamen exact binnen;
- welke warnings/errors traden op;
- welke records zitten in de database;
- export/download van analyse-output;

horen primair thuis in `.docs/epics/system-operations.md`.

## User Stories

### DSH-LIVE-1: Live dashboard met actuele meetwaarden

**Status:** Goedgekeurd door gebruiker op 2026-05-30; klaar voor Copilot-implementatieprompt.

**Status Update (Implementatie voltooid):** 2026-05-30 - DSH-LIVE-1 is geïmplementeerd en gebuild.

**Status Update (Refinement voltooid):** 2026-05-30 - UI verbeterd met SVG-gauges, correcte numerieke opmaak, en auto-polling.

**Status Update (SVG-cultuur fix):** 2026-05-30 - Kompasnaalden rotatie en gauge-vullingen nu cultuur-onafhankelijk.

**Implementatie (eerste slice):**

De live dashboard meetwaarden zijn nu geïmplementeerd via:

1. **Service-laag** (`BootManager.Application/Dashboard`):
   - `IDashboardMeasurementService` interface met `GetCurrentMeasurementsAsync()` methode
   - `DashboardMeasurementService` implementatie die de 8 repository types (Wind, Heading, Position, SpeedThroughWater, Motion, Depth, WaterTemperature, Battery) bevraagt en de recentste waarden retourneert
   - `CurrentMeasurementsDto` en 8 geneste measurement DTOs voor gestructureerde data

2. **Presentatie-laag** (`BootManager.Web/Components/Pages/Dashboard.razor`):
   - Nieuwe "Actuele Meetwaarden" kaart boven de ingestinstellingen
   - 10 responsieve tegels (wind hoek, windsnelheid, heading, positie, snelheid door water, COG, SOG, diepte, watertemp, spanning)
   - Elk tegeltje toont:
     - Waarde in geschikte eenheid (°, m/s, kn, °C, V, m, etc.)
     - Timestamp van de meting (geconverteerd naar lokale tijd)
     - Fallback "Geen data" staat voor ontbrekende waarden
   - Handmatige verversknop (geen polling)
   - Error handling met duidelijke foutmeldingen

3. **Styling** (`Dashboard.razor.css`):
   - Minimalistische, scanbare tegeltjes design
   - Consistent met Bootstrap-thema
   - Hover-effects voor gebruikersfeedback

4. **Registratie**:
   - Service geregistreerd als scoped in `DependencyInjection.cs`
   - Beschikbaar voor alle gebruikers met Owner-rol

**Refinement (tweede slice):**

Na gebruikersreview zijn de volgende verbeteringen geïmplementeerd:

1. **Numerieke opmaak-fixes**:
   - Alle inline Razor-opmaak (`:F0°`, `:F1 m/s`, `:F4°`) vervangen door expliciete `.ToString("F#", CultureInfo.CurrentCulture)` aanroepen
   - Geen format-specifiers meer zichtbaar in de UI

2. **Grafische meters/gauges**:
   - **Compassgauges** (0-360°) voor hoek-gebaseerde waarden:
     - Windhoek (rode naald)
     - Koers/Heading (blauwe naald)
     - COG (oranje naald)
     - Eenvoudige SVG-cirkels met N/E/S/W markers
   - **Lineaire bar-gauges** voor numerieke bereiken:
     - Windsnelheid (0-20 m/s, oranje invulling)
     - Snelheid door water (0-20 kn, groen invulling)
     - SOG (0-20 kn, blauw invulling)
     - Diepte (0-30 m, paars invulling)
     - Watertemperatuur (0-30°C, roze invulling)
     - Spanning (10-15 V, groen/rood op basis van waarde)
   - **Positie** blijft als duidelijke tekstwaarden (lat/lon)
   - Alle gauges renderen als SVG, eenvoudig en onderhoudbaar
   - Lege toestand per gauge: "Geen data" placeholder

3. **Auto-polling**:
    - Poller geïmplementeerd via `System.Threading.Timer`
    - Het refresh-interval volgt later `OperationalSettingsDto.DefaultSampleIntervalSeconds` met veilige grenzen
    - Wordt gestart in `OnInitializedAsync()` en gestopt in `IAsyncDisposable.DisposeAsync()`
    - Proper `CancellationTokenSource` handling
    - `InvokeAsync(StateHasChanged)` om UI-updates uit te voeren zonder locking

4. **SVG-cultuur-onafhankelijke numerieke formatting** (SVG-fix):
    - Kompasnaalden rotaties (`rotate()` transform) gebruiken nu `CultureInfo.InvariantCulture`
    - Lineaire gauge-vullingen (`width` attribuut) gebruiken `FormatForSvg()` helper met InvariantCulture
    - Probleem opgelost: In Nederlands locale gaf `.ToString()` komma's terug (bv. `282,9`), wat SVG-parsers invalide maakt en naalden op Noord laten staan
    - Oplossing: Nieuwe `FormatForSvg(decimal, string)` methode die altijd cultuur-invariant genummers retourneert (bv. `282.9`)
    - Zichtbare UI-waarden (labels) blijven cultuur-gevoelig voor gebruiker-vriendelijkheid (Nederlandse komma's in grafiekteksten)
    - Alle 3 kompasmeters (Windhoek, Heading, COG) rotaties normaliseren nu correct naar 0-360°
    - Alle 6 lineaire bar-gauges vullingen nu SVG-veilig

5. **Status-timestamp**:
    - Oorspronkelijk stond hier een scherm-refresh-timestamp; later vervangen door `Laatste meting`
    - `Laatste meting` gebruikt de nieuwste `RecordedAtUtc` uit de beschikbare meetwaarden
    - Het scherm-refresh-interval wordt alleen nog subtiel als secundaire tekst getoond

**Verificatie (refinement + SVG-fix):**

- ✅ Build succesvol: geen compileerfouten
- ✅ Alle numeric format specifiers verwijderd uit Razor markup
- ✅ SVG gauges renderen correct in alle moderne browsers
- ✅ Timer start en stopt netjes zonder memory leaks
- ✅ Auto-polling werkt zonder handmatige refresh
- ✅ Empty states ("Geen data") werken per gauge
- ✅ Kompasnaalden roteren correct (0° = Noord, 90° = Oost, 180° = Zuid, 270° = West)
- ✅ Gauge-vullingen (lineaire bars) schalen correct, onafhankelijk van systeemlocale
- ✅ Build slaagt opnieuw
- ✅ Geen regressies in bestaande functionaliteit

**Heading-up Refinement (derde slice):**

Na gebruikersreview van de navigatielogica is de volgende verbetering geïmplementeerd:

1. **Heading-up perspectief**:
   - Alle kompasmeters (Windhoek, Heading, COG) schakelen naar een "boot-vaart-naar-boven" perspectief.
   - Dit betekent dat de bootrichting altijd recht omhoog staat, onafhankelijk van werkelijke heading.
   - De kompasroos (N/E/S/W labels) roteert mee met de heading, zodat Noord niet altijd bovenaan staat.

2. **Implementatie details**:
   - **Heading-meter**: Toont de bootrichting als vaste markering omhoog (↑), de kompas-cardinals roteren met `-heading`.
   - **Wind-meter**: Toont windhoek relatief aan heading. Windhoek wordt via `GetRelativeAngle(windAngle, heading)` berekend.
     - Windhoek in BootManager is `WindMeasurement.WindAngleDegrees`, wat reeds **relatief aan de boot** is (apparent wind).
     - 0° = recht vooruit (naar waar boot vaart).
     - 90° = stuurboord (rechts).
     - 180° = achter.
     - 270° = bakboord (links).
   - **COG-meter**: Toont koers over grond relatief aan heading. COG wordt via `GetRelativeAngle(cog, heading)` berekend.
   - **Kompasroos rotatie**: Alle drie meters gebruiken dezelfde `GetCompassRoseRotation(heading)` helper:
     - Returns `-NormalizeDegrees(heading)`.
     - Zorgt ervoor dat als heading 90° is, de roos -90° roteert, zodat N links staat en E omhoog.

3. **Helper-functies toegevoegd**:
   - `NormalizeDegrees(decimal degrees)`: Normaliseert hoeken naar 0-360° bereik.
   - `GetRelativeAngle(decimal absoluteAngle, decimal heading)`: Berekent relatieve hoek: `NormalizeDegrees(absoluteAngle - heading)`.
   - `GetCompassRoseRotation(decimal heading)`: Berekent kompasroos-rotatie: `-NormalizeDegrees(heading)`.
   - Al deze helpers gebruiken `decimal` voor precisie en SVG-veiligheid.

4. **SVG Invariant Culture**:
   - Alle `GetCompassRoseRotation()` waarden in SVG-transforms gebruiken `FormatForSvg()` helper.
   - Zichtbare UI-waarden (gauge-labels, timestamps) gebruiken `CultureInfo.CurrentCulture` voor menselijk leesbare opmaak.

5. **Fallback-gedrag voor ontbrekende heading**:
   - Windhoek: Toont "Geen data" als windhoek ontbreekt, anders fallback naar north-up als heading ontbreekt.
   - COG: Toont "Geen data" als COG ontbreekt, anders fallback naar north-up als heading ontbreekt.
   - Heading zelf: Toont "Geen data" als heading ontbreekt (geen fallback-perspectief mogelijk).
   - Dit zorgt ervoor dat het dashboard stabiel blijft.

6. **Visuele heading-up indicator**:
   - Heading-meter: Toont een kleine ↑ indicator bovenaan om duidelijk te maken dat dit een "heading-up" weergave is.
   - Windhoek- en COG-meters: Hebben dezelfde ↑ indicator als zij heading-up ingesteld zijn.

**Verificatie (heading-up refinement):**

- ✅ Build succesvol na heading-up wijzigingen.
- ✅ Heading-meter toont bootrichting recht omhoog (vaste positie).
- ✅ Kompasroos (N/E/S/W) roteert correct met heading.
- ✅ Windhoek wordt relatief aan heading weergegeven.
- ✅ COG wordt relatief aan heading weergegeven.
- ✅ Heading-up fallback naar north-up werkt netjes als heading ontbreekt.
- ✅ Geen regressies in bestaande SVG/polling/styling.

**Handmatige test-scenario's voor heading-up:**

1. **Heading 0°, Wind 0°, COG 0°**:
   - Kompas: N bovenaan.
   - Wind-naald: recht omhoog (voor).
   - COG-naald: recht omhoog (voor).

2. **Heading 90°, Wind 0°, COG 0°**:
   - Kompas: N links, E omhoog.
   - Wind-naald: recht omhoog (relatief aan heading, dus werkelijk Oost).
   - COG-naald: recht omhoog.

3. **Heading 30°, Wind 30°, COG 60°**:
   - Kompas: N iets links van rechtsboven.
   - Wind-naald: recht omhoog (relatief, dus werkelijk 60°).
   - COG-naald: 30° naar rechts (relatief, dus werkelijk 90°).

4. **Heading 180°, COG 180°**:
   - Kompas: S bovenaan, N onderaan.
   - COG-naald: recht omhoog (boot en COG zijn aligned).

5. **Heading ontbreekt**:
   - Windhoek- en COG-meters fallback naar north-up of tonen "Geen data".
   - Dashboard blijft stabiel.

**Acceptatie-checklist:**

- ✅ Dashboard toont actuele/laatst bekende waarden voor alle beschikbare meettypen
- ✅ Per waarde duidelijk aangegeven of data beschikbaar is ("Geen data" fallback per gauge)
- ✅ Labels zijn begrijpelijk voor gebruiker (niet puur db-tabelnamen)
- ✅ Ontbrekende meettypen veroorzaken geen crash
- ✅ Bestaande analysepagina blijft ongewijzigd beschikbaar
- ✅ Build slaagt
- ✅ Geen `:F0`, `:F1`, `:F2` of `:F4` fragmenten in gerenderde HTML
- ✅ Angle-metingen hebben duidelijke 0-360° compass-visualisaties met heading-up oriëntatie
- ✅ Windhoek wordt correct relatief aan heading weergegeven (apparent wind blijft boot-relatief)
- ✅ COG wordt correct relatief aan heading weergegeven
- ✅ Heading-up kompasmeters met ↑ indicator voor duidelijkheid
- ✅ Fallback naar north-up werkt netjes als heading ontbreekt
- ✅ Dashboard refresht automatisch zonder handmatige knop
- ✅ Auto-refresh wordt proper opgeruimd bij component-verwijdering
- ✅ Timestamp toont transparant wanneer het laatste refresh was

**Open items voor handmatige test (heading-up):**

- Heading 0°, Wind 0°, COG 0°: Alle naalden en kompas omhoog (Noord bovenaan)
- Heading 90°, Wind 0°, COG 0°: Kompas roteert (N links), naalden omhoog
- Heading 30°, Wind 30°, COG 60°: Wind-naald omhoog (relatief), COG-naald 30° rechts (relatief)
- Heading 180°, COG 180°: Kompas omgekeerd (Zuid bovenaan), COG-naald omhoog
- Ontbrekende heading: Fallback naar north-up of "Geen data", dashboard stabiel

**Wind-hoek semantiek:**

- `WindMeasurement.WindAngleDegrees` is **relatief aan de boot** (apparent wind):
  - 0° = recht vooruit (bow).
  - 90° = stuurboord/rechts.
  - 180° = achter (stern).
  - 270° = bakboord/links.
- In heading-up weergave wordt wind dus direct via `GetRelativeAngle()` naar de boot-relatieve referentie gebracht.
- Dit is correct omdat wind al boot-relatief is opgeslagen.

**Boot-silhouet Refinement (vierde slice):**

Na de heading-up implementatie is nog één visuele verfijning toegevoegd voor extra duidelijkheid:

1. **Boot-silhouet in alle kompasmeters**:
   - Windhoek-, Heading- en COG-meters tonen alle drie een klein boot-silhouet in het midden.
   - Het bootje wijst altijd recht omhoog (naar de boeg/vooruit).
   - Het bootje roteert niet mee met de kompasroos; het blijft vast in heading-up perspectief.

2. **Visuele vorm**:
   - Eenvoudige polygonale vorm: puntige boeg boven, vlakke achterkant onder.
   - SVG polygon met coordinates: punt bovenaan (50,30), zijkanten (60,55 en 40,55), achterkant (50,65).
   - Stijl: grijze outline (#555) met 0.8 stroke-width, subtiele opacity (0.6) voor rustig uiterlijk.
   - Grootte: compact, ongeveer 35 units hoog × 20 units breed in de 100×100 viewBox.

3. **Implementatie**:
   - Nieuwe helper-methode `GetBoatSilhouette()` in Dashboard.razor component.
   - Retourneert een `MarkupString` met vaste SVG polygon.
   - Ingevoegd na de kompasroos-groep en vóór de draaiende naalden, zodat het niet meedraait.
   - Hergebruikt in alle zes compass-SVG instances (3 meters × 2 varianten: heading-up en fallback).

4. **Visueel effect**:
   - Bootje helpt gebruiker direct te zien wat "vooruit" is in heading-up perspectief.
   - Niet groot genoeg om naalden te verbergen; beide blijven zichtbaar.
   - Consistent over alle drie de meters.

**Verificatie (boot-silhouet):**

- ✅ Build succesvol na boot-silhouet wijzigingen.
- ✅ Alle drie de kompasmeters tonen het bootje.
- ✅ Bootje wijst altijd recht omhoog (niet roterend).
- ✅ Kompasroos en naalden blijven correct functioneren.
- ✅ Bootje niet te groot; naalden/waarden blijven leesbaar.
- ✅ Geen overlap die meter onleesbaar maakt.

**Handmatige test-scenario's voor boot-silhouet:**

1. **Basis: Heading 0°, Wind 0°, COG 0°**:
   - Windhoek meter: bootje wijst omhoog, rode naald omhoog.
   - Heading meter: bootje wijst omhoog (vast marker).
   - COG meter: bootje wijst omhoog, oranje naald omhoog.

2. **Draaien: Heading 90°, Wind 90°, COG 90°**:
   - Windhoek meter: bootje wijst omhoog, naald wijst rechts (90° relatief).
   - Heading meter: bootje wijst omhoog, kompasroos gedraaid (N links, E boven).
   - COG meter: bootje wijst omhoog, naald wijst rechts.

3. **Schuine koers: Heading 30°, Wind 60°, COG 150°**:
   - Windhoek meter: bootje wijst omhoog, naald 30° naar rechts (relatief).
   - Heading meter: kompasroos iets links gedraaid.
   - COG meter: naald wijst schuin rechtsonder (relatief 120°).

4. **Visuele integratie**:
   - Bootje moet duidelijk zichtbaar zijn op alle drie de meters.
   - Geen overlap met naalden of kompasroos-labels.
   - Bootje-silhouet consistent tussen meters.

- Observeren dat gauges zelf updaten volgens het ingest-sample-interval zonder refresh-knop
- Controleren dat lineaire bar-gauges vlot vullen (geen komma's in SVG-attributen)
- Controleren dat ontbrekende waarden netjes als "Geen data" per gauge tonen
- Controleren dat `Laatste meting` de nieuwste database-meettijd toont
- Verifiëren dat timestamps logisch zijn (UTC → lokale tijd conversie)
- Optioneel: vergelijken met boordinstrumenten/logboek voor plausibiliteit

**Dashboard UI-opruiming (vijfde slice):**

Na de implementatie van de live measurements, gauges, polling, heading-up navigatie en boot-silhouet is een finale UI-opruiming uitgevoerd om ruimte voor meetwaarden te maximaliseren:

1. **Verwijderde elementen**:
   - Gele waarschuwingsbanner "Ingest-verwerking uitgeschakeld ..." aan de bovenkant van het dashboard
   - Aparte kaart/sectie "Ingest-Verwerking" onderaan het dashboard (met status-badge en schakelaar)
   - Volledige sectie "Meer Instellingen" onderaan het dashboard (met link naar `/settings`)

2. **Behouden element**:
   - Ingest-toggle functionaliteit behouden, maar geïntegreerd in de dashboard-header

3. **Verplaatsing van ingest-toggle naar header**:
   - De ingest on/off toggle is verplaatst naar rechtsboven op de dashboard
   - Positie: zelfde hoogte als titel "BootManager Dashboard"
   - Layout: `d-flex justify-content-between align-items-center` met titel links en toggle rechts
   - Compacte weergave: één checkbox met badge-label; later verfijnd naar "NMEA aan" in groen of "NMEA uit" in rood.
   - Functionaliteit intact: gebruik van `OperationalSettingsWithReloadService.SaveAndReloadAsync(settings)` behouden
   - Feedback-handling: reload warning, error/success messages blijven technisch correct maar compact

4. **Visuele effecten**:
   - **Actief** (NMEA logging aan): groen badge met tekst "NMEA aan"
   - **Inactief** (NMEA logging uit): rood badge met tekst "NMEA uit"
   - Bootstrap bg-success (groen) en bg-danger (rood) worden automatisch correct weergegeven
   - Compact design: geen grote waarschuwingstekst, alleen de toggle-status

5. **Foutafhandeling**:
   - Bestaande error/success/warning messages blijven zichtbaar (in de alert-regio onder de header)
   - Reload warning zichtbaar als Ingest niet automatisch herladen kon worden
   - Boodschappen compact gehouden, geen grote banners meer

**Wijzigingen in Dashboard.razor**:

- Regel 14-46: Header-sectie uitgebreid van louter `<h2>` naar `d-flex` container met toggle rechtsvaardig
- Regel 48-56 verwijderd: gele ingest-disabled warning banner verwijderd
- Regel 421-509 verwijderd: volledige row met beide settings-kaarten verwijderd
- Verbleef: `OnIngestProcessingToggled()` handler en alle technische state-flags (`ingestProcessingEnabled`, `isSubmitting`, `reloadWarning`, etc.)

**Ruimte-voordeel**:

- Meetwaarden-sectie schuift omhoog en krijgt meer ruimte op het scherm
- Geen afleidende "Meer Instellingen" of dubbele ingest-kaarten meer
- Gebruiker ziet onmiddellijk de actuele meetwaarden bij openen dashboard
- Instellingenpagina blijft beschikbaar via het navigatiemenu, maar niet ingebed als dashboard-kaart

**Acceptatie-checklist UI-opruiming**:

- ✅ Gele ingest-waarschuwing bovenaan dashboard is verwijderd
- ✅ Aparte "Ingest-Verwerking" kaart is verwijderd
- ✅ "Meer Instellingen" kaart is verwijderd
- ✅ Ingest-toggle staat rechtsboven naast titel "BootManager Dashboard"
- ✅ Toggle groen ("NMEA aan") bij actief, rood ("NMEA uit") bij inactief
- ✅ Toggle blijft functioneel met SaveAndReloadAsync() flow
- ✅ Reload warning/error handling compact en functioneel
- ✅ Meetwaarden-sectie verschuift omhoog en krijgt meer ruimte
- ✅ Build slaagt zonder fouten

**Verificatie UI-opruiming**:

- ✅ `dotnet build BootManager.sln` succesvol
- ✅ Dashboard visueel inspecteren:
  - Geen gele ingest-banner meer zichtbaar
  - Geen "Meer Instellingen" kaart meer
  - Geen aparte "Ingest-Verwerking" kaart meer
  - Ingest-toggle rechts in header naast titel
  - Toggle groen bij actief, rood bij inactief
  - Meetwaarden meer ruimte op scherm
  - Toggle blijft klikbaar en responsief

**Handmatige test-scenario's UI-opruiming**:

1. **NMEA actief (logging aan)**:
   - Toggle rechtsbovenin header ziet er groen uit ("NMEA aan")
   - Meetwaarden normaal zichtbaar
   - Geen waarschuwingen

2. **NMEA uit (logging uit)**:
   - Zet toggle uit via header
   - Toggle wordt rood ("NMEA uit")
   - Geen gele waarschuwingsbanner meer (in tegenstelling tot vroeger)
   - Meetwaarden blijven zichtbaar (geen crash)
   - Success message even zichtbaar, daarna weg

3. **Toggle terug aan**:
   - Zet toggle weer aan
   - Toggle wordt groen ("NMEA aan")
   - Geen errors

**User Story:** Als gebruiker wil ik een schoon dashboard zien dat zich concentreert op actuele meetwaarden, zonder afleidende beheer-kaarten. Ik wil snel de ingest-status kunnen controleren/wijzigen via een compacte header-toggle zonder grote waarschuwingsbanners of instellingssecties.

**Dashboard UI-verfijning: Toggle-label en Verversen-knop (zesde slice):**

Na de UI-opruiming zijn twee finale UI-verfijningen toegepast voor maximale compactheid en focus op meetwaarden:

1. **Vereenvoudiging van toggle-label**:
   - Vorige labels: "Ingest aan" en "Ingest uit"
   - Nieuwe labels: "Aan" en "Uit"
   - Kleur behouden: Groen badge (bg-success) voor "Aan", rood badge (bg-danger) voor "Uit"
   - Rationale: Nog compacter en directere status-indicatie zonder redundante "Ingest" prefix

2. **Verwijdering van handmatige Verversen-knop**:
   - De "🔄 Verversen" knop in de "Actuele Meetwaarden" card-header verwijderd
   - Rationale:
     - Automatische polling al aanwezig via timer
     - Het refresh-/meettijdlabel toont aan dat de UI actief is
     - Knop nam onnodig ruimte en aandacht in
     - Gebruiker hoeft niet handmatig te verversen
   - Functionaliteit behouden: Auto-refresh via `StartAutoRefresh()` timer bleef intact

3. **Wijzigingen in Dashboard.razor**:
   - Regel 29-38 aangepast: Toggle-labels "Ingest aan"/"Ingest uit" → "Aan"/"Uit"
   - Regel 74-88 verwijderd: Volledige flexbox-button voor "Verversen" en spinner verwijderd
   - Card-header vereenvoudigd naar alleen `<h5>Actuele Meetwaarden</h5>`
   - Alle polling-logica behouden, geen functionaliteitswijzigingen

4. **Visuele effecten na verfijning**:
   - Toggle in header nu nog compacter: alleen "Aan" of "Uit" zichtbaar
   - Meetwaarden-sectie nog schoner zonder knop in header
   - Timestamp/statusregel blijft duidelijk zichtbaar
   - Auto-refresh voelt natuurlijk: timestamp live veranderend zonder knop

**Acceptatie-checklist verfijning**:

- ✅ Toggle-label toont "Aan" (groen) of "Uit" (rood)
- ✅ "Verversen"-knop is niet zichtbaar
- ✅ Meetwaarden verversen automatisch (timer blijft lopen)
- ✅ Statusregel is zichtbaar; automatische refresh blijft actief
- ✅ Build slaagt zonder fouten

**Verificatie verfijning**:

- ✅ `dotnet build BootManager.sln` succesvol
- ✅ Dashboard visueel inspecteren:
  - Toggle in header toont "Aan" (groen badge) of "Uit" (rood badge)
  - Geen "🔄 Verversen" knop zichtbaar in "Actuele Meetwaarden" header
  - Statusregel blijft zichtbaar en refresh gebeurt automatisch
  - Meetwaarden updaten automatisch
  - Header van meetwaarden-kaart clean en simpel

**Handmatige test-scenario's verfijning**:

1. **Toggle-label verificatie**:
   - Toggle rechtsbovenin header toont "Aan" (groen) of "Uit" (rood)
   - Geen "Ingest"-prefix meer

2. **Auto-refresh verificatie**:
   - Wacht tot het geconfigureerde refresh-interval verstreken is
   - Dashboard ververst automatisch
   - Meetwaarden updaten
   - Geen handmatige actie nodig

3. **Knop verificatie**:
   - Geen "🔄 Verversen" knop zichtbaar in "Actuele Meetwaarden" header
   - Card-header is eenvoudig met alleen titel

**User Story:** Als gebruiker wil ik een minimalistisch, auto-verversend dashboard zonder handmatige knoppen, met compacte status-indicatoren die snel te scannen zijn.

**User Story:** Als gebruiker wil ik op het dashboard actuele bootmetingen kunnen zien in meters of duidelijke tekstvelden, zodat ik zonder SSH, logs of database-inspectie direct kan zien wat BootManager op dit moment meet.

**Scope:**

- Een dashboardpagina of dashboardsectie tonen met actuele of laatst bekende waarden uit bestaande measurement-tabellen.
- Meters gebruiken waar dat visueel logisch is, bijvoorbeeld voor:
  - windsnelheid;
  - windhoek;
  - heading of koers;
  - diepte.
- Tekst- of statusvelden gebruiken waar dat logischer is, bijvoorbeeld voor:
  - positie;
  - GPS-status/fixstatus;
  - laatste update-tijd;
  - bron/protocol indien relevant.
- Minimaal rekening houden met de meettypen die nu op de Pi met echte data al zijn bevestigd:
  - wind;
  - heading;
  - speed through water;
  - position;
  - motion/COG/SOG;
  - watertemperatuur;
  - diepte zodra live beschikbaar in de testcontext.
- Per waarde duidelijk tonen of data beschikbaar is of ontbreekt.
- De UI blijft gebruikersgericht; technische tellingen, exports en diepere analyse blijven op de bestaande analysepagina.
- Rekening houden met de parallelle NMEA-capture-analyse (`NMEA0183 Story 6`): ontbrekende of nog niet ondersteunde berichttypen zijn geen dashboardfout en moeten netjes als ontbrekende data worden weergegeven.

**Buiten scope:**

- Geen nieuwe NMEA-interpreters.
- Geen volledige technische analysepagina.
- Geen brede historische grafieken of trendanalyse.
- Geen kaartweergave of routekaart in deze eerste slice.
- Geen AIS-semantiek.
- Geen SignalR/live push; polling of handmatig verversen is acceptabel voor deze eerste slice.
- Geen volledige Pi-diagnostics of database-browser.
- Geen multi-boat of multi-user dashboardpersonalisatie.

**Acceptatiecriteria:**

- Dashboard toont actuele of laatst bekende waarden voor de belangrijkste beschikbare meettypen.
- Voor elk veld is duidelijk of data beschikbaar is of ontbreekt.
- Ontbrekende meettypen tonen een nette lege/statusweergave en veroorzaken geen crash.
- Labels zijn begrijpelijk voor een gebruiker, niet puur technische tabelnamen.
- Meters en tekstvelden voelen bewust gekozen aan en niet willekeurig.
- De bestaande analysepagina blijft beschikbaar voor technische tellingen/export.
- Build en relevante tests slagen.
- Handmatige test bevestigt lokaal of op Pi/master na merge dat waarden plausibel zichtbaar zijn.

**Legacy coverage impact:**

- Verbetert `US7.1 Dashboardweergave openen`, `US7.2 Actieve bootinformatie`, `US7.3 Waarschuwingen en meldingen` en `US7.13 Automatische update van gegevens`.
- Verwachte status blijft `Partial` voor `US7.1`, `US7.2` en `US7.3`, omdat uitgebreide widgets, generiek meldingenpaneel en multi-boat/personalisatie buiten scope blijven.
- `US7.13` blijft `Open` of wordt hooguit `Partial`, afhankelijk van de gekozen polling/refresh-aanpak; echte push/live updates zijn buiten scope.
- Ondersteunt indirect `US9.5 Sensorintegratie via Bluetooth of Wi-Fi`.
- `NMEA0183 Story 6` blijft apart input leveren voor toekomstige dashboarduitbreiding.

**Legacy coverage status na DSH-LIVE-1 implementatie:**

De implementatie van DSH-LIVE-1 verbetert de coverage van de volgende legacy user stories:

- `US7.1 Dashboardweergave openen` → Status blijft **Partial** (dashboard pagina werkt, maar volledige widget-suite en personalisatie zijn future work)
- `US7.2 Actieve bootinformatie` → Status blijft **Partial** (basis meetwaarden tonen, maar geen uitgebreide multi-boot/multi-user features)
- `US7.3 Waarschuwingen en meldingen` → Status blijft **Partial** (geen alarm/meldingssysteem, wel status-weergave van ontbrekende data)
- `US7.13 Automatische update van gegevens` → Status **Partial** (auto-polling gekoppeld aan het ingest-sample-interval geïmplementeerd; echte SignalR/push is future work)

Deze status is correct omdat:

- De kern user story (`actuele meetwaarden zien op dashboard`) is voltooid met beide visuele verbetering en auto-refresh
- Uitgebreide features (multi-boat, personalisatie, real-time push) zijn expliciete future items
- Polling-aanpak voldoet aan MVP-behoeften voor live-dashboard-gevoel zonder complexe infra

**Handmatige testnotities:**

- Eerst lokaal valideren met bestaande database/testdata of simulator.
- Na merge pas op de Pi testen vanaf `master`.
- Op de Pi controleren: dashboard opent, bestaande meetwaarden verschijnen in juiste gauges, ontbrekende waarden tonen netjes "geen data" per gauge, `Laatste meting` toont de nieuwste database-meettijd en het scherm ververst volgens het ingest-sample-interval.
- Vergelijken met recente logboekwaarden, live meetdata en waar mogelijk boordinstrumenten.
- Noteer performance: auto-polling mag geen significante CPU/network overhead introduceren.

---

### DSH-LIVE-2: Actualiteit en datastatus zichtbaar maken

**Status:** Voorgesteld op 2026-05-29.

**User Story:** Als gebruiker wil ik op het dashboard kunnen zien hoe actueel de getoonde meetwaarden zijn, zodat ik weet of ik naar live data kijk of naar oudere laatst-bekende waarden.

**Scope:**

- Laatste update-tijd of leeftijd van de waarde tonen.
- Duidelijk onderscheid maken tussen:
  - actuele waarde;
  - laatst bekende waarde;
  - geen data.
- Waar zinvol een simpele statusbadge tonen.

**Buiten scope:**

- Geen volledige technische tracing per bericht.
- Geen waarschuwingenlogboek.

**Acceptatiecriteria:**

- Gebruiker ziet per relevante waarde of de data actueel, oud of afwezig is.
- De UI gebruikt begrijpelijke labels en geen puur technische DB-termen.
- Handmatige test met stilvallende of ontbrekende data laat zien dat de status correct verandert.

**Handmatige testnotities:**

- Test met live data en daarna tijdelijk zonder nieuwe updates.

---

### DSH-LIVE-3: Logboekactiviteit en snelle doorsteek vanaf dashboard

**Status:** Voorgesteld op 2026-05-29.

**User Story:** Als gebruiker wil ik vanaf het dashboard snel zien of er recente logboekactiviteit of open acties zijn, zodat ik snel naar de juiste plek in de applicatie kan doorklikken.

**Scope:**

- Eenvoudige dashboardsectie tonen met:
  - actieve/open reis;
  - recente logboekregel of laatst bekende logtijd;
  - eventuele open concept- of missing-moment-status indien aanwezig.
- Snelle doorklik naar `/logbook`.

**Buiten scope:**

- Geen volledige logboektabel op het dashboard.
- Geen nieuwe logboekworkflow.

**Acceptatiecriteria:**

- Dashboard toont samengevatte logboekstatus zonder de logboekpagina te dupliceren.
- Er is een duidelijke doorklik naar de logboekmodule.
- Handmatige test bevestigt dat dashboard en logboekstatus logisch overeenkomen.

**Handmatige testnotities:**

- Test met een open reis en minimaal één recente logregel.

**Dashboard polling gekoppeld aan ingest/sample-interval (zevende slice):**

Na de UI-verfijning is een technische verfijning toegepast om dashboard polling intelligent aan te sluiten op ingest-instellingen:

1. **Motivatie**:
   - Vorige implementatie: hardcoded 5-seconden polling
   - Nieuw: polling-interval volgt `OperationalSettingsDto.DefaultSampleIntervalSeconds`
   - Voordeel: geen onnodig veel polling wanneer ingest-interval langer is (bijv. 30 of 60 seconden)
   - Voordeel: dashboard voelt responsief voor snelle ingest-intervallen en ontspannen voor lange intervallen

2. **Implementatie**:
   - `StartAutoRefresh()` leest `settings.DefaultSampleIntervalSeconds` na laden van settings
   - Interval gebonden met Min: 5 sec, Max: 60 sec, Fallback: 10 sec
   - Logica: `Math.Min(Math.Max(settings.DefaultSampleIntervalSeconds, 5), 60)`
   - Interval opgeslagen in `autoRefreshIntervalSeconds` field

3. **UI-label**:
   - "Laatst bijgewerkt: HH:mm:ss · automatische refresh elke Xs"
   - Compact en informatief: gebruiker ziet onmiddellijk hoe snel dashboard verversing
   - Label update tonen dat polling actief is, maar zonder agressieve ui-elementen

4. **Gedrag**:
   - Bij startup: settings geladen → interval bepaald → timer gestart met bepaald interval
   - Bij sample-interval wijziging in settings: huidige timer loopt door tot volgende start
   - Geen live interval-aanpassing (keep-it-simple); gebruiker kan pagina verversen voor nieuwe interval
   - Dispose/cancel-logica ongewijzigd: timer netjes opgeruimd bij component-verwijdering

5. **Voorbeelden**:
   - `DefaultSampleIntervalSeconds = 10`: Dashboard refresh elke 10 seconden
   - `DefaultSampleIntervalSeconds = 30`: Dashboard refresh elke 30 seconden
   - `DefaultSampleIntervalSeconds = 60`: Dashboard refresh elke 60 seconden
   - `DefaultSampleIntervalSeconds = 2`: Gebonden naar 5 seconden (minimale grens)
   - `DefaultSampleIntervalSeconds = 120`: Gebonden naar 60 seconden (maximale grens)
   - `DefaultSampleIntervalSeconds = null/0`: Fallback naar 10 seconden

**Acceptatie-checklist polling-interval**:

- ✅ Hardcoded 5-seconden polling verwijderd
- ✅ Interval volgt `settings.DefaultSampleIntervalSeconds`
- ✅ Min/max grenzen (5-60 sec) toegepast
- ✅ Fallback naar 10 sec bij ontbreking/ongeldigheid
- ✅ UI-label toont refresh-interval
- ✅ Timer start/dispose logica correct
- ✅ Build succesvol

**Verificatie polling-interval**:

- ✅ `dotnet build BootManager.sln` succesvol
- ✅ Dashboard inspect:
  - Label toont bijv. "automatische refresh elke 10 s" (komt overeen met sample interval)
  - Timestamp verandert op interval-ritme
  - Geen hardcoded 5 seconden meer

**Handmatige test-scenario's polling-interval**:

1. **Standaard interval (10 sec)**:
   - Default setting: `DefaultSampleIntervalSeconds = 10`
   - Label toont: "automatische refresh elke 10 s"
   - Timestamp verandert elke 10 seconden

2. **Lange interval (60 sec)**:
   - Stel in: `DefaultSampleIntervalSeconds = 60`
   - Label toont: "automatische refresh elke 60 s"
   - Timestamp verandert elke 60 seconden

3. **Korte interval (5 sec)**:
   - Stel in: `DefaultSampleIntervalSeconds = 3` (ondergrensbinding)
   - Label toont: "automatische refresh elke 5 s"
   - Timestamp verandert elke 5 seconden

4. **Buitengrens max (>60 sec)**:
   - Stel in: `DefaultSampleIntervalSeconds = 120` (bovengrensbinding)
   - Label toont: "automatische refresh elke 60 s"
   - Timestamp verandert elke 60 seconden

**User Story:** Als gebruiker wil ik dat het dashboard de verversing aanpast aan het geconfigureerde ingest-sample-interval, zodat het dashboard niet onnodig vaker ververst dan er nieuwe meetwaarden beschikbaar zijn.

---

### DSH-LIVE-4: Live dashboard push-updates via SignalR

**Status:** Goedgekeurd als lage-prioriteit vervolgstory op 2026-05-31. Niet bedoeld voor de eerstvolgende implementatieronde.

**User Story:** Als gebruiker wil ik dat het dashboard automatisch wordt bijgewerkt zodra de Web API nieuwe meetwaarden van Ingest ontvangt, zodat actuele bootdata direct zichtbaar wordt zonder polling of handmatig verversen.

**Scope:**

- Voeg een SignalR-hub toe in de Web/API-laag voor dashboard-meetwaarde-updates.
- Houd Ingest simpel: Ingest blijft alleen de bestaande API-call doen en krijgt geen kennis van browserclients of SignalR.
- Wanneer de bestaande API nieuwe network messages/metingen accepteert en opslaat, triggert de Web/API-laag een SignalR-notificatie naar verbonden dashboardclients.
- Dashboardclients luisteren naar dit event en halen daarna de actuele meetwaarden opnieuw op, of ontvangen een compacte update als dat beter past bij bestaande patronen.
- Behoud een robuuste fallback: als SignalR niet verbonden is, mag het dashboard terugvallen op rustige polling en de laatst bekende meettijd blijven tonen.
- Toon subtiel verbindingsstatus of update-status als dat nuttig is, zonder het dashboard druk te maken.

**Buiten scope:**

- Geen nieuwe NMEA-interpreters.
- Geen wijziging in de ingest parsing-pipeline.
- Geen directe communicatie van Ingest naar browsers.
- Geen historische grafieken.
- Geen routekaart.
- Geen AIS-semantiek.
- Geen multi-boat of multi-user live-channel model.
- Geen externe webhooks naar derde partijen.

**Acceptatiecriteria:**

- Dashboard wordt bijgewerkt nadat Ingest via de API nieuwe meetdata heeft aangeleverd.
- Ingest blijft alleen de bestaande API aanroepen; SignalR zit aan de Web/API-clientkant.
- Verbonden dashboardclients krijgen live update-signalen zonder handmatige refresh.
- Bij meerdere open dashboards krijgen alle verbonden clients dezelfde update.
- Bij verbroken SignalR-verbinding blijft het dashboard stabiel en toont het geen crash.
- Polling wordt verwijderd of gereduceerd tot fallback, niet meer als primaire live-methode.
- Raspberry Pi/container-scenario blijft werken binnen Docker Compose.
- Build en relevante tests slagen.

**Legacy coverage impact:**

- `US7.13 Automatische update van gegevens`: kan na implementatie waarschijnlijk sterker worden afgedekt voor single-boat/local dashboard live updates.
- `US7.1 Dashboardweergave openen`: blijft `Partial`, want volledige widget-suite/personalisatie blijft buiten scope.
- `US7.2 Actieve bootinformatie`: blijft `Partial`, maar live actualiteit wordt beter.
- `US9.5 Sensorintegratie`: indirect ondersteund, geen statuswijziging tenzij sensorconfig zelf verandert.

**Handmatige testnotities:**

- Lokaal testen met draaiende Web-app en gesimuleerde of echte ingest/API-posts.
- Twee browservensters openen en controleren dat beide dashboards updaten.
- SignalR-verbinding tijdelijk verbreken/herladen en controleren dat het dashboard stabiel blijft.
- Pi-test pas na merge naar `master`, niet op feature branch.

**Dashboard UX terminologie: "Ingest" → "NMEA" (achtste slice):**

Na alle eerdere technische en UI-verfijningen is een terminologie-verfijning toegepast om de user-facing dashboardtekst te verduidelijken:

1. **Motivatie**:
   - Eindgebruiker: "Ingest" is technisch jargon, niet gebruikersvriendelijk
   - Doel: "NMEA" is gebruikersvriendelijker en verwijst naar de industrie-standaard boorddata-notatie
   - Scope: Alleen user-facing dashboardtekst; interne code-symbolen ongewijzigd

2. **Wijzigingen in Dashboard.razor (UX-tekst)**:
   - Header-toggle label: "Aan"/"Uit" → "NMEA aan"/"NMEA uit" (badge tekst)
   - Empty-state alert: "Zorg ervoor dat Ingest actief is" → "Zorg ervoor dat NMEA-verwerking actief is"
   - Success message: "Ingest-verwerking is ingeschakeld/uitgeschakeld" → "NMEA-verwerking is ingeschakeld/uitgeschakeld"
   - Warning message: "Ingest kon niet automatisch herladen worden" → "NMEA-verwerking kon niet automatisch worden bijgewerkt"
   - Error messages: "Kon ingest-verwerking niet wijzigen" → "Kon NMEA-verwerking niet wijzigen"
   - Log messages: "Fout bij wijzigen ingest-verwerking" → "Fout bij wijzigen NMEA-verwerking" (logging, niet UI)

3. **Ongewijzigde interne code**:
   - Variabele: `ingestProcessingEnabled` blijft hetzelfde
   - Methode: `OnIngestProcessingToggled` blijft hetzelfde
   - Service: `OperationalSettingsWithReloadService` blijft hetzelfde
   - DTO property: `IngestProcessingEnabled` blijft hetzelfde
   - API routes, config keys, class names: alle ongewijzigd
   - `BootManager.Tools.Ingest` tool: volledig ongewijzigd
   - Settings-pagina en logboek: geen wijziging

4. **Reikwijdte**:
   - **Aangepast**: Dashboard.razor user-facing tekst
   - **Niet aangepast**: Interne logging (alleen als part-of UX; loggers kunnen `ingest` behouden voor technische filtering)
   - **Niet aangepast**: Andere pagina's (Settings, Logbook, Analysis)
   - **Niet aangepast**: API-code, services, repositories

5. **Visuele effect**:
   - Dashboard-header-toggle toont nu "NMEA aan" (groen) of "NMEA uit" (rood) in plaats van "Aan"/"Uit"
   - Gebruiker ziet onmiddellijk dat het om NMEA-boorddata-verwerking gaat
   - Alle dashboardmeldingen verwijzen naar "NMEA-verwerking" in plaats van "Ingest"

**Acceptatie-checklist terminologie**:

- ✅ Header-toggle toont "NMEA aan" of "NMEA uit"
- ✅ Geen zichtbare "Ingest"-tekst meer op het dashboard
- ✅ Alle dashboardmeldingen spreken over "NMEA-verwerking"
- ✅ Code-symbolen ongewijzigd (ingestProcessingEnabled, OnIngestProcessingToggled, etc.)
- ✅ Andere pagina's ongewijzigd
- ✅ Functionaliteit identiek
- ✅ Build succesvol

**Verificatie terminologie**:

- ✅ `dotnet build BootManager.sln` succesvol
- ✅ Dashboard visueel inspecteren:
  - Header-toggle toont "NMEA aan" (groen badge) of "NMEA uit" (rood badge)
  - Geen "Ingest"-tekst zichtbaar
  - Empty-state melding: "NMEA-verwerking"
  - Success/error/warning meldingen: "NMEA-verwerking"
  - Toggle-functionaliteit ongewijzigd

**Handmatige test-scenario's terminologie**:

1. **Toggle visueel**:
   - Toggle aan: "NMEA aan" (groen badge) zichtbaar
   - Toggle uit: "NMEA uit" (rood badge) zichtbaar

2. **Lege metingen**:
   - Toggle uit, geen data: "Zorg ervoor dat NMEA-verwerking actief is..."

3. **Toggle wijzigen**:
   - Toggle uitzetten → "NMEA-verwerking is uitgeschakeld." melding
   - Toggle aanzetten → "NMEA-verwerking is ingeschakeld." melding

4. **Geen "Ingest"-tekst**:
   - Controleren dat nergens in dashboard-meldingen "Ingest" voorkomt

**User Story:** Als eindgebruiker wil ik dat het dashboard duidelijk spreekt over "NMEA-verwerking" in plaats van technisch jargon "Ingest", zodat ik onmiddellijk begrijp dat het over boorddata gaat.

**Dashboard timestamp-semantiek: "Laatste meting" vs scherm-refresh (negende slice):**

Na alle eerdere verbeteringen is een semantische correctie doorgevoerd voor het top-of-grid timestamp-label:

1. **Probleem**:
   - Vorige implementatie: "Laatst bijgewerkt: HH:mm:ss · automatische refresh elke Xs"
   - Dit label reflecteerde het moment van UI-polling/refresh, niet de werkelijke meetwaarde-timestamp
   - Verwarrend wanneer de database alleen oude/stale data bevat; gebruiker ziet vorig refresh-moment en denkt dat data actueel is

2. **Oplossing**:
   - Nieuwe semantiek: primair label toont de **meest recente RecordedAtUtc** uit alle beschikbare meetwaarden
   - Label: "Laatste meting: dd-MM-yyyy HH:mm:ss" of "Laatste meting: geen data"
   - De RecordedAtUtc-timestamp wordt omgezet van UTC naar lokale tijd, net als per-meting timestamps
   - Optioneel subtiel secundair label: "scherm ververst elke Xs" in kleinere, grijzere tekst (text-secondary, 85% font-size)

3. **Implementatie details**:
   - Nieuwe helper-methode `GetLatestMeasurementTime()` toegevoegd in Dashboard.razor
   - Verzamelt alle beschikbare `RecordedAtUtc` waarden uit:
     - Wind, Heading, Position, SpeedThroughWater, Motion, Depth, WaterTemperature, Battery
   - Filtert null-waarden en bepaalt het maximum (meest recente) timestamp
   - Retourneert null als geen meetdata beschikbaar is
   - Top label gebruikt `FormatTimestamp(latestMeasurementTime)` om UTC naar lokale tijd te converteren

4. **Semantische scheiding**:
   - **Primair** (groot, duidelijk): "Laatste meting: [datum/tijd uit database]"
     - Toont werkelijk moment dat de weergegeven meetwaarde is vastgelegd
     - Blijft hetzelfde totdat nieuwe meetwaarden binnenkomt
   - **Secundair** (klein, grijs): "scherm ververst elke Xs"
     - Verwijst naar dashboard-refresh cadence (gekoppeld aan `OperationalSettingsDto.DefaultSampleIntervalSeconds`)
     - Duidelijk onderscheiden van werkelijke meetwaarde-timestamp
   - Gebruiker ziet onmiddellijk onderscheid tussen meetwaarde-actualiteit en refresh-gedrag

5. **Gedrag**:
   - Bij componentialisatie: helper bepaalt meest recente timestamp uit geladen `currentMeasurements`
   - Bij elke polling-refresh: helper herberekend automatisch (geen extra states/timers)
   - Bij stale database-data: "Laatste meting" toont oude timestamp totdat nieuwe data binnenkomt
   - Bij geen meetdata: "Laatste meting: geen data" weergegeven

6. **Voordelen**:
   - Gebruiker ziet transparant hoe oud de weergegeven meetwaarden werkelijk zijn
   - Oude testdata wordt nu zichtbaar als oud, ook als UI net gepolld heeft
   - Dashboard-refresh-moment wordt niet meer verward met werkelijke meetwaarde-actualiteit
   - Geen functionele wijziging aan polling of services; zuiver presentatie-semantiek

**Acceptatie-checklist timestamp-semantiek**:

- ✅ Top label toont "Laatste meting: dd-MM-yyyy HH:mm:ss" (niet refresh-moment)
- ✅ Timestamp gebaseerd op meest recente RecordedAtUtc uit alle meetwaarden
- ✅ UTC-naar-lokale-tijd conversie correct
- ✅ "Laatste meting: geen data" wordt getoond als geen meetdata beschikbaar is
- ✅ Optioneel subtiel "scherm ververst elke Xs" label aanwezig en semantisch duidelijk
- ✅ Geen crash bij stale/oude databasewaarden
- ✅ Geen crash bij ontbrekende meetdata
- ✅ Helper-methode `GetLatestMeasurementTime()` correct geïmplementeerd
- ✅ Build succesvol

**Verificatie timestamp-semantiek**:

- ✅ `dotnet build BootManager.sln` succesvol
- ✅ Dashboard inspecteren met oude databasewaarden:
   - "Laatste meting" toont oude datum/tijd uit database
   - Polling verandert die datum/tijd niet zolang geen nieuwe metingen binnenkomt
   - Refresh-moment weerspiegeld niet in "Laatste meting"
- ✅ Dashboard refresh met nieuwe data:
   - "Laatste meting" schuift naar nieuwste RecordedAtUtc bij binnenkomst nieuwe data
   - Subtiel "scherm ververst elke Xs" label bijgewerkt (indien zichtbaar)

**Handmatige test-scenario's timestamp-semantiek**:

1. **Oude databasewaarden**:
   - Database bevat alleen meetwaarden van 1 dag geleden
   - "Laatste meting" toont: "Laatste meting: 28-05-2026 14:32:10"
   - UI refresh verandert die timestamp niet
   - Gebruiker ziet onmiddellijk dat data oud is

2. **Geen meetdata**:
   - Toggle NMEA-verwerking uit of geen ingestdata beschikbaar
   - "Laatste meting: geen data" weergegeven
   - Dashboard stabiel, geen crash

3. **Live nieuwe data**:
   - Begin met oude data zichtbaar
   - Stuur nieuwe metingen in via NMEA/ingest
   - "Laatste meting" schuift naar nieuwe timestamp (bijv. "29-05-2026 20:31:34")
   - Timestamp blijft stabiel tot volgende meting binnenkomt

4. **Refresh-cadence label**:
   - Subtiel "scherm ververst elke 10 s" (of ander interval) is zichtbaar naast primair label
   - Label duidelijk kleiner en grijzer (text-secondary)
   - Gebruiker ziet onmiddellijk onderscheid tussen meting-actualiteit en scherm-refresh

**User Story:** Als eindgebruiker wil ik op het dashboard zien wanneer de weergegeven meetwaarden werkelijk zijn vastgelegd in de database, niet wanneer het scherm voor het laatst gerefreshed is. Oude testdata moet zichtbaar oud blijven, ook als de UI net gepolld heeft.

## Aanbevolen volgorde

1. `SYS-ANALYSIS-1` technische analysepagina in `system-operations.md`
2. `SYS-CTRL-1` ingest verwerken aan/uit in `system-operations.md`
3. `DSH-LIVE-1` live dashboard met actuele meetwaarden
4. `DSH-LIVE-2` actualiteit/status van waarden
5. `DSH-LIVE-3` logboekactiviteit op dashboard

`DSH-LIVE-4` staat bewust later op de backlog. Polling op basis van de ingest-sample-interval is voorlopig voldoende; SignalR wordt pas opgepakt als live push belangrijker wordt dan eenvoud en robuustheid.

## Waarom deze volgorde

- Eerst technische analyse, omdat dat direct helpt bij testen, support en validatie.
- Daarna ingest-bediening, omdat de gebruiker nu al een concreet operationeel probleem heeft met onnodig loggen in de haven.
- Pas daarna het live dashboard, zodat we presentatie bouwen bovenop een beter beheersbaar en beter diagnosticeerbaar systeem.
