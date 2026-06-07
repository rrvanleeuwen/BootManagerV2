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

## Vaste architectuurregels

- Target framework is .NET 8.
- Behoud de bestaande solution-, project-, feature- en naamgevingsstructuur.
- Respecteer de bestaande Clean Architecture-afhankelijkheden:
  - `BootManager.Core` bevat domeinobjecten en domeincontracten;
  - `BootManager.Application` bevat use-cases, applicatielogica, DTO's en applicatiecontracten;
  - `BootManager.Infrastructure` bevat EF Core, repositories, opslag en externe technische implementaties;
  - `BootManager.Web` bevat presentatie, Blazor-componenten, endpoints en composition root.
- Laat afhankelijkheden naar binnen wijzen; Core kent Application, Infrastructure en Web niet.
- Plaats geen domein- of applicatielogica in Razor-componenten, controllers of infrastructuurcode.
- Gebruik bestaande repository-, DI-, DTO-, configuratie- en featurepatronen voordat een nieuw patroon wordt geïntroduceerd.
- Voeg geen nieuwe dependency, framework, architectuurstijl of generieke abstractie toe zonder expliciete instructie in het implementation packet.
- Sluit nieuwe code aan op omliggende conventies en hergebruik bestaande voorzieningen waar dat logisch is.
- Voeg Nederlandse XML-documentatie toe aan nieuwe of aangepaste publieke C#-code waar die documentatie relevant is.
- Neem simulator-, parsing-, opslag- en testaanpassingen mee wanneer de goedgekeurde verticale slice die functioneel raakt.
- Houd interne canonieke meetwaarden gescheiden van presentatieconversies en gebruikersvoorkeuren.
- Bij twijfel of een wijziging de architectuur verandert: stop en meld de concrete ontwerpkeuze aan Codex in plaats van zelf een nieuw patroon te kiezen.

Het implementation packet kan voor een specifieke story aanvullende of strengere architectuurregels geven.

## Uitvoering

- De story is al goedgekeurd; formuleer haar niet opnieuw en vraag geen nieuw akkoord.
- Geef een kort uitvoeringsplan en implementeer daarna direct.
- Blijf binnen scope en verwachte write-set.
- Maak geen ongerelateerde refactors, formatting, upgrades of documentatieaanpassingen.
- Volg bestaande project- en architectuurpatronen.
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
