# BootManagerV2

BootManagerV2 is een lokale, Raspberry Pi-vriendelijke bootmanagementapplicatie met digitaal logboek, NMEA/YDEN-ingest, live dashboard en systeembeheer voor gebruik aan boord.

<!-- PROJECT-STATUS:START -->
## Projectstatus

_Laatst bijgewerkt: 2026-07-17. Gegenereerd met `scripts/update-readme-status.ps1`._

De percentages zijn voortgangsindicatoren, geen harde planning. Berekening: `Done` en `Replaced` tellen als 100%, `Partial` telt als 50%, `Open` telt als 0%. `Parked` en `Obsolete` tellen niet mee in de actieve scope.

Legacy-percentages worden automatisch berekend uit `.docs/legacy-analysis/legacy-coverage-register.md`. BootManagerV2-epicpercentages en de vakantiepilot worden expliciet onderhouden in het generator-script, omdat de bron-documenten nog niet overal dezelfde statusstructuur hebben.

### Samenvatting

| Scope | Voortgang | Actieve items |
|---|---:|---:|
| Vakantiepilot 2026 | `[##################--]` 91.7% | 24 |
| BootManagerV2 huidige epics | `[##############------]` 72% | 66 |
| Legacy scope | `[#######-------------]` 33.7% | 129 |

### Vakantiepilot 2026

| Voortgang | Done | Partial | Open | Parked | Bron | Eerstvolgende story |
|---:|---:|---:|---:|---:|---|---|
| `[##################--]` 91.7% | 22 | 0 | 2 | 0 | [.docs/releases/holiday-pilot-2026.md](.docs/releases/holiday-pilot-2026.md) | PILOT-E2E-01 - End-to-end gebruikstest |

### BootManagerV2 Epics

| Epic | Voortgang | Done | Partial | Open | Parked | Bron | Notitie |
|---|---:|---:|---:|---:|---:|---|---|
| First-run onboarding & authenticatie | `[####################]` 100% | 7 | 0 | 0 | 0 | [.docs/epics/first-run-onboarding.md](.docs/epics/first-run-onboarding.md) | Kernflow afgerond |
| Owner profile & vessel settings | `[####################]` 100% | 5 | 0 | 0 | 0 | [.docs/epics/owner-profile-settings.md](.docs/epics/owner-profile-settings.md) | Settings-basis en actuele tellerstanden afgerond |
| NMEA ingest & sensordata | `[################----]` 78.9% | 14 | 2 | 3 | 1 | [.docs/epics/nmea0183-support.md](.docs/epics/nmea0183-support.md) | Basis, simulator, Pi-analyse en tankniveau; bronvoorkeuren open |
| Digitaal logboek | `[###############-----]` 76.7% | 9 | 5 | 1 | 0 | [.docs/epics/digital-logbook.md](.docs/epics/digital-logbook.md) | Basis en tellerstandvoorinvulling klaar; routekaart en export open |
| Dashboard & live overzicht | `[##########----------]` 50% | 2 | 0 | 2 | 1 | [.docs/epics/dashboard-overview.md](.docs/epics/dashboard-overview.md) | Live meters en configureerbare tegels klaar; widgets/push open |
| Meetweergave & eenheidsvoorkeuren | `[--------------------]` 0% | 0 | 0 | 2 | 0 | [.docs/epics/measurement-unit-preferences.md](.docs/epics/measurement-unit-preferences.md) | Gebruikerskeuze voor nautische eenheden en consistente weergave open |
| System operations & recovery | `[##########----------]` 50% | 7 | 0 | 7 | 1 | [.docs/epics/system-operations.md](.docs/epics/system-operations.md) | Reset, Pi analyse/control/shutdown klaar; backup/diagnostics open |

### Legacy Epics

| Legacy epic | Voortgang | Done | Replaced | Partial | Open | Parked |
|---|---:|---:|---:|---:|---:|---:|
| Epic 0: Installatie & Authenticatie | `[####################]` 100% | 4 | 2 | 0 | 0 | 0 |
| Epic 1: Bootbeheer & Gebruikersbeheer | `[###############-----]` 75% | 7 | 0 | 4 | 1 | 5 |
| Epic 2: Inventarisbeheer | `[#############-------]` 65.8% | 10 | 0 | 5 | 4 | 2 |
| Epic 3: Passageplanning | `[--------------------]` 0% | 0 | 0 | 0 | 13 | 1 |
| Epic 4: Documentbeheer | `[--------------------]` 0% | 0 | 0 | 0 | 12 | 1 |
| Epic 5: Logboek | `[############--------]` 61.5% | 4 | 0 | 8 | 1 | 1 |
| Epic 6: Onderhoudsbeheer | `[--------------------]` 0% | 0 | 0 | 0 | 13 | 1 |
| Epic 7: Dashboard | `[#####---------------]` 25% | 0 | 0 | 6 | 6 | 2 |
| Epic 8: Systeembeheer & Configuratie | `[#####---------------]` 25% | 1 | 0 | 3 | 6 | 4 |
| Epic 9: Integraties & Synchronisatie | `[###-----------------]` 16.7% | 0 | 0 | 2 | 4 | 1 |
| Epic 10: Rapportage & Analyse | `[###-----------------]` 16.7% | 0 | 0 | 2 | 4 | 0 |
| Epic 11: Notificaties & Waarschuwingen | `[--------------------]` 0% | 0 | 0 | 0 | 6 | 0 |
| Epic 12: Slimme Herkenning & AI-Ondersteuning | `[##########----------]` 50% | 0 | 0 | 1 | 0 | 5 |

<!-- PROJECT-STATUS:END -->

## Actieve release

De leidende release is de
[BootManager Holiday Pilot 2026](.docs/releases/holiday-pilot-2026.md). De pilotstatus,
voortgang en eerstvolgende story worden bij iedere documentatie-update samen met deze
README gecontroleerd en bijgewerkt.
