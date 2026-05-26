# BootManager Status & Roadmap

## Current Implementation Status

### ✅ Completed Vertical Slices

| Measurement Type | PGN | Core Entity | Parser | Interpreter | Service | Storage | Status |
|------------------|-----|-------------|--------|-------------|---------|---------|--------|
| **Battery** | 127508 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Depth** | 128267 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Wind** | 130306 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Motion** (COG/SOG) | 129026 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Position** | 129025 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Heading** | 127250 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Speed Through Water** | 128259 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Water Temperature** | 130312 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |

### 📋 Backlog

#### High Priority

- [ ] **System Operations & Recovery – gecontroleerde Pi database reset**
  - **Aanleiding 2026-05-26:** Na de eerste geslaagde Raspberry Pi Docker Compose deployment is een veilige resetflow nodig voor ontwikkel-, test- en helpdeskscenario's. Handmatig databasebestanden verwijderen of `docker compose down -v` gebruiken is op de Pi te foutgevoelig.
  - **Status:** user story `SYS-RESET-1` is vastgelegd in `.docs/epics/system-operations.md` en moet binnenkort worden opgepakt.
  - **Doel:** operator-only reset via SSH/lokale beheercontext waarmee de actieve SQLite database eerst timestamped wordt bewaard of geback-upt, waarna BootManager opnieuw door bootstrap login en onboarding kan lopen.
  - **Niet-doel:** geen publieke webknop, geen remote reset endpoint, geen algemene backup/restore UI, geen verwijdering van `.env`, Git checkout, bijlagen, capture logs of volledige Docker volumes.
  - **Legacy-impact:** raakt `US0.5`, `US8.8` en `US8.14`; volledige backup/restore en standaardinstellingen herstellen blijven latere stories.
  - **Voorgestelde volgende actie:** maak hiervoor de eerstvolgende systeembeheer/deployment feature-branch en genereer daarna pas de Copilot-prompt.

- [ ] **Operationele instellingen via UI/database** *(fundament voor ingest/sampling)*
  - **Status 2026-05-23:** basis geïmplementeerd: `/settings` bevat operationele instellingen voor luisteradres, poorten, API basis-URL, raw opslagmodus, standaard sample-interval en capture logging.
  - **Status 2026-05-23 (slice 2):** Ingest haalt bij startup operationele instellingen op via `GET /api/operationalsettings/ingest`. appsettings.json blijft fallback als Web niet bereikbaar is. Settings worden niet live herladen tijdens runtime.
  - **Status 2026-05-23 (slice 3):** `RawStorageMode` en `DefaultSampleIntervalSeconds` zijn volledig toegepast:
    - `IIngestSamplingPolicy` en `IngestSamplingPolicy` implementeren per-stream-key sampling.
    - `RawStorageMode.All`: alle berichten naar API/database (bestaand gedrag).
    - `RawStorageMode.Sampled`: maximaal 1 bericht per stream key per interval.
    - `RawStorageMode.OffAfterSuccessfulParse`: voorlopig gelijk aan Sampled; post-parse cleanup volgt later.
    - Stream key is `Protocol:MessageId` (genormaliseerd). Capture logging onafhankelijk van sampling.
    - Bij interval ≤ 0 wordt fallback naar 10 seconden met waarschuwing.
    - Unit tests dekken alle modes en edge cases.
  - **Status 2026-05-23 (slice 4 - Ingest Control API & Settings Reload):** Live settings reload zonder procesrestart geïmplementeerd:
    - **Ingest control server:** HttpListener-gebaseerde minimale API op localhost (127.0.0.1:5010 standaard, configureerbaar).
    - **Endpoints:**
      - `GET /status`: geeft huidige runtime settings + restart-required flags voor UDP listener.
      - `POST /reload-settings`: haalt settings opnieuw op, appliceert veilige settings live (ApiBaseUrl, RawStorageMode, DefaultSampleIntervalSeconds), rapporteert restart-required voor ListenAddress/ListenPort/CaptureLoggingEnabled.
    - **Runtime settings service:** IIngestRuntimeSettings + IngestRuntimeSettings voor thread-safe live updates.
    - **Sampling policy updates:** IIngestSamplingPolicy.Update(mode, interval) voor live RawStorageMode en interval wijzigingen met state reset bij mode-wijziging.
    - **Web-side client:** IngestControlClient om control API aan te roepen.
    - **Settings endpoint:** POST /api/operationalsettings naast GET; appliceert settings en verzendt reload-commando naar Ingest via control client.
    - **UI feedback:** duidelijke melding van applied fields, restart-required fields en connection status (unreachable).
    - **Thread-safety:** UDP loop en reload kunnen gelijktijdig plaatsvinden via lock-based runtime settings.
    - **Security:** control API bindt standaard alleen op 127.0.0.1; geen binding op 0.0.0.0.
    - Unit tests valideren runtime settings thread-safety en live policy updates.
  - **Vervolg:** post-parse raw-retentie toepassen (Web moet succesvol parsen rapporteren).

- [ ] **Digitaal Logboek** *(epic – UI richting eindgebruiker)*
  - **Aanleiding:** Dataverwerking voor ondersteunde YDEN/NMEA0183-data werkt voldoende om richting een bruikbaar logboek te gaan.
  - **Referentie:** bestaand logboekvoorbeeld in `.docs/extraInfo/LogboekVoorbeeld.png`.
  - **Status:** basislogboek, reis-samenvatting, meetdatasuggesties, read-only detailpagina per logboekregel, browser-printweergave, akkoordflow (LogbookEntryStatus: Draft/Confirmed), en ontbrekende logmomenten feature (banner met overzicht en bulk-aanmaak tot 24 Draft-regels) geïmplementeerd op 2026-05-24. Delete-functionaliteit per regel toegevoegd.
  - **Status 2026-05-24 (accordeer-slice):** Detailpagina herontworpen als accordeerhulpmiddel:
    - **Context-header:** reisnaam, logboektijd lokaal, **logtijdvak lokaal** (HH:mm — HH:mm), status badge.
    - **Waarschuwingen:** duidelijke alerts voor ontbrekende periode-start en geen meetdata in logtijdvak.
    - **Logregelwaarden:** compacte overzicht van alle velden (Barometer, Log, Koers, Gem. SOG, Wind, GPS-status, Breedtegraad, Lengtegraad, Opmerkingen), met "Nog niet ingevuld" voor lege waarden.
    - **Meetdata-overzicht:** samplecounts per meettype in visueel 6-kolom grid (Positie, COG/SOG, Heading, Wind, Diepte, Watertemperatuur).
    - **Sampletabellen:** secundair onder "Samples"-kopje, alleen zichtbaar als samples beschikbaar.
    - **Verwijderde elementen:** "Bronmetingen voor automatische suggesties" sectie verwijderd; `CourseBron`, `WindBron`, `PositieBron` DTOs en bijbehorende service-methodes verwijderd. Reden: oude bronmetingen vóór logtijdvak conflicteren met domeinregel dat Draft-regels alleen periode-data gebruiken.
    - **DTO-cleanup:** `LogbookSourceMeasurementDto` klasse en properties uit `LogbookEntryDetailDto` verwijderd.
    - **Service-cleanup:** `BepaalCourseSourceAsync()`, `BepaalWindSourceAsync()`, `BepaalPositieSourceAsync()` verwijderd.
  - **Huidige detailweergave:** `/logbook/entries/{entryId:int}/details` toont accordeer-layout met waarschuwingen, logregelwaarden, meetdata-overzicht en sampletabellen.
  - **Status 2026-05-25 (confirm-slice):** Accordeer-knop op detailpagina geïmplementeerd:
    - **Bevestigknop:** "✓ Accorderen" knop zichtbaar voor Draft-regels in context-header (rechts).
    - **Knopgedrag:** disabled staat voor Confirmed-regels; loading-state tijdens bevestiging; spinner en "Bezig met accorderen..." tekst.
    - **Functionaliteit:** roept `ILogbookService.ConfirmEntryAsync` aan; herlaadt detail automatisch na succes met bijgewerkte status.
    - **Foutafhandeling:** foutmeldingen tonen in alert boven context-header; gebruiker blijft op detailpagina.
    - **Status-update:** na bevestiging wijzigt statusbadge van "Concept" naar "Akkoord" en verdwijnt knop.
    - Geen service-aanpassingen nodig (ConfirmEntryAsync bestond al).
    - `dotnet build` slaagt.
    - Printweergave ongewijzigd: Confirmed-only filters blijven intact.
  - **Huidige printweergave:** `/logbook/trips/{tripId:int}/print` toont alleen logboekinhoud in een printvriendelijke layout; PDF loopt via browser print. Alleen Confirmed-regels worden afgedrukt.
  - **Huidige missing-moments-feature (2026-05-24):** banner boven logboektabel toont totaalaantal gemiste logmomenten en compacte lijst (max 5 zichtbaar + "+ meer"); knop "Conceptregels aanmaken" maakt tot 24 Draft-regels in één beurt aan met automatische meetdatasuggesties (alleen periode-data); herberekening van banner na batch. Delete-knop (🗑) per regel met bevestigingsdialoog; na verwijdering herbereken banner.
  - **Volgende slice:** gerelateerde features als detailweergave verbeteren of browser push notifications.
  - Zie: [.docs/epics/digital-logbook.md](epics/digital-logbook.md)

- [ ] **First-Run Onboarding & Auth Simplification** *(epic – gefaseerd)*
  - **Doel:** Single-owner eerste-start flow met bootstrap wachtwoord, verplichte onboarding, en vereenvoudigde wachtwoord-only auth.
  - [x] **US1 (2026-05-24):** Auth UI vereenvoudigd naar wachtwoord-only
    - Loginpagina: alleen wachtwoordveld, "ingelogd blijven", login-knop
    - Pincode-veld verwijderd uit Login.razor
    - Recovery/master-key link verwijderd uit Login.razor
    - Settingspagina: pincode-card verwijderd
    - Settingspagina expliciet beschermd met owner-autorisatie
    - Loginpagina stuurt al ingelogde gebruikers terug naar dashboard
    - Niet-persistente login wordt ongeldig na applicatieherstart; "ingelogd blijven" blijft persistent
    - `/recover` niet meer bereikbaar via normale navigatie
    - OwnerLoginService gebruikt alleen wachtwoord (pincode/recovery services blijven als legacy)
    - `dotnet build` slaagt
    - Geen EF migration, geen bootstrap, geen onboarding
  - [x] **US2 (2026-05-24):** Bootstrap owner maken op eerste start
    - OwnerProfile uitgebreid met PasswordChangeRequired en OnboardingCompleted flags
    - IBootstrapOwnerService maakt automatisch eigenaar aan bij lege database
    - Bootstrap: naam `BootManager Owner`, e-mail `owner@bootmanager.local`
    - Wachtwoord uit configuratie `Bootstrap:DefaultPassword`
    - Production: moet expliciet via environment variable ingesteld worden (niet in appsettings.json)
    - Development: fallback naar `BootManagerDev123!` met waarschuwing
    - Startup faalt duidelijk als geen owner en geen password in Production
    - EF migration toegevoegd: `20260524183942_AddOwnerSetupFlags.cs`
    - Program.cs startup aangepast
    - 6 unit tests, alle slagen
    - Handmatig gevalideerd: Development bootstrap, geen dubbele owner, Production failure zonder password en Production start met bestaande owner
    - Aandachtspunt buiten US2: Production dashboard gaf `BootManager.Web.styles.css` 404 na login; later apart onderzoeken als Production/static asset issue
  - [x] **US3 (2026-05-24):** Verplichte onboarding-flow implementeren
    - `IOwnerSetupStateService` aangemaakt: haalt setup-status op (HasOwner, PasswordChangeRequired, OnboardingCompleted, SetupRequired)
    - `OnboardingGate.razor` component afdwingt routing voor ingelogde users met ongemaakte setup
    - Redirect naar `/onboarding` als setup verplicht is; verbied dashboard/settings/logboek
    - Whitelist routes vóór onboarding: `/login`, `/logout`, `/onboarding`, `/health`
    - Anonieme gebruikers krijgen normale login-flow via AuthorizeRouteView
    - `/onboarding` minimale placeholder-pagina voor ingelogde owner
    - Setup-klaar users worden van `/onboarding` naar `/dashboard` geleid
    - Geen redirect-loop voorkomen via expliciete whitelist en authenticated check
    - 5 unit tests voor OwnerSetupStateService, alle slagen
    - Build slaagt, 61/62 unit tests slagen (1 pre-existing failure in recovery test)
    - Handmatig gevalideerd: setup-required owner blijft op `/onboarding`; dashboard/settings/logbook redirecten terug; setup-klaar owner kan normale routes gebruiken
    - Testnotitie: lokale development-database staat na validatie op `PasswordChangeRequired=0`, `OnboardingCompleted=1`; voor US4-test tijdelijk terugzetten naar `1,0` of verse bootstrap database gebruiken
    - Geïmplementeerd: 2026-05-24
  - [x] **US5 (2026-05-24):** VesselProfile introduceren als datalaag voor onboarding
    - Nieuwe singleton entity `VesselProfile` voor bootgegevens per installatie (Id, VesselName, HomePort, CallSign, Mmsi, CreatedUtc, UpdatedUtc)
    - EF Core configuratie met tabel, constraints en index
    - DTOs: `VesselProfileDto` (output) en `UpdateVesselProfileRequestDto` (input)
    - Service interface `IVesselProfileService`: `GetOrCreateVesselProfileAsync()` en `UpdateVesselProfileAsync()`
    - Service implementatie: validates VesselName (verplicht, max 128), HomePort (optioneel, max 128), CallSign (optioneel, max 64), Mmsi (optioneel, max 32)
    - Singleton semantiek via service logica
    - DI: service geregistreerd als Scoped
    - EF migration: `20260524201623_AddVesselProfile.cs` met VesselProfiles tabel
    - 11 unit tests, alle slagen
    - Geen UI-wijzigingen
    - Build slaagt
    - Handmatig minimaal gevalideerd: Development-start past migratie toe en `VesselProfiles` tabel bestaat in SQLite
    - Aandachtspunt voor US4: service injecteren, GetOrCreate bij load, Update bij opslag; UI kan dun blijven
    - Geïmplementeerd: 2026-05-24
  - [x] **US4 (2026-05-24):** Onboardingformulier bouwen met eigenaar-, boot- en wachtwoordgegevens
    - `/onboarding` pagina vervangen door volledig formulier met drie secties: Eigenaargegevens, Bootgegevens, Wachtwoordwijziging
    - Eigenaargegevens: Naam (verplicht), E-mail (optioneel)
    - Bootgegevens: Bootnaam (verplicht), Thuishaven (optioneel), Roepnaam (optioneel), MMSI (optioneel)
    - Wachtwoord: Huidig (verplicht), Nieuw (verplicht, 8+ chars), Bevestiging (verplicht, moet gelijk zijn aan Nieuw)
    - `IOnboardingService` interface met `CompleteInitialOnboardingAsync(request)` methode
    - `OnboardingService` implementatie met volledige validatie logica:
      - Verplichte velden (naam, bootnaam)
      - Wachtwoord minimaal 8 tekens
      - Nieuw wachtwoord ≠ huidig wachtwoord
      - Bevestiging moet gelijk zijn aan nieuw
      - Huidig wachtwoord verificatie tegen OwnerProfile hash
    - Serviceflow: password verify → vessel get-or-create → vessel update → owner payload encrypt → password update → setup flags naar false/true → redirect
    - DTO's: `CompleteOnboardingRequestDto` (request) en `CompleteOnboardingResponseDto` (response)
    - DI: `IOnboardingService` geregistreerd als Scoped
    - UI: Razor component met formulier, foutweergave, validatie feedback, submit button en logout knop
    - Error handling: catch exceptions en return failure response met bericht
    - Redirect naar `/dashboard` na succes
    - 9 unit tests in OnboardingServiceTests, alle slagen
    - Build slaagt; gerichte onboardingtests slagen
    - Status 2026-05-25: verse-database runtime-test uitgevoerd. Bestaande `bootmanager.db` is tijdelijk hernoemd, app maakte een bootstrap owner aan, login met `BootManager123!` leidde naar `/onboarding`, formulieropslag leidde naar `/dashboard`, oud bootstrap-wachtwoord werd ongeldig, nieuw wachtwoord werkte en `/onboarding` redirectte daarna terug naar dashboard.
    - Status 2026-05-25: opslagbug gefixt waarbij `UpdateVesselProfileAsync()` faalde als er nog geen `VesselProfile` bestond. `OnboardingService` maakt het singleton bootprofiel nu eerst aan via `GetOrCreateVesselProfileAsync()` en werkt het daarna bij.
    - Status 2026-05-25: SQLite-validatie op de testdatabase gaf `PasswordChangeRequired=0`, `OnboardingCompleted=1` en de ingevulde vesselgegevens in `VesselProfiles`.
    - Acceptatiecriteria vervuld: alle verplichte velden gevalideerd, alle wachtwoordregels toegepast, owner/vessel/flags bijgewerkt bij succes
    - Geïmplementeerd: 2026-05-24
  - [x] **US6 (2026-05-25):** Documentatie en deployment-config bijwerken
    - `.env.example` uitgebreid met `BOOTMANAGER_BOOTSTRAP_PASSWORD`.
    - `docker-compose.yml` geeft `Bootstrap__DefaultPassword` door aan `bootmanager-web` via verplichte `.env` variabele.
    - Docker deploymentdocumentatie beschrijft eerste-start flow, bootstrap login, verplichte onboarding en reset bij vergeten wachtwoord.
    - Raspberry Pi deploymentdocumentatie beschrijft production bootstrap-configuratie, eerste-start flow en resetprocedure.
    - First install runbook beschrijft secrets, eerste login, onboardingcontrole en reset bij vergeten wachtwoord.
    - Vastgelegd: production zonder bestaande owner en zonder `Bootstrap:DefaultPassword` faalt bewust.
    - Vastgelegd: pincode/recovery/master-key zitten niet meer in de normale gebruikersflow.
    - Vastgelegd: bootgegevens wijzigen na onboarding is een toekomstige story.
  - Zie: [.docs/epics/first-run-onboarding.md](epics/first-run-onboarding.md)

- [ ] **Authentication & Authorization**
  - JWT-based API authentication
  - Role-based access control (Admin, User, Viewer)
  - Secure owner profile management

- [x] **Owner Profile & Vessel Settings (US1/US2/US3 afgerond)** *(slice gestart 2026-05-25)*
  - **Aanleiding 2026-05-25:** Na onboarding zijn eigenaargegevens, bootgegevens en wachtwoordbeheer niet duidelijk genoeg als normale beheerflow beschikbaar. Bootgegevens uit onboarding moeten achteraf wijzigbaar zijn; wachtwoord wijzigen moet expliciet gevalideerd en goed vindbaar blijven.
  - [x] **US1 (2026-05-25):** Eigenaargegevens wijzigen in instellingen
    - `/settings` toont huidige eigenaarnaam en e-mailadres uit encrypted owner payload.
    - Gebruiker kan beide velden wijzigen; naam is verplicht, e-mail optioneel.
    - IOwnerSettingsService uitgebreid met GetOwnerProfileAsync() en UpdateOwnerProfileAsync()
    - DTOs: GetOwnerProfileResponseDto, UpdateOwnerProfileRequestDto
    - OwnerSettingsService: decryptie/encryptie van encrypted payload met IEncryptionService
    - Settings.razor: nieuwe sectie "Eigenaarprofiel" boven "Wachtwoord wijzigen" met formulier
    - Validatie: naam verplicht, email optioneel/geldig, wachtwoordhash/flags blijven ongewijzigd
    - 8 unit tests voor GetOwnerProfileAsync/UpdateOwnerProfileAsync + edge cases, alle slagen
    - Validatie-, fout- en succesmeldingen Nederlands
    - `dotnet build` slaagt, 13/13 OwnerSettings-tests slagen
    - Handmatig gevalideerd: eigenaargegevens laden en wijzigen werkt, validatie werkt, refresh persistent
  - [x] **US2 (2026-05-25):** Bootgegevens wijzigen in instellingen
    - `/settings` toont bootgegevens uit `VesselProfile` (bootnaam, thuishaven, roepnaam, MMSI).
    - Gebruiker kan alle velden wijzigen; bootnaam is verplicht, overigen optioneel.
    - Lege optionele velden (null/empty) worden correct verwerkt.
    - Opslaan gebruikt bestaande `IVesselProfileService.UpdateVesselProfileAsync()`.
    - Bestaande validatie uit service (veldlengtes, verplicht bootnaam) werkt via `ArgumentException`.
    - Succesmeldingen en foutmeldingen tonen duidelijk.
    - Na refresh zijn wijzigingen persistent zichtbaar.
    - Onboardinggegevens kunnen achteraf worden aangepast.
    - `dotnet build` slaagt.
    - **Handmatige test vóór PR:** Settings openen, bootgegevens wijzigen/opslaan, refresh, leeg veld testen.
  - [x] **US3 (2026-05-25):** Wachtwoord wijzigen verifiëren
    - Handmatig getest door gebruiker: wachtwoord wijzigen in Settings werkt.
    - Legacy `US0.4` is hiermee administratief afgedekt voor de huidige single-owner scope; pincode blijft buiten de normale V2-flow.
  - [x] **US4 (2026-05-25):** Settings pagina ordenen met accordion
    - Bestaande `/settings` pagina herstructureerd naar Bootstrap 5 accordion-indeling.
    - Drie groepen: Account, Boot, Operationeel.
    - Account standaard open (collapse show); Boot en Operationeel standaard ingeklapt (collapse).
    - Account bevat: Eigenaarprofiel, Wachtwoord wijzigen.
    - Boot bevat: Bootgegevens.
    - Operationeel bevat: Operationele instellingen.
    - Alle bestaande formulieren, handlers, state en validatie behouden.
    - Geen businesslogica-wijzigingen; alleen visuele/structurele reorganisatie.
    - Alle saves werken nog: eigenaarprofiel, wachtwoord, bootgegevens, operationele instellingen.
    - Nederlandse meldingen intact.
    - `dotnet build` slaagt.
    - Settings.razor is het enige gewijzigde bestand.
  - Zie: [.docs/epics/owner-profile-settings.md](epics/owner-profile-settings.md)

- [ ] **Query API Enhancements**
  - Date range filtering on measurements
  - Pagination support
  - Aggregation queries (avg, max, min over time windows)

- [ ] **Data Visualization**
  - Blazor dashboard with real-time charts
  - Historical trend display
  - Map integration for Position data

#### Medium Priority

- [ ] **UI Framework Modernization**
  - **Aanleiding 2026-05-25:** BootManager gebruikt lokaal Bootstrap 5 CSS, terwijl `MainLayout.razor` Bootstrap 4.6.2 scripts, jQuery en SB Admin 2 laadt. Dit kan interactieproblemen veroorzaken bij componenten zoals accordions, dropdowns en modals.
  - **Doel:** Bootstrap/SB Admin-afhankelijkheden opschonen en één duidelijke UI-basis kiezen.
  - **Opties later onderzoeken:**
    - volledig Bootstrap 5;
    - een Blazor component library;
    - eigen lichte Blazor-componenten bovenop Bootstrap CSS.
  - **Niet onderdeel van US4:** Settings accordion wordt nu klein opgelost met Blazor-state, zonder frameworkmigratie.
  - **Acceptatie later:** regressietest van navbar, dropdowns, modals, layout en pagina-interacties.

- [ ] **Raspberry Pi/Docker deployment & veilige shutdown**
  - **Aanleiding 2026-05-23:** BootManager moet later op een Raspberry Pi in Docker kunnen draaien. Bij direct stroomloos maken kan een Raspberry Pi/SD-kaart en SQLite-database corrupt raken.
  - **Doel:** Docker deployment ontwerpen met correcte netwerkkeuzes voor UDP ingest, persistente volumes voor database/logs en een veilige afsluitflow.
  - **Status 2026-05-26:** eerste Raspberry Pi 4 Docker Compose deployment is geslaagd:
    - Raspberry Pi OS Lite 64-bit op Raspberry Pi 4 Model B met 32 GB SD.
    - SSH vanaf laptop werkt via `bootmanager-pi.local`.
    - GitHub private repo werkt via SSH-key op de Pi.
    - Repo staat schoon op `master` en wordt bijgewerkt via `git pull`.
    - Docker en Docker Compose geinstalleerd; images lokaal op ARM64 gebouwd.
    - `.env` lokaal aangemaakt met encryption key, JWT key en bootstrap password; secrets blijven buiten GitHub.
    - `bootmanager-web` draait healthy op poort `5000/tcp`.
    - `bootmanager-ingest` draait met UDP `10110/udp` en control API `127.0.0.1:5010`.
    - `/health` geeft `HTTP 200` met `{"status":"ok"}`.
    - App is bereikbaar vanaf laptop via `http://<pi-ip>:5000`.
    - Reboot-test geslaagd: beide containers kwamen automatisch terug.
    - 32 GB SD en 1 GB RAM zijn acceptabel voor weekendtest/proof-of-concept; productie/pilot vraagt liever eMMC/NVMe/SSD en 4 GB of 8 GB RAM.
  - **Docker fixes 2026-05-26:**
    - `124c7af`: .NET runtime base images gebruiken multi-arch `8.0-jammy` tags zonder niet-bestaande `-arm64` suffix.
    - `4ef3d73`: Ingest `HttpListener` vertaalt `0.0.0.0` naar prefix `http://*:5010/`.
  - **Aandachtspunten:**
    - UDP-poorten correct mappen of bewust `host networking` gebruiken. **Status:** Docker Compose port mapping voor `10110/udp` is gevalideerd.
    - Web en Ingest praten binnen Docker niet vanzelf via `localhost`; gebruik service names/netwerkconfiguratie of host networking. **Status:** service name `bootmanager-web` en Ingest control URL via Compose-netwerk zijn gevalideerd.
    - SQLite/database en capture logs moeten op persistente volumes staan. **Status:** volumes zijn ingericht en containers starten ermee; langdurige retentie/backup blijft open.
    - Ingest/Web moeten netjes reageren op container shutdown (`SIGTERM`) en open writes afsluiten.
    - Control API blijft intern/lokaal bereikbaar, niet publiek. **Status:** hostbinding `127.0.0.1:5010:5010` is gevalideerd.
  - **Latere UI-story:** owner/admin knop "Systeem afsluiten" met bevestiging. Web mag niet rechtstreeks vrije shell-commando's uitvoeren; gebruik een beperkte lokale helper/service die Docker/OS veilig afsluit.
  - **Gebruikersmelding:** "Wacht tot de Raspberry Pi volledig uit is voordat je de stroom loshaalt."

- [ ] **Extended Heading Fields**
  - Deviation storage (magnetic correction)
  - Variation storage (declination)
  - Reference type tracking (True vs. Magnetic)

- [ ] **NMEA 0183 Support** *(epic – gefaseerd)*
  - **Aanleiding:** YDEN-03 gateway zendt NMEA 2000-data als NMEA 0183 sentences uit via UDP (poort 2000 en 10110) en TCP (poort 1456).
  - **Fase 1 – Foundation ✅:** één gecombineerde UDP listener in Ingest, protocoldetectie op regelinhoud, raw NMEA 0183 opslag
  - **Fase 2 – Parser laag ✅:** `Nmea0183ParserService` in Application voor sentence-type herkenning, veldextractie en checksum-validatie; integratie in `NetworkMessageService` voor `Protocol == NMEA0183`
  - **Fase 3 – Interpreters ✅:** VHW, MWV, DBT/DPT, RMC/GGA, HDT/HDM en MTW verticale slices
  - **Simulator NMEA 0183 ✅:** standaard NMEA 0183 output via `BootManager.Tools.Simulator`
  - **Runtime/SQLite acceptatietest ✅:** handmatig uitgevoerd via simulator NMEA0183-modus
  - **Echte boot-test 2026-05-23:** UDP/raw opslag werkt; vervolgstories zijn deels afgerond voor AIS `!`-sentences, NMEA0183 `MessageId` en realistischer simulatorprofiel; capture replay is geparkeerd
  - [x] Story 1: Ingest herkent `$...` én `!...` als `Protocol = "NMEA0183"`; raw-like simulatorregels blijven `NMEA2000`
  - [x] Story 2: `Nmea0183ParserService` accepteert `$` en `!`; `!AIVDM` parsed als talker `AI`, type `VDM`
  - [x] Story 3: NMEA0183 krijgt stabiele niet-lege `MessageId` op basis van sentence-id, zodat derived measurements opgeslagen worden
  - [x] Story 4: Simulator krijgt een YDEN03-achtig profiel met `YD` talker-prefixen, AIS `!AIVDM`/`!AIVDO` en raw-only YDEN-sentences
  - [ ] Story 5: Replay-validatie voor echte NDJSON capture naar API/SQLite
  - [ ] Future story: Ingest haalt operationele instellingen op uit BootManager.Web en gebruikt die voor configureerbare ingest/sampling-retentie; ruwe niet-geparseerde data kan na succesvolle periodieke parsing optioneel worden opgeschoond
  - Bestaande NMEA2000 slices blijven intact
  - Raw opslag altijd leidend; onbekende sentences worden opgeslagen maar niet verwerkt
  - Zie: [.docs/epics/nmea0183-support.md](epics/nmea0183-support.md) en [.docs/features/nmea0183-parser-interpreter-architecture.md](features/nmea0183-parser-interpreter-architecture.md)

- [ ] **Logging & Diagnostics**
  - Serilog integration with structured logging
  - Correlation IDs for message tracing
  - Performance metrics collection

#### Low Priority

- [ ] **Additional PGN Support**
  - Barometric Pressure (PGN 130314)
  - Additional navigation data

- [ ] **Export/Reporting**
  - CSV export of measurements
  - Time-series reports
  - Compliance reporting (if needed)

- [ ] **Historical Data Management**
  - Archive old data
  - Data retention policies
  - Backup/restore procedures

## Architecture Status

### Strengths ✅

- **Clear layering:** Core → Application → Infrastructure → Web
- **Consistent patterns:** All measurement types follow identical structure
- **Error resilience:** Parse/interpret/store errors are non-fatal
- **Extensibility:** New PGN support requires minimal changes
- **Type safety:** Enums, DTOs, strong typing throughout

### Improvements Needed 🔧

- **API documentation:** Swagger/OpenAPI not yet configured
- **Integration tests:** Only the vertical slice pattern is validated
- **Performance metrics:** No benchmarking or load testing done
- **Error codes:** API returns generic HTTP status, needs specific error codes
- **Audit logging:** No audit trail for data changes
- **Data validation:** Range checks are service-level, not schema-level

## Known Limitations

1. **No persistent authentication state** - Every API call should include credentials (implement session/token caching)

2. **Simulator is independent** - Not integrated with API; requires Ingest tool as intermediary

3. **No real-time updates** - Blazor components don't get WebSocket notifications; need SignalR or polling

4. **Single-instance database** - No replication or clustering support

5. **Heading payload incomplete** - Deviation/Variation/Reference fields available but not decoded

## Recent Changes

### 2026-05-17: NMEA 0183 Fase 2 – Parser laag geïmplementeerd

**Toegevoegd:**
- `BootManager.Application/NetworkMessageParsing/DTOs/Nmea0183ParseResultDto.cs` – DTO met TalkerPrefix, SentenceType, Fields, ChecksumValid, ErrorMessage
- `BootManager.Application/NetworkMessageParsing/Services/INmea0183ParserService.cs` – interface voor NMEA 0183 parser
- `BootManager.Application/NetworkMessageParsing/Services/Nmea0183ParserService.cs` – implementatie: talker-prefix herkenning, veldextractie, XOR-checksum validatie

**Bijgewerkt:**
- `BootManager.Application/NetworkMessages/Services/NetworkMessageService.cs` – `INmea0183ParserService` geïnjecteerd; parse-aanroep toegevoegd voor `Protocol == "NMEA0183"`; parse-fouten blokkeren raw opslag niet
- `BootManager.Application/DependencyInjection.cs` – `INmea0183ParserService` als Scoped geregistreerd

**Gedrag:**
- Sentences zoals `$IIVHW,...`, `$GPRMC,...`, `$WIMWV,...` worden herkend op Talker + SentenceType
- Ongeldige of onbekende sentences worden gelogd; raw opslag blijft altijd leidend
- Bestaande NMEA2000 flow (via MessageId/PayloadHex) is ongewijzigd
- Runtime-tests zijn niet uitgevoerd; `dotnet build` en `dotnet test` zijn geslaagd (1 niet-gerelateerde authenticatietest gefaald)

**Status:** Geïmplementeerd, geen EF migrations

### 2026-05-17: NMEA 0183 Parser/Interpreter Architectuur – Documentatie

**Toegevoegd:**
- `.docs/features/nmea0183-parser-interpreter-architecture.md` – gedetailleerd ontwerpdocument voor Fase 2/3: parser/interpreter-aanpak, sentence-prioriteiten, entity-mappings, vaststaande keuzes en open ontwerpvragen

**Bijgewerkt:**
- `.docs/epics/nmea0183-support.md` – Fase 2 uitgebreid met scope, acceptatiecriteria en link naar architectuurdoc; sectie open ontwerpvragen toegevoegd
- `.docs/ARCHITECTURE.md` – Fase 1 als done gemarkeerd, parser/interpreter routeringschema toegevoegd, sentence-prioriteitstabel, link naar ontwerpdoc
- `.docs/TODO.md` – Fase 1 als done gemarkeerd, Fase 2 als eerstvolgende implementatie-story aangeduid
- `.docs/features/README.md` – verwijzing naar nmea0183-parser-interpreter-architecture.md toegevoegd
- `docs/bootmanager_codex_handoff.md` – eerstvolgende story en open vragen bijgewerkt

**Status:** Documentatie bijgewerkt, geen codewijzigingen

### 2026-05-17: NMEA 0183 Epic – Documentatie

**Toegevoegd:**
- `.docs/epics/nmea0183-support.md` – volledig epicdocument met gefaseerde aanpak
- `.docs/extraInfo/yden-03.md` – YDEN-03 gateway configuratie en context

**Bijgewerkt:**
- `.docs/ARCHITECTURE.md` – NMEA 0183 sectie toegevoegd (parallelle flow, protocol tagging, fasering)
- `.docs/TODO.md` – NMEA 0183 backlog-item uitgebreid
- `.docs/features/README.md` – NMEA 0183 epic en YDEN-03 context toegevoegd
- `docs/bootmanager_codex_handoff.md` – hardwarecontext en NMEA 0183 epic opgenomen

**Status:** Documentatie bijgewerkt, geen codewijzigingen

### 2026-03-27: Heading Slice Implementation

**Added:**
- `HeadingMeasurement` entity + EF configuration
- `HeadingMessageInterpreterService` (PGN 127250 decoder)
- `IHeadingMeasurementService` + implementation
- Parser recognition of PGN 127250
- Integration into `NetworkMessageService`
- Full dependency injection setup

**Documentation:**
- `.docs/ARCHITECTURE.md` - System design overview
- `.docs/DEVELOPMENT.md` - Dev workflow & guidelines
- `.docs/features/heading-slice-spec.md` - Heading slice detailed spec

**Status:** Complete (inclusief EF migratie `AddHeadingMeasurement`)

**Build:** ✅ Compileert succesvol

## Next Steps (Immediate)

### 1. Digitaal Logboek – vervolgslices

De basis voor het eindgebruikerslogboek is geïmplementeerd:

- `LogbookTrip` entity voor reis-header en reis-samenvatting (nu met per-reis `LogIntervalMinutes`).
- `LogbookEntry` entity voor logboekregels per uur/event (nu met `Status: Draft | Confirmed`).
- `/logbook` Blazor-pagina met kolommen uit `.docs/extraInfo/LogboekVoorbeeld.png`.
  - Banner met "Volgende logmoment verstreken" indien vervallen moment gedetecteerd.
  - Knop "Conceptregel maken" → maakt automatische Draft-regel voor gemist moment.
- Handmatige invoer voor opmerkingen/zeilvoering en basisvelden.
- Meetdatasuggesties op basis van bestaande measurements.
  - **2026-05-24:** Draft-regels gebruiken nu ALLEEN meetdata BINNEN het logtijdvak (kritieke veiligheidsfix).
  - Handmatige regels behouden "laatst bekende vóór logmoment" voor gebruikergemak.
- Read-only detailpagina per logboekregel met samples en samenvattingen.
  - Toont opgeslagen waarden, periode-samples en bronmetingen apart.
  - Lege Draft-regels tonen correct "Geen data" wanneer geen meetdata in logtijdvak.
- Browser-printweergave per reis zonder app-menu/topbar in de afdruk (Confirmed-only).

Zie: [.docs/epics/digital-logbook.md](epics/digital-logbook.md)

### 2. Later

- Sampling/raw-retentiebeleid toepassen op database-opslag.
- Server-side PDF-generatie.
- Bijlagen uploaden.
- Query API enhancements voor meetdata over tijdvakken.

## Deployment Checklist

- [ ] Production database setup (not SQLite)
- [x] Raspberry Pi Docker deployment ontwerp en eerste Pi 4 smoke test
- [x] Persistent volumes voor database en logs ingericht in Docker Compose
- [x] UDP ingest netwerkkeuze: port mapping gevalideerd op `10110/udp`
- [ ] Veilige shutdown-flow voor Raspberry Pi vanuit UI/helper-service
- [ ] Authentication implementation
- [ ] Rate limiting & API security
- [ ] Monitoring & alerting
- [ ] Backup & disaster recovery
- [ ] Performance load testing
- [ ] User acceptance testing (UAT)

## Metrics & Health Checks

### Current State
- **Lines of Code:** ~15,000 (excluding tests)
- **Test Coverage:** TBD (no unit tests yet)
- **Database Size:** ~1MB (typical, depends on message volume)
- **API Response Time:** <100ms (typical, unload tested)

### Desired State
- **Test Coverage:** >80%
- **API Response Time:** <50ms (p95)
- **Uptime:** 99.9%
- **Data Consistency:** 100% (ACID compliance)

---

**Last Updated:** 2026-05-24
**Maintained By:** Development Team
