# Review Fix Packet

## Task

- Story ID: `PILOT-INV-03`
- Base packet: `.codex/PILOT-INV-03-implementation-packet.md`
- Previous fix packets:
  - `.codex/PILOT-INV-03-review-fix-packet-01.md`
  - `.codex/PILOT-INV-03-review-fix-packet-02.md`
- Required branch: `codex/pilot-inv-03-scan-inruimflow`
- Goal: sluit het laatste open functionele gat in de onbekende-productcode-flow.

Dit is bedoeld als laatste, zeer gerichte correctieronde. Los alleen dit defect op.

## Remaining Defect

Bij `Nieuw product` in de onbekende-productcode-flow is de gescande code nu alleen
zichtbaar als tekst. Dat voldoet niet aan de story. De code moet in het formulier
**vooraf ingevuld én bewerkbaar** zijn voordat het product en de codekoppeling worden
opgeslagen.

## Required Behavioral Rules

Volg deze regels letterlijk:

- In de `Nieuw product`-flow moet een echt invoerveld voor de code aanwezig zijn.
- Dat invoerveld moet vooraf gevuld zijn met de gescande code.
- De gebruiker moet die code kunnen wijzigen vóór opslaan.
- Bij opslaan moet de uiteindelijk ingevoerde code gebruikt worden voor de
  `AddCodeAsync`-stap, niet per definitie de oorspronkelijk gescande waarde.
- Het bestaande gedrag van de rest van de flow moet behouden blijven:
  - onbekende productcode toont nog steeds de drie keuzes;
  - `Code koppelen` blijft werken;
  - na succesvol nieuw product aanmaken gaat de flow nog steeds direct verder naar
    locatiekeuze en hoeveelheid.
- Alleen tekst tonen met een `<code>`-blok of label is **niet voldoende**.

## Scope

- Pas alleen de onbekende-productcode `Nieuw product`-flow in `Scan.razor` aan.
- Voeg alleen de minimaal noodzakelijke tests toe of wijzig ze zodat dit gedrag echt
  bewezen wordt.

## Outside Scope

- Geen extra flowwijzigingen.
- Geen nieuwe service- of contractwijzigingen tenzij compile-technisch strikt nodig.
- Geen migraties.
- Geen documentatie-updates buiten dit packet.
- Geen commit, push, branch, PR, merge of deployment.

## Expected Write-Set

Wijzig alleen:

- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`.

Wijzig niets anders tenzij een compile-time dependency dat aantoonbaar vereist.

## Execution Boundaries

- Controleer vóór bewerken dat de actieve branch exact
  `codex/pilot-inv-03-scan-inruimflow` is en niet `master`.
- Houd alle nu al geslaagde paden intact.
- Voeg geen refactor toe.
- Eindig alleen met `ready for Codex review` of `not ready`.

## Minimal Context

Lees alleen:

- `CLAUDE.md`;
- `.codex/PILOT-INV-03-implementation-packet.md`;
- `.codex/PILOT-INV-03-review-fix-packet-01.md`;
- `.codex/PILOT-INV-03-review-fix-packet-02.md`;
- dit bestand;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`.

## Test Evidence Requirements

Nieuwe of gewijzigde tests moeten expliciet dit bewijzen:

1. In de `Nieuw product`-flow staat een echt code-invoerveld met de gescande code
   vooraf ingevuld.
2. De gebruiker kan dat veld wijzigen.
3. Na opslaan wordt de gewijzigde code gebruikt voor de codekoppeling, niet de oude
   gescande waarde.
4. Na succesvol opslaan gaat de flow nog steeds door naar locatiekeuze of het volgende
   inventory-stapscherm.

Belangrijk:

- Een test die alleen controleert dat de gescande code ergens in de markup staat is
  onvoldoende.
- Een test moet echte componentinteractie doen: veld uitlezen of aanpassen, daarna
  opslaan en de relevante servicecall verifiëren.

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
