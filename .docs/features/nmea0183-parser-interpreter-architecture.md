# NMEA 0183 Parser/Interpreter Architectuur

**Datum:** 2026-05-17  
**Status:** Ontwerp – Fase 2/3 voorbereiding  
**Epic:** [nmea0183-support.md](../epics/nmea0183-support.md)

---

## Doel van dit document

Dit document legt de architectuur vast voor de verwerking van NMEA 0183 sentences nadat ze door de
ingest foundation (Fase 1) zijn opgeslagen als raw `NetworkMessage`.

Het beschrijft:
- hoe de parser/interpreter-laag eruit ziet
- welke sentence-types als eerste prioriteit krijgen
- hoe bestaande measurement entities hergebruikt worden
- welke ontwerpkeuzes vaststaan
- welke ontwerpkeuzes nog open zijn

Geen codewijzigingen in dit document; het dient als ontwerp- en besluitdocument voor de implementatie.

---

## Context: wat er al is

### Fase 1 – Geïmplementeerd

- `Nmea0183IngestService` luistert op UDP `0.0.0.0:10110` en stuurt raw sentences door naar `BootManager.Web`.
- `NetworkMessage.Protocol` wordt getagd als `NMEA0183`.
- Raw sentences worden opgeslagen in de `NetworkMessages`-tabel.
- De bestaande NMEA2000-flow op `127.0.0.1:2000` is ongewijzigd.

### Bestaande NMEA2000 slices – onaangetast

De volgende slices bestaan al volledig en worden niet aangeraakt:

| Slice | PGN | Entity |
|-------|-----|--------|
| Battery | 127508 | `BatteryMeasurement` |
| Depth | 128267 | `DepthMeasurement` |
| Wind | 130306 | `WindMeasurement` |
| Motion (COG/SOG) | 129026 | `MotionMeasurement` |
| Position | 129025 | `PositionMeasurement` |
| Heading | 127250 | `HeadingMeasurement` |
| Speed Through Water | 128259 | `SpeedThroughWaterMeasurement` |
| Water Temperature | 130312 | `WaterTemperatureMeasurement` |

---

## Architectuurprincipes voor NMEA 0183

### 1. Parser en interpreter blijven strikt gescheiden

Conform de bestaande NMEA2000-aanpak:

- **Parser (technisch):** Herkent het sentence-type, valideert de structuur, splitst velden op.
  Produceert een generiek parse-resultaat of een typed DTO.
- **Interpreter (semantisch):** Leidt betekenisvolle waarden af uit het parse-resultaat.
  Produceert een `{Type}MessageInterpretationDto`.

De scheiding voorkomt dat parseerfouten measurement-opslag blokkeren en houdt de lagen testbaar.

### 2. Integratie via `NetworkMessageService`

`NetworkMessageService` stuurt al via `Protocol` naar de juiste verwerkingsketen.
Voor `NMEA0183` berichten wordt de aanroep naar `Nmea0183ParserService` doorgeleid,
analoog aan de bestaande `NetworkMessageParserService` voor NMEA2000.

```
NetworkMessageService
	├─ Protocol == NMEA2000 → NetworkMessageParserService → type-specifieke interpreter
	└─ Protocol == NMEA0183 → Nmea0183ParserService → sentence-specifieke interpreter
```

### 3. Hergebruik van bestaande measurement entities

Bestaande entities worden waar mogelijk hergebruikt.
Alleen als een NMEA 0183 sentence significant andere semantiek heeft, wordt een aparte entity overwogen.

### 4. Raw opslag blijft leidend

Sentences die niet herkenbaar of parsebaar zijn, worden raw opgeslagen en niet verder verwerkt.
Dit is al geborgd door Fase 1.

### 5. Geen NMEA2000 write-back

NMEA 0183-data wordt niet teruggeschreven naar de NMEA2000-keten of de YDEN-03 gateway.

---

## Sentence-prioriteiten (Fase 3)

De volgende volgorde geldt op basis van praktisch nut voor de boot:

### Prioriteit 1 – Directe sensordata

| Sentence | Meetwaarde(n) | Entity | Opmerking |
|----------|---------------|--------|-----------|
| `VHW` | Speed Through Water + Magnetic Heading | `SpeedThroughWaterMeasurement` / `HeadingMeasurement` | Zie open vraag: gecombineerd of gesplitst |
| `DBT` / `DPT` | Diepte | `DepthMeasurement` | `DBT` = below transducer, `DPT` = depth + offset |
| `MTW` | Watertemperatuur | `WaterTemperatureMeasurement` | Eenvoudigste mapping |

### Prioriteit 2 – Navigatiedata

| Sentence | Meetwaarde(n) | Entity | Opmerking |
|----------|---------------|--------|-----------|
| `MWV` | Windsnelheid + Windhoek | `WindMeasurement` | Onderscheid werkelijke/schijnbare wind vastleggen |
| `HDT` | Heading True | `HeadingMeasurement` | Absolute heading, geen magnetische correctie nodig |
| `HDM` | Heading Magnetic | `HeadingMeasurement` | Zelfde entity, ander veld of ander Reference-type |

### Prioriteit 3 – GPS / Positie

| Sentence | Meetwaarde(n) | Entity | Opmerking |
|----------|---------------|--------|-----------|
| `RMC` | Positie, COG, SOG, datum/tijd | `PositionMeasurement` + `MotionMeasurement` | Bevat ook datum/tijd, zie open vraag |
| `GGA` | Positie, fix-kwaliteit, satellietaantal | `PositionMeasurement` | Meer detail dan `RMC`; zie open vraag over prioriteit |

---

## Mapping naar bestaande measurement entities

### `SpeedThroughWaterMeasurement`

- Bron: `VHW` veld 4 (Speed Through Water in knots) of veld 6 (km/h)
- Eenheid: opslaan in m/s, conform bestaande entity
- Conversie nodig: knots → m/s (`× 0.514444`)

### `WindMeasurement`

- Bron: `MWV` velden 1 (angle), 2 (reference: R=relative/apparent, T=true), 3 (speed), 4 (unit)
- Opmerking: NMEA 0183 `MWV` kan zowel werkelijke als schijnbare wind bevatten; dit moet expliciet worden vastgelegd per opgeslagen meting.
- **Open vraag:** hoe wordt onderscheid werkelijk/schijnbaar in de huidige `WindMeasurement` vastgelegd?

### `DepthMeasurement`

- Bron: `DBT` veld 3 (meters below transducer) of `DPT` veld 1 (depth) + veld 2 (offset)
- Voorkeur: `DPT` als dat beschikbaar is (bevat expliciete offsetcorrectie)
- Eenheid: meters, conform bestaande entity

### `PositionMeasurement`

- Bron: `RMC` velden 3+4 (latitude) en 5+6 (longitude), of `GGA` velden 2+3 en 4+5
- Conversie: NMEA `DDMM.MMM` notatie → decimale graden

### `MotionMeasurement`

- Bron: `RMC` veld 7 (SOG in knots) en veld 8 (COG in graden)
- Conversie SOG: knots → centiknots (voor opslag), of direct als double
- Conversie COG: graden → radialen indien entity dat verwacht

### `HeadingMeasurement`

- Bron: `HDT` (True heading, veld 1) of `HDM` (Magnetic heading, veld 1) of `VHW` veld 1/3
- Opmerking: `HDT` en `HDM` hebben beide een headingwaarde maar een ander referentietype.
- **Open vraag:** wordt het reference-type (True/Magnetic) al bijgehouden in `HeadingMeasurement`?
  (Het payload-veld bestaat in PGN 127250 maar wordt nu nog niet gedecodeerd – zie Architecture Known Limitations.)

### `WaterTemperatureMeasurement`

- Bron: `MTW` veld 1 (temperatuur in °C) + veld 2 (eenheid: C)
- Conversie: °C → Kelvin (`+ 273.15`) indien entity in Kelvin opslaat

---

## Nmea0183ParserService – verantwoordelijkheden

De `Nmea0183ParserService` (te implementeren in `BootManager.Application`) heeft de volgende taken:

1. **Ontvangst van raw sentence** als string uit het `NetworkMessage.RawData`-veld.
2. **Checksum-validatie** (optioneel in Fase 2, verplicht in Fase 3).
3. **Sentence-type herkenning:** talker-prefix negeren (bijv. `II`, `GP`, `HE`), sentence-code extraheren (bijv. `VHW`, `RMC`).
4. **Veldextractie:** kommagescheiden velden als string-array beschikbaar stellen.
5. **Routering:** op basis van sentence-code wordt de juiste interpreter aangeroepen.
6. **Fallback:** onbekende sentence-types worden genegeerd (raw is al opgeslagen).

Beoogde interface (indicatief, ter besluitvorming bij implementatie):

```csharp
// Geeft null terug als sentence onbekend of onparseerbaar is
INmea0183ParseResult? Parse(string rawSentence);
```

---

## Sentence-specifieke interpreters

Per sentence-type komt er een interpreter analoog aan de bestaande NMEA2000 interpreters:

| Interpreter | Input | Output |
|-------------|-------|--------|
| `VhwMessageInterpreterService` | `INmea0183ParseResult` (VHW) | `SpeedThroughWaterInterpretationDto` + `HeadingInterpretationDto` |
| `MwvMessageInterpreterService` | `INmea0183ParseResult` (MWV) | `WindInterpretationDto` |
| `DbtMessageInterpreterService` | `INmea0183ParseResult` (DBT/DPT) | `DepthInterpretationDto` |
| `RmcMessageInterpreterService` | `INmea0183ParseResult` (RMC) | `PositionInterpretationDto` + `MotionInterpretationDto` |
| `GgaMessageInterpreterService` | `INmea0183ParseResult` (GGA) | `PositionInterpretationDto` |
| `HdtHdmMessageInterpreterService` | `INmea0183ParseResult` (HDT/HDM) | `HeadingInterpretationDto` |
| `MtwMessageInterpreterService` | `INmea0183ParseResult` (MTW) | `WaterTemperatureInterpretationDto` |

---

## Vaststaande ontwerpkeuzes

| Keuze | Beslissing |
|-------|-----------|
| Parallelle keten | NMEA 0183 loopt parallel aan NMEA2000, geen vervanging |
| Protocolherkenning | Via `NetworkMessage.Protocol` |
| Parser/interpreter scheiding | Strikt gescheiden, conform bestaand patroon |
| Hergebruik entities | Bestaande measurement entities worden hergebruikt |
| Raw opslag | Altijd leidend, ook voor onbekende sentences |
| NMEA2000 slices | Onaangetast |
| Write-back | Buiten scope |
| Simulator | Geen wijzigingen in deze fase |

---

## Open ontwerpvragen

De volgende vragen zijn nog niet beslecht en moeten worden beantwoord vóór of tijdens Fase 3-implementatie.

### 1. `Protocol` als string, enum of aparte tabel?

Huidige situatie: `Protocol` is een string op `NetworkMessage` (`"NMEA2000"` / `"NMEA0183"`).
Open vraag: wordt dit een enum of blijft het een string? Een enum biedt type-safety maar vereist een migratie.

### 2. `Protocol` toevoegen aan measurement entities?

Momenteel staat `Protocol` alleen op `NetworkMessage`.
Open vraag: is het nuttig om measurement entities te taggen met het bronprotocol?
Relevant als NMEA2000 en NMEA 0183 dezelfde measurement-tabel vullen en traceerbaarheid gewenst is.

### 3. `VHW` – gecombineerde of opgesplitste interpretatie?

`VHW` bevat zowel Speed Through Water als Magnetic Heading.
Open vraag: worden beide waarden in één interpreter geproduceerd en apart opgeslagen,
of komen er twee aparte interpreters?
Voorkeur is één interpreter die beide DTO's produceert.

### 4. `RMC` versus `GGA` – welke krijgt prioriteit?

`RMC` bevat positie, COG en SOG in één sentence.
`GGA` bevat meer GPS-detail (fix-kwaliteit, hoogte, satellietaantal).
Open vraag: welke wordt als primaire bron behandeld als beide beschikbaar zijn?

### 5. TCP-ondersteuning

De YDEN-03 biedt ook TCP-output (poort 1456).
TCP-ondersteuning is buiten scope voor Fase 2/3 maar blijft een mogelijke latere uitbreiding.

### 6. Simulator `Both`-modus

De simulator kan later via instellingen zowel NMEA2000 als NMEA 0183 genereren.
Dit is buiten scope voor Fase 2/3.

### 7. Conflict-resolutie NMEA2000 versus NMEA 0183

Als dezelfde meetwaarde (bijv. diepte) binnenkomt via zowel NMEA2000 als NMEA 0183,
worden beide opgeslagen (beide komen in dezelfde measurement-tabel).
Open vraag: is hier een expliciete deduplicatie- of prioriteitsregel nodig, of volstaat
het opslaan van alle waarden met een timestamp en eventueel het `Protocol`-veld?

### 8. Checksum-validatie

NMEA 0183 sentences bevatten een optioneel checksum-veld (`*HH`).
Open vraag: is checksum-validatie verplicht in Fase 2 (parser) of pas in Fase 3 (per interpreter)?

### 9. Windmeting: werkelijk vs. schijnbaar

`MWV` met reference `R` = relative (schijnbaar), `T` = true (werkelijk).
Open vraag: hoe wordt dit onderscheid bijgehouden in `WindMeasurement`?
De huidige NMEA2000 winddata wordt behandeld als werkelijke wind.

---

## Eerstvolgende implementatie-story (Fase 2)

> **Story:** Implementeer `Nmea0183ParserService` in `BootManager.Application`

**Scope:**
- Nieuwe service `Nmea0183ParserService` in `BootManager.Application` (feature-map: `Nmea0183`).
- Service herkent sentence-type op basis van `NetworkMessage.RawData`.
- Service extraheert velden als string-array.
- Service retourneert een generiek parse-resultaat of `null` voor onbekende sentences.
- `NetworkMessageService` roept `Nmea0183ParserService` aan als `Protocol == NMEA0183`.
- Geen measurement-opslag in deze story; alleen parsing en logging.
- Checksum-validatie optioneel in deze fase.
- Geen simulator-aanpassingen.
- Geen migrations.

**Acceptatiecriteria:**
- Een raw `NMEA0183`-bericht met een bekende sentence-code (bijv. `VHW`) wordt correct herkend en geveldextraheerd.
- Een onbekende sentence-code wordt gelogd en niet verder verwerkt.
- Bestaande NMEA2000-flow blijft ongewijzigd.
- Build slaagt zonder fouten.

---

## Gerelateerde documenten

- [nmea0183-support.md](../epics/nmea0183-support.md) – Epic overzicht
- [ARCHITECTURE.md](../ARCHITECTURE.md) – Architectuuroverzicht
- [TODO.md](../TODO.md) – Backlog en status
- [features/README.md](README.md) – Feature specifications index
- [yden-03.md](../extraInfo/yden-03.md) – Hardware context

---

*Aangemaakt: 2026-05-17*
