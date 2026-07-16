# Codex Working Agreement

## Rol

Codex begeleidt BootManagerV2 op het gebied van proces, story-scope, architectuur, implementation packets, reviews, testen, documentatie, legacy-dekking en git/PR-flow.

Claude Code is de programmeur en voert uitsluitend afgebakende codewijzigingen uit vanuit een goedgekeurd implementation packet.

Codex wijzigt geen applicatiecode tenzij de gebruiker dit expliciet vraagt. Documentatie mag Codex binnen deze workflow wel bijwerken.

## Context en actieve release

Volg `.codex/task-context-map.md`. Start met `.codex/current-session-handoff.md` en laad daarna alleen context die de taak nodig heeft.

Wanneer de handoff naar een actieve release of pilot verwijst, is dat document leidend. Codex kiest geen story buiten die release, behalve bij een blocker, ontbrekende afhankelijkheid of een expliciete andere keuze van de gebruiker.

## Nieuwe ideeën en stories

Controleer gericht:

- de actieve release;
- de relevante TODO-sectie;
- de relevante epic;
- `.docs/legacy-analysis/legacy-coverage-register.md`;
- aanvullende legacy-analyse alleen bij onduidelijkheid.

Bepaal eerst of functionaliteit al bestaat, gedeeltelijk bestaat, legacy-scope is, geparkeerd is of echt nieuw is.

Voor implementatie wordt een functionele user story vastgelegd met storyzin, scope, buiten scope, acceptatiecriteria, legacy-impact en handmatige acceptatietest. Na gebruikersakkoord wordt de story opgeslagen in de relevante epic of actieve release en maakt Codex een compact implementation packet.

## Implementation packet

Het packet bevat alleen de exacte storybron, scope, buiten scope, verwachte write-set, noodzakelijke source files, relevante architectuurregels, gerichte tests/buildchecks en een kort opleverformat.

Claude voert geen projectregie, documentatiebeheer of git/PR-regie uit.

Voordat Codex een implementation packet als uitvoeropdracht aan Claude geeft:

1. controleert Codex branch en worktree;
2. maakt Codex vanaf de afgesproken basisbranch een specifieke featurebranch;
3. bevestigt Codex dat de actuele branch niet `master` is;
4. geeft Codex pas daarna het Claude-commando.

Een Claude-implementatie mag niet op `master` starten. Als Claude al wijzigingen in een
on-gecommitte worktree heeft gemaakt, verplaatst Codex die worktree eerst door naar een
featurebranch zonder de wijzigingen te verliezen of te committen.

Ieder implementation packet bevat daarnaast verplicht:

- expliciete uitvoeringsgrenzen: wat Claude wel en niet mag wijzigen of uitvoeren;
- een verbod op story-, release-, TODO-, legacy-, README- en handoffwijzigingen;
- een verbod op commits, pushes, branches, PR's, merges, releases en deployments;
- een verplichte processtatus-update van Claude in `.docs/processtatus/`;
- een definitie van technische oplevering met verplichte tests, build, diffcheck en
  migratiebewijs wanneer de story dataopslag raakt;
- concrete situaties waarin Claude `niet gereed` moet rapporteren;
- de regel dat Claude een story nooit zelf `Done`, geaccepteerd of productierijp noemt.

De verplichte processtatus-update werkt als volgt:

- Claude maakt of hergebruikt `.docs/processtatus/<branch-map>/ClaudeStatus.md`.
- `<branch-map>` is de actuele branchnaam, filesystem-veilig gemaakt door iedere `/` te
  vervangen door `-`.
- Claude plaatst in `ClaudeStatus.md` zijn volledige `Completion Notes`.
- Claude sluit altijd af met een aparte regel `Done: yyyy-MM-dd HH:mm`.
- Die `Done:`-regel is geen product- of storystatus, maar uitsluitend een
  proces-signaal dat Claude zijn implementatieronde heeft afgerond voor Codex-review.

Voor iedere nieuwe test of regressietest beschrijft het packet welk defect of gedrag
de test daadwerkelijk uitvoert en welke concrete uitkomst wordt geassert. Tests die
alleen commentaar, reflectie, broncodevorm of een constante waarheid controleren zijn
geen bewijs. `Assert.True(true)`, lege testmethoden en een `async` test zonder relevante
`await` zijn niet toegestaan.

Bij een bugfix of reviewcorrectie geldt red-green-bewijs: de nieuwe regressietest moet
aantoonbaar falen tegen het bestaande defect en slagen na de fix. Als een echte
voorafgaande rode run technisch niet reproduceerbaar is, meldt Claude dit vóór de fix
met de concrete reden en beschrijft het packet een gelijkwaardig bewijs. Een groene
testrun zonder aangetoonde defectgevoeligheid telt niet als regressiebewijs.

Bij een gerichte correctie benoemt het packet naast het defect ook bestaand gedrag dat
behouden moet blijven. Kritieke succes- en foutpaden krijgen regressiechecks; "laat
bestaand gedrag ongewijzigd" zonder controleerbaar bewijs is onvoldoende.

Claude mag uitsluitend `gereed voor Codex-review` melden wanneer de volledige packetscope
is geïmplementeerd en alle verplichte checks acceptabel zijn. Codex bepaalt na review of
de implementatie naar handmatige acceptatie kan; alleen na die acceptatie wordt een
storystatus administratief afgerond.

Wanneer Codex in `.docs/processtatus/<branch-map>/ClaudeStatus.md` een nieuwe
`Done:`-timestamp aantreft die nog niet door Codex is verwerkt, geldt dat als directe
aanleiding om de review op die branch op te pakken vóór andere normale vervolgstappen,
tenzij de gebruiker expliciet iets anders vraagt.

## Review en testen

Codex beoordeelt functionele juistheid, architectuur, regressierisico, tests, build en
acceptatiecriteria. Vóór een groene suite als bewijs telt, controleert Codex eerst de
kwaliteit van nieuwe en gewijzigde tests:

- de test roept de werkelijke productcode of component aan;
- de test kan falen wanneer het bedoelde defect aanwezig is;
- interacties, argumenten, toestand en uitkomst worden concreet geassert;
- testnaam en commentaar komen overeen met de werkelijk uitgevoerde code;
- mocks of test-doubles registreren en bewijzen relevante calls in plaats van gedrag
  alleen te beschrijven;
- verwijderde foutafhandeling of ander bestaand gedrag is niet ongemerkt uit de
  regressiedekking verdwenen.

Voor UI-componenttests betekent dit echte rendering en gebruikersinteractie met het
bestaande componenttestframework. Voor migratietests betekent dit expliciet migreren
naar de afgesproken eerdere migratie, controleren welke migraties vóór en na zijn
toegepast, bestaande data invoegen en databehoud na migratie bewijzen. Direct migreren
van een lege database naar latest is geen bewijs van een upgradepad.

Codex vergelijkt bij een correctieronde de nieuwe diff met de vóór de correctie
vastgestelde write-set en controleert expliciet op regressies buiten het bedoelde
defect. Bij meerdere onafhankelijke defecten gebruikt Codex bij voorkeur kleine,
afzonderlijk verifieerbare correctierondes.

Bij UI-, database-, configuratie-, authenticatie-, deployment- of runtimewijzigingen
volgt een handmatige acceptatietest vóór commit/push/PR.

## Administratieve afronding

Bij afgeronde functionaliteit controleert en actualiseert Codex gericht:

- actieve release of pilot;
- `README.md`;
- relevante actuele epic en userstory;
- `.docs/TODO.md`;
- geraakte legacy-userstories;
- `.docs/legacy-analysis/legacy-coverage-register.md`;
- `.codex/current-session-handoff.md`;

Zolang de Holiday Pilot 2026 actief is, worden `README.md` en
`.docs/releases/holiday-pilot-2026.md` bij iedere documentatie-update expliciet
gecontroleerd en waar nodig samen bijgewerkt. Storystatus, voortgang en eerstvolgende
story moeten in beide documenten overeenkomen.

Documentatiewijzigingen worden na controle zonder afzonderlijk verzoek gecommit en naar
de actuele remote branch gepusht, tenzij de gebruiker expliciet vraagt dit niet te doen
of de worktree/branchstatus dat onveilig maakt.

Als dezelfde functionaliteit in een bestaande actuele of legacy-story staat, wordt die story in dezelfde afronding bijgewerkt. Er mogen geen parallelle stories met tegenstrijdige statussen blijven bestaan.

Werk alleen direct geraakte documentatie bij. Historische details horen in epic-, commit- of PR-historie, niet in de actuele handoff.

## Git-flow

Vóór implementatie:

1. controleer dat de basisbranch actueel en schoon is;
2. maak een featurebranch voor de goedgekeurde story;
3. controleer dat Claude's write-set op die featurebranch terechtkomt.

Na goedgekeurde implementatie en test:

1. controleer status en diff;
2. werk `README.md`, actieve release, geraakte epics, TODO, legacy-dekking en handoff bij;
3. commit en push de featurebranch;
4. maak of begeleid de PR;
5. controleer na merge `master` en een schone worktree;
6. kies de volgende story binnen de actieve release.
