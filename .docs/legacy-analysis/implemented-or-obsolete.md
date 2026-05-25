# Implemented, Replaced, Or Obsolete Legacy Stories

Status: eerste triage (2026-05-25).

## Al Geïmplementeerd Of Grotendeels Afgedekt

### Installatie/Auth/Onboarding

- Legacy US0.2 Registratie eerste eigenaar:
  - Vervangen door bootstrap owner + first-run onboarding.
- Legacy US0.3 Inloggen als eigenaar:
  - Wachtwoord-only login aanwezig.
- Legacy US1.1 Eerste opstart en bootaanmaak:
  - Afgedekt door onboarding + `VesselProfile`.

### Logboek

- Legacy US5.1/US5.x Logboek/reis aanmaken:
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

### Systeem/Configuratie

- Legacy US8.1 Instellingenpagina openen:
  - Settings-pagina bestaat.
- Legacy US8.5 Sensorintegratie configureren:
  - Deels afgedekt door operationele ingest settings.
- Legacy NMEA/sensorintegratie uit brede legacy-scope:
  - BootManagerV2 heeft NMEA 0183/NMEA2000 ingest, parsing, interpretation en opslag.

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

## Deels Geïmplementeerd Maar Nieuwe Stories Nodig

- Owner profile beheren:
  - Nieuwe epic `Owner Profile & Vessel Settings`.
- Bootinformatie bewerken:
  - Nieuwe epic `Owner Profile & Vessel Settings`.
- Wachtwoord wijzigen:
  - Technisch aanwezig in Settings, maar UX/runtime-validatie als story nodig.
- Documentbeheer:
  - Logboekbijlagen bestaan; algemene documentkluis ontbreekt.
- Logboek export:
  - Browser print bestaat; PDF/CSV blijft open.
- Systeembeheer:
  - Operationele ingest settings bestaan; backup/restore, eenheden en systeeminfo ontbreken.
