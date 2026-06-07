# Current Codex Handoff

Updated: 2026-06-07.

## Current Task

`DSH-BUG-DBCTX-1: Gelijktijdige dashboard- en logboekqueries isoleren` is de
huidige afgeronde bugfix op branch
`codex/dashboard-dbcontext-concurrency`.

Status 2026-06-07:

- PR #85 is gemerged met mergecommit `1d0366d`.
- Lokale `master` is gelijk aan `origin/master` en de werkmap was schoon.
- De Raspberry Pi is bijgewerkt van `b7818f8` naar `1d0366d`.
- Op de Pi trad incidenteel EF Core-fout `A second operation was started on
  this context instance before a previous operation completed` op.
- Waargenomen rond de eerste NMEA-toggle op het dashboard en aansluitend laden
  van het logboek; refresh of een tweede poging werkte meestal.
- Codeanalyse:
  - Blazor-circuitservices delen een scoped `BootManagerDbContext`;
  - dashboardpolling gebruikt `System.Threading.Timer` met async callback;
  - polling kan overlappen met een vorige poll, NMEA-toggle of pagina-load.
- De story staat in `.docs/epics/dashboard-overview.md` en is op 2026-06-07
  door de gebruiker goedgekeurd.
- De gebruiker heeft Codex expliciet toestemming gegeven de applicatiecode voor
  deze bugfix zelf te wijzigen.
- Implementatie is lokaal gereed:
  - dashboardmetingen gebruiken via `IDbContextFactory` een context per
    laadoperatie;
  - dashboardpolling gebruikt een sequentiële, annuleerbare `PeriodicTimer`;
  - dispose annuleert en wacht de pollingloop af;
  - `OnboardingGate` gebruikt per navigatie een eigen async DI-scope en
    annuleert verouderde evaluaties.
- Gerichte tests slagen: dashboardcontext 1/1; dashboard/setup-state 6/6.
- Volledige unit-testsuite blijft 147/148 door de bestaande ongerelateerde
  recovery-test.
- Build slaagt met 0 errors en 11 bestaande nullable warnings.
- Geen migratie.
- De gebruiker heeft de lokale handmatige acceptatietest op 2026-06-07
  uitgevoerd en goedgekeurd:
  - meerdere automatische pollintervallen afgewacht;
  - NMEA herhaald aan/uit geschakeld;
  - direct tussen dashboard en logboek genavigeerd;
  - concurrencyfout trad niet opnieuw op.
- `.docs/TODO.md`, het dashboard-epic en legacy coverage voor `US7.1` en
  `US7.13` zijn administratief bijgewerkt.
- Status: klaar voor commit, push en PR.

Uitgesteld vervolg:

- `DSH-LIVE-3: Logboekactiviteit en snelle doorsteek vanaf dashboard` is door
  de gebruiker inhoudelijk akkoord bevonden, maar wordt nu bewust niet
  opgepakt.
- Na afronding van `DSH-BUG-DBCTX-1` blijft `DSH-LIVE-3` de eerstvolgende
  functionele dashboardstory, tenzij de gebruiker een andere prioriteit kiest.

Vorige taak:

`DEV-AGENT-1: Resourcezuinige Claude Code-workflow` is op branch
`codex/claude-code-workflow` geïmplementeerd als documentatiewijziging.

Status 2026-06-07:

- PR #84 is gemerged met mergecommit `f25dfc8`.
- Lokale `master` is fast-forward gesynchroniseerd met `origin/master` en was
  schoon.
- Nieuwe branch `codex/claude-code-workflow` is vanaf actuele `master`
  aangemaakt.
- De gebruiker heeft `DEV-AGENT-1` goedgekeurd.
- Nieuwe bestanden:
  - root `CLAUDE.md`;
  - `.codex/implementation-packet-template.md`;
  - `.docs/epics/development-agent-workflow.md`.
- `AGENTS.md` en `.codex/working-agreement.md` gebruiken nu
  `implementatie-agent` als generieke rol; Claude Code is de huidige primaire
  uitvoerder.
- `.codex/task-context-map.md` bevat een minimale Claude Code-contextflow.
- Claude laadt standaard niet de Codex-handoff, volledige TODO, legacy-analyse,
  ongerelateerde epics of brede source trees.
- Historische Copilot-vermeldingen in afgeronde stories blijven behouden.
- Geen applicatiecode, productroadmap of legacy coverage gewijzigd.
- Alleen statische documentatie; geen handmatige runtime-test nodig.

Vorige taak:

Branch `codex/logbook-header-autofill` is gesynchroniseerd met
`origin/master @ eba2f8a` nadat op 2026-06-06 nieuwe epic- en
user-storydocumentatie op GitHub was toegevoegd.

Actuele status:

- `origin/master` voegde twee documenten toe voor automatisch voorinvullen van
  logboekreisgegevens:
  - `.docs/epics/logbook-trip-autofill.md` als uitgebreide story;
  - `.docs/epics/logbook-trip-autofill-functional.md` als korte functionele
    samenvatting.
- Deze nieuwe story overlapte met de lokaal voorbereide en eerder goedgekeurde
  story in `.docs/epics/digital-logbook.md`, maar week inhoudelijk af:
  - de masterstory voegt vorige-reiswaarden, aankomsttijd en reisduur toe;
  - GPS-/havenherkenning en reverse geocoding vallen buiten de eerste slice;
  - tank-, motoruren- en logstandsensors zijn vervolgslices.
- De documentatie is rechtgetrokken:
  - `logbook-trip-autofill.md` is de canonieke overkoepelende story;
  - `logbook-trip-autofill-functional.md` is alleen een samenvatting;
  - `digital-logbook.md` verwijst naar de canonieke story en bevat geen
    afwijkende tweede scope meer;
  - `.docs/TODO.md` verwijst naar de eerste slice
    `LOG-TRIP-AUTO-1A`.
- `LOG-TRIP-AUTO-1A` is door de gebruiker goedgekeurd op 2026-06-07.
- De goedgekeurde tellerstandregels zijn:
  - `VesselProfile` bewaart actuele motoruren- en logstandwaarden;
  - een nieuwe reis neemt deze per veld alleen over na een expliciete
    knop-/icoonactie;
  - reisopslag schrijft alleen geldige hogere standen voort;
  - `0`, lege, negatieve of lagere reiswaarden verlagen een bestaande positieve
    profielwaarde niet;
  - een gebruiker mag in Settings bewust een lagere resetwaarde opslaan;
  - historische reizen worden niet opnieuw gescand, zodat een reset niet wordt
    teruggedraaid;
  - de gebruiker voert `LogstandEnd` in;
  - `LoggedMiles` wordt alleen-lezen berekend als
    `LogstandEnd - LogstandStart`.
- De slice omvat daarnaast bootnaam/boordtijd-defaults, aankomsttijd en
  reisduur. GPS/havenherkenning, tankdata en sensorgebaseerde tellerstanden
  blijven buiten scope.
- De goedgekeurde story staat in
  `.docs/epics/logbook-trip-autofill.md`.
- Copilot heeft de eerste implementatie uitgevoerd. Codex heeft die op
  2026-06-07 gereviewd en met expliciete toestemming van de gebruiker vier
  correcties zelf doorgevoerd:
  - tellerstanden worden bij aanmaken en normaal opslaan van een reis
    voortgeschreven, niet alleen bij `Beëindig reis`;
  - lege tellerstanden in Settings wissen de optionele profielwaarde;
  - een nieuwe reis gebruikt bootnaam uit `VesselProfile` en de actuele lokale
    boordtijd;
  - reisduur is zichtbaar in logboek en print, en overnameknoppen zijn
    uitgeschakeld met uitleg als geen profielwaarde beschikbaar is.
- Technische verificatie na deze correcties:
  - `dotnet build BootManager.sln` geslaagd met 0 errors; 11 bestaande nullable
    warnings in `Dashboard.razor`;
  - gerichte VesselProfile-tests geslaagd: 16/16;
  - `git diff --check` schoon.
- De gebruiker heeft de volledige runtimeflow op 2026-06-07 handmatig getest
  en geaccepteerd.
- Runtime-test op 2026-06-07 vond dat het oude invoerveld `Gelogde mijlen`
  verwarrend als eindstand werd gebruikt. Dit is gecorrigeerd:
  - `Logstand eind` is nu het invoerveld;
  - `Gelogde mijlen` is alleen-lezen en wordt berekend;
  - voortschrijving gebruikt begin- en eindstand rechtstreeks;
  - eindstand lager dan beginstand wordt geweigerd;
  - migratie `AddLogbookTripLogstandEnd` voegt het nieuwe veld toe en leidt
    historische eindstanden waar mogelijk af uit de oude velden.
- Administratieve afronding is uitgevoerd:
  - `LOG-TRIP-AUTO-1A` staat als geïmplementeerd en geaccepteerd in de
    logboekdocumenten;
  - Owner Profile & Vessel Settings US5 staat als afgerond;
  - `.docs/TODO.md` vinkt de slice af;
  - legacy coverage voor `US1.2`, `US5.3`, `US5.6` en `US5.11` is bijgewerkt;
  - README-projectstatus is opnieuw gegenereerd.
- De branch is klaar voor commit, push en PR.

Afgeronde processtap na `LOG-TRIP-AUTO-1A`:

- De overstap van Copilot naar Claude Code als programmeeragent is voorbereid.
- Codex blijft architect-, proces-, review- en testassistent.
- De workflow is programmeeragent-neutraal gemaakt in `AGENTS.md` en
  `.codex/working-agreement.md`.
- Een compacte, resourcezuinige `CLAUDE.md` is toegevoegd.
- `.codex/task-context-map.md` gebruikt een minimaal implementatiepakket.
- Een herbruikbaar implementation-packet-template bevat exacte scope,
  verwachte write-set, relevante bestanden, gerichte tests en kort
  opleverformat.
- Historische Copilot-vermeldingen in afgeronde stories blijven ongewijzigd.
- De wijziging staat op de aparte branch `codex/claude-code-workflow`.

Vorige afgeronde taak:

`README Projectstatusoverzicht` is direct op `master` gezet en afgerond.

Status 2026-06-01:

- Root `README.md` bestaat nu en wordt door GitHub als startpagina getoond.
- README bevat een gegenereerd projectstatusoverzicht met:
  - totale voortgang voor BootManagerV2 huidige epics;
  - totale voortgang voor legacy scope;
  - statusbalk per BootManagerV2-epic;
  - statusbalk per legacy epic.
- Generator toegevoegd: `scripts/update-readme-status.ps1`.
- Legacy-percentages worden automatisch berekend uit `.docs/legacy-analysis/legacy-coverage-register.md`.
- BootManagerV2-epicpercentages worden expliciet onderhouden in het generator-script zolang de epicdocumenten nog niet uniform genoeg zijn voor betrouwbare automatische parsing.
- Rekenregel:
  - `Done` en `Replaced` tellen als 100%;
  - `Partial` telt als 50%;
  - `Open` telt als 0%;
  - `Parked` en `Obsolete` tellen niet mee in actieve scope.
- Commit `1d04ea0 Add README project status overview` is gepusht naar `origin/master`.
- Lokale checkout staat op `master`, gelijk met `origin/master`, met schone worktree.

Vaste documentatieregel vanaf nu:

- Bij iedere documentatie-update die projectstatus, stories, epics, TODO of legacy coverage raakt, moet ook `./scripts/update-readme-status.ps1` worden gedraaid.
- Controleer daarna de README-diff en commit de README-statusupdate mee.
- Alleen puur technische docs zonder statusimpact, zoals een typo of klein runbookcommando, hoeven de README-statussectie niet per se te wijzigen.

Volgende logische actie:

- Als de gebruiker de laatste dashboardwijzigingen op de Raspberry Pi wil controleren: Pi op `master` bijwerken en containers rebuilden/starten.
- Daarna een nieuwe story kiezen. Waarschijnlijke kandidaten blijven: bronvoorkeuren/source registry, verdere logboekintegratie van tank-/tripdata, of dashboardverfijning.

Vorige afgeronde taak:

`DSH-LIVE-5: Alleen Beschikbare Meetwaarden En Verberg-/Herstelbare Tegels` is afgerond en gemerged naar `master`.

Status 2026-06-01:

- Dashboard toont alleen meetwaardetegels waarvoor daadwerkelijk data beschikbaar is.
- Dashboard toont tankniveaus uit `FluidLevelMeasurements`, alleen voor tanks waarvoor records bestaan.
- Invalid/unknown tankniveau blijft zichtbaar als bestaande tank met `Geen niveau bekend`.
- Tegels kunnen met `X` worden verborgen en via een herstel-lijst met `+` worden teruggezet.
- Verborgen tegels worden bewaard in browser `localStorage` en blijven na refresh/herstart verborgen.
- Dashboard is opgesplitst in herbruikbare Blazor-componenten onder `BootManager.Web/Components/Dashboard/`.
- CSS/component-regressies zijn gecorrigeerd:
  - dashboardtegels zijn weer compact;
  - kompasletters blijven rechtop leesbaar terwijl heading-up posities blijven kloppen.

Verificatie:

- `dotnet build BootManager.sln` geslaagd met 0 warnings/errors.
- `git diff --check` schoon.
- `dotnet test src\BootManager.Tools.Simulator.Tests\BootManager.Tools.Simulator.Tests.csproj --no-build` geslaagd: 5 tests.
- Volledige unit test run had één bestaande, ongerelateerde failure: `OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`.
- User heeft lokaal gevalideerd:
  - tegels zijn zichtbaar;
  - meerdere tegels aan/uit gezet;
  - verborgen tegels blijven ook na herstart verborgen;
  - UI is akkoord.

Administratieve status:

- `.docs/epics/dashboard-overview.md` markeert `DSH-LIVE-5` als geïmplementeerd en lokaal gevalideerd.
- `.docs/TODO.md` bevat `DSH-LIVE-5` onder Dashboard & Live Overzicht.
- `.docs/legacy-analysis/legacy-coverage-register.md` is bijgewerkt voor `US5.3`, `US7.1`, `US7.2`, `US7.11`, `US7.12`, `US7.13` en `US10.1`.
- PR #82 (`codex/dashboard-fluid-levels`) is gemerged op 2026-06-01 met merge commit `06241a1`.
- Lokale checkout staat weer op `master`, is fast-forward bijgewerkt naar `origin/master` en de worktree was schoon na de merge-afhandeling.

Volgende actie:

- Als de gebruiker dit op de Raspberry Pi wil controleren: Pi op `master` bijwerken, containers rebuilden/starten en dashboard controleren op tanktegels, alleen beschikbare meetwaarden en persistent verborgen tegels.
- Daarna kan de volgende story gekozen worden. Waarschijnlijke kandidaten: bronvoorkeuren per meetwaarde/source registry, verdere logboekintegratie van tank-/tripdata, of dashboardverfijning.

Vorige afgeronde taak: `NMEA Story 9: Fluid Level interpreter voor meerdere tanktypes` is afgerond op branch `codex/fluid-level-interpreter`.

Status Story 9:

- Nieuwe `FluidLevelMeasurement` slice toegevoegd voor NMEA 2000 PGN `127505` Fluid Level.
- Gatewayregels `$PCDIN` en `$MXPGN` met PGN `01F211` worden via de NMEA0183 raw-route herkend en opgeslagen.
- Ondersteunt meerdere tanktypes en meerdere tanks per type via `RawFluidType`/`FluidType` + `FluidInstance`.
- Decode volgt de Pi-analyse:
  - raw fluid type `0` => fuel;
  - raw fluid type `1` => fresh water;
  - raw fluid type `2` => gray water;
  - raw fluid type `5` => oil;
  - level scale `0.004%`;
  - `0x7FFF` => invalid/unknown level.
- Bronidentiteit komt uit NMEA-inhoud en niet uit UDP/YDEN endpoint.
- Simulatorprofiel `YDEN03` zendt naast bestaande navigatie-, wind-, diepte-, temperatuur- en AIS-regels ook Fluid Level `PCDIN`/`MXPGN` gatewayregels uit.
- Duplicate handling is pragmatisch: `MXPGN` wordt overgeslagen wanneer er al een `PCDIN`-meting in dezelfde minuut voor dezelfde tank bestaat. De latere source-registry/source-preference stories moeten dit robuuster normaliseren.

Verificatie:

- `dotnet build BootManager.sln` geslaagd met 0 warnings/errors.
- `dotnet test BootManager.UnitTests\BootManager.UnitTests.csproj --filter FluidLevel` geslaagd: 19 tests.
- `dotnet test src\BootManager.Tools.Simulator.Tests\BootManager.Tools.Simulator.Tests.csproj` geslaagd: 5 tests.
- Handmatige lokale validatie met `BootManager.Web`, `BootManager.Tools.Ingest` en YDEN03 simulator geslaagd.
- Databasecontrole bevestigde recente `PCDIN`/`MXPGN` raw opslag en gevulde `FluidLevelMeasurements` voor:
  - fuel instance 0: 100%, 150L;
  - fuel instance 1: 99.62%, 150L;
  - fresh water instance 0: 17.7%, 200L;
  - fresh water instance 1: 17.7%, 200L;
  - gray water instance 0: 50%, 100L;
  - oil/invalid example: level null, capacity 100L, `IsLevelInvalid = true`.

Administratieve status:

- `.docs/epics/nmea0183-support.md` markeert Story 9 als geïmplementeerd en lokaal gevalideerd.
- `.docs/TODO.md` vinkt Story 9 af.
- `.docs/features/fluid-level-slice-spec.md` documenteert de nieuwe slice.
- `.docs/features/README.md` indexeert de Fluid Level slice.
- `.docs/legacy-analysis/legacy-coverage-register.md` is bijgewerkt voor `US5.3`, `US5.11`, `US8.5` en `US10.1`.

Volgende actie:

- Branch committen, pushen en een ready PR maken.
- Na merge naar `master` kan de Raspberry Pi worden bijgewerkt als de gebruiker echte actuele tankmetingen of simulatorgedrag op de Pi wil valideren.

Vorige afgeronde taak: `NMEA Story 8: Bronidentiteit en bronvoorkeuren ontwerpen` is uitgewerkt op branch `codex/source-identity-design`.

Story 8 ontwerpstatus:

- User story is goedgekeurd door de gebruiker op 2026-05-31.
- Ontwerp is vastgelegd in `.docs/features/source-identity-preferences-design.md`.
- `.docs/epics/nmea0183-support.md` markeert Story 8 als goedgekeurd en uitgewerkt.
- `.docs/TODO.md` vinkt Story 8 af.
- `.docs/legacy-analysis/legacy-coverage-register.md` is bijgewerkt voor `US8.5` en `US9.5`.
- Kernbesluit: bronidentiteit moet uit NMEA-inhoud komen, niet uit UDP/YDEN transport.
- UDP/YDEN endpoint blijft alleen transportdiagnostiek.
- NMEA 0183 bronbasis: protocol + talker-id + sentence type, aangevuld met kwaliteits-/statusvelden waar nuttig.
- NMEA 2000 via `PCDIN`/`MXPGN`: bronbasis is PGN + source address waar beschikbaar + instance/subtype/payloadmetadata.
- Aanbevolen vervolgstories: SourceIdentity value object/parsercontract, source registry/last-seen, source preferences service, Settings UI, daarna Fluid Level interpreter met dit bronmodel.
- Docs/design-only; geen runtime-test nodig.

Vorige afgeronde taak: `NMEA Story 7: Pi database analyseren voor logboekvelden en bronduplicatie` is uitgevoerd op branch `codex/pi-database-analysis`.

Analyse-uitkomst:

- Lokale Pi database-export `bootmanager-db-20260531.tar.gz` is uitgepakt naar `.analysis/pi-db-20260531/bootmanager.db` en read-only geanalyseerd.
- De export en `.analysis/` zijn lokale onderzoeksdata en worden niet gecommit.
- Analyse is vastgelegd in `.docs/analysis/pi-database-2026-05-31.md`.
- De database bevat 444.454 ruwe `NetworkMessages` over `2026-05-29 12:18:21 UTC` t/m `2026-05-31 10:05:48 UTC`.
- Bestaande interpreters vullen positie, motion, heading, wind, snelheid door water en watertemperatuur.
- `DepthMeasurements` en `BatteryMeasurements` zijn leeg in deze export.
- Tankniveau is zichtbaar als sterke kandidaat via `PCDIN`/`MXPGN` met PGN `01F211`, waarschijnlijk NMEA 2000 PGN `127505` Fluid Level.
- Er lijken minimaal drie fluid-kanalen zichtbaar: brandstof instance 0, water instance 0 en water instance 1.
- `YDVLW` is een sterke kandidaat voor logstand/afstand door water.
- Motoruren zijn niet duidelijk zichtbaar in deze export.
- De huidige `Source`-waarde lijkt Docker/UDP-endpointmetadata en is niet stabiel genoeg als fysieke apparaatidentiteit.
- Aanbevolen vervolg: Story 8 bronidentiteit/bronvoorkeuren ontwerpen, daarna Story 9 starten met PGN 127505 Fluid Level, daarna `YDVLW`.

Administratieve status:

- `.docs/epics/nmea0183-support.md` markeert Story 7 als geanalyseerd en linkt naar het analysedocument.
- `.docs/TODO.md` vinkt Story 7 af.
- `.docs/legacy-analysis/legacy-coverage-register.md` is bijgewerkt voor `US5.3`, `US5.11`, `US8.5` en `US9.5`; alle relevante legacy-items blijven `Partial`, omdat dit analyse/documentatie is en nog geen nieuwe interpreter of UI oplevert.

Vorige afgeronde taak: `SYS-SHUTDOWN-1: BootManager Pi Afsluiten Vanuit Beheerderpagina` is complete.

Final status:

- PR #76 delivered the first shutdown UI/API/systemd-helper implementation and was merged into `master` with merge commit `ad07958`.
- The first Pi validation found two separate problem areas:
  - host-side systemd/helper setup issues;
  - the Blazor Server UI used a server-side `HttpClient` self-call to the authenticated `/api/system/shutdown` endpoint, so the browser auth cookie was not carried and the shutdown action did not reach the controller.
- Host-side Pi fixes were folded back into `.docs/deployment/`:
  - `/var/log/bootmanager` must exist because `ProtectSystem=strict` and `ReadWritePaths=/var/log/bootmanager` require the directory;
  - `shutdown-helper.sh` uses `read -r COMMAND`;
  - `bootmanager-shutdown@.service` does not contain `Type=accept`, `Restart=always`, or `RestartSec=5`;
  - `Accept=yes` remains only in `bootmanager-shutdown.socket`.
- PR #77 fixed the UI path by replacing the Blazor Server `HttpClient` self-call with a direct server-side call to `IShutdownService.InitiateShutdownAsync()`.
- PR #77 was merged into `master` with merge commit `b7818f8`.
- Local `master` was fast-forwarded to `b7818f8`.
- The Raspberry Pi was updated from `master` to `b7818f8`, containers were rebuilt/restarted, and `bootmanager-web` started cleanly.
- User validated the real shutdown flow on the Pi:
  - clicking `BootManager Pi afsluiten` caused the browser connection/page to stop immediately;
  - SSH became unavailable;
  - the Pi shutdown therefore succeeded end-to-end.
- Validation before PR #77:
  - `dotnet build BootManager.sln` passed with `0` warnings/errors;
  - `dotnet test BootManager.UnitTests\BootManager.UnitTests.csproj --filter SystemShutdown --no-build` passed `13/13`.

Administrative status:

- `.docs/epics/system-operations.md` marks `SYS-SHUTDOWN-1` complete and Pi-validated.
- `.docs/TODO.md` marks the safe shutdown deployment checklist item complete.
- `.docs/legacy-analysis/legacy-coverage-register.md` keeps `US8.6 Raspberry Pi-configuratie beheren` as `Partial`, now explicitly including Pi-validated in-app shutdown. It remains partial because full in-app Pi system status/configuration management, persistent warning/error overview, and long-duration observations remain open.

Next action:

- No further shutdown action is required.
- The next story should be chosen from the open roadmap, with legacy-scope check first as usual.

The first Raspberry Pi 4 Docker Compose deployment-smoke-test has succeeded:

- Raspberry Pi 4 Model B, 32 GB SD, Raspberry Pi OS Lite 64-bit.
- SSH access via `bootmanager-pi.local`.
- GitHub private repo access via SSH-key on the Pi.
- Pi builds Docker images locally from `master`; no zip workflow.
- Required local `.env`: `BOOTMANAGER_ENCRYPTION_KEY`, `BOOTMANAGER_JWT_KEY`, `BOOTMANAGER_BOOTSTRAP_PASSWORD`.
- Docker ARM64 build works after commit `124c7af` removed non-existent `-arm64` .NET base image tags.
- Ingest control API works after commit `4ef3d73` maps `0.0.0.0` to HttpListener prefix `http://*:5010/`.
- `bootmanager-web` is healthy on port 5000.
- `bootmanager-ingest` is up with UDP 10110 and localhost-bound control API 5010.
- `/health` returns `HTTP 200` with `{"status":"ok"}`.
- App is reachable on the LAN via `http://<pi-ip>:5000`.
- Reboot test succeeded; both containers came back automatically.
- 32 GB SD and 1 GB RAM are acceptable for weekend test/proof-of-concept, but production/pilot should prefer eMMC/NVMe/SSD and 4 GB or 8 GB RAM.

Pi update instruction from user:

- Do not assume the Raspberry Pi must pull immediately after every push.
- The Raspberry Pi only pulls `master`. Never tell the user to pull or test a feature branch on the Pi.
- Feature-branch implementation and pre-PR validation happen on the development computer/local dev environment, not on the Pi.
- Do not imply that the Pi already has a feature-branch change before that change has been merged/pushed to `origin/master`.
- For docs-only changes, normally do not tell the user to update the Pi.
- If a Pi update is needed, explicitly tell the user the exact SSH commands and whether containers must be rebuilt, only restarted, or left running.
- Use this default command set only when code/container changes need to run on the Pi:
  `cd ~/BootManagerV2`, `git pull`, `docker compose build`, `docker compose up -d`, `docker compose ps`, `curl -i http://localhost:5000/health`.
  For restart-only: `cd ~/BootManagerV2`, `docker compose restart`, `docker compose ps`.

Relevant docs updated:

- `.docs/docker-deployment.md`
- `.docs/pi-first-install-runbook.md`
- `.docs/raspberry-pi-deployment.md`
- `.docs/extraInfo/yden-03.md`
- `.docs/TODO.md`
- `.docs/legacy-analysis/legacy-coverage-register.md`
- `.docs/legacy-analysis/mapped-epics.md`
- `.docs/legacy-analysis/implemented-or-obsolete.md`

Current completed stories:

- `US7: Legacy Register Owner Route En Menu Verwijderen` is complete and was merged via PR #64 on 2026-05-26.
- `SYS-RESET-1: Gecontroleerde Database Reset Voor Pi Testinstallatie` is complete and was merged via PR #65 on 2026-05-27.
- `SYS-RESET-1` was manually validated on the Raspberry Pi on `master`:
  - reset script executed successfully;
  - timestamped backup was created;
  - `bootmanager-web` returned to healthy state;
  - bootstrap login worked again;
  - onboarding was forced again;
  - after onboarding, only the newly chosen password worked.
- After that validation, a follow-up master commit `1db5534` documented the explicit `sudo` requirement for `scripts/reset-database.sh`.
- The first real Raspberry Pi field test with boat data succeeded on 2026-05-29 while the Pi was on `master @ 1db5534`.
  - `bootmanager-web` was healthy.
  - `bootmanager-ingest` received real boat-network traffic on UDP `10110`.
  - Ingest posted successfully to `http://bootmanager-web:5000/api/networkmessages`.
  - The Web API returned repeated `HTTP 201 Created`.
  - Raw `NetworkMessages` and multiple interpreted measurement tables were confirmed via logs.
  - No recent `error`, `exception` or `fail` messages were seen during the targeted log checks.
  - Observed open points:
    - `sqlite3` was not available on the Pi host or in the container, so direct SQL counts were not run;
    - repeated `GGA` fix-quality `0` warnings created log noise but did not break the pipeline;
    - UI validation of live data is still open;
    - longer-duration validation for WAL growth, retention and capture logs is still open.

Next near-term story decision:

- The digital logbook trip-header improvement for `vertrek- en aankomstmoment met datum en tijd` was merged to `master` via PR #66 on 2026-05-27.
- It was manually validated in the local dev environment:
  - trip creation with a non-round departure time persisted correctly;
  - later arrival time persisted correctly;
  - missing-moments and detail periods still aligned to the true departure moment;
  - print view showed departure and arrival consistently without duplicate date fields.
- Legacy `US5.6 Logboekheader invullen` remains `Partial`, but now explicitly covers departure and arrival as full date+time in logbook and print context.
- The digital logbook closeout flow for `US5.14 Logboek afronden bij aankomst` was merged to `master` via PR #68 on 2026-05-27.
- It was manually validated in the local dev environment:
  - an open trip without arrival time could be completed with a required arrival datetime;
  - an open trip with existing arrival time could be completed without data loss;
  - completed trips no longer showed open-trip actions such as adding new entries;
  - completed trips no longer produced missing-moments behavior;
  - print view remained usable after closeout.
- Legacy `US5.14 Logboek afronden bij aankomst` is now functionally covered.
- During implementation an EF migration mismatch and a local dev SQLite schema mismatch were found and fixed before merge.
  - The broken intermediate migration state was never deployed to the Raspberry Pi.
  - Because the Pi was offline long before this story and only pulls `master`, this specific local-dev migration issue is not expected there.
- The next logical follow-up story inside the digital logbook is now route map and/or true PDF/CSV export.
- For Raspberry Pi deployment hardening, `SYS-DEPLOY-LEAN-1` is now captured in `.docs/epics/system-operations.md` and should be brought back explicitly before the first deployment for a different boat owner / different Pi.
- `SYS-ANALYSIS-1: Technische analysepagina in de webinterface` was merged to `master` via PR #70 and manually validated on the Raspberry Pi on 2026-05-29.
  - The Blazor page uses `IAnalysisService` directly instead of self-HTTP.
  - The page shows counts for raw `NetworkMessages` and measurement tables in a selected time window.
  - JSON/CSV export works via the existing Blazor download helper.
  - Count queries use `IRepository.CountAsync(...)` instead of loading lists only to count them.
  - Test export evidence is stored at `.docs/extraInfo/analysis-20260528-1546-20260529-1546.json`.
  - Pi export evidence is stored at `veldtests/analysis-20260528-1635-20260529-1635.json` and `veldtests/analysis-20260528-1635-20260529-1635.csv`; both show `431021` raw `NetworkMessages` for the selected 24-hour window.
  - `SYS-ANALYSIS-2` was merged via PR #73 and manually validated on the Raspberry Pi on 2026-05-29. Native `datetime-local`, `date` and `time` controls were rejected because browser segment editing remained unreliable. The analysis page now uses two plain text fields (`Van`, `Tot`) with format `dd-MM-jjjj uu:mm`; parsing/validation happens only when clicking `Analyseren`. UX is not ideal, but acceptable for this fix.
  - Persistent warning/error storage is still open; the page reports this limitation.
- `SYS-CTRL-1: Ingest verwerken aan/uit via dashboard/logboek` was merged via PR #71 and manually validated on the Raspberry Pi on 2026-05-29.
  - Dashboard shows ingest status/toggle and no longer exposes broad operational settings.
  - Toggle uses the reload flow so the running Ingest process applies `IngestProcessingEnabled` without restart.
  - Logbook shows a warning bar while ingest processing is disabled.
  - Creating a new trip while ingest processing is disabled shows a confirmation modal.
  - `BootManager.Tools.Ingest` still has no Infrastructure/database reference and only uses Web API/control-flow.
  - Disabled-mode skips before parsing/capture/sampling/API post and logs only throttled summaries.
  - Pi validation confirmed that after correcting `apiBaseUrl` to `http://bootmanager-web:5000`, `IngestProcessingEnabled=False` caused incoming UDP lines to be skipped without API posts.
- `SYS-CTRL-2: Ingest reload robuust maken tegen foutieve ApiBaseUrl` was merged via PR #72 and manually validated on the Raspberry Pi on 2026-05-29.
  - `POST /reload-settings` now tries configured/bootstrap `Ingest__ApiBaseUrl` first and uses mutable runtime `ApiBaseUrl` only as fallback.
  - `IngestCaptureLogger` now requires both appsettings capture logging and runtime/database `CaptureLoggingEnabled` to be true before creating/writing capture logs.
  - New unit tests cover capture logging enabled/disabled combinations and reload URL fallback behavior.
  - Architecture check: `BootManager.Tools.Ingest` still has no Infrastructure/database reference.
  - Verification: `dotnet build BootManager.sln` passed with 0 warnings/errors; targeted `IngestTools|OperationalSettings` tests passed 35/35.
  - Pi validation confirmed `apiBaseUrl=http://bootmanager-web:5000`, settings fetch via `http://bootmanager-web:5000`, effective capture logging disabled when database/runtime is false, and skipped-line summaries without `POST /api/networkmessages` while ingest processing is disabled.
- Broader system operations topics remain open for later:
  - full backup/restore UI;
  - web factory reset;
  - safe shutdown;
  - system action log.
- For Raspberry Pi/system operations, the next explicit choice is no longer whether the Pi receives real boat data, whether local technical analysis is feasible, whether ingest can be toggled, or whether reload/capture logging resilience works on the Pi; those are now confirmed. The next system-operations follow-up should be one of:
  - Pi diagnostics without manual `sqlite3`;
  - `SYS-ANALYSIS-2` date/time input fix for the analysis page;
  - logging profile cleanup for Pi/field-test use;
  - GPS fix diagnostics around `GGA`/`RMC`;
  - UI validation of live stored measurements;
  - longer-duration database/WAL/retention observation;
  - capture-log validation and replay readiness;
  - a reusable field-test checklist against Raymarine/Axiom/on-board instruments.
  - After those operator-oriented steps, the next user-facing UI track is a live dashboard showing current measurements.

Standing instruction from the user:

1. When the user proposes an idea, check the full legacy scope automatically before answering.
2. When Codex proposes a next story or continuation, check the full legacy scope automatically before proposing it.
3. Do not wait for the user to explicitly ask for this scope check.
4. Determine whether the idea is already defined in the legacy scope, already implemented, partially implemented, parked, dependent on other modules, or genuinely new.
5. Then map the answer to the current BootManagerV2 architecture, roadmap and implementation-agent workflow.
6. For implementation work, after creating/selecting a branch, formulate the user story first and ask the user for approval before generating an implementation packet.
7. The user story must include scope, out-of-scope, acceptance criteria, legacy US coverage impact, and manual test notes when relevant.
8. After the user approves a user story, save it automatically in the relevant `.docs/epics/*.md` file before generating an implementation packet. Do not wait for the user to explicitly ask for this.
9. During implementation/review/closure, keep that epic file updated with status, implementation details and verification.

Primary scope files:

- `.docs/legacy-analysis/scope-inventory.md`
- `.docs/legacy-analysis/mapped-epics.md`
- `.docs/legacy-analysis/legacy-coverage-register.md`
- `.docs/legacy-analysis/proposed-backlog.md`
- `.docs/legacy-analysis/implemented-or-obsolete.md`

Coverage rule:

- `legacy-coverage-register.md` is the primary story-level checklist.
- Whenever functionality is completed in BootManagerV2, update the relevant legacy US statuses before considering the story administratively complete.
- Use `Partial` when BootManagerV2 covers only part of a legacy story and record what remains open.

## Current Branch And PR Context

- Current branch is `master`.
- PR #82 (`codex/dashboard-fluid-levels`) was merged on 2026-06-01 with merge commit `06241a1`.
- Local `master` has been fast-forwarded from `origin/master` after PR #82 and the worktree is clean.
- PR #64 (`feature/register-owner-cleanup`) was merged on 2026-05-26 with merge commit `a441ca8`.
- PR #65 (`feature/pi-database-reset`) was merged on 2026-05-27 with merge commit `c0b7590`.
- PR #66 (`feature/logbook-trip-header-datetimes`) was merged on 2026-05-27 with merge commit `0a8c067`.
- PR #67 (`feature/pi-minimal-deployment-checkout-story`) was merged on 2026-05-27 with merge commit `367ef17`.
- PR #68 (`feature/logbook-arrival-closeout`) was merged on 2026-05-27 with merge commit `0a5ac57`.
- Local `master` had previously been fast-forwarded from `origin/master` after PR #68 and the worktree was clean.
- Follow-up master commit `1db5534 Document sudo requirement for Pi reset script` was pushed to `origin/master` after the Raspberry Pi test.
- The real-data Pi field test on 2026-05-29 was performed while the Pi was still on `master @ 1db5534`; later logbook stories merged after that commit were intentionally not relevant to the field test.
- The user tested the reset flow on the Raspberry Pi after PR #65 and confirmed it worked.
- The Raspberry Pi still only pulls `master`; never use it for feature-branch validation.
- The Raspberry Pi must remain on/pull only `master`; update it only when the user is explicitly told the exact commands.
- Recent relevant commits on `master`:
  - `06241a1 Merge pull request #82 from rrvanleeuwen/codex/dashboard-fluid-levels`
  - `146c063 Add configurable dashboard measurement tiles`
  - `0a5ac57 Merge pull request #68 from rrvanleeuwen/feature/logbook-arrival-closeout`
  - `5299b73 Add logbook trip closeout flow`
  - `367ef17 Merge pull request #67 from rrvanleeuwen/feature/pi-minimal-deployment-checkout-story`
  - `6f52c2a Add lean Pi deployment story`
  - `0a8c067 Merge pull request #66 from rrvanleeuwen/feature/logbook-trip-header-datetimes`
  - `1db5534 Document sudo requirement for Pi reset script`
  - `c0b7590 Merge pull request #65 from rrvanleeuwen/feature/pi-database-reset`
  - `2bc30ca Add Pi database reset runbook and logbook epic updates`
  - `f8e93c4 Update current handoff after Pi sync`
  - `ab38c00 Record Register Owner cleanup completion`
  - `a441ca8 Merge pull request #64 from rrvanleeuwen/feature/register-owner-cleanup`

## Processed Word Files

Processed:

- `BootManager_Softwarevisie_v0.7.docx`
- `BootManager_Epic0_Installatie_Authenticatie.docx`
- `BootManager_Epic1_Bootbeheer_en_Gebruikersbeheer.docx`
- `BootManager_Epic2_Inventarisbeheer.docx`
- `BootManager_Epic3_PassagePlanning.docx`
- `BootManager_Epic4_Documentbeheer.docx`
- `BootManager_Epic5_Logboek.docx`
- `BootManager_Epic6_OnderhoudsbeheerL.docx`
- `BootManager_Epic7_Dashboard.docx`
- `BootManager_Epic8_Systeembeheer.docx`
- `BootManager_Epic9_Integratie.docx`
- `BootManager_Epic10_Rapportage.docx`
- `BootManager_Epic11_Notificaties.docx`
- `BootManager_Epic12_AI.docx`

Next file to process after user approval:

- None. All available Word exports have been processed.

## Important Findings So Far

- Software vision confirms the broad functional scope, but old technical architecture is ignored.
- Epic 0 confirms US0.1 through US0.6; no extra stories beyond OCR.
- Epic 1 resolves the OCR gap for US1.9 through US1.13:
  - boot areas;
  - storage locations;
  - QR/tag generation;
  - QR scan opening storage location detail.
- Epic 2 resolves the OCR gap for US2.1 and US2.2:
  - product categories;
  - category icons via library or PNG/SVG upload.
- Epic 3 confirms US3.1 through US3.14:
  - passage plans, crew list, consumption profiles;
  - inventory comparison and shopping list;
  - menu planning, document links, export, dashboard and logbook sync.
- Epic 4 resolves the OCR gap for US4.1 and adds US4.13:
  - document upload/categorization;
  - document open, print or share.
- Epic 5 resolves the OCR gap for US5.1:
  - manual logbook entry with weather information.
  - Existing BootManagerV2 logbook covers a substantial part already, but route map, passage link, arrival close-out, richer stats and PDF/CSV remain open.
- Epic 6 confirms US6.1 through US6.14:
  - maintenance tasks, parts/components, intervals, execution records, costs, mechanic, attachments and history.
  - First maintenance slices can start with simple time-based tasks before usage-based motor-hour intervals.
- Epic 7 resolves the OCR gap for US7.1 through US7.8:
  - dashboard open, active boat info, alerts, weather/tides;
  - widgets for inventory, maintenance, documents and passage planning.
  - Many widgets depend on modules not yet implemented.
- Epic 8 confirms US8.1 through US8.14:
  - settings, units, language/region, sensors, users, backup/restore, Pi system info, action log, export/import/defaults.
  - BootManagerV2 already has settings and ingest/sampling config; backup/restore UI and Pi status remain open.
- Epic 9 resolves the OCR gap for US9.1 through US9.5:
  - weather, AIS, Navionics/GPX, harbor information, Bluetooth/Wi-Fi sensors.
  - BootManagerV2 has strong NMEA/YDEN ingest and raw AIS sentence recognition, but no AIS semantic ship overview yet.
- Epic 10 confirms US10.1 through US10.6:
  - fuel, inventory, maintenance and trip cost analyses plus PDF/CSV and charts.
  - Broad reporting depends on future inventory/maintenance/passage data; logbook export/statistics are the closest near-term fit.
- Epic 11 confirms US11.1 through US11.6:
  - low inventory, document expiry, maintenance reminders, passage departure warnings, preferences and notification history.
  - BootManagerV2 only has partial in-app logbook warnings; push/email should wait.
- Epic 12 confirms US12.1 through US12.6:
  - barcode/QR, photo recognition, categorization, restock suggestions, predictive maintenance and speech input.
  - AI remains low priority; non-AI barcode/QR can be inventory scope earlier.
- Multi-user and multi-boat remain parked for now.
- Storage locations, product categories, inventory quantities and QR/barcode flows belong under the future inventory/storage-location epic.
- Passage planning depends on inventory/document basics; crew data there is trip data and does not require multi-user accounts first.
- General document management remains separate from existing logbook attachments.

## Current Worktree Expectation

At session end, the worktree should be clean on the active documentation branch except for intentional documentation updates and the untracked `veldtests/` source material if it has not been committed yet.
