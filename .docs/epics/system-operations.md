# Epic: System Operations & Recovery

Status: gestart op 2026-05-26. Eerste kandidaat-story is vastgelegd, nog niet geïmplementeerd.

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

**Status:** Goedgekeurd als binnenkort op te pakken story op 2026-05-26.

**User story:** Als ontwikkelaar/helpdesk wil ik via een veilige onderhoudsprocedure de lokale BootManager database kunnen resetten, zodat ik een Raspberry Pi testinstallatie opnieuw door bootstrap login en onboarding kan laten lopen zonder handmatig Docker volumes of databasebestanden te verwijderen.

**Prioriteit:** Hoog binnen deployment/operability. Deze story hoort vóór verdere productiehardening en vóór veel herhaalde Pi-testcycli opgepakt te worden. De echte YDEN-broadcasttest kan eventueel nog tussendoor als korte hardwarecheck, maar deze resetflow is de eerstvolgende logische systeembeheer-slice.

**Scope:**

- Voeg een gecontroleerd resetmechanisme toe voor Docker Compose Pi-installaties.
- De reset is operator-only via SSH/lokale beheercontext, niet publiek via de web UI.
- Stop de containers netjes voordat de database wordt verplaatst of gereset.
- Bewaar de bestaande SQLite database eerst met een timestamped naam of maak een timestamped backup.
- Verwijder of vervang daarna alleen de actieve SQLite database, niet de volledige Docker volumes.
- Laat `.env`, Git checkout, capture logs en logboekbijlagen ongemoeid, tenzij expliciet anders gekozen in documentatie.
- Start de containers opnieuw.
- BootManager.Web moet bij startup opnieuw migraties toepassen en een bootstrap owner maken op basis van `BOOTMANAGER_BOOTSTRAP_PASSWORD`.
- Documenteer de procedure in `.docs/docker-deployment.md`, `.docs/pi-first-install-runbook.md` en waar nodig `.docs/raspberry-pi-deployment.md`.

**Buiten scope:**

- Geen web-UI knop voor factory reset.
- Geen remote reset endpoint.
- Geen algemene back-up/restore UI.
- Geen terugzetten van oude backups.
- Geen reset van `.env`, Git repo, Docker images of Docker build cache.
- Geen automatische verwijdering van bijlagen, capture logs of applicatielogs.
- Geen multi-user of rollenmodel.

**Acceptatiecriteria:**

- Een operator kan op de Pi een reset uitvoeren zonder `docker compose down -v`.
- De actieve database wordt niet stilzwijgend vernietigd; er ontstaat eerst een herkenbare timestamped backup of hernoemde database.
- Na de reset starten `bootmanager-web` en `bootmanager-ingest` opnieuw via Docker Compose.
- `curl -i http://localhost:5000/health` geeft opnieuw `HTTP 200`.
- Login met `BOOTMANAGER_BOOTSTRAP_PASSWORD` werkt opnieuw bij de verse database.
- De gebruiker wordt opnieuw verplicht naar `/onboarding` gestuurd.
- Na afronden van onboarding werkt het bootstrap-wachtwoord niet meer en werkt het nieuw gekozen wachtwoord.
- De procedure laat `.env` en Git checkout ongemoeid.
- De procedure is gedocumenteerd inclusief waarschuwing dat actieve applicatiedata uit de database wordt losgekoppeld.

**Legacy coverage impact:**

- `US0.5 Herstel van toegang`: blijft `Replaced`, maar de operationele resetprocedure wordt concreter en veiliger.
- `US8.8 Back-up maken en herstellen`: blijft `Open` of hooguit `Partial`; deze story maakt alleen een reset-backup/rename en is geen volledige restorefunctie.
- `US8.14 Standaardinstellingen herstellen`: blijft `Open` of `Partial`; deze story herinitialiseert de database voor test/herstel, maar biedt geen algemene instellingen-reset in de UI.
- `US8.11 Systeemactie-logboek`: blijft `Open`; logging van deze procedure kan later worden toegevoegd.

**Handmatige testnotities:**

- Test op een Raspberry Pi Docker Compose installatie met bestaande database.
- Noteer vooraf `docker compose ps`, `/health`, en of login/onboarding al afgerond is.
- Voer de reset uit.
- Controleer dat een timestamped databasebackup of hernoemde database bestaat.
- Controleer `docker compose ps`.
- Controleer `/health`.
- Log in met `BOOTMANAGER_BOOTSTRAP_PASSWORD`.
- Doorloop onboarding met testgegevens en nieuw wachtwoord.
- Controleer dat het bootstrap-wachtwoord daarna niet meer werkt.

## Relatie Tot Latere Stories

Deze story is een kleine operator-slice en vervangt niet:

- volledige back-up maken van database, bijlagen en configuratie;
- restore vanuit een gekozen back-up;
- Raspberry Pi systeemstatus in de UI;
- systeemactie-logboek;
- veilige shutdown vanuit UI/helper-service.

Die onderwerpen blijven aparte systeembeheerstories.
