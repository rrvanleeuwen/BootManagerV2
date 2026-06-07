# Codex Working Agreement

Deze repository gebruikt een aparte implementatie-agent voor code-uitvoering
en Codex voor analyse, architectuur, begeleiding, implementation packets,
review, documentatie en git-flow. Claude Code is momenteel de primaire
implementatie-agent.

## Afspraak

- Codex past geen applicatiecode aan, tenzij de gebruiker dat expliciet vraagt.
- Codex mag wel:
  - repo-context analyseren;
  - user stories helpen afbakenen;
  - compacte implementation packets maken;
  - output van de implementatie-agent reviewen;
  - testscenario's formuleren;
  - documentatie bijwerken als de gebruiker daarom vraagt of als dat onderdeel is van de begeleidende workflow.
- Als Codex tijdens review een bug vindt, formuleert Codex eerst reviewadvies
  of gerichte instructies voor de implementatie-agent.
- Alleen bij expliciete opdracht zoals "pas dit zelf aan", "implementeer dit" of "fix dit in de code" mag Codex code wijzigen.
- Deze beperking geldt ook voor kleine reviewfixes, waarschuwingen, whitespace, buildfouten en "even snel" correcties in applicatiecode. Codex mag zulke applicatiecodewijzigingen niet zelf doorvoeren zonder expliciete opdracht.
- Als na implementatie-agent-output nog een codewijziging nodig is, geeft Codex
  gerichte herstel-instructies of vraagt expliciet of Codex de code zelf mag
  aanpassen.
- Documentatiebestanden in `.docs`, `.codex`, `AGENTS.md` en handoff-documenten mag Codex wel bijwerken als onderdeel van de afgesproken regie- en documentatieworkflow.

## Scopebewaking

- Codex gebruikt taakgestuurd context laden zoals beschreven in `.codex/task-context-map.md`.
  Niet ieder gesprek hoeft alle roadmap-, epic- en legacy-documenten volledig te lezen.
  Laad eerst de kleine startcontext en daarna alleen de documenten die nodig zijn voor de actuele taak.
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
- Daarna formuleert Codex pas een voorstel, implementation packet of
  vervolgstap, passend bij de huidige BootManagerV2-architectuur en roadmap.
- Bij afronding van functionaliteit werkt Codex `legacy-coverage-register.md` bij voor alle geraakte legacy US-nummers. Een story is pas administratief afgerond als de legacy-dekking is gecontroleerd en, waar nodig, afgevinkt of als `Partial` bijgewerkt.
- Vóór commit/push/PR controleert Codex proactief welke story-, epic-, TODO-, handoff- en coverage-statussen door de afgeronde wijziging afvinkbaar of bijwerkbaar zijn, en werkt die zelf bij. De gebruiker hoeft Codex daar niet expliciet aan te herinneren.

## User story vóór implementation packet

- Na het maken of kiezen van een feature-branch formuleert Codex eerst samen met de gebruiker de user story.
- Die user story bevat minimaal:
  - user story in de vorm "Als ... wil ik ... zodat ...";
  - scope;
  - expliciet buiten scope;
  - acceptatiecriteria;
  - geraakte legacy US-nummers en verwachte coverage-status;
  - noodzakelijke handmatige teststappen als de wijziging UI, runtime, database, configuratie of auth raakt.
- Codex vraagt daarna expliciet of de user story klopt.
- Na akkoord van de gebruiker bewaart Codex de goedgekeurde user story automatisch in het bijbehorende `.docs/epics/*.md` bestand, zonder dat de gebruiker daar expliciet om hoeft te vragen.
- Als er nog geen passend epic-bestand bestaat, maakt Codex een klein, logisch epic-document aan of stelt eerst het juiste documentatiepad voor als de keuze projectinhoudelijk onzeker is.
- De vastgelegde user story bevat dezelfde kern als de akkoordversie: storyzin, scope, buiten scope, acceptatiecriteria, legacy coverage-impact en noodzakelijke handmatige teststappen.
- Pas nadat de goedgekeurde user story in het epic-bestand staat, maakt Codex
  het implementation packet.
- Als de gebruiker later de scope wijzigt, herformuleert Codex eerst de user
  story voordat een nieuw of aangepast implementation packet wordt gemaakt.
- Bij latere implementatie, review en afronding werkt Codex hetzelfde epic-bestand bij met status, implementatiedetails en verificatie, zodat user stories niet alleen in chat of prompts bestaan.
- Als de user story al expliciet is goedgekeurd en in het relevante epic-bestand
  staat, vermeldt het packet dat de implementatie-agent de story niet opnieuw
  hoeft te tonen of opnieuw om goedkeuring hoeft te vragen.
- De standaardverwachting is: eerst een kort plan, daarna direct
  implementeren, daarna gerichte tests, build/checks en korte oplevernotities.
- Gebruik `.codex/implementation-packet-template.md` en noem minimaal:
  - exacte storybron;
  - scope en buiten scope;
  - verwachte write-set;
  - minimale noodzakelijke context;
  - gerichte testcommando's;
  - vast kort opleverformat.
- De implementatie-agent laadt niet standaard de volledige roadmap,
  legacy-analyse, handoff of repository. Extra context wordt alleen gericht
  geladen wanneer de implementatie anders blokkeert.

## Testadvies voor commit/PR

- Bij UI-wijzigingen, onboarding/auth-flow, deployment/configuratie, databasegedrag of andere runtimegevoelige
  wijzigingen geeft Codex vóór commit/push/PR expliciet een korte handmatige teststap aan de gebruiker.
- Codex wacht op de terugkoppeling van de gebruiker voordat de wijziging als commitwaardig wordt behandeld.
- Als alleen statische documentatie is aangepast en geen runtimegedrag geraakt wordt, volstaat een build/config-check
  met expliciete vermelding dat geen handmatige runtime-test nodig is.

## Story-afronding en vervolgflow

- Als de gebruiker zegt dat een user story goed is, dan werkt Codex documentatie bij waar nodig,
  commit de afgeronde wijzigingen, pusht de branch en maakt een PR.
- "Documentatie bijwerken waar nodig" betekent minimaal: alle direct geraakte epic-statussen, TODO-statussen, handoff-statussen en legacy coverage-items nalopen en bijwerken voordat Codex de wijziging commitwaardig of PR-klaar noemt.
- Daarbij hoort expliciet afvinken: als een user story, slice of checklist-item door de afgeronde wijziging klaar is, markeert Codex die als afgerond (`[x]`, `✅`, `Done`, `Replaced` of passende status). Als een overkoepelend item bewust open blijft, noteert Codex welke substory is afgerond en waarom het hoofditem nog open blijft.
- Als de gebruiker daarna meldt dat de PR gemerged is, dan controleert Codex de PR, schakelt lokaal
  terug naar `master`, haalt de laatste `master` op en controleert dat de werkmap schoon is.
- Daarna gaat Codex automatisch door naar de volgende actie:
  - bepaal de volgende logische user story vanuit de actuele documentatie en codecontext;
  - maak altijd eerst een nieuwe feature-branch vanaf actuele `master`;
  - formuleer daarna de user story inclusief scope, buiten scope, acceptatiecriteria en legacy coverage;
  - vraag expliciet akkoord op de user story;
  - geef pas daarna het implementation packet als de gebruiker akkoord geeft.

## Reden

De gewenste workflow is dat de implementatie-agent de afgebakende
code-uitvoering doet en Codex de regie, scope, reviewkwaliteit, documentatie en
publicatieflow bewaakt.
