# Epic: Owner Profile & Vessel Settings

Status: US1 (Eigenaargegevens wijzigen), US2 (Bootgegevens wijzigen), US3 (Wachtwoord wijzigen verifiëren) en US4 (Settings pagina ordenen met accordion) afgerond op 2026-05-25. US5 (actuele tellerstanden beheren) geïmplementeerd en handmatig geaccepteerd op 2026-06-07 als onderdeel van `LOG-TRIP-AUTO-1A`.

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

**Status:** ✅ Geïmplementeerd op 2026-05-25.

**User story:** Als eigenaar wil ik mijn naam en e-mailadres na onboarding kunnen wijzigen in Instellingen, zodat mijn eigenaarprofiel actueel blijft zonder de eerste-start onboarding opnieuw te hoeven doorlopen.

**Doel:** De eigenaar kan naam en e-mail uit het onboardingprofiel later aanpassen.

Velden:

- Naam: verplicht.
- E-mail: optioneel.

Gedrag:

- Settings toont huidige eigenaargegevens.
- Opslaan werkt de encrypted owner payload bij.
- Wachtwoordhash, setup/onboarding-status en bootgegevens blijven ongewijzigd.
- Validatie-, succes- en foutmeldingen zijn Nederlandstalig.

**Implementatie (2026-05-25):**

- DTOs toegevoegd: `GetOwnerProfileResponseDto`, `UpdateOwnerProfileRequestDto` in `BootManager.Application/Authentication/DTOs/`
- `IOwnerSettingsService` uitgebreid met `GetOwnerProfileAsync()` en `UpdateOwnerProfileAsync()`
- `OwnerSettingsService` geïmplementeerd met decryptie/encryptie via `IEncryptionService`
  - `GetOwnerProfileAsync()`: decrypteert payload, retourneert naam/email
  - `UpdateOwnerProfileAsync()`: valideert input (naam verplicht, email optioneel), decrypteert huidige payload, update naam/email, re-encrypteert, slaat op
  - Wachtwoord-hash en setup-flags blijven ongewijzigd
- Settings.razor aangepast:
  - Nieuwe "Eigenaarprofiel"-sectie boven "Wachtwoord wijzigen"
  - Form voor naam (verplicht) en e-mail (optioneel)
  - Laden op OnInitializedAsync, validatie, Nederlands fout-/succesmeldingen
- Unit tests: 8 nieuwe eigenaarprofieltests, alle slagen
  - GetOwnerProfile succesvol + edge cases
  - UpdateOwnerProfile succesvol, validatie (lege naam, ongeldige email), payload-integriteit
- Build succesvol, 13/13 OwnerSettings-tests slagen
- Acceptatiecriteria afgedekt

**Test coverage:**
- `OwnerSettingsServiceTests.GetOwnerProfile_Succeeds_ReturnsNameAndEmail` ✅
- `OwnerSettingsServiceTests.GetOwnerProfile_Fails_WhenNoOwnerExists` ✅
- `OwnerSettingsServiceTests.UpdateOwnerProfile_Succeeds_UpdatesNameAndEmail` ✅
- `OwnerSettingsServiceTests.UpdateOwnerProfile_Succeeds_AllowsEmptyEmail` ✅
- `OwnerSettingsServiceTests.UpdateOwnerProfile_Fails_WhenNameEmpty` ✅
- `OwnerSettingsServiceTests.UpdateOwnerProfile_Fails_WhenEmailInvalid` ✅
- `OwnerSettingsServiceTests.UpdateOwnerProfile_Fails_WhenNoOwnerExists` ✅
- `OwnerSettingsServiceTests.UpdateOwnerProfile_Succeeds_PreservesPasswordHash` ✅

**Acceptatiecriteria:**

- ✅ `/settings` toont eigenaarnaam en e-mail.
- ✅ Naam leeg opslaan faalt met Nederlandse melding.
- ✅ E-mail mag leeg zijn.
- ✅ Geldige wijziging opslaan toont Nederlandse succesmelding.
- ✅ Succesvolle opslag blijft zichtbaar na refresh.
- ✅ Bestaande wachtwoord- en bootgegevensflows blijven werken.
- ✅ `dotnet build` slaagt.
- ✅ Gerichte service/unit tests toegevoegd.


- Geen nieuwe onboarding-flow.

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

**Status:** ✅ Afgerond op 2026-05-25 op basis van handmatige runtime-test door gebruiker.

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

- ✅ Wachtwoord wijzigen via `/settings` werkt na onboarding.
- ✅ Na wijziging werkt het nieuwe wachtwoord.
- ✅ Handmatige runtime-test uitgevoerd door gebruiker op 2026-05-25.

Open voor latere hardening indien nodig:

- Onjuist huidig wachtwoord expliciet hertesten.
- Mismatch bevestiging expliciet hertesten.
- Te kort wachtwoord expliciet hertesten volgens bestaande regels.

### US4: Settings Pagina Ordenen Voor Beheer

**Status:** ✅ Geïmplementeerd op 2026-05-25.

**User story:** Als eigenaar wil ik dat de Settings-pagina is gegroepeerd in een uitklapbare accordion met Account, Boot en Operationeel, zodat ik instellingen snel kan vinden zonder door een lange pagina met losse formulieren te moeten scrollen.

**Doel:** `/settings` wordt logisch gegroepeerd, zodat gebruiker onderscheid ziet tussen account/bootbeheer en operationele instellingen.

UX-richting:

- Gebruik een accordion-weergave.
- Account is standaard open.
- Boot en Operationeel zijn standaard ingeklapt.
- De gebruiker kan groepen open- en dichtklappen.
- De accordion is alleen presentatie/structuur; bestaande formulieren blijven functioneel hetzelfde.

Indeling:

- Account
  - eigenaargegevens;
  - wachtwoord wijzigen.
- Boot
  - bootgegevens.
- Operationeel
  - ingest/netwerk;
  - sampling/raw opslag;
  - bijlagenpad.

Scope:

- `/settings` herstructureren naar een accordion-indeling.
- Bestaande secties verplaatsen naar de juiste groep.
- Bestaande save-flows behouden:
  - eigenaarprofiel opslaan;
  - wachtwoord wijzigen;
  - bootgegevens opslaan;
  - operationele instellingen opslaan.
- Nederlandse labels, foutmeldingen en succesmeldingen behouden.
- UI moet bruikbaar blijven op desktop en mobiel.
- Gebruik bestaande Bootstrap/Blazor-stijl; geen nieuw design framework.

Niet-doelen:

- Geen nieuwe accountfunctionaliteit.
- Geen wijziging aan wachtwoordlogica.
- Geen wijziging aan eigenaarprofiel-, bootprofiel- of operationele services.
- Geen nieuwe databasevelden of migraties.
- Geen brede design-system refactor.
- Geen extra settings-groepen zoals Systeem, Back-up of Notificaties.

Acceptatiecriteria:

- Settings toont een accordion met Account, Boot en Operationeel.
- Account is standaard open bij het laden van `/settings`.
- Boot en Operationeel zijn standaard ingeklapt.
- De gebruiker kan elke groep open- en dichtklappen.
- Eigenaarprofiel opslaan werkt nog.
- Wachtwoord wijzigen werkt nog.
- Bootgegevens opslaan werkt nog.
- Operationele instellingen opslaan werkt nog.
- Bestaande Nederlandse meldingen blijven intact.
- `dotnet build` slaagt.
- Handmatige UI-test vóór commit/PR:
  - `/settings` openen;
  - controleren dat Account open is;
  - Boot en Operationeel openklappen;
  - alle secties visueel controleren;
  - minimaal één bestaande save-flow per groep nalopen.

**Implementatie (2026-05-25):**

- `BootManager.Web/Components/Pages/Settings.razor` herstructureert de bestaande vier losse cards naar een accordion met drie groepen:
  - Account: eigenaarprofiel en wachtwoord wijzigen.
  - Boot: bootgegevens.
  - Operationeel: operationele instellingen.
- De accordion wordt bewust via Blazor-state aangestuurd met `@onclick`, niet via Bootstrap collapse JavaScript. Dit voorkomt afhankelijkheid van de gemengde huidige UI-stack met Bootstrap 5 CSS en Bootstrap 4/SB Admin scripts.
- Bootstrap accordion classes blijven alleen voor styling gebruikt: `.accordion`, `.accordion-item`, `.accordion-header`, `.accordion-button`, `.accordion-body`, `.accordion-collapse`, `.collapse` en `.show`.
- Alle bestaande state, handlers, validatie, service-aanroepen en Nederlandse meldingen blijven inhoudelijk gelijk.
- Geen migrations, DTO-wijzigingen of service-wijzigingen.
- Settings-pagina blijft responsive op desktop en mobiel.

Legacy coverage:

- Geen nieuwe legacy-functionaliteit; dit is UX/structuur op bestaande dekking.
- Raakt indirect `US0.4`, `US0.6`, `US1.2` en `US8.x`.
- Verwachte coverage-status verandert niet door deze story.

### US5: Actuele Motoruren- En Logstand Beheren

**Status:** Geïmplementeerd en handmatig geaccepteerd op 2026-06-07;
onderdeel van `LOG-TRIP-AUTO-1A`.

**User story:** Als eigenaar wil ik bij de bootinstellingen de actuele
motorurenstand en logstand beheren, zodat nieuwe reizen deze waarden bewust
kunnen overnemen en ik na vervanging van apparatuur een nieuwe lagere
beginstand kan instellen.

Scope:

- Voeg optionele actuele motorurenstand en actuele logstand toe aan
  `VesselProfile`.
- Toon en wijzig deze waarden in de bestaande Settings-accordiongroep `Boot`.
- Sta niet-negatieve decimalen en lege waarden toe.
- Een expliciete Settings-update mag een lagere waarde opslaan als reset.
- Reisopslag mag deze waarden alleen automatisch verhogen met geldige hogere
  tellerstanden; lagere, lege, negatieve of `0`-reiswaarden verlagen een
  bestaande positieve stand niet.
- Historische reizen worden niet opnieuw gescand, zodat een reset behouden
  blijft.

Buiten scope:

- Tellerhistorie/auditlog.
- Sensorselectie of bronvoorkeuren.
- Multi-vessel ondersteuning.

Acceptatiecriteria:

- Waarden blijven na refresh behouden.
- Een bewuste lagere reset kan worden opgeslagen.
- De logboekflow kan de actuele waarden lezen voor expliciete overname.
- De logboekflow kan na reisopslag alleen hogere geldige waarden
  voortschrijven.
- EF Core-migratie, build en relevante tests slagen.

Zie voor de volledige end-to-endregels:
[`logbook-trip-autofill.md`](logbook-trip-autofill.md).

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
