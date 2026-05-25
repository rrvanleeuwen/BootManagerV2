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

## Scopebewaking

- Bij elk nieuw idee van de gebruiker, elk verzoek om een vervolgstory en elke situatie waarin Codex zelf een volgende stap voorstelt, controleert Codex actief de legacy-scope analyse zonder dat de gebruiker daar expliciet om hoeft te vragen.
- De vaste scopebronnen zijn:
  - `.docs/legacy-analysis/scope-inventory.md` voor de volledige legacy functionele scope;
  - `.docs/legacy-analysis/mapped-epics.md` voor mapping naar BootManagerV2-status;
  - `.docs/legacy-analysis/legacy-coverage-register.md` voor story-level afvinkstatus per legacy US;
  - `.docs/legacy-analysis/proposed-backlog.md` voor voorgestelde BootManagerV2-slices;
  - `.docs/legacy-analysis/implemented-or-obsolete.md` voor wat al afgedekt, vervangen, geparkeerd of niet meer relevant is.
- Codex bepaalt eerst of een idee:
  - al in legacy-scope gedefinieerd is;
  - al geheel of gedeeltelijk in BootManagerV2 bestaat;
  - bewust geparkeerd is;
  - afhankelijk is van andere modules;
  - of echt nieuwe scope is.
- Daarna formuleert Codex pas een voorstel, Copilot-prompt of vervolgstap, passend bij de huidige BootManagerV2-architectuur en roadmap.
- Bij afronding van functionaliteit werkt Codex `legacy-coverage-register.md` bij voor alle geraakte legacy US-nummers. Een story is pas administratief afgerond als de legacy-dekking is gecontroleerd en, waar nodig, afgevinkt of als `Partial` bijgewerkt.

## User story vóór Copilot-prompt

- Na het maken of kiezen van een feature-branch formuleert Codex eerst samen met de gebruiker de user story.
- Die user story bevat minimaal:
  - user story in de vorm "Als ... wil ik ... zodat ...";
  - scope;
  - expliciet buiten scope;
  - acceptatiecriteria;
  - geraakte legacy US-nummers en verwachte coverage-status;
  - noodzakelijke handmatige teststappen als de wijziging UI, runtime, database, configuratie of auth raakt.
- Codex vraagt daarna expliciet of de user story klopt.
- Pas na akkoord van de gebruiker maakt Codex de Copilot-prompt.
- Als de gebruiker later de scope wijzigt, herformuleert Codex eerst de user story voordat een nieuwe of aangepaste Copilot-prompt wordt gemaakt.

## Testadvies voor commit/PR

- Bij UI-wijzigingen, onboarding/auth-flow, deployment/configuratie, databasegedrag of andere runtimegevoelige
  wijzigingen geeft Codex vóór commit/push/PR expliciet een korte handmatige teststap aan de gebruiker.
- Codex wacht op de terugkoppeling van de gebruiker voordat de wijziging als commitwaardig wordt behandeld.
- Als alleen statische documentatie is aangepast en geen runtimegedrag geraakt wordt, volstaat een build/config-check
  met expliciete vermelding dat geen handmatige runtime-test nodig is.

## Story-afronding en vervolgflow

- Als de gebruiker zegt dat een user story goed is, dan werkt Codex documentatie bij waar nodig,
  commit de afgeronde wijzigingen, pusht de branch en maakt een PR.
- Als de gebruiker daarna meldt dat de PR gemerged is, dan controleert Codex de PR, schakelt lokaal
  terug naar `master`, haalt de laatste `master` op en controleert dat de werkmap schoon is.
- Daarna gaat Codex automatisch door naar de volgende actie:
  - bepaal de volgende logische user story vanuit de actuele documentatie en codecontext;
  - maak altijd eerst een nieuwe feature-branch vanaf actuele `master`;
  - formuleer daarna de user story inclusief scope, buiten scope, acceptatiecriteria en legacy coverage;
  - vraag expliciet akkoord op de user story;
  - geef pas daarna de Copilot-prompt als de gebruiker akkoord geeft.

## Reden

De gewenste workflow is dat Copilot de code-implementatie doet en Codex de regie,
controle en promptkwaliteit bewaakt.
