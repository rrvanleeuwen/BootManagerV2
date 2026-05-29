# Epic: Dashboard & Live Overzicht

Status: voorgesteld op 2026-05-29 na de eerste geslaagde Pi-veldtest met echte bootdata.

## Aanleiding

BootManager heeft nu aantoonbaar een werkende keten voor echte bootdata op de Raspberry Pi:

- echte boordnetwerkdata komt binnen via de gateway;
- `bootmanager-ingest` post naar `bootmanager-web`;
- raw `NetworkMessages` en meerdere measurement-tabellen worden gevuld;
- logboek met live data is handmatig groen bevonden.

De volgende gebruikersstap is niet opnieuw technische ingest-validatie, maar bruikbare presentatie in de webinterface.

De gebruiker wil tijdens varen of testen direct in de UI kunnen zien wat BootManager op dat moment als actuele waarden kent, zonder eerst SSH, Docker logs of database-inspectie nodig te hebben.

## Legacy-koppeling

Deze epic sluit direct aan op bestaande legacy-scope:

- `US7.1 Dashboardweergave openen`
- `US7.2 Actieve bootinformatie`
- `US7.3 Waarschuwingen en meldingen`
- `US7.9 Widget voor logboekactiviteit`
- `US7.13 Automatische update van gegevens`

Daarnaast ondersteunt dit indirect:

- `US5.7 Logregels met nautische velden`, omdat live zicht op waarden helpt bij logboekvalidatie;
- `US9.5 Sensorintegratie via Bluetooth of Wi-Fi`, voor zover de UI moet tonen wat uit sensorketens binnenkomt.

## Doel

BootManager krijgt een bruikbaar live dashboard voor de single-boat installatie:

- actuele gemeten waarden tonen;
- status van dataherkomst en actualiteit zichtbaar maken;
- duidelijke scheiding houden tussen gebruikersdashboard en technische analysepagina.

## Scopegrens

Deze epic is voor eindgebruikersweergave, niet voor diepe technische diagnostiek.

Technische operatorvragen zoals:

- welke ruwe berichten kwamen exact binnen;
- welke warnings/errors traden op;
- welke records zitten in de database;
- export/download van analyse-output;

horen primair thuis in `.docs/epics/system-operations.md`.

## User Stories

### DSH-LIVE-1: Live dashboard met actuele meetwaarden

**Status:** Voorgesteld op 2026-05-29.

**User Story:** Als gebruiker wil ik op het dashboard actuele bootmetingen kunnen zien in meters of duidelijke tekstvelden, zodat ik zonder technische hulpmiddelen direct inzicht heb in wat BootManager nu meet.

**Scope:**

- Een dashboardpagina of dashboardsectie tonen met actuele waarden uit bestaande measurement-tabellen.
- Meters gebruiken waar dat visueel logisch is, bijvoorbeeld voor:
  - windsnelheid;
  - windhoek;
  - heading of koers;
  - diepte.
- Tekst- of statusvelden gebruiken waar dat logischer is, bijvoorbeeld voor:
  - positie;
  - GPS-status/fixstatus;
  - laatste update-tijd;
  - bron/protocol indien relevant.
- Minimaal rekening houden met de meettypen die nu op de Pi met echte data al zijn bevestigd:
  - wind;
  - heading;
  - speed through water;
  - position;
  - motion/COG/SOG;
  - watertemperatuur;
  - diepte zodra live beschikbaar in de testcontext.

**Buiten scope:**

- Geen volledige technische analysepagina.
- Geen brede historische grafieken of trendanalyse.
- Geen kaartweergave of routekaart in deze eerste slice.
- Geen multi-boat of multi-user dashboardpersonalisatie.

**Acceptatiecriteria:**

- Dashboard toont actuele waarden voor de belangrijkste beschikbare meettypen.
- Voor elk veld is duidelijk of het een actuele waarde is of dat data ontbreekt.
- Dashboard crasht niet als een meettype tijdelijk niet beschikbaar is.
- Meters en tekstvelden voelen bewust gekozen aan en niet willekeurig.
- Handmatige Pi-test met live data toont plausibele waarden in de UI.

**Handmatige testnotities:**

- Uitvoeren tegen de Pi-installatie met live bootdata of een representatieve lokale runtime-test.
- Vergelijken met recente logboekwaarden, live meetdata en waar mogelijk boordinstrumenten.

---

### DSH-LIVE-2: Actualiteit en datastatus zichtbaar maken

**Status:** Voorgesteld op 2026-05-29.

**User Story:** Als gebruiker wil ik op het dashboard kunnen zien hoe actueel de getoonde meetwaarden zijn, zodat ik weet of ik naar live data kijk of naar oudere laatst-bekende waarden.

**Scope:**

- Laatste update-tijd of leeftijd van de waarde tonen.
- Duidelijk onderscheid maken tussen:
  - actuele waarde;
  - laatst bekende waarde;
  - geen data.
- Waar zinvol een simpele statusbadge tonen.

**Buiten scope:**

- Geen volledige technische tracing per bericht.
- Geen waarschuwingenlogboek.

**Acceptatiecriteria:**

- Gebruiker ziet per relevante waarde of de data actueel, oud of afwezig is.
- De UI gebruikt begrijpelijke labels en geen puur technische DB-termen.
- Handmatige test met stilvallende of ontbrekende data laat zien dat de status correct verandert.

**Handmatige testnotities:**

- Test met live data en daarna tijdelijk zonder nieuwe updates.

---

### DSH-LIVE-3: Logboekactiviteit en snelle doorsteek vanaf dashboard

**Status:** Voorgesteld op 2026-05-29.

**User Story:** Als gebruiker wil ik vanaf het dashboard snel zien of er recente logboekactiviteit of open acties zijn, zodat ik snel naar de juiste plek in de applicatie kan doorklikken.

**Scope:**

- Eenvoudige dashboardsectie tonen met:
  - actieve/open reis;
  - recente logboekregel of laatst bekende logtijd;
  - eventuele open concept- of missing-moment-status indien aanwezig.
- Snelle doorklik naar `/logbook`.

**Buiten scope:**

- Geen volledige logboektabel op het dashboard.
- Geen nieuwe logboekworkflow.

**Acceptatiecriteria:**

- Dashboard toont samengevatte logboekstatus zonder de logboekpagina te dupliceren.
- Er is een duidelijke doorklik naar de logboekmodule.
- Handmatige test bevestigt dat dashboard en logboekstatus logisch overeenkomen.

**Handmatige testnotities:**

- Test met een open reis en minimaal één recente logregel.

## Aanbevolen volgorde

1. `SYS-ANALYSIS-1` technische analysepagina in `system-operations.md`
2. `SYS-CTRL-1` ingest verwerken aan/uit in `system-operations.md`
3. `DSH-LIVE-1` live dashboard met actuele meetwaarden
4. `DSH-LIVE-2` actualiteit/status van waarden
5. `DSH-LIVE-3` logboekactiviteit op dashboard

## Waarom deze volgorde

- Eerst technische analyse, omdat dat direct helpt bij testen, support en validatie.
- Daarna ingest-bediening, omdat de gebruiker nu al een concreet operationeel probleem heeft met onnodig loggen in de haven.
- Pas daarna het live dashboard, zodat we presentatie bouwen bovenop een beter beheersbaar en beter diagnosticeerbaar systeem.
