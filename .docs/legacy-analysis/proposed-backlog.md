# Proposed BootManagerV2 Backlog From Legacy Scope

Status: eerste voorstel (2026-05-25).

Dit document vertaalt de legacy PDF naar BootManagerV2-epics in de huidige stijl.
Het is een voorstel, geen definitieve roadmap.

## Prioriteit 1: Beheer Na Onboarding

Epic bestaat al:

- `.docs/epics/owner-profile-settings.md`

Voorgestelde volgorde:

1. Bootgegevens wijzigen in instellingen.
2. Eigenaargegevens wijzigen in instellingen.
3. Wachtwoord wijzigen runtime/UX verifiëren.
4. Settings logisch ordenen.

Reden:

- Direct voortgekomen uit handmatige test.
- Sluit aan op afgeronde onboarding.
- Kleine, gecontroleerde slices.

## Prioriteit 2: Logboek Verder Afronden

Bestaande epic:

- `.docs/epics/digital-logbook.md`

Open legacy-scope die nog relevant is:

### US-LB1: Logboek Afronden Bij Aankomst

Als eigenaar wil ik bij aankomst ontbrekende eindgegevens invullen, zodat een reislogboek compleet wordt afgesloten.

Velden kunnen later bevatten:

- aankomsthaven;
- eind-logstand;
- motoruren eind;
- brandstofniveau eind;
- totale afstand;
- opmerkingen.

### US-LB2: Motoruren En Brandstof In Reisheader

Als eigenaar wil ik motoruren en brandstofgegevens in de reisheader vastleggen, zodat verbruik later inzichtelijk wordt.

### US-LB3: Routekaart Op Basis Van Positiemetingen

Als gebruiker wil ik de gevaren route als kaart/track zien, zodat het logboek visueel bruikbaar wordt.

### US-LB4: Export Naar PDF/CSV

Als eigenaar wil ik logboekgegevens exporteren, zodat ik reizen kan archiveren of delen.

Let op:

- Browser print bestaat al.
- Server-side PDF en CSV zijn aparte stories.

### US-LB5: Logboekstatistieken Uitbreiden

Als eigenaar wil ik reisstatistieken zien, zoals afstand, duur, gemiddelde snelheid en eventueel brandstofverbruik.

## Prioriteit 3: Inventory & Storage Locations

Nieuwe epic voorgesteld:

- `.docs/epics/inventory-management.md`

Eerste slices:

### US-INV1: Opslaglocaties Modelleren

Als eigenaar wil ik opslaggebieden en opslaglocaties vastleggen, zodat voorraad later aan fysieke plekken gekoppeld kan worden.

Voorbeelden:

- kajuit;
- kombuis;
- machinekamer;
- kast;
- lade;
- bak.

### US-INV2: Productcatalogus Aanmaken

Als eigenaar wil ik producten kunnen aanmaken met naam, categorie en eenheid, zodat een inventarisbasis ontstaat.

### US-INV3: Product Aan Opslaglocatie Koppelen

Als eigenaar wil ik producten aan locaties koppelen met hoeveelheden, zodat ik weet wat waar ligt.

### US-INV4: Voorraad Aanpassen En Loggen

Als gebruiker wil ik voorraad kunnen corrigeren of verbruik registreren, zodat aantallen actueel blijven.

### US-INV5: Zoeken En Filteren

Als gebruiker wil ik producten kunnen zoeken op naam, categorie en locatie.

Later:

- minimumvoorraad;
- QR/barcode;
- import/export;
- voorraadstatus dashboard;
- passageplanning-integratie.

## Prioriteit 4: General Document Management

Nieuwe epic voorgesteld:

- `.docs/epics/document-management.md`

Eerste slices:

### US-DOC1: Document Toevoegen

Als eigenaar wil ik documenten uploaden met titel, type, beschrijving en optionele vervaldatum.

### US-DOC2: Documenten Zoeken En Filteren

Als eigenaar wil ik documenten snel terugvinden op type, titel, status of vervaldatum.

### US-DOC3: Vervaldatumstatus Tonen

Als eigenaar wil ik zien welke documenten bijna verlopen of verlopen zijn.

Later:

- koppelen aan passage;
- koppelen aan onderhoud;
- audit trail;
- export documentlijst.

## Prioriteit 5: Maintenance Log

Nieuwe epic voorgesteld:

- `.docs/epics/maintenance-log.md`

Eerste slices:

### US-MAINT1: Onderhoudstaak Aanmaken

Als eigenaar wil ik onderhoudstaken kunnen vastleggen met onderdeel, beschrijving en geplande datum.

### US-MAINT2: Uitgevoerd Onderhoud Registreren

Als eigenaar wil ik een taak als uitgevoerd registreren met datum, kosten, monteur en opmerkingen.

### US-MAINT3: Onderhoudshistorie Bekijken

Als eigenaar wil ik onderhoud per onderdeel of periode kunnen bekijken.

Later:

- intervalplanning;
- herinneringen;
- bijlagen;
- export;
- dashboard.

## Prioriteit 6: Passageplanning

Nieuwe epic voorgesteld:

- `.docs/epics/passage-planning.md`

Niet starten voordat inventory/document basics duidelijk zijn.

Eerste slices:

- passage aanmaken met vertrek, bestemming en duur;
- bemanningslijst;
- passage koppelen aan logbook trip;
- later pas voorraadberekening en menuplanning.

## Prioriteit 7: Systeembeheer, Backup En Device Status

Uitbreiding op bestaande settings/deployment-docs.

Mogelijke stories:

- backup maken van SQLite database en bijlagen;
- restoreprocedure in UI of helper;
- Raspberry Pi systeemstatus tonen;
- veilige shutdown-flow;
- systeemactie-logboek.

## Geparkeerd / Lage Prioriteit

Deze legacy-scope is bewust niet voor de korte termijn:

- multi-user rollenmodel;
- meerdere boten;
- cloud-synchronisatie;
- externe API-integraties;
- AI-herkenning;
- spraakinput;
- volledige notificatie-infrastructuur;
- dashboard widgetpersonalisatie.
