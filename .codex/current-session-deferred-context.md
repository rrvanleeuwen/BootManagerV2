# Current Codex Deferred Context

Dit bestand bevat context die niet iedere Codex-sessie nodig heeft, maar later wel
snel terugvindbaar moet zijn. Lees dit niet standaard bij sessiestart.

## Wanneer wel lezen

Lees dit bestand alleen bij concrete noodzaak, bijvoorbeeld voor:

- historische pilotbeslissingen of afgeronde storysamenvattingen;
- scan-, QR-, auth- of storage-grondslagen die opnieuw relevant worden;
- bekende baseline-teststatus of laatste verificatiehistorie;
- Raspberry Pi-, runtime-, acceptatie- of post-vakantie vervolgvragen.

## Afgeronde pilotbasis tot en met 2026-06-19

- `PILOT-SCAN-01` is handmatig geaccepteerd op de Raspberry Pi en beide Samsung-telefoons.
- `PILOT-AUTH-01` is technisch gecontroleerd en handmatig geaccepteerd op 2026-06-17.
- `PILOT-LOC-01` tot en met `PILOT-LOC-04` zijn technisch gecontroleerd en handmatig
  geaccepteerd op 2026-06-18 en 2026-06-19.
- De eerstvolgende geplande story blijft `PILOT-INV-01`.

## Samenvatting scan, auth en storage

- Scanbasis: QR- en productbarcodescan is voor de pilot bewezen op de goedgekeurde
  telefoons; de functionele details staan in het release-archief.
- Authbasis: lokaal Owner/Crew-model met aparte login voor Carla is gereed.
- Storagebasis: opslaggebieden, locaties, stabiele locatie-QR's, print/export,
  tokenvervanging en tagoverzicht zijn gereed.
- Navigatiebasis: opslag loopt nu via het Owner-only hoofdmenu `Opslag` met
  `Locaties` en `Tagoverzicht`.

## Smalle aanvullende fixes tijdens acceptatie

- `/_framework` is toegestaan in de Crew-PCR-gate.
- Open Blazor-sessies worden periodiek gevalideerd tegen `CredentialVersion`.

## Laatste verificatiehistorie

Laatste volledige handoffverificatie: 2026-06-19.

- Handmatige acceptatie van `PILOT-LOC-04` is geslaagd.
- Gerichte storage/navigation unit-tests voor de laatste navigatiecheck: 20/20.
- Eerdere bredere storage/tag unit-suite: 138/138.
- Gerichte storage-integratietests voor tokenvervanging en migratie: geslaagd.
- `dotnet build BootManager.sln --no-restore`: geslaagd.
- `git diff --check`: geslaagd.
- Bestaande repositorybrede baseline warnings buiten deze story blijven aanwezig.

## Verwijzingen

- Actuele release: `.docs/releases/holiday-pilot-2026.md`
- Archief afgeronde pilotstories:
  `.docs/releases/holiday-pilot-2026-archive-completed-stories.md`
- Huidige compacte sessiestart: `.codex/current-session-handoff.md`
