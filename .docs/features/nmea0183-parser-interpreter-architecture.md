# NMEA 0183 Parser/Interpreter Architectuur

**Datum:** 2026-05-17  
**Status:** Fase 2, Fase 3a, Fase 3b en Fase 3c geïmplementeerd
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

### Fase 2 – Geïmplementeerd (2026-05-17)

De volgende klassen zijn toegevoegd aan `BootManager.Application`:

- `Nmea0183ParseResultDto` – technisch parse-resultaat met `TalkerPrefix`, `SentenceType`, `Fields`, `ChecksumValid`, `ErrorMessage`
- `INmea0183ParserService` – interface
- `Nmea0183ParserService` – implementatie: talker-prefix herkenning, veldextractie, XOR-checksum validatie (optioneel)
- `NetworkMessageService` roept `Nmea0183ParserService` aan als `Protocol == "NMEA0183"` – parse-fouten blokkeren raw opslag niet

### Fase 3a – Geïmplementeerd (2026-05-17)

Sentence-specifieke interpreters voor VHW, MTW en DBT/DPT toegevoegd aan `BootManager.Application`:

- `INmea0183MessageInterpreter<T>` – generiek interface voor NMEA 0183 sentence-interpreters (accepteert `Nmea0183ParseResultDto`)
- `Nmea0183VhwInterpreterService` – VHW → `SpeedThroughWaterMessageInterpretationDto` (knoten als primaire bron, fallback km/h; opgeslagen in m/s + knoten)
- `Nmea0183MtwInterpreterService` – MTW → `WaterTemperatureMessageInterpretationDto` (Celsius parse, Kelvin afgeleid via +273.15)
- `Nmea0183DbtDptInterpreterService` – DBT/DPT → `DepthMessageInterpretationDto` (meters prefereren; voor DBT fallback voet → meter; DPT veld [0] direct in meters)
- `NetworkMessageService` roept de drie interpreters sequentieel aan na geslaagde NMEA0183-parse – fouten per interpreter blokkeren raw opslag niet
- DI-registraties toegevoegd in `DependencyInjection.cs`

### Fase 3b – Geïmplementeerd (2026-05-17)

Sentence-specifieke interpreters voor MWV, HDT en HDM toegevoegd aan `BootManager.Application`:

- `Nmea0183MwvInterpreterService` – MWV → `WindMessageInterpretationDto` (windhoek + windsnelheid; eenheden K/M/N omgezet naar m/s; alleen status A levert geldige interpretatie)
- `Nmea0183HdtHdmInterpreterService` – HDT/HDM → `HeadingMessageInterpretationDto` (koers in graden)
- `NetworkMessageService` roept beide interpreters sequentieel aan na geslaagde NMEA0183-parse
- DI-registraties toegevoegd in `DependencyInjection.cs`

### Fase 3c – Geïmplementeerd (2026-05-17)

Sentence-specifieke interpreters voor RMC en GGA toegevoegd aan `BootManager.Application`:

- `Nmea0183RmcInterpretationDto` – gecombineerd DTO voor positie + motion uit RMC
- `Nmea0183RmcInterpreterService` – RMC → `PositionMeasurement` en/of `MotionMeasurement`
  - Checksumbeleid: `ChecksumValid == false` → geen interpretatie
  - Alleen bij status `A`
  - Positie (lat/lon) en motion (SOG/COG) worden onafhankelijk opgeslagen indien geldig
  - NMEA ddmm.mmmm/dddmm.mmmm geconverteerd naar decimale graden
- `Nmea0183GgaInterpreterService` – GGA → `PositionMeasurement`
  - Checksumbeleid: `ChecksumValid == false` → geen interpretatie
  - Alleen bij fixkwaliteit > 0
  - Hoogte, satellieten en HDOP niet opgeslagen in fase 3c
- `NetworkMessageService` roept beide interpreters sequentieel aan na geslaagde NMEA0183-parse
- DI-registraties toegevoegd in `DependencyInjection.cs`

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

## Nmea0183ParserService – geïmplementeerde verantwoordelijkheden

De `Nmea0183ParserService` (geïmplementeerd in `BootManager.Application/NetworkMessageParsing/Services`) voert de volgende taken uit:

1. **Ontvangst van raw sentence** als string.
2. **Structuurvalidatie:** sentence moet beginnen met `$`.
3. **Checksum-validatie** (optioneel – aanwezig als `*HH` suffix; gevalideerd via XOR).
4. **Sentence-type herkenning:** talker-prefix negeren (bijv. `II`, `GP`, `HE`), sentence-code extraheren (bijv. `VHW`, `RMC`).
5. **Veldextractie:** kommagescheiden velden als `IReadOnlyList<string>` beschikbaar stellen.
6. **Fallback:** onbekende of onparseerbare sentence-types leveren `IsSuccess = false` op; raw opslag blijft leidend.

Geïmplementeerde interface:

```csharp
public interface INmea0183ParserService
{
    Nmea0183ParseResultDto Parse(string rawSentence);
}
```

`Nmea0183ParseResultDto` bevat:
- `bool IsSuccess`
- `string RawSentence`
- `string TalkerPrefix`
- `string SentenceType`
- `IReadOnlyList<string> Fields`
- `bool? ChecksumValid`
- `string ErrorMessage`

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

### 6. Simulator outputmodus

De simulator ondersteunt drie configureerbare outputmodi via `Simulator:OutputMode` in `appsettings.json`:
- `NMEA2000` – bestaande NMEA2000-achtige raw output (standaard)
- `NMEA0183` – NMEA 0183 sentences voor alle fase 3a-3c types
- `Both` – beide stromen tegelijk, elk met eigen runtime state vanuit hetzelfde scenario

Bij `Both` zijn de waarden **scenario-consistent** maar **niet exact tick-gesynchroniseerd**;
elke stream heeft zijn eigen tick-loop en random variaties. Geïmplementeerd in de simulator NMEA 0183 output story (2026-05-18).

### 7. Conflict-resolutie NMEA2000 versus NMEA 0183

Als dezelfde meetwaarde (bijv. diepte) binnenkomt via zowel NMEA2000 als NMEA 0183,
worden beide opgeslagen (beide komen in dezelfde measurement-tabel).
Open vraag: is hier een expliciete deduplicatie- of prioriteitsregel nodig, of volstaat
het opslaan van alle waarden met een timestamp en eventueel het `Protocol`-veld?

### 8. Checksum-validatie

NMEA 0183 sentences bevatten een optioneel checksum-veld (`*HH`).
**Besloten:** checksum-validatie is geïmplementeerd als optioneel in Fase 2 (aanwezig indien `*HH` aanwezig is).
Open vraag voor Fase 3: moet een ontbrekende of foute checksum een sentence afwijzen of alleen loggen?

### 9. Windmeting: werkelijk vs. schijnbaar

`MWV` met reference `R` = relative (schijnbaar), `T` = true (werkelijk).
Open vraag: hoe wordt dit onderscheid bijgehouden in `WindMeasurement`?
De huidige NMEA2000 winddata wordt behandeld als werkelijke wind.

---

## Status Fase 3 – Geïmplementeerd

Alle sentence-specifieke interpreters zijn geïmplementeerd (Fase 3a t/m 3c, 2026-05-17):

| Sentence | Meetwaarde(n) | Doelentity | Status |
|----------|---------------|------------|--------|
| `VHW` | Speed Through Water + Heading | `SpeedThroughWaterMeasurement` | ✅ |
| `DBT` / `DPT` | Diepte | `DepthMeasurement` | ✅ |
| `MTW` | Watertemperatuur | `WaterTemperatureMeasurement` | ✅ |
| `MWV` | Windsnelheid + Windhoek | `WindMeasurement` | ✅ |
| `HDT` / `HDM` | Heading True/Magnetic | `HeadingMeasurement` | ✅ |
| `RMC` | Positie + COG/SOG | `PositionMeasurement` + `MotionMeasurement` | ✅ |
| `GGA` | Positie | `PositionMeasurement` | ✅ |

## Eerstvolgende stap

Runtime/SQLite acceptatietest uitvoeren via de simulator in `NMEA0183`-modus.
Zie `docs/bootmanager_codex_handoff.md` sectie 16 voor startcommando's en verwachte tabelinhoud.

---

## Gerelateerde documenten

- [nmea0183-support.md](../epics/nmea0183-support.md) – Epic overzicht
- [ARCHITECTURE.md](../ARCHITECTURE.md) – Architectuuroverzicht
- [TODO.md](../TODO.md) – Backlog en status
- [features/README.md](README.md) – Feature specifications index
- [yden-03.md](../extraInfo/yden-03.md) – Hardware context

---

*Aangemaakt: 2026-05-17*
