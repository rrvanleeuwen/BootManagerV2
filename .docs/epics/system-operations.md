# Epic: System Operations & Recovery

Status: SYS-RESET-1 geïmplementeerd, gemerged naar `master` en handmatig gevalideerd op Raspberry Pi op 2026-05-27.

## Aanleiding

BootManager draait nu succesvol op een Raspberry Pi 4 met Docker Compose. Voor ontwikkel-, test- en helpdeskscenario's is een gecontroleerde manier nodig om een installatie terug te zetten naar de eerste-start toestand zonder handmatig Docker volumes of SQLite-bestanden te verwijderen.

Handmatig de database verwijderen of `docker compose down -v` gebruiken is op de Raspberry Pi ongewenst:

- het risico op onbedoeld dataverlies is groot;
- bijlagen, logs of capturebestanden kunnen onnodig verdwijnen;
- het is lastig reproduceerbaar voor helpdesk of ontwikkeltests;
- GitHub `master` moet leidend blijven en lokale afwijkingen op de Pi zijn niet gewenst.

## Doel

BootManager krijgt kleine, gecontroleerde operationele hulpmiddelen voor Raspberry Pi/Docker beheer:

- testinstallatie gecontroleerd opnieuw initialiseren;
- bestaande data eerst veiligstellen;
- bootstrap/onboarding opnieuw starten;
- latere uitbreiding richting back-up, restore, systeemstatus en veilige shutdown mogelijk houden.

## Uitgangspunten

- De eerste slice is bedoeld voor ontwikkelaar/helpdesk/operator via SSH of lokale beheercontext.
- Geen publieke webknop voor factory reset.
- Geen reset zonder expliciete waarschuwing en bevestiging.
- `.env` blijft bestaan en blijft lokaal per apparaat.
- Secrets worden niet gelogd, niet geback-upt naar Git en niet in documentatie opgenomen.
- De bestaande Docker Compose deployment blijft leidend.

## User Stories

### SYS-RESET-1: Gecontroleerde Database Reset Voor Pi Testinstallatie

**Status:** ✅ Geïmplementeerd, gemerged naar `master` en handmatig gevalideerd op 2026-05-27.

**Implementation Status (2026-05-27):**

Code, script, and documentation are complete. The following have been delivered:

**Implementation Components:**

1. **`scripts/reset-database.sh`** (new)
   - Bash operator script for Raspberry Pi
   - Moet expliciet met `sudo` worden gestart
   - Dynamically detects Docker volume name (not hardcoded)
   - Safety checks: Docker/Compose availability, docker-compose.yml present
   - User confirmation prompt before any destructive action
   - Stops containers cleanly with `docker compose stop`
   - Creates timestamped backup of SQLite database
   - Removes active database file only
   - Restarts containers and validates health check
   - Full error handling and operator-friendly output

2. **`.docs/docker-deployment.md`** (updated)
   - "Gecontroleerde Database Reset (Testinstallatie)" section
   - Usage scenarios, procedures, verification steps
   - Troubleshooting guide, safety warnings
   - Exact Pi commands documented

3. **`.docs/pi-first-install-runbook.md`** (updated)
   - Section 18a: "Gecontroleerde Database Reset"
   - References automated reset script
   - Backup location and naming documentation

4. **`.docs/raspberry-pi-deployment.md`** (updated)
   - "Onderhoud en Operaties" section
   - Reset procedure overview and scenarios
   - Cross-references to detailed docs

**Pi Validatie Uitgevoerd (2026-05-27):**

Handmatige validatie is uitgevoerd op een Raspberry Pi Docker Compose installatie op `master`.

- [x] Operator kon reset script uitvoeren zonder `docker compose down -v`
- [x] Script detecteerde correct het Docker volume met project-prefix (`bootmanagerv2_bootmanager-db`)
- [x] Timestamped database backup werd succesvol aangemaakt
- [x] Actieve database file werd clean verwijderd
- [x] Containers startten opnieuw en bereikten gezonde status
- [x] Health check gaf opnieuw HTTP 200
- [x] Login met `BOOTMANAGER_BOOTSTRAP_PASSWORD` werkte op de verse database
- [x] Eerste login stuurde correct naar `/onboarding`
- [x] Na onboarding werkte bootstrap-wachtwoord niet meer
- [x] Nieuw wachtwoord werkte voor daaropvolgende login
- [x] `.env`, Git checkout, attachments en logs bleven ongemoeid voor zover gevalideerd in deze resetflow
- [x] Backupbestand bleef aanwezig met verwacht timestamp-formaat

**Manual Validation Steps (to be performed on Pi):**

```bash
# 1. Verify current state before reset
cd ~/BootManagerV2
docker compose ps
curl -i http://localhost:5000/health
docker volume ls | grep bootmanager-db

# 2. Execute reset script
sudo bash scripts/reset-database.sh
# Type 'yes' at confirmation prompt
# Observe container stop, database backup, removal, and restart

# 3. Verify reset completion
docker compose ps
curl -i http://localhost:5000/health

# 4. Check backup created with timestamp
PROJECT_NAME=$(docker compose config 2>/dev/null | grep -m1 "name:" | sed 's/.*name: //' | xargs) || PROJECT_NAME=$(basename "$(pwd)" | tr '[:upper:]' '[:lower:]' | tr -cd 'a-z0-9')
VOLUME_PATH=$(docker volume inspect "${PROJECT_NAME}_bootmanager-db" -f '{{.Mountpoint}}')
ls -lh $VOLUME_PATH/bootmanager.db*
# Should see: bootmanager.db and bootmanager.db.backup.YYYYMMDD_HHMMSS

# 5. Test login flow
# Access http://localhost:5000
# Login with BOOTMANAGER_BOOTSTRAP_PASSWORD (from .env)
# Verify forced redirect to /onboarding
# Complete onboarding with test data
# Set new password during onboarding
# Logout and login with new password (should work)
# Try login with BOOTMANAGER_BOOTSTRAP_PASSWORD (should fail)

# 6. Verify resources preserved
cat .env | grep BOOTMANAGER  # Should be intact
git status  # Should be clean
ls -la BootManager.Web/data/logbook-attachments 2>/dev/null || echo "No attachments yet"
docker volume ls | grep bootmanager-logs  # Should exist
```

**Administratieve Status:**

Deze story is administratief afgerond:

1. Implementatie is via PR #65 naar `master` gemerged.
2. Raspberry Pi validatie is succesvol uitgevoerd op `master`.
3. Reset, health check, bootstrap login, onboarding en nieuw wachtwoord zijn handmatig bevestigd.

**Legacy Coverage Impact:**

- `US0.5 Herstel van toegang`: `Replaced` en nu handmatig gevalideerd via operationele resetprocedure op de Pi
- `US8.8 Back-up maken en herstellen`: blijft `Open` (dit is reset-backup, geen volledige restore)
- `US8.14 Standaardinstellingen herstellen`: blijft `Open` (CLI reset, geen algemene UI-instellingen-reset)
- `US8.11 Systeemactie-logboek`: blijft `Open` (logging nog niet geïmplementeerd)

**User Story:** Als ontwikkelaar/helpdesk wil ik via een veilige onderhoudsprocedure de lokale BootManager database kunnen resetten, zodat ik een Raspberry Pi testinstallatie opnieuw door bootstrap login en onboarding kan laten lopen zonder handmatig Docker volumes of databasebestanden te verwijderen.

## Relatie Tot Latere Stories

Deze story is een kleine operator-slice en vervangt niet:

- volledige back-up maken van database, bijlagen en configuratie;
- restore vanuit een gekozen back-up;
- Raspberry Pi systeemstatus in de UI;
- systeemactie-logboek;
- veilige shutdown vanuit UI/helper-service.

Die onderwerpen blijven aparte systeembeheerstories.
