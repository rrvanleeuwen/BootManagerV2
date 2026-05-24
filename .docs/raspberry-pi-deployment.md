# Raspberry Pi Deployment Plan

Doel: BootManager headless draaien op een Raspberry Pi, eerst zonder Docker. Docker kan later, maar voor de eerste deployment is een gewone Linux-service beter te begrijpen en te debuggen.

## Huidige uitgangssituatie

- Beschikbaar apparaat: Raspberry Pi 3 B+.
- Beschikbare SD-kaart: 8GB.
- Geen monitor of toetsenbord beschikbaar.
- Toegang moet dus headless via netwerk en SSH.
- De bestaande SD-kaart kan vervangen of opnieuw beschreven worden.

## Belangrijke keuze

Gebruik bij voorkeur een grotere microSD-kaart dan 8GB.

Raspberry Pi OS Lite kan klein draaien, maar 8GB is krap zodra het volgende samenkomt:
- besturingssysteem;
- .NET runtime;
- BootManager publish-output;
- SQLite database;
- logboekbijlagen;
- capture logs;
- systeemlogs en updates.

Praktisch advies: 32GB of groter, liefst een betrouwbare A1/A2 microSD-kaart.

## Aanbevolen eerste installatie

- OS: Raspberry Pi OS Lite, 64-bit als beschikbaar voor de Pi 3 B+.
- Geen desktopomgeving.
- SSH aanzetten tijdens het flashen met Raspberry Pi Imager.
- Ethernet gebruiken voor de eerste boot.
- Hostname: `bootmanager-pi`.
- Gebruiker: nog te kiezen tijdens installatie.

Waarom:
- Lite bespaart ruimte en geheugen.
- SSH is nodig omdat er geen scherm/toetsenbord is.
- Ethernet voorkomt wifi-problemen tijdens de eerste setup.
- 64-bit sluit het beste aan op `linux-arm64` deployments. Als 64-bit op de gekozen Pi/OS onpraktisch blijkt, vallen we terug naar `linux-arm`.

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

Voor de eerste deployment starten we waarschijnlijk eerst alleen `BootManager.Web`. Daarna voegen we `Ingest` toe als tweede service.

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

## Publish-strategie

Eerste voorkeur: framework-dependent publish.

Voor Web:

```powershell
dotnet publish BootManager.Web/BootManager.Web.csproj -c Release -r linux-arm64 --self-contained false -o .publish/raspi/web
```

Voor Ingest:

```powershell
dotnet publish src/BootManager.Tools.Ingest/BootManager.Tools.Ingest.csproj -c Release -r linux-arm64 --self-contained false -o .publish/raspi/ingest
```

Als we 32-bit Raspberry Pi OS gebruiken:

```powershell
dotnet publish BootManager.Web/BootManager.Web.csproj -c Release -r linux-arm --self-contained false -o .publish/raspi/web
dotnet publish src/BootManager.Tools.Ingest/BootManager.Tools.Ingest.csproj -c Release -r linux-arm --self-contained false -o .publish/raspi/ingest
```

Waarom niet meteen self-contained:
- self-contained neemt veel meer ruimte in op de SD-kaart;
- 8GB is krap;
- framework-dependent is overzichtelijker zolang we de .NET runtime op de Pi installeren.

## Services

Uiteindelijk willen we twee `systemd` services:

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

## Volgorde voor de echte installatiedag

1. Nieuwe microSD-kaart flashen met Raspberry Pi OS Lite.
2. SSH, hostname en gebruiker vooraf instellen in Raspberry Pi Imager.
3. Pi via ethernet aansluiten en booten.
4. IP-adres vinden in router of via hostname `bootmanager-pi.local`.
5. Eerste SSH-login testen.
6. Systeem updaten.
7. .NET runtime installeren of publish-strategie heroverwegen.
8. Mappen aanmaken onder `/opt`, `/var/lib`, `/var/log`.
9. Web publish-output overzetten.
10. Web config instellen.
11. Web handmatig starten.
12. Web UI testen vanaf laptop.
13. Web als `systemd` service installeren.
14. Daarna pas Ingest toevoegen en UDP-testen.

## Open vragen voor de volgende sessie

- Kopen/gebruiken we een grotere SD-kaart?
- Gaan we Raspberry Pi OS Lite 64-bit gebruiken?
- Welke hostname en Linux-gebruikersnaam wil je?
- Welke poort wil je voor de web UI?
- Moet BootManager alleen thuisnetwerk-bereikbaar zijn of later ook van buitenaf?
- Gaat Ingest op dezelfde Pi draaien als Web?
- Waar komen NMEA-data vandaan: echte boot-hardware, simulator, of later?

## Bronnen

- Raspberry Pi headless setup en Imager: https://www.raspberrypi.com/documentation/computers/getting-started.html
- Raspberry Pi OS varianten en Lite: https://www.raspberrypi.com/documentation/usage/video/config_txt.html
- .NET 8 downloads met Linux Arm32/Arm64 binaries: https://dotnet.microsoft.com/en-US/download/dotnet/8.0
