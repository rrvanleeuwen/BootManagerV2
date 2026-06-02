# BootManager – Epic 5: Logboek (Nautisch Journaal)

Dit document bevat de volledige beschrijving van Epic 5 van het BootManager-project. De Epic behandelt het logboek (nautisch journaal) waarmee gebruikers handmatig of automatisch reisgegevens kunnen vastleggen, inclusief positie, koers, wind, brandstof, motoruren en observaties. Het logboek vormt een compleet overzicht van een passage, gekoppeld aan de passageplanning.

## Doel

Tijdens een passage kunnen gebruikers (bemanning of eigenaar) automatisch of handmatig reisgegevens vastleggen — zoals tijd, positie, koers, snelheid, weersomstandigheden, verbruik en bijzonderheden — zodat een compleet logboek van de reis ontstaat. Het logboek vormt een historisch en operationeel overzicht van de tocht, gekoppeld aan de passageplanning (Epic 3).

## Belangrijkste functionaliteiten

- Handmatig of automatisch loggen tijdens passages

- GPS/NMEA-integratie voor automatische positie-, snelheid- en koersbepaling

- Logboekheader met vertrek, aankomst, bemanning, brandstof, motoruren, logstand en tijdsduur

- Invoer van windrichting, windsnelheid, temperatuur, barometer en opmerkingen per logregel

- Automatische berekening van afgelegde afstand, brandstofverbruik en gemiddelde snelheid

- Koppeling van logboek aan passageplan (Epic 3)

- Toevoegen van bijlagen (foto’s, documenten) per logregel

- Weergave van logboek in klassiek nautisch format

- Export naar PDF/CSV inclusief route en statistieken

- Offline werking met latere cloud-synchronisatie

## User Stories + Acceptatiecriteria

### US5.1 – Handmatig logboek invoeren (met weerinformatie)

Als bemanningslid of eigenaar wil ik handmatig een logboekvermelding kunnen toevoegen met tijd, positie, koers, snelheid, wind en opmerkingen, zodat ik mijn reis nauwkeurig kan vastleggen.

Given dat er een actieve passage bestaat, When de gebruiker een log-entry toevoegt met de genoemde gegevens, Then wordt deze opgeslagen in het logboek en zichtbaar in de lijst- en kaartweergave.

### US5.2 – Automatisch loggen en intervalinstelling

Als eigenaar wil ik dat BootManager automatisch logboekregels aanmaakt op basis van GPS/NMEA-data, met een instelbaar interval, zodat ik zonder handmatige invoer mijn reisgegevens kan bijhouden.

Given dat BootManager verbonden is met GPS/NMEA, When automatische logging actief is, Then registreert BootManager automatisch tijd, positie, koers, snelheid en windinformatie op het ingestelde interval.

### US5.3 – Motoruren en brandstof in header

Als eigenaar wil ik in het logboek de motoruren en het brandstofniveau kunnen vastleggen, zodat ik het verbruik van de motor tijdens de reis kan volgen.

Given dat een logboekheader bestaat, When de gebruiker motoruren (start/eind) en brandstofniveau opgeeft, Then berekent BootManager automatisch de verbruikte brandstof en het aantal motoruren van de reis.

### US5.4 – Notities en gebeurtenissen toevoegen

Als bemanningslid of eigenaar wil ik bij iedere log-entry notities of gebeurtenissen kunnen vastleggen, zodat ik persoonlijke observaties in het logboek kan opnemen.

Given dat een log-entry bestaat, When de gebruiker tekst toevoegt in het notitieveld, Then wordt deze opgeslagen en weergegeven bij die entry.

### US5.5 – Logboek koppelen aan passage

Als eigenaar wil ik dat het logboek automatisch wordt gekoppeld aan de actieve passage, zodat de reisgegevens logisch worden gestructureerd.

Given dat een passage actief is, When BootManager logregels aanmaakt of de gebruiker handmatig logt, Then worden deze entries gekoppeld aan die passage.

### US5.6 – Logboekheader invullen

Als eigenaar wil ik bij het aanmaken van een logboek de reisgegevens (vertrek, aankomst, bemanning, brandstof, motoruren, logstand, etc.) kunnen vastleggen, zodat het logboek een volledig overzicht van de reis vormt.

Given dat een nieuwe passage is gestart, When de gebruiker een logboek aanmaakt, Then kan hij alle header-velden invullen of automatisch laten overnemen uit de passageplanning.

### US5.7 – Logregels met nautische velden

Als bemanningslid of eigenaar wil ik per logregel nautische gegevens kunnen vastleggen (tijd, koers, wind, barometer, logstand, opmerkingen), zodat de reis gedetailleerd wordt bijgehouden.

Given dat een logboek bestaat, When de gebruiker een logregel toevoegt, Then kan hij alle relevante velden invullen, inclusief windinformatie en opmerkingen.

### US5.8 – Bijlagen toevoegen aan logregel

Als bemanningslid of eigenaar wil ik een foto of bestand kunnen koppelen aan een logregel, zodat ik bijzondere momenten of omstandigheden visueel kan vastleggen.

Given dat een logregel bestaat, When de gebruiker een bijlage toevoegt, Then wordt deze gekoppeld en zichtbaar in het logboekoverzicht.

### US5.9 – Logboekweergave in klassiek format en routekaart

Als gebruiker wil ik dat het logboek in een klassiek nautisch format wordt weergegeven, met een kaart van de afgelegde route, zodat het aansluit bij het traditionele scheepsjournaal.

Given dat logboekgegevens bestaan, When de gebruiker het logboek bekijkt, Then toont BootManager de header met reisinfo en een tabel met tijd, koers, wind, positie, opmerkingen, inclusief een routekaart.

### US5.10 – Exporteren van logboek

Als eigenaar wil ik mijn logboek kunnen exporteren naar PDF of CSV, zodat ik de reis kan archiveren of delen met anderen.

Given dat een logboek bestaat, When de gebruiker kiest voor export, Then genereert BootManager een bestand met alle logregels, inclusief de headerinformatie en kaart.

### US5.11 – Statistieken en samenvatting

Als eigenaar wil ik na afloop van een passage een samenvatting kunnen zien van de totale reisduur, afstand, gemiddelde snelheid en brandstofverbruik, zodat ik inzicht krijg in de prestaties van de boot.

Given dat een logboek volledig is, When de gebruiker de samenvatting bekijkt, Then toont BootManager afstand, duur, verbruik, gemiddelde snelheid en windstatistieken.

### US5.12 – Offline werking

Als bemanningslid of eigenaar wil ik dat het logboek volledig offline werkt, zodat ik altijd kan loggen, ook zonder internet of GPS-signaal.

Given dat BootManager offline draait, When de gebruiker logregels toevoegt, Then worden deze lokaal opgeslagen en later gesynchroniseerd zodra verbinding beschikbaar is.

### US5.13 – Cloud-synchronisatie

Als eigenaar wil ik dat mijn logboek automatisch wordt gesynchroniseerd zodra er internetverbinding is, zodat mijn gegevens veilig worden opgeslagen en ook thuis beschikbaar zijn.

Given dat cloud-synchronisatie is ingeschakeld, When BootManager online komt, Then worden alle lokale logboekgegevens geüpload naar de cloud en gekoppeld aan de juiste passage.

### US5.14 – Logboek afronden bij aankomst

Als eigenaar wil ik bij aankomst de resterende logboekgegevens kunnen invullen of bijwerken (zoals aankomsthaven, motorureneindstand, brandstofniveau, eind-logstand en totale afstand), zodat het logboek compleet en accuraat wordt afgesloten.

Given dat een logboek is gestart met vertrekgegevens, When de gebruiker aangeeft dat de passage is voltooid, Then opent BootManager een afrondingsscherm met de ontbrekende velden (zoals aankomsthaven, motoruren-eind, afstand, enz.), And na bevestiging wordt het logboek als voltooid gemarkeerd en opgenomen in de statistieken.
