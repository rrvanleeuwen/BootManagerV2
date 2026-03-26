---
applyTo: "src/BootManager.Infrastructure/**/*.cs"
---

# Instructies voor BootManager.Infrastructure

Deze instructies gelden voor code in `BootManager.Infrastructure`.

## Rol van BootManager.Infrastructure
- `BootManager.Infrastructure` bevat technische implementaties van de oplossing.
- Gebruik deze laag voor persistence, EF Core configuraties, repository-implementaties en andere infrastructuurdetails.
- Houd deze laag technisch en ondersteunend aan de rest van de architectuur.

## Persistence en EF Core
- Plaats EF Core configuraties in de bestaande map `Persistence/Configurations`.
- Neem nieuwe entiteiten op in `BootManagerDbContext` op dezelfde manier als bestaande entiteiten.
- Sluit qua naamgeving aan op de bestaande conventie, bijvoorbeeld `<EntityName>Configuration`.
- Houd entity-configuratie overzichtelijk en technisch van aard.
- Maak alleen migrations wanneer daar expliciet om gevraagd wordt.

## Repositories
- Gebruik de bestaande generieke repository-structuur met `IRepository<T>` en `EfRepository<T>` als dat voldoende is.
- Voeg geen aparte repository-interface of repository-implementatie per entiteit toe als de generieke aanpak al volstaat.
- Introduceer alleen nieuwe infrastructuurinterfaces of implementaties wanneer daar een duidelijke architectonische reden voor is.

## Grenzen
- Plaats geen businesslogica in Infrastructure.
- Plaats geen webspecifieke code in Infrastructure.
- Voeg geen featuregerichte application-logica toe in Infrastructure.
- Laat deze laag ondersteunend blijven aan `Core`, `Application` en `Web`.

## Dependency Injection
- Registreer infrastructuurservices en implementaties op de bestaande manier in de daarvoor bestemde dependency injection code.
- Sluit qua stijl en naamgeving aan op de bestaande oplossing.

## Structuur en consistentie
- Respecteer de bestaande mapstructuur, zoals:
  - `Persistence`
  - `Persistence/Configurations`
  - `Repositories`
- Houd wijzigingen klein en lokaal.
- Voorkom onnodige refactors van bestaande infrastructuurcode.

## Documentatie
- Voeg op nieuwe of aangepaste code Nederlandse XML-documentatie toe op:
  - configuratieklassen
  - publieke methoden
  - relevante publieke members
- Voeg alleen korte Nederlandse inline comments toe waar technische configuratie of infrastructuurgedrag niet direct vanzelfsprekend is.

## Database-afspraken
- Als opslag via `BootManager.Web` moet lopen voor externe tools of processen, voeg dan geen directe omweg in Infrastructure toe om die afspraak te omzeilen.
- Respecteer de gekozen route van data-invoer in de architectuur.