# BootManager – Epic 0: Installatie & Authenticatie

## Doel

De gebruiker kan BootManager voor het eerst installeren, een eigenaarprofiel aanmaken en veilig inloggen.

## Belangrijkste functionaliteiten

\- Eerste installatie en configuratie van BootManager op Raspberry Pi of laptop.  
- Aanmaken van het eerste eigenaarprofiel (registratie met naam, e-mail, wachtwoord/pincode).  
- Lokale versleuteling van login-gegevens.  
- Inlogproces voor de eigenaar bij volgende sessies.  
- Beheer van inloggegevens (wijzigen, herstel via back-upcode).

## User Stories met Acceptatiecriteria

### US0.1 – Installatie uitvoeren

Als nieuwe gebruiker wil ik BootManager kunnen installeren, zodat ik het op mijn boot kan gebruiken.

- Given dat de installatiebestanden aanwezig zijn op de Raspberry Pi,  
  When ik het installatieproces start,  
  Then wordt de webapp lokaal geïnstalleerd en kan ik de setup starten.

### US0.2 – Registratie eerste eigenaar

Als nieuwe gebruiker wil ik een eigenaarprofiel kunnen aanmaken, zodat ik persoonlijke toegang krijg tot BootManager.

- Given dat BootManager voor het eerst wordt gestart en geen eigenaarprofiel bestaat,  
  When de gebruiker op 'Registreren als eigenaar' klikt en zijn gegevens invoert,  
  Then wordt een nieuw eigenaarprofiel aangemaakt en lokaal versleuteld opgeslagen.

### US0.3 – Inloggen als eigenaar

Als eigenaar wil ik kunnen inloggen, zodat mijn gegevens en instellingen beschermd zijn.

- Given dat er een eigenaarprofiel bestaat,  
  When ik mijn pincode of wachtwoord invoer,  
  Then opent de applicatie en worden mijn persoonlijke boten geladen.

### US0.4 – Wachtwoord of pincode wijzigen

Als eigenaar wil ik mijn inloggegevens kunnen wijzigen, zodat ik mijn beveiliging kan bijwerken.

- Given dat ik ben ingelogd,  
  When ik via instellingen mijn pincode/wachtwoord aanpas,  
  Then wordt de nieuwe code versleuteld opgeslagen en is de oude ongeldig.

### US0.5 – Herstel van toegang

Als eigenaar wil ik mijn toegang kunnen herstellen als ik mijn wachtwoord ben vergeten, zodat ik mijn data niet verlies.

- Given dat ik mijn wachtwoord niet meer weet,  
  When ik kies voor 'Herstel via back-upcode' of 'Beheersleutel importeren',  
  Then wordt mijn toegang hersteld en mijn profiel opnieuw actief.

### US0.6 – Eigenaarprofiel beheren

Als eigenaar wil ik mijn persoonlijke gegevens kunnen wijzigen, zodat mijn profiel actueel blijft.

- Given dat ik ben ingelogd,  
  When ik mijn profielgegevens open,  
  Then kan ik naam, e-mail of voorkeuren aanpassen en opslaan.
