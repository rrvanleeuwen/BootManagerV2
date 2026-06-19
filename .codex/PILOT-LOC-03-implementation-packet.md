# Implementation Packet

## Task

- Story ID: `PILOT-LOC-03`
- Approved story: QR-tag printen en PNG exporteren
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-LOC-03`
- Goal: laat alleen Owner voor een opslaglocatie met bestaande BootManager QR-token
  een printvriendelijke tagpagina openen, browserprint starten en een scanbare
  PNG-tag downloaden zonder storage-, scan- of authgedrag van eerdere stories te
  breken.
- Required branch: `feature/pilot-loc-03-qr-tag-print-png`

De story is al goedgekeurd. Formuleer haar niet opnieuw en vraag geen nieuw akkoord.
Geef een kort uitvoeringsplan, implementeer direct, voer de checks uit en rapporteer
volgens `Completion Notes`.

## Scope

- Voeg een Owner-only QR-tagpagina toe voor één opslaglocatie, bij voorkeur op een
  route als `/storage/locations/{locationId:guid}/tag`, met `PrintLayout` of een
  gelijkwaardig bestaand printvriendelijk layoutpatroon.
- Gebruik uitsluitend de bestaande stabiele `QrValue` uit `PILOT-LOC-02`. Voeg geen
  nieuw tokenformaat, geen tokenvervanging en geen databasewijzigingen toe.
- Toon op de tagpagina minimaal:
  - opslaggebied;
  - locatienaam;
  - een visueel scanbare QR-code op basis van de bestaande `QrValue`.
- Houd de tagpagina Owner-only. Crew mag de locatie-detailpagina blijven lezen maar
  mag geen tagpagina openen, printen of PNG exporteren.
- Voeg op de bestaande locatie-detailpagina voor Owner een gerichte actie toe om de
  tagpagina te openen wanneer de locatie al een `QrValue` heeft.
- Als een locatie nog geen `QrValue` heeft, behoud de bestaande knop
  `QR-token genereren`. Voeg in dat geval geen tagactie toe en genereer niet
  automatisch impliciet via de tagpagina.
- Maak browserprint beschikbaar vanaf de tagpagina via de bestaande JS-interoppatroon
  met `window.print`.
- Maak PNG-download per locatie beschikbaar. De download moet de QR-code bevatten die
  via de bestaande scanflow naar dezelfde locatie resolveert.
- Gebruik een onderhoudbaar lokaal renderpad voor de QR-afbeelding. De aanbevolen
  richting is één kleine QR-renderdependency in de Web-laag of één lokaal committed
  client-side library onder `wwwroot`, zonder CDN of externe service.
- Houd QR-rendering en downloadlogica buiten `StorageService`; dit is presentatiegedrag
  bovenop de al bestaande storage- en tokenfunctionaliteit.

## Outside Scope

- Geen wijziging aan `StorageService`, `LocationQrValue`, entiteiten, EF-configuratie,
  migraties of databaseconstraints, tenzij een echte compile-time dependency dat
  onvermijdelijk maakt.
- Geen token vervangen, ongeldig maken, tagstatus of tagoverzicht; dat hoort bij
  `PILOT-LOC-04`.
- Geen server-side PDF-, CSV- of batchexport.
- Geen labelvelindeling, snijtekens, printerprofielen, labelprinterintegratie of
  multi-tag printflow.
- Geen scanrouteringwijziging, geen barcodegedrag, geen producten of voorraad.
- Geen documentatie-, commit-, push-, branch-, PR-, merge-, release- of
  deploymentacties.

## Expected Write-Set

Wijzig alleen deze bestanden of modules, tenzij een noodzakelijke compile-time
dependency wordt ontdekt:

- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- één nieuwe Owner-only tagpagina onder `BootManager.Web/Components/Pages/`;
- optioneel een kleine Web-helper voor QR-rendering of PNG-filenameopbouw onder
  `BootManager.Web/Helpers/` of een vergelijkbare bestaande Web-map;
- `BootManager.Web/wwwroot/app.js` alleen als een kleine extra download-helper nodig is;
- optioneel één lokaal QR-renderasset onder `BootManager.Web/wwwroot/lib/` of één
  kleine package reference in `BootManager.Web/BootManager.Web.csproj`;
- gerichte component- en autorisatietests onder
  `BootManager.UnitTests/Storage/` en `BootManager.UnitTests/Web/`.

Wijzig geen application-, core- of infrastructure-bestanden zonder vooraf uit te leggen
waarom de gekozen renderstrategie anders niet compileert of onderhoudbaar is.

## Execution Boundaries

- Implementeer alleen applicatiecode, presentatiecode, kleine Web-assets en tests die
  dit packet expliciet vereist.
- Controleer vóór bewerken dat de actieve branch exact
  `feature/pilot-loc-03-qr-tag-print-png` is en niet `master`. Rapporteer
  `not ready` als dat niet zo is.
- Wijzig geen story-, release-, TODO-, legacy-, README-, handoff- of andere
  projectdocumentatie.
- Maak geen commit, push, branch, PR, merge, release of deployment.
- Verander scope, acceptatiecriteria, autorisatierichting of QR-tokenformaat niet.
- Gebruik geen CDN, webservice of internetafhankelijke QR-generator.
- Vermijd brede UI-refactors van layout, scanpagina of storagebeheer. Houd de wijziging
  verticaal en klein.
- Noem de story nooit `Done`, geaccepteerd of productierijp. Meld alleen
  `ready for Codex review` wanneer de technische completion definition volledig is
  gehaald.

## Minimal Context

Lees:

- `CLAUDE.md`;
- `.codex/PILOT-LOC-03-implementation-packet.md`;
- alleen de sectie `PILOT-LOC-03` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- `BootManager.Web/Components/Pages/LogbookPrint.razor`;
- `BootManager.Web/wwwroot/app.js`;
- `BootManager.Application/Storage/Services/IStorageService.cs`;
- `BootManager.Application/Storage/DTOs/StorageLocationDetailDto.cs`;
- `BootManager.UnitTests/Storage/StorageLocationDetailsComponentTests.cs`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`;
- `BootManager.UnitTests/Web/RouteAuthorizationTests.cs`.

Gebruik gerichte zoekopdrachten en kleine bestandssecties. Lees niet standaard:

- de volledige `.docs/TODO.md` of andere releaseverhalen;
- `.docs/legacy-analysis/` of `.docs/legacy-input/`;
- `.codex/current-session-handoff.md` of `.codex/working-agreement.md`;
- ongerelateerde source trees.

## Existing Constraints

- Volg .NET 8 en de Clean Architecture-regels in `CLAUDE.md`.
- `StorageLocationDetails.razor` is al `Owner,Crew`; behoud dat. De nieuwe tagroute is
  expliciet `Owner` only en moet ook als directe URL afgeschermd zijn.
- De bestaande `GenerateOrGetQrTokenAsync` en `GetLocationDetailAsync` leveren alle
  storagedata die deze story nodig heeft. Verplaats QR-rendering niet naar
  `StorageService`.
- Gebruik voor printen het bestaande patroon uit `LogbookPrint.razor` in plaats van een
  nieuw printmechanisme.
- Houd bestandsnamen en zichtbare labels praktisch voor de pilot. Een scanbare PNG en
  een eenduidige locatienaam/gebiedsaanduiding zijn belangrijker dan geavanceerde
  styling.
- Als je een QR-renderlibrary toevoegt, houd die lokaal, klein en direct verklaarbaar.
  Voeg geen tweede, alternatieve renderstack toe.

## Acceptance Focus

- Alleen Owner kan de tagpagina openen en acties uitvoeren.
- De tagpagina gebruikt exact de bestaande `QrValue`; dezelfde geprinte of gedownloade
  code moet de bestaande scanflow naar dezelfde locatie laten leiden.
- De locatie-detailpagina blijft voor Crew leesbaar en toont voor Crew geen print- of
  exportactie.
- Locaties zonder token blijven de bestaande genereerflow gebruiken; de story introduceert
  geen verborgen autogeneratie.
- De wijziging blijft beperkt tot presentatiegedrag; storage- en scanfunctionaliteit uit
  `PILOT-LOC-02` blijft intact.

## Test Evidence Requirements

Voeg defectgevoelige tests toe die echte productcode/componenten uitvoeren en concreet
bewijzen:

- Owner ziet op `StorageLocationDetails` bij bestaande `QrValue` een tagactie en bij
  ontbrekende `QrValue` alleen de bestaande genereeractie.
- Crew ziet op `StorageLocationDetails` geen tagactie en geen export-/printactie.
- De nieuwe tagpagina rendert gebied, locatienaam en een QR-afbeelding of
  rendercontainer voor de bestaande `QrValue`.
- De printactie roept via echte componentinteractie exact `window.print` aan.
- De PNG-actie roept via echte componentinteractie de gekozen downloadhelper aan met
  een `.png` bestandsnaam en echte afbeeldingsdata of een echte renderbron.
- Route-autorisatie bewijst dat de nieuwe tagpagina `Owner` only is, terwijl
  `StorageLocationDetails` `Owner,Crew` blijft.
- Bestaande scanrouting hoeft niet opnieuw via integratietests bewezen te worden, maar
  de nieuwe QR-weergave mag het gebruikte `QrValue` niet veranderen.

Inspecteer iedere nieuwe of gewijzigde test: geen `Assert.True(true)`, lege test,
bronvormtest als vervanging van gedrag of `async` test zonder relevante `await`.

## Required Checks

Voer eerst gerichte unit/componentchecks uit, bijvoorbeeld:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StorageLocationDetailsComponentTests|FullyQualifiedName~StorageLocationTag|FullyQualifiedName~RouteAuthorizationTests"
```

Voer daarna uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

De bekende
`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`-failure mag
alleen als ongerelateerde baseline worden gemeld wanneer exact die ene bestaande
unit-testfailure overblijft. Alle nieuwe of gewijzigde tests moeten slagen.

## Definition of Technical Completion

Meld uitsluitend `ready for Codex review` wanneer:

- ieder scopepunt en acceptatiecriterium technisch is geïmplementeerd;
- de tagpagina Owner-only is en de detailpagina `Owner,Crew` blijft;
- de QR-tagweergave exact de bestaande `QrValue` gebruikt;
- print- en PNG-acties via echte componenttests aantoonbaar werken;
- alle gerichte tests slagen en alle nieuwe of gewijzigde tests echte productcode
  uitvoeren;
- volledige unit- en integratietestruns geen nieuwe failure bevatten;
- build en `git diff --check` slagen;
- geen onverklaarde wijziging buiten de verwachte write-set staat;
- resterende handmatige acceptatiestappen expliciet zijn vermeld.

Meld `not ready` wanneer scope onvolledig is, de QR-rendering geen bestaande `QrValue`
gebruikt, de route-autorisatie of acties onvoldoende bewezen zijn, een nieuwe of
gewijzigde test faalt, build/diffcheck faalt, een vereiste beslissing ontbreekt of
extra write-area niet kan worden verantwoord.

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden en geïmplementeerd gedrag;
2. tests/checks en resultaten;
3. exacte nieuwe/gewijzigde testnamen en welk productiegedrag zij uitvoeren;
4. package-, asset- of configuratie-impact;
5. resterende risico's en exacte handmatige testvereisten;
6. eindstatus: `ready for Codex review` of `not ready`, met concrete reden.
