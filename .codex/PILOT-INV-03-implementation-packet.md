# Implementation Packet

## Task

- Story ID: `PILOT-INV-03`
- Approved story: scan-gestuurde inruimflow met locatievoorstel
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-INV-03`
- Goal: breid de bestaande route `Scannen` uit zodat BootManager naast locatie-QR's ook productcodes kan verwerken, daarna een compacte scan-gestuurde inruimflow start met locatievoorstel, alternatieve locaties, handmatige fallback, onbekende-code-afhandeling en herstart van dezelfde scansessie, zonder bestaande locatie-QR-routing of handmatige inventory-basis uit `PILOT-INV-02` te breken.
- Required branch: `codex/pilot-inv-03-scan-inruimflow`

De story is al goedgekeurd. Formuleer haar niet opnieuw en vraag geen nieuw akkoord.
Geef een kort uitvoeringsplan, implementeer direct, voer de checks uit en rapporteer
volgens `Completion Notes`.

## Scope

- Laat de bestaande pagina `Scannen` zowel BootManager locatie-QR's als productcodes
  afhandelen.
- Behoud het bestaande gedrag waarbij een bekende locatie-QR direct naar de juiste
  locatiepagina navigeert.
- Herken een bekende productcode via de bestaande productcode-opslag en start dan een
  scan-gestuurde inruimflow voor dat product.
- Bepaal voor een bekend product de laatst gebruikte locatie op basis van de meest
  recente bestaande of aangevulde voorraadregel voor dat product.
- Toon de voorgestelde locatie altijd met leesbare gebied- en locatienaam.
- Toon daarnaast een kleine lijst met andere bekende locaties voor dat product, zonder
  dubbels en zonder interne identifiers in de UI.
- Als een product nog geen locatiegeschiedenis heeft, vraag direct om een locatie te
  kiezen of te scannen.
- Ondersteun twee manieren om een locatie te kiezen in deze flow:
  - bevestigen of kiezen uit een handmatige lijst;
  - een locatie-QR scannen binnen dezelfde flow.
- Laat na locatiekeuze alleen een hoeveelheid invoeren; de standaardeenheid van het
  product moet zichtbaar zijn maar niet wijzigbaar.
- Sla op via de bestaande additieve voorraadregels van `PILOT-INV-02`.
- Vraag na succesvol opslaan direct of de gebruiker nog een product wil scannen.
- Laat `Ja` terugkeren naar de scanner in dezelfde sessie en `Nee` eindigen op de
  locatiepagina waar het product is weggelegd.
- Als een gescande productcode onbekend is, bied in dezelfde scanflow precies deze
  keuzes:
  - nieuw product aanmaken;
  - gescande code koppelen aan bestaand product;
  - annuleren.
- Laat nieuw product aanmaken plaatsvinden in een modaal venster binnen de scanflow,
  met de gescande code vooraf ingevuld maar bewerkbaar.
- Laat na nieuw product aanmaken of code koppelen de inruimflow direct doorgaan naar
  locatie en hoeveelheid.

## Outside Scope

- Geen nieuwe hoofdroute buiten het bestaande menu `Scannen`.
- Geen brede redesign van de scanpagina, inventory-pagina's of algemene layout.
- Geen verbruik, correcties, overschrijven van voorraad, mutatiehistorie of batchacties.
- Geen externe productherkenning, EAN-database, AI-herkenning of internetlookup.
- Geen volledige productbeheerervaring buiten wat minimaal nodig is voor onbekende codes
  in deze flow.
- Geen documentatie-, commit-, push-, branch-, PR-, merge-, release- of
  deploymentacties.

## Expected Write-Set

Wijzig alleen deze bestanden of modules, tenzij een noodzakelijke compile-time
dependency wordt ontdekt:

- `BootManager.Application/Inventory/Contracts/IProductService.cs`;
- `BootManager.Application/Inventory/Contracts/IStockService.cs`;
- `BootManager.Application/Inventory/Services/ProductService.cs`;
- `BootManager.Application/Inventory/Services/StockService.cs`;
- optioneel kleine nieuwe DTO's of result-types onder
  `BootManager.Application/Inventory/DTOs/` of `.../Results/`;
- `BootManager.Web/Components/Pages/Scan.razor`;
- optioneel één of enkele kleine nieuwe inventory-componenten onder
  `BootManager.Web/Components/Inventory/` voor de scanflow;
- gerichte tests onder `BootManager.UnitTests/Storage/`,
  `BootManager.UnitTests/Inventory/` en
  `BootManager.IntegrationTests/Inventory/`.

Wijzig geen storage-QR-format, geen auth-inrichting, geen algemene routingstructuur en
geen ongerelateerde inventory- of storagepagina's zonder vooraf uit te leggen waarom
dat nodig is.

## Execution Boundaries

- Implementeer alleen applicatiecode, presentatiecode en tests die dit packet
  expliciet vereist.
- Controleer vóór bewerken dat de actieve branch exact
  `codex/pilot-inv-03-scan-inruimflow` is en niet `master`. Rapporteer `not ready`
  als dat niet zo is.
- Wijzig geen story-, release-, TODO-, legacy-, README-, handoff- of andere
  projectdocumentatie.
- Maak geen commit, push, branch, PR, merge, release of deployment.
- Houd de scanflow compact en taakgericht. Voeg geen generiek workflow-framework of
  grote state-machine-infrastructuur toe als een eenvoudige lokale componentopzet
  volstaat.
- Gebruik de bestaande product-, productcode-, locatie- en voorraadopslag als bron van
  waarheid. Introduceer geen parallelle scan- of inventory-opslag.
- Noem de story nooit `Done`, geaccepteerd of productierijp. Meld alleen
  `ready for Codex review` wanneer de technische completion definition volledig is
  gehaald.

## Minimal Context

Lees:

- `CLAUDE.md`;
- `.codex/PILOT-INV-03-implementation-packet.md`;
- alleen de sectie `PILOT-INV-03` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Inventory/AddStockDialog.razor`;
- `BootManager.Application/Inventory/Contracts/IProductService.cs`;
- `BootManager.Application/Inventory/Contracts/IStockService.cs`;
- `BootManager.Application/Inventory/Services/ProductService.cs`;
- `BootManager.Application/Inventory/Services/StockService.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`;
- `BootManager.UnitTests/Inventory/ProductServiceTests.cs`;
- `BootManager.UnitTests/Inventory/StockServiceTests.cs`;
- `BootManager.IntegrationTests/Inventory/StockMigrationTests.cs`.

Gebruik gerichte zoekopdrachten en kleine bestandssecties. Lees niet standaard:

- de volledige `.docs/TODO.md` of andere releaseverhalen;
- `.docs/legacy-analysis/` of `.docs/legacy-input/`;
- `.codex/current-session-handoff.md` of `.codex/working-agreement.md`;
- repositorybrede source trees.

## Existing Constraints

- Volg .NET 8 en de Clean Architecture-regels in `CLAUDE.md`.
- `Scan.razor` navigeert nu direct bij gekoppelde locatie-QR's; dit gedrag moet intact
  blijven.
- Productcodes bestaan al als unieke koppeling per product via `ProductService`; bouw
  daarop voort in plaats van een nieuwe code-opslag te introduceren.
- Voorraadtoevoeging gebruikt nu additief gedrag via `StockService.AddOrIncrementStockAsync`;
  de scanflow moet exact daarop landen.
- De laatst gebruikte locatie moet feitelijk gebaseerd zijn op bestaande
  voorraadgegevens, niet op een losse voorkeursinstelling of dummy-cache.
- UI mag functioneel en klein blijven. Een eenvoudige modale of conditionele flow op de
  scanpagina is voldoende.

## Acceptance Focus

- Een bekende locatie-QR blijft direct de locatiepagina openen.
- Een bekende productcode start direct de inruimflow.
- De flow toont voor bekende producten de laatst gebruikte locatie en eventuele andere
  bekende locaties als leesbare opties.
- De gebruiker kan een locatie bevestigen, handmatig kiezen of via scan kiezen.
- Een onbekende productcode kan binnen dezelfde flow leiden tot nieuw product of code
  koppelen, waarna de inruimflow direct doorgaat.
- Opslaan blijft additief werken conform `PILOT-INV-02`.
- Na opslaan werkt de keuze `Ja/Nee` voor dezelfde scansessie respectievelijk terug naar
  scanner of door naar de gebruikte locatiepagina.

## Test Evidence Requirements

Voeg defectgevoelige tests toe die echte productcode/componenten uitvoeren en concreet
bewijzen:

- `Scan.razor` onderscheidt locatie-QR, bekende productcode en onbekende productcode
  correct;
- een bekende productcode opent niet de locatie-QR-flow maar de inventory-scanflow;
- de voorgestelde locatie werkelijk gebaseerd is op de meest recente bestaande
  voorraadregel voor dat product;
- alternatieve locaties zonder dubbels worden teruggegeven met leesbare namen;
- opslaan vanuit de scanflow de bestaande additieve voorraadservice correct gebruikt;
- een onbekende productcode kan tot nieuw product aanmaken of code koppelen leiden,
  waarna dezelfde scanflow doorgaat;
- `Ja` na opslaan de scanner opnieuw activeert en `Nee` naar de gebruikte locatiepagina
  navigeert;
- bestaand locatie-QR-gedrag voor Owner en Crew niet regressief is.

Inspecteer iedere nieuwe of gewijzigde test: geen `Assert.True(true)`, lege test,
bronvormtest als vervanging van gedrag of `async` test zonder relevante `await`.

Deze story is geen bugfix, dus formeel red-green-bewijs is niet verplicht. Als je een
bestaand defect tegenkomt en meeneemt, lever daarvoor alsnog expliciet red-green of
gelijkwaardig bewijs.

## Required Checks

Voer eerst gerichte checks uit, bijvoorbeeld:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ScanComponentTests|FullyQualifiedName~ProductServiceTests|FullyQualifiedName~StockServiceTests"
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~Inventory"
```

Voer daarna uit:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

Als je voor deze story extra componenttests toevoegt met een nieuwe klassenaam, neem die
dan ook expliciet op in de gerichte testrun.

## Definition of Technical Completion

Meld uitsluitend `ready for Codex review` wanneer:

- ieder scopepunt en acceptatiecriterium technisch is geïmplementeerd;
- bekende locatie-QR en bekende productcode aantoonbaar verschillende juiste flows
  starten;
- locatievoorstel en alternatieve locaties op echte voorraaddata zijn gebaseerd;
- onbekende productcode binnen dezelfde flow naar nieuw product of codekoppeling kan
  leiden;
- alle gerichte tests slagen en alle nieuwe of gewijzigde tests echte productcode
  uitvoeren;
- build en `git diff --check` slagen;
- geen onverklaarde wijziging buiten de verwachte write-set staat;
- resterende handmatige acceptatiestappen expliciet zijn vermeld.

Meld `not ready` wanneer scope onvolledig is, locatievoorstel niet op echte data
berust, onbekende-code-afhandeling niet rond is, een nieuwe of gewijzigde test faalt,
build/diffcheck faalt, een vereiste beslissing ontbreekt of extra write-area niet kan
worden verantwoord.

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden en geïmplementeerd gedrag;
2. tests/checks en resultaten;
3. exacte nieuwe/gewijzigde testnamen en welk productiegedrag zij uitvoeren;
4. migratie-, package- of configuratie-impact;
5. resterende risico's en exacte handmatige testvereisten;
6. eindstatus: `ready for Codex review` of `not ready`, met concrete reden.
