# Raspberry Pi toegang en dataveiligheid

Doel: voorkomen dat Raspberry Pi-toegang kwijtraakt en voorkomen dat test- of velddata per ongeluk wordt gewist.

## SSH-toegang

De gevalideerde Pi-test gebruikte:

- Hostname: `bootmanager-pi`
- Gebruiker: `roelof`
- SSH-voorbeeld: `ssh roelof@bootmanager-pi.local`

Het Linux/SSH-wachtwoord van de Raspberry Pi hoort **niet** in GitHub te staan, ook niet in een private repository. Bewaar dit wachtwoord in een wachtwoordmanager of een andere persoonlijke veilige opslag.

Waarom:

- Het wachtwoord geeft toegang tot de Pi.
- Dezelfde gebruiker kan via `sudo` beheeracties uitvoeren.
- Een private repository kan later gedeeld, gekloond of per ongeluk openbaar gemaakt worden.

## Wachtwoord vergeten

Wis de SD-kaart niet automatisch wanneer het wachtwoord vergeten is. Controleer eerst of er nog waardevolle data op staat, bijvoorbeeld:

- echte boot-logdata;
- SQLite databasebestanden;
- Docker volumes;
- capture logs;
- configuratiebestanden zoals `.env`.

Als toegang nog lukt, maak eerst een backup. Als toegang niet meer lukt, probeer eerst de SD-kaart op een Linux-systeem uit te lezen voordat je opnieuw flasht.

## SD-kaart niet wissen zonder backup

Bij BootManager-tests kan waardevolle data op de SD-kaart staan. Een nieuwe flash wist deze data.

Belangrijke locaties bij de Docker Compose deployment:

```text
/var/lib/docker/volumes/*bootmanager-db*/_data/
/var/lib/docker/volumes/*bootmanager-logs*/_data/
/var/lib/docker/volumes/*bootmanager-attachments*/_data/
```

Gebruik `docker volume ls` om de exacte volumenamen te vinden.

## Aanbevolen backup zodra SSH weer werkt

Vanaf de Pi, in de repositorymap:

```bash
cd ~/BootManagerV2
mkdir -p ~/bootmanager-backups

docker compose exec bootmanager-web tar czf - /var/lib/bootmanager > ~/bootmanager-backups/bootmanager-db-and-data.tar.gz
docker compose exec bootmanager-web tar czf - /var/log/bootmanager > ~/bootmanager-backups/bootmanager-logs.tar.gz
```

Kopieer de backups daarna naar een andere machine, bijvoorbeeld vanaf de laptop:

```bash
scp roelof@bootmanager-pi.local:~/bootmanager-backups/*.tar.gz .
```

## Aanbevolen werkwijze na eerste installatie

1. Bewaar het Pi Linux/SSH-wachtwoord in een wachtwoordmanager.
2. Controleer dat SSH werkt.
3. Controleer dat GitHub SSH-toegang werkt.
4. Maak na een geslaagde boot- of veldtest een backup van database en logs.
5. Flash de SD-kaart pas opnieuw wanneer duidelijk is dat de data niet meer nodig is of veilig is gekopieerd.
