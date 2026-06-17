# Review Fix Packet 6

## Task

- Story ID: `PILOT-AUTH-01`
- Required branch: `feature/pilot-auth-01-local-users`
- Source packets:
  - `.codex/PILOT-AUTH-01-implementation-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-2-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-3-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-4-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-5-packet.md`
- Status: niet gereed; handmatige acceptatie blijft geblokkeerd.

Werk uitsluitend de onderstaande JavaScript-module-/circuitblocker af. Behoud alle
eerdere fixes.

## Blocking Finding

Op de echte loginpagina gebeurt na klikken op `Inloggen` niets. De browser meldt:

```text
Uncaught SyntaxError: Unexpected token '<'
```

De actuele componenten importeren dezelfde module zo:

```csharp
await Js.InvokeAsync<IJSObjectReference>("import", "./js/authClient.js");
```

Dit gebeurt in:

- `BootManager.Web/Components/Pages/Login.razor`;
- `BootManager.Web/Components/Pages/Account.razor`;
- `BootManager.Web/Components/Layout/MainLayout.razor`.

`BootManager.Web/wwwroot/js/authClient.js` is syntactisch geldige JavaScript. Bij een
correct gestarte lokale build retourneert `GET /js/authClient.js`:

- HTTP 200;
- `Content-Type: text/javascript`;
- een body die begint met `export async function postJson`.

De syntaxfout betekent daarom dat de browser voor ten minste een module-import een
HTML-response ontvangt en die als JavaScript probeert te parsen. Het eerste teken van
de HTML-response is `<`. De importfout ontsnapt momenteel uit `OnAfterRenderAsync`.
Daardoor kan het interactieve Blazor-circuit falen voordat de loginhandler bruikbaar
is, wat het stille gedrag van de knop verklaart.

## Required Diagnosis

Leg vóór wijziging in de browsernetworktrace en console concreet vast:

- de exacte request-URL die direct vóór `Unexpected token '<'` wordt geladen;
- statuscode en `Content-Type`;
- de eerste herkenbare regel van de responsebody;
- welke component de import start;
- of de request `/js/authClient.js`, een route-relatieve variant, een
  `/_framework/...`-variant of iets anders is;
- of de module uit browsercache komt;
- of het Blazor-circuit na de importfout nog verbonden is.

Gebruik een harde reload met cache uitgeschakeld. Rapporteer de feitelijke URL en
response. Accepteer geen aanname op basis van alleen de consolemelding.

## Required Fix

- Gebruik voor alle imports van `authClient.js` één canonieke, base-path-veilige URL.
- De URL mag niet afhangen van de huidige Razor-route of van de locatie van Blazors
  frameworkscript.
- Voor deze applicatie met `<base href="/">` is `/js/authClient.js` de verwachte
  canonieke assetroute. Een via `NavigationManager.BaseUri` opgebouwde equivalente
  absolute URL is ook toegestaan.
- Verwijder alle imports van `"./js/authClient.js"` uit de betrokken componenten.
- Los een eventuele server-, static-file- of deploymentoorzaak op wanneer de
  vastgelegde networktrace bewijst dat `/js/authClient.js` zelf HTML retourneert.
- Wijzig niet blind antiforgery, loginendpoints, PCR-middleware of authenticatielogica;
  die veroorzaken deze JavaScript-parsefout niet.

Maak module-initialisatie daarnaast foutbestendig:

- vang de relevante `JSException` tijdens `OnAfterRenderAsync` af;
- laat de uitzondering niet het circuit beëindigen;
- toon een duidelijke gebruikersmelding wanneer de module niet geladen kan worden;
- schakel login-, wachtwoordwijzigings- en logoutacties uit of laat ze gecontroleerd
  falen zolang de module ontbreekt;
- voorkom een stille knop;
- blijf `JSDisconnectedException` tijdens disposal veilig negeren;
- maskeer geen overige onverwachte fouten zonder zichtbare melding of logging.

## Required Behavior

- `GET /js/authClient.js` retourneert JavaScript, nooit Razor-HTML;
- openen van `/login` veroorzaakt geen `Unexpected token '<'`;
- klikken op `Inloggen` roept precies `POST /auth/login` aan;
- een geldige login zet het browsercookie en navigeert naar de juiste route;
- Crew met `PasswordChangeRequired=true` bereikt `/account`;
- wachtwoord wijzigen roept precies `POST /auth/change-password` aan;
- uitloggen roept precies `POST /auth/logout` aan;
- geen van deze flows veroorzaakt een native form-POST, antiforgeryfout,
  module-parsefout of verbroken circuit;
- bij een bewust onbereikbare module krijgt de gebruiker een zichtbare fout en blijft
  de pagina bestuurbaar.

## Required Tests

Voeg gerichte regressiedekking toe die minimaal bewijst:

- `Login.razor`, `Account.razor` en `MainLayout.razor` bevatten geen
  `"./js/authClient.js"`;
- alle drie gebruiken exact dezelfde canonieke module-URL;
- de module-importfout wordt op componentniveau afgehandeld en laat geen stille actie
  achter;
- een integratietest voor `GET /js/authClient.js` controleert HTTP 200,
  JavaScript-content-type en een JavaScript-body in plaats van HTML;
- de bestaande login-, wachtwoordwijzigings- en logoutendpointtests blijven groen.

Voer daarna met cache uitgeschakeld deze browsertest uit:

1. open `/login`;
2. controleer dat de module-request JavaScript retourneert;
3. selecteer Carla en log in;
4. controleer exact `POST /auth/login`;
5. wijzig bij PCR het wachtwoord op `/account`;
6. controleer exact `POST /auth/change-password`;
7. controleer toegang tot Dashboard/Logboek;
8. log uit via het profielmenu;
9. controleer exact `POST /auth/logout`;
10. controleer de volledige console op syntax-, JSInterop- en circuitfouten.

## Full Feature Regression Gate

Deze fix is alleen voldoende om opnieuw naar gebruikersacceptatie te gaan wanneer ook
de eerder gerealiseerde `PILOT-AUTH-01`-scope aantoonbaar intact blijft. Voer na de
gerichte browsertest daarom een volledige technische regressiecontrole uit.

Controleer minimaal:

- een bestaande databasekopie migreert zonder verlies van Owner-id, wachtwoord,
  profielgegevens en onboardingstatus;
- Roelof kan na migratie met zijn bestaande wachtwoord als Owner inloggen;
- een lege tijdelijke database maakt precies één bootstrap-Owner en behoudt de
  verplichte Owner-onboarding;
- de loginselector toont alleen actieve accounts en lekt geen profiel- of
  wachtwoordgegevens;
- accountnamen blijven hoofdletterongevoelig uniek;
- Owner kan Crew aanmaken, wachtwoord resetten, uitschakelen en opnieuw activeren;
- Crew wordt na aanmaak en reset verplicht naar `/account` geleid;
- vóór de verplichte wachtwoordwijziging zijn andere Crew-routes geblokkeerd;
- na wijziging werkt het oude wachtwoord niet en het nieuwe wel;
- Crew kan Dashboard, Scan en het huidige Logboek gebruiken;
- Crew ziet geen Instellingen- of Beheerderlink en directe Owner-routes blijven
  geweigerd;
- reset en uitschakelen maken twee reeds geopende Crew-sessies ongeldig;
- uitschakelen weigert nieuwe login;
- opnieuw activeren herstelt login met het laatst geldige wachtwoord en behoudt de
  actuele wachtwoordwijzigingsstatus;
- Owner kan zichzelf niet uitschakelen en kan geen tweede Owner of andere rol maken;
- cookie- en JWT-claims bevatten de werkelijke gebruikers-id, naam en rol;
- `Ingelogd blijven` behoudt het afgesproken persistente cookiegedrag;
- Owners bestaande onboarding-, instellingen- en beheerflows blijven bruikbaar.

Gebruik hiervoor uitsluitend een tijdelijke testdatabase of een tijdelijke kopie van
een bestaande lokale ontwikkel-/acceptatiedatabase. Gebruik niet de productie- of
Raspberry Pi-database.

Waar browserautomatisering praktisch beschikbaar is, voer de volledige flow uit met
een geïsoleerde test-Owner en test-Crew. Waar een punt al door een gerichte
integratie-/componenttest wordt bewezen, rapporteer de exacte test. Meld niet dat een
punt is gecontroleerd wanneer alleen de implementatie is gelezen.

Als deze JavaScriptfix een van deze regressiepunten breekt:

- herstel alleen wanneer de oorzaak binnen de Expected Write-Set van dit packet valt;
- wijzig niet zelfstandig overige auth-, domein-, migratie- of autorisatiescope;
- meld anders `niet gereed` met het exacte falende criterium voor een nieuw
  Codex-packet.

## Expected Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Components/Pages/Login.razor`;
- `BootManager.Web/Components/Pages/Account.razor`;
- `BootManager.Web/Components/Layout/MainLayout.razor`;
- een kleine gedeelde modulepad-helper of constante alleen wanneer dit aantoonbaar
  eenvoudiger en consistenter is;
- `BootManager.Web/wwwroot/js/authClient.js` alleen wanneer de diagnose dit vereist;
- gerichte component- en integratietests.

Wijzig `Program.cs`, middleware of deploymentconfiguratie alleen wanneer de
vastgelegde HTTP-response bewijst dat de correcte `/js/authClient.js`-route daar wordt
vervangen door HTML. Wijzig geen migratie, domeinmodel, overige functionaliteit of
documentatie.

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

- de exacte HTML-teruggevende module-request is vastgelegd;
- alle authcomponenten de canonieke module-URL gebruiken;
- de module-import geen circuit meer kan laten wegvallen;
- login, PCR-wachtwoordwijziging en logout in de echte browser slagen;
- geen `Unexpected token '<'`, antiforgery- of circuitfout optreedt;
- de volledige `PILOT-AUTH-01`-regressiepoort hierboven geen blocker toont;
- alle vereiste tests en checks acceptabel zijn.

Anders meld je `niet gereed` met de concrete resterende blokkade.

## Completion Notes

Retourneer alleen:

1. exacte request-URL, status, content-type, responsesoort en circuitdiagnose;
2. gewijzigde bestanden en gedrag;
3. component-, integratie- en volledige testresultaten;
4. exacte handmatige browsertest en resultaat;
5. regressiematrix met ieder punt uit `Full Feature Regression Gate` als
   `bewezen`, `niet bewezen` of `gefaald`, inclusief bewijs;
6. eindstatus: `gereed voor Codex-review` of `niet gereed`.
