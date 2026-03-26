---
applyTo: "src/BootManager.Application/**/*.cs"
---

# Instructies voor BootManager.Application

Deze instructies gelden voor code in `BootManager.Application`.

## Structuur
- Werk feature-georiënteerd.
- Voeg nieuwe functionaliteit toe in een eigen feature-map onder `BootManager.Application`.
- Gebruik per feature minimaal:
  - een map `DTOs`
  - een map `Services`

## DTO's
- Gebruik DTO's voor invoer en uitvoer van use-cases.
- Houd DTO's klein en duidelijk.
- Laat DTO's aansluiten op de use-case, niet één-op-één op de database.

## Services
- Maak per feature een service-interface en een service-implementatie.
- Gebruik logische namen zoals `I<Feature>Service` en `<Feature>Service`.
- Gebruik constructor-injectie.
- Laat services samenwerken met bestaande Core-interfaces en infrastructuur, zoals `IRepository<T>`.

## Dependency Injection
- Registreer nieuwe application services in de bestaande `DependencyInjection.cs` van `BootManager.Application`.
- Sluit aan op de bestaande stijl van registreren.

## Grenzen
- Voeg geen EF Core configuratie toe in Application.
- Plaats geen webspecifieke code in Application.
- Voeg geen directe databasecode toe als bestaande abstrahering via Core/Infrastructure beschikbaar is.

## Documentatie
- Voeg op nieuwe of aangepaste code Nederlandse XML-documentatie toe op:
  - klassen
  - interfaces
  - publieke methoden
  - relevante properties
- Voeg alleen korte Nederlandse inline comments toe waar nodig om logica snel te kunnen herleiden.