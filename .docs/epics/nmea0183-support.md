# Epic: NMEA 0183 Support

**Datum:** 2026-05-17  
**Status:** Fase 1 geïmplementeerd

---

## Aanleiding

De fysieke boot gebruikt een **YDEN-03 gateway** om NMEA 2000-data naar het netwerk te brengen.
In de huidige configuratie van de YDEN-03 wordt de output als **NMEA 0183** verzonden, niet als raw NMEA 2000.

Dit betekent dat BootManager NMEA 0183 sentences moet kunnen ontvangen, opslaan en later verwerken
als het systeem gekoppeld wordt aan de echte bootinfrastructuur.

Zie ook: [YDEN-03 configuratie](./../extraInfo/yden-03.md)

---

## Doel van deze epic

- NMEA 0183 als parallelle inputstroom naast de bestaande NMEA2000/raw-like flow ondersteunen.
- Raw NMEA 0183 sentences altijd bewaren, ook als ze (nog) niet parseerbaar zijn.
- Per NMEA 0183 sentence-type later interpreters en measurement-opslag toevoegen.
- De bestaande NMEA2000 slices en keten ongewijzigd laten.

---

## Architectuurkeuzes

### Parallelle input, niet vervanging

NMEA 0183 wordt **naast** de bestaande NMEA2000-keten ondersteund.
De bestaande verticale slices (Battery, Depth, Wind, Motion, Position, Heading, SpeedThroughWater, WaterTemperature)
blijven intact en ongewijzigd.

### Ingest blijft transportlaag

- Ingest ontvangt UDP-datagrammen en stuurt ze door – geen semantische logica.
- Ingest hoeft geen YDEN-IP te kennen. Luisteren op `0.0.0.0` of een configureerbaar lokaal adres volstaat.
- De remote endpoint mag als `Source` worden vastgelegd op het `NetworkMessage`.
- Ingest krijgt uiteindelijk **twee configureerbare UDP listeners**:
  - één endpoint voor NMEA2000/raw-like input (bestaand)
  - één endpoint voor NMEA0183 sentence input (nieuw)

### Protocol tagging

Raw berichten worden getagd met een `Protocol`-veld op `NetworkMessage`.
Zo kan downstream-logica onderscheid maken tussen NMEA2000 en NMEA0183 berichten.

### Geen protocol-uitbreiding op measurement entities

Het veld `Protocol` blijft op `NetworkMessages` staan.
Het toevoegen van `Protocol` aan alle measurement entities is een **expliciete latere ontwerpkeuze**,
niet automatisch nu ingevoerd.

### Simulator kiest later via settings

De simulator krijgt later een instelling om te kiezen welk outputformaat wordt gebruikt:
- `NMEA2000` (huidige standaard)
- `NMEA0183` (nieuw te implementeren)
- Eventueel later: `Both` (buiten scope eerste codefase)

### Schrijven naar NMEA2000 is buiten scope

Vanuit BootManager terugschrijven naar NMEA2000 of de YDEN-03 is buiten scope voor deze epic.
Dit is een mogelijk toekomstig onderwerp (versie 2/3).

---

## Gefaseerde aanpak

### Fase 1 – NMEA 0183 Ingest Foundation ✅ *Geïmplementeerd – 2026-05-17*

**Doel:** NMEA 0183 sentences ontvangen, protocol taggen en raw opslaan.

**Scope:**
- Tweede configureerbare UDP listener (`Nmea0183IngestService`) toegevoegd aan Ingest naast de bestaande NMEA2000 listener.
- Protocol tagging op `NetworkMessage` – onderscheid `NMEA2000` / `NMEA0183`.
- Raw NMEA 0183 sentences opgeslagen in de bestaande `NetworkMessages`-tabel.
- Geen verplichte semantische measurement-opslag in deze fase.
- Onbekende of niet-parsebare NMEA 0183 sentences worden raw opgeslagen en niet verder verwerkt.
- TCP is buiten scope gebleven in deze fase.

**Poortkeuze:**
- NMEA2000/raw-like: `127.0.0.1:2000` (bestaand, ongewijzigd)
- NMEA0183: `0.0.0.0:10110` (nieuw, configureerbaar)

**Gewijzigde bestanden:**
- `src/BootManager.Tools.Ingest/Options/IngestOptions.cs` – `Nmea0183ListenerOptions` sub-object toegevoegd
- `src/BootManager.Tools.Ingest/Services/Nmea0183IngestService.cs` – nieuw: UDP listener voor NMEA 0183
- `src/BootManager.Tools.Ingest/Services/IngestService.cs` – protocol-tag gewijzigd van `YdenRawLike` naar `NMEA2000`
- `src/BootManager.Tools.Ingest/appsettings.json` – NMEA 0183 endpoint toegevoegd
- `src/BootManager.Tools.Ingest/Program.cs` – `Nmea0183IngestService` geregistreerd

**Acceptatiecriteria (voldaan):**
- Ingest luistert op twee configureerbare UDP endpoints.
- Ontvangen NMEA 0183 sentences komen raw terecht in de `NetworkMessages`-tabel.
- Protocol-tagging is zichtbaar in opgeslagen berichten.
- Bestaande NMEA2000-flow blijft ongewijzigd werken.

---

### Fase 2 – NMEA 0183 Parser laag

**Doel:** Aparte parser- en interpreterlijn voor NMEA 0183 sentences toevoegen aan `BootManager.Application`.

**Scope:**
- `Nmea0183ParserService` in `BootManager.Application` (feature-map: `Nmea0183`):
  - Ontvangt raw sentence-string uit `NetworkMessage.RawData`.
  - Valideert structuur (optioneel: checksum-validatie).
  - Herkent sentence-type door talker-prefix te negeren en sentence-code te extraheren.
  - Extraheert kommagescheiden velden als string-array.
  - Retourneert generiek parse-resultaat of `null` voor onbekende sentences.
- `NetworkMessageService` roept `Nmea0183ParserService` aan als `Protocol == NMEA0183`.
- Onbekende of onparseerbare sentences worden gelogd; raw opslag is al geborgd in Fase 1.
- Nog geen measurement-opslag per sensortype in deze fase.

**Acceptatiecriteria:**
- Een raw `NMEA0183`-bericht met een bekende sentence-code (bijv. `VHW`) wordt correct herkend en geveldextraheerd.
- Een onbekende sentence-code wordt gelogd en niet verder verwerkt.
- Bestaande NMEA2000-flow blijft ongewijzigd.
- Build slaagt zonder fouten.

**Ontwerpdetails:**
Zie: [.docs/features/nmea0183-parser-interpreter-architecture.md](./../features/nmea0183-parser-interpreter-architecture.md)

---

### Fase 3 – Sentence-specifieke interpreters en measurement-opslag

Per NMEA 0183 sentence-type een verticale slice toevoegen, analoog aan de bestaande NMEA2000 slices.

Kandidaat-sentences (volgorde op basis van prioriteit):

| Sentence | Meetwaarde(n) | Mapping naar bestaande entity |
|----------|---------------|-------------------------------|
| `VHW` | Speed Through Water + Magnetic Heading | `SpeedThroughWaterMeasurement` / `HeadingMeasurement` |
| `MWV` | Wind Speed + Wind Angle | `WindMeasurement` |
| `DBT` / `DPT` | Diepte | `DepthMeasurement` |
| `RMC` / `GGA` | Positie (lat/lon), COG, SOG | `PositionMeasurement` / `MotionMeasurement` |
| `HDT` / `HDM` | Heading True/Magnetic | `HeadingMeasurement` |
| `MTW` | Watertemperatuur | `WaterTemperatureMeasurement` |

**Principe:** Bestaande measurement entities zijn herbruikbaar. Alleen als een NMEA 0183 sentence
significant andere semantiek heeft, wordt een aparte entity overwogen.

---

### Mogelijke latere fase – Simulator NMEA 0183 output

- Simulator kan via instellingen NMEA 0183 sentences genereren.
- Gebruik voor testing en integratie zonder echte hardware.

---

## Open ontwerpvragen

De volgende vragen staan nog open en moeten worden beantwoord vóór of tijdens Fase 3-implementatie.
Zie ook: [nmea0183-parser-interpreter-architecture.md](./../features/nmea0183-parser-interpreter-architecture.md)

| Vraag | Status |
|-------|--------|
| `Protocol` als string, enum of aparte tabel? | Open |
| `Protocol` toevoegen aan measurement entities? | Open |
| `VHW` – gecombineerde of opgesplitste interpretatie? | Open |
| `RMC` versus `GGA` – welke heeft prioriteit als primaire positiebron? | Open |
| TCP-ondersteuning (YDEN-03 poort 1456) | Buiten scope Fase 2/3 |
| Simulator `Both`-modus | Buiten scope Fase 2/3 |
| Conflict-resolutie NMEA2000 versus NMEA 0183 bij dubbele metingen | Open |
| Checksum-validatie verplicht in Fase 2 of pas Fase 3? | Open |
| `MWV` windmeting: onderscheid werkelijk/schijnbaar in `WindMeasurement` | Open |

---

## Raw opslag als leidend principe

Ook in de NMEA 0183 keten geldt: **raw opslag altijd, parsing optioneel**.

Berichten die niet parseerbaar zijn of waarvoor nog geen interpreter bestaat,
worden raw opgeslagen in `NetworkMessages` en kunnen later alsnog verwerkt worden.

---

## Gerelateerde documenten

- [YDEN-03 configuratie](./../extraInfo/yden-03.md)
- [ARCHITECTURE.md](./../ARCHITECTURE.md)
- [TODO.md](./../TODO.md)
- [features/README.md](./../features/README.md)

---

*Aangemaakt: 2026-05-17*
