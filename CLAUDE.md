# Claude Code Instructions

Claude Code is de programmeur van BootManagerV2.

Codex beheert proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow. Claude voert alleen de codewijziging uit die in een goedgekeurd implementation packet staat.

## Startcontext

Lees alleen:

1. dit bestand;
2. het implementation packet;
3. de exact genoemde goedgekeurde storysectie;
4. de source files die het packet expliciet noemt.

Gebruik gerichte zoekopdrachten en kleine bestandssecties als aanvullende codecontext nodig is.

Lees niet standaard:

- `AGENTS.md`;
- `.codex/current-session-handoff.md`;
- `.codex/working-agreement.md`;
- TODO, roadmap of legacy-analyse;
- ongerelateerde epics;
- brede source trees.

## Uitvoering

- De story is al goedgekeurd; formuleer haar niet opnieuw en vraag geen nieuw akkoord.
- Geef een kort uitvoeringsplan en implementeer daarna direct.
- Blijf binnen scope en verwachte write-set.
- Maak geen ongerelateerde refactors, formatting, upgrades of documentatieaanpassingen.
- Volg bestaande project- en architectuurpatronen.
- Houd domein- en applicatielogica uit Razor-componenten waar praktisch mogelijk.
- Voeg gerichte tests toe die passen bij het gewijzigde gedrag.
- Draai eerst gerichte tests en daarna de voorgeschreven build/checks.
- Commit, push, PR, storystatus en projectdocumentatie horen niet bij Claude, tenzij het packet dat uitzonderlijk expliciet opdraagt.

## Oplevering

Rapporteer alleen:

- gewijzigde bestanden en gedrag;
- uitgevoerde tests/checks en resultaten;
- migratie- of configuratie-impact;
- resterende risico's en noodzakelijke handmatige test.

Bij blokkade: meld het concrete probleem en de kleinste ontbrekende beslissing. Verbreed de repositoryverkenning niet automatisch.
