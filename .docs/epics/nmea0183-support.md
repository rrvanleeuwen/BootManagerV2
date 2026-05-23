# Epic: NMEA 0183 Support

**Datum:** 2026-05-17  
**Status:** Fase 1, Fase 2, Fase 3a, Fase 3b en Fase 3c geïmplementeerd

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
- Ingest heeft **één gecombineerde UDP listener** op een configureerbaar endpoint (`Ingest:ListenAddress`/`Ingest:ListenPort`).
  - Protocoldetectie vindt plaats op basis van regelinhoud: regels die beginnen met `$` zijn NMEA 0183, overige regels zijn NMEA 2000/raw-like.
  - Dit voorkomt dubbele verwerking wanneer de YDEN dezelfde data op meerdere UDP-poorten uitzendt.
  - Aanbevolen poort: `10110` (standaard NMEA 0183 UDP-poort). Alternatief: `2000` als de YDEN op die poort is geconfigureerd.
  - Luister **niet** tegelijk op `2000` én `10110` om dubbele YDEN-opslag te voorkomen.

### Protocol tagging

Raw berichten worden getagd met een `Protocol`-veld op `NetworkMessage`.
Zo kan downstream-logica onderscheid maken tussen NMEA2000 en NMEA0183 berichten.

### Geen protocol-uitbreiding op measurement entities

Het veld `Protocol` blijft op `NetworkMessages` staan.
Het toevoegen van `Protocol` aan alle measurement entities is een **expliciete latere ontwerpkeuze**,
niet automatisch nu ingevoerd.

### Simulator outputmodus

De simulator ondersteunt drie configureerbare outputmodi via `Simulator:OutputMode` in `appsettings.json`:
- `NMEA2000` – bestaande NMEA2000-achtige raw output (standaard)
- `NMEA0183` – NMEA 0183 sentences voor alle fase 3a-3c types
- `Both` – beide stromen tegelijk; scenario-consistent maar niet exact tick-gesynchroniseerd

Geïmplementeerd in de simulator NMEA 0183 output story (2026-05-18).
Zie `docs/bootmanager_codex_handoff.md` sectie 16 voor startcommando's.

### Schrijven naar NMEA2000 is buiten scope

Vanuit BootManager terugschrijven naar NMEA2000 of de YDEN-03 is buiten scope voor deze epic.
Dit is een mogelijk toekomstig onderwerp (versie 2/3).

---

## Gefaseerde aanpak

### Fase 1 – NMEA 0183 Ingest Foundation ✅ *Geïmplementeerd – 2026-05-17, herzien naar gecombineerde listener*

**Doel:** NMEA 0183 sentences ontvangen, protocol taggen en raw opslaan via één gecombineerde UDP-listener.

**Scope:**
- Één gecombineerde UDP listener (`IngestService`) verwerkt zowel NMEA 2000/raw-like als NMEA 0183 regels.
- Protocoldetectie op regelinhoud: regels die beginnen met `$` → `NMEA0183`, overig → `NMEA2000`.
- Raw NMEA 0183 sentences opgeslagen in de bestaande `NetworkMessages`-tabel.
- Geen verplichte semantische measurement-opslag in deze fase.
- Onbekende of niet-parsebare NMEA 0183 sentences worden raw opgeslagen en niet verder verwerkt.
- TCP is buiten scope gebleven.

**Poortkeuze:**
- Eén gecombineerd endpoint: `0.0.0.0:10110` (aanbevolen standaard)
- Alternatief: `0.0.0.0:2000` als de YDEN op die poort is geconfigureerd
- Niet tegelijk op beide poorten luisteren om dubbele YDEN-verwerking te voorkomen

**Gewijzigde bestanden:**
- `src/BootManager.Tools.Ingest/Options/IngestOptions.cs` – `Nmea0183ListenerOptions` verwijderd; defaults bijgewerkt naar `0.0.0.0:10110`
- `src/BootManager.Tools.Ingest/Services/Nmea0183IngestService.cs` – verwijderd (samengevoegd in `IngestService`)
- `src/BootManager.Tools.Ingest/Services/IngestService.cs` – NMEA 0183 detectie toegevoegd; source = remote endpoint
- `src/BootManager.Tools.Ingest/appsettings.json` – Nmea0183-sectie verwijderd
- `src/BootManager.Tools.Ingest/Program.cs` – `Nmea0183IngestService` registratie verwijderd

**Acceptatiecriteria (voldaan):**
- Ingest luistert op één configureerbaar UDP endpoint.
- NMEA 0183 sentences (beginnen met `$`) worden als `Protocol = "NMEA0183"` doorgestuurd.
- NMEA 2000/raw-like regels worden als `Protocol = "NMEA2000"` doorgestuurd.
- Geen dubbele verwerking van YDEN-output.

---

### Fase 2 – NMEA 0183 Parser laag ✅ *Geïmplementeerd – 2026-05-17*

**Doel:** Aparte parserlaag voor NMEA 0183 sentences toevoegen aan `BootManager.Application`.

**Scope:**
- `Nmea0183ParserService` toegevoegd aan `BootManager.Application` (map: `NetworkMessageParsing`):
  - Ontvangt raw sentence-string.
  - Valideert structuur en optionele XOR-checksum.
  - Herkent sentence-type door talker-prefix te negeren en sentence-code te extraheren.
  - Extraheert kommagescheiden velden als string-array.
  - Retourneert `Nmea0183ParseResultDto` inclusief `TalkerPrefix`, `SentenceType`, `Fields`, `ChecksumValid`, `ErrorMessage`.
- `NetworkMessageService` roept `Nmea0183ParserService` aan als `Protocol == "NMEA0183"`.
- Onbekende of onparseerbare sentences worden gelogd; raw opslag is niet geblokkeerd.
- Nog geen measurement-opslag per sensortype in deze fase.
- Geen EF migrations.

**Gewijzigde bestanden:**
- `BootManager.Application/NetworkMessageParsing/DTOs/Nmea0183ParseResultDto.cs` – nieuw
- `BootManager.Application/NetworkMessageParsing/Services/INmea0183ParserService.cs` – nieuw
- `BootManager.Application/NetworkMessageParsing/Services/Nmea0183ParserService.cs` – nieuw
- `BootManager.Application/NetworkMessages/Services/NetworkMessageService.cs` – `INmea0183ParserService` geïnjecteerd, aanroep voor NMEA0183
- `BootManager.Application/DependencyInjection.cs` – DI registratie toegevoegd

**Acceptatiecriteria (voldaan):**
- `$IIVHW,...`, `$GPRMC,...`, `$WIMWV,...` leveren herkend TalkerPrefix + SentenceType op in parse-resultaat.
- Ongeldige of onbekende sentences blokkeren de raw message flow niet.
- Bestaande NMEA2000-flow blijft ongewijzigd.
- Build slaagt (`dotnet build` ✅). Unit tests: `dotnet test` geslaagd (1 niet-gerelateerde authenticatietest gefaald, pre-existent).
- Runtime-tests zijn niet uitgevoerd.

**Ontwerpdetails:**
Zie: [.docs/features/nmea0183-parser-interpreter-architecture.md](./../features/nmea0183-parser-interpreter-architecture.md)

---

### Fase 3 – Sentence-specifieke interpreters en measurement-opslag

Per NMEA 0183 sentence-type een verticale slice toevoegen, analoog aan de bestaande NMEA2000 slices.

#### Fase 3a – VHW, MTW, DBT/DPT ✅ *Geïmplementeerd – 2026-05-17*

**Scope:**
- `INmea0183MessageInterpreter<T>` interface toegevoegd in `NetworkMessageInterpretation.Contracts`.
- `Nmea0183VhwInterpreterService` – VHW sentence → `SpeedThroughWaterMeasurement` (knoten + m/s, fallback km/h).
- `Nmea0183MtwInterpreterService` – MTW sentence → `WaterTemperatureMeasurement` (Celsius + Kelvin).
- `Nmea0183DbtDptInterpreterService` – DBT/DPT sentence → `DepthMeasurement` (meters, fallback voet).
- `NetworkMessageService` roept de drie interpreters aan in het NMEA0183-blok; fouten blokkeren raw opslag niet.
- DI-registratie toegevoegd in `DependencyInjection.cs`.

**Gewijzigde/toegevoegde bestanden:**
- `BootManager.Application/NetworkMessageInterpretation/Contracts/INmea0183MessageInterpreter.cs` – nieuw
- `BootManager.Application/NetworkMessageInterpretation/Services/Nmea0183VhwInterpreterService.cs` – nieuw
- `BootManager.Application/NetworkMessageInterpretation/Services/Nmea0183MtwInterpreterService.cs` – nieuw
- `BootManager.Application/NetworkMessageInterpretation/Services/Nmea0183DbtDptInterpreterService.cs` – nieuw
- `BootManager.Application/NetworkMessages/Services/NetworkMessageService.cs` – NMEA0183-blok uitgebreid
- `BootManager.Application/DependencyInjection.cs` – drie DI-registraties toegevoegd

**Acceptatiecriteria (voldaan):**
- Build slaagt (`dotnet build` ✅).
- Voor `Protocol == NMEA0183` met VHW/MTW/DBT/DPT sentences worden measurement records opgeslagen via bestaande services.
- Onbekende of ongeldige NMEA0183 sentences blokkeren raw opslag niet.
- Bestaande NMEA2000-gedrag is intact.
- Runtime-tests zijn niet uitgevoerd.

#### Fase 3b – MWV, HDT/HDM ✅ *Geïmplementeerd – 2026-05-17*

**Scope:**
- `Nmea0183MwvInterpreterService` – MWV sentence → `WindMeasurement` (windhoek + windsnelheid; eenheden K/M/N omgezet naar m/s; alleen status A opgeslagen).
- `Nmea0183HdtHdmInterpreterService` – HDT/HDM sentence → `HeadingMeasurement` (koers in graden).
- `NetworkMessageService` roept beide interpreters aan in het NMEA0183-blok; fouten blokkeren raw opslag niet.
- DI-registratie toegevoegd in `DependencyInjection.cs`.

**Gewijzigde/toegevoegde bestanden:**
- `BootManager.Application/NetworkMessageInterpretation/Services/Nmea0183MwvInterpreterService.cs` – nieuw
- `BootManager.Application/NetworkMessageInterpretation/Services/Nmea0183HdtHdmInterpreterService.cs` – nieuw
- `BootManager.Application/NetworkMessages/Services/NetworkMessageService.cs` – NMEA0183-blok uitgebreid met MWV en HDT/HDM
- `BootManager.Application/DependencyInjection.cs` – twee DI-registraties toegevoegd

**Acceptatiecriteria (voldaan):**
- Build slaagt (`dotnet build` ✅).
- Voor `Protocol == NMEA0183` met MWV (status A) wordt een `WindMeasurement` opgeslagen.
- Voor `Protocol == NMEA0183` met HDT of HDM wordt een `HeadingMeasurement` opgeslagen.
- MWV met ongeldige checksum of status V levert geen meting op, maar raw opslag is intact.
- Bestaande Fase 3a-interpreters (VHW/MTW/DBT/DPT) en NMEA2000-gedrag zijn intact.

#### Fase 3c – RMC, GGA ✅ *Geïmplementeerd – 2026-05-17*

**Scope:**
- `Nmea0183RmcInterpretationDto` – gecombineerd DTO voor positie + motion afgeleid uit RMC.
- `Nmea0183RmcInterpreterService` – RMC sentence → `PositionMeasurement` en/of `MotionMeasurement`.
  - Alleen bij status `A`; checksum `false` levert geen interpretatie.
  - Positie (lat/lon) en motion (SOG knoten + COG graden) onafhankelijk opgeslagen als geldig.
  - NMEA ddmm.mmmm/dddmm.mmmm geconverteerd naar decimale graden.
- `Nmea0183GgaInterpreterService` – GGA sentence → `PositionMeasurement`.
  - Alleen bij fixkwaliteit > 0; checksum `false` levert geen interpretatie.
  - Hoogte, satellieten en HDOP worden in deze stap niet opgeslagen.
- `NetworkMessageService` roept beide interpreters sequentieel aan na geslaagde NMEA0183-parse; fouten blokkeren raw opslag niet.
- DI-registratie toegevoegd in `DependencyInjection.cs`.

**Gewijzigde/toegevoegde bestanden:**
- `BootManager.Application/NetworkMessageInterpretation/DTOs/Nmea0183RmcInterpretationDto.cs` – nieuw
- `BootManager.Application/NetworkMessageInterpretation/Services/Nmea0183RmcInterpreterService.cs` – nieuw
- `BootManager.Application/NetworkMessageInterpretation/Services/Nmea0183GgaInterpreterService.cs` – nieuw
- `BootManager.Application/NetworkMessages/Services/NetworkMessageService.cs` – NMEA0183-blok uitgebreid met RMC en GGA
- `BootManager.Application/DependencyInjection.cs` – twee DI-registraties toegevoegd

**Acceptatiecriteria (voldaan):**
- Build slaagt (`dotnet build` ✅).
- RMC met geldige checksum en status `A` slaat `PositionMeasurement` op.
- RMC met geldige SOG/COG slaat `MotionMeasurement` op.
- GGA met fixkwaliteit > 0 slaat `PositionMeasurement` op.
- RMC/GGA met `ChecksumValid == false` levert geen meting op; raw opslag intact.
- Ongeldige RMC-status of GGA-fixkwaliteit levert geen meting op; raw opslag intact.
- Bestaande Fase 3a/3b-interpreters en NMEA2000-gedrag zijn intact.

**Status:** ~~Runtime/SQLite acceptatietest voor NMEA 0183 fase 3a-3c~~ ✅ Afgerond (2026-05-18, handmatig via simulator NMEA0183-modus). TCP-ondersteuning voor YDEN-03 poort 1456 is voorlopig geparkeerd; UDP volstaat voor BootManager.

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

### Simulator NMEA 0183 output ✅ *Geïmplementeerd – 2026-05-18*

De simulator ondersteunt `NMEA0183`- en `Both`-modus. Alle fase 3a-3c sentence-types worden gegenereerd met correcte XOR-checksum. Zie `docs/bootmanager_codex_handoff.md` sectie 16.

---

## Nieuwe user stories na echte boot-test – 2026-05-23

De echte YDEN-03 capture `ingest-capture-20260523-093220.ndjson` bevestigt dat UDP ontvangst en raw opslag werken, maar toont ook drie concrete gaten:

- AIS sentences met `!AIVDM` / `!AIVDO` worden foutief als `NMEA2000` gelabeld.
- NMEA 0183-derived measurements worden niet opgeslagen omdat NMEA 0183 requests geen bruikbare `MessageId` hebben.
- De simulator mist nog echte YDEN-achtige variatie zoals AIS `!` sentences, `YD` talker-prefixen en extra raw-only sentences.

### Story 1 – Correcte NMEA 0183 protocolherkenning voor `$` en `!`

**Als** ontwikkelaar/operator  
**wil ik** dat Ingest zowel `$...` als `!...` sentences als NMEA 0183 herkent  
**zodat** echte YDEN-03 output inclusief AIS niet meer in de NMEA2000/raw-like keten terechtkomt.

**Scope**
- `IngestService` protocolherkenning uitbreiden: regels met `$` of `!` zijn `Protocol = "NMEA0183"`.
- Raw simulator/NMEA2000-regels zonder `$` of `!` blijven `Protocol = "NMEA2000"`.
- Capture logging blijft hetzelfde NDJSON-formaat gebruiken.

**Acceptatiecriteria**
- `$YDGGA,...` wordt als `NMEA0183` doorgestuurd.
- `!AIVDM,...` en `!AIVDO,...` worden als `NMEA0183` doorgestuurd.
- Een bestaande raw-like simulatorregel blijft `NMEA2000`.
- Unit tests dekken `$`, `!` en raw-like regels.
- Replay of nieuwe boot-test toont geen `!AIVDM` / `!AIVDO` records meer met `Protocol = "NMEA2000"`.

### Story 2 – NMEA 0183 parser accepteert AIS-startteken `!`

**Als** systeem  
**wil ik** NMEA 0183 sentences kunnen parsen die met `$` of `!` beginnen  
**zodat** AIS en andere `!`-sentences technisch correct als NMEA 0183 verwerkt kunnen worden.

**Scope**
- `Nmea0183ParserService` accepteert `$` en `!` als geldig startteken.
- Checksumberekening blijft XOR over de body tussen startteken en `*`.
- Talker/type extractie blijft gelijk: `!AIVDM` -> talker `AI`, type `VDM`.
- Deze story decodeert AIS payloads nog niet semantisch.

**Acceptatiecriteria**
- `$YDGGA,...*HH` parsed succesvol als talker `YD`, type `GGA`.
- `!AIVDM,...*HH` parsed succesvol als talker `AI`, type `VDM`.
- Ongeldige checksum blijft `ChecksumValid = false`.
- Een sentence zonder `$` of `!` wordt afgewezen.
- Bestaande NMEA 0183 parser/interpreter tests blijven slagen.

### Story 3 – Bruikbare `MessageId` voor NMEA 0183-derived measurements

**Als** ontwikkelaar  
**wil ik** dat NMEA 0183 berichten een stabiele `MessageId` krijgen  
**zodat** afgeleide measurements kunnen worden opgeslagen zonder de bestaande measurementvalidatie te versoepelen.

**Scope**
- Ingest of Application bepaalt voor NMEA 0183 een `MessageId` uit sentence-id.
- Voorbeelden:
  - `$YDGGA,...` -> `YDGGA`
  - `$YDMWV,...` -> `YDMWV`
  - `!AIVDM,...` -> `AIVDM`
- `PayloadHex` blijft `null` voor NMEA 0183.
- Measurement services blijven een niet-lege `MessageId` eisen.

**Acceptatiecriteria**
- Nieuwe `NetworkMessages` voor NMEA 0183 hebben een niet-lege `MessageId`.
- `GGA`/`RMC` slaan `PositionMeasurements` op.
- `RMC` slaat `MotionMeasurements` op.
- `MWV` slaat `WindMeasurements` op.
- `HDT`/`HDM` slaan `HeadingMeasurements` op.
- `MTW` slaat `WaterTemperatureMeasurements` op.
- `VHW` slaat `SpeedThroughWaterMeasurements` op.
- Geen `DepthMeasurements` worden verwacht zolang de bron geen `DBT` of `DPT` stuurt.

### Story 4 – Simulator realistischer maken op basis van echte YDEN-capture

**Als** ontwikkelaar die niet op de boot is  
**wil ik** dat de simulator representatieve YDEN-achtige NMEA 0183 output kan genereren  
**zodat** ingest, parser en SQLite-verwerking lokaal dezelfde randgevallen testen als aan boord.

**Scope**
- Simulator behoudt de bestaande `NMEA0183` en `Both` modes.
- Voeg een configureerbare realistische modus toe, bijvoorbeeld `Simulator:Nmea0183Profile=YDEN03`.
- In de YDEN03-modus gebruikt de simulator minimaal:
  - `YD` talker-prefixen voor bestaande navigatie/wind/heading/temperatuur sentences.
  - AIS-achtige `!AIVDM` en `!AIVDO` sentences met geldige checksum als raw-only NMEA 0183 verkeer.
  - Extra YDEN-achtige raw-only sentences uit de capture, zoals `ZDA`, `MWD`, `XDR`, `MDA`, `VTG`, `GSA`, `GSV`, `GLL`, `ROT`, `VLW`, `VWR`, `VWT`, `VDR`, `MXPGN` en `PCDIN`, voor zover praktisch.
- Semantische AIS-decoding is buiten scope; raw opslag en parseracceptatie zijn voldoende.

**Acceptatiecriteria**
- Lokale simulator-output bevat naast de bestaande ondersteunde sentences ook `!AIVDM` / `!AIVDO`.
- Alle door de simulator gegenereerde `$` en `!` sentences hebben geldige NMEA checksum.
- Ingest labelt alle simulator `$` en `!` sentences als `NMEA0183`.
- End-to-end lokale test vult dezelfde measurementtabellen als een echte boot-test voor ondersteunde sentence-types.
- Raw-only sentences blijven bewaard in `NetworkMessages` zonder de flow te blokkeren.

### Story 5 – Replay-validatie van echte capture naar SQLite

**Als** ontwikkelaar  
**wil ik** de echte boot-capture opnieuw kunnen afspelen tegen de API  
**zodat** fixes reproduceerbaar gevalideerd kunnen worden zonder opnieuw op de boot te zijn.

**Scope**
- Maak een kleine replay-route of toolkeuze voor NDJSON capturebestanden.
- Replay leest `rawLine`, `receivedAtUtc` en `remoteEndpoint` uit capture records.
- Replay post naar `/api/networkmessages` met dezelfde protocol- en `MessageId`-logica als Ingest.
- Replay mag bestaande data dupliceren; deduplicatie is buiten scope.

**Acceptatiecriteria**
- Replay van `ingest-capture-20260523-093220.ndjson` post alle 863 regels zonder crash.
- SQLite bevat na replay 863 nieuwe `NetworkMessages`.
- Alle `!AIVDM` / `!AIVDO` replay-records hebben `Protocol = "NMEA0183"`.
- Ondersteunde sentence-types leveren nieuwe records in de verwachte measurementtabellen.
- De replayprocedure is gedocumenteerd met concrete commando's en SQLite-controlequeries.

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
| TCP-ondersteuning (YDEN-03 poort 1456) | Geparkeerd; poort lijkt bedoeld voor YDEN-software, UDP volstaat voor BootManager |
| Simulator `Both`-modus | ✅ Geïmplementeerd (2026-05-18) |
| Conflict-resolutie NMEA2000 versus NMEA 0183 bij dubbele metingen | Open |
| Checksum-validatie verplicht in Fase 2 of pas Fase 3? | Besloten: optioneel in Fase 2 (geïmplementeerd), verplicht in Fase 3 |
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
