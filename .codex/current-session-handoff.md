# Current Codex Handoff

Updated: 2026-05-27.

## Current Task

Legacy BootManager Word exports from `.docs/legacy-input/` have all been processed into the current BootManagerV2 documentation.

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

Next near-term story decision:

- The next logical story is now the digital logbook trip-header improvement already captured in `.docs/epics/digital-logbook.md`:
  `vertrek- en aankomstmoment met datum en tijd`.
- This fits legacy `US5.6 Logboekheader invullen` and supports later `US5.14 Logboek afronden bij aankomst`.
- The digital logbook epic itself was refreshed during this session so it better reflects already implemented slices versus still-open follow-up work.
- Broader system operations topics remain open for later:
  - full backup/restore UI;
  - web factory reset;
  - safe shutdown;
  - system action log.

Standing instruction from the user:

1. When the user proposes an idea, check the full legacy scope automatically before answering.
2. When Codex proposes a next story or continuation, check the full legacy scope automatically before proposing it.
3. Do not wait for the user to explicitly ask for this scope check.
4. Determine whether the idea is already defined in the legacy scope, already implemented, partially implemented, parked, dependent on other modules, or genuinely new.
5. Then map the answer to the current BootManagerV2 architecture, roadmap and Copilot workflow.
6. For implementation work, after creating/selecting a branch, formulate the user story first and ask the user for approval before generating a Copilot prompt.
7. The user story must include scope, out-of-scope, acceptance criteria, legacy US coverage impact, and manual test notes when relevant.
8. After the user approves a user story, save it automatically in the relevant `.docs/epics/*.md` file before generating a Copilot prompt. Do not wait for the user to explicitly ask for this.
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
- PR #64 (`feature/register-owner-cleanup`) was merged on 2026-05-26 with merge commit `a441ca8`.
- PR #65 (`feature/pi-database-reset`) was merged on 2026-05-27 with merge commit `c0b7590`.
- Local `master` has been fast-forwarded from `origin/master` after PR #65.
- Follow-up master commit `1db5534 Document sudo requirement for Pi reset script` was pushed to `origin/master` after the Raspberry Pi test.
- The user tested the reset flow on the Raspberry Pi after PR #65 and confirmed it worked.
- The Raspberry Pi still only pulls `master`; never use it for feature-branch validation.
- The Raspberry Pi must remain on/pull only `master`; update it only when the user is explicitly told the exact commands.
- Recent relevant commits on `master`:
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

At session end, the worktree should be clean on `master` except for intentional documentation updates if the handoff file has just been edited and not yet committed.
