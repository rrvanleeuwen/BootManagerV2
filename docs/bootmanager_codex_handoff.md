# BootManager – Codex Handoff

## Doel van dit document
Dit document is bedoeld als vaste werkbasis voor Codex bij het BootManager-project. Het beschrijft:

- de projectcontext
- de architectuurafspraken
- de huidige functionele status
- de manier van samenwerken met ChatGPT/Codex/Copilot
- de test- en kwaliteitsaanpak
- de eerstvolgende logische stappen

Gebruik dit document als leidraad bij het bepalen van volgende user stories, het beoordelen van Copilot-output en het formuleren van acceptatietests.

---

## 1. Projectdoel

BootManager is een **.NET 8**-oplossing voor het ontvangen, parseren, interpreteren en opslaan van netwerkdata van boordapparatuur op een boot.

Functioneel einddoel:

- netwerkdata van boordapparatuur ontvangen
- lokaal verwerken
- lokaal opslaan
- betekenisvolle gegevens afleiden uit ruwe berichten
- dashboards, logging, alarmen en historische data ondersteunen
- later eventueel data kunnen pushen naar externe systemen of een website

De initiële keten is:

**Simulator → Ingest → BootManager.Web → Database**

De focus ligt nu op:

- dataverzameling
- correcte verwerking
- verticale slices per berichttype

Presentatie in Blazor of extra uitlees-API’s is nu nog niet de hoofdprioriteit.

---

## 2. Vaste solutionstructuur

Deze structuur moet altijd gerespecteerd worden:

- `BootManager.Core`
- `BootManager.Application`
- `BootManager.Infrastructure`
- `BootManager.Web`
- `BootManager.Tools.Simulator`
- `BootManager.Tools.Ingest`

### Verantwoordelijkheden per laag

#### BootManager.Core
Bevat de domeinkern:
- entiteiten
- interfaces
- value objects / domeinmodellen

#### BootManager.Application
Bevat businesslogica, feature-georiënteerd:
- DTO’s
- services
- interpreters
- parser-gerelateerde logica

#### BootManager.Infrastructure
Bevat persistentie:
- EF Core
- DbContext
- configuraties
- repositories
- migraties

#### BootManager.Web
Bevat de web/API-laag:
- controllers
- web-endpoints
- eventueel later presentatie

#### BootManager.Tools.Simulator
Los uitvoerbaar project dat netwerkberichten simuleert.

#### BootManager.Tools.Ingest
Los uitvoerbaar project dat UDP ontvangt en berichten naar BootManager.Web doorstuurt.

---

## 3. Architectuurafspraken

Deze regels zijn leidend:

1. **Tools schrijven niet rechtstreeks naar de database.**
   Alles loopt via `BootManager.Web`.

2. **Controllers blijven dun.**
   Logica hoort niet in controllers.

3. **Ingest blijft transportlaag.**
   Geen protocolinhoudelijke logica in Ingest stoppen als dat in parser/interpreter/service thuishoort.

4. **Parser en interpreter blijven strikt gescheiden.**
   - Parser = technisch
   - Interpreter = semantisch

5. **Generieke repositorystructuur blijft leidend.**
   Gebruik `IRepository<T>` en `EfRepository<T>`.
   Geen losse repository per entiteit als de generieke repository volstaat.

6. **Verticale slices zijn de standaardaanpak.**
   Per berichttype wordt de volledige keten gebouwd:
   - entity
   - EF-configuratie
   - DbSet
   - DTO’s
   - service
   - interpreter
   - parsermapping indien nodig
   - flow-koppeling
   - DI
   - migratie

7. **Nieuwe of aangepaste code krijgt waar relevant Nederlandse XML-documentatie.**
   Vooral interfaces en belangrijke publieke onderdelen.

8. **Inline comments alleen waar nodig.**
   Dus alleen bij niet-triviale logica of byte-layouts die anders lastig te volgen zijn.

9. **Waar nodig moet de simulator mee aangepast worden.**
   Een verticale slice kan dus beginnen met simulator-aanpassing als de huidige simulatie nog te ver van echte data afstaat.

10. **Waar relevant moeten NMEA 2000-definities gevolgd worden.**
    Niet stilzwijgend afwijken van publiek bekende semantiek.

---

## 4. Huidige verwerkingsketen

De formele flow is:

1. `BootManager.Tools.Simulator` genereert NMEA2000-achtige raw regels
2. `BootManager.Tools.Ingest` ontvangt UDP en post raw berichten door
3. `BootManager.Web` ontvangt het raw bericht
4. `NetworkMessageService` slaat het raw bericht op
5. `NetworkMessageParserService` doet technische parsing
6. type-specifieke interpreter haalt semantische waarden uit payload
7. type-specifieke measurement service zet dit om naar entity en slaat op
8. SQLite bevat raw berichten én afgeleide metingen

Belangrijk onderscheid:

- **raw opslag** blijft bestaan, ook als parser/interpreter later faalt
- parserclassificatie en interpretatie zijn een aparte laag bovenop raw opslag

---

## 5. Simulatorafspraken

De simulator is niet bedoeld als volledige gecertificeerde NMEA 2000-implementatie, maar moet **zo dicht mogelijk bij echte device-data liggen** zodat overstap naar echte hardware later minder impact heeft.

### Belangrijke inhoudelijke afspraken

- De simulator gebruikt NMEA2000-achtige PGN’s en payloads.
- Als een berichttype inhoudelijk te sterk afwijkt van echte data, mag de simulator aangepast worden.
- Het **raw tekstcontract** tussen Simulator → Ingest → Web moet zo stabiel mogelijk blijven.
- Veranderingen gebeuren bij voorkeur in:
  - payload builder
  - parser
  - interpreters
  - application services

### Huidige windafspraak

De **huidige windgegevens behandelen we als werkelijke wind**.
Niet als schijnbare wind.

Dus:
- bestaand windbericht niet stilzwijgend interpreteren als apparent wind
- apparent wind later alleen als aparte expliciete slice / simulatoruitbreiding toevoegen

---

## 6. Werkwijze met Codex en Copilot

### Rolverdeling

#### Codex / ChatGPT
Helpt met:
- objectief lezen waar het project staat
- bepalen wat de volgende kleine of middelgrote user story moet zijn
- maken van goede Copilot-prompts
- toetsen of Copilot iets heeft gemaakt zoals verwacht
- formuleren van acceptatietests
- beoordelen of een stap een logisch commit/push-moment is

#### Copilot in Visual Studio
Doet het echte codewerk op basis van een goed afgebakende prompt.

### Werkwijze per stap

Per stap werkt Codex idealiter als volgt:

1. **Huidige stand objectief lezen**
   - code
   - docs
   - tests
   - buildstatus

2. **Volgende user story bepalen**
   - klein tot middelgroot
   - gecontroleerd
   - passend bij bestaande architectuur

3. **Copilot-prompt formuleren**
   - duidelijk afgebakend
   - projectspecifiek
   - geen brede refactors zonder noodzaak

4. **Copilot-output beoordelen**
   - architectuur
   - compileerbaarheid
   - semantische juistheid
   - scopebewaking

5. **Acceptatietests geven**
   - build
   - runtime-keten
   - logging
   - Swagger
   - SQLite
   - regressiecheck

6. **Commit/push-moment expliciet benoemen**
   - alleen als de stap inhoudelijk klopt en getest is

### Belangrijke werkafspraken

- We werken in **kleine tot middelgrote gecontroleerde stappen**.
- De stappen mogen iets groter zijn dan in het begin, omdat het patroon inmiddels bewezen is.
- We committen **niet blind na iedere build**.
- Eerst inhoudelijk kloppend en getest, daarna pas commit/push.
- Geen onnodige herstructureringen.
- Geen brede refactors als de user story daar niet om vraagt.

---

## 7. Testaanpak

### Basistest per stap

#### Build
```bash
dotnet build
```

#### Runtime-keten
Start waar relevant:
- `BootManager.Web`
- `BootManager.Tools.Ingest`
- `BootManager.Tools.Simulator`

#### Logging
Controleer of:
- parserclassificatie klopt
- interpretatie slaagt
- opslag niet stukloopt

#### SQLite-controle
Controleer raw berichten en measurement-tabellen via `sqlite3`.

Voorbeeldcontroles:
- `NetworkMessages`
- `BatteryMeasurements`
- `DepthMeasurements`
- `WindMeasurements`
- `MotionMeasurements`
- `PositionMeasurements`
- `HeadingMeasurements`

#### Swagger
Gebruik waar nodig handmatige JSON-posts met velden als:
- `receivedAtUtc`
- `source`
- `protocol`
- `rawLine`
- `messageId`
- `payloadHex`

Belangrijk:
Swagger-berichten moeten aansluiten op de **actuele simulator/payloadafspraak**. Oude testpayloads kunnen ongeldig zijn als de simulator is aangepast.

#### Regressiecheck
Na een nieuwe slice altijd kort checken of bestaande slices nog steeds records blijven toevoegen.

---

## 8. Documentatie uit de repo

De `.docs` map in de repo is leidend als actuele architectuurbron.
In elk geval is de architectuurlijn daar bevestigd:

- BootManager is .NET 8
- verticale slices zijn de standaardaanpak
- `NetworkMessageService` orkestreert parse + interpret + store
- `Heading` is semantisch anders dan `Motion`
- payload decoding is little-endian met geschaalde integers
- parser/interpreter blijven gescheiden

Codex moet de `.docs` map daarom meenemen bij het bepalen van volgende stappen.

Let op:
- repo-documentatie kan soms iets achterlopen op de actuele code
- bij conflict geldt: objectief vaststellen wat code, migraties, build en tests werkelijk zeggen
- daarna eventueel documentatie laten bijwerken in een aparte stap

---

## 9. Huidige functionele status

De volgende verticale slices werken nu volledig, inclusief opslag in database:

- `Battery`
- `Depth`
- `Wind`
- `Motion`
- `Position`
- `Heading`

### Extra detail over Heading

De Heading-slice is recent afgerond en getest.
Daarvoor is eerst de simulator aangepast, omdat de headingpayload nog te vereenvoudigd was.

#### Wat is gedaan

1. Simulator aangepast voor PGN `127250`
2. Headingpayload gewijzigd naar een NMEA2000-achtiger **8-byte** structuur
3. Daarna parser/interpreter/opslag toegevoegd
4. Getest via:
   - Swagger
   - volledige runtime-keten
   - SQLite
5. Regressiecheck uitgevoerd op bestaande slices

#### Heading-payloadafspraak
Voor de huidige simulator geldt:

- byte 0 = SID
- bytes 1-2 = Heading
- bytes 3-4 = Deviation
- bytes 5-6 = Variation
- byte 7 = Reference

Voor de huidige slice is `HeadingDegrees` de primaire opgeslagen waarde.
Deviation/Variation/Reference zitten wel in payloadstructuur, maar hoeven nog niet volledig als losse velden in opslag benut te worden.

---

## 10. Waar we nu gebleven zijn

We zijn geëindigd op:

- branch: `feature/NetwerkData/Interpretation`
- Heading-slice is afgerond en getest
- regressiecheck liet zien dat de hele keten nog werkt

Werkende slices:
- Battery
- Depth
- Wind
- Motion
- Position
- Heading

---

## 11. Volgende logische stap

De volgende logische stap is **niet direct weer een nieuwe interpreter op bestaande simulatoroutput**, maar eerst een **simulatoruitbreiding** voor gegevens die nog niet in de keten zitten.

### Eerstvolgende kandidaat-uitbreidingen

1. **Snelheid door water**
2. **Watertemperatuur**

Dat betekent waarschijnlijk deze volgorde:

### Stap A – simulatoruitbreiding
- `BoatState` uitbreiden waar nodig
- scenario-startwaarden aanvullen
- payloadbuilder uitbreiden
- relevante PGN-specificatie toevoegen of actualiseren
- simulatoroutput laten meesturen

### Stap B – parser/interpreter/opslag
Pas daarna per nieuw berichttype weer verticale slices toevoegen:
- parsermapping
- interpreter
- measurement entity
- EF-configuratie
- DbSet
- service
- flow-koppeling
- DI
- migratie

### Mogelijke latere stap
- expliciete **schijnbare wind** als aparte slice, maar alleen correct en expliciet volgens NMEA 2000-semantiek

---

## 12. Wat Codex expliciet moet bewaken

Codex moet bij toekomstige stappen actief bewaken:

1. **Past de volgende stap in de architectuur?**
2. **Moet de simulator eerst aangepast worden?**
3. **Is een slice echt volledig verticaal uitgewerkt?**
4. **Worden parser en interpreter niet door elkaar gehaald?**
5. **Blijft Ingest dun?**
6. **Blijven controllers dun?**
7. **Loopt opslag via Web?**
8. **Sluit payloadsemantiek aan op NMEA2000 waar relevant?**
9. **Zijn huidige windgegevens nog steeds werkelijke wind?**
10. **Is een stap klein genoeg om veilig te reviewen en testen?**
11. **Is dit een logisch commit/push-moment of nog niet?**

---

## 13. Praktische instructie aan Codex

Gebruik bij nieuwe werkstappen bij voorkeur deze aanpak:

### Eerst analyseren
- lees actuele code
- lees `.docs`
- controleer build/test-status als dat relevant is
- bepaal of docs, tests en code nog in sync zijn

### Daarna voorstellen
- formuleer één volgende kleine of middelgrote user story
- geef acceptatiecriteria
- maak een Copilot-prompt die precies past bij de bestaande solutionstructuur

### Daarna reviewen
Na Copilot-output:
- scope checken
- architectuur checken
- build/test checken
- acceptatietest geven
- commit/push-moment expliciet benoemen

---

## 14. Samenvatting in één alinea

BootManager is een .NET 8 oplossing met een verticale-slice architectuur voor NMEA2000-achtige bootdata. De huidige keten Simulator → Ingest → Web → Parser → Interpreter → Measurement Service → SQLite werkt voor Battery, Depth, Wind, Motion, Position en Heading. Tools schrijven niet direct naar de database, parser en interpreter blijven strikt gescheiden, Ingest en controllers blijven dun, en simulator-aanpassingen zijn toegestaan als de simulatie anders te ver van echte data afwijkt. Huidige wind is werkelijke wind. De eerstvolgende logische stap is simulatoruitbreiding voor snelheid door water en watertemperatuur, gevolgd door nieuwe verticale slices voor verwerking en opslag.

