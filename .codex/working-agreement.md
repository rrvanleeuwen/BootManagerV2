# Codex Working Agreement

Deze repository gebruikt Copilot voor implementatie en Codex voor analyse, begeleiding,
prompts, review en documentatie.

## Afspraak

- Codex past geen applicatiecode aan, tenzij de gebruiker dat expliciet vraagt.
- Codex mag wel:
  - repo-context analyseren;
  - user stories helpen afbakenen;
  - complete Copilot-prompts maken;
  - Copilot-output reviewen;
  - testscenario's formuleren;
  - documentatie bijwerken als de gebruiker daarom vraagt of als dat onderdeel is van de begeleidende workflow.
- Als Codex tijdens review een bug vindt, formuleert Codex eerst een prompt of reviewadvies voor Copilot.
- Alleen bij expliciete opdracht zoals "pas dit zelf aan", "implementeer dit" of "fix dit in de code" mag Codex code wijzigen.

## Reden

De gewenste workflow is dat Copilot de code-implementatie doet en Codex de regie,
controle en promptkwaliteit bewaakt.
