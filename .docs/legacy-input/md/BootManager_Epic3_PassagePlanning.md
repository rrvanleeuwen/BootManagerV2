# BootManager – Epic 3: Passageplanning

Dit document bevat de volledige, definitieve user stories en acceptatiecriteria voor Epic 3 (Passageplanning), conform het canvas.

## Doel

Gebruikers (voornamelijk de eigenaar) kunnen zeiltochten plannen, de bemanning registreren, de duur en bestemming vastleggen en automatisch berekenen welke voorraad en documenten nodig zijn voor een veilige en goed voorbereide reis.

## Belangrijkste functionaliteiten

- Aanmaken, bewerken en verwijderen van passageplannen

- Invoeren van vertrekdatum, duur, bestemming(en) en opvarenden

- Berekenen van voorraadbehoefte op basis van duur en aantal personen

- Vergelijken met actuele voorraad (Epic 2) en boodschappenlijst genereren

- Menuplanning per dag met koppeling aan verbruiksberekening

- Koppelen van documenten en certificaten aan een passage

- Exporteren van een compleet reisplan (PDF/CSV)

- Dashboard met overzicht van planning, status en waarschuwingen

- Instelbare verbruiksprofielen (brandstof per uur, water per persoon per dag)

- Synchronisatie en offline modus

## User Stories + Acceptatiecriteria

**US3.1 – Passage aanmaken**

\*Als eigenaar wil ik een nieuwe passage kunnen aanmaken met vertrekdatum, bestemming en duur, zodat ik een reis kan voorbereiden.\*

Given dat de eigenaar is ingelogd,

When hij kiest voor “Nieuwe Passage” en vertrekdatum, bestemming en duur invult,

Then wordt het plan opgeslagen in de lokale database en zichtbaar in de passage-lijst.

**US3.2 – Bemanning toevoegen**

\*Als eigenaar wil ik bemanningsleden kunnen toevoegen aan een passage, inclusief hun volledige naam en geboortedatum, zodat ik een volledige bemanningslijst kan vastleggen voor de reis.\*

Given dat een passageplan bestaat,

When de eigenaar bemanningsleden toevoegt of selecteert uit bestaande gebruikers en daarbij de volledige naam en geboortedatum invult,

Then worden deze gegevens opgeslagen bij de passage en opgenomen in de bemanningslijst en exportdocumenten.

**US3.3 – Benodigdheden berekenen (met instellingen)**

\*Als eigenaar wil ik dat BootManager automatisch berekent hoeveel voedsel, water en brandstof nodig is voor de duur van de reis, op basis van mijn ingestelde verbruiksprofielen, zodat ik nauwkeurige berekeningen krijg.\*

Given dat een passageplan bestaat met duur, aantal personen en een verbruiksprofiel,

When de eigenaar de berekening start,

Then gebruikt BootManager de volgende instellingen:

\- Brandstofverbruik per uur (instelbaar in instellingen) × aantal geplande motoruren

\- Waterverbruik per persoon per dag (instelbaar in instellingen) × aantal personen × duur

\- Voedselverbruik afgeleid van geplande menu’s (zie US3.6 en US3.14)

And toont het systeem per categorie de benodigde hoeveelheden.

**US3.4 – Vergelijking met voorraad**

\*Als eigenaar wil ik dat BootManager de berekende benodigdheden vergelijkt met mijn huidige voorraad, zodat ik zie wat ik nog moet aanschaffen.\*

Given dat een berekening is uitgevoerd,

When de vergelijking met de actuele voorraad (Epic 2) plaatsvindt,

Then toont BootManager een boodschappenlijst van ontbrekende producten.

**US3.5 – Boodschappenlijst genereren**

\*Als eigenaar wil ik een boodschappenlijst kunnen exporteren of printen, zodat ik aan wal efficiënt kan inkopen.\*

Given dat de berekening en vergelijking zijn uitgevoerd,

When de eigenaar kiest voor “Exporteer lijst”,

Then wordt een overzicht (PDF/CSV) gegenereerd met productnaam, hoeveelheid en locatiecategorie.

**US3.6 – Menu’s plannen en beheren**

\*Als eigenaar wil ik per dag en per maaltijd (ontbijt, lunch, diner) gerechten kunnen plannen, aanmaken en beheren, en per gerecht kunnen vastleggen welke producten en hoeveelheden daarvoor nodig zijn, zodat ik mijn maaltijden voor de reis kan voorbereiden en BootManager automatisch het verbruik kan berekenen.\*

Given dat een passageplan bestaat,

When de eigenaar kiest voor “Menuplanning” en per dag gerechten toevoegt met naam, beschrijving, maaltijdtype en gekoppelde producten (met hoeveelheden),

Then worden deze menu’s opgeslagen per dag, zichtbaar in een overzicht en gekoppeld aan de passage,

And kan BootManager deze gegevens gebruiken voor de berekening van benodigdheden en voorraadverbruik (zie US3.14).

**US3.7 – Documenten koppelen**

\*Als eigenaar wil ik documenten zoals verzekeringspapieren, certificaten of vaarvergunningen kunnen koppelen aan een passage, zodat ik weet dat alles geldig is tijdens de reis.\*

Given dat documenten beschikbaar zijn in de documentmodule,

When de eigenaar ze koppelt aan een passage,

Then toont BootManager waarschuwingen bij vervaldatums binnen de geplande reisduur.

**US3.8 – Statusdashboard**

\*Als eigenaar wil ik in één oogopslag kunnen zien of mijn passageplanning compleet is, zodat ik weet of ik klaar ben voor vertrek.\*

Given dat een passage bestaat,

When de gebruiker het dashboard opent,

Then toont BootManager de status van voorraad, documenten, bemanning en vertrekdatum.

**US3.9 – Export reisplan**

\*Als eigenaar wil ik mijn complete reisplan kunnen exporteren naar PDF, zodat ik een overzicht heb van bemanning, route, voorraad en documenten.\*

Given dat een passageplan volledig is ingevuld,

When de eigenaar op “Exporteer reisplan” klikt,

Then genereert BootManager een gestructureerd rapport met alle relevante gegevens.

**US3.10 – Synchronisatie met logboek**

\*Als eigenaar wil ik dat het logboek automatisch gekoppeld wordt aan de passage, zodat tijdens de reis de positie en gebeurtenissen worden vastgelegd binnen hetzelfde plan.\*

Given dat de reis is gestart,

When BootManager in logmodus staat,

Then worden locatie, tijd en verbruik automatisch geregistreerd in de gekoppelde passage.

**US3.11 – Herbruikbare passage templates**

\*Als eigenaar wil ik eerdere passages kunnen dupliceren als template, zodat ik vergelijkbare reizen sneller kan voorbereiden.\*

Given dat er eerdere passages bestaan,

When de eigenaar kiest voor “Dupliceer”,

Then wordt een nieuw passageplan aangemaakt met dezelfde basisgegevens (duur, bemanning, routes) die nadien bewerkbaar zijn.

**US3.12 – Synchronisatie en offline modus**

\*Als eigenaar wil ik mijn passageplanning kunnen synchroniseren met de cloud wanneer internet beschikbaar is, zodat ik zowel aan boord als thuis mijn plannen kan beheren.\*

Given dat cloud-synchronisatie is ingeschakeld,

When wijzigingen worden gemaakt tijdens of buiten de reis,

Then worden deze gesynchroniseerd zodra een verbinding beschikbaar is.

**US3.13 – Verbruiksinstellingen beheren**

\*Als eigenaar wil ik in de instellingen kunnen aangeven wat het brandstofverbruik per uur is en hoeveel water per persoon per dag gemiddeld wordt gebruikt, zodat de berekening van benodigdheden realistisch is.\*

Given dat BootManager verbruiksinstellingen ondersteunt,

When de eigenaar waarden invult of wijzigt (bijv. brandstof: 2,3 liter/uur, water: 15 liter/persoon/dag),

Then worden deze waarden opgeslagen in de lokale configuratie en gebruikt bij toekomstige berekeningen van benodigdheden.

**US3.14 – Menu’s koppelen aan verbruiksberekening**

\*Als eigenaar wil ik bij het plannen van menu’s kunnen aangeven welke producten en hoeveelheden daarvoor worden gebruikt, zodat BootManager het totale voedselverbruik kan berekenen en vergelijken met de voorraad.\*

Given dat er menu’s zijn ingevoerd voor de passage,

When de eigenaar per gerecht producten selecteert met benodigde hoeveelheden,

Then berekent BootManager automatisch het totale verbruik van elk product over de hele reis en vergelijkt dit met de actuele voorraad (Epic 2).

## Samenvatting van de Epic

| Categorie | Functionaliteit                                           |
|-----------|-----------------------------------------------------------|
| Beheer    | Plannen en beheren van passages en bemanning              |
| Structuur | Koppeling tussen passages, menu’s en voorraad             |
| Controle  | Berekening van verbruik, waarschuwingen en voorspellingen |
| Toegang   | Offline beschikbaarheid en cloud-synchronisatie           |
| Overzicht | Dashboard, rapportages, integratie met andere modules     |
