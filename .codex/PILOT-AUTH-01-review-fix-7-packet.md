# Review Fix Packet 7

## Task

- Story ID: `PILOT-AUTH-01`
- Required branch: `feature/pilot-auth-01-local-users`
- Source story: `.docs/releases/holiday-pilot-2026.md`, section
  `PILOT-AUTH-01 — Lokale Owner- en Crew-accounts`
- Previous packet: `.codex/PILOT-AUTH-01-review-fix-6-packet.md`
- Status: `niet gereed`; gebruikersacceptatie blijft geblokkeerd.

Werk uitsluitend de twee onderstaande reviewfindings af en voeg de ontbrekende
regressiedekking toe. Behoud alle werkende authflows uit de eerdere packets.

## Independent Review Results

Codex heeft op 2026-06-11 onafhankelijk vastgesteld:

- `GET /js/authClient.js`: HTTP 200, `text/javascript`, JavaScript-body;
- Crew-login: HTTP 200;
- Crew met `PasswordChangeRequired=true`: `/dashboard` -> `/account`;
- `POST /auth/change-password`: HTTP 200, geen antiforgeryfout;
- oud Crew-wachtwoord wordt geweigerd, nieuw wachtwoord werkt;
- Crew bereikt daarna Dashboard, Scan en Logboek;
- Crew krijgt op `/settings` een access-deniedredirect en ziet geen
  `Instellingen`-link;
- reset en uitschakelen maken twee bestaande Crew-sessies ongeldig;
- uitgeschakelde Crew kan niet inloggen;
- reactiveren behoudt het laatst geldige wachtwoord en de PCR-status;
- logout werkt;
- `Ingelogd blijven` geeft een persistent cookie met expiry;
- JWT bevat werkelijke gebruikers-id, naam, rol en credentialversie;
- een tijdelijke bestaande SQLite-database migreert met behoud van Owner-id,
  wachtwoordvelden, profielpayload, onboardingstatus, pin/recoveryvelden en
  timestamps;
- de gemigreerde Owner kan met het bestaande wachtwoord inloggen;
- unit-tests: 197 geslaagd, alleen de bekende
  `OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`
  baseline faalt;
- integratietests: 6/6 geslaagd;
- solution-build: geslaagd zonder warnings of errors;
- `git diff --check`: geslaagd.

Deze resultaten hoeven niet opnieuw door aannames te worden vervangen; na de fix
moeten de relevante controles wel opnieuw worden uitgevoerd.

## Blocking Finding 1: Bootstrap Owner Cannot Complete Onboarding

Op een werkelijk lege tijdelijke database maakt de applicatie correct één bootstrap
Owner met:

- `Role=Owner`;
- `PasswordChangeRequired=true`;
- `OnboardingCompleted=false`.

Na geldige login is het feitelijke HTTP-gedrag:

```text
GET /dashboard  -> 302 Location: /account
GET /onboarding -> 302 Location: /account
GET /account    -> 200
```

Oorzaak: `PcrGateMiddleware` kijkt alleen naar
`bm.password_change_required=true` en niet naar de rol. Daardoor behandelt de
middleware de bootstrap Owner als Crew en blokkeert zij `/onboarding`.

Dit breekt expliciet de acceptatiecriteria:

- een lege database dwingt de bestaande Owner-onboarding af;
- Owners bestaande onboardingflow blijft bruikbaar.

### Required Fix

- De server-side PCR-gate naar `/account` geldt uitsluitend voor `Crew`.
- Een Owner met onvoltooide onboarding moet via de bestaande Owner-onboardingflow
  naar `/onboarding` kunnen gaan en deze kunnen voltooien.
- Houd de Crew-gate server-side afdwingbaar; verzwak Crew-beveiliging niet.
- Voeg `Role=Crew` expliciet toe aan de middlewaretests. De huidige tests maken geen
  rolclaim en konden deze regressie daardoor niet detecteren.
- Voeg tests toe voor een Owner met `PasswordChangeRequired=true`:
  - de Crew-PCR-middleware stuurt deze Owner niet naar `/account`;
  - `/onboarding` blijft bereikbaar;
  - de bestaande onboardinggate/service blijft de Owner-onboarding afdwingen.
- Voeg een integratietest toe die een lege tijdelijke SQLite-database start, met de
  bootstrap Owner inlogt en bewijst dat `/onboarding` niet naar `/account` wordt
  omgeleid.

Los dit rolgericht op. Voeg `/onboarding` niet blind toe als algemene Crew-whitelist,
want Crew met PCR mag de Owner-onboarding niet bereiken.

## Blocking Finding 2: MainLayout Import Failure Is Not Fully Contained

`Login.razor` en `Account.razor` zetten bij een module-importfout een zichtbare fout
en roepen `StateHasChanged()` aan. `MainLayout.razor` doet dat niet:

```csharp
catch (JSException)
{
    _logoutError = "Navigatiemodule kon niet worden geladen. Ververs de pagina.";
}
await Js.InvokeVoidAsync("setupAutoCollapse", ...);
```

Gevolgen:

- de vereiste foutmelding wordt na `OnAfterRenderAsync` niet onmiddellijk opnieuw
  gerenderd;
- `setupAutoCollapse` staat buiten foutafhandeling en kan alsnog een `JSException`
  uit `OnAfterRenderAsync` laten ontsnappen en het circuit verbreken;
- de bestaande bron-teksttest controleert alleen dat ergens `JSException` staat en
  bewijst dit gedrag niet.

### Required Fix

- Zorg dat een mislukte import in `MainLayout` onmiddellijk een zichtbare melding
  rendert.
- Laat geen relevante `JSException` uit de resterende initialisatie ontsnappen.
- Houd importfouten en auto-collapsefouten diagnostisch onderscheidbaar in logging
  of melding.
- Logout moet zonder geladen authmodule gecontroleerd falen en nooit een stille knop
  opleveren.
- Blijf `JSDisconnectedException` bij circuitverlies en disposal veilig behandelen.
- Voeg een echte componenttest of een voldoende gerichte gedragsmatige test toe die
  een falende module-import simuleert en bewijst:
  - geen ongehandelede uitzondering;
  - zichtbare foutstatus;
  - logout doet geen endpointaanroep zonder module.
- Vervang of versterk de huidige oppervlakkige
  `Assert.Contains("JSException", source)`-test. Alleen bron-tekst zoeken is hiervoor
  geen gedragsbewijs.

## Required Regression Tests

Voeg naast bovenstaande minimaal integratiedekking toe voor:

- bootstrap Owner kan na login `/onboarding` bereiken;
- Crew met PCR blijft buiten `/account`, `/auth/change-password` en `/auth/logout`
  geblokkeerd;
- Owner met PCR wordt niet door de Crew-PCR-middleware geraakt;
- twee Crew-cookies worden na wachtwoordreset beide ongeldig;
- twee Crew-cookies worden na uitschakelen beide ongeldig;
- uitgeschakelde Crew kan niet opnieuw inloggen;
- reactiveren behoudt wachtwoord en actuele PCR-status;
- login-, change-password- en logoutendpoints blijven zonder antiforgeryfout werken;
- `/js/authClient.js` blijft echte JavaScript retourneren.

Gebruik uitsluitend tijdelijke SQLite-databases. Gebruik geen productie- of
Raspberry Pi-database.

## Browser Verification

Voer na de geautomatiseerde checks met cache uitgeschakeld uit:

1. start met een lege tijdelijke database;
2. log in als bootstrap Owner;
3. bewijs dat Owner naar `/onboarding` gaat en niet naar `/account`;
4. voltooi onboarding en bewijs toegang tot Dashboard en Instellingen;
5. maak een geïsoleerde Crew aan;
6. log als Crew in met tijdelijk wachtwoord;
7. bewijs PCR-redirect naar `/account`;
8. wijzig het wachtwoord en controleer exact
   `POST /auth/change-password`;
9. bewijs Dashboard, Scan en Logboek;
10. bewijs dat Crew geen Beheerder/Instellingen ziet en directe Owner-routes worden
    geweigerd;
11. open het profielmenu en log uit;
12. controleer console en networktrace op syntax-, JSInterop-, antiforgery- en
    circuitfouten.

Wanneer browserautomatisering niet beschikbaar is, meld deze stappen als
`niet bewezen`; noem de story dan niet gereed voor gebruikersacceptatie.

## Expected Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Middleware/PcrGateMiddleware.cs`;
- `BootManager.Web/Components/Layout/MainLayout.razor`;
- gerichte unit-/componenttests;
- gerichte integratietests;
- testprojectconfiguratie alleen wanneer een componenttestpakket noodzakelijk is.

Wijzig geen migratie, domeinmodel, logincontract, accountservice, Settings-UI,
auth-endpoints, storydocumentatie of overige applicatiefunctionaliteit zonder een
nieuw aangetoond blockerbewijs.

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

Voer eerst de nieuwe gerichte tests uit. Voer daarna sequentieel uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

De bekende
`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`-failure mag
alleen als bestaande baseline worden gemeld wanneer dit exact de enige
unit-testfailure blijft.

## Completion Rule

Meld alleen `gereed voor Codex-review` wanneer:

- bootstrap Owner-onboarding end-to-end werkt;
- Crew-PCR server-side intact blijft;
- MainLayout-initialisatie geen import- of vervolg-JSException laat ontsnappen;
- een importfout onmiddellijk zichtbaar is en logout gecontroleerd faalt;
- alle nieuwe regressietests slagen;
- de browserflow volledig is bewezen;
- de vereiste tests, build en diffcheck acceptabel zijn.

Anders meld je `niet gereed` met de exacte resterende blokkade.

## Completion Notes

Retourneer alleen:

1. oorzaak en oplossing per finding;
2. gewijzigde bestanden;
3. nieuwe tests en wat elke test werkelijk bewijst;
4. volledige test-, build- en diffcheckresultaten;
5. exacte browserflow, networkrequests en console-uitkomst;
6. eindstatus: `gereed voor Codex-review` of `niet gereed`.
