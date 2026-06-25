# User Preference Update

## Context

Deze notitie legt een expliciete gebruikersvoorkeur vast na de huidige `PILOT-UX-01`
home-implementatie.

De huidige homezoekflow laat een klik op een product nog doorlopen naar:

- direct de locatiepagina bij exact één actieve locatie; of
- een locatiekeuzestaat bij meerdere actieve locaties.

Dat was een eerdere werkhypothese, maar is **niet** de gewenste eindrichting volgens de
gebruiker.

## Expliciete gebruikerswens

Bij klikken op een product in het homezoekresultaat wil de gebruiker **niet** direct
naar een locatiepagina.

Gewenste richting:

- open een productgerichte pagina of werkcontext met alle relevante informatie van dat
  product;
- toon daar de relevante productinformatie en locatie-informatie bij elkaar;
- maak vanuit die productcontext direct de vervolgflow mogelijk om verbruik te
  registreren.

## Wat dit betekent voor vervolgwerk

- Behandel de huidige homeklik-naar-locatie-flow als **tijdelijke tussenstap**, niet
  als bekrachtigde UX-beslissing.
- Pas deze voorkeur niet stilzwijgend weg in een losse kleine fix.
- Gebruik deze notitie als leidende gebruikerscorrectie bij de eerstvolgende
  Claude-opdracht die de homezoekflow, productdetailflow of verbruiksflow raakt.
- Als dit functioneel doorwerkt in `PILOT-UX-01`, `PILOT-INV-06` of een nieuwe kleine
  vervolgstory, moet die opdracht expliciet uitgaan van:
  - product eerst;
  - locatie daarna als context;
  - verbruik direct bereikbaar vanuit de productcontext.

## Niet nu aanpassen

- De gebruiker heeft expliciet gevraagd om de huidige wijziging nu te laten staan.
- Deze notitie is dus een instructie voor de **volgende** Claude-ronde, niet voor een
  onmiddellijke codewijziging in deze beurt.
