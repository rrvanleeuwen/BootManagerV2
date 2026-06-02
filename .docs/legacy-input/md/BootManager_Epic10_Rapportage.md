# BootManager – Epic 10: Rapportage & Analyse

BootManager biedt inzicht in trends, verbruik, onderhoudsintervallen en vaargedrag door rapportages te genereren (PDF, CSV, dashboardgrafieken).

## Belangrijkste functionaliteiten

- Statistieken over brandstof-, water- en voedselverbruik

- Overzicht van motoruren, onderhoudskosten, passages per periode

- Analyse van voorraadverloop

- Export naar PDF en CSV

- Grafieken en visuele dashboards

- Kostenanalyse per tocht of per categorie

## User Stories + Acceptatiecriteria

### US10.1 – Brandstofanalyse

Als eigenaar wil ik brandstofverbruik per passage kunnen zien.

Given dat motoruren geregistreerd zijn, When de gebruiker een periode kiest, Then toont BootManager het verbruik per tocht.

### US10.2 – Voorraadanalyse

Als eigenaar wil ik zien hoe de voorraad zich ontwikkelt over tijd.

Given dat voorraadmutaties zijn geregistreerd, When de gebruiker de analysepagina opent, Then toont BootManager trends per categorie.

### US10.3 – Onderhoudsrapportage

Als eigenaar wil ik overzicht van uitgevoerd en gepland onderhoud.

Given dat onderhoudsdata beschikbaar is, When de gebruiker rapportage opent, Then toont BootManager taken met datum en kosten.

### US10.4 – Kostenanalyse per tocht

Als eigenaar wil ik totale kosten per passage kunnen berekenen.

Given dat kosten zijn geregistreerd, When de gebruiker een tocht selecteert, Then toont BootManager totale en gemiddelde kosten.

### US10.5 – Export naar PDF/CSV

Als gebruiker wil ik rapportages kunnen exporteren.

Given dat een rapportage is aangemaakt, When de gebruiker kiest voor export, Then wordt een bestand aangemaakt.

### US10.6 – Visuele trends en grafieken

Als gebruiker wil ik data kunnen bekijken in grafieken.

Given dat historische data beschikbaar is, When de gebruiker de rapportagepagina opent, Then toont BootManager interactieve grafieken.
