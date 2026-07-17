# PILOT-LOG-02 — ClaudeStatus

Story: Gebeurteniskeuze, weericonen en notitie. Branch: `codex/pilot-log-02`.

## 1. Gewijzigde bestanden en geïmplementeerd gedrag

Domein / enums (nieuw):
- `BootManager.Core/Enums/LogbookEventType.cs` — stabiele gebeurtenis-domeinwaarde met 10 pilotwaarden
  (Overstag, Gijp, ZeilGewijzigd, MotorGestart, MotorGestopt, Vertrek, Aankomst, VoorAnker,
  BijzonderMoment, Momentopname), expliciete int-waarden.
- `BootManager.Core/Enums/LogbookWeatherCondition.cs` — stabiele weer-domeinwaarde met 9 pilotwaarden
  (Zonnig, LichtBewolkt, HalfBewolkt, Bewolkt, Buien, Regen, Onweer, Mist, VeelWind), expliciete int-waarden.

Domein:
- `BootManager.Core/Entities/LogbookEntry.cs` — nieuwe optionele properties `EventType` en
  `WeatherCondition`; toegevoegd als optionele parameters aan constructor en `Update(...)` met behoud
  van bestaande snapshot-/statuslogica. `Remarks` blijft de korte notitie (geen tweede notitieveld).

Application:
- `LogbookEntryDto.cs`, `SaveLogbookEntryDto.cs`, `LogbookEntryDetailDto.cs`
  (`LogbookSavedEntryValuesDto`) — nieuwe `EventType`/`WeatherCondition` velden.
- `LogbookService.cs` — `CreateEntryAsync` en `UpdateEntryAsync` sturen gebeurtenis/weer door naar de
  entiteit; `MapEntryAsync` mapt ze terug. `CreateManualDraftEntryAsync` blijft ongewijzigd (event/weer
  blijven null bij de eerste vastlegging).
- `LogbookEntryDetailService.cs` — `MapToSavedEntryValuesDto` mapt gebeurtenis/weer naar de detail-DTO.
- `ILogbookService.cs` — NIET gewijzigd (geen publieke API-wijziging nodig; nieuwe waarden lopen via
  bestaande `SaveLogbookEntryDto`).

Infrastructure:
- `LogbookEntryConfiguration.cs` — `EventType`/`WeatherCondition` als stabiele integer opgeslagen
  (`HasConversion<int?>()`), optioneel (nullable), backward compatible.
- Migratie `20260717092947_AddLogbookEntryEventAndWeather` (+ Designer) en
  `BootManagerDbContextModelSnapshot.cs` — twee nullable INTEGER-kolommen op `LogbookEntries`.

Web (presentatie):
- `Logbook.razor` — na `Moment vastleggen` opent direct een taakgericht context-formulier voor het
  nieuwe concept: gebeurteniskeuze (pilotlijst), weerkeuze via grote pictogramknoppen (stabiele waarde,
  icoon/label zijn presentatie), en korte notitie via bestaande `Remarks`. Opslaan verrijkt het concept
  via `UpdateEntryAsync` met behoud van snapshot en Draft-status. Overslaan behoudt het concept
  ongewijzigd. Opgeslagen gebeurtenis/weer worden getoond in tabel- en cardweergave; de bestaande
  generieke bewerk-/nieuwe-regelformulieren kunnen de waarden lezen, kiezen en opslaan zonder verlies.
  `Owner,Crew`-autorisatie en `_foutmelding`-stijl ongewijzigd.
- `LogbookEntryDetails.razor` — toont opgeslagen gebeurtenis (badge), weer (pictogram + label) en notitie.

## 2. Uitgevoerde tests/checks en resultaten

- `dotnet test BootManager.UnitTests ... --filter LogbookServiceTests|LogbookComponentTests` → Passed 10/10.
- `dotnet test BootManager.IntegrationTests ... --filter Logbook` → Passed 3/3.
- Volledige set incl. `LogbookEntryDetailServiceTests` → Passed 12/12 (unit) + 3/3 (integratie).
- `dotnet build BootManager.sln --no-restore` → Build succeeded, 0 Error(s).
- `git diff --check` → geen witruimtefouten (alleen LF/CRLF-waarschuwingen op vooraf al gewijzigde docs
  die niet door deze story zijn aangepast).

## 3. Nieuwe/gewijzigde testnamen, uitgevoerd productiegedrag en migratiebewijs

Unit — `BootManager.UnitTests/Logbook/LogbookServiceTests.cs`:
- `CreateEntryAsync_PersistsSelectedEventWeatherAndNote_ThroughRemarks` — bewijst dat `CreateEntryAsync`
  gebeurtenis/weer als stabiele domeinwaarde en de notitie via `Remarks` persistent maakt en terugmapt.
- `UpdateEntryAsync_EnrichesDraftWithEventWeatherNote_WhilePreservingSnapshotAndDraftStatus` — bewijst
  dat de taakgerichte opslag de Draft verrijkt met event/weer/notitie terwijl snapshot (Course, Wind,
  GPS, Lat/Long, SOG) en Draft-status behouden blijven.

Unit — `BootManager.UnitTests/Logbook/LogbookComponentTests.cs` (bUnit, echt component + echte klikken):
- `CaptureMoment_ThenChooseEventWeatherNote_SavesStableValues_AndRendersContext` — rendert `Logbook`,
  klikt `Moment vastleggen`, kiest gebeurtenis + weerpictogram, typt notitie, klikt `Moment opslaan`;
  verifieert dat `UpdateEntryAsync` één keer met de juiste stabiele waarden + notitie + behouden snapshot
  wordt aangeroepen en dat de context daarna in het overzicht verschijnt.
- `EntriesWithoutEventOrWeather_RenderSafely_InList` — bewijst dat bestaande regels met null event/weer
  veilig renderen (geen gebeurtenis-badge).

Unit — `BootManager.UnitTests/Logbook/LogbookEntryDetailServiceTests.cs` (nieuw, gerechtvaardigd door de
Test Evidence Requirement "LogbookEntryDetailService maps the saved event/weather/note back to the detail DTO"):
- `GetEntryDetailAsync_MapsSavedEventWeatherAndNote_BackToDetailDto`.
- `GetEntryDetailAsync_LeavesEventAndWeatherNull_ForLegacyEntry`.

Integratie — `BootManager.IntegrationTests/Logbook/LogbookEntryEventWeatherMigrationTests.cs` (nieuw):
- `Upgrade_PreservesExistingEntry_AddsColumns_AndKeepsEventWeatherNullForOldRows` — migreert expliciet
  naar `20260621074251_AddStockExpectedLocations`, bewijst de migratielijst vóór (bevat vorige, niet de
  nieuwe) en dat de nieuwe kolommen nog niet bestaan, voegt een bestaande reis + logboekregel toe (regel
  via raw SQL met alleen de oude kolommen), migreert naar latest, bewijst de migratielijst ná (bevat
  beide), dat `EventType`/`WeatherCondition` nu bestaan, dat de regel behouden is en dat beide nieuwe
  waarden null zijn voor de oude regel.
- `Upgrade_AllowsPersistingStableEventAndWeatherValues_AfterMigration` — bewijst dat na migratie een
  regel met event/weer round-trip persistent is en dat de weerwaarde als stabiele integer (niet als
  icoon/label) is opgeslagen (directe SQL-kolomcontrole).

Red-green: dit is een nieuwe slice, geen pre-existing defect; formeel red-green bugfixbewijs is per packet
niet vereist. Alle nieuwe tests asserten op vastgelegde DTO-/kolomwaarden, servicecalls en gerenderde
markup die uitsluitend door de nieuwe productiecode ontstaan, en kunnen daardoor falen bij ontbrekende of
foutieve wiring.

## 4. Migratie-/configuratie-impact

- Nieuwe migratie `20260717092947_AddLogbookEntryEventAndWeather` voegt twee nullable INTEGER-kolommen
  (`EventType`, `WeatherCondition`) toe aan `LogbookEntries`. Geen datamigratie; bestaande rijen krijgen
  null. Down-migratie verwijdert beide kolommen. Snapshot bijgewerkt met exact deze twee properties.
- Geen nieuwe dependencies, DI-wijzigingen of configuratiewijzigingen buiten de EF-kolomconfiguratie.

## 5. Resterende risico's en noodzakelijke handmatige test

- UI-pictogrammen zijn emoji; weergave kan per platform/lettertype licht verschillen (presentatie, geen
  invloed op opslag).
- Handmatige acceptatietest (uit release): maak een handmatig moment vanuit een actieve reis, kies
  gebeurtenis + weer + notitie, sla op, controleer dat gebeurtenis/weer/notitie later zichtbaar blijven
  in lijst en detail, en dat de opgeslagen weerwaarde niet enkel van het icoonlabel afhangt. Kon niet in
  een echte actieve-reisdatabase worden uitgevoerd binnen deze sessie.

## 6. Eindstatus

ready for Codex review — alle scope-items en acceptatiecriteria zijn technisch geïmplementeerd,
gebeurtenis/weer worden als stabiele domeinwaarden opgeslagen (niet als label/icoon), de directe
post-capture-bewerkflow werkt, de korte notitie loopt via bestaand `Remarks`, het upgradepad vanaf
`20260621074251_AddStockExpectedLocations` is bewezen, alle gerichte tests slagen, en build en
`git diff --check` zijn schoon. Dit is geen acceptatie of productieverklaring; het is een handoffsignaal
voor Codex-review.

Done: 2026-07-17 11:39
