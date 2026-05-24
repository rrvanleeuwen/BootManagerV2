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
- Username: zelf kiezen
- Password: zelf kiezen
- Locale/timezone: Europe/Amsterdam
- Keyboard: niet kritisch voor headless, maar kies Nederlands/US naar voorkeur
- WiFi: optioneel; eerste setup liever via ethernet

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

Voer op de Pi uit:

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

## 7. Kleine Docker-test

Voer uit:

```bash
docker run --rm hello-world
```

Waarom:

- Dit test of Docker echt containers kan starten.

Controlepunt:

- Je ziet een “Hello from Docker!”-achtige melding.

## 8. BootManager code ophalen

Kies een werkmap:

```bash
mkdir -p ~/src
cd ~/src
git clone https://github.com/rrvanleeuwen/BootManagerV2.git
cd BootManagerV2
```

Waarom:

- De Pi bouwt de Docker images uit de repo.

Controlepunt:

- `ls` toont onder andere `docker-compose.yml`, `Dockerfile` en `Dockerfile.ingest`.

## 9. Secrets instellen

Maak een `.env`:

```bash
cp .env.example .env
nano .env
```

Vervang beide waarden:

```text
BOOTMANAGER_ENCRYPTION_KEY=...
BOOTMANAGER_JWT_KEY=...
```

Gebruik lange willekeurige strings. Voor een eerste test mag dit handmatig, bijvoorbeeld twee lange zinnen zonder spaties.

Waarom:

- BootManager heeft een encryptiesleutel en JWT signing key nodig.
- `.env` wordt niet gecommit.

Controlepunt:

- `cat .env` toont twee ingevulde waarden.

## 10. Docker Compose configuratie controleren

Voer uit:

```bash
docker compose config
```

Waarom:

- Dit valideert `docker-compose.yml` en `.env` zonder containers te starten.

Controlepunt:

- Geen foutmelding.

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

## 16. Wat we later pas doen

Niet in de eerste installatieronde:

- shutdown-knop in de webapp;
- HTTPS/certificaten;
- externe toegang vanaf internet;
- automatische backups;
- Docker image registry;
- echte NMEA hardware koppelen.

Die onderwerpen komen pas nadat de basis betrouwbaar draait.

## 17. Als iets misgaat

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
