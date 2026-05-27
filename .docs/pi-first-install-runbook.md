# Raspberry Pi First Install Runbook

Doel: BootManager voor het eerst headless installeren op een Raspberry Pi met Docker Compose.

Dit runbook is bewust stap-voor-stap. Voer de stappen niet als één groot blok uit. Stop na elk controlepunt.

## 0. Benodigdheden

Regel vooraf:

- Raspberry Pi, bij voorkeur Pi 4 of nieuwer. Pi 3 B+ kan, maar is krapper.
- MicroSD-kaart van minimaal 32GB, bij voorkeur A1/A2.
- MicroSD naar SD/USB adapter voor de Windows-pc.
- Ethernetkabel.
- Stabiele Raspberry Pi voeding.
- Windows-pc met internet.

Installeer op Windows:

- Raspberry Pi Imager: https://www.raspberrypi.com/software/
- Git: https://git-scm.com/download/win
- Een SSH-client. Windows PowerShell heeft meestal `ssh` ingebouwd.

Controlepunt:

- SD-kaart kan in de pc.
- Raspberry Pi Imager start.
- Ethernetpoort op router is beschikbaar.

## Gevalideerde installatie

Dit runbook is op 2026-05-26 succesvol doorlopen op:

- Raspberry Pi 4 Model B.
- 32 GB microSD.
- Raspberry Pi OS Lite 64-bit.
- Hostname: `bootmanager-pi`.
- User: `roelof`.
- SSH vanaf prive-laptop.
- GitHub private repo via SSH-key op de Pi.
- Docker images lokaal gebouwd op de Pi.
- Web en Ingest gestart via Docker Compose.
- Reboot-test geslaagd.

De gemeten Pi leek een 1 GB-model. Dat is voldoende voor een weekendtest/proof-of-concept, maar voor productie of langere pilots blijft 4 GB of 8 GB aanbevolen.

## 1. SD-kaart flashen

Open Raspberry Pi Imager.

Kies:

- Device: jouw Raspberry Pi model.
- OS: Raspberry Pi OS Lite, bij voorkeur 64-bit.
- Storage: de microSD-kaart.

Open de advanced/customisation settings.

Stel in:

- Hostname: `bootmanager-pi`
- SSH: enabled
- Username: zelf kiezen; eerste test gebruikte `roelof`
- Password: zelf kiezen
- Locale/timezone: Europe/Amsterdam
- Keyboard: niet kritisch voor headless, maar kies Nederlands/US naar voorkeur
- WiFi: instellen als ethernet niet direct beschikbaar is; eerste boot via ethernet blijft betrouwbaarder

Schrijf de SD-kaart.

Waarom:

- Lite is klein en heeft geen desktop nodig.
- SSH is verplicht omdat we geen monitor/toetsenbord gebruiken.
- Hostname maakt later verbinden makkelijker.

Controlepunt:

- Imager is klaar zonder foutmelding.
- SD-kaart is veilig verwijderd.

## 2. Eerste boot

Plaats de SD-kaart in de Pi.

Sluit aan:

- Ethernet naar router.
- Voeding.

Wacht 2-5 minuten.

Waarom:

- De eerste boot kan wat langer duren omdat Raspberry Pi OS initialisatie uitvoert.

Controlepunt:

- Pi heeft stroom.
- Ethernetlampjes op Pi/router knipperen.

## 3. IP-adres vinden

Probeer vanaf Windows PowerShell:

```powershell
ssh <gebruikersnaam>@bootmanager-pi.local
```

Als dat niet werkt:

- open de router webinterface;
- zoek bij verbonden apparaten naar `bootmanager-pi`;
- noteer het IP-adres;
- probeer:

```powershell
ssh <gebruikersnaam>@<ip-adres>
```

Waarom:

- SSH is onze afstandsbediening voor de Pi.
- `.local` werkt alleen als mDNS goed werkt; het IP-adres via de router is de fallback.

Controlepunt:

- Je ziet een login prompt of bent ingelogd op de Pi.

## 4. Eerste SSH-login

Na login voer je één commando uit:

```bash
uname -a
```

Daarna:

```bash
cat /etc/os-release
```

Waarom:

- We controleren Linux-versie en architectuur voordat we Docker installeren.

Controlepunt:

- Output is zichtbaar.
- Noteer of het systeem 64-bit lijkt (`aarch64`) of 32-bit (`armv7l`).
- Eerste Pi 4-test: `uname -m` gaf `aarch64`.

## 5. Systeem bijwerken

Voer uit op de Pi:

```bash
sudo apt update
sudo apt upgrade -y
sudo reboot
```

Wacht na reboot weer 1-2 minuten en log opnieuw in met SSH.

Waarom:

- Nieuwe installaties hebben vaak security- en pakketupdates nodig.

Controlepunt:

- SSH werkt opnieuw na reboot.

## 6. Docker installeren

Gebruik bij voorkeur de officiele Docker Debian repository. De korte installatieroute is:

```bash
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER
```

Log daarna uit:

```bash
exit
```

Log opnieuw in via SSH.

Controleer:

```bash
docker --version
docker compose version
```

Waarom:

- Docker draait de BootManager containers.
- Opnieuw inloggen is nodig zodat je gebruiker lid is van de `docker` groep.

Controlepunt:

- Beide Docker-commando's geven een versie terug.
- Eerste Pi 4-test:
  - `Docker version 29.5.2`
  - `Docker Compose version v5.1.4`
- `docker ps` werkt zonder `sudo`.

## 7. Kleine Docker-test

Voer uit:

```bash
docker run --rm hello-world
```

Waarom:

- Dit test of Docker echt containers kan starten.

Controlepunt:

- Je ziet een “Hello from Docker!”-achtige melding.

## 8. GitHub SSH-key en BootManager code ophalen

Maak op de Pi een SSH-key voor GitHub:

```bash
ssh-keygen -t ed25519 -C "bootmanager-pi"
cat ~/.ssh/id_ed25519.pub
```

Voeg de public key toe aan GitHub. Voeg nooit de private key toe aan GitHub, documentatie of chat.

Test GitHub SSH:

```bash
ssh -T git@github.com
```

Clone daarna de private repo:

```bash
cd ~
git clone git@github.com:rrvanleeuwen/BootManagerV2.git
cd BootManagerV2
```

Waarom:

- De Pi bouwt de Docker images uit de repo.
- GitHub `master` blijft leidend.
- De Pi hoort geen lokale afwijkingen te bevatten; update altijd via `git pull`.

Controlepunt:

- `ls` toont onder andere `docker-compose.yml`, `Dockerfile` en `Dockerfile.ingest`.
- `git status --short --branch` toont `master` zonder wijzigingen.

## 9. Secrets instellen

Maak een `.env`:

```bash
cp .env.example .env
nano .env
```

Vervang alle waarden:

```text
BOOTMANAGER_ENCRYPTION_KEY=...
BOOTMANAGER_JWT_KEY=...
BOOTMANAGER_BOOTSTRAP_PASSWORD=...
```

Gebruik lange willekeurige strings voor de encryptie- en JWT-sleutel. Kies voor `BOOTMANAGER_BOOTSTRAP_PASSWORD` een tijdelijk eerste-login-wachtwoord. Dit wachtwoord wordt gebruikt voor de bootstrap owner bij een lege database en is na onboarding niet meer geldig.

Waarom:

- BootManager heeft een encryptiesleutel en JWT signing key nodig.
- Een lege productie-installatie heeft expliciet een bootstrap-wachtwoord nodig.
- `.env` wordt niet gecommit.
- Geheime waarden horen niet in GitHub.

Controlepunt:

- `cat .env` toont drie ingevulde waarden.

## 10. Docker Compose configuratie controleren

Voer uit:

```bash
docker compose config --services
```

Waarom:

- Dit valideert `docker-compose.yml` en `.env` zonder containers te starten.

Controlepunt:

- Geen foutmelding.
- De services zijn:
  - `bootmanager-web`
  - `bootmanager-ingest`

## 11. Images bouwen

Voer uit:

```bash
docker compose build
```

Waarom:

- Docker bouwt de Web- en Ingest-images voor de Pi.

Let op:

- Op een Pi kan dit lang duren.
- Bij een Pi 3 B+ kan dit traag zijn.

Controlepunt:

- Build eindigt zonder foutmelding.

Bekende build-valkuilen:

- .NET base images gebruiken multi-arch tags zoals `8.0-jammy`; gebruik geen niet-bestaande `8.0-jammy-arm64` tags.
- Een tijdelijke fout `Could not resolve 'ports.ubuntu.com'` kan verdwijnen na opnieuw bouwen als host- en Docker-DNS werken.
- DNS-check:
  ```bash
  docker run --rm alpine nslookup ports.ubuntu.com
  docker run --rm alpine ping -c 3 ports.ubuntu.com
  ```

## 12. Containers starten

Voer uit:

```bash
docker compose up -d
docker compose ps
```

Waarom:

- `up -d` start op de achtergrond.
- `ps` toont status en health.

Controlepunt:

- `bootmanager-web` draait.
- `bootmanager-ingest` draait of wacht logisch op Web health.
- Eerste Pi 4-test:
  - `bootmanager-web`: `Up` / `healthy`, poort `5000/tcp`.
  - `bootmanager-ingest`: `Up`, UDP `10110/udp`, control API `127.0.0.1:5010->5010/tcp`.

Controleer health:

```bash
curl -i http://localhost:5000/health
```

Verwacht:

```text
HTTP/1.1 200 OK
{"status":"ok"}
```

## 13. Web UI testen

Vanaf Windows browser:

```text
http://bootmanager-pi.local:5000
```

Als dat niet werkt:

```text
http://<ip-adres>:5000
```

Waarom:

- Dit bewijst dat Web bereikbaar is vanaf het netwerk.

Controlepunt:

- BootManager webpagina opent.
- Eerste Pi 4-test: app was bereikbaar via `http://192.168.2.29:5000`.

## 13a. Eerste login en onboarding

Open de BootManager webpagina.

Bij een lege database verwacht je:

1. De app maakt automatisch een bootstrap owner aan.
2. Je komt op `/login`.
3. Log in met de waarde van `BOOTMANAGER_BOOTSTRAP_PASSWORD` uit `.env`.
4. Je wordt verplicht naar `/onboarding` gestuurd.
5. Vul eigenaargegevens, bootgegevens en een nieuw wachtwoord in.
6. Na opslaan kom je op `/dashboard`.
7. Het bootstrap-wachtwoord werkt daarna niet meer; het nieuwe wachtwoord wel.

Waarom:

- Dit bevestigt dat de eerste-start setup compleet is.
- De bootgegevens worden opgeslagen in het singleton `VesselProfile`.
- De setup flags worden afgerond zodat de rest van de app bereikbaar is.

Controlepunt:

- `/dashboard` opent na onboarding.
- Handmatig openen van `/onboarding` stuurt terug naar `/dashboard`.

## 14. Logs bekijken

Op de Pi:

```bash
docker compose logs bootmanager-web
docker compose logs bootmanager-ingest
```

Live volgen:

```bash
docker compose logs -f
```

Waarom:

- Logs zijn de eerste plek om fouten te vinden.

Controlepunt:

- Geen herhalende startup errors.

## 14a. Updateprocedure

Update de Pi niet automatisch na iedere GitHub-push. Bij documentatie-only wijzigingen is dat meestal niet nodig. Als een Pi-update nodig is, hoort Codex precies te zeggen welke commando's je in de SSH-sessie moet uitvoeren en of een rebuild nodig is.

De Pi pullt alleen `master`, nooit feature-branches. Ontwikkeling en pre-PR validatie gebeuren op de ontwikkelcomputer of lokale devomgeving. Pas nadat een wijziging naar `origin/master` is gemerged/gepusht en Codex expliciet zegt dat de Pi moet worden bijgewerkt, voer je onderstaande updateprocedure op de Pi uit.

Vuistregels:

- Alleen documentatie op `master`: geen Pi-pull nodig.
- Code, Dockerfile, dependencies of Compose-config op `master`: pull en meestal opnieuw bouwen.
- Alleen `.env` of runtimeconfig gewijzigd: geen imagebuild, wel Compose opnieuw toepassen.
- Alleen proces herstarten: `docker compose restart`.

Bij nieuwe code op `master` waarvoor Codex expliciet aangeeft dat de Pi moet worden bijgewerkt:

```bash
cd ~/BootManagerV2
git pull
docker compose build
docker compose up -d
docker compose ps
curl -i http://localhost:5000/health
```

Alleen herstarten zonder nieuwe code:

```bash
cd ~/BootManagerV2
docker compose restart
docker compose ps
```

## 15. Stoppen en herstarten

Stoppen:

```bash
docker compose stop
```

Starten:

```bash
docker compose up -d
```

Niet doen tenzij je bewust alle data wilt verwijderen:

```bash
docker compose down -v
```

Waarom:

- `down -v` verwijdert volumes en dus mogelijk database/bijlagen/logs.

Reboot-test:

```bash
sudo reboot
```

Na opnieuw inloggen:

```bash
cd ~/BootManagerV2
docker compose ps
curl -i http://localhost:5000/health
```

Controlepunt:

- Beide containers komen automatisch terug.
- `bootmanager-web` blijft `healthy`.
- `bootmanager-ingest` blijft `Up`.
- `/health` geeft opnieuw `200 OK`.

## 16. Boot/YDEN test

De YDEN stuurt UDP broadcast naar een poort. Meestal hoeft de YDEN dus geen vast Pi-IP ingesteld te krijgen.

Voorwaarden:

- YDEN en Raspberry Pi zitten in hetzelfde LAN/subnet.
- UDP-poort komt overeen met de Ingest listener, standaard `10110`.
- Broadcast gaat normaal niet over router-, VLAN- of gastnetwerkgrenzen.

Checklist:

```bash
hostname -I
cd ~/BootManagerV2
docker compose ps
docker compose logs -f bootmanager-ingest
```

In een tweede SSH-sessie:

```bash
sudo apt install -y tcpdump
sudo tcpdump -i any udp port 10110
```

Interpretatie:

- `tcpdump` toont pakketten, maar BootManager verwerkt niets: kijk naar Ingest/configuratie/parser.
- `tcpdump` toont niets: kijk naar netwerk/YDEN/Teltonika/subnet/broadcast.

## 17. Resourcechecks

Na eerste start:

```bash
df -h
docker system df
free -h
uptime
```

Eerste Pi 4-test:

- Root filesystem 29 GB, 6.0 GB gebruikt, 22 GB beschikbaar.
- Docker images 2.302 GB, build cache 2.58 GB.
- RAM totaal 905 MiB, 338 MiB gebruikt, 567 MiB beschikbaar.
- Swap 904 MiB, 0 B gebruikt.
- Load average ongeveer `0.07, 0.10, 0.06`.

Conclusie:

- 32 GB SD is voldoende voor weekendtest/proof-of-concept.
- Voor productie liever Compute Module/industrial Pi, eMMC/NVMe/SSD en 4 GB of 8 GB RAM.

## 18. Wat we later pas doen

Niet in de eerste installatieronde:

- shutdown-knop in de webapp;
- HTTPS/certificaten;
- externe toegang vanaf internet;
- automatische backups;
- Docker image registry;
- echte NMEA hardware koppelen.

## 18a. Gecontroleerde Database Reset

Er is geen normale pincode-, recovery- of master-key flow meer in de UI.

### Scenario's voor database reset

- Testinstallatie opnieuw doorlopen met schone database.
- Vergeten wachtwoord: enige gebruiker kan niet meer inloggen.
- Onboarding opnieuw starten na ontwikkelingen.
- Helpdesk-ondersteuning: testgebruiker herstarten.

### Automatische reset procedure

Gebruik het gecontroleerde reset-script:

```bash
cd ~/BootManagerV2
bash scripts/reset-database.sh
```

Het script:
1. Vraagt bevestiging (typ 'yes' om door te gaan).
2. Stopt containers netjes.
3. Maakt timestamped backup van huidige database.
4. Verwijdert actieve database file.
5. Start containers opnieuw.
6. Wacht tot health check OK is.

Na reset:

1. Log in met `BOOTMANAGER_BOOTSTRAP_PASSWORD` (uit `.env`).
2. Doorloop onboarding en kies nieuw wachtwoord.
3. Bootstrap-wachtwoord werkt daarna niet meer.
4. Normale login met het nieuwe wachtwoord wordt actief.

### Wat wordt behouden

- `.env` configuratie
- Git repository
- Logboekbijlagen
- Capture- en applicatielogs
- Docker volumes (behalve database inhoud)

### Backup-locatie

Vorige databases worden timestamped bewaard. De locatie varieert afhankelijk van je Docker Compose project:

```bash
# Bepaal de werkelijke volume naam uit docker compose config
PROJECT_NAME=$(docker compose config 2>/dev/null | grep -m1 "name:" | sed 's/.*name: //' | xargs) || PROJECT_NAME=$(basename "$(pwd)" | tr '[:upper:]' '[:lower:]' | tr -cd 'a-z0-9')
VOLUME="${PROJECT_NAME}_bootmanager-db"

# Haal het volume mountpoint op
VOLUME_PATH=$(docker volume inspect "$VOLUME" -f '{{.Mountpoint}}' 2>/dev/null)

# Bekijk backup bestanden
ls -lh "$VOLUME_PATH"/bootmanager.db*
```

Meer informatie: zie `.docs/docker-deployment.md` sectie "Gecontroleerde Database Reset".

Let op:

- Dit is een operationele factory-reset van de actieve database.
- Database inhoud en gebruikersgegevens gaan verloren.
- Backups blijven beschikbaar voor noodgebruik.
- Bootgegevens wijzigen na onboarding is later een aparte story.

Die onderwerpen komen pas nadat de basis betrouwbaar draait.

## 19. Als iets misgaat

Stop en verzamel:

```bash
docker compose ps
docker compose logs --tail 100
df -h
free -h
uname -a
cat /etc/os-release
```

Waarom:

- Hiermee kunnen we gericht zien of het probleem Docker, opslag, geheugen, architectuur of applicatie-startup is.

## Gerelateerde documentatie

- `.docs/raspberry-pi-deployment.md`
- `.docs/docker-deployment.md`
