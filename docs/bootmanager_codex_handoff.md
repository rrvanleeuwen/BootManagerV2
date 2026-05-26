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

### Hardwaresituatie op de echte boot

De fysieke boot gebruikt een **YDEN-03 gateway** om NMEA 2000-busdata naar het netwerk te brengen.
In de huidige YDEN-03-configuratie wordt de output als **NMEA 0183 sentences** verzonden via:
- UDP poort 2000
- UDP poort 10110
- TCP poort 1456

Dit betekent dat verbinding met de echte boot NMEA 0183 inputverwerking vereist.
Zie: `.docs/extraInfo/yden-03.md` en `.docs/epics/nmea0183-support.md`.

### Toekomstige flow: NMEA 0183 (parallel)

```
YDEN-03 (UDP poort 2000 / 10110)
      ↓
   Ingest Tool (één gecombineerde UDP listener, protocolherkenning per regel)
      ↓
BootManager.Web API (CreateNetworkMessage, Protocol=NMEA0183)
      ↓
NetworkMessageService (raw sentence opgeslagen)
      ↓
[Fase 2 ✅] Nmea0183ParserService (sentence-type herkenning, veldextractie, checksum-validatie)
      ↓
[Fase 3] Sentence-specifieke Interpreter → Measurement Service → Database
```

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
- geen applicatiecode aanpassen zonder expliciete opdracht van de gebruiker, ook niet voor kleine reviewfixes, warnings, whitespace of buildfouten

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

3. **User story expliciet formuleren en laten goedkeuren**
   - formuleer de user story als "Als ... wil ik ... zodat ..."
   - benoem scope
   - benoem expliciet buiten scope
   - benoem acceptatiecriteria
   - benoem geraakte legacy US-nummers en verwachte coverage-status
   - benoem handmatige teststappen als UI, runtime, database, configuratie of auth geraakt wordt
   - vraag de gebruiker expliciet of deze user story klopt
   - bewaar de goedgekeurde user story daarna automatisch in het relevante `.docs/epics/*.md` bestand
   - maak of stel eerst een passend epic-bestand voor als er nog geen logisch epic-document bestaat
   - ga pas door naar een Copilot-prompt nadat de goedgekeurde user story in het epic-bestand staat

4. **Copilot-prompt formuleren**
   - duidelijk afgebakend
   - projectspecifiek
   - geen brede refactors zonder noodzaak
   - beschrijf doel, scope, architectuurafspraken en acceptatiecriteria
   - schrijf Copilot niet onnodig voor welke bestanden exact nieuw of aangepast moeten worden
   - laat Copilot de bestaande structuur en instructies gebruiken om de implementatiedetails te bepalen
   - neem bij nieuwe features expliciet op dat relevante `.docs` documentatie moet worden bijgewerkt
   - neem bij documentatie-updates expliciet op dat datums de actuele sessiedatum moeten gebruiken en niet automatisch oude documentdatums mogen overnemen

5. **Copilot-output beoordelen**
   - architectuur
   - compileerbaarheid
   - semantische juistheid
   - scopebewaking
   - als een codefix nodig is: geef een gerichte Copilot-prompt of vraag expliciet of Codex de applicatiecode zelf mag aanpassen

6. **Acceptatietests geven**
   - build
   - runtime-keten
   - logging
   - Swagger
   - SQLite
   - regressiecheck

7. **Commit/push-moment expliciet benoemen**
   - alleen als de stap inhoudelijk klopt en getest is

### Belangrijke werkafspraken

- We werken in **kleine tot middelgrote gecontroleerde stappen**.
- De stappen mogen iets groter zijn dan in het begin, omdat het patroon inmiddels bewezen is.
- We committen **niet blind na iedere build**.
- Eerst inhoudelijk kloppend en getest, daarna pas commit/push.
- Geen onnodige herstructureringen.
- Geen brede refactors als de user story daar niet om vraagt.

### Git-regie door Codex

Bij akkoord op een nieuwe slice of grotere werkstap regelt Codex de git-flow:

1. Controleer huidige branch en `git status`.
2. Controleer of er relevante open PR's of recent gemergde PR's zijn.
3. Zorg dat lokale `master` actueel is via een fast-forward pull vanaf `origin/master`.
4. Maak vanaf actuele `master` een nieuwe feature-branch met een duidelijke naam.
5. Formuleer daarna eerst de user story met scope, buiten scope, acceptatiecriteria en legacy coverage-impact.
6. Vraag expliciet akkoord op de user story.
7. Bewaar de goedgekeurde user story automatisch in het relevante `.docs/epics/*.md` bestand.
8. Maak pas daarna de Copilot-prompt en laat Copilot of codewijzigingen daarna uitvoeren.
9. Review na wijzigingen de scope, build/teststatus en eventuele runtimechecks.
10. Controleer dat er geen long-running repo-processen of `dotnet` processen zijn blijven hangen.
11. Vraag de gebruiker expliciet om akkoord voordat Codex commit, push en PR aanmaakt.
12. Na akkoord voert Codex commit, push en PR-aanmaak uit en controleert daarna opnieuw de werkmapstatus.

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

Belangrijke procesafspraak:
- Start long-running processen zoals `BootManager.Tools.Simulator`, `BootManager.Tools.Ingest` of `BootManager.Web` alleen gecontroleerd.
- Gebruik geen simpele command-timeout als cleanup-mechanisme voor runtimechecks.
- Als een proces tijdelijk wordt gestart, stop het expliciet en controleer daarna of er geen repo-gerelateerde `dotnet` processen zijn blijven draaien.
- Gebruik waar nodig `dotnet build-server shutdown` na build- of runtimeproblemen met locks.

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

- branch: `master`
- NMEA 0183 Fase 1 t/m 3c, simulator NMEA 0183 output en runtime/SQLite acceptatietest zijn afgerond
- Owner/settings/onboarding beheerflows zijn afgerond
- Eerste Raspberry Pi 4 Docker Compose deployment-smoke-test is geslaagd op 2026-05-26
- Laatste relevante deploymentfixes:
  - `124c7af Fix Docker base image tags for ARM64`
  - `4ef3d73 Fix IngestControlServer HttpListener prefix for wildcard binding`
- Pi-updateafspraak: de Pi hoeft niet automatisch na iedere push te pullen. Bij documentatie-only wijzigingen meestal geen Pi-update. Als een Pi-update nodig is, moet Codex exact zeggen welke SSH-commando's nodig zijn en of containers opnieuw gebouwd, alleen herstart of ongemoeid moeten blijven.
- Pi-updateflow bij code/containerwijzigingen: `git pull`, `docker compose build`, `docker compose up -d`, `docker compose ps`, `/health` controleren
- Volgende hardwarestap: echte boot UDP-broadcasttest met YDEN-03/Teltonika op poort `10110`

Werkende NMEA2000 slices (ongewijzigd):
- Battery
- Depth
- Wind
- Motion
- Position
- Heading
- Speed Through Water
- Water Temperature

NMEA 0183 status:
- Fase 1: raw ingest via UDP ✅
- Fase 2: parserlaag ✅
- Fase 3a: VHW, MTW, DBT/DPT interpreters ✅
- Fase 3b: MWV, HDT/HDM interpreters ✅
- Fase 3c: RMC/GGA positie + motion ✅
- Simulator NMEA 0183 output (`OutputMode=NMEA0183/Both`) ✅
- Runtime/SQLite acceptatietest fase 3a-3c ✅ (2026-05-18, handmatig)

---

## 11. Volgende logische stap

~~Runtime/SQLite acceptatietest voor NMEA 0183 fase 3a-3c~~ ✅ Afgerond (2026-05-18, handmatig).

TCP-ondersteuning voor YDEN-03 poort 1456 is voorlopig **niet nodig**.
De TCP-poort lijkt bedoeld voor de eigen YDEN-software; BootManager gebruikt de bewezen UDP NMEA 0183 route.

### Mogelijke volgende stappen
- First-Run Onboarding bugfix: legacy `Register Owner` menu-item en `/register-owner` route verwijderen/neutraliseren. Tijdens de Raspberry Pi test vóór login/onboarding bleek deze oude vrije registratieflow nog bereikbaar. Deze story staat als US7 in `.docs/epics/first-run-onboarding.md` en moet vóór verdere onboarding/deployment polish worden opgepakt.
- System Operations & Recovery: gecontroleerde Pi database reset (`SYS-RESET-1`) zodat ontwikkelaar/helpdesk een Docker Compose testinstallatie opnieuw door bootstrap login en onboarding kan laten lopen zonder handmatig databasebestanden of volumes te verwijderen. Deze story is hoog geprioriteerd voor deployment/operability en staat in `.docs/epics/system-operations.md`.
- Digitaal logboek: ontbrekende logmomenten zichtbaar maken. Er bestaat al `Draft`/`Confirmed`, badges, accorderen, print-filtering en meetdatasuggesties. Volgende slice moet klein blijven: banner/melding voor verlopen logmoment + knop om een `Draft`-regel voor dat logmoment aan te maken. Nog geen browser push notifications.
- Conflict/deduplicatiebeleid tussen NMEA2000 en NMEA0183 measurements
- Protocoltraceerbaarheid op measurement entities (`Protocol`-veld)
- Echte boot UDP-test met YDEN-03 op poort 2000/10110
- Expliciete **schijnbare wind** als aparte slice

### Raspberry Pi/Docker deployment en veilige shutdown

De eerste Raspberry Pi 4 Docker Compose deployment-smoke-test is geslaagd op 2026-05-26.

Gevalideerd:

- Raspberry Pi 4 Model B met 32 GB SD en Raspberry Pi OS Lite 64-bit.
- SSH via `bootmanager-pi.local`.
- GitHub private repo via SSH-key op de Pi.
- Pi bouwt Docker images lokaal vanaf `master`; geen zip-workflow.
- Lokale `.env` bevat `BOOTMANAGER_ENCRYPTION_KEY`, `BOOTMANAGER_JWT_KEY` en `BOOTMANAGER_BOOTSTRAP_PASSWORD`; secrets horen niet in GitHub.
- Docker ARM64 build werkt na commit `124c7af` met multi-arch .NET base images zonder `-arm64` suffix.
- Ingest control API werkt na commit `4ef3d73`; `0.0.0.0` wordt intern `http://*:5010/` voor `HttpListener`.
- `bootmanager-web` draait healthy op poort `5000`.
- `bootmanager-ingest` draait met UDP `10110` en control API `127.0.0.1:5010`.
- `/health` geeft `HTTP 200` met `{"status":"ok"}`.
- App is bereikbaar vanaf laptop via `http://<pi-ip>:5000`.
- Reboot-test geslaagd; beide containers komen automatisch terug.

Resterende aandachtspunten:

- UDP ingest vereist expliciete Docker-netwerkkeuze: poorten mappen of `host networking`.
- Web en Ingest kunnen in containers niet vanzelf via `localhost` met elkaar praten; gebruik service names, gedeeld Docker-netwerk of host networking.
- SQLite/database en capture logs moeten op persistente volumes staan.
- Containers moeten netjes reageren op `SIGTERM`, zodat writes en logs correct afsluiten.
- De Ingest control API blijft intern/lokaal bereikbaar en mag niet publiek op het bootnetwerk hangen.
- Omdat een Raspberry Pi/SD-kaart niet goed tegen hard uitschakelen kan, is een latere owner/admin UI-story gewenst: "Systeem veilig afsluiten".
- Die UI-knop moet via een beperkte lokale helper/service werken, niet via vrije shell-commando's vanuit Web.
- De gebruiker moet melding krijgen dat de stroom pas los mag als de Raspberry Pi volledig uit is.
- 32 GB SD en 1 GB RAM zijn voldoende voor weekendtest/proof-of-concept; productie/pilot vraagt liever eMMC/NVMe/SSD en 4 GB of 8 GB RAM.

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
12. **Raakt dit toekomstige Raspberry Pi/Docker deployment?**
    Bewaak dan UDP-netwerkkeuzes, persistente volumes, graceful shutdown en het vermijden van hard power-off risico's.

---

## 13. Praktische instructie aan Codex

Gebruik bij nieuwe werkstappen bij voorkeur deze aanpak:

### Eerst analyseren
- lees actuele code
- lees `.docs`
- raadpleeg bij elk nieuw idee of elke volgende-story keuze de legacy-scope analyse:
  - `.docs/legacy-analysis/scope-inventory.md`
  - `.docs/legacy-analysis/mapped-epics.md`
  - `.docs/legacy-analysis/legacy-coverage-register.md`
  - `.docs/legacy-analysis/proposed-backlog.md`
  - `.docs/legacy-analysis/implemented-or-obsolete.md`
- bepaal expliciet of het idee al in de legacy-scope staat, al in BootManagerV2 bestaat, deels bestaat, geparkeerd is, afhankelijk is van andere modules of nieuwe scope is
- controleer build/test-status als dat relevant is
- bepaal of docs, tests en code nog in sync zijn

### Daarna voorstellen
- formuleer één volgende kleine of middelgrote user story
- benoem scope, buiten scope, acceptatiecriteria en legacy coverage-impact
- vraag expliciet akkoord op de user story voordat je een Copilot-prompt maakt
- bewaar de goedgekeurde user story automatisch in het relevante `.docs/epics/*.md` bestand voordat je de Copilot-prompt maakt
- geef acceptatiecriteria
- maak een Copilot-prompt die de gewenste functionaliteit, grenzen en architectuurafspraken beschrijft
- vermijd een uitputtende bestandslijst of te gedetailleerde implementatie-instructies, tenzij de gebruiker daar expliciet om vraagt
- vertrouw erop dat Copilot de bestaande solutionstructuur, `.docs` en repository-instructies gebruikt voor de concrete code-indeling
- vermeld bij nieuwe features dat Copilot de relevante projectdocumentatie moet bijwerken
- vermeld bij documentatie datums expliciet de actuele sessiedatum en vraag Copilot om geen verouderde datum uit bestaande docs over te nemen

### Daarna reviewen
Na Copilot-output:
- scope checken
- architectuur checken
- build/test checken
- documentatie en datums checken
- legacy-dekking checken en `legacy-coverage-register.md` bijwerken voor geraakte legacy US-nummers
- acceptatietest geven
- commit/push-moment expliciet benoemen

---

## 14. Samenvatting in één alinea

Fase 1 (ingest foundation), Fase 2 (parserlaag), Fase 3a (VHW/MTW/DBT/DPT), Fase 3b (MWV/HDT/HDM), Fase 3c (RMC/GGA positie + motion), simulator NMEA 0183 output en de runtime/SQLite acceptatietest fase 3a-3c zijn afgerond. De eerste Raspberry Pi 4 Docker Compose deployment-smoke-test is geslaagd: Web en Ingest draaien op ARM64, `/health` is OK, de app is via het LAN bereikbaar en de reboot-test is geslaagd. TCP-ondersteuning voor YDEN-03 poort 1456 is voorlopig niet nodig; BootManager richt zich op de bewezen UDP NMEA 0183 route. Een logische volgende stap is de echte boot UDP-broadcasttest met YDEN-03/Teltonika of ontwerpkeuze rond conflict/deduplicatie en protocoltraceerbaarheid.

---

## 15. NMEA 0183 Epic – samenvatting

**Aanleiding:** De YDEN-03 gateway op de echte boot zendt NMEA 2000-data als NMEA 0183 sentences via UDP poort 2000 en 10110.

**Gefaseerde aanpak:**

| Fase | Inhoud | Status |
|------|--------|--------|
| **1 – Foundation** | Één gecombineerde UDP listener in Ingest, protocoldetectie op regelinhoud (`$`→NMEA0183), raw opslag | ✅ Geïmplementeerd (herzien: gecombineerde listener) |
| **2 – Parser laag** | `Nmea0183ParserService` in Application, sentence-type herkenning, veldextractie, checksum-validatie | ✅ Geïmplementeerd |
| **3a – Interpreters** | VHW, MTW, DBT/DPT | ✅ Geïmplementeerd |
| **3b – Interpreters** | MWV, HDT/HDM | ✅ Geïmplementeerd |
| **3c – Interpreters** | RMC/GGA positie + motion | ✅ Geïmplementeerd |
| **Simulator NMEA 0183** | Configureerbare NMEA 0183 output in Tools.Simulator | ✅ Geïmplementeerd (2026-05-18) |
| **Runtime/SQLite acceptatietest** | Handmatige end-to-end test fase 3a-3c via simulator NMEA0183-modus | ✅ Uitgevoerd (2026-05-18) |
| **TCP YDEN-03** | Poort 1456 lijkt bedoeld voor YDEN-software; voorlopig niet nodig | Geparkeerd |
| **Mogelijke volgende stap** | Echte boot UDP-test, conflict/deduplicatie of protocoltraceerbaarheid | Te kiezen |

**Vaste principes voor deze epic:**
- Bestaande NMEA2000 slices blijven intact.
- Raw opslag altijd leidend; ook onbekende sentences worden opgeslagen.
- `Protocol`-veld blijft op `NetworkMessages`; uitbreiden naar measurement entities is latere keuze.
- Ingest hoeft het YDEN-03 IP-adres niet te kennen; luisteren op `0.0.0.0` volstaat.
- Schrijven naar NMEA2000 of de YDEN-03 is buiten scope.

**Open ontwerpvragen (te beslissen vóór of tijdens Fase 3):**
- `Protocol` als string, enum of aparte tabel?
- `Protocol` toevoegen aan measurement entities voor traceerbaarheid?
- `VHW` – gecombineerde of opgesplitste interpretatie?
- `RMC` versus `GGA` als primaire positiebron?
- Conflict-resolutie bij dubbele metingen van NMEA2000 en NMEA 0183?
- Checksum-fout in Fase 3: afwijzen of alleen loggen? (Fase 2: optioneel, alleen gelogd)
- `MWV` windtype (werkelijk/schijnbaar) vastleggen in `WindMeasurement`?

Zie volledig: `.docs/epics/nmea0183-support.md`, `.docs/extraInfo/yden-03.md` en `.docs/features/nmea0183-parser-interpreter-architecture.md`

---

## 16. Simulator NMEA 0183 output – geïmplementeerd (2026-05-18)

De simulator ondersteunt nu configureerbare NMEA 0183 output naast de bestaande NMEA2000-achtige output.

### Nieuwe configuratieopties (`Simulator` sectie in appsettings.json)

| Optie | Standaard | Beschrijving |
|-------|-----------|--------------|
| `OutputMode` | `NMEA0183` | `NMEA0183` (standaard), `NMEA2000` of `Both`. Standaard NMEA0183, omdat de echte YDEN-03 route UDP NMEA 0183 gebruikt. Bij `Both` sturen beide stromen naar dezelfde ingestpoort; Ingest herkent het protocol per regel. |
| `TargetPort` | `10110` | UDP-doelpoort voor NMEA2000/raw-like output. Standaard gelijk aan `Nmea0183TargetPort` zodat de gecombineerde Ingest listener beide ontvangt. Alternatief: `2000`. |
| `Nmea0183TargetIp` | `127.0.0.1` | UDP-doeladres voor NMEA 0183 sentences |
| `Nmea0183TargetPort` | `10110` | UDP-doelpoort voor NMEA 0183 sentences. Standaard `10110` (gecombineerde Ingest listener). Alternatief: `2000`. |
| `IncludeNegativeTestSentences` | `false` | Stuurt ook ongeldige/negatieve testvarianten mee |

### NMEA 0183 sentence-types per tick

| Sentence | Verwachte measurement |
|----------|-----------------------|
| `IIVHW`  | SpeedThroughWaterMeasurement |
| `IIMTW`  | WaterTemperatureMeasurement |
| `IIDBT`  | DepthMeasurement |
| `IIMWV` (status A) | WindMeasurement |
| `IIHDT`  | HeadingMeasurement |
| `IIRMC` (status A) | PositionMeasurement + MotionMeasurement |
| `IIGGA` (fix 1) | PositionMeasurement |

### Negatieve testvarianten (alleen bij `IncludeNegativeTestSentences: true`)

| Sentence | Verwacht gedrag |
|----------|-----------------|
| `IIMWV` status V | Raw opslag, geen WindMeasurement |
| `IIRMC` status V | Raw opslag, geen Position- of MotionMeasurement |
| `IIGGA` fix 0 | Raw opslag, geen PositionMeasurement |
| `IIVHW` ongeldige checksum | Raw opslag, geen SpeedThroughWaterMeasurement |

### Opmerking: `OutputMode=Both`

Bij `OutputMode=Both` worden zowel `SimulationService` (NMEA2000) als `Nmea0183SimulationService` (NMEA0183) als afzonderlijke BackgroundServices geregistreerd. Beide initialiseren hun eigen runtime-state vanuit hetzelfde scenario. De waarden zijn daardoor **scenario-consistent** (zelfde startpositie, snelheid, koers, enz.), maar de twee stromen zijn **niet exact tick-gesynchroniseerd**: elke service heeft zijn eigen tick-loop en random variaties. Dit is voldoende voor functionele end-to-end tests, maar niet geschikt als exacte datasynchronisatie vereist is.

### Simulator starten in NMEA 0183 modus

```bash
# Optie 1: appsettings.json aanpassen (OutputMode: "NMEA0183")
cd src/BootManager.Tools.Ingest && dotnet run
cd src/BootManager.Tools.Simulator && dotnet run

# Optie 2: via environment override
cd src/BootManager.Tools.Simulator
dotnet run -- --Simulator:OutputMode=NMEA0183

# Optie 3: Both (NMEA2000 + NMEA0183 tegelijk; scenario-consistent, niet exact gesynchroniseerd)
dotnet run -- --Simulator:OutputMode=Both

# Met negatieve testvarianten
dotnet run -- --Simulator:OutputMode=NMEA0183 --Simulator:IncludeNegativeTestSentences=true
```

### Nieuwe bestanden

- `src/BootManager.Tools.Simulator/Options/SimulatorOptions.cs` – uitgebreid met `OutputMode`, `Nmea0183TargetIp`, `Nmea0183TargetPort`, `IncludeNegativeTestSentences`
- `src/BootManager.Tools.Simulator/appsettings.json` – nieuwe opties toegevoegd
- `src/BootManager.Tools.Simulator/NMEA0183/Nmea0183SentenceBuilder.cs` – nieuw: checksum-helper en alle sentence-builders
- `src/BootManager.Tools.Simulator/Services/Nmea0183SimulationService.cs` – nieuw: BackgroundService
- `src/BootManager.Tools.Simulator/Program.cs` – conditionele service-registratie op basis van `OutputMode`

### Buildresultaat

`dotnet build` geslaagd; bestaande SYSLIB0053 warnings in AesGcmEncryptionService zijn niet gerelateerd.

### Runtime/SQLite acceptatietest – uitgevoerd (2026-05-18, handmatig)

Test uitgevoerd via `BootManager.Tools.Simulator` met `Simulator:OutputMode=NMEA0183`, samen met draaiende Ingest en BootManager.Web.

**Positieve resultaten (measurements opgeslagen):**
- `NetworkMessages` – raw NMEA0183 berichten zichtbaar met `Protocol = 'NMEA0183'`
- `SpeedThroughWaterMeasurements` – via VHW
- `WaterTemperatureMeasurements` – via MTW
- `DepthMeasurements` – via DBT
- `WindMeasurements` – via MWV (status A)
- `HeadingMeasurements` – via HDT
- `PositionMeasurements` – via RMC (status A) en GGA (fix > 0)
- `MotionMeasurements` – via RMC (status A)

**Negatieve varianten gecontroleerd (raw opslag intact, geen measurement):**
- MWV status V → geen WindMeasurement
- RMC status V → geen Position- of MotionMeasurement
- GGA fixkwaliteit 0 → geen PositionMeasurement
- VHW met ongeldige checksum → geen SpeedThroughWaterMeasurement

Dit was een handmatige runtime/SQLite test, geen geautomatiseerde integratietest.

### TCP-status

TCP-ondersteuning voor YDEN-03 poort 1456 is voorlopig geparkeerd.
De huidige BootManager-flow gebruikt UDP NMEA 0183 via poort 2000/10110.
Als later blijkt dat TCP toch nodig is, kan dit alsnog als aparte transportstory worden opgepakt.

### Mogelijke volgende story

- Echte boot UDP-test met YDEN-03
- Conflict/deduplicatiebeleid tussen NMEA2000 en NMEA0183
- Protocoltraceerbaarheid op measurement entities

Zie volledig: `.docs/epics/nmea0183-support.md`, `.docs/extraInfo/yden-03.md` en `.docs/features/nmea0183-parser-interpreter-architecture.md`

---

## Capture logging in BootManager.Tools.Ingest

### Doel

Tijdens de echte boot-test kunnen we ruwe YDEN-data vastleggen voor analyse achteraf. Ingest schrijft per ontvangen regel direct een NDJSON-record naar een timestamped logbestand, vóór de API-post wordt uitgevoerd. Daardoor blijft de raw data beschikbaar, ook als de Web API traag is, hangt of tijdelijk niet bereikbaar is.

### Configuratie (`appsettings.json`)

Standaard is capture logging **uitgeschakeld**. Zet `Enabled` op `true` vóór de boot-test:

```json
"Ingest": {
  "CaptureLogging": {
    "Enabled": true,
    "Directory": "logs/ingest-capture",
    "FilePrefix": "ingest-capture"
  }
}
```

- `Enabled`: `false` (standaard) – geen bestand wordt aangemaakt.
- `Directory`: relatief aan de werkdirectory van het Ingest-proces, of absoluut zoals `C:\\tmp\\logs\\ingest-capture`.
- `FilePrefix`: voorvoegsel van de bestandsnaam; het volledige bestandsnaam-formaat is `{FilePrefix}-yyyyMMdd-HHmmss.ndjson`.

### Logformaat (NDJSON)

Één JSON-object per regel:

```json
// NMEA 2000 voorbeeld
{
  "receivedAtUtc": "2026-05-17T19:30:12.000Z",
  "remoteEndpoint": "192.168.1.100:2000",
  "detectedProtocol": "NMEA2000",
  "rawLine": "19:30:12.000 R 0A1B2C3D AA BB CC",
  "messageId": "0A1B2C3D",
  "payloadHex": "AA BB CC",
  "apiPostSucceeded": null,
  "apiStatusCode": null,
  "errorMessage": null
}

// NMEA 0183 voorbeeld
{
  "receivedAtUtc": "2026-05-17T19:30:13.000Z",
  "remoteEndpoint": "192.168.1.100:10110",
  "detectedProtocol": "NMEA0183",
  "rawLine": "$GPRMC,193013,A,5230.000,N,00456.000,E,0.0,0.0,170526,,*1A",
  "messageId": null,
  "payloadHex": null,
  "apiPostSucceeded": null,
  "apiStatusCode": null,
  "errorMessage": null
}
```

- De raw capture wordt bewust **vóór** de API-post geschreven; API-velden zijn daarom altijd `null`.
- NMEA 0183 records: `messageId = null`, `payloadHex = null`.
- Capture logging fouten blokkeren de ingest-flow nooit.
- `IngestCaptureLogger` gebruikt `AutoFlush = true`, zodat elke regel direct naar schijf wordt geschreven zonder afhankelijkheid van expliciete flush-aanroepen.

### Boot-test procedure

1. Start `BootManager.Web` (API + SQLite).
2. Zet in `src/BootManager.Tools.Ingest/appsettings.json`: `CaptureLogging.Enabled = true`.
3. Kies de YDEN UDP-poort: `10110` (aanbevolen) of `2000` (alternatief).
4. Start `BootManager.Tools.Ingest`.
5. Ingest logt bij start welk capture logbestand wordt gebruikt.
6. Na de test: stop Ingest en Web.

### Mee te nemen na de boot-test

- Capture logbestand(en) in `logs/ingest-capture/` (NDJSON)
- `BootManager.Web/bootmanager.db` (SQLite)

### Nieuwe bestanden (capture logging)

- `src/BootManager.Tools.Ingest/Options/CaptureLoggingOptions.cs` – configuratie-opties
- `src/BootManager.Tools.Ingest/Models/CaptureRecord.cs` – NDJSON-record model
- `src/BootManager.Tools.Ingest/Services/IIngestCaptureLogger.cs` – interface
- `src/BootManager.Tools.Ingest/Services/IngestCaptureLogger.cs` – implementatie
