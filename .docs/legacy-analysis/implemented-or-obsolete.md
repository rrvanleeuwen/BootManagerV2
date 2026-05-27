# Implemented, Replaced, Or Obsolete Legacy Stories

Status: eerste triage (2026-05-25).

## Al Geïmplementeerd Of Grotendeels Afgedekt

### Installatie/Auth/Onboarding

- Legacy US0.1 Installatie uitvoeren:
  - Afgedekt door eerste succesvolle Raspberry Pi 4 Docker Compose deployment-smoke-test op 2026-05-26.
  - Gevalideerd: OS Lite 64-bit, SSH, GitHub SSH clone, lokale `.env`, ARM64 Docker build, Web/Ingest containers, healthcheck, netwerkbereikbaarheid en reboot.
- Legacy US0.2 Registratie eerste eigenaar:
  - Vervangen door bootstrap owner + first-run onboarding.
- Legacy US0.3 Inloggen als eigenaar:
  - Wachtwoord-only login aanwezig.
- Legacy US1.1 Eerste opstart en bootaanmaak:
  - Afgedekt door onboarding + `VesselProfile`.

Word-verificatie Epic 0:

- `BootManager_Epic0_Installatie_Authenticatie.docx` bevestigt dat US0.1 t/m US0.6 de volledige legacy install/auth-scope vormen.
- Er zijn geen extra Epic 0 user stories gevonden buiten de eerder geinventariseerde lijst.

### Logboek

- Legacy US5.1 Handmatig logboek invoeren:
  - Deels afgedekt door handmatige logboekregels met nautische velden en opmerkingen.
- Legacy US5.x Logboek/reis aanmaken:
  - BootManagerV2 heeft `LogbookTrip` en `LogbookEntry`.
- Legacy US5.2 Automatisch loggen en interval:
  - Deels afgedekt door missing moments + Draft-regels op loginterval.
- Legacy US5.7 Nautische logregels:
  - Deels afgedekt door logregelvelden en measurement suggestions.
- Legacy US5.8 Bijlagen toevoegen:
  - Afgedekt door logbook attachments.
- Legacy US5.9 Klassiek format:
  - Deels afgedekt door logboek UI en printweergave.
- Legacy US5.10 Export logboek:
  - Deels afgedekt door browser print; PDF/CSV export blijft open.

Word-verificatie Epic 5:

- `BootManager_Epic5_Logboek.docx` bevestigt US5.1 t/m US5.14 volledig.
- US5.1 is handmatig logboek invoeren met weerinformatie.
- Open blijft vooral: passagekoppeling, routekaart, brandstof/motoruren afronding, uitgebreide statistieken en echte PDF/CSV-export.

### Systeem/Configuratie

- Legacy US8.1 Instellingenpagina openen:
  - Settings-pagina bestaat.
- Legacy US8.5 Sensorintegratie configureren:
  - Deels afgedekt door operationele ingest settings.
- Legacy NMEA/sensorintegratie uit brede legacy-scope:
  - BootManagerV2 heeft NMEA 0183/NMEA2000 ingest, parsing, interpretation en opslag.

### Integraties

- Legacy US9.2 AIS integratie:
  - Deels technisch voorbereid doordat NMEA 0183 `!`-sentences en AIS sentence ids raw/parser-technisch worden herkend.
  - AIS-semantiek en een schepenoverzicht ontbreken nog.
- Legacy US9.5 Sensorintegratie via Bluetooth of Wi-Fi:
  - Deels vervangen door UDP/Web API ingest voor NMEA/YDEN.
  - Bluetooth/Wi-Fi sensor onboarding zelf is niet aanwezig.

Word-verificatie Epic 8:

- `BootManager_Epic8_Systeembeheer.docx` bevestigt US8.1 t/m US8.14 volledig.
- Open blijft vooral: eenheden, taal/regio, back-up/herstel UI, Raspberry Pi systeeminfo, systeemactielog, instellingen export/import en standaardinstellingen herstellen.
- Gebruikersrollen, cloudaccounts, synchronisatieplanning en offline-sync toggle blijven geparkeerd zolang BootManagerV2 single-owner en local-first is.

## Bewust Vervangen Of Niet Meer Relevant In Oude Vorm

### Multi-user en rollen

Legacy stories:

- US1.3 Gebruikers aanmaken en rollen toewijzen.
- US1.4 Inloggen als bestaande gebruiker.
- US1.7 Gebruikersrechten wijzigen.
- US1.8 Gebruiker verwijderen.
- US8.4 Gebruikersrollen beheren.
- US8.7 Gebruikersbeheer.

Besluit:

- Voorlopig niet relevant in oude vorm.
- BootManagerV2 volgt single-owner flow.
- Rollen/crew accounts kunnen later als aparte epic terugkomen, maar niet als onderdeel van huidige onboarding.

Word-verificatie Epic 1:

- `BootManager_Epic1_Bootbeheer_en_Gebruikersbeheer.docx` bevestigt dat deze rollen eigenaar, bemanning en alleen-lezen waren.
- Er zijn geen extra multi-user stories buiten US1.3, US1.4, US1.7 en US1.8 gevonden in Epic 1.

### Pincode/recovery/master-key

Legacy stories:

- US0.4 Wachtwoord of pincode wijzigen.
- US0.5 Herstel van toegang.

Besluit:

- Pincode en recovery/master-key UI zijn bewust uit de normale flow gehaald.
- Wachtwoord wijzigen blijft relevant.
- Herstel gebeurt voorlopig via operationele resetprocedure met fysieke/admin toegang.

### Cloud-synchronisatie

Legacy stories verspreid over:

- inventory;
- passageplanning;
- documenten;
- logboek;
- onderhoud;
- dashboard;
- systeembeheer.

Besluit:

- Geparkeerd.
- BootManagerV2 richt zich voorlopig op lokale/offline Raspberry Pi werking.
- Cloud/sync niet meenemen in korte/middellange user stories.

### Meerdere boten

Legacy stories:

- US1.5 Meerdere boten beheren.
- US1.6 Boot selecteren bij opstart.
- US1.17 Cloud-bootselectie.

Besluit:

- Geparkeerd.
- BootManagerV2 gebruikt voorlopig singleton `VesselProfile` per installatie.

Word-verificatie Epic 1:

- Multi-boot context moest voorraad, documenten en andere gegevens per boot laden.
- Dit bevestigt dat multi-boot niet losstaand is, maar brede data-isolatie vraagt en daarom terecht geparkeerd blijft.

## Deels Geïmplementeerd Maar Nieuwe Stories Nodig

- Owner profile beheren:
  - Nieuwe epic `Owner Profile & Vessel Settings`.
- Bootinformatie bewerken:
  - Nieuwe epic `Owner Profile & Vessel Settings`.
- Bootstructuur, gebieden, opslaglocaties en QR-tags:
  - Nieuwe stories nodig binnen toekomstige inventaris/opslaglocatie-epic.
- Inventarisbeheer:
  - Word-verificatie van Epic 2 bevestigt US2.1 t/m US2.21.
  - Nog niet geïmplementeerd in BootManagerV2.
  - Eerste relevante slices zijn categorieën, opslaglocaties, productcatalogus, voorraad per locatie, voorraadmutaties en zoeken/filteren.
- Passageplanning:
  - Word-verificatie van Epic 3 bevestigt US3.1 t/m US3.14.
  - Nog niet geïmplementeerd als eigen module.
  - BootManagerV2 heeft wel `LogbookTrip`, maar dat is nog geen passageplanning.
  - Eerste passage-slices moeten lokaal/offline blijven en niet wachten op cloud-sync.
- Onderhoudsbeheer:
  - Word-verificatie van Epic 6 bevestigt US6.1 t/m US6.14.
  - Nog niet geïmplementeerd in BootManagerV2.
  - Eerste relevante slices zijn onderhoudstaak CRUD, onderdeelkoppeling, uitgevoerd onderhoud registreren en onderhoudshistoriek.
- Dashboard:
  - Word-verificatie van Epic 7 bevestigt US7.1 t/m US7.14 en lost US7.1 t/m US7.8 op.
  - BootManagerV2 heeft een basisdashboard/home, maar geen legacy widgetdashboard.
  - Logboekwaarschuwingen bestaan deels; voorraad-, onderhouds-, document- en passagewidgets wachten op die modules.
- Integraties:
  - Word-verificatie van Epic 9 bevestigt US9.1 t/m US9.7 en lost US9.1 t/m US9.5 op.
  - Weer/getijden, GPX/Navionics, haveninformatie, generiek API-sleutelbeheer en device-sync zijn niet geïmplementeerd.
- Rapportage:
  - Word-verificatie van Epic 10 bevestigt US10.1 t/m US10.6.
  - Alleen logboek browser-print is deels aanwezig.
  - Brandstof-, voorraad-, onderhouds- en kostenanalyse wachten op onderliggende data en modules.
- Notificaties:
  - Word-verificatie van Epic 11 bevestigt US11.1 t/m US11.6.
  - BootManagerV2 heeft in-app logboeksignalen voor missing moments, maar geen generieke notificatiemodule.
  - Lage voorraad, documentverval, onderhoud en passagewaarschuwingen wachten op die modules.
- AI-ondersteuning:
  - Word-verificatie van Epic 12 bevestigt US12.1 t/m US12.6.
  - Niet geïmplementeerd en lage prioriteit.
  - Barcode/QR zonder AI kan later als inventarisfeature eerder komen dan AI-herkenning, predictief onderhoud of spraakinput.
- Wachtwoord wijzigen:
  - Technisch aanwezig in Settings, maar UX/runtime-validatie als story nodig.
- Documentbeheer:
  - Logboekbijlagen bestaan; algemene documentkluis ontbreekt.
  - Word-verificatie van Epic 4 bevestigt US4.1 t/m US4.13.
  - Documentbeheer moet apart worden ontworpen van logboekbijlagen, met lokale/offline opslag, metadata, categorieën en vervaldatums als eerste kern.
- Systeembeheer:
  - Operationele ingest settings bestaan; backup/restore, eenheden en systeeminfo ontbreken.
