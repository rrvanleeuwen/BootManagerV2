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

## Story-afronding en vervolgflow

- Als de gebruiker zegt dat een user story goed is, dan werkt Codex documentatie bij waar nodig,
  commit de afgeronde wijzigingen, pusht de branch en maakt een PR.
- Als de gebruiker daarna meldt dat de PR gemerged is, dan controleert Codex de PR, schakelt lokaal
  terug naar `master`, haalt de laatste `master` op en controleert dat de werkmap schoon is.
- Daarna gaat Codex automatisch door naar de volgende actie:
  - bepaal de volgende logische user story vanuit de actuele documentatie en codecontext;
  - maak altijd eerst een nieuwe feature-branch vanaf actuele `master`;
  - leg daarna aan de gebruiker voor wat de volgende user story zou kunnen zijn;
  - geef pas daarna de Copilot-prompt als de gebruiker daarom vraagt of akkoord geeft.

## Reden

De gewenste workflow is dat Copilot de code-implementatie doet en Codex de regie,
controle en promptkwaliteit bewaakt.
