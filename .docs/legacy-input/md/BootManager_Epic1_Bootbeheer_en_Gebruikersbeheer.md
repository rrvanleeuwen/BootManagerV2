# BootManager – Epic 1: Bootbeheer & Gebruikersbeheer

## Doel

De basis van de applicatie: het aanmaken van een boot met alle eigenschappen, inclusief gebruikers, machtigingen en bootstructuur (gebieden en opslaglocaties).

## Belangrijkste functionaliteiten

\- Eerste opstart van de applicatie (initial setup).  
- Aanmaken van een bootprofiel met eigenschappen: naam, type, bouwjaar, afmetingen, foto’s.  
- Beheren van meerdere boten binnen één account.  
- Gebruikers aanmaken en rollen toewijzen (eigenaar, bemanning, alleen-lezen).  
- Opbouwen van de bootstructuur met gebieden en opslaglocaties.  
- Koppelen van QR-codes of tags aan opslaglocaties voor snelle herkenning.

## Uitgebreide User Stories met Acceptatiecriteria

### US1.1 – Eerste opstart en bootaanmaak

Als eigenaar wil ik bij de eerste opstart een bootprofiel kunnen aanmaken, zodat ik BootManager kan gebruiken voor mijn schip.

- Given dat de applicatie voor het eerst wordt gestart,  
  When de eigenaar het welkomstscherm ziet en op 'Nieuwe boot aanmaken' klikt,  
  Then wordt een wizard getoond waarmee bootgegevens kunnen worden ingevoerd (naam, type, bouwjaar, afmetingen, foto) en opgeslagen in de lokale database.

### US1.2 – Bootinformatie bewerken

Als eigenaar wil ik bootinformatie kunnen wijzigen of verwijderen, zodat mijn gegevens up-to-date blijven.

- Given dat er één of meer boten bestaan,  
  When de eigenaar op de bootinstellingen klikt,  
  Then kan hij eigenschappen aanpassen of de boot verwijderen (met bevestiging).

### US1.3 – Gebruikers aanmaken en rollen toewijzen

Als eigenaar wil ik extra gebruikers kunnen aanmaken en machtigingen toekennen, zodat mijn bemanning ook toegang heeft.

- Given dat de eigenaar is ingelogd,  
  When hij een nieuw gebruikersprofiel aanmaakt met een e-mailadres of gebruikersnaam,  
  Then kan hij kiezen tussen rollen (Eigenaar, Bemanning, Alleen-lezen) en toegang verlenen.

### US1.4 – Inloggen als bestaande gebruiker

Als bemanningslid of eigenaar wil ik kunnen inloggen, zodat ik mijn rolgebaseerde toegang krijg.

- Given dat een gebruiker reeds bestaat,  
  When de bemanning of eigenaar de pincode of het wachtwoord invoert,  
  Then opent BootManager met de functies die bij hun rol horen.

### US1.5 – Meerdere boten beheren

Als eigenaar wil ik meerdere boten kunnen beheren, zodat ik voor elk schip aparte gegevens heb.

- Given dat de eigenaar meer dan één boot in zijn profiel heeft,  
  When hij de bootselectie opent,  
  Then kan hij tussen boten wisselen en worden alle gegevens (voorraad, documenten, etc.) contextueel geladen.

### US1.6 – Boot selecteren bij opstart

Als bemanningslid of eigenaar wil ik bij het opstarten kunnen kiezen welke boot ik wil beheren, zodat ik de juiste context heb — tenzij er slechts één boot actief is, in dat geval wordt die automatisch geselecteerd.

- Given dat er één of meer boten geregistreerd zijn,  
  When BootManager wordt gestart,  
  Then  
  - als één boot actief is, wordt deze automatisch geselecteerd en geladen;  
  - als meerdere boten actief zijn, verschijnt een keuzescherm waarin de gebruiker de gewenste boot kan kiezen.

### US1.7 – Gebruikersrechten wijzigen

Als eigenaar wil ik rechten van bestaande gebruikers kunnen aanpassen, zodat ik de toegang kan beheren.

- Given dat een gebruiker al bestaat,  
  When de eigenaar de gebruikerslijst opent,  
  Then kan hij de rol wijzigen en de wijziging wordt direct actief.

### US1.8 – Gebruiker verwijderen

Als eigenaar wil ik een gebruiker kunnen verwijderen, zodat ongewenste toegang wordt voorkomen.

- Given dat de eigenaar is ingelogd,  
  When hij een gebruiker selecteert en verwijdert,  
  Then wordt deze verwijderd uit de lokale database en heeft geen toegang meer.

### US1.9 – Bootstructuurbeheer: Gebieden en Opslaglocaties

Als eigenaar wil ik mijn boot kunnen opdelen in gebieden en opslaglocaties, zodat ik overzicht heb waar spullen liggen.

- Given dat de eigenaar een bootprofiel heeft geopend,  
  When hij nieuwe gebieden toevoegt of bestaande aanpast,  
  Then worden deze opgeslagen in de structuur en kunnen opslaglocaties worden toegevoegd per gebied.

### US1.10 – Opslaglocatie aanmaken binnen gebied

Als eigenaar wil ik opslaglocaties kunnen toevoegen binnen een gebied, met een naam en een korte omschrijving, zodat ik precies weet waar iets ligt.

- Given dat een gebied geselecteerd is,  
  When de eigenaar op 'Nieuwe opslaglocatie' klikt en een naam en omschrijving invoert,  
  Then wordt deze locatie toegevoegd onder het geselecteerde gebied met beide gegevens opgeslagen in de database.

### US1.11 – Opslaglocatie bewerken

Als eigenaar wil ik een opslaglocatie kunnen bewerken, zodat ik de naam en omschrijving kan aanpassen wanneer de situatie aan boord verandert.

- Given dat een opslaglocatie bestaat,  
  When de eigenaar de bewerkfunctie opent en een nieuwe naam of omschrijving invoert,  
  Then worden de wijzigingen opgeslagen en direct zichtbaar in de bootstructuur.

### US1.12 – Tag genereren voor opslaglocatie

Als eigenaar wil ik voor elke opslaglocatie een unieke QR-code of tag kunnen genereren, zodat ik deze fysiek in de boot kan aanbrengen.

- Given dat er een opslaglocatie bestaat,  
  When de eigenaar op 'Genereer tag' klikt,  
  Then maakt het systeem een unieke QR-code aan met de locatie-ID en biedt de optie om deze te printen of te exporteren als afbeelding.

### US1.13 – Locatie openen via QR-code

Als bemanningslid of eigenaar wil ik een QR-code in de boot kunnen scannen, zodat ik direct de digitale opslaglocatie zie met de actuele voorraad.

- Given dat de QR-code is gekoppeld aan een bestaande opslaglocatie,  
  When de bemanning of eigenaar de code scant via een mobiel apparaat of camera,  
  Then opent BootManager automatisch de detailpagina van die opslaglocatie met de bijbehorende producten en aantallen.

### US1.14 – Tag opnieuw koppelen of vervangen

Als eigenaar wil ik een tag opnieuw kunnen koppelen of vervangen, zodat ik beschadigde of verplaatste QR-codes kan vernieuwen.

- Given dat een locatie al een tag heeft,  
  When de eigenaar op 'Tag vervangen' klikt,  
  Then wordt de oude koppeling ongeldig gemaakt en een nieuwe QR-code aangemaakt.

### US1.15 – Overzicht van alle tags

Als eigenaar wil ik een overzicht van alle gegenereerde tags kunnen zien, zodat ik weet welke locaties zijn voorzien van QR-codes.

- Given dat er meerdere opslaglocaties bestaan,  
  When de eigenaar het overzicht opent,  
  Then toont BootManager een lijst met locaties, hun ID’s en tagstatus (geprint, gekoppeld, vervangen).

### US1.16 – Bootgegevens exporteren/importeren (toekomst)

Als eigenaar wil ik mijn bootgegevens kunnen exporteren en later importeren, zodat ik mijn configuratie kan verplaatsen of herstellen.

- Given dat een boot bestaat,  
  When de eigenaar kiest voor exporteren,  
  Then wordt een back-upbestand (.json of .zip) aangemaakt met alle bootinstellingen, dat later via import kan worden teruggezet.

### US1.17 – Toekomstige cloud-bootselectie

Als bemanningslid of eigenaar wil ik op afstand een boot kunnen selecteren, zodat ik die kan bekijken of onderhouden zonder fysieke toegang tot de Raspberry Pi.

- Given dat cloud-synchronisatie actief is,  
  When de bemanning of eigenaar de webapp op afstand opent,  
  Then kan hij kiezen welke bootgegevens hij wil bekijken of bewerken.
