# BootManager – Epic 4: Documentbeheer

Dit document bevat de volledige, definitieve user stories en acceptatiecriteria voor Epic 4 (Documentbeheer), conform het canvas.

## Doel

Gebruikers (voornamelijk de eigenaar) kunnen digitale documenten toevoegen, beheren, koppelen en raadplegen aan boord van de boot. Hiermee blijven alle belangrijke scheeps- en reisgerelateerde papieren veilig opgeslagen, gemakkelijk toegankelijk, en kunnen ze worden gecontroleerd op geldigheid.

## Belangrijkste functionaliteiten

- Uploaden en opslaan van documenten (PDF, JPG, PNG, DOCX, enz.)

- Categoriseren van documenten (verzekering, certificaat, vergunning, handleiding, enz.)

- Optionele vervaldatum en herinneringsdatum + waarschuwingen

- Koppeling van documenten aan boot, bemanningslid of passage

- Offline beschikbaarheid en synchronisatie met cloud

- Zoeken, filteren en sorteren op naam, beschrijving, categorie of status

- Dashboard met status (geldig, bijna verlopen, verlopen)

- Export van documentlijst (PDF/CSV)

- Bekijken, printen, mailen of exporteren van individuele documenten

- Audit trail (wie/wat/wanneer)

## User Stories + Acceptatiecriteria

**US4.1 – Document toevoegen en categoriseren**

\*Als eigenaar wil ik een document kunnen uploaden met naam, beschrijving, categorie (bestaand of nieuw), en – indien van toepassing – een vervaldatum, zodat ik mijn scheepsdocumenten digitaal kan bewaren en direct correct kan indelen.\*

Given dat de eigenaar is ingelogd,

When hij een document uploadt en de vereiste gegevens invult,

Then kan hij:

\- Een bestaande categorie selecteren of direct een nieuwe categorie aanmaken (met naam, beschrijving en optioneel icoontje)

\- Een optionele vervaldatum opgeven

\- Het document opslaan in de database met alle metadata

And BootManager toont het document direct in het overzicht met de juiste categorie en status.

**US4.2 – Document bewerken of verwijderen**

\*Als eigenaar wil ik documenten kunnen aanpassen of verwijderen, zodat mijn administratie up-to-date blijft.\*

Given dat een document al bestaat,

When de eigenaar wijzigingen aanbrengt of kiest voor verwijderen,

Then worden de wijzigingen opgeslagen of het document na bevestiging verwijderd uit het systeem.

**US4.3 – Document koppelen aan boot, bemanningslid of passage**

\*Als eigenaar wil ik documenten kunnen koppelen aan een specifieke boot, bemanningslid of passage, zodat ik de context behoud waarin het document relevant is.\*

Given dat documenten en entiteiten (boot, bemanningsleden, passages) bestaan,

When de eigenaar een document koppelt aan één of meerdere van deze entiteiten,

Then wordt deze relatie opgeslagen en weergegeven bij zowel het document als het gekoppelde onderdeel.

**US4.4 – Vervaldatum en waarschuwingen**

\*Als eigenaar wil ik per document een optionele vervaldatum kunnen opslaan en automatisch een waarschuwing ontvangen wanneer deze bijna verloopt, zodat ik tijdig actie kan ondernemen.\*

Given dat documenten met een vervaldatum bestaan,

When de huidige datum binnen de ingestelde herinneringstermijn valt (bijv. 30 dagen),

Then toont BootManager een melding op het dashboard en in het documentenoverzicht,

And documenten zonder vervaldatum blijven onbeperkt geldig zonder waarschuwing.

**US4.5 – Documentstatusoverzicht (dashboard)**

\*Als eigenaar wil ik een dashboard zien met alle documenten die binnenkort verlopen of al verlopen zijn, zodat ik overzicht heb over mijn administratieve status.\*

Given dat er documenten met vervaldatums bestaan,

When de eigenaar het documentdashboard opent,

Then toont BootManager een lijst met documenten per status: geldig, bijna verlopen, verlopen.

**US4.6 – Zoeken, filteren en sorteren (uitgebreid)**

\*Als eigenaar wil ik documenten kunnen zoeken op naam of beschrijving, en kunnen filteren op categorie of status, zodat ik snel vind wat ik nodig heb.\*

Given dat meerdere documenten zijn opgeslagen,

When de eigenaar een zoekterm invoert of filters kiest,

Then zoekt BootManager in naam en beschrijving,

And toont het systeem alleen de documenten die voldoen aan de ingevoerde zoekcriteria of filters.

**US4.7 – Offline beschikbaarheid**

\*Als eigenaar wil ik dat alle documenten offline beschikbaar blijven aan boord, zodat ik ze ook zonder internet kan raadplegen.\*

Given dat BootManager lokaal draait op de Raspberry Pi,

When de eigenaar een document opent,

Then wordt het bestand lokaal geladen, ongeacht of er internettoegang is.

**US4.8 – Cloud-synchronisatie**

\*Als eigenaar wil ik dat documenten automatisch worden gesynchroniseerd zodra er internetverbinding is, zodat mijn administratie altijd actueel is, zowel aan boord als thuis.\*

Given dat cloud-synchronisatie is ingeschakeld,

When nieuwe documenten worden toegevoegd of bestaande worden gewijzigd,

Then worden de wijzigingen gesynchroniseerd zodra een verbinding beschikbaar is.

**US4.9 – Exporteren van documentlijst**

\*Als eigenaar wil ik mijn documentoverzicht kunnen exporteren naar PDF of CSV, zodat ik een back-up of printbaar overzicht kan maken.\*

Given dat documenten bestaan,

When de eigenaar kiest voor “Exporteer documentlijst”,

Then wordt een bestand aangemaakt met documentnamen, categorieën, vervaldatums en gekoppelde entiteiten.

**US4.10 – Documentgeschiedenis / audit trail**

\*Als eigenaar wil ik kunnen zien wie wanneer een document heeft toegevoegd, gewijzigd of verwijderd, zodat ik inzicht heb in de documentgeschiedenis.\*

Given dat documentwijzigingen worden gelogd,

When de eigenaar de geschiedenis van een document opent,

Then toont BootManager een overzicht met datum, tijd, gebruiker en actie.

**US4.11 – Herinneringsinstellingen beheren**

\*Als eigenaar wil ik kunnen instellen hoeveel dagen vooraf ik een waarschuwing krijg voor een vervallend document, zodat ik zelf bepaal hoe vroeg ik herinnerd wil worden.\*

Given dat BootManager waarschuwingen ondersteunt,

When de eigenaar een herinneringstermijn aanpast (bijv. 14, 30 of 60 dagen),

Then past het systeem de waarschuwingen daarop aan en berekent toekomstige meldingen volgens die instelling.

**US4.12 – Documenten koppelen aan passageplanning (Epic 3)**

\*Als eigenaar wil ik tijdens het plannen van een passage relevante documenten kunnen toevoegen (zoals vaarplan of vergunningen), zodat ik alles bij de hand heb voor vertrek.\*

Given dat een passageplan bestaat,

When de eigenaar documenten koppelt aan die passage,

Then worden ze automatisch gecontroleerd op geldigheid binnen de geplande reisperiode.

**US4.13 – Document openen, printen of delen**

\*Als eigenaar wil ik een opgeslagen document kunnen openen, printen, mailen of exporteren, zodat ik het eenvoudig kan bekijken of delen met anderen (bijv. havenmeester, verzekering of onderhoudsbedrijf).\*

Given dat een document in het systeem is opgeslagen,

When de eigenaar het documentdetail opent,

Then kan hij:

\- Het document bekijken in een ingebouwde viewer (PDF/afbeelding)

\- Het document printen direct vanaf het apparaat

\- Het document mailen of exporteren als bijlage (indien internet beschikbaar)

And BootManager registreert deze actie in de audit trail.

## Samenvatting van de Epic

| Categorie | Functionaliteit                                               |
|-----------|---------------------------------------------------------------|
| Beheer    | Uploaden, categoriseren, bewerken, verwijderen van documenten |
| Structuur | Categorieën, koppelingen aan boot/bemanning/passage           |
| Controle  | Optionele vervaldatums, waarschuwingen en herinneringen       |
| Toegang   | Offline beschikbaarheid + cloud-synchronisatie                |
| Overzicht | Dashboard, filtering, audit trail, exportfunctie              |
