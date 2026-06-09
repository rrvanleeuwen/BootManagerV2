# Current Codex Handoff

Updated: 2026-06-09.

## Rollen

- Claude Code is de programmeur en maakt uitsluitend code vanuit een goedgekeurd implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve featurebranch: `feature/pilot-scan-01`.
- `PILOT-SCAN-01` is geïmplementeerd en lokaal op de laptop geaccepteerd.
- Branch moet na commit/push tijdelijk op de Raspberry Pi worden getest.

## Actuele productdoelstelling

De leidende productrelease is de **BootManager Holiday Pilot 2026** voor drie weken praktisch gebruik op Linde door Roelof en Carla.

Bron:

- `.docs/releases/holiday-pilot-2026.md`

Tot deze pilot gereed is kiest Codex geen story buiten deze release, tenzij:

- een blocker eerst opgelost moet worden;
- een noodzakelijke afhankelijkheid ontbreekt;
- de gebruiker expliciet een andere prioriteit vaststelt.

## Eerstvolgende actie

Push de technisch geaccepteerde featurebranch en voer daarna de handmatige productietest
op de Raspberry Pi uit.

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

De relevante release-, TODO-, legacy- en testdocumentatie is bijgewerkt. Open voor
storyacceptatie: commit/push en de volledige QR-/EAN-13-productietest op de Raspberry Pi
in Edge en Chrome op beide telefoons, inclusief ingest-regressie via HTTP en HTTPS.

De volledige story, scope, buiten-scope en acceptatietest staan in `.docs/releases/holiday-pilot-2026.md`.

## Documentatieregel

Bij iedere pilotstory controleert en actualiseert Codex ook:

- relevante bestaande actuele userstories/epics;
- `.docs/TODO.md`;
- geraakte legacy-userstories en `legacy-coverage-register.md`;
- deze handoff;
- README-status wanneer cijfers wijzigen.

Als dezelfde functionaliteit al in een bestaande of legacy-story staat, wordt die status bijgewerkt en wordt geen los tegenstrijdig verhaal achtergelaten.

## Laatste relevante productwijziging

De geïsoleerde native `BarcodeDetector`-test herkende EAN-13 `9789059965607` op de
Samsung-telefoon direct vanaf circa 15 cm bij 1080×1920. Daarom wordt de productiepagina
`/scan` aangepast naar één gedeelde camerastream met ZXing uitsluitend voor QR en native
`BarcodeDetector` uitsluitend voor EAN-13. Browsers zonder native EAN-13 houden QR en
handmatige invoer.

Claude heeft hiervoor niet-gecommitte wijzigingen gemaakt in `Scan.razor`,
`Scan.razor.css`, `barcodeScanner.js` en de lokale verificatieharness. De implementatie is
technisch geaccepteerd, maar nog niet administratief afgerond, gecommit, gepusht of op de
Raspberry Pi geaccepteerd.

## Relevante actuele documenten

- `.docs/releases/holiday-pilot-2026.md` — leidende release-scope en prioriteitsvolgorde;
- `.codex/working-agreement.md` — proces en administratieve afronding;
- `.codex/task-context-map.md` — contextkeuze per taaktype;
- `.docs/epics/digital-logbook.md` — bestaande logboekstatus;
- `.docs/legacy-analysis/legacy-coverage-register.md` — legacy-dekking;
- `.docs/TODO.md` — algemene backlog, ondergeschikt aan de actieve pilot.

## Handoffregel

Houd dit bestand kort. Bewaar alleen actuele branch, release-doel, blokkades, laatste relevante productwijziging en eerstvolgende actie. Historische details blijven in git, PR's, epics en release-documentatie.
