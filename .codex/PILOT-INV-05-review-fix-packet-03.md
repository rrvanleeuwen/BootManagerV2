# Review Fix Packet

## Task

- Story ID: `PILOT-INV-05`
- Base packet: `.codex/PILOT-INV-05-implementation-packet.md`
- Previous correction packets:
  - `.codex/PILOT-INV-05-review-fix-packet-01.md`
  - `.codex/PILOT-INV-05-review-fix-packet-02.md`
- Required branch: `codex/pilot-inv-05-mutaties-historie`
- Goal: sluit de laatste twee open gaten definitief:
  1. in `Scan.razor` moet de fysieke mutatieflow na locatiecontext verdergaan via een
     **echte productscan**, niet via lijstselectie als vervanging;
  2. de componenttests moeten echte interactie en serviceverificatie doen, niet alleen
     markup of “component rendered”.

Dit packet is bedoeld als een laatste, zeer gerichte correctieronde. Los alleen deze
restpunten op. Als Claude opnieuw een semantische omweg kiest, is de status `not ready`.

## Mandatory Start Check

Controleer vóór iedere wijziging:

1. de actieve branch is exact `codex/pilot-inv-05-mutaties-historie` en niet `master`;
2. de bestaande worktreewijzigingen voor `PILOT-INV-05` zijn nog aanwezig;
3. packet `02` en dit packet zijn beide gelezen;
4. er zijn geen onverwachte staged wijzigingen.

Stop en rapporteer `not ready` wanneer een van deze checks faalt.

## Exact Remaining Defects

Er zijn nog exact twee functionele review-openingen:

1. `Scan.razor` gebruikt nu na locatiekeuze een productlijst als vervolgstap. Dat telt
   niet als de vereiste fysieke route “locatie scannen, daarna product scannen”.
2. `ScanComponentTests.cs` en `ProductsComponentTests.cs` bevatten nog documentaire
   tests die alleen renderen of markuptekst checken in plaats van echte interactie en
   servicecalls te bewijzen.

Alles buiten deze twee punten laat je zoveel mogelijk ongemoeid.

## Non-Negotiable Rules

Volg deze regels letterlijk:

1. In de fysieke mutatieflow van `Scan.razor` mag productkeuze **niet** uitsluitend via
   een lijst met knoppen worden opgelost.
2. Na het vastleggen van locatiecontext moet een productscan de flow verderbrengen.
3. Een handmatige productlijst mag hoogstens een fallback zijn wanneer expliciet als
   fallback aangeboden, maar niet het primaire pad van deze fysieke flow.
4. Een test die alleen `Assert.Contains(...)` op markup doet, telt niet als bewijs voor
   scan- of fallbackgedrag.
5. Een test die alleen `RenderComponent<T>()` en `Assert.NotNull(cut.Instance)` doet,
   telt niet als bewijs.
6. Zonder echte componentinteractie én serviceverificatie is de status `not ready`.

## Required File Outcome

Claude moet inhoudelijk wijzigen:

- `BootManager.Web/Components/Pages/Scan.razor`
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`

Optioneel en alleen als strikt nodig:

- `BootManager.Web/Components/Pages/Inventory/Products.razor`
- kleine ondersteunende inventory-bestanden binnen de al toegestane write-set

Als één van de drie verplichte bestanden hierboven niet wijzigt, rapporteer `not ready`.

## Allowed Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- optioneel `BootManager.Web/Components/Pages/Inventory/Products.razor` wanneer een
  kleine interactie-aanpassing nodig is om de test bewijsbaar te maken;
- optioneel kleine inventory service/helperbestanden binnen de eerdere
  `PILOT-INV-05`-write-set, maar alleen wanneer compile-technisch of test-technisch
  strikt vereist.

Wijzig niets anders tenzij een compile-time dependency dat aantoonbaar vereist.

## Forbidden Moves

Niet toegestaan:

- de fysieke flow laten eindigen in een productlijst zonder productscan;
- componenttests toevoegen die alleen tekst in markup zoeken;
- componenttests toevoegen die alleen bewijzen dat een component rendert;
- alleen comments of namen aanpassen om reviewtekst te “matchen”;
- service-tests uitbreiden als vervanging voor componentbewijs.

## Exact Behavioral Requirement For `Scan.razor`

De fysieke mutatieflow moet nu expliciet dit primaire pad hebben:

1. start vanuit de bestaande scan/terugvindcontext;
2. locatiecontext wordt gezet via locatiekeuze of locatie-QR;
3. daarna komt de flow in een stap waarin een **productcode gescand** wordt;
4. die productscan bepaalt welk product op de gekozen locatie gemuteerd wordt;
5. daarna volgt pas type/hoeveelheid/notitie en opslaan;
6. na succes keert de flow terug naar het begin van dezelfde fysieke mutatieflow.

Toegestane fallback:

- als expliciete secundaire keuze mag een handmatige productselectie bestaan, maar
  alleen duidelijk als fallback naast of achter de primaire scanstap.

Niet toegestaan:

- direct na locatiekeuze automatisch een productlijst tonen als hoofdroute;
- de gebruiker alleen laten klikken in plaats van scannen.

## Exact Test Requirements For `ScanComponentTests.cs`

Voeg of wijzig tests die **echt** bewijzen:

1. de mutatieflow na locatiecontext in een productscanstap terechtkomt;
2. een gescande productcode in die mutatieflow de juiste productcontext selecteert;
3. een mutatie-opslagcall (`MutateStockAsync`) vanuit die flow echt plaatsvindt;
4. de flow na succes terugkeert naar het begin van de fysieke mutatieroute.

Elk van deze punten moet door echte componentinteractie of relevante methodeaanroep
worden bewezen, niet door alleen markuptekst te inspecteren.

## Exact Test Requirements For `ProductsComponentTests.cs`

Vervang de documentaire fallbacktest door defectgevoelig bewijs dat minimaal laat zien:

1. de fallbackmodal of fallbackflow opent via gebruikersinteractie;
2. productselectie in die flow echt werkt;
3. auto-locatiekeuze of handmatige locatiekeuze echt door de component wordt verwerkt;
4. opslaan in die flow echt `MutateStockAsync` aanroept.

Ook hier geldt: geen pure markup- of render-assertions als eindbewijs.

## Minimal Context

Lees alleen:

- `CLAUDE.md`;
- `.codex/PILOT-INV-05-review-fix-packet-02.md`;
- dit packet;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- alleen de minimaal noodzakelijke aanvullende inventory-bestanden als een compile-time
  dependency daarom vraagt.

## Required Checks

Voer minimaal uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ScanComponentTests|FullyQualifiedName~ProductsComponentTests"
dotnet build BootManager.sln --no-restore
git diff --check
git diff --name-only
```

Controleer in je completion notes expliciet:

1. dat `Scan.razor` is gewijzigd;
2. dat `ScanComponentTests.cs` is gewijzigd;
3. dat `ProductsComponentTests.cs` is gewijzigd;
4. dat de fysieke route nu een primaire productscanstap heeft;
5. welke test exact de `MutateStockAsync`-aanroep vanuit `Scan.razor` bewijst;
6. welke test exact de `MutateStockAsync`-aanroep vanuit `Products.razor` bewijst.

## Definition Of Technical Completion

Rapporteer alleen `ready for Codex review` wanneer:

- de fysieke flow in `Scan.razor` primair via productscan werkt;
- documentaire tests zijn vervangen door defectgevoelige interactietests;
- zowel `ScanComponentTests.cs` als `ProductsComponentTests.cs` echte
  `MutateStockAsync`-verificatie bevatten;
- de gerichte tests slagen;
- build en `git diff --check` slagen;
- geen verboden omweg is gebruikt.

Rapporteer `not ready` wanneer één van deze punten ontbreekt.

## Completion Notes

Retourneer uitsluitend:

1. exacte gewijzigde bestanden;
2. welke codewijziging de primaire productscanstap in `Scan.razor` realiseert;
3. exacte nieuwe/gewijzigde testnamen in `ScanComponentTests.cs` en wat zij bewijzen;
4. exacte nieuwe/gewijzigde testnamen in `ProductsComponentTests.cs` en wat zij bewijzen;
5. de gerichte checkresultaten;
6. eindstatus `ready for Codex review` of `not ready`, met concrete reden.
