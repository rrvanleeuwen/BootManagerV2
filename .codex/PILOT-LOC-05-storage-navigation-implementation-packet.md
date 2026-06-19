# Implementation Packet

## Task

- Story ID: `PILOT-LOC-05`
- Approved story: opslagnavigatie met menu `Opslag`, submenu `Locaties` en `Tagoverzicht`
- Story source: gebruikerprioriteit binnen `BootManager Holiday Pilot 2026`; sluit direct aan op `PILOT-LOC-01` t/m `PILOT-LOC-04`
- Goal: maak de opslagfunctionaliteit voor Owner vindbaar via de hoofdnavigatie door
  een nieuw menu `Opslag` toe te voegen met ingangen naar het bestaande
  locatiebeheer en het bestaande tagoverzicht, zonder storagegedrag, settingslogica
  of autorisatie buiten deze navigatieroute te verbreden.
- Required branch: `feature/pilot-loc-04-token-replacement-tag-overview`

De story is al inhoudelijk gekozen. Formuleer haar niet opnieuw en vraag geen nieuw
akkoord. Geef een kort uitvoeringsplan, implementeer direct, voer de checks uit en
rapporteer volgens `Completion Notes`.

## Scope

- Voeg in de Owner-only hoofdnavigatie een nieuw menu `Opslag` toe.
- Onder `Opslag` zijn precies twee Owner-only ingangen zichtbaar:
  - `Locaties`;
  - `Tagoverzicht`.
- `Locaties` opent een aparte opslagbeheerpagina die het bestaande
  `StorageManagement`-scherm hergebruikt; kopieer de CRUD-flow niet naar een tweede
  implementatie.
- `Tagoverzicht` opent het bestaande `StorageLocationTagOverview`-scherm.
- De nieuwe opslagbeheerpagina toont functioneel hetzelfde locatiescherm dat nu onder
  `Instellingen > Opslag` hangt: gebieden, locaties en de bestaande beheeracties.
- De bestaande toegang via `Instellingen > Opslag` mag blijven bestaan; deze story
  voegt vindbare navigatie toe en hoeft de settings-accordion niet te verwijderen.
- Crew ziet geen `Opslag`-menu en krijgt geen toegang tot de nieuwe Owner-only routes.

## Outside Scope

- Geen wijziging aan storage-domeinlogica, QR-tokenlogica, tagstatus, scanflow,
  print/export of migraties.
- Geen nieuwe voorraadpagina's of inventoryfunctionaliteit.
- Geen brede herindeling van de hele topnavigatie of settingspagina.
- Geen documentatie-, commit-, push-, branch-, PR-, merge-, release- of
  deploymentacties.

## Expected Write-Set

Wijzig alleen deze bestanden of modules, tenzij een noodzakelijke compile-time
dependency wordt ontdekt:

- `BootManager.Web/Components/Layout/NavMenu.razor`;
- één nieuwe Owner-only pagina onder `BootManager.Web/Components/Pages/` voor
  opslagbeheer, bij voorkeur iets als `StorageLocations.razor`;
- optioneel `BootManager.Web/Components/Pages/Settings.razor` alleen als een kleine
  gedeelde route- of linkaanpassing nodig is;
- optioneel een kleine gedeelde opslag-UI-helper onder `BootManager.Web/Components/Settings/`
  als dat nodig is om `StorageManagement` netjes te hergebruiken;
- gerichte component- en autorisatietests onder `BootManager.UnitTests/Storage/` en
  `BootManager.UnitTests/Web/`.

Wijzig niets in `BootManager.Application`, `BootManager.Core`, `BootManager.Infrastructure`
of integratietests; deze story is puur webnavigatie en route-autorisatie.

## Execution Boundaries

- Implementeer alleen presentatiecode, routecode en tests die dit packet expliciet vereist.
- Controleer vóór bewerken dat de actieve branch exact
  `feature/pilot-loc-04-token-replacement-tag-overview` is en niet `master`.
  Rapporteer `not ready` als dat niet zo is.
- Wijzig geen story-, release-, TODO-, legacy-, README-, handoff- of andere
  projectdocumentatie.
- Maak geen commit, push, branch, PR, merge, release of deployment.
- Behoud de bestaande `Settings > Opslag`-route als werkende fallback; verwijder geen
  bestaand opslagbeheer uit settings.
- Bouw geen tweede storagebeheerimplementatie op; hergebruik de bestaande component.
- Noem de story nooit `Done`, geaccepteerd of productierijp. Meld alleen
  `ready for Codex review` wanneer de technische completion definition volledig is gehaald.

## Minimal Context

Lees:

- `CLAUDE.md`;
- dit packet;
- `BootManager.Web/Components/Layout/NavMenu.razor`;
- `BootManager.Web/Components/Pages/Settings.razor`;
- `BootManager.Web/Components/Settings/StorageManagement.razor`;
- `BootManager.Web/Components/Pages/StorageLocationTagOverview.razor`;
- `BootManager.UnitTests/Web/RouteAuthorizationTests.cs`;
- relevante bestaande bUnit-tests onder `BootManager.UnitTests/Storage/` voor
  route- of navigatiepatronen.

Gebruik gerichte zoekopdrachten en kleine bestandssecties. Lees niet standaard:

- `.docs/TODO.md`, releasehistorie of legacy-analyse;
- `.codex/current-session-handoff.md` of `.codex/working-agreement.md`;
- repositorybrede source trees.

## Existing Constraints

- Volg .NET 8 en de Clean Architecture-regels in `CLAUDE.md`.
- `StorageManagement` is nu een childcomponent van `Settings.razor`; reuse die component
  in plaats van storagebeheerlogica te dupliceren.
- `StorageLocationTagOverview` is al Owner-only op route `/storage/tag-overview`; behoud
  die route en autorisatierichting.
- `NavMenu.razor` gebruikt nu eenvoudige topnav-links; houd de wijziging klein en
  onderhoudbaar. Een compacte Owner-only dropdown/expandable sectie is prima zolang
  die binnen het bestaande navigatiepatroon werkt.
- Crew mag bestaande locatie-detailpagina's blijven lezen, maar krijgt geen nieuw
  opslagbeheer- of tagbeheer-menu.

## Acceptance Focus

- Owner kan vanuit het hoofdmenu direct naar:
  - locatiesbeheer;
  - tagoverzicht.
- `Locaties` toont het bestaande opslagbeheerscherm, niet een versimpelde of losse kopie.
- Crew ziet het nieuwe `Opslag`-menu niet.
- Directe route-toegang tot de nieuwe locatiesbeheerpagina is Owner-only.
- Bestaand `Instellingen > Opslag` blijft bruikbaar.

## Test Evidence Requirements

Voeg defectgevoelige tests toe die echte productcode/componenten uitvoeren en concreet bewijzen:

- de hoofdnavigatie toont voor Owner het menu `Opslag` met links naar `Locaties` en
  `Tagoverzicht`;
- Crew ziet het menu `Opslag` niet;
- de nieuwe locatiesbeheerpagina rendert het bestaande `StorageManagement`-component;
- route-autorisatie bewijst dat zowel de nieuwe locatiesbeheerpagina als
  `StorageLocationTagOverview` `Owner` only zijn;
- de bestaande settingsroute blijft `StorageManagement` nog steeds renderen.

Gebruik echte componentrendering en interactie via het bestaande componenttestframework.
Geen snapshot- of bronvormtests als vervanging van gedrag.

## Required Checks

Voer eerst gerichte checks uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~Storage|FullyQualifiedName~RouteAuthorization|FullyQualifiedName~NavMenu"
```

Voer daarna uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
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
- Owner het opslagmenu en beide ingangen ziet;
- Crew het opslagmenu niet ziet;
- de nieuwe locatiesbeheerpagina het bestaande storagebeheer hergebruikt;
- route-autorisatie voor de nieuwe opslagroute bewezen is;
- alle gerichte tests slagen en alle nieuwe of gewijzigde tests echte productcode uitvoeren;
- build en `git diff --check` slagen;
- geen onverklaarde wijziging buiten de verwachte write-set staat;
- resterende handmatige acceptatiestappen expliciet zijn vermeld.

Meld `not ready` wanneer scope onvolledig is, Crew het menu alsnog ziet, opslagbeheer
gedupliceerd is, een nieuwe of gewijzigde test faalt, build/diffcheck faalt, een
vereiste beslissing ontbreekt of extra write-area niet kan worden verantwoord.

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden en geïmplementeerd gedrag;
2. hoe `Opslag`, `Locaties` en `Tagoverzicht` nu navigeren;
3. exacte nieuwe/gewijzigde testnamen en welk productiegedrag zij uitvoeren;
4. eventuele kleine route-/componentimpact;
5. resterende risico's en exacte handmatige testvereisten;
6. eindstatus: `ready for Codex review` of `not ready`, met concrete reden.
