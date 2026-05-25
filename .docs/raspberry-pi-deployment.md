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

## Docker Compose Deployment (alternatief)

Voor eenvoudige containerisatie op de Pi kan Docker Compose gebruikt worden. Dit biedt:

- Reproduceerbare deployments zonder handmatige service-configuratie;
- eenvoudig volume management voor database, logs en bijlagen;
- service orchestration met health checks;
- network isolation;
- gemakkelijk schalen naar meerdere Pi's.

Zie `.docs/docker-deployment.md` voor volledige details.

Voordeel Docker Compose: sneller testen en reproduceerbaar.
Voordeel systemd services: directe lage-level controle, slanker resource-gebruik.

De voorkeur kan per use-case verschillen. Beide methoden zijn ondersteund.

## Bronnen

- Raspberry Pi headless setup en Imager: https://www.raspberrypi.com/documentation/computers/getting-started.html
- Raspberry Pi OS varianten en Lite: https://www.raspberrypi.com/documentation/usage/video/config_txt.html
- .NET 8 downloads met Linux Arm32/Arm64 binaries: https://dotnet.microsoft.com/en-US/download/dotnet/8.0
