# Docker Compose Deployment voor BootManager

Doel: BootManager als containerized applicatie op Raspberry Pi (of andere Linux-systemen) draaien met Docker Compose, zonder handmatige service-configuratie.

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
/var/lib/docker/volumes/bootmanager-db/_data/
/var/lib/docker/volumes/bootmanager-attachments/_data/
/var/lib/docker/volumes/bootmanager-logs/_data/
```

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

### 2. Geheugen

Pi 3 B+ heeft 1GB RAM. Docker Compose gebruikt ~50-100MB. Applicatie zelf:
- Web service: ~100-200MB (ASP.NET Core)
- Ingest: ~50MB (console app)

Total ~150-350MB, dus behapbaar.

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

2. **Clone/Upload code:**
   ```bash
   git clone https://github.com/rrvanleeuwen/BootManagerV2 bootmanager
   cd bootmanager
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
