# Raspberry Pi Deployment Plan

Doel: BootManager headless draaien op een Raspberry Pi. De eerste geslaagde deployment gebruikt Docker Compose; systemd blijft een latere optie voor situaties waar directe hostservices gewenst zijn.

## Huidige uitgangssituatie

- Eerste geslaagde test: Raspberry Pi 4 Model B met 32 GB microSD.
- OS: Raspberry Pi OS Lite 64-bit.
- Hostname: `bootmanager-pi`.
- Gebruiker tijdens test: `roelof`.
- Toegang headless via SSH vanaf prive-laptop.
- GitHub private repo via SSH-key op de Pi.
- Docker images lokaal gebouwd op de Pi.
- GitHub `master` blijft leidend; de Pi hoort een schone afspiegeling van `origin/master` te zijn.

## Belangrijke keuze

Gebruik bij voorkeur minimaal een 32 GB microSD-kaart voor tests. Voor langdurige logging of productie is SD-opslag niet ideaal.

Raspberry Pi OS Lite kan klein draaien, maar opslag loopt op zodra het volgende samenkomt:
- besturingssysteem;
- Docker images en build cache;
- SQLite database;
- logboekbijlagen;
- capture logs;
- systeemlogs en updates.

Praktisch advies:

- Weekendtest/proof-of-concept: 32 GB microSD is voldoende.
- Langere pilot/productie: Compute Module/industrial Pi met eMMC, NVMe of SSD.
- RAM: 1 GB werkt voor de basistest, maar 4 GB of 8 GB blijft aanbevolen.

Gemeten tijdens de eerste Pi 4-test:

- Root filesystem 29 GB, 6.0 GB gebruikt, 22 GB beschikbaar.
- Docker images 2.302 GB.
- Docker build cache 2.58 GB, waarvan 2.234 GB reclaimable.
- RAM totaal 905 MiB, 338 MiB gebruikt, 567 MiB beschikbaar.
- Swap 904 MiB, 0 B gebruikt.
- Load average ongeveer `0.07, 0.10, 0.06`.

## Aanbevolen eerste installatie

- OS: Raspberry Pi OS Lite 64-bit.
- Geen desktopomgeving.
- SSH aanzetten tijdens het flashen met Raspberry Pi Imager.
- Ethernet gebruiken voor de eerste boot waar mogelijk; Wi-Fi kan voor thuisnetwerk worden ingesteld.
- Hostname: `bootmanager-pi`.
- Gebruiker: nog te kiezen tijdens installatie.

Waarom:
- Lite bespaart ruimte en geheugen.
- SSH is nodig omdat er geen scherm/toetsenbord is.
- Ethernet voorkomt wifi-problemen tijdens de eerste setup.
- 64-bit sluit aan op ARM64 Docker builds. De .NET Docker base images gebruiken multi-architecture tags; Docker kiest op de Pi automatisch ARM64.

## BootManager runtime-model

BootManager bestaat uit minimaal twee processen:

1. `BootManager.Web`
   - ASP.NET Core webapp.
   - Gebruikt SQLite via `BootManager.Web/bootmanager.db` als default.
   - Beheert logboek, instellingen, API en UI.

2. `BootManager.Tools.Ingest`
   - Console/worker process.
   - Luistert standaard op UDP `10110`.
   - Stuurt data naar de Web API.
   - Heeft een control API op `127.0.0.1:5010`.

De eerste Docker Compose deployment startte Web en Ingest samen. `bootmanager-ingest` wacht daarbij op de healthcheck van `bootmanager-web`.

Geteste poorten:

- Web UI/API: hostpoort `5000/tcp`.
- Ingest UDP: hostpoort `10110/udp`.
- Ingest control API: `127.0.0.1:5010->5010/tcp`, lokaal op de host gebonden.

Healthcheck:

```bash
curl -i http://localhost:5000/health
```

Verwacht:

```text
HTTP/1.1 200 OK
{"status":"ok"}
```

## Eerste-start flow

Op een lege Pi/database werkt BootManager als volgt:

1. `BootManager.Web` past database-migraties toe.
2. Als er geen `OwnerProfile` bestaat, wordt een bootstrap owner aangemaakt.
3. De eerste login gebruikt `Bootstrap:DefaultPassword`.
4. Een ingelogde bootstrap owner wordt verplicht naar `/onboarding` gestuurd.
5. In onboarding worden eigenaargegevens, bootgegevens en een nieuw wachtwoord ingevuld.
6. Na succesvolle onboarding zijn `PasswordChangeRequired=false` en `OnboardingCompleted=true`.
7. Het dashboard en de rest van de applicatie zijn daarna bereikbaar.

De normale gebruikersflow gebruikt geen pincode, recovery-code of master-key UI. Het bootstrap-wachtwoord is alleen bedoeld voor eerste installatie en wordt na onboarding vervangen door het gekozen nieuwe wachtwoord.

## Persistente paden op de Pi

Aanbevolen layout:

```text
/opt/bootmanager/web/              # gepubliceerde Web-app
/opt/bootmanager/ingest/           # gepubliceerde Ingest-tool
/var/lib/bootmanager/bootmanager.db
/var/lib/bootmanager/logbook-attachments/
/var/log/bootmanager/
```

Waarom:
- `/opt` is gebruikelijk voor applicatiebestanden.
- `/var/lib` is bedoeld voor persistente applicatiedata.
- `/var/log` is bedoeld voor logs.
- Bij updates kunnen we `/opt/bootmanager/...` vervangen zonder database/bijlagen kwijt te raken.

## Configuratie die aangepast moet worden

### Web

Default nu:

```json
"ConnectionStrings": {
  "Default": "Data Source=bootmanager.db"
}
```

Productievoorstel:

```json
"ConnectionStrings": {
  "Default": "Data Source=/var/lib/bootmanager/bootmanager.db"
}
```

Daarnaast moet `Encryption:Key` niet op `CHANGE_THIS_PRODUCTION_KEY` blijven staan.

Voor production moet ook een bootstrap-wachtwoord expliciet ingesteld worden:

```text
Bootstrap:DefaultPassword=<tijdelijk-eerste-login-wachtwoord>
```

Bij Docker Compose gebeurt dit via `.env`:

```text
BOOTMANAGER_BOOTSTRAP_PASSWORD=replace-with-first-login-password
```

Bij systemd kan dit bijvoorbeeld als environment variable in de service worden gezet:

```ini
Environment=Bootstrap__DefaultPassword=replace-with-first-login-password
```

Als de database leeg is en dit wachtwoord ontbreekt, hoort `BootManager.Web` in production niet door te starten. Dat is bewust: een lege productie-installatie moet altijd een expliciet eerste-login-wachtwoord hebben.

### Ingest

Default nu:

```json
"ApiBaseUrl": "http://localhost:5046",
"ListenPort": 10110,
"ControlApi": {
  "ListenAddress": "127.0.0.1",
  "ListenPort": 5010
}
```

Productievoorstel:

```json
"ApiBaseUrl": "http://127.0.0.1:5000",
"ListenPort": 10110,
"ControlApi": {
  "ListenAddress": "127.0.0.1",
  "ListenPort": 5010
}
```

## Docker Compose strategie

De gevalideerde route is Docker Compose vanaf een clone van de GitHub repo op de Pi.

Installatie/update:

```bash
cd ~/BootManagerV2
git pull
docker compose build
docker compose up -d
docker compose ps
curl -i http://localhost:5000/health
```

Belangrijke keuzes:

- Geen zip-workflow.
- Geen werklaptop nodig na clone; de Pi bouwt images lokaal.
- Geen lokale afwijkingen op de Pi; wijzigingen gaan via `master`.
- `.env` blijft lokaal per apparaat en wordt niet gecommit.

Bekende fixes uit eerste test:

- Commit `124c7af`: .NET Docker base images gebruiken multi-arch tags zonder niet-bestaande `-arm64` suffix.
- Commit `4ef3d73`: `IngestControlServer` vertaalt `0.0.0.0` naar HttpListener-prefix `http://*:5010/`.
- Tijdelijke DNS-fout `Could not resolve 'ports.ubuntu.com'` verdween na opnieuw bouwen nadat host- en Docker-DNS werkten.

## Publish-strategie zonder Docker

Framework-dependent publish naar systemd services is niet de eerste gevalideerde route meer, maar blijft mogelijk voor latere low-level deployments. Zie eerdere publish-commando's in gitgeschiedenis als deze route opnieuw nodig wordt.

## Services

Als Docker Compose niet passend blijkt, kunnen we later twee `systemd` services maken:

- `bootmanager-web.service`
- `bootmanager-ingest.service`

Voordeel:
- start automatisch na reboot;
- logs via `journalctl`;
- stoppen/starten met `systemctl`;
- duidelijke scheiding tussen webapp en ingest.

## Netwerkpoorten

- Web UI/API: voorstel `5000` intern op de Pi.
- Ingest UDP: `10110`.
- Ingest control API: `5010`, alleen localhost.

Als de UI vanaf andere apparaten bereikbaar moet zijn, moet de webapp luisteren op `0.0.0.0:5000`, niet alleen op `localhost`.

## Volgorde voor installatiedag met Docker Compose

1. Nieuwe microSD-kaart flashen met Raspberry Pi OS Lite.
2. SSH, hostname en gebruiker vooraf instellen in Raspberry Pi Imager.
3. Pi via ethernet of voorbereid Wi-Fi aansluiten en booten.
4. IP-adres vinden in router of via hostname `bootmanager-pi.local`.
5. Eerste SSH-login testen.
6. Systeem updaten.
7. Docker installeren via de officiele Debian repository of `get.docker.com`.
8. User toevoegen aan de `docker` groep en opnieuw inloggen.
9. GitHub SSH-key aanmaken en public key aan GitHub toevoegen.
10. Repo clonen via `git@github.com:rrvanleeuwen/BootManagerV2.git`.
11. Lokale `.env` maken met `BOOTMANAGER_ENCRYPTION_KEY`, `BOOTMANAGER_JWT_KEY` en `BOOTMANAGER_BOOTSTRAP_PASSWORD`.
12. `docker compose config --services` controleren.
13. `docker compose build`.
14. `docker compose up -d`.
15. `docker compose ps` en `/health` controleren.
16. Web UI testen vanaf laptop via `http://<pi-ip>:5000`.
17. Reboot-test uitvoeren en containers/health opnieuw controleren.

## Open vragen voor de volgende sessie

- Boot-test met YDEN UDP broadcast op het bootnetwerk/Teltonika.
- Definitieve hardwarekeuze voor productie/pilot: SD versus eMMC/NVMe/SSD, 4 GB/8 GB RAM.
- Backup/restore-procedure voor database, bijlagen en logs.
- Veilige shutdown-flow vanuit UI/helper-service.
- Monitoring van opslag, RAM en containerstatus in de applicatie.

## Reset bij vergeten wachtwoord

Voor deze epic is er geen in-app recovery. Als de enige gebruiker niet meer kan inloggen:

1. Zorg voor fysieke/admin toegang tot de Pi.
2. Stop `BootManager.Web` en `BootManager.Tools.Ingest` of de Docker containers.
3. Maak een backup van `/var/lib/bootmanager/bootmanager.db` of van het Docker volume.
4. Hernoem of verwijder daarna pas de actieve SQLite database.
5. Controleer dat `Bootstrap:DefaultPassword` opnieuw is ingesteld.
6. Start BootManager opnieuw.
7. Doorloop opnieuw de bootstrap login en onboarding.

Deze reset wist de actieve applicatie-state in de database. Bewaar backups zolang je oude logboek- of meetdata nog nodig kunt hebben. Bootgegevens wijzigen na onboarding is later een aparte story.

## Docker Compose Deployment

Docker Compose is de eerste gevalideerde deploymentroute op de Pi. Dit biedt:

- Reproduceerbare deployments zonder handmatige service-configuratie;
- eenvoudig volume management voor database, logs en bijlagen;
- service orchestration met health checks;
- network isolation;
- gemakkelijk schalen naar meerdere Pi's.

Zie `.docs/docker-deployment.md` en `.docs/pi-first-install-runbook.md` voor volledige details.

Voordeel Docker Compose: reproduceerbaar, getest op ARM64 en geschikt voor pull/build/up workflows op de Pi.
Voordeel systemd services: directe lage-level controle en mogelijk slanker resource-gebruik.

De voorkeur kan per use-case verschillen. Beide methoden zijn ondersteund.

## Bronnen

- Raspberry Pi headless setup en Imager: https://www.raspberrypi.com/documentation/computers/getting-started.html
- Raspberry Pi OS varianten en Lite: https://www.raspberrypi.com/documentation/usage/video/config_txt.html
- .NET 8 downloads met Linux Arm32/Arm64 binaries: https://dotnet.microsoft.com/en-US/download/dotnet/8.0
