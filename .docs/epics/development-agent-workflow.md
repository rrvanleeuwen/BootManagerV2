# Epic: Development Agent Workflow

**Datum:** 2026-06-07
**Status:** Geïmplementeerd op 2026-06-07.

## DEV-AGENT-1: Resourcezuinige Claude Code-workflow

**User story:** Als projecteigenaar wil ik een programmeeragent-neutrale en
resourcezuinige workflow voor Claude Code, zodat Codex architectuur, scope,
review en proces bewaakt terwijl Claude alleen gerichte implementatietaken
uitvoert.

### Scope

- Maak `AGENTS.md` en `.codex/working-agreement.md`
  programmeeragent-neutraal.
- Leg Claude Code vast als de huidige primaire implementatie-agent.
- Voeg een compacte root `CLAUDE.md` toe.
- Breid `.codex/task-context-map.md` uit met minimale context voor
  implementatietaken.
- Voeg een herbruikbaar implementation-packet-template toe.
- Houd Codex verantwoordelijk voor scope, storygoedkeuring, review,
  documentatie, handmatige testregie en git/PR-flow.
- Laat de implementatie-agent alleen de goedgekeurde story, expliciet genoemde
  bestanden en noodzakelijke codecontext lezen.

### Buiten scope

- Geen applicatiecode wijzigen.
- Geen functionele roadmap- of legacy-status wijzigen.
- Geen historische Copilot-vermeldingen in afgeronde stories herschrijven.
- Geen verplichting om Claude Code te gebruiken als later een andere
  implementatie-agent geschikter wordt.

### Acceptatiecriteria

- Actuele procesdocumentatie gebruikt `implementatie-agent` als generieke rol.
- `CLAUDE.md` is compact en verwijst naar bestaande bronbestanden in plaats van
  projectcontext te dupliceren.
- Een goedgekeurde story hoeft niet opnieuw te worden getoond of goedgekeurd.
- Een implementation packet bevat exacte scope, buiten scope, verwachte
  write-set, relevante context, testcommando's en opleverformat.
- Claude laadt niet standaard de volledige roadmap, legacy-analyse of
  repository.
- Claude geeft eerst een kort plan, implementeert daarna direct en rapporteert
  alleen wijzigingen, tests en resterende risico's.
- Codex reviewt de diff en vraagt expliciete toestemming voordat Codex zelf
  applicatiecode corrigeert.

### Legacy-impact

Geen. Dit is uitsluitend proces- en agentdocumentatie.

### Verificatie

- Controleer actuele workflowbestanden op normatieve Copilot-verwijzingen.
- Controleer links en genoemde paden.
- `git diff --check` moet schoon zijn.
- Geen handmatige runtime-test nodig.

### Implementatie

- `AGENTS.md` gebruikt een generieke implementatie-agentrol en bevat
  afzonderlijke startregels voor Codex en Claude Code.
- `CLAUDE.md` beperkt Claude tot packet, goedgekeurde story en expliciet
  genoemde bronbestanden.
- `.codex/implementation-packet-template.md` legt scope, write-set, context,
  checks en opleverformat vast.
- `.codex/working-agreement.md` en `.codex/task-context-map.md` zijn bijgewerkt
  voor de nieuwe rolverdeling.
- Historische Copilot-vermeldingen buiten de actuele workflow zijn niet
  herschreven.
- Geen applicatiecode, roadmapstatus of legacy coverage gewijzigd.
