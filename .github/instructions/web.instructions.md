---

applyTo: "src/BootManager.Web/\*\*/\*.cs"

---



\# Instructies voor BootManager.Web



Deze instructies gelden voor code in `BootManager.Web`.



\## Rol van BootManager.Web

\- `BootManager.Web` is de web- en API-laag van de oplossing.

\- Gebruik deze laag voor HTTP endpoints, webspecifieke afhandeling en de toegang van externe processen tot de applicatie.

\- Houd deze laag dun en laat businesslogica zoveel mogelijk in `BootManager.Application`.



\## Controllers en endpoints

\- Houd controllers klein, overzichtelijk en taakgericht.

\- Laat controllers vooral:

&nbsp; - input ontvangen

&nbsp; - application services aanroepen

&nbsp; - HTTP responses teruggeven

\- Plaats geen complexe businesslogica in controllers.

\- Gebruik duidelijke en consistente routes en endpointnamen.

\- Sluit aan op de bestaande stijl van controllers in deze solution.



\## Samenwerking met Application

\- Laat controllers samenwerken met services uit `BootManager.Application`.

\- Gebruik DTO’s uit `BootManager.Application` als contract voor input en output, tenzij er een duidelijke reden is om daarvan af te wijken.

\- Voeg geen domein- of persistence-logica toe in de weblaag als die al in Application of Infrastructure hoort.



\## Database en infrastructuur

\- Laat `BootManager.Web` niet zelf direct databasecode bevatten als daar al services of repositories voor bestaan in de bestaande architectuur.

\- Gebruik de bestaande dependency injection en application services om data op te slaan of op te halen.

\- Laat externe tools of processen via deze Web API communiceren als dat de afgesproken architectuur is.



\## Validatie en responses

\- Controleer input op een manier die past bij de bestaande oplossing.

\- Geef duidelijke en passende HTTP responses terug.

\- Houd foutafhandeling eenvoudig en consistent met de rest van de oplossing.

\- Voeg geen zware response-abstracties toe zonder expliciete reden.



\## Structuur

\- Plaats API-controllers in de bestaande map `Controllers`.

\- Sluit qua naamgeving aan op bestaande controllers, bijvoorbeeld `<Feature>Controller`.

\- Houd wijzigingen lokaal en voorkom onnodige refactors in de weblaag.



\## Documentatie

\- Voeg op nieuwe of aangepaste code Nederlandse XML-documentatie toe op:

&nbsp; - controllers

&nbsp; - publieke acties

&nbsp; - relevante publieke members

\- Voeg alleen korte Nederlandse inline comments toe waar de logica of route-afhandeling niet direct vanzelfsprekend is.



\## Grenzen

\- Voeg geen nieuwe businesslogica toe in BootManager.Web als die in `BootManager.Application` thuishoort.

\- Voeg geen EF Core configuratie of repository-implementaties toe in BootManager.Web.

\- Vermijd code die de lagenstructuur van de oplossing doorbreekt.

