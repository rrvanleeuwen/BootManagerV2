---

applyTo: "src/BootManager.Tools.\*/\*\*/\*.cs"

---



\# Instructies voor BootManager.Tools projecten



Deze instructies gelden voor alle toolprojecten onder `BootManager.Tools.\*`.



\## Doel van tools

\- Tools zijn zelfstandige uitvoerbare projecten voor ondersteunende taken zoals simulatie, ingest, import of debugging.

\- Houd tools praktisch en doelgericht.

\- Vermijd onnodige complexiteit.



\## Structuur

\- Sluit qua naamgeving aan op de bestaande `BootManager.Tools.\*` conventie.

\- Houd de structuur eenvoudig, bijvoorbeeld met mappen zoals:

&nbsp; - `Options`

&nbsp; - `Services`

&nbsp; - `Models`

&nbsp; - eventueel `Scenarios` of andere domeinspecifieke mappen als dat logisch is



\## Technische aanpak

\- Gebruik .NET 8.

\- Gebruik hosting en dependency injection als dat helpt om de tool netjes en beheersbaar te houden.

\- Gebruik eenvoudige logging naar console als eerste keuze, tenzij anders gevraagd.

\- Werk in kleine, testbare stappen.



\## Samenwerking met de rest van de oplossing

\- Laat tools niet rechtstreeks naar de database schrijven als daarvoor de afgesproken route via `BootManager.Web` bestaat.

\- Gebruik voor opslag of mutaties de bestaande Web API wanneer dat onderdeel is van de architectuur.

\- Houd de verantwoordelijkheid van de tool beperkt tot zijn eigen taak, zoals luisteren, simuleren, parsen of doorsturen.



\## Simulators en ingest-processen

\- Zorg dat simulators en consumers lokaal testbaar zijn.

\- Gebruik configureerbare opties via `appsettings.json`.

\- Houd netwerklogica bewust simpel in de eerste versie.

\- Bouw eerst een werkende verticale slice voordat er extra verfijning wordt toegevoegd.



\## Documentatie

\- Voeg op nieuwe of aangepaste code Nederlandse XML-documentatie toe op:

&nbsp; - klassen

&nbsp; - publieke methoden

&nbsp; - belangrijke private methoden als de logica niet triviaal is

\- Voeg alleen korte Nederlandse inline comments toe waar de logica niet direct vanzelfsprekend is.

