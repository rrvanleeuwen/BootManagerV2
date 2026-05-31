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
  - Protocoldetectie vindt plaats op basis van regelinhoud: regels die beginnen met `$` of `!` zijn NMEA 0183, overige regels zijn NMEA 2000/raw-like.
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
- Protocoldetectie op regelinhoud: regels die beginnen met `$` of `!` → `NMEA0183`, overig → `NMEA2000`.
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
- NMEA 0183 sentences (beginnen met `$` of `!`) worden als `Protocol = "NMEA0183"` doorgestuurd.
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

**Status 2026-05-26:** Raspberry Pi 4 Docker Compose deployment-smoke-test is geslaagd. Ingest draait op de Pi met UDP `10110/udp`; de app is via het LAN bereikbaar. De volgende hardwarestap is de echte boot UDP-broadcasttest met YDEN-03/Teltonika. Zie `.docs/extraInfo/yden-03.md` en `.docs/docker-deployment.md` voor de `tcpdump` checklist.

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

De echte YDEN-03 capture `ingest-capture-20260523-093220.ndjson` bevestigde dat UDP ontvangst en raw opslag werken, maar toonde ook drie concrete gaten:

- AIS sentences met `!AIVDM` / `!AIVDO` worden foutief als `NMEA2000` gelabeld.
- NMEA 0183-derived measurements worden niet opgeslagen omdat NMEA 0183 requests geen bruikbare `MessageId` hebben.
- De simulator mist nog echte YDEN-achtige variatie zoals AIS `!` sentences, `YD` talker-prefixen en extra raw-only sentences.

Status 2026-05-23: Story 1 t/m 4 zijn geïmplementeerd en gevalideerd. Ingest herkent `$` en `!` als NMEA 0183, de parser accepteert AIS-startteken `!`, NMEA 0183 krijgt een stabiele sentence-id als `MessageId`, en de simulator heeft een `YDEN03`-profiel. Story 5 (capture replay) is bewust geparkeerd.

### Story 1 – Correcte NMEA 0183 protocolherkenning voor `$` en `!` ✅

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

### Story 2 – NMEA 0183 parser accepteert AIS-startteken `!` ✅

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

### Story 3 – Bruikbare `MessageId` voor NMEA 0183-derived measurements ✅

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

### Story 4 – Simulator realistischer maken op basis van echte YDEN-capture ✅

**Als** ontwikkelaar die niet op de boot is  
**wil ik** dat de simulator representatieve YDEN-achtige NMEA 0183 output kan genereren  
**zodat** ingest, parser en SQLite-verwerking lokaal dezelfde randgevallen testen als aan boord.

**Scope**
- Simulator behoudt de bestaande `NMEA0183` en `Both` modes.
- Voeg een configureerbare realistische modus toe, bijvoorbeeld `Simulator:Nmea0183Profile=YDEN03`.
- In de YDEN03-modus gebruikt de simulator nu:
  - `YD` talker-prefixen voor bestaande navigatie/wind/heading/temperatuur sentences.
  - AIS-achtige `!AIVDM` en `!AIVDO` sentences met geldige checksum als raw-only NMEA 0183 verkeer.
  - Extra YDEN-achtige raw-only sentences: `ZDA`, `MWD`, `XDR`, `MDA` en `VTG`.
  - Negatieve testvarianten wanneer `Simulator:IncludeNegativeTestSentences=true`: MWV status `V`, RMC status `V`, GGA fixkwaliteit `0` en VHW met ongeldige checksum.
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

### Story 6 - Volledige boordnetwerk-capture analyseren voor dashboarddekking

**Status:** Voorgesteld op 2026-05-30.

**Als** ontwikkelaar/operator
**wil ik** een volledige capture van de boordnetwerkberichten analyseren, inclusief berichten die BootManager nu nog niet interpreteert
**zodat** duidelijk wordt welke sensordata beschikbaar is, welke berichten ontbreken in de huidige verwerking en welke dashboardwaarden betrouwbaar gebouwd kunnen worden.

**Aanleiding**
- De eerste Raspberry Pi-veldtest bevestigde ingest, raw opslag en meerdere measurement-tabellen.
- Er is nog onzekerheid of BootManager alle relevante berichten ontvangt en interpreteert die het boordnetwerk verstuurt.
- De gebruiker wil bij een volgende test extra logging/capture aanzetten om ook overige berichten vast te leggen.
- De analyse-uitkomst heeft directe invloed op `DSH-LIVE-1` en latere dashboard-slices.

**Scope**
- Capture logging op de Pi bewust aanzetten tijdens een representatieve test met zoveel mogelijk boordnetwerkberichten.
- Vastleggen welke ruwe NMEA 0183 sentence-types, talker-prefixen en message IDs binnenkomen.
- Bepalen welke berichten al leiden tot bestaande measurements.
- Bepalen welke berichten raw worden opgeslagen maar nog geen interpreter of measurement hebben.
- Per relevant berichttype aangeven of het dashboard er direct iets mee kan, later een interpreter nodig heeft, of buiten scope blijft.
- Resultaat documenteren als input voor `DSH-LIVE-1` en latere dashboard-slices.

**Buiten scope**
- Geen nieuwe dashboardwidgets bouwen in deze story.
- Geen nieuwe NMEA-interpreters implementeren, tenzij later apart besloten.
- Geen AIS-semantiek of kaartweergave bouwen.
- Geen definitieve retentie- of cleanup-policy voor raw data.
- Geen Raspberry Pi feature-branch test; de Pi blijft alleen `master` gebruiken.

**Acceptatiecriteria**
- Er is een capturebestand of analyse-export van een representatieve test met extra logging.
- De analyse bevat een lijst van gevonden sentence-types/message IDs met aantallen.
- Per type is duidelijk: `ondersteund`, `raw-only`, `onbekend`, `ongeldig`, of `kandidaat voor dashboard`.
- Er is expliciet benoemd welke dashboardwaarden nu betrouwbaar beschikbaar zijn en welke nog niet.
- Eventuele vervolgstories voor extra interpreters of diagnostics zijn concreet benoemd.

**Legacy coverage impact**
- Raakt `US9.5 Sensorintegratie via Bluetooth of Wi-Fi`; status blijft `Partial`, maar de YDEN/UDP sensordekking wordt beter onderbouwd.
- Raakt `US7.13 Automatische update van gegevens`; status blijft `Open` tot dashboard livewaarden echt gebouwd zijn.
- Ondersteunt later `US7.1`, `US7.2` en `US7.3` via betere dashboarddekking.
- Raakt mogelijk `US9.2 AIS integratie` als `!AIVDM`/`!AIVDO` relevant blijken, maar AIS-semantiek blijft buiten deze story.

**Handmatige testnotities**
- Op de Pi alleen uitvoeren nadat de benodigde capture/logging-instelling op `master` beschikbaar is.
- Tijdens test noteren: starttijd, eindtijd, boot-/instrumentconfiguratie, ingest settings, capture logging status en eventuele bijzonderheden.
- Na afloop niet alleen kijken of er data is, maar vooral welke berichttypen wel binnenkomen en nog niet worden geinterpreteerd.

---

### Story 7 - Pi database analyseren voor logboekvelden en bronduplicatie

**Status:** Analyse uitgevoerd op 2026-05-31; zie `.docs/analysis/pi-database-2026-05-31.md`.

**Als** ontwikkelaar/operator
**wil ik** de vandaag op de Raspberry Pi verzamelde BootManager-database analyseren op ruwe berichten, afgeleide measurements en broninformatie
**zodat** we objectief kunnen bepalen welke logboekvelden nog automatisch gevuld kunnen worden en waar meerdere apparaten dezelfde soort meetwaarde leveren.

**Aanleiding**
- De Pi heeft op 2026-05-31 opnieuw echte boordnetwerkdata verzameld.
- Voor het logboek missen nog onder meer motoruren, brandstof/tankniveau, logstand en rijkere reisstatistiek.
- De gebruiker verwacht dat het NMEA-systeem mogelijk tankniveau of andere nog niet geimplementeerde berichten uitzendt.
- Eenzelfde meetsoort kan op een boot via meerdere apparaten binnenkomen, bijvoorbeeld GPS/positie via plotter, AIS of marifoon.
- Voordat we bronvoorkeuren of nieuwe interpreters bouwen, moet duidelijk zijn welke broninformatie stabiel uit de huidige data afleidbaar is.

**Scope**
- Een kopie/export van de Pi-database of relevante JSON/CSV analyse-uitvoer verzamelen.
- Per `NetworkMessage` groeperen op protocol, message id/sentence type, talker-prefix, source/remote endpoint en aantallen.
- Vaststellen welke ruwe berichten al tot bestaande measurement-tabellen leiden.
- Vaststellen welke ruwe berichten raw-only blijven maar kandidaat zijn voor logboekvelden zoals tankniveau, brandstof, motoruren, logstand of afstand.
- Vaststellen waar dezelfde meetsoort via meerdere bronnen voorkomt, in het bijzonder positie/GPS, COG/SOG, heading, wind, diepte en eventueel tank/motor.
- Documenteren welke bronkenmerken bruikbaar lijken voor latere voorkeuren: protocol, message id, talker-prefix, remote endpoint, of een combinatie daarvan.

**Buiten scope**
- Geen nieuwe interpreter implementeren.
- Geen database-schema wijzigen.
- Geen bronvoorkeuren-UI bouwen.
- Geen automatische correctie of deduplicatie van bestaande measurements.
- Geen definitieve keuze maken voor alle boten; dit is een analyse van de huidige installatie als input voor generieke keuzes.

**Acceptatiecriteria**
- Er is een analyseoverzicht met aantallen per protocol/message id/sentence type/talker/source.
- Er is een lijst van gevonden raw-only kandidaatberichten met reden waarom ze interessant zijn.
- Er is expliciet vastgelegd of tankniveau, motoruren, brandstof, logstand of vergelijkbare logboekvelden in de verzamelde data zichtbaar lijken.
- Er is expliciet vastgelegd of meerdere bronnen dezelfde meetsoort leveren.
- Er is een voorstel voor de eerstvolgende interpreter of datamodelwijziging, gebaseerd op de analyse.
- Er is een voorstel voor hoe BootManager bronnen later stabiel kan identificeren voor gebruikersvoorkeuren.

**Legacy coverage impact**
- Raakt `US5.3 Motoruren en brandstof in header`; status blijft `Partial` tot velden/interpreters echt geimplementeerd zijn.
- Raakt `US5.6 Logboekheader invullen` en `US5.11 Statistieken en samenvatting`; status blijft `Partial`.
- Raakt `US8.5 Sensorintegratie configureren` en `US9.5 Sensorintegratie via Bluetooth of Wi-Fi`; bron- en sensordekking wordt beter onderbouwd.
- Raakt mogelijk `US9.2 AIS integratie` als AIS als alternatieve positie- of bewegingsbron zichtbaar wordt, maar AIS-semantiek blijft buiten scope.

**Handmatige testnotities**
- De Pi blijft alleen `master` volgen; analyse gebeurt op een database/export die van de Pi is opgehaald.
- Noteer bij de export: datum/tijd, branch/commit van de Pi, ingest settings, NMEA aan/uit-status en globale duur van de test.
- Analyseer eerst read-only; wijzig de Pi-database niet.

**Resultaat 2026-05-31**
- De Pi-database bevat 444.454 ruwe `NetworkMessages` over de periode `2026-05-29 12:18:21 UTC` tot `2026-05-31 10:05:48 UTC`.
- Bestaande interpreters vullen positie, motion, heading, wind, snelheid door water en watertemperatuur.
- `DepthMeasurements` en `BatteryMeasurements` zijn leeg in deze export.
- Tankniveau is zichtbaar als sterke kandidaat via `PCDIN`/`MXPGN` met PGN `01F211`, waarschijnlijk NMEA 2000 PGN `127505` Fluid Level.
- Er lijken minimaal drie fluid-kanalen zichtbaar: brandstof instance 0, water instance 0 en water instance 1.
- `YDVLW` is een sterke kandidaat voor logstand/afstand door water.
- Motoruren zijn niet duidelijk zichtbaar in deze export.
- De huidige `Source`-waarde lijkt Docker/UDP-endpointmetadata en is niet stabiel genoeg als fysieke apparaatidentiteit.
- Aanbevolen volgorde: eerst Story 8 bronidentiteit/bronvoorkeuren ontwerpen, daarna Story 9 starten met PGN 127505 Fluid Level, daarna `YDVLW`.

---

### Story 8 - Bronidentiteit en bronvoorkeuren ontwerpen

**Status:** Voorgesteld op 2026-05-31; oppakken na Story 7.

**Als** eigenaar/beheerder
**wil ik** per meetsoort kunnen kiezen welke databron BootManager primair gebruikt
**zodat** mijn logboek en dashboard de waarden tonen van het apparaat dat ik op mijn boot vertrouw.

**Aanleiding**
- Op een boot kunnen meerdere apparaten dezelfde soort data publiceren.
- Voorbeelden: GPS/positie via plotter, AIS of marifoon; heading via kompas of plotter; wind via masttopinstrument of gateway; diepte via verschillende transducers.
- BootManager moet niet blind de laatste meting nemen als meerdere bronnen verschillende kwaliteit of betekenis hebben.
- Huidige measurements bewaren niet overal expliciet genoeg welke bron de meting heeft geleverd; broninformatie zit vooral in of rond `NetworkMessage`.

**Scope**
- Ontwerpen hoe BootManager bronnen stabiel identificeert en benoemt voor gebruikers.
- Bepalen welke bronmetadata minimaal bij of naast measurements beschikbaar moet zijn: protocol, message id/sentence type, talker-prefix, source/remote endpoint, en eventueel later gebruikerslabel.
- Per meetsoort een bronvoorkeurmodel ontwerpen, bijvoorbeeld voor positie, COG/SOG, heading, wind, diepte, temperatuur, tankniveau en motoruren.
- Bepalen wat fallbackgedrag is als de voorkeursbron tijdelijk geen recente data levert.
- Bepalen hoe de UI in Settings later bronnen toont zonder technisch overweldigend te worden.

**Buiten scope**
- Geen volledige implementatie van settings-UI in deze ontwerpstory.
- Geen automatische device discovery met vendornaam of productnaam, tenzij die al betrouwbaar in data zit.
- Geen multi-boot of multi-user configuratie.
- Geen historische herberekening van bestaande logboekregels.

**Acceptatiecriteria**
- Er is een concreet ontwerp voor bronidentiteit en bronvoorkeuren per meetsoort.
- Het ontwerp beschrijft waar bronmetadata opgeslagen of herleid wordt.
- Het ontwerp beschrijft fallbackgedrag bij ontbrekende voorkeursbron.
- Het ontwerp beschrijft hoe bestaande dashboards/logboek-suggesties bronvoorkeuren later moeten toepassen.
- Er zijn vervolgstories gesneden voor datamodel, servicebeleid en Settings-UI.

**Legacy coverage impact**
- Raakt `US8.5 Sensorintegratie configureren`; status blijft `Partial` tot de gebruiker voorkeuren echt kan beheren.
- Raakt `US9.5 Sensorintegratie via Bluetooth of Wi-Fi`; BootManagerV2 blijft voorlopig UDP/YDEN-first, maar bronkeuze bereidt bredere sensorintegratie voor.
- Ondersteunt `US7.2 Actieve bootinformatie`, `US7.13 Automatische update van gegevens` en meerdere `US5.*` logboekvelden door betrouwbaardere bronselectie.

---

### Story 9 - Eerste nieuwe interpreter op basis van Pi-analyse

**Status:** Voorgesteld op 2026-05-31; kandidaat pas kiezen na Story 7.

**Als** eigenaar
**wil ik** dat BootManager een nieuw in mijn boordnetwerk aanwezig berichttype interpreteert
**zodat** logboek, dashboard of reisstatistiek extra beschikbare bootdata kan gebruiken.

**Kandidaatvelden**
- Tankniveau of brandstofvoorraad.
- Motoruren.
- Logstand of afstand door water.
- Andere tijdens Story 7 gevonden raw-only berichten met duidelijke waarde.

**Scope**
- Een eerste kandidaatbericht kiezen op basis van de Pi-databaseanalyse.
- Parser/interpreter toevoegen voor dat ene berichttype.
- Measurement- of domeinopslag toevoegen als het bestaande model het veld nog niet kan dragen.
- Unit tests toevoegen met realistische voorbeeldsentences/velden uit de analyse.
- Documenteren hoe het nieuwe bericht logboek/dashboard later kan voeden.

**Buiten scope**
- Geen meerdere nieuwe interpreters tegelijk.
- Geen bronvoorkeuren-UI, tenzij strikt nodig voor deze ene meting.
- Geen dashboard- of logboek-UI-polish behalve minimale zichtbaarheid of testbaarheid als acceptatie dat vereist.
- Geen AI- of vendor-specifieke decodeerlogica zonder duidelijke brondata.

**Acceptatiecriteria**
- Het gekozen berichttype wordt uit ruwe netwerkberichten herkend en gevalideerd.
- Geldige berichten leveren een opgeslagen measurement of domeinrecord op.
- Ongeldige berichten blijven raw opgeslagen maar veroorzaken geen crash.
- Unit tests dekken geldige, ongeldige en ontbrekende velden.
- De analyse- en architectuurdocumentatie is bijgewerkt met de gemaakte keuze.

**Legacy coverage impact**
- Hangt af van gekozen kandidaat:
  - tank/brandstof/motoruren raakt `US5.3`, `US5.11` en `US10.1`;
  - logstand/afstand raakt `US5.6`, `US5.11` en logboekstatistiek;
  - sensorbron-diagnostiek raakt `US8.5` en `US9.5`.

---

### Future Story – Configureerbare sampling en raw-dataretentie

**Als** eindgebruiker/operator  
**wil ik** kunnen instellen hoe vaak BootManager metingen uit de continue netwerkstroom verwerkt  
**zodat** de database beheersbaar blijft zonder relevante bootstatus te verliezen.

**Aanleiding**
- De boot beweegt langzaam, terwijl de YDEN-03 veel berichten per seconde kan sturen.
- Voor operationele logging is meestal niet elke ruwe regel nodig als periodieke parsing succesvol is.
- Raw data is waardevol voor diagnose, maar kan op termijn te veel opslag innemen.

**Status 2026-05-23**
- Het fundament voor gebruikersconfiguratie is gelegd: operationele instellingen worden via `/settings` in de database opgeslagen.
- Beschikbare instellingen: luisteradres, primaire en alternatieve poort, API basis-URL, raw opslagmodus, standaard sample-interval en capture logging.
- `BootManager.Tools.Ingest` gebruikt deze database-instellingen nog niet; appsettings blijft nu nog de runtimebron voor de tool.

**Mogelijke instelling**
- `High` – verwerk/bewaar metingen met ongeveer 1 seconde interval.
- `Medium` – verwerk/bewaar metingen met ongeveer 10 seconden interval.
- `Low` – verwerk/bewaar metingen met ongeveer 60 seconden interval.
- Exacte semantiek later ontwerpen: per sentence-type, per meetwaarde, of globaal.

**Ontwerpvragen**
- Geldt sampling voor raw ingest, derived measurements, of beide?
- Moet raw data altijd tijdelijk bewaard worden en pas later opgeschoond worden?
- Wanneer mag raw data verwijderd worden: alleen als parsing in hetzelfde tijdvenster succesvol was?
- Moeten onbekende of niet-geparseerde sentence-types langer bewaard blijven voor diagnose?
- Wordt de instelling beheerd door de eindgebruiker in de UI, via appsettings, of beide? UI/database is leidend voor gebruikersconfiguratie; appsettings blijft fallback/default voor losse tools.
- Hoe voorkomen we dat sampling belangrijke events of foutcondities wegfiltert?

**Voorlopig principe**
- Raw opslag blijft tijdens ontwikkeling en diagnose leidend.
- Automatisch verwijderen van raw data mag pas nadat er een expliciet retentiebeleid, gebruikersinstelling en herstel/diagnosepad is ontworpen.
- Vervolgstap: Ingest haalt operationele instellingen bij startup op bij `BootManager.Web`, met appsettings als fallback wanneer Web niet bereikbaar is.
- Daarna kan de gekozen raw opslagmodus en het sample-interval daadwerkelijk worden toegepast in de opslagpipeline.

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
*Bijgewerkt: 2026-05-23 — Ingest startup koppeling aan BootManager.Web operationele instellingen toegevoegd (Fase 4); RawStorageMode sampling policy toegepast (Fase 5).*

---

## Fase 4: Ingest startup koppeling aan Web/database settings (2026-05-23)

**Branch:** `feature/ingest-operational-settings`

### Wat is geïmplementeerd

- Nieuw endpoint `GET /api/operationalsettings/ingest` in `BootManager.Web`.
  - Geeft `IngestSettingsDto` terug: `ListenAddress`, `ListenPort`, `ApiBaseUrl`, `CaptureLoggingEnabled`, `RawStorageMode`, `DefaultSampleIntervalSeconds`.
  - Voorlopig anoniem bereikbaar (**TODO:** beveiligen in volgende iteratie).
- `IngestRemoteSettings` model in Ingest (mirror van de Web API response).
- `IOperationalSettingsClientService` + `OperationalSettingsClientService` in Ingest.
  - Timeout: 5 seconden.
  - Bij failure: waarschuwing loggen en appsettings gebruiken als fallback.
- `Program.cs` van Ingest haalt bij startup settings op en past ze toe vóór `host.RunAsync()`.

### Gedrag

| Situatie | Gedrag |
|---|---|
| Web bereikbaar | Ingest overschrijft `ListenAddress`, `ListenPort`, `ApiBaseUrl`, `CaptureLogging.Enabled` |
| Web niet bereikbaar | Waarschuwing gelogd; appsettings.json als fallback |
| `RawStorageMode` / `DefaultSampleIntervalSeconds` | Opgehaald en gelogd, **nog niet toegepast** |

### Niet gedaan in deze slice

- Geen sampling toepassen.
- Geen raw storage mode toepassen.
- Geen database writes vanuit Ingest.
- Geen background polling.

---

## Fase 5: RawStorageMode sampling policy toepassen (2026-05-23)

**Branch:** `feature/apply-ingest-sampling-policy`

### Wat is geïmplementeerd

- `IIngestSamplingPolicy` interface: beslissing `ShouldProcessMessage(protocol, messageId) → bool` per berichten-flow.
- `IngestSamplingPolicy` implementatie:
  - **All mode:** Alle berichten doorlaten (huidig gedrag behouden).
  - **Sampled mode:** Per stream key (`Protocol:MessageId`, genormaliseerd) maximaal 1 bericht per `DefaultSampleIntervalSeconds`.
  - **OffAfterSuccessfulParse mode:** Voorlopig identiek aan Sampled; echte post-parse cleanup volgt later.
  - Thread-safe per-stream-key timing met `Dictionary<string, DateTime>`.
  - Defensieve interval-validatie: `≤ 0` → fallback 10 seconden met waarschuwing.
  - `Reset()` methode voor testing.
- `IngestOptions` uitgebreid met:
  - `RawStorageMode` (enum, standaard `All`)
  - `DefaultSampleIntervalSeconds` (int, standaard 10)
- `IngestService` aangepast:
  - `IIngestSamplingPolicy` constructor-injection.
  - `ShouldProcessMessage()` controle vóór `SendToApiWithDetailsAsync()`.
  - Capture logging onafhankelijk van sampling (diagnose blijft altijd werkend).
- `Program.cs` uitgebreid:
  - Enum-parsing van remote `RawStorageMode` (case-insensitive).
  - Fallback naar `All` bij ongeldige waarde.
  - Volledige logging van actieve mode en interval bij startup.
  - Bij `OffAfterSuccessfulParse`: expliciete opmerking dat dit voorlopig als Sampled gedaan wordt.
- Unit tests: 11 tests in `IngestSamplingPolicyTests` dekken:
  - All mode: alles doorgelaten.
  - Sampled mode: eerste bericht door, volgende geblokkeerd, na interval weer door.
  - Stream key isolation per MessageId en Protocol.
  - Null/empty MessageId handling.
  - OffAfterSuccessfulParse gedrag.
  - Interval fallback.
  - MessageId normalization (case-insensitive).
  - Reset functie.

### Gedrag

| Mode | Gedrag | Doel |
|---|---|---|
| **All** | Alle berichten → API/database | Geen sampling; volledige diagnose-opslag |
| **Sampled** | Per stream key ≤ 1 bericht per interval → API | 5-6u boot stroom beheerbaar; belangrijk beperken |
| **OffAfterSuccessfulParse** | Zoals Sampled (voorlopig) | Voorzichtige voorbereiding; post-parse cleanup later |

### Stream key

Format: `{Protocol}:{MessageId}` (MessageId genormaliseerd uppercase).

Voorbeelden:
- `NMEA0183:YDGGA` (positie via NMEA 0183)
- `NMEA0183:YDHDM` (heading via NMEA 0183)
- `NMEA0183:AIVDM` (AIS via NMEA 0183)
- `NMEA0183:Unknown` (NMEA 0183 zonder herkenbare MessageId)
- `NMEA2000:01F80403` (Motion via NMEA 2000)

### Capture logging onafhankelijk

- `CaptureLoggingEnabled` in database bepaalt of NDJSON capture-logging actief is.
- Capture logging **werkt onafhankelijk van sampling**.
- Overgeslagen berichten kunnen dus via capture-log gediagnosticeerd worden.
- Dit maakt debugging van sampling-beleid mogelijk.

### Logging bij startup

Ingest logt nu:
```
RawStorageMode set to Sampled; sample interval is 10 seconds.
RawStorageMode set to OffAfterSuccessfulParse; treating as Sampled for now. True post-parse raw-retention not yet supported (will be implemented in future slice).
```

### Niet gedaan in deze slice

- Post-parse raw-retention (verwijdering na succesvolle Web-parsing).
- Database writes vanuit Ingest sampling.
- Live-herconfiguratie van policy (settings worden alleen bij startup geladen).
- Automatische database-cleanup.

### Acceptatiecriteria (volledig)

- ✅ `dotnet build` slaagt.
- ✅ `dotnet test` slaagt; 11 nieuwe tests, alle groen.
- ✅ IngestService logt bij startup welke RawStorageMode en interval actief zijn.
- ✅ Bij `RawStorageMode=All` blijft huidig gedrag intact.
- ✅ Bij `RawStorageMode=Sampled` wordt aantal posts naar `/api/networkmessages` beperkt per stream key.
- ✅ Verschillende berichttypes verdringen elkaar niet (aparte stream keys).
- ✅ Capture logging blijft onafhankelijk werkend.
- ✅ `OffAfterSuccessfulParse` veilig en duidelijk als Sampled geïmplementeerd.
- ✅ Geen capture logs, testdata of lokale databasebestanden in commit.

