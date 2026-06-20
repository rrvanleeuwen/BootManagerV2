# Review Fix Packet

## Task

- Story ID: `PILOT-INV-03`
- Base packet: `.codex/PILOT-INV-03-implementation-packet.md`
- Previous fix packet: `.codex/PILOT-INV-03-review-fix-packet-01.md`
- Required branch: `codex/pilot-inv-03-scan-inruimflow`
- Goal: rond de scanflow af door twee resterende defecten te herstellen en expliciet
  bestaand scan-gedrag buiten de inventory-flow te behouden.

Dit is een smalle correctieronde. Los alleen de hieronder beschreven punten op.

## Defects To Fix

1. `Handmatig selecteren` toont nog geen bruikbare locatielijst wanneer de flow al een
   voorgestelde of alternatieve locatie heeft.
2. Een onbekende BootManager locatie-QR navigeert nu direct naar de koppelpagina;
   dat is regressief. Buiten de inventory-flow moet het oude role-based gedrag terug:
   - Owner ziet informatieve melding plus knop `Koppelen…`;
   - Crew ziet alleen de informatieve melding zonder navigatie.

## Required Behavioral Rules

Volg deze regels letterlijk:

- Bij klik op `Handmatig selecteren` moet de volledige handmatige locatielijst geladen
  en zichtbaar zijn, ook als er al een voorgestelde locatie of alternatieve locaties
  aanwezig zijn.
- Het is niet voldoende dat alleen de knop zichtbaar is; de lijst moet daarna echt
  bruikbaar zijn.
- Een onbekende BootManager locatie-QR mag buiten de inventory-flow niet automatisch
  navigeren.
- Het bestaande normale scanresultaatscherm voor onbekende BootManager locatie-QR's
  moet terugkomen.
- Alleen wanneer de gebruiker expliciet op `Koppelen…` klikt, mag Owner naar
  `/storage/link-location-qr?...` navigeren.
- Crew mag bij een onbekende BootManager locatie-QR nooit automatisch navigeren en ook
  geen `Koppelen…`-actie zien.
- Het nieuwe inventory-flow-gedrag voor onbekende productcodes moet behouden blijven.
- Het bestaande directe navigatiegedrag voor gekoppelde locatie-QR's moet behouden
  blijven.

## Scope

- Pas `Scan.razor` gericht aan zodat `Handmatig selecteren` de locatielijst altijd
  kan tonen.
- Pas `Scan.razor` gericht aan zodat onbekende BootManager locatie-QR's weer via het
  bestaande resultaatscherm lopen in plaats van directe navigatie.
- Voeg of wijzig alleen de minimaal benodigde tests om deze twee concrete gedragingen
  echt te bewijzen.

## Outside Scope

- Geen nieuwe feature-uitbreiding.
- Geen extra migraties.
- Geen service- of contractwijziging tenzij compile-technisch onvermijdelijk.
- Geen documentatie-updates.
- Geen commit, push, branch, PR, merge of deployment.

## Expected Write-Set

Wijzig alleen:

- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`.

Wijzig niets anders tenzij een compile-time dependency dat aantoonbaar vereist. Als dat
toch nodig blijkt, meld in de oplevering precies waarom.

## Execution Boundaries

- Controleer vóór bewerken dat de actieve branch exact
  `codex/pilot-inv-03-scan-inruimflow` is en niet `master`.
- Behoud alle al werkende paden:
  - gekoppelde locatie-QR navigeert direct;
  - bekende productcode start inventory-flow;
  - onbekende productcode toont de drie keuzes;
  - `Locatie scannen` en `Ja, nog een` blijven de scanner echt herstarten.
- Voeg geen brede state-refactor toe.
- Los dit lokaal en klein op.
- Eindig alleen met `ready for Codex review` of `not ready`.

## Minimal Context

Lees alleen:

- `CLAUDE.md`;
- `.codex/PILOT-INV-03-implementation-packet.md`;
- `.codex/PILOT-INV-03-review-fix-packet-01.md`;
- dit bestand;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`.

## Test Evidence Requirements

Nieuwe of gewijzigde tests moeten expliciet dit bewijzen:

1. Na klik op `Handmatig selecteren` in een situatie met voorgestelde locatie wordt een
   echte locatielijst zichtbaar, niet alleen een lege view.
2. Na klik op `Handmatig selecteren` in een situatie met alternatieve locaties wordt
   eveneens een echte locatielijst zichtbaar.
3. Owner met onbekende BootManager locatie-QR blijft op de scanpagina, ziet de melding
   en ziet een klikbare knop `Koppelen…`.
4. Pas na klik op `Koppelen…` navigeert Owner naar de linkpagina.
5. Crew met onbekende BootManager locatie-QR blijft op de scanpagina, ziet de melding
   en ziet géén knop `Koppelen…`.

Belangrijk:

- Een test die alleen controleert dat `Handmatig selecteren` bestaat is onvoldoende.
- Een test die alleen controleert dat `NavigateToLinkPage` uiteindelijk werkt is
  onvoldoende; eerst moet bewezen worden dat er geen automatische navigatie plaatsvindt.
- Gebruik echte componentinteractie in bUnit.

## Required Checks

Voer minimaal uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ScanComponentTests"
dotnet build BootManager.sln --no-restore
git diff --check
```

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden en exact hersteld gedrag;
2. tests/checks en resultaten;
3. exacte nieuwe of gewijzigde testnamen en welk defect ze nu echt afdekken;
4. eventuele resterende risico's;
5. eindstatus: `ready for Codex review` of `not ready`.
