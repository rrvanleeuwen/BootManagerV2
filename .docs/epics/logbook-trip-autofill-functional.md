# Digitaal Logboek: Reisgegevens automatisch voorinvullen

**Status:** Voorgesteld op 2026-06-06.

## User story

Als gebruiker wil ik dat BootManager bij het starten, bewerken en afronden van een reis alle bekende of betrouwbaar afleidbare reisgegevens automatisch voorinvult, zodat ik zo weinig mogelijk dubbel hoef in te voeren.

## Functionele uitgangspunten

- Automatisch ingevulde waarden zijn voorstellen.
- Alle voorgestelde waarden blijven bewerkbaar.
- Een handmatig gewijzigde waarde wordt niet opnieuw overschreven.
- Een onbekende of onbetrouwbare waarde blijft leeg of wordt herkenbaar als schatting getoond.

## Bij het starten van een reis

BootManager vult waar mogelijk automatisch in:

- actieve boot en bootnaam;
- datum en vertrektijd;
- vertrekplaats vanuit de vorige afgeronde reis of een bruikbare actuele positie;
- motoruren start vanuit motoruren eind van de vorige afgeronde reis van dezelfde boot;
- logstand start vanuit de vorige eindstand of een betrouwbare actuele waarde;
- eventueel de laatst gebruikte bemanning als voorstel;
- een voorstel voor de reisnaam op basis van datum, vertrekplaats en bestemming wanneer die bekend zijn.

## Bij het stoppen of afronden van een reis

BootManager vult of berekent waar mogelijk automatisch:

- aankomsttijd;
- aankomstplaats vanuit een bruikbare eindpositie;
- reisduur uit vertrek- en aankomsttijd;
- vaartijd, maar alleen wanneer betrouwbaar kan worden vastgesteld wanneer de boot daadwerkelijk voer;
- motoruren eind, alleen als daarvoor een betrouwbare waarde beschikbaar is;
- gebruikte motoruren uit begin- en eindstand;
- logstand eind;
- gevaren afstand uit begin- en eindstand of andere betrouwbare reisgegevens;
- brandstofverbruik of eindvoorraad, alleen als daarvoor betrouwbare gegevens beschikbaar zijn.

## Betekenis van tijdsvelden

- **Reisduur:** tijd tussen vertrek en aankomst, inclusief stilliggen.
- **Vaartijd:** tijd waarin de boot daadwerkelijk onderweg was.
- **Motorlooptijd:** tijd waarin de motor draaide, als dit betrouwbaar bekend is.

Deze waarden worden niet als synoniemen gebruikt. Als vaartijd of motorlooptijd niet betrouwbaar kan worden bepaald, blijft die waarde leeg.

## Gedrag in het scherm

- Voorstellen staan direct in de bijbehorende velden.
- De gebruiker kan alle velden aanpassen.
- Onzekere waarden worden herkenbaar als schatting weergegeven.
- Opnieuw berekenen gebeurt alleen na een expliciete gebruikersactie.
- Voor definitief afronden toont BootManager een compacte samenvatting van tijden, afstanden en standen.

## Acceptatiecriteria

- Bij een nieuwe reis wordt de actieve boot automatisch geselecteerd.
- Datum en vertrektijd worden voorgesteld op basis van het startmoment.
- Motoruren start worden, indien beschikbaar, gevuld met motoruren eind van de vorige afgeronde reis van dezelfde boot.
- Logstand start wordt, indien beschikbaar, gevuld vanuit de vorige eindstand of een betrouwbare actuele waarde.
- Bij stoppen wordt aankomsttijd automatisch voorgesteld.
- Reisduur wordt correct berekend uit vertrek en aankomst.
- Vaartijd wordt alleen ingevuld als deze betrouwbaar kan worden bepaald.
- Begin- en eindlocatie worden alleen voorgesteld wanneer een bruikbare bron beschikbaar is.
- Alle voorgestelde waarden zijn door de gebruiker te wijzigen.
- Handmatig gewijzigde waarden blijven behouden na verversen of binnenkomst van nieuwe informatie.
- Ontbrekende informatie veroorzaakt geen fout en levert geen verzonnen waarde op.
- Definitief opslaan bewaart de door de gebruiker bevestigde waarden.

## Functionele testgevallen

- Een vorige reis van een andere boot wordt niet gebruikt.
- Zonder vorige reis blijft motoruren start leeg als geen betrouwbare actuele waarde beschikbaar is.
- Reisduur wordt correct berekend op dezelfde dag en over middernacht.
- Een aankomsttijd vóór vertrek wordt niet zonder duidelijke correctie geaccepteerd.
- Een handmatig gewijzigde waarde blijft behouden wanneer voorstellen opnieuw worden geladen.
- Verschillen voor motoruren en logstand worden alleen berekend met geldige begin- en eindwaarden.
- Ontbrekende gegevens leveren een leeg voorstel en geen foutmelding op.

## Relatie met bestaande epic

Deze story verfijnt `Story 1 - Reis aanmaken en beheren` uit `.docs/epics/digital-logbook.md`.
