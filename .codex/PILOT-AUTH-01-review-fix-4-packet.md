# Review Fix Packet 4

## Task

- Story ID: `PILOT-AUTH-01`
- Required branch: `feature/pilot-auth-01-local-users`
- Source packets:
  - `.codex/PILOT-AUTH-01-implementation-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-2-packet.md`
  - `.codex/PILOT-AUTH-01-review-fix-3-packet.md`
- Status: niet gereed; handmatige acceptatie is geblokkeerd.

Werk uitsluitend de onderstaande navigatieblocker af. Alle eerdere scope-,
uitvoerings- en oplevergrenzen blijven gelden.

Behoud de bestaande fixes voor de Crew-wachtwoordflow, `StartupGate` en `Home`.

## Blocking Finding

### Rechter gebruikersdropdown opent niet

Handmatige reproductie:

1. log in als Owner of Crew;
2. klik rechtsboven op de zichtbare naam of het profielicoon;
3. er gebeurt niets;
4. daardoor zijn `Mijn account`, `Instellingen` en `Uitloggen` niet bereikbaar.

De actuele markup gebruikt Bootstrap 4 `data-toggle="dropdown"` en scripts vanaf
externe CDN's. BootManager is lokaal/offline-first en de werking van essentiële
accountacties mag niet afhangen van jQuery, Bootstrap-JavaScript, CDN-beschikbaarheid
of automatische uitvoering van scripts in een Blazor-layout.

## Required Behavior

- bestuur openen en sluiten van de gebruikersdropdown volledig met Blazor-state;
- gebruik de reeds aanwezige `_menuOpen`, `ToggleMenu` en `CloseMenu` of vereenvoudig
  deze tot één duidelijke implementatie;
- het profiel-trigger-element is een echte `button` met:
  - begrijpelijk label/title;
  - `aria-haspopup="true"`;
  - actuele `aria-expanded`;
- toon de dropdown met Razor/CSS-state, niet via Bootstrap-JavaScript;
- klik op het profielicoon opent en sluit de dropdown;
- `Mijn account` navigeert voor Owner en Crew naar `/account`;
- `Instellingen` blijft uitsluitend zichtbaar voor Owner;
- `Uitloggen` roept de bestaande `Logout`-handler aan;
- succesvolle logout wist de cookie en navigeert naar `/login`;
- bij mislukte logout wordt `_logoutError` zichtbaar aan de gebruiker getoond;
- tijdens logout is de actie disabled en kan geen dubbele request ontstaan;
- de dropdown sluit bij `Mijn account`, `Instellingen` en succesvolle logout;
- er blijft precies één zichtbare account-entry en één logoutactie in de header;
- Crew ziet geen Owner-only items.

Verwijder voor deze fix geen CSS-only gebruik van het SB Admin-thema. Verwijder of
neutraliseer alleen de JavaScript-/CDN-afhankelijkheid voor de essentiële
gebruikersdropdown. Voer geen brede layout- of themarefactor uit.

## Required Tests

Voeg gerichte regressietests toe die minimaal bewijzen:

- `MainLayout` gebruikt geen `data-toggle="dropdown"` voor de gebruikersdropdown;
- de profieltrigger heeft een Blazor click-handler en `aria-expanded`;
- `Mijn account` en precies één `Uitloggen` staan in de rechter dropdown;
- Owner-only `Instellingen` blijft onder een Owner-authorisatieview;
- de oude losse logoutactie en accountlink in `NavMenu` blijven afwezig.

Wanneer de bestaande teststack interactieve componenttests praktisch ondersteunt,
test dan ook open/dicht en logout-click. Anders is een gerichte markup/componenttest
acceptabel, maar de handmatige open/dicht/logouttest blijft dan verplicht.

Behoud en draai de integratietests voor `/auth/logout` en de Crew-wachtwoordflow.

## Expected Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Components/Layout/MainLayout.razor`;
- `BootManager.Web/Components/Layout/MainLayout.razor.css` indien strikt nodig;
- `BootManager.Web/wwwroot/js/ui.js` alleen wanneer achtergebleven
  dropdowninitialisatie gericht moet worden verwijderd;
- betrokken gerichte unit-/componenttests;
- een ontbrekende logout-integratietest indien die nog niet bestaat.

Wijzig geen authmodel, migratie, accountservice, overige pagina's of documentatie.

## Execution Boundaries

- Controleer dat de actieve branch exact
  `feature/pilot-auth-01-local-users` is en niet `master`.
- Behoud alle bestaande ongecommitte wijzigingen van Codex en gebruiker.
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

- de dropdown zonder Bootstrap-/jQuery-JavaScript opent en sluit;
- logout vanuit die dropdown technisch en handmatig bereikbaar is;
- logoutendpoint, cookiewissen en redirect zijn getest;
- foutfeedback zichtbaar is;
- Owner/Crew-zichtbaarheid correct blijft;
- alle vereiste checks acceptabel zijn.

Anders meld je `niet gereed` met de concrete blokkade.

## Completion Notes

Retourneer alleen:

1. vastgestelde oorzaak;
2. gewijzigde bestanden en gedrag;
3. test- en checkresultaten;
4. exacte resterende handmatige test;
5. eindstatus: `gereed voor Codex-review` of `niet gereed`.
