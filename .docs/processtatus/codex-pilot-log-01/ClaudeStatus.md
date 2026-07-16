# PILOT-LOG-01 — ClaudeStatus

## 1. Gewijzigde bestanden en geïmplementeerd gedrag

- `BootManager.Application/Logbook/Services/ILogbookService.cs`
  - Nieuwe publieke API `Task<LogbookEntryDto> CreateManualDraftEntryAsync(int tripId, CancellationToken)` met Nederlandse XML-documentatie. Legt handmatig een logboekmoment vast voor een lopende reis.
- `BootManager.Application/Logbook/Services/LogbookService.cs`
  - Implementatie van `CreateManualDraftEntryAsync`:
    - Laadt de reis; gooit `InvalidOperationException` als de reis niet bestaat.
    - Weigert een afgesloten reis (`LogbookTripStatus.Completed`) **vóór** het opvragen van suggesties en vóór het toevoegen van een regel.
    - Legt het moment vast op `DateTime.UtcNow`.
    - Vraagt suggesties op met `onlyPeriodData: false` (laatst bekende boordwaarden).
    - Persisteert een `Draft`-regel met de beschikbare snapshotvelden (course, wind, GPS-status, latitude, longitude, gemiddelde SOG). Ontbrekende waarden blijven null; `BaroPressure`, `LogValue` en `Remarks` blijven leeg.
    - Retourneert de aangemaakte Draft als DTO.
  - De bestaande `CreateDraftEntryAsync` (automatische gemiste-momentflow met `onlyPeriodData: true`) is ongewijzigd.
- `BootManager.Web/Components/Pages/Logbook.razor`
  - Nieuwe actie `📍 Moment vastleggen` in de header van het logboekoverzicht, uitsluitend zichtbaar bij `LogbookTripStatus.Open`.
  - Nieuw veld `_capturingMoment` (knop-disabled + spinner tijdens vastleggen).
  - Nieuwe handler `MomentVastleggen()`: roept `CreateManualDraftEntryAsync` aan voor de geselecteerde reis, voegt het geretourneerde concept toe aan het overzicht (chronologisch), reset card-paging, herberekent gemiste logmomenten, en houdt `_selectedTrip` open. Fouten via het bestaande `_foutmelding`-patroon.
- `BootManager.UnitTests/Logbook/LogbookServiceTests.cs` (nieuw)
- `BootManager.UnitTests/Logbook/LogbookComponentTests.cs` (nieuw)

## 2. Tests/checks en resultaten

- Gerichte tests:
  `dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --filter "FullyQualifiedName~LogbookServiceTests|FullyQualifiedName~LogbookComponentTests|FullyQualifiedName~LogbookTripTests"`
  → **Passed! Failed: 0, Passed: 9, Skipped: 0, Total: 9.**
- Build: `dotnet build BootManager.sln --no-restore` → **Build succeeded, 0 Error(s)** (5 bestaande, niet-gerelateerde warnings).
- `git diff --check` → schoon (geen witruimte-/conflictproblemen).

## 3. Nieuwe/gewijzigde testnamen en het uitgevoerde productiegedrag

- `LogbookServiceTests.CreateManualDraftEntryAsync_RequestsLatestKnownSuggestions_AndPersistsDraftSnapshot`
  - Voert `LogbookService.CreateManualDraftEntryAsync` uit tegen een open reis. Verifieert dat suggesties worden opgevraagd met `onlyPeriodData: false` (en nooit `true`), dat de regel precies één keer wordt gepersisteerd als `Draft`, met correcte tripId, tijdstempel binnen [before, after], en alle snapshotvelden; `BaroPressure`/`LogValue`/`Remarks` blijven null. Ook de geretourneerde DTO wordt geasserteerd.
- `LogbookServiceTests.CreateManualDraftEntryAsync_LeavesUnavailableSnapshotValuesNull`
  - Suggesties zonder waarden → de gepersisteerde Draft heeft alle meetvelden null, maar is nog steeds een Draft voor de juiste reis (geen verzonnen data).
- `LogbookServiceTests.CreateManualDraftEntryAsync_RejectsCompletedTrip_AndDoesNotRequestSuggestionsOrAddEntry`
  - Afgesloten reis → `InvalidOperationException`; `GetSuggestionsAsync` en `AddAsync` worden nooit aangeroepen.
- `LogbookServiceTests.CreateDraftEntryAsync_StillUsesPeriodOnlyFlow_ForAutomaticMissedMoments`
  - Regressiebewaking: de automatische flow blijft `onlyPeriodData: true` gebruiken (en nooit `false`).
- `LogbookComponentTests.OpenTrip_CaptureMoment_CallsManualApiOnce_RendersDraft_AndKeepsControls`
  - Rendert `Logbook.razor` via bUnit met een open reis, klikt de echte knop `Moment vastleggen`. Verifieert dat `CreateManualDraftEntryAsync` één keer wordt aangeroepen voor de reis, `CompleteTripAsync` nooit, dat het geretourneerde concept zichtbaar wordt (`NW 4`, badge `Te accorderen`) en dat de actieve-reisbediening (`Moment vastleggen`, `Beëindig reis`) zichtbaar blijft.
- `LogbookComponentTests.CompletedTrip_DoesNotRenderMomentVastleggenAction`
  - Rendert `Logbook.razor` met een afgesloten reis; de knop `Moment vastleggen` ontbreekt en de status `Afgesloten` wordt getoond.

## 4. Migratie-/configuratie-impact

- Geen. Geen EF-migratie, geen nieuwe persisted velden, geen DI-/configuratiewijziging. De bestaande `LogbookEntry`-velden dragen de snapshot.

## 5. Resterende risico's en handmatige test

- Het capture-moment gebruikt `DateTime.UtcNow` in de service; er is geen actor-/gebruikersveld in het huidige registratiemodel, dus de uitvoerende gebruiker wordt (conform packet) niet vastgelegd.
- Handmatige acceptatietest:
  1. Open een actieve (open) reis in het scheepsdagboek.
  2. Klik `Moment vastleggen`.
  3. Controleer dat één nieuwe conceptregel (`Te accorderen`) verschijnt met de actuele beschikbare boordwaarden en lege velden waar geen data beschikbaar is.
  4. Controleer dat de reis open blijft en de conceptregel via het bestaande draftfilter terugvindbaar/bewerkbaar is.

## 6. Eindstatus

`ready for Codex review` — alle scope-items en acceptatiecriteria zijn technisch geïmplementeerd; handmatige capture (`onlyPeriodData: false`) en de automatische gemiste-momentflow (`onlyPeriodData: true`) hebben bewezen gescheiden semantiek; alle gerichte tests slagen (9/9), de build slaagt zonder errors en `git diff --check` is schoon; er is geen wijziging buiten de verwachte write-set.

Done: 2026-07-16 15:59
