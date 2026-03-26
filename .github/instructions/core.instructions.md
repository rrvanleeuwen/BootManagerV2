---
applyTo: "src/BootManager.Core/**/*.cs"
---

# Instructies voor BootManager.Core

Deze instructies gelden voor code in `BootManager.Core`.

## Rol van BootManager.Core
- `BootManager.Core` bevat de kern van de oplossing.
- Gebruik deze laag voor entiteiten, interfaces, value objects en andere domeinconcepten.
- Houd deze laag vrij van technische infrastructuurdetails, webspecifieke code en application-specifieke workflowlogica.

## Entiteiten
- Plaats entiteiten in de bestaande map `Entities`.
- Sluit qua naamgeving, opbouw en stijl aan op bestaande entiteiten in de oplossing.
- Houd entiteiten compact en domeingericht.
- Voeg alleen properties en gedrag toe die echt bij het domeinmodel horen.
- Vermijd technische of frameworkgerichte logica in entiteiten, tenzij dat al een bestaand patroon in de oplossing is.

## Interfaces
- Plaats generieke of domeinrelevante interfaces in de bestaande map `Interfaces`.
- Houd interfaces klein en doelgericht.
- Voeg geen infrastructuurimplementatie toe in Core.
- Respecteer de bestaande scheiding waarbij implementaties in `BootManager.Infrastructure` thuishoren.

## Domeingrenzen
- Plaats geen EF Core configuratie in Core.
- Plaats geen controllerlogica of HTTP-afhandeling in Core.
- Plaats geen application services in Core.
- Plaats geen directe databasecode in Core.
- Houd Core onafhankelijk van Infrastructure, Web en Tools.

## Structuur en consistentie
- Respecteer de bestaande mapstructuur, zoals:
  - `Entities`
  - `Interfaces`
- Houd wijzigingen klein en lokaal.
- Voorkom onnodige refactors van bestaande domeincode.

## Documentatie
- Voeg op nieuwe of aangepaste code Nederlandse XML-documentatie toe op:
  - entiteiten
  - interfaces
  - publieke methoden
  - relevante properties
- Voeg alleen korte Nederlandse inline comments toe waar het domeingedrag of een ontwerpkeuze niet direct vanzelfsprekend is.

## Ontwerpkeuzes
- Voeg geen extra abstracties toe zonder duidelijke reden.
- Houd het domeinmodel begrijpelijk en passend bij de bestaande oplossing.
- Als een generieke interface of bestaand patroon al volstaat, introduceer dan geen nieuwe structuur zonder expliciete noodzaak.