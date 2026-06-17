# Review Fix Packet 3

## Task

- Story ID: `PILOT-AUTH-01`
- Required branch: `feature/pilot-auth-01-local-users`
- Source packets:
  - `.codex/PILOT-AUTH-01-implementation-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-2-packet.md`
- Status: niet gereed; handmatige acceptatie is geblokkeerd.

Werk uitsluitend de twee onderstaande acceptatieblockers af. Alle oorspronkelijke
scope-, uitvoerings- en oplevergrenzen blijven gelden.

Behoud de reeds aanwezige fixes in `StartupGate.razor` en `Home.razor`; draai die niet
terug.

## Blocking Findings

### 1. Crew kan de verplichte wachtwoordwijziging niet opslaan

Handmatige reproductie:

1. Owner maakt Crew `Carla` aan met tijdelijk wachtwoord `12345678`;
2. Carla logt in;
3. de verplichte `/account`-gate wordt correct getoond;
4. Carla kiest nieuw wachtwoord `abcd1234`;
5. opslaan faalt met:

```text
A valid antiforgery token was not provided with the request. Add an antiforgery
token, or disable antiforgery validation for this endpoint.
```

`/auth/change-password` bevat in de actuele bron al `.DisableAntiforgery()`. Voeg dit
daarom niet blind nogmaals toe. Stel eerst vast waarom de echte browserrequest alsnog
antiforgeryvalidatie bereikt en herstel de werkelijke oorzaak.

Vereist gedrag:

- de bestaande same-origin JSON-browserflow via `authClient.js` werkt voor een
  ingelogde Crew met `PasswordChangeRequired=true`;
- huidig wachtwoord, nieuw wachtwoord en bevestiging worden via het bestaande
  `IAccountService`-pad gevalideerd;
- wachtwoord en credentialversie worden eenmaal gewijzigd;
- `PasswordChangeRequired` wordt `false`;
- dezelfde response vernieuwt de huidige authenticatiecookie;
- de oude niet-persistente sessie-id wordt ingetrokken en vervangen;
- de gebruiker kan daarna zonder fout naar de normale Crew-routes;
- overige sessies/tokens met de oude credentialversie blijven ongeldig;
- logout en login blijven werken.

Voeg een integratietest toe die de handmatige Crew-flow werkelijk reproduceert:

- maak of seed een Crew met `PasswordChangeRequired=true`;
- log via `/auth/login` in met het tijdelijke wachtwoord;
- POST via dezelfde JSON/cookievorm als de browser naar `/auth/change-password`;
- bewijs HTTP 200 zonder antiforgeryfout;
- bewijs een vernieuwde cookie;
- bewijs dat het tijdelijke wachtwoord daarna niet meer werkt;
- bewijs dat het nieuwe wachtwoord wel werkt;
- bewijs dat de nieuwe sessie niet meer door de PCR-gate wordt geblokkeerd.

Een test met alleen een Owner of alleen een rechtstreekse servicecall is onvoldoende.

### 2. Accountnavigatie staat dubbel in de header

De zichtbare accountnaam is als extra menu-item aan `NavMenu.razor` toegevoegd, terwijl
`MainLayout.razor` rechts al de bestaande gebruikersdropdown met naam en user-icoon
heeft.

Herstel naar één account-entry:

- verwijder het toegevoegde account-/naam-menu-item uit `NavMenu.razor`;
- behoud in de rechter dropdown de werkelijke displaynaam;
- voeg in die bestaande dropdown voor zowel Owner als Crew een item `Mijn account`
  toe dat naar `/account` navigeert;
- toon `Instellingen` uitsluitend aan Owner;
- behoud `Uitloggen` in dezelfde dropdown;
- voorkom dubbele uitlogacties in de header: er hoort één zichtbare logoutactie te
  zijn, in de rechter dropdown;
- Crew ziet geen Instellingen of Beheerder;
- Owner en Crew zien ieder precies één account-entry.

Voeg een gerichte component-/markup-regressietest toe waar praktisch, of breid de
bestaande route-/layouttest uit zodat de dubbele accountlink niet ongemerkt terugkomt.

## Expected Write-Set

Wijzig alleen wat voor deze twee blockers nodig is:

- `BootManager.Web/Program.cs`;
- `BootManager.Web/wwwroot/js/authClient.js` indien de echte oorzaak daar ligt;
- `BootManager.Web/Components/Pages/Account.razor`;
- `BootManager.Web/Components/Layout/NavMenu.razor`;
- `BootManager.Web/Components/Layout/MainLayout.razor`;
- betrokken authmiddleware uitsluitend wanneer de reproductie bewijst dat dit nodig is;
- gerichte unit- en integratietests.

Wijzig geen migratie, domeinmodel, overige pagina's, documentatie of pilotstatus.
Leg vóór iedere noodzakelijke wijziging buiten deze lijst uit waarom die nodig is.

## Execution Boundaries

- Controleer vóór wijziging dat de actieve branch exact
  `feature/pilot-auth-01-local-users` is en niet `master`.
- Werk met de bestaande ongecommitte worktree en behoud wijzigingen van Codex en
  gebruiker.
- Wijzig geen story-, release-, TODO-, legacy-, README-, handoff- of andere
  projectdocumentatie.
- Maak geen commit, push, branch, PR, merge, release of deployment.
- Gebruik geen productie- of Raspberry Pi-database.
- Noem de story niet `Done`, geaccepteerd of productierijp.
- Meld `niet gereed` wanneer de echte Crew/PCR-flow niet geautomatiseerd kan worden
  bewezen.

## Required Checks

Voer eerst de nieuwe en geraakte gerichte tests uit. Voer daarna sequentieel uit:

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

- de echte Crew/PCR-wachtwoordflow zonder antiforgeryfout slaagt;
- cookie- en sessievernieuwing door de integratietest zijn bewezen;
- tijdelijk en nieuw wachtwoord na wijziging correct worden afgewezen/geaccepteerd;
- de header precies één account-entry en één logoutactie heeft;
- `Mijn account` voor Owner en Crew in de bestaande rechter dropdown staat;
- Owner-only navigatie Owner-only blijft;
- alle vereiste checks acceptabel zijn en geen nieuwe failure bestaat.

Anders meld je `niet gereed` met de concrete resterende blokkade.

## Completion Notes

Retourneer alleen:

1. vastgestelde oorzaak van de antiforgeryfout;
2. gewijzigde bestanden en gedrag;
3. gerichte en volledige testresultaten;
4. resterende handmatige acceptatiestappen;
5. eindstatus: `gereed voor Codex-review` of `niet gereed`.
