# BootManager – Epic 6: Onderhoudsbeheer (Technisch Logboek)

Dit document bevat de volledige beschrijving van Epic 6 van het BootManager-project. De Epic behandelt het technisch logboek waarmee gebruikers onderhoudstaken kunnen plannen, uitvoeren en opvolgen. Zo blijft duidelijk wat wanneer is gedaan, wat binnenkort moet gebeuren en welke onderdelen of kosten daarbij horen.

## Doel

Gebruikers (voornamelijk de eigenaar) kunnen onderhoudstaken aan de boot plannen, uitvoeren en opvolgen. Zo blijft duidelijk wat wanneer is gedaan, wat binnenkort moet gebeuren en welke onderdelen of kosten daarmee gepaard gaan.

## Belangrijkste functionaliteiten

- Onderhoudstaken aanmaken (handmatig of automatisch op interval: tijd, draaiuren of vaarkilometers)

- Koppeling aan onderdelen (motor, romp, zeilen, accu’s, elektronica, filters, etc.)

- Prioriteit, status en verwachte uitvoerdatum beheer per taak

- Vastleggen van uitgevoerd onderhoud met datum, omschrijving, kosten, monteur en bijlagen

- Automatische herinneringen op basis van interval of gebruiksdata

- Dashboard met openstaande, binnenkort vervallende en uitgevoerde taken

- Koppeling met Documentbeheer (Epic 4) voor facturen en handleidingen

- Rapportage / export naar PDF of CSV

- Volledig offline gebruik en cloudsynchronisatie

## User Stories + Acceptatiecriteria

### US6.1 – Onderhoudstaak aanmaken

Als eigenaar wil ik een onderhoudstaak kunnen aanmaken met naam, beschrijving, onderdeel, prioriteit en interval (tijd of draaiuren), zodat ik gepland onderhoud kan bijhouden.

Given dat de eigenaar is ingelogd, When hij een nieuwe onderhoudstaak toevoegt met de benodigde velden, Then wordt deze taak opgeslagen en verschijnt ze in de lijst van geplande taken.

### US6.2 – Onderhoud koppelen aan onderdeel

Als eigenaar wil ik onderhoudstaken kunnen koppelen aan specifieke bootonderdelen (motor, tuigage, romp, elektronica, etc.), zodat ik per onderdeel een onderhoudshistoriek heb.

Given dat onderdelen bestaan, When de eigenaar een taak aanmaakt of wijzigt, Then kan hij één of meerdere onderdelen selecteren waarop de taak van toepassing is.

### US6.3 – Onderhoud op interval (planning)

Als eigenaar wil ik onderhoud kunnen plannen op tijds- of gebruiksintervallen, zodat BootManager mij herinnert wanneer het onderhoud nodig is.

Given dat een taak een interval heeft (bijv. elke 6 maanden of na 100 motoruren), When de eigenaar deze instelt, Then berekent BootManager automatisch de volgende uitvoerdatum en toont die in het dashboard.

### US6.4 – Automatische herinneringen en waarschuwingen

Als eigenaar wil ik automatische waarschuwingen ontvangen bij aankomend of achterstallig onderhoud, zodat ik tijdig actie kan ondernemen.

Given dat onderhoudstaken met vervaldatum bestaan, When de huidige datum de herinneringstermijn bereikt, Then toont BootManager een melding in het dashboard en markeert de taak als ‘bijna vervallen’.

### US6.5 – Uitgevoerd onderhoud registreren

Als eigenaar wil ik uitgevoerd onderhoud kunnen registreren met datum, beschrijving, kosten, monteur en gebruikte onderdelen, zodat ik kan bijhouden wat er is gedaan.

Given dat een onderhoudstaak bestaat, When de gebruiker op ‘Markeer als uitgevoerd’ klikt en de gegevens invoert, Then wordt de taak verplaatst naar de historiek met alle details.

### US6.6 – Bijlagen toevoegen aan onderhoud

Als eigenaar wil ik foto’s of documenten (zoals facturen of handleidingen) kunnen koppelen aan een onderhoudsvermelding, zodat ik bewijs en referentie heb.

Given dat onderhoud is uitgevoerd, When de gebruiker een bestand uploadt (PDF, JPG, PNG, DOCX), Then wordt de bijlage opgeslagen en zichtbaar bij de onderhoudsdetails.

### US6.7 – Onderhoud wijzigen of verwijderen

Als eigenaar wil ik onderhoudstaken kunnen bewerken of verwijderen, zodat ik mijn onderhoudsplanning actueel houd.

Given dat onderhoudstaken bestaan, When de eigenaar een taak bewerkt of verwijdert, Then worden de wijzigingen opgeslagen of de taak na bevestiging verwijderd.

### US6.8 – Onderhoudshistoriek per onderdeel

Als eigenaar wil ik de onderhoudshistoriek per onderdeel kunnen bekijken, zodat ik weet wat er in het verleden is uitgevoerd.

Given dat onderdelen onderhoudsrecords hebben, When de gebruiker een onderdeel selecteert, Then toont BootManager alle gerelateerde uitgevoerde taken met datum en details.

### US6.9 – Dashboard met onderhoudsstatus

Als eigenaar wil ik op een dashboard zien hoeveel onderhoud gepland, bijna vervallen en uitgevoerd is, zodat ik overzicht houd over de staat van mijn boot.

Given dat onderhoudstaken bestaan, When het dashboard wordt geopend, Then toont BootManager een samenvatting van taken per status en onderdeel.

### US6.10 – Zoeken en filteren

Als eigenaar wil ik onderhoudstaken kunnen filteren op status, onderdeel of periode, zodat ik snel specifieke informatie vind.

Given dat er meerdere onderhoudstaken zijn, When de gebruiker filters toepast of een zoekterm invoert, Then toont BootManager alleen de relevante taken.

### US6.11 – Exporteren van onderhoudslogboek

Als eigenaar wil ik mijn onderhoudslogboek kunnen exporteren naar PDF of CSV, zodat ik dit kan delen of bewaren.

Given dat onderhoudsgegevens bestaan, When de gebruiker kiest voor export, Then genereert BootManager een bestand met alle relevante taken, kosten en onderdelen.

### US6.12 – Koppeling met Documentbeheer

Als eigenaar wil ik documenten (facturen, handleidingen, certificaten) kunnen koppelen aan onderhoudsvermeldingen, zodat ik alles centraal kan beheren.

Given dat Documentbeheer (Epic 4) actief is, When de gebruiker documenten koppelt aan onderhoud, Then worden de bestanden gelinkt en zichtbaar bij de taak.

### US6.13 – Offline werking

Als eigenaar wil ik onderhoudstaken offline kunnen beheren, zodat ik ook zonder internet toegang heb tot alle gegevens.

Given dat BootManager offline draait, When de gebruiker taken aanmaakt of wijzigt, Then worden deze lokaal opgeslagen en later gesynchroniseerd.

### US6.14 – Cloud-synchronisatie

Als eigenaar wil ik dat mijn onderhoudslogboek automatisch wordt gesynchroniseerd zodra er internetverbinding is, zodat ik overal over actuele data beschik.

Given dat cloud-synchronisatie is ingeschakeld, When BootManager online komt, Then worden alle onderhoudsgegevens gesynchroniseerd met de cloud en eventueel beschikbaar op andere apparaten.
