# Review Fix Packet 5

## Task

- Story ID: `PILOT-AUTH-01`
- Required branch: `feature/pilot-auth-01-local-users`
- Source packets:
  - `.codex/PILOT-AUTH-01-implementation-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-2-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-3-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-4-packet.md`
- Status: niet gereed; handmatige acceptatie blijft geblokkeerd.

Werk uitsluitend de onderstaande browser-/circuitblocker af. Behoud alle eerdere fixes.

## Blocking Finding

De verplichte Crew-wachtwoordwijziging faalt in de echte browser nog steeds met:

```text
A valid antiforgery token was not provided with the request. Add an antiforgery
token, or disable antiforgery validation for this endpoint.
```

De directe integratietest naar `/auth/change-password` slaagt en dat endpoint heeft
`.DisableAntiforgery()`. Die test reproduceert de browserfout dus niet.

De actuele `/account`-pagina gebruikt:

```razor
<EditForm Model="@_changePasswordModel" OnValidSubmit="HandleChangePassword">
```

De fout wijst erop dat de browser in de falende situatie een native form-POST naar de
Razor-route `/account` uitvoert, in plaats van `HandleChangePassword` via een actief
Blazor-circuit aan te roepen.

De runtime-log bevat daarnaast herhaaldelijk:

```text
Microsoft.JSInterop.JSDisconnectedException:
JavaScript interop calls cannot be issued at this time.
This is because the circuit has disconnected and is being disposed.
```

onder andere vanuit `Login.DisposeAsync()` en `MainLayout.DisposeAsync()`.

## Required Diagnosis

Stel vóór wijziging met concrete logging of browsernetwerkinspectie vast:

- welke URL de falende submit werkelijk ontvangt;
- of dit `POST /account` of `POST /auth/change-password` is;
- of `HandleChangePassword` vóór de fout wordt aangeroepen;
- of het Blazor-circuit op dat moment verbonden is;
- welke request/content-type de antiforgeryfout veroorzaakt.

Rapporteer deze feitelijke oorzaak. Accepteer niet opnieuw alleen een groene directe
HTTP-integratietest als bewijs.

## Required Behavior

- opslaan op `/account` mag nooit als native formulier-POST naar `/account` eindigen;
- de actie wordt uitsluitend door de interactieve Blazor-handler gestart;
- de handler gebruikt daarna exact `/auth/change-password`;
- de echte Crew/PCR-flow retourneert geen antiforgeryfout;
- client-side validatie blijft werken;
- een dubbele submit tijdens `_busy` is onmogelijk;
- bij ontbrekend of verbroken circuit verschijnt geen server-antiforgerypagina;
- na succes wordt de cookie vernieuwd en gaat Carla naar een toegestane Crew-route;
- het oude wachtwoord werkt niet meer, het nieuwe wel;
- de bestaande PCR-gate blijft actief vóór een geslaagde wijziging.

Een robuuste richting is een expliciete `type="button"`-actie met Blazor `@onclick`
en een `EditContext.Validate()`-pad, zodat de browser nooit zelfstandig het formulier
submit. Een andere oplossing is toegestaan wanneer die aantoonbaar hetzelfde gedrag
garandeert. Voeg niet simpelweg antiforgery uit aan `/account`.

## Circuit Cleanup

Herstel daarnaast de aangetoonde disposal-fouten:

- `Login.DisposeAsync`;
- `Account.DisposeAsync`;
- `MainLayout.DisposeAsync`;
- andere direct betrokken authcomponenten alleen wanneer dezelfde fout daar aantoonbaar
  optreedt.

`IJSObjectReference.DisposeAsync()` tijdens een verbroken circuit moet
`JSDisconnectedException` veilig negeren. Maskeer geen andere onverwachte fouten.

## Required Tests

Behoud de bestaande endpointintegratietests, maar voeg dekking toe voor de werkelijke
componentflow:

- bewijs dat `/account` geen native submitactie/method naar zichzelf bevat;
- bewijs dat de knop `type="button"` of een gelijkwaardig niet-native-submitmechanisme
  gebruikt;
- bewijs dat de Blazor-handler het bestaande validatiepad uitvoert;
- bewijs dat de handler uitsluitend `/auth/change-password` aanroept;
- bewijs dat een verbroken JS-circuit tijdens disposal geen componentfout veroorzaakt,
  waar praktisch testbaar.

Voer daarnaast handmatig of via een browsertest exact uit:

1. Crew met `PasswordChangeRequired=true` logt in;
2. open `/account`;
3. vul huidig, nieuw en bevestiging in;
4. klik eenmaal op wijzigen;
5. controleer in netwerk/log dat alleen `POST /auth/change-password` plaatsvindt;
6. controleer HTTP 200 en vernieuwde cookie;
7. controleer navigatie naar Crew-route;
8. log uit en bewijs oud wachtwoord fout, nieuw wachtwoord goed.

## Expected Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Components/Pages/Account.razor`;
- `BootManager.Web/Components/Pages/Login.razor`;
- `BootManager.Web/Components/Layout/MainLayout.razor`;
- `BootManager.Web/wwwroot/js/authClient.js` alleen wanneer diagnose dit vereist;
- gerichte component-, unit- en integratietests.

Wijzig `Program.cs`, antiforgeryconfiguratie of middleware alleen wanneer de vastgelegde
requestdiagnose bewijst dat dit noodzakelijk is. Wijzig geen migratie, domeinmodel,
overige functionaliteit of documentatie.

## Execution Boundaries

- Controleer dat de actieve branch exact
  `feature/pilot-auth-01-local-users` is en niet `master`.
- Behoud alle bestaande ongecommitte wijzigingen.
- Wijzig geen story-, release-, TODO-, legacy-, README-, handoff- of andere
  projectdocumentatie.
- Maak geen commit, push, branch, PR, merge, release of deployment.
- Gebruik geen productie- of Raspberry Pi-database.
- Noem de story niet `Done`, geaccepteerd of productierijp.

## Required Checks

Voer eerst de nieuwe en geraakte tests uit. Voer daarna sequentieel uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

De bekende
`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`-failure mag
alleen als bestaande baseline worden gemeld wanneer dit exact de enige unit-testfailure
blijft.

## Completion Rule

Meld alleen `gereed voor Codex-review` wanneer:

- de werkelijk falende browserrequest is geïdentificeerd;
- native `POST /account` aantoonbaar niet meer kan optreden;
- de volledige echte Crew/PCR-browserflow slaagt;
- geen antiforgeryfout optreedt;
- geen `JSDisconnectedException` uit de betrokken disposalpaden ontsnapt;
- alle vereiste tests en checks acceptabel zijn.

Anders meld je `niet gereed` met de concrete resterende blokkade.

## Completion Notes

Retourneer alleen:

1. feitelijke request- en circuitdiagnose;
2. gewijzigde bestanden en gedrag;
3. component-, integratie- en volledige testresultaten;
4. exacte handmatige browsertest en resultaat;
5. eindstatus: `gereed voor Codex-review` of `niet gereed`.
