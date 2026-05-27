# Epic: System Operations & Recovery

Status: SYS-RESET-1 **implemented in branch** `feature/pi-database-reset` on 2026-05-27. **Pending manual Raspberry Pi validation** before administrative completion.

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

**Status:** 🔄 **Implemented in branch, pending manual Raspberry Pi validation.**

**Branch:** `feature/pi-database-reset`

**Implementation Status (2026-05-27):**

Code, script, and documentation are complete. The following have been delivered:

**Implementation Components:**

1. **`scripts/reset-database.sh`** (new)
   - Bash operator script for Raspberry Pi
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

**What Requires Manual Pi Validation (Pending):**

The following acceptance criteria require actual execution on a Raspberry Pi running Docker Compose to verify:

- [ ] Operator can execute reset script without `docker compose down -v`
- [ ] Script correctly detects Docker volume name (project-prefixed, not hardcoded)
- [ ] Timestamped database backup is created successfully
- [ ] Active database file is removed cleanly
- [ ] Containers restart and reach healthy state
- [ ] Health check returns HTTP 200 after restart
- [ ] Login with `BOOTMANAGER_BOOTSTRAP_PASSWORD` works with fresh database
- [ ] First login forces `/onboarding` flow
- [ ] After onboarding, bootstrap password no longer works
- [ ] New password set during onboarding works for subsequent logins
- [ ] `.env`, Git checkout, attachments, logs remain untouched
- [ ] Backup files persist at expected location with correct timestamp format

**Manual Validation Steps (to be performed on Pi):**

```bash
# 1. Verify current state before reset
cd ~/BootManagerV2
docker compose ps
curl -i http://localhost:5000/health
docker volume ls | grep bootmanager-db

# 2. Execute reset script
bash scripts/reset-database.sh
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

**Administrative Status:**

This story is **not yet administratively complete**. After successful manual validation on Pi:

1. Run the manual validation steps above
2. Document observations in session notes
3. Update this epic with validation results
4. Mark legacy coverage when validation confirms behavior
5. Create PR with validation confirmation

**Legacy Coverage Impact (Pending Validation):**

- `US0.5 Herstel van toegang`: Status will remain `Replaced` after Pi validation confirms operational reset works
- `US8.8 Back-up maken en herstellen`: Status remains `Open` (this is reset-backup only, not full restore)
- `US8.14 Standaardinstellingen herstellen`: Status remains `Open` (CLI reset, no UI settings reset)
- `US8.11 Systeemactie-logboek`: Status remains `Open` (logging not yet implemented)

**User Story:** Als ontwikkelaar/helpdesk wil ik via een veilige onderhoudsprocedure de lokale BootManager database kunnen resetten, zodat ik een Raspberry Pi testinstallatie opnieuw door bootstrap login en onboarding kan laten lopen zonder handmatig Docker volumes of databasebestanden te verwijderen.

## Relatie Tot Latere Stories

Deze story is een kleine operator-slice en vervangt niet:

- volledige back-up maken van database, bijlagen en configuratie;
- restore vanuit een gekozen back-up;
- Raspberry Pi systeemstatus in de UI;
- systeemactie-logboek;
- veilige shutdown vanuit UI/helper-service.

Die onderwerpen blijven aparte systeembeheerstories.
