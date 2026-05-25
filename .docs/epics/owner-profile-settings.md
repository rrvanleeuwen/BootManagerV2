# Epic: Owner Profile & Vessel Settings

Status: US2 (Bootgegevens wijzigen) geïmplementeerd op 2026-05-25; US1/US3/US4 in backlog.

## Aanleiding

Na afronding van de first-run onboarding kan de eigenaar BootManager gebruiken, maar de gegevens die tijdens onboarding zijn ingevoerd moeten later ook beheerd kunnen worden.

Tijdens handmatige test op 2026-05-25 viel op:

- eigenaargegevens uit onboarding zijn achteraf niet duidelijk wijzigbaar;
- bootgegevens uit onboarding zijn achteraf niet duidelijk wijzigbaar;
- wachtwoord wijzigen moet expliciet en betrouwbaar vindbaar blijven in de normale instellingenflow.

De eerste-start onboarding blijft bedoeld voor initiële setup. Structureel beheer hoort daarna thuis in `/settings` of een duidelijke subpagina onder instellingen.

## Doel

De eigenaar kan na onboarding zelf de belangrijkste single-owner installatiegegevens beheren:

- eigenaarprofiel;
- bootprofiel;
- wachtwoord;
- duidelijke feedback en validatie bij opslaan.

## Uitgangspunten

- BootManager blijft voorlopig single-owner.
- Geen rollenbeheer of meerdere gebruikers.
- Geen e-mail recovery of master-key flow.
- Geen wijziging aan bootstrap/onboarding gate.
- Bestaande services hergebruiken waar mogelijk.
- Settings blijft owner-only.
- Gevoelige wijzigingen, vooral wachtwoord, moeten expliciet bevestigd worden met het huidige wachtwoord.

## Niet-doelen

- Meerdere owners of crew accounts.
- Wachtwoordreset via e-mail.
- Recovery-code UI.
- Publieke accountregistratie.
- Bootgegevenshistorie of audittrail.
- Meerdere vessels per installatie.

## Huidige Stand

Relevant bestaand:

- `OwnerProfile` bevat encrypted payload met naam/e-mail en wachtwoordhash.
- `VesselProfile` is een singleton voor bootgegevens.
- `IVesselProfileService` ondersteunt get-or-create en update.
- `IOwnerSettingsService` ondersteunt wachtwoord wijzigen.
- `/settings` bevat operationele instellingen en een wachtwoordwijzigingsblok.

Open probleem:

- Er is geen duidelijke beheerflow voor owner naam/e-mail.
- Er is geen UI-flow om `VesselProfile` na onboarding te wijzigen.
- Wachtwoord wijzigen moet functioneel en UX-matig opnieuw worden gevalideerd, omdat het voor de gebruiker niet duidelijk vindbaar/werkend overkwam.

## User Stories

### US1: Eigenaargegevens Wijzigen In Instellingen

**Doel:** De eigenaar kan naam en e-mail uit het onboardingprofiel later aanpassen.

Velden:

- Naam: verplicht.
- E-mail: optioneel.

Gedrag:

- Settings toont huidige eigenaargegevens.
- Opslaan werkt de encrypted owner payload bij.
- Wachtwoordhash en setup flags blijven ongewijzigd.
- Validatiefouten worden duidelijk getoond.

Acceptatiecriteria:

- `/settings` toont eigenaarnaam en e-mail.
- Naam leeg opslaan faalt.
- E-mail mag leeg zijn.
- Succesvolle opslag blijft zichtbaar na refresh.
- `dotnet build` slaagt.
- Gerichte service/unit tests waar businesslogica wordt toegevoegd.

### US2: Bootgegevens Wijzigen In Instellingen

**Doel:** De eigenaar kan het singleton bootprofiel na onboarding aanpassen.

**Status:** ✅ Afgerond op 2026-05-25.

Velden:

- Bootnaam: verplicht.
- Thuishaven: optioneel.
- Roepnaam: optioneel.
- MMSI: optioneel.

Gedrag:

- Settings toont huidige `VesselProfile`.
- Opslaan gebruikt `IVesselProfileService.UpdateVesselProfileAsync`.
- Bestaande validatie uit `VesselProfileService` blijft leidend.
- Geen nieuwe migration nodig, tenzij de huidige velden onvoldoende blijken.

Acceptatiecriteria:

- ✅ `/settings` toont bootnaam, thuishaven, roepnaam en MMSI.
- ✅ Bootnaam leeg opslaan faalt met duidelijke foutmelding.
- ✅ Optionele velden mogen leeg zijn.
- ✅ Succesvolle opslag blijft zichtbaar na refresh.
- ✅ Onboardinggegevens kunnen achteraf worden aangepast.
- ✅ `dotnet build` slaagt.
- ✅ Bestaande `VesselProfileServiceTests` blijven slagen.

**Implementatiedetails:**

- **Component:** `BootManager.Web/Components/Pages/Settings.razor`
  - Nieuwe sectie "Bootgegevens" tussen wachtwoord en operationele instellingen.
  - `EditForm` gebonden aan `VesselEditModel` (lokaal model met settable properties).
  - `OnInitializedAsync()` laadt bootgegevens via `IVesselProfileService.GetOrCreateVesselProfileAsync()`.
  - `HandleVesselSubmit()` verwerkt opslaan, trim whitespace, handelt null/lege optionele velden af.
  - Validatiefouten uit service (`ArgumentException`) tonen in alert met rode kleur.
  - Succesmeldingen tonen in groene alert.

- **Service:** `VesselProfileService` gebruikt voor opslaan; user-facing validatie- en fallbackteksten voor bootgegevens zijn Nederlandstalig gemaakt.

- **DTOs:** `VesselProfileDto` en `UpdateVesselProfileRequestDto` ongewijzigd gebruikt.

- **Tests:** `VesselProfileServiceTests` aangepast voor Nederlandse fallbacktekst; bestaande VesselProfile-tests blijven slagen.

### US3: Wachtwoord Wijzigen Verifiëren En UX Verbeteren

**Doel:** Wachtwoord wijzigen blijft betrouwbaar beschikbaar na onboarding en is duidelijk herkenbaar voor de gebruiker.

Gedrag:

- Settings bevat een duidelijke wachtwoordsectie.
- Huidig wachtwoord is verplicht.
- Nieuw wachtwoord is verplicht.
- Bevestiging moet overeenkomen.
- Nieuw wachtwoord mag niet gelijk zijn aan huidig wachtwoord.
- Na succes kan de gebruiker opnieuw inloggen met het nieuwe wachtwoord.
- Fouten worden helder getoond.

Acceptatiecriteria:

- Wachtwoord wijzigen via `/settings` werkt na onboarding.
- Onjuist huidig wachtwoord faalt.
- Mismatch bevestiging faalt.
- Te kort wachtwoord faalt volgens bestaande regels.
- Na wijziging werkt oud wachtwoord niet meer en nieuw wachtwoord wel.
- Handmatige runtime-test wordt vóór PR expliciet uitgevoerd.
- `dotnet build` slaagt.

### US4: Settings Pagina Ordenen Voor Beheer

**Doel:** `/settings` wordt logisch gegroepeerd, zodat gebruiker onderscheid ziet tussen account/bootbeheer en operationele instellingen.

Mogelijke indeling:

- Account
  - eigenaargegevens;
  - wachtwoord wijzigen.
- Boot
  - bootgegevens.
- Operationeel
  - ingest/netwerk;
  - sampling/raw opslag;
  - bijlagenpad.

Acceptatiecriteria:

- Settings is scanbaar en niet verwarrend.
- Bestaande operationele instellingen blijven werken.
- Geen nested cards of brede visuele refactor zonder noodzaak.
- UI is bruikbaar op desktop en mobiel.
- Handmatige UI-test vóór PR.

## Aanbevolen Volgorde

1. US2: Bootgegevens wijzigen in instellingen.
2. US1: Eigenaargegevens wijzigen in instellingen.
3. US3: Wachtwoord wijzigen verifiëren en UX verbeteren.
4. US4: Settings pagina ordenen voor beheer.

US2 is waarschijnlijk de kleinste eerste stap, omdat `VesselProfileService` al bestaat en de onboardinggegevens daar al worden opgeslagen.

## Documentatie- en Testafspraken

- Werk `.docs/TODO.md` bij per afgeronde user story.
- Bij UI-wijzigingen altijd een handmatige teststap aan de gebruiker geven vóór commit/PR.
- Bij wachtwoordwijzigingen expliciet testen met oud en nieuw wachtwoord.
- Bij boot/eigenaargegevens expliciet refresh/heropen testen.
