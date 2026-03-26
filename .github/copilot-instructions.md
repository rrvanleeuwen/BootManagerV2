# BootManager repository-brede Copilot-instructies

Deze instructies gelden voor de hele BootManager-repository.

## Algemene uitgangspunten
- Gebruik altijd .NET 8.
- Respecteer de bestaande solutionstructuur en naamgeving.
- Nieuwe projecten, namespaces en bestandsnamen moeten aansluiten op de bestaande `BootManager.*` conventie.
- Werk in kleine, gecontroleerde stappen die na elke stap moeten kunnen builden en runnen.
- Voorkom grote refactors tenzij daar expliciet om gevraagd wordt.
- Sluit aan op bestaande code, patronen en mapstructuur in plaats van nieuwe patronen te introduceren.

## Architectuur
- Respecteer de bestaande laagindeling:
  - `BootManager.Core` voor entiteiten, interfaces en value objects
  - `BootManager.Application` voor feature-mappen met `DTOs` en `Services`
  - `BootManager.Infrastructure` voor EF Core persistence, configuraties, repositories en technische implementaties
  - `BootManager.Web` voor web/API toegang
  - `BootManager.Tools.*` voor losse tools zoals simulator of ingest
- Voeg nieuwe code toe in de bestaande laag waar deze logisch hoort.
- Gebruik de bestaande generieke repository-structuur met `IRepository<T>` en `EfRepository<T>` als dat past bij de use-case.
- Maak geen extra repositories per entiteit als de bestaande generieke repository volstaat.

## Database en opslag
- Als data door externe processen of tools moet worden opgeslagen, gebruik daarvoor de route via `BootManager.Web` als dat de afgesproken architectuur is.
- Laat tools zoals ingest-processen niet rechtstreeks naar de database schrijven als opslag via de Web API hoort te lopen.
- Maak migrations alleen wanneer daar expliciet om gevraagd wordt.

## Coding style
- Houd controllers dun.
- Houd services overzichtelijk en taakgericht.
- Voeg geen onnodige abstracties of over-engineering toe.
- Gebruik constructor-injectie en dependency injection op een manier die aansluit bij de bestaande solution.
- Houd wijzigingen klein en lokaal.

## Documentatie
- Voeg bij nieuwe of aangepaste code, waar relevant, altijd Nederlandse XML-documentatie toe.
- Voeg alleen korte Nederlandse inline comments toe waar de logica niet direct vanzelfsprekend is.
- Verander geen functionele code alleen om documentatie mooier te maken.

## Werkwijze
- Voer wijzigingen stapsgewijs uit.
- Na iedere stap moet de oplossing kunnen builden.
- Geef bij grotere wijzigingen kort aan welke bestanden zijn toegevoegd of aangepast.