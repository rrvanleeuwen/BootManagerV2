# Review Fix Packet

## Task

- Story ID: `PILOT-INV-03`
- Base packet: `.codex/PILOT-INV-03-implementation-packet.md`
- Previous fix packets:
  - `.codex/PILOT-INV-03-review-fix-packet-01.md`
  - `.codex/PILOT-INV-03-review-fix-packet-02.md`
  - `.codex/PILOT-INV-03-review-fix-packet-03.md`
- Required branch: `codex/pilot-inv-03-scan-inruimflow`
- Goal: herstel de productaanmaak in de onbekende-productcode-flow zodat de gebruiker
  expliciet een standaardeenheid moet kiezen.

Dit is een zeer gerichte correctieronde. Los alleen dit defect op.

## Remaining Defect

Als een onbekende productcode tijdens de scanflow wordt gebruikt en de gebruiker kiest
`Nieuw product`, kan de gebruiker nu geen eenheid kiezen. De implementatie gebruikt
impliciet een default/first unit. Dat is functioneel onjuist.

## Required Behavioral Rules

Volg deze regels letterlijk:

- In de `Nieuw product`-flow moet een verplichte keuze voor standaardeenheid aanwezig
  zijn.
- De flow mag niet automatisch stilzwijgend de eerste beschikbare eenheid kiezen.
- De gebruiker moet vóór opslaan expliciet een eenheid kiezen.
- Zonder gekozen eenheid mag `Aanmaken en doorgaan` niet slagen.
- Bij opslaan moet precies de gekozen eenheid naar `CreateAsync` worden doorgegeven.
- Het bestaande gedrag van de rest van de flow moet behouden blijven:
  - codeveld blijft vooraf ingevuld en bewerkbaar;
  - de uiteindelijke codewaarde gaat naar `AddCodeAsync`;
  - na succesvol aanmaken gaat de flow nog steeds direct door naar locatiekeuze of de
    volgende inventory-stap.

## Scope

- Pas alleen de `Nieuw product`-flow in `Scan.razor` aan.
- Voeg alleen de minimaal benodigde tests toe of wijzig ze zodat dit gedrag echt
  bewezen wordt.

## Outside Scope

- Geen extra flowwijzigingen buiten de eenheidskeuze.
- Geen service- of contractwijzigingen tenzij compile-technisch strikt noodzakelijk.
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
- `.codex/PILOT-INV-03-review-fix-packet-03.md`;
- dit bestand;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`.

## Test Evidence Requirements

Nieuwe of gewijzigde tests moeten expliciet dit bewijzen:

1. In de `Nieuw product`-flow is een verplichte eenheidskeuze zichtbaar.
2. Zonder gekozen eenheid kan de flow niet succesvol aanmaken en doorgaan.
3. Met gekozen eenheid wordt precies die eenheid naar `CreateAsync` doorgegeven.
4. De flow gaat na succesvol aanmaken nog steeds door naar locatiekeuze of het volgende
   inventory-stapscherm.

Belangrijk:

- Een test die alleen controleert dat er ergens een eenheidslabel staat is
  onvoldoende.
- Een test moet echte componentinteractie doen: eenheid wel/niet kiezen, opslaan, en
  de relevante servicecall verifiëren.

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
