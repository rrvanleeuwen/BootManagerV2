# Current Codex Handoff

Updated: 2026-06-09.

## Rollen

- Claude Code is de programmeur en maakt uitsluitend code vanuit een goedgekeurd implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Er is geen actieve featurebranch; `master` is gelijk aan `origin/master`.
- `PILOT-SCAN-01` is afgerond, gemerged en op de Raspberry Pi uitgerold.
- De geïntegreerde scanflow is op de Samsung-telefoons van Roelof en Carla in Edge en Chrome geaccepteerd.

## Actuele productdoelstelling

De leidende productrelease is de **BootManager Holiday Pilot 2026** voor drie weken praktisch gebruik op Linde door Roelof en Carla.

Bron:

- `.docs/releases/holiday-pilot-2026.md`

Tot deze pilot gereed is kiest Codex geen story buiten deze release, tenzij:

- een blocker eerst opgelost moet worden;
- een noodzakelijke afhankelijkheid ontbreekt;
- de gebruiker expliciet een andere prioriteit vaststelt.

## Eerstvolgende actie

Bereid de eerstvolgende pilotstory `PILOT-AUTH-01` voor: Owner/Crew-model en een eigen
login voor Carla.

De productiecode en deterministische echte-module-harness zijn op 2026-06-09 technisch
geaccepteerd. De zes harnessscenario's slagen, inclusief sessie-isolatie,
supportrevisions, EAN-13-resultaatcleanup en idempotente native cleanup bij pending
detecties.

Geslaagde checks:

- JavaScript-syntaxcontrole productiecode en harness;
- `node test-final-verification-harness.js`: 6/6 scenario's;
- `dotnet build BootManager.sln --no-restore`: 0 warnings, 0 errors;
- publishcontrole geslaagd met bestaande waarschuwingen buiten deze story;
- simulator-tests 5/5; unit-tests 147/148 met alleen de bekende ongerelateerde
  `OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect` rood;
- `git diff --check`.

De relevante release-, TODO-, legacy- en testdocumentatie is bijgewerkt. PR #88 is
gemerged naar `master` als mergecommit `a8b5d96`.

Pi- en telefoonstatus:

- `master` is door de gebruiker handmatig op de Raspberry Pi uitgerold;
- HTTP-webapp en login zijn gecontroleerd; na het wissen van een oude browsercookie
  werkte de login normaal;
- HTTPS-scannen op de Samsung-telefoons van Roelof en Carla is in Edge en Chrome
  geslaagd, inclusief QR-codes en verschillende productbarcodes;
- `PILOT-SCAN-01` voldoet aan alle acceptatiecriteria en is op 2026-06-09 afgerond.

De volledige story, scope, buiten-scope en acceptatietest staan in `.docs/releases/holiday-pilot-2026.md`.

## Documentatieregel

Bij iedere pilotstory controleert en actualiseert Codex ook:

- `README.md`;
- `.docs/releases/holiday-pilot-2026.md`;
- relevante bestaande actuele userstories/epics;
- `.docs/TODO.md`;
- geraakte legacy-userstories en `legacy-coverage-register.md`;
- deze handoff;

Storystatus, voortgang en eerstvolgende story blijven in README en pilotdocument gelijk.
Documentatiewijzigingen worden na controle automatisch gecommit en naar de actuele
remote branch gepusht, tenzij de gebruiker expliciet anders vraagt of dit door de
branch/worktreestatus onveilig is.

Als dezelfde functionaliteit al in een bestaande of legacy-story staat, wordt die status bijgewerkt en wordt geen los tegenstrijdig verhaal achtergelaten.

## Laatste relevante productwijziging

De geïsoleerde native `BarcodeDetector`-test herkende EAN-13 `9789059965607` op de
Samsung-telefoon direct vanaf circa 15 cm bij 1080×1920. Daarom wordt de productiepagina
`/scan` aangepast naar één gedeelde camerastream met ZXing uitsluitend voor QR en native
`BarcodeDetector` uitsluitend voor EAN-13. Browsers zonder native EAN-13 houden QR en
handmatige invoer.

De productie-integratie, verificatieharness en documentatie staan op `master`.
`PILOT-SCAN-01` is volledig geaccepteerd; `PILOT-AUTH-01` is de volgende story.

## Relevante actuele documenten

- `.docs/releases/holiday-pilot-2026.md` — leidende release-scope en prioriteitsvolgorde;
- `.codex/working-agreement.md` — proces en administratieve afronding;
- `.codex/task-context-map.md` — contextkeuze per taaktype;
- `.docs/epics/digital-logbook.md` — bestaande logboekstatus;
- `.docs/legacy-analysis/legacy-coverage-register.md` — legacy-dekking;
- `.docs/TODO.md` — algemene backlog, ondergeschikt aan de actieve pilot.

## Handoffregel

Houd dit bestand kort. Bewaar alleen actuele branch, release-doel, blokkades, laatste relevante productwijziging en eerstvolgende actie. Historische details blijven in git, PR's, epics en release-documentatie.
