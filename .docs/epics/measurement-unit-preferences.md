# Epic: Meetweergave en eenheidsvoorkeuren

**Status:** Voorgesteld op 2026-06-06 naar aanleiding van praktijkgebruik tijdens varen.

## Aanleiding

Tijdens varen bleek dat het dashboard windsnelheid standaard in meter per seconde toont. Voor nautisch gebruik kan een gebruiker echter een andere voorkeur hebben, bijvoorbeeld knopen. Hetzelfde geldt voor snelheid door het water, snelheid over de grond, afstand, diepte, temperatuur en andere meetwaarden.

BootManager moet meetgegevens intern eenduidig en volgens de gebruikte domein- en NMEA-definities blijven verwerken en opslaan, maar de gebruiker moet kunnen bepalen in welke eenheden deze gegevens in de gebruikersinterface worden gepresenteerd.

## Doel

Gebruikers kunnen via Instellingen hun voorkeurs-eenheden kiezen. Dashboard, logboek en toekomstige gebruikersgerichte weergaven gebruiken vervolgens consequent die voorkeuren, zonder de opgeslagen bronwaarden te wijzigen.

## Relatie met andere epics

Deze epic sluit aan op:

- `dashboard-overview.md`, in het bijzonder `DSH-LIVE-1` en `DSH-LIVE-5`;
- instellingen en gebruikers-/booteigenaarsvoorkeuren;
- toekomstige logboek-, grafiek- en rapportageweergaven.

De eerste implementatieslice richt zich op het dashboard en de instellingenpagina. Brede toepassing in historische rapportages kan daarna per verticale slice volgen.

---

## UNIT-PREF-1: Voorkeurs-eenheden configureren

**Status:** Voorgesteld op 2026-06-06.

**User story:** Als gebruiker wil ik in Instellingen kunnen kiezen in welke eenheden BootManager snelheden, wind, diepte en temperatuur toont, zodat de weergave aansluit bij mijn nautische voorkeuren en bestaande boordinstrumenten.

### Scope

- Voeg op de instellingenpagina een sectie **Eenheden** of **Meetweergave** toe.
- Ondersteun minimaal afzonderlijke voorkeuren voor:
  - bootsnelheid, waaronder snelheid door het water en snelheid over de grond;
  - windsnelheid;
  - diepte;
  - temperatuur.
- Ondersteun minimaal de volgende keuzes:
  - bootsnelheid: `knopen`, `km/u` en `m/s`;
  - windsnelheid: `knopen`, `m/s`, `km/u` en optioneel `Beaufort` als afgeleide presentatiewaarde;
  - diepte: `meter`, `voet` en optioneel `vadem`;
  - temperatuur: `Celsius` en `Fahrenheit`.
- Maak windsnelheid en bootsnelheid afzonderlijk instelbaar. Een gebruiker kan bijvoorbeeld bootsnelheid in knopen en windsnelheid in m/s willen zien.
- Bewaar de voorkeuren persistent, zodat zij na herstart, opnieuw aanmelden en browser-refresh behouden blijven.
- Pas de gekozen eenheden minimaal toe op alle relevante dashboardtegels en bijbehorende labels/gauges.
- Gebruik één centrale conversie- en formatteringsvoorziening, zodat conversies niet verspreid in Razor-componenten worden geïmplementeerd.
- Houd interne domeinwaarden, databasewaarden en ontvangen NMEA-waarden ongewijzigd. Conversie vindt uitsluitend plaats aan de presentatierand of in een daarvoor bedoelde applicatieservice.
- Toon bij iedere waarde expliciet de actieve eenheid.
- Zorg dat gauge-bereiken, schaalverdeling en formattering passen bij de gekozen eenheid.

### Voorgestelde standaardwaarden

Voor een nieuwe installatie:

- bootsnelheid: knopen;
- windsnelheid: knopen;
- diepte: meter;
- temperatuur: Celsius.

Deze defaults moeten tijdens refinement nog worden getoetst aan bestaande BootManager- en legacy-afspraken. Bestaande installaties krijgen bij migratie veilige defaults zonder dat het dashboard uitvalt.

### Domein- en architectuurregels

- Opslag gebruikt één canonieke eenheid per meettype.
- Conversies zijn deterministisch en centraal getest.
- Eenheidsvoorkeuren zijn presentatievoorkeuren en mogen parsing, interpretatie of opslag van NMEA-berichten niet beïnvloeden.
- API-contracten moeten duidelijk maken of zij canonieke waarden of reeds geconverteerde presentatiewaarden leveren.
- Gebruik sterke typen of enums voor eenheidskeuzes; geen vrije tekstwaarden in businesslogica.
- Nieuwe of aangepaste C#-code krijgt Nederlandse XML-documentatie waar relevant.
- Target blijft .NET 8 en de bestaande solution- en featurestructuur blijft leidend.

### Buiten scope

- Geen conversie van ruwe NMEA-payloads vóór interpretatie.
- Geen wijziging van historische databasewaarden wanneer een voorkeur wordt aangepast.
- Geen automatische detectie van eenheden op basis van locatie of taal.
- Geen afzonderlijke voorkeur per dashboardtegel in de eerste slice.
- Geen volledig metrisch/imperiaal profiel dat alle toekomstige meettypen automatisch omvat.
- Geen verandering aan alarmdrempels zonder expliciete aparte story; drempels moeten intern canoniek blijven.

### Acceptatiecriteria

- Op Instellingen kan de gebruiker de voorkeurs-eenheden voor bootsnelheid, windsnelheid, diepte en temperatuur kiezen.
- De instellingen worden persistent opgeslagen en na herstart opnieuw toegepast.
- Snelheid door het water en SOG gebruiken de ingestelde voorkeur voor bootsnelheid.
- Windsnelheid gebruikt de afzonderlijk ingestelde windvoorkeur.
- Dashboardwaarden, eenheidslabels en gauge-schalen veranderen direct of na een duidelijk aangegeven herladen van de pagina.
- Een wijziging van de presentatie-eenheid verandert geen opgeslagen meetwaarde in de database.
- Conversies leveren aantoonbaar correcte waarden op, minimaal voor:
  - `1 m/s = 1,943844 kn`;
  - `1 kn = 1,852 km/u`;
  - `1 m = 3,28084 ft`;
  - `0 °C = 32 °F`.
- Ontbrekende of onbekende voorkeuren vallen terug op veilige standaardwaarden.
- Bestaande gebruikers/installaties blijven na database- of configuratiemigratie werken.
- Build en relevante unit-/integratietests slagen.

### Handmatige acceptatietest

1. Open het dashboard met live of gesimuleerde wind-, STW-, SOG-, diepte- en temperatuurdata.
2. Noteer de getoonde waarden en database-/API-bronwaarden.
3. Wijzig bootsnelheid van knopen naar km/u.
4. Controleer dat STW en SOG correct converteren en windsnelheid ongewijzigd blijft.
5. Wijzig windsnelheid van knopen naar m/s.
6. Controleer dat alleen de windweergave en bijbehorende gauge-schaal veranderen.
7. Wijzig diepte naar voet en temperatuur naar Fahrenheit.
8. Refresh de browser en herstart de applicatie/container.
9. Controleer dat de keuzes behouden blijven.
10. Controleer dat opgeslagen bronwaarden niet zijn aangepast.

### Testgevallen voor automatische tests

- Conversie tussen iedere ondersteunde snelheids-eenheid.
- Conversie meter/voet en Celsius/Fahrenheit.
- Formatterings- en afrondingsregels per eenheid.
- Fallback bij ontbrekende of ongeldige opgeslagen voorkeur.
- Onafhankelijkheid van bootsnelheid- en windsnelheidvoorkeur.
- Dashboard-DTO of presenter gebruikt voorkeuren zonder domeinentiteiten te muteren.

### Legacy-impact

- `US7.1 Dashboardweergave openen`: betere personaliseerbaarheid van het dashboard.
- `US7.2 Actieve bootinformatie`: meetwaarden sluiten beter aan bij nautische gebruikersverwachtingen.
- Eventuele bestaande legacy-scope rond instellingen/personalisatie moet tijdens refinement expliciet aan deze story worden gekoppeld.

---

## UNIT-PREF-2: Eenheidsvoorkeuren consequent toepassen buiten het dashboard

**Status:** Toekomstige vervolgstory.

**User story:** Als gebruiker wil ik dat mijn eenheidsvoorkeuren ook in logboek, grafieken, exports en rapportages worden gebruikt, zodat BootManager nergens tegenstrijdige eenheden toont.

### Scope op hoofdlijnen

- Pas dezelfde centrale conversie en formattering toe in logboekweergaven, grafieken, rapportages en gebruikersgerichte exports.
- Benoem in machinegerichte exports/API's expliciet welke eenheid wordt gebruikt.
- Voorkom dubbele conversie wanneer data al voor presentatie is omgezet.

### Acceptatiecriteria op hoofdlijnen

- Dezelfde meetwaarde wordt in alle gebruikersgerichte schermen in dezelfde gekozen eenheid getoond.
- Exports vermelden ondubbelzinnig de gebruikte eenheid.
- Canonieke opslag blijft ongewijzigd.
