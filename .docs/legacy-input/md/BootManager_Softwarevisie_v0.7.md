# ⚓️ BootManager – Softwarevisie v0.7

## 1. Doel van de applicatie

BootManager is een integraal boordbeheersysteem voor zeil- en motorboten.  
De applicatie ondersteunt booteigenaren bij het beheren van alle essentiële zaken aan boord:  
- Voorraadbeheer – weten wat waar ligt  
- Passageplanning – berekenen van benodigde voorraden voor een reis  
- Documentbeheer – bewaren van certificaten, verzekeringen en vergunningen  
- Reislogboek – registreren van vaartochten en omstandigheden  
- Onderhoudsbeheer – vastleggen van onderhoud en herinneringen  
BootManager draait volledig offline, bijvoorbeeld op een Raspberry Pi of laptop via een lokale Blazor WebApp (Interactive Server).  
Het doel is om alle operationele en administratieve informatie van de boot te centraliseren, zodat de gebruiker altijd overzicht houdt.

## 2. Gebruikers en doelgroep

Primaire gebruiker: booteigenaar of vaste schipper.  
Secundaire gebruikers: partner of bemanning.  
Technisch beheerder: optioneel, verantwoordelijk voor installatie en back-ups.  
Gebruikers werken vaak zonder internetverbinding, hebben behoefte aan snelle toegang tot informatie en willen één systeem voor administratie, planning en onderhoud.

## 3. Hoofdfuncties (uitgebreid)

3.1 Boot- en locatiebeheer  
- Ondersteuning voor meerdere boten.  
- Bootgegevens: naam, type, bouwjaar, lengte, breedte, diepgang, foto’s.  
- Indeling in gebieden (kajuit, kombuis, machinekamer, voorpunt, enz.).  
- Binnen elk gebied één of meer opslagruimten (kast, lade, bak).  
- Mogelijkheid tot visuele weergave van de bootindeling.  
  
3.2 Voorraadbeheer  
- Registratie van producten met naam, categorie, hoeveelheid, eenheid, houdbaarheidsdatum.  
- Locatie (gebied + opslagruimte).  
- Foto, barcode of QR-code.  
- Waarschuwingen bij lage voorraad of verlopen producten.  
- Filters op categorie, locatie of status.  
- Export naar CSV/Excel.  
  
3.3 Passageplanning  
- Invoer van reisduur en aantal bemanningsleden.  
- Opgeven van gemiddeld verbruik per persoon per dag.  
- Automatische berekening van benodigde hoeveelheden.  
- Vergelijking met actuele voorraad → inkooplijst genereren.  
- Opslaan, kopiëren en hergebruiken van plannen.  
  
3.4 Documentbeheer  
- Opslag van documenten (PDF, JPG, PNG) met titel, type, beschrijving, vervaldatum, gekoppelde boot.  
- Notificaties bij naderende vervaldatum.  
- Bestanden lokaal versleuteld opgeslagen.  
- Categorieën: verzekeringen, certificaten, vergunningen, keuringen.  
  
3.5 Reislogboek  
- Handmatig of automatisch logregels aanmaken.  
- Tijdstip, positie, koers, snelheid, windrichting, windkracht, diepte.  
- Ondersteuning voor meerdere reizen.  
- Export naar PDF of CSV.  
  
3.6 Onderhoudsbeheer  
- Registratie van uitgevoerd onderhoud: datum, omschrijving, kosten, onderdelen, uitvoerder.  
- Onderhoudsschema’s per onderdeel.  
- Herinneringen op basis van datum of draaiuren.  
- Historisch overzicht en integratie met documenten.  
  
3.7 Gebruikersbeheer  
- Rollen: eigenaar, bemanning, alleen-lezen.  
- Lokale login of pincode.  
- Gastmodus voor tijdelijke gebruikers.  
  
3.8 Offline werking  
- Draait volledig lokaal op Raspberry Pi of laptop.  
- SQLite-database voor opslag.  
- Back-up naar USB-stick of netwerkschijf.  
- Geen internetverbinding vereist.

## 4. Architectuur en technologie

Frontend: Blazor WebApp (.NET 8, Interactive Server)  
Backend: ASP.NET Core (.NET 8)  
Database: SQLite  
ORM: Entity Framework Core  
Architectuurstijl: Clean Architecture + Repository Pattern + CQRS (Feature-gebaseerd)  
Lagen: Core → Application → Infrastructure → Presentation  
Hosting: Raspberry Pi (Kestrel Server)  
Logging: Serilog  
Testing: xUnit  
Security: Lokale encryptie voor documenten en gebruikersdata

## 4.1 Clean Architecture

Vier lagen met duidelijke verantwoordelijkheden:  
1. Core: domeinmodellen, value objects, interfaces (IRepository\<T\>).  
2. Application: businesslogica, CQRS-handlers, validatie, DTO’s.  
3. Infrastructure: datatoegang, repositories, bestandsbeheer.  
4. Presentation: Blazor-UI, componenten, services, navigatie.  
Voordelen: testbaarheid, onderhoudbaarheid en uitbreidbaarheid.

## 4.2 Feature-gebaseerde CQRS-structuur

CQRS is per domeinfeature georganiseerd. Elke feature heeft eigen commands, queries, handlers en DTO’s.  
Voorbeeldstructuur:  
/Application  
/Inventory  
/Commands  
/Queries  
/Handlers  
/DTOs  
/Validators  
/PassagePlanning  
/Documents  
/Logbook  
/Maintenance  
/Users  
  
Voordelen:  
- Schaalbaar en overzichtelijk.  
- Duidelijke eigenaarschap per domein.  
- Team-friendly ontwikkeling.  
- Aansluiting op Blazor-navigatie (RBN-principe).

## 4.3 Architectuuroverzicht (schematisch)

Presentation Layer → Blazor Components, Pages, Navigation  
Application Layer → CQRS Handlers per Feature, DTOs, Validators  
Infrastructure Layer → Repositories, EF Core, File Storage, Logging  
Core → Entities, Interfaces, Value Objects

## 5. Toekomstige uitbreidingen

\- Cloud-synchronisatie en mobiele app (.NET MAUI / Blazor Hybrid)  
- AI-onderhoudsadviezen en voorraadvoorspellingen  
- Integratie met NMEA2000-data (positie, motorinformatie)  
- Rapportages in PDF of Excel  
- Push-meldingen bij verlopen documenten of onderhoud

## 6. Niet-functionele eisen

\- Offline bruikbaar  
- Snelle zoek- en filterfuncties  
- Eenvoudige back-ups  
- Responsive design voor tablet en mobiel  
- Versleutelde lokale opslag  
- Gebruiksvriendelijke UI  
- Minimale installatie (kopie + starten)

## 7. Volgende stap

Na goedkeuring van BootManager v0.7:  
1. Scrum-Epics definiëren:  
- Inventarisbeheer  
- Passageplanning  
- Documentbeheer  
- Reislogboek  
- Onderhoudsbeheer  
- Offlinegebruik & Beveiliging  
2. Per Epic concrete user stories opstellen.  
3. Implementatie starten met Inventory-feature als MVP.
