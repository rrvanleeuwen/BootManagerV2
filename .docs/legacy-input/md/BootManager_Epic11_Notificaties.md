# BootManager – Epic 11: Notificaties & Waarschuwingen

BootManager waarschuwt gebruikers automatisch voor belangrijke gebeurtenissen zoals lage voorraad, onderhoud of vervallen documenten.

## Belangrijkste functionaliteiten

- Push- en e-mailmeldingen

- Waarschuwingen bij lage voorraad, onderhoud, documenten

- Instelbare notificatiefrequentie per gebruiker

- Dashboardindicatoren en kleuren

- Logging van verzonden meldingen

## User Stories + Acceptatiecriteria

### US11.1 – Waarschuwing bij lage voorraad

Als eigenaar wil ik melding ontvangen bij lage voorraad.

Given dat minimumwaarden ingesteld zijn, When voorraad onder de drempel zakt, Then verstuurt BootManager melding.

### US11.2 – Documentvervalmelding

Als eigenaar wil ik melding ontvangen bij bijna verlopen document.

Given dat documenten vervaldatums hebben, When vervaldatum nadert, Then ontvangt de eigenaar melding.

### US11.3 – Onderhoudsherinnering

Als eigenaar wil ik herinnerd worden aan gepland onderhoud.

Given dat onderhoud gepland is, When datum nadert, Then toont BootManager waarschuwing.

### US11.4 – Passageplanning waarschuwing

Als gebruiker wil ik melding ontvangen als vertrekdatum dichtbij komt.

Given dat passage gepland is, When vertrekdatum binnen dagen valt, Then verstuurt BootManager melding.

### US11.5 – Instellingen voor notificaties beheren

Als eigenaar wil ik meldingstype kunnen kiezen.

Given dat notificaties bestaan, When gebruiker voorkeuren instelt, Then bewaart BootManager voorkeuren.

### US11.6 – Notificatiegeschiedenis bekijken

Als gebruiker wil ik overzicht van meldingen zien.

Given dat meldingen zijn verstuurd, When gebruiker logboek opent, Then toont BootManager meldingen.
