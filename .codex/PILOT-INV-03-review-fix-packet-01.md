# Review Fix Packet

## Task

- Story ID: `PILOT-INV-03`
- Base packet: `.codex/PILOT-INV-03-implementation-packet.md`
- Required branch: `codex/pilot-inv-03-scan-inruimflow`
- Goal: herstel de drie functionele gaten die in Codex-review zijn gevonden in de
  scan-gestuurde inruimflow, zonder de bestaande locatie-QR-flow of de al toegevoegde
  productcodeherkenning te verbreden.

Gebruik dit als gerichte correctieronde op de bestaande implementatie. Geen nieuwe
story-interpretatie, geen brede refactor.

## Defects To Fix

1. Onbekende productcode krijgt nog niet de verplichte keuzes `nieuw product`,
   `code koppelen` en `annuleren` binnen dezelfde scanflow.
2. Handmatige locatiekeuze is niet beschikbaar wanneer er al een voorgestelde locatie
   of alternatieve locaties bestaan.
3. De paden `Locatie scannen` en `Ja, nog een` zetten alleen status terug, maar starten
   de scanner niet echt opnieuw; bovendien verliest `Locatie scannen` de inventory-flow
   context waardoor een gescande locatie naar de detailpagina navigeert in plaats van
   als gekozen locatie door te gaan.

## Scope

- Voeg binnen `Scan.razor` expliciete onbekende-productcode-afhandeling toe:
  - `Nieuw product`;
  - `Code koppelen aan bestaand product`;
  - `Annuleren`.
- Laat `Nieuw product` in dezelfde scanflow een compacte modal of vergelijkbare
  ingebedde invoer openen met de gescande code vooraf ingevuld maar bewerkbaar.
- Laat `Code koppelen aan bestaand product` in dezelfde scanflow een bestaand product
  kiezen en daarna de gescande code eraan koppelen met bestaande application-services.
- Laat na beide acties de inventory-flow direct doorgaan naar locatiekeuze en
  hoeveelheid.
- Maak handmatige locatiekeuze altijd beschikbaar als fallback, ook wanneer er al een
  voorgestelde of alternatieve locatie bestaat.
- Herstel `Locatie scannen` zodat de scanner echt opnieuw start binnen dezelfde sessie
  en de inventory-flowcontext behouden blijft.
- Herstel `Ja, nog een` zodat de scanner echt opnieuw start binnen dezelfde sessie.
- Zorg dat een locatie-QR die tijdens locatiekeuze wordt gescand de gekozen locatie in
  de flow invult en niet naar de locatie-detailpagina navigeert.

## Outside Scope

- Geen wijziging van de storyscope buiten bovenstaande defects.
- Geen documentatie-updates.
- Geen commit, push, branch, PR, merge of deployment.
- Geen extra migraties of datamodelverbreding tenzij strikt noodzakelijk voor compile of
  bestaand gedrag.

## Expected Write-Set

Wijzig alleen wat nodig is in:

- `BootManager.Web/Components/Pages/Scan.razor`;
- optioneel een kleine extra component onder `BootManager.Web/Components/Inventory/`
  als dat de onbekende-code-flow compacter maakt;
- `BootManager.Application/Inventory/Contracts/IProductService.cs` alleen als een
  kleine aanvullende methode echt nodig is;
- `BootManager.Application/Inventory/Services/ProductService.cs` alleen voor de
  minimaal benodigde ondersteuning van code koppelen of productaanmaak vanuit deze flow;
- gerichte tests onder `BootManager.UnitTests/Storage/` en eventueel
  `BootManager.UnitTests/Inventory/`.

Wijzig geen storage-QR-format, geen algemene scanmodule-JavaScript en geen
ongerelateerde inventory- of storagepagina's zonder noodzaak.

## Execution Boundaries

- Controleer vóór bewerken dat de actieve branch exact
  `codex/pilot-inv-03-scan-inruimflow` is en niet `master`.
- Behoud bestaande geslaagde paden:
  - bekende locatie-QR navigeert nog steeds direct;
  - bekende productcode start nog steeds de inventory-flow;
  - opslaan blijft additief via `AddOrIncrementStockAsync`.
- Houd de oplossing klein en lokaal. Los dit niet op met een brede workflow-engine of
  nieuwe generieke scanarchitectuur.
- Noem de story nooit `Done` of geaccepteerd. Eindig alleen met
  `ready for Codex review` of `not ready`.

## Minimal Context

Lees alleen:

- `CLAUDE.md`;
- `.codex/PILOT-INV-03-implementation-packet.md`;
- dit bestand;
- `BootManager.Web/Components/Pages/Scan.razor`;
- alleen de direct geraakte inventory-servicebestanden als je die echt moet aanpassen;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`;
- eventuele direct geraakte inventory-tests.

## Test Evidence Requirements

Voeg of wijzig tests die concreet bewijzen:

- onbekende productcode toont de drie vereiste keuzes in de scanflow;
- `Nieuw product` en `Code koppelen` brengen de flow daarna daadwerkelijk naar
  locatiekeuze/hoeveelheid;
- handmatige locatiekeuze beschikbaar blijft ook wanneer een suggestie aanwezig is;
- `Locatie scannen` start de scanner opnieuw en gebruikt een daarna gescande locatie
  als selectie binnen de inventory-flow;
- `Ja, nog een` start de scanner opnieuw;
- bestaand locatie-QR-gedrag buiten de inventory-flow niet regressief is.

Geen documentaire tests; laat echte componentinteractie of echte servicecalls zien.

## Required Checks

Voer minimaal uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ScanComponentTests"
dotnet build BootManager.sln --no-restore
git diff --check
```

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden en hersteld gedrag;
2. tests/checks en resultaten;
3. exacte nieuwe of gewijzigde testnamen en wat ze bewijzen;
4. resterende risico's of handmatige testpunten;
5. eindstatus: `ready for Codex review` of `not ready`.
