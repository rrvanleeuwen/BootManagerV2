# Docker Compose Deployment voor BootManager

Doel: BootManager als containerized applicatie op Raspberry Pi (of andere Linux-systemen) draaien met Docker Compose, zonder handmatige service-configuratie.

## Gevalideerde Status

De eerste Raspberry Pi Docker Compose deployment is geslaagd op 2026-05-26.

Geteste omgeving:

- Raspberry Pi 4 Model B.
- 32 GB microSD.
- Raspberry Pi OS Lite 64-bit.
- Hostname: `bootmanager-pi`.
- Gebruiker: `roelof`.
- GitHub private repo via SSH-key op de Pi.
- Docker images lokaal gebouwd op de Pi; geen zip-workflow en geen werklaptop nodig.
- Repo op de Pi staat op `master` en moet een afspiegeling van `origin/master` blijven.

Gevalideerd:

- SSH vanaf laptop naar `roelof@bootmanager-pi.local`.
- `uname -m` gaf `aarch64`.
- GitHub SSH-toegang en clone via `git@github.com:rrvanleeuwen/BootManagerV2.git`.
- `docker compose config --services` toont `bootmanager-web` en `bootmanager-ingest`.
- `docker compose build` werkt op ARM64.
- `docker compose up -d` start beide containers.
- `bootmanager-web` is `healthy`.
- `bootmanager-ingest` blijft `Up`.
- `curl -i http://localhost:5000/health` geeft `HTTP/1.1 200 OK` met `{"status":"ok"}`.
- App is bereikbaar vanaf een laptop via `http://<pi-ip>:5000`.
- Reboot-test geslaagd: na `sudo reboot` kwamen beide containers automatisch terug en bleef `/health` OK.

## Overzicht

Docker Compose orchestreert twee services:

1. **bootmanager-web**: ASP.NET Core web API en UI
   - Luistert op TCP poort 5000
   - SQLite database in persistent volume
   - Gezondheidscheck elke 30 seconden

2. **bootmanager-ingest**: Console worker tool
   - Luistert op UDP poort 10110 (NMEA ontvangst)
   - Control API op TCP 5010 (localhost-only in de host)
   - Verstuurt geparste data naar web API
   - Capture logs opgeslagen in persistent volume

Beide services delen volumes voor:
- SQLite database (`bootmanager-db`)
- Logboekbijlagen (`bootmanager-attachments`)
- Capture en applicatielogs (`bootmanager-logs`)

## Systeemvereisten

- **Docker** (20.10+) en **Docker Compose** (2.0+)
- **Linux ARM64** (Raspberry Pi 3 B+ of hoger, 64-bit OS)
- Minimaal **1GB RAM** beschikbaar
- Minimaal **2GB schijfruimte** (excl. logs en capture data)
- Netwerk naar UDP poort 10110 (van sensor/simulator naar Pi)

Voor Raspberry Pi OS:
- Installeer Docker: https://docs.docker.com/engine/install/debian/
- Voeg user toe aan docker group: `usermod -aG docker $USER`

Tijdens de eerste Pi 4-test waren de versies:

- Docker: `Docker version 29.5.2`
- Docker Compose: `Docker Compose version v5.1.4`

## Build

De Dockerfiles gebruiken multi-stage builds om images klein te houden:

```bash
# Build beide images
docker compose build

# Of alleen web
docker compose build bootmanager-web

# Of alleen ingest
docker compose build bootmanager-ingest
```

**Opmerking:** Build kan 5-10 minuten duren op de eerste keer, zeker op RPi. Daarna zijn layers gecached.

De .NET runtime base images moeten multi-architecture tags gebruiken. Gebruik dus:

- `mcr.microsoft.com/dotnet/aspnet:8.0-jammy`
- `mcr.microsoft.com/dotnet/runtime:8.0-jammy`

Gebruik geen niet-bestaande `8.0-jammy-arm64` tags. Docker kiest op een Raspberry Pi automatisch de ARM64-variant van de multi-arch tag. De SDK-image `mcr.microsoft.com/dotnet/sdk:8.0` blijft ongewijzigd.

Als een build tijdelijk faalt met DNS-fouten zoals `Could not resolve 'ports.ubuntu.com'`, controleer eerst host- en Docker-DNS:

```bash
docker run --rm alpine nslookup ports.ubuntu.com
docker run --rm alpine ping -c 3 ports.ubuntu.com
```

Als die checks slagen, kan `docker compose build` opnieuw uitvoeren voldoende zijn. Tijdens de eerste Pi-test bleek dit een tijdelijke registry/build-netwerkfout, geen structurele Dockerfile-fout.

## Start

Maak eerst een lokale `.env` op basis van `.env.example`:

```bash
cp .env.example .env
nano .env
```

Vervang alle waarden. Gebruik lange willekeurige strings voor de encryptie- en JWT-sleutel. Kies voor `BOOTMANAGER_BOOTSTRAP_PASSWORD` een tijdelijk eerste-login-wachtwoord dat je na onboarding niet meer gebruikt. `.env` staat in `.gitignore` en mag niet worden gecommit.

Minimale `.env`:

```text
BOOTMANAGER_ENCRYPTION_KEY=replace-with-a-long-random-encryption-key
BOOTMANAGER_JWT_KEY=replace-with-a-long-random-jwt-signing-key
BOOTMANAGER_BOOTSTRAP_PASSWORD=replace-with-first-login-password
```

```bash
# Start services in achtergrond
docker compose up -d

# Of in voorgrand (voor debugging)
docker compose up

# Controleer status
docker compose ps

# Wacht tot health check slaagt (bootmanager-web moet 'healthy' zijn)
docker compose exec bootmanager-web dotnet --version
```

Na start is:
- Web UI bereikbaar op: http://localhost:5000 (of http://<pi-hostname>:5000)
- Ingest luistert op: UDP 10110
- Ingest control API beschikbaar op: 127.0.0.1:5010 (alleen op dezelfde host)

Werkende containerstatus tijdens de Pi 4-test:

- `bootmanager-web`: `Up` / `healthy`, `0.0.0.0:5000->5000/tcp` en `[::]:5000->5000/tcp`.
- `bootmanager-ingest`: `Up`, `0.0.0.0:10110->10110/udp`, `[::]:10110->10110/udp` en `127.0.0.1:5010->5010/tcp`.

## Eerste Start En Onboarding

Bij een lege database voert `bootmanager-web` bij startup de eerste-start flow uit:

1. EF Core migraties worden toegepast.
2. Als er nog geen owner bestaat, maakt de app exact een bootstrap owner aan.
3. De bootstrap owner gebruikt het wachtwoord uit `Bootstrap:DefaultPassword`, in Docker gezet via `BOOTMANAGER_BOOTSTRAP_PASSWORD`.
4. De eerste login leidt verplicht naar `/onboarding`.
5. De gebruiker vult eigenaargegevens, bootgegevens en een nieuw wachtwoord in.
6. Na succesvolle onboarding worden `PasswordChangeRequired=false` en `OnboardingCompleted=true`.
7. De gebruiker krijgt toegang tot het dashboard.

Production zonder bestaande owner en zonder `Bootstrap:DefaultPassword` faalt bewust bij startup. Dit voorkomt dat een deployment met een lege database onbedoeld zonder eerste-login-wachtwoord start.

Na onboarding is het bootstrap-wachtwoord ongeldig. Bewaar het dus alleen als tijdelijk installatiegeheim, niet als blijvend beheerderswachtwoord.

## Logs

```bash
# Logs van alle services
docker compose logs

# Logs van web service alleen
docker compose logs bootmanager-web

# Logs van ingest service alleen
docker compose logs bootmanager-ingest

# Real-time monitoring
docker compose logs -f

# Logs van ingest de afgelopen minuut
docker compose logs --since 1m bootmanager-ingest
```

Logs worden ook persistent opgeslagen in `bootmanager-logs` volume (beschikbaar op host).

## Stop

```bash
# Stop services (volumes blijven)
docker compose stop

# Stop en verwijder containers (volumes blijven)
docker compose down

# Stop en verwijder alles inclusief volumes
docker compose down -v
```

**Voorzichtig:** `docker compose down -v` verwijdert alle volumes, inclusief database en logs!

## Persistent Storage

Drie volumes zorgen voor data persistentie:

| Volume | Doel | Mount in container |
|--------|------|-------------------|
| `bootmanager-db` | SQLite database | `/var/lib/bootmanager` |
| `bootmanager-attachments` | Logboekbijlagen | `/app/data/logbook-attachments` |
| `bootmanager-logs` | Applicatie- en capture logs | `/var/log/bootmanager` |

Volumes worden automatisch aangemaakt door Docker.

Op de host zijn ze meestal hier terug te vinden:
```
/var/lib/docker/volumes/<project>_bootmanager-db/_data/
/var/lib/docker/volumes/<project>_bootmanager-attachments/_data/
/var/lib/docker/volumes/<project>_bootmanager-logs/_data/
```

(Vervang `<project>` door je Docker Compose projectnaam, meestal de directoryaam in lowercase.)

De bijlagenmap volgt de huidige applicatiestandaard `data/logbook-attachments`. Omdat de container met `/app` als werkmap draait, wordt dit gemount als `/app/data/logbook-attachments`.

Voor handmatige backup:

```bash
# Backup database
docker compose exec bootmanager-web tar czf - /var/lib/bootmanager/bootmanager.db | gzip > bootmanager-db-backup.tar.gz

# Backup logboekbijlagen
docker compose exec bootmanager-web tar czf - /app/data/logbook-attachments | gzip > bootmanager-attachments-backup.tar.gz

# Backup logs
docker compose exec bootmanager-web tar czf - /var/log/bootmanager | gzip > bootmanager-logs-backup.tar.gz
```

## Configuratie

Alle configuratie gebeurt via environment variables in `docker-compose.yml`. Geen `.appsettings.json` aanpassingen nodig.

### Web Service

| Var | Standaard | Toelichting |
|-----|-----------|------------|
| `ASPNETCORE_URLS` | `http://0.0.0.0:5000` | Bind adres en poort |
| `ConnectionStrings__Default` | `/var/lib/bootmanager/bootmanager.db` | SQLite database pad |
| `Encryption__Key` | uit `.env` | Encryptiesleutel voor gevoelige owner-data |
| `Jwt__Key` | uit `.env` | Signing key voor JWT API-authenticatie |
| `Bootstrap__DefaultPassword` | uit `.env` | Tijdelijk eerste-login-wachtwoord voor bootstrap owner bij lege database |
| `Logging__LogLevel__Default` | `Information` | Log niveau applicatie |
| `Logging__LogLevel__Microsoft.AspNetCore` | `Warning` | Log niveau ASP.NET Core |

### Ingest Service

| Var | Standaard | Toelichting |
|-----|-----------|------------|
| `Ingest__ListenAddress` | `0.0.0.0` | Bind adres UDP listener |
| `Ingest__ListenPort` | `10110` | UDP poort sensoren |
| `Ingest__ApiBaseUrl` | `http://bootmanager-web:5000` | Web service URL (gebruik service naam!) |
| `Ingest__CaptureLogging__Enabled` | `true` | Capture logs aanpassen |
| `Ingest__CaptureLogging__Directory` | `/var/log/bootmanager/ingest-capture` | Capture log pad |
| `Ingest__ControlApi__ListenAddress` | `0.0.0.0` | Bind adres control API (in container) |
| `Ingest__ControlApi__ListenPort` | `5010` | Control API poort |

De Ingest control API gebruikt intern `HttpListener`. Als `Ingest__ControlApi__ListenAddress`
op `0.0.0.0` staat, vertaalt BootManager dit naar de HttpListener-prefix
`http://*:5010/`. Dit is bewust: Docker bindt de poort aan de buitenkant nog steeds
localhost-only via `127.0.0.1:5010:5010`, terwijl de listener binnen de container op
alle containerinterfaces kan luisteren voor verkeer vanuit het Compose-netwerk.

Om configuratie aan te passen, edit `docker-compose.yml` en herstart:

```bash
# Edit dan:
docker compose up -d

# Automatic restart van services die config nodig hebben
```

## Netwerk

Beide services draaien in dezelfde Docker bridge network (`bootmanager-network`), dus:
- `bootmanager-ingest` bereikt `bootmanager-web` via hostname `bootmanager-web`
- Geen hardcoded IP-adressen of localhost nodig voor inter-service communicatie

Van buiten de containers:
- Web API: beschikbaar op host poort 5000
- Ingest UDP: beschikbaar op host poort 10110
- Ingest control API: beschikbaar op 127.0.0.1:5010 (alleen localhost in docker-compose.yml)

### Ingest Control API: Container-Intern vs. Host-Toegang

De Ingest control API werkt op twee niveaus:

**Container-intern (Web → Ingest):**
- `bootmanager-web` bereikt de Ingest control API via `http://bootmanager-ingest:5010` (service-naam).
- Dit is ingesteld in `docker-compose.yml` via `Ingest__ControlApi__BaseUrl=http://bootmanager-ingest:5010`.
- Werkt **alleen** binnenin het Docker Compose netwerk.
- Zonder deze override zou Web proberen `http://127.0.0.1:5010` te bereiken, wat binnen de Web-container naar zichzelf wijst, niet naar Ingest.

**Host-toegang (Pi/admin-machine → Ingest):**
- Docker Compose port binding: `127.0.0.1:5010:5010` (vastgesteld in `docker-compose.yml`).
- De control API is bereikbaar op de Pi/host zelf via `http://127.0.0.1:5010`.
- **Niet** bereikbaar van buiten de Pi (veilig voor localhost-only operationele commando's).

**Samengevat:**
| Context | Control API URL | Beschrijving |
|---------|-----------------|-------------|
| Web-container (naar Ingest) | `http://bootmanager-ingest:5010` | Service-naam within Docker network |
| Host/Pi (naar Ingest) | `http://127.0.0.1:5010` | Docker port binding, localhost-only |

## Relatie tot systemd Services

Dit Docker Compose setup is een **alternatief** voor systemd services (zie `.docs/raspberry-pi-deployment.md`).

| Aspect | Docker Compose | systemd Services |
|--------|-----------------|-----------------|
| Setup | Eenvoudig, één bestand | Meer handmatig |
| Versie-updates | Rebuild images | Republish en restart |
| Resource-overhead | Container runtime (~50MB RAM) | Minimal |
| Volume management | Automatisch | Handmatig |
| Log aggregatie | `docker compose logs` | `journalctl` |
| Debugging | Makkelijker (containers geïsoleerd) | Direct op systeem |

**Keuze advies:**
- Docker Compose: Voor reproduceerbare deployments, CI/CD-integratie, test-omgevingen
- systemd: Voor productie met strikte resource constraints, directe controle gewenst

## Health Check

`BootManager.Web` exposeert een eenvoudige anonieme endpoint:

```text
GET /health
```

Docker Compose gebruikt deze endpoint om te bepalen of de webcontainer klaar is. `bootmanager-ingest` wacht daardoor met starten tot de webservice gezond is.

Controleer handmatig:

```bash
curl -i http://localhost:5000/health
```

Verwacht resultaat:

```text
HTTP/1.1 200 OK
{"status":"ok"}
```

## Updateprocedure

GitHub `master` is leidend. De Pi hoort geen lokale afwijkingen te bevatten.

De Pi pullt alleen `master`, nooit feature-branches. Zolang een wijziging op een feature-branch staat en nog geen PR/merge naar `master` heeft gehad, vindt ontwikkeling en pre-PR validatie plaats op de ontwikkelcomputer/devomgeving. Codex mag in die fase niet suggereren dat de Pi de wijziging al heeft of moet testen.

De Pi hoeft niet automatisch na iedere push naar `master` direct een `git pull` te doen. Bij documentatie-only wijzigingen is een Pi-update meestal niet nodig. Als een update op de Pi nodig is, geeft Codex expliciet de exacte SSH-commando's en vermeldt Codex of containers opnieuw gebouwd, alleen herstart of helemaal niet aangepast moeten worden.

Beslisregel:

- Alleen documentatie gewijzigd: geen Pi-pull nodig, tenzij de documentatie lokaal op de Pi gelezen moet worden.
- Applicatiecode, Dockerfile, projectbestand, NuGet dependency of `docker-compose.yml` gewijzigd: `git pull`, daarna meestal `docker compose build` en `docker compose up -d`.
- Alleen runtimeconfiguratie via `.env` of `docker-compose.yml` gewijzigd: geen imagebuild nodig, wel `docker compose up -d`.
- Alleen containers herstarten zonder codewijziging: `docker compose restart`.

Wanneer Codex zegt dat de Pi bijgewerkt moet worden met nieuwe code:

```bash
cd ~/BootManagerV2
git pull
docker compose build
docker compose up -d
docker compose ps
curl -i http://localhost:5000/health
```

Alleen containers herstarten zonder nieuwe code:

```bash
cd ~/BootManagerV2
docker compose restart
docker compose ps
```

Logs bekijken:

```bash
cd ~/BootManagerV2
docker compose logs -f bootmanager-web
docker compose logs -f bootmanager-ingest
```

## Shutdown-knop

Er is geen shutdown-knop in deze skeleton. Redenen:

1. Voorkeur gegeven aan expliciete commando's (`docker compose stop/down`)
2. Accidenteel afsluiten voorkomen
3. Geen ongedocumenteerde state changes

Als later een shutdown-knop gewenst is, moet dit als aparte beheer-slice worden ontworpen:
- Owner/admin-only autorisatie;
- extra bevestiging;
- duidelijke logging van wie shutdown vraagt;
- host-side mechanisme voor Raspberry Pi shutdown, omdat een container de host normaal niet zomaar mag afsluiten.

Vooralsnog: `docker compose stop` is de standaard.

## Troubleshooting

### Web service start niet

```bash
# Logs controleren
docker compose logs bootmanager-web

# Typische oorzaken:
# - .env ontbreekt of bevat geen BOOTMANAGER_ENCRYPTION_KEY / BOOTMANAGER_JWT_KEY / BOOTMANAGER_BOOTSTRAP_PASSWORD
# - Database niet beschrijfbaar: volume permissions
# - Poort 5000 al in gebruik: sudo lsof -i :5000
# - Insufficient RAM: docker stats
```

### Eerste login of wachtwoord kwijt

De normale gebruikersflow bevat geen pincode, recovery-code of master-key UI. Als de enige gebruiker niet meer kan inloggen, is fysieke/admin toegang tot de Pi nodig.

Veilige resetprocedure:

1. Stop de containers:
   ```bash
   docker compose stop
   ```
2. Maak een backup van de database uit het Docker volume.
3. Hernoem of verwijder daarna pas de SQLite database in het `bootmanager-db` volume.
4. Controleer dat `.env` een geldig `BOOTMANAGER_BOOTSTRAP_PASSWORD` bevat.
5. Start opnieuw:
   ```bash
   docker compose up -d
   ```
6. Doorloop opnieuw bootstrap login en onboarding.

Deze reset maakt een nieuwe installatie-state aan. Bootgegevens wijzigen na afgeronde onboarding is een aparte toekomstige story.

### Ingest ontvangt geen berichten

```bash
# Controleer UDP listener
docker compose exec bootmanager-ingest netstat -uln | grep 10110

# Test connectiviteit van buiten container
echo "test" | nc -u localhost 10110

# Logs controleren
docker compose logs -f bootmanager-ingest
```

### YDEN UDP broadcast boot-test

De YDEN stuurt UDP broadcast naar een poort, niet naar een vast Pi-IP. Daarom hoeft de YDEN waarschijnlijk geen Raspberry Pi IP-adres ingesteld te krijgen.

Voorwaarden:

- YDEN en Raspberry Pi zitten in hetzelfde LAN/subnet.
- De YDEN UDP-poort komt overeen met de Ingest listener, standaard `10110`.
- Broadcast gaat normaal niet over router-, VLAN- of gastnetwerkgrenzen.

Checklist op de boot:

```bash
# Pi-IP controleren
hostname -I

# App openen vanaf laptop/tablet
# http://<pi-ip>:5000

# Containers controleren
cd ~/BootManagerV2
docker compose ps

# Ingest logs volgen
docker compose logs -f bootmanager-ingest

# UDP broadcast controleren
sudo apt install -y tcpdump
sudo tcpdump -i any udp port 10110
```

Interpretatie:

- Als `tcpdump` pakketten toont maar BootManager niets verwerkt, zit het probleem vermoedelijk in Ingest/configuratie/parser.
- Als `tcpdump` niets toont, zit het probleem vermoedelijk in netwerk/YDEN/Teltonika/subnet/broadcast.

### IngestControlServer crasht op HttpListener

Symptoom:

```text
System.Net.HttpListenerException (50): The request is not supported.
Starting IngestControlServer on 0.0.0.0:5010...
```

Oorzaak: `HttpListener` accepteert op Linux/.NET niet betrouwbaar een prefix zoals `http://0.0.0.0:5010/`.

Oplossing in `master`:

- Commit `4ef3d73 Fix IngestControlServer HttpListener prefix for wildcard binding`.
- BootManager vertaalt `0.0.0.0` intern naar `http://*:5010/`.
- Geen lokale Docker Compose workaround nodig.

### Database-permissie errors

Volumes hebben soms permission issues op Linux:

```bash
# Controleer ownership in volume
docker compose exec bootmanager-web ls -la /var/lib/bootmanager

# Eventueel: herstart met andere user mode
# Dit vraagt Docker setup en advanced configurations
```

### Performance op Raspberry Pi

- Monitor resource use: `docker stats`
- Zet Ingest capture logging uit als niet nodig: `Ingest__CaptureLogging__Enabled=false`
- Beperk log grootte: zie `docker-compose.yml` logging options

## Raspberry Pi Specifieke Aandachtspunten

### 1. Schijfruimte

Raspberry Pi heeft beperkte schijfruimte. Let op:
- Capture logs kunnen groeien (gigabytes per dag bij veel sensoren)
- Zet capture uit of limiteer: `Ingest__CaptureLogging__Enabled=false`
- Verwijder oude logs: `docker compose exec bootmanager-web sh -c 'find /var/log/bootmanager -type f -mtime +7 -delete'`

Eerste Pi 4-test met 32 GB SD:

- Root filesystem: 29 GB.
- Gebruikt: 6.0 GB.
- Beschikbaar: 22 GB.
- Gebruik: 22%.
- Docker images: 3 totaal, 2 actief, 2.302 GB.
- Docker build cache: 2.58 GB, waarvan 2.234 GB reclaimable.

Conclusie: 32 GB SD is voldoende voor een weekendtest/proof-of-concept. Voor langdurige logging of productie is een Compute Module/industrial Pi met eMMC, NVMe of SSD beter.

### 2. Geheugen

Pi 3 B+ of een 1 GB Pi 4 heeft weinig marge, maar de eerste Pi 4-test op 1 GB RAM was acceptabel. Docker Compose gebruikt ~50-100MB. Applicatie zelf:
- Web service: ~100-200MB (ASP.NET Core)
- Ingest: ~50MB (console app)

Total ~150-350MB, dus behapbaar.

Gemeten tijdens de eerste Pi 4-test:

- RAM totaal: 905 MiB.
- RAM gebruikt: 338 MiB.
- RAM beschikbaar: 567 MiB.
- Swap totaal: 904 MiB.
- Swap gebruikt: 0 B.
- Load average ongeveer `0.07, 0.10, 0.06`.

Conclusie: 1 GB is acceptabel voor weekendtest/proof-of-concept. Voor productie of langere pilots blijft 4 GB of 8 GB aanbevolen.

Monitoren:
```bash
docker stats
```

### 3. OS

- Gebruik **64-bit Raspberry Pi OS** waar mogelijk (betere ARM64 ondersteuning)
- .NET runtime hoeft niet op de Pi-host geïnstalleerd te worden voor Docker Compose; de runtime zit in de container images.
- Keep OS up-to-date: `sudo apt update && sudo apt upgrade`

### 4. Netwerk

- Gebruik Ethernet voor betrouwbaarheid (Pi 3 B+ wifi is kwetsbaar)
- Zorg dat sensor/simulator kan bereiken UDP 10110 op Pi's IP

### 5. Power

- Raspberry Pi 3 B+ vraagt 2.5A; zorg voor goede voeding
- Thermal throttling kan optreden; zet eventueel een koellichaam op

## Deployment Workflow (Pi-gereed Scenario)

1. **Prepare Pi:**
   ```bash
   # Op de Pi:
   curl -sSL https://get.docker.com | sh
   sudo usermod -aG docker $USER
   # Log out en in
   docker --version
   ```

2. **Clone code via GitHub SSH:**
   ```bash
   ssh-keygen -t ed25519 -C "bootmanager-pi"
   cat ~/.ssh/id_ed25519.pub
   # Voeg de public key toe aan GitHub.
   ssh -T git@github.com
   git clone git@github.com:rrvanleeuwen/BootManagerV2.git
   cd BootManagerV2
   ```

3. **Build images (local of via CI/CD push):**
   ```bash
   docker compose build
   ```

4. **Start services:**
   ```bash
   docker compose up -d
   ```

5. **Verify:**
   ```bash
   docker compose ps
   curl http://localhost:5000/health
   ```

6. **Monitor & Logs:**
   ```bash
   docker compose logs -f
   ```

## Gecontroleerde Database Reset (Testinstallatie)

Voor test- en helpdeskscenario's kun je de BootManager database veilig resetten zonder Docker Compose volumes of Git repo aan te raken.

### Wanneer gebruiken

- Testinstallatie terug naar eerste-start toestand zetten.
- Onboarding opnieuw doorlopen met schone data.
- Bestaande logboekgegevens verwijderen zonder volledige reinstallatie.
- Helpdesk-ondersteuning voor gebruikerstest herstarten.

### Wat gebeurt er

Het reset-script:
1. Stopt containers netjes (`docker compose stop`).
2. Maakt een timestamped backup van de huidige database.
3. Verwijdert de actieve SQLite database file.
4. Start containers opnieuw.
5. BootManager.Web past migraties toe en creëert bootstrap owner opnieuw.

### Wat wordt behouden

- `.env` configuratie
- Git repository status (`master` branch)
- Docker images en build cache
- Logboekbijlagen (`bootmanager-attachments` volume)
- Capture en applicatielogs (`bootmanager-logs` volume)
- Alle Docker volumes (alleen inhoud van `bootmanager-db` wordt vervangen)

### Procedure op Raspberry Pi

Op de Pi, via SSH of lokale shell:

```bash
# 1. Ga naar repo root
cd ~/BootManagerV2

# 2. Voer reset script uit
sudo bash scripts/reset-database.sh

# 3. Script vraagt bevestiging. Typ 'yes' om door te gaan.
# Wacht tot containers opnieuw zijn gestart en health check slaagt.
```

Het script moet met `sudo` worden gestart, omdat het direct werkt met Docker volume mountpoints onder `/var/lib/docker/volumes/`.

### Na reset

Controleer container status:
```bash
docker compose ps
```

Controleer gezondheidscheck:
```bash
curl -i http://localhost:5000/health
```

Verwacht antwoord:
```
HTTP/1.1 200 OK
{"status":"ok"}
```

Toegang tot applicatie:
```
http://localhost:5000  (lokaal op Pi)
http://<pi-hostname>:5000  (van ander apparaat)
```

Login:
1. Gebruik `BOOTMANAGER_BOOTSTRAP_PASSWORD` (uit `.env` file)
2. Je wordt doorgestuurd naar `/onboarding`
3. Vul eigenaar- en installatie-gegevens in
4. Kies een nieuw permanente wachtwoord
5. Na onboarding werkt bootstrap-wachtwoord niet meer

### Backed-up databases

Alle vorige databases worden bewaard met een timestamp. De locatie varieert afhankelijk van je Docker Compose project:

```bash
# Bepaal de werkelijke volume naam uit docker compose config
PROJECT_NAME=$(docker compose config 2>/dev/null | grep -m1 "name:" | sed 's/.*name: //' | xargs) || PROJECT_NAME=$(basename "$(pwd)" | tr '[:upper:]' '[:lower:]' | tr -cd 'a-z0-9')
VOLUME="${PROJECT_NAME}_bootmanager-db"

# Haal het volume mountpoint op
VOLUME_PATH=$(docker volume inspect "$VOLUME" -f '{{.Mountpoint}}' 2>/dev/null)

# Bekijk backup bestanden
ls -lh "$VOLUME_PATH"/bootmanager.db*
```

Backup-naamformaat: `bootmanager.db.backup.YYYYMMDD_HHMMSS`

### Waarschuwing

- Na reset zijn alle gebruikers, instellingen en logboekgegevens uit de database verwijderd.
- Dit kan niet ongedaan worden gemaakt via de UI.
- Backup files kunnen handmatig ter plekke worden gerehabiliteerd als noodprocedure, maar dat valt buiten standaard ondersteuning.

### Problemen

**Health check faalt na reset:**
```bash
# Controleer logs
docker compose logs bootmanager-web | tail -50

# Containers opnieuw starten
docker compose restart
```

**Database backup weg:**
- Backups bevinden zich in de Docker volume directory van het actieve Compose-project, bijvoorbeeld `/var/lib/docker/volumes/<project>_bootmanager-db/_data/`
- Bepaal de werkelijke locatie via `docker volume inspect "$VOLUME" -f '{{.Mountpoint}}'`
- Deze worden niet verwijderd door het reset script, alleen de actieve `bootmanager.db` file

**Onboarding loopt vast:**
- Controleer applicatie logs: `docker compose logs bootmanager-web`
- Controleer dat BOOTMANAGER_BOOTSTRAP_PASSWORD is ingesteld in `.env`
- Voer reset opnieuw uit

## Nog te onderzoeken

- Persist volumes gebruiken device mappings (advanced volume plugins)
- GPU-acceleration voor data processing (Pi 3 B+ heeft geen GPU)
- Load balancing tussen meerdere Pi's (toekomstig)
- Kubernetes deployment (later, for multi-node)

## Referenties

- Docker Compose docs: https://docs.docker.com/compose/
- Docker on Raspberry Pi: https://docs.docker.com/install/linux/docker-ce/debian/
- .NET 8 on ARM: https://dotnet.microsoft.com/download/dotnet/8.0
- SQLite volumes in Docker: https://www.sqlite.org/appfileformat.html

Zie ook: `.docs/raspberry-pi-deployment.md` voor systemd-gebaseerde deployments.
