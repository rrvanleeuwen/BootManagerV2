# Current Codex Handoff

Updated: 2026-05-25.

## Current Task

Legacy BootManager Word exports from `.docs/legacy-input/` have now all been processed into the current BootManagerV2 documentation.

Standing instruction from the user:

1. When the user proposes an idea, check the full legacy scope automatically before answering.
2. When Codex proposes a next story or continuation, check the full legacy scope automatically before proposing it.
3. Do not wait for the user to explicitly ask for this scope check.
4. Determine whether the idea is already defined in the legacy scope, already implemented, partially implemented, parked, dependent on other modules, or genuinely new.
5. Then map the answer to the current BootManagerV2 architecture, roadmap and Copilot workflow.
6. For implementation work, after creating/selecting a branch, formulate the user story first and ask the user for approval before generating a Copilot prompt.
7. The user story must include scope, out-of-scope, acceptance criteria, legacy US coverage impact, and manual test notes when relevant.

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

- PR `#60` was merged and local `master` was fast-forwarded to merge commit `152a0443061f41783f54015ecffbb7700067a750`.
- Current branch: `feature/owner-vessel-settings`
- This branch was created from current `master`.
- Copilot has already been given a prompt for bootgegevens wijzigen in Settings, before the new "user story before prompt" rule was recorded. Apply the new rule from now on.

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

There may be uncommitted documentation changes recording the new "user story before Copilot prompt" workflow. These should be committed with the owner/vessel settings branch if still present.
