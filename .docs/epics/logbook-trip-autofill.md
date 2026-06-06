# Digitaal Logboek: Reisgegevens automatisch voorinvullen

**Status:** Voorgesteld op 2026-06-06 naar aanleiding van praktijkgebruik tijdens varen.

## Relatie met bestaande epic

Deze story is een verfijning van `.docs/epics/digital-logbook.md`, in het bijzonder:

- `Story 1 - Reis aanmaken en beheren`;
- de reis-header en reis-samenvatting;
- het uitgangspunt dat automatische waarden mogen worden voorgesteld en dat de gebruiker deze kan overschrijven.

## Aanleiding

Bij het aanmaken en afronden van een reis vraagt BootManager verschillende gegevens die het systeem geheel of gedeeltelijk al kent. De gebruiker zou deze waarden niet opnieuw handmatig hoeven overnemen.

Voorbeelden:

- de bootnaam is bekend vanuit de actieve boot;
- de startstand van de motoruren kan worden overgenomen van de eindstand van de vorige afgeronde reis;
- vertrek- en aankomsttijd zijn bekend wanneer de reis in BootManager wordt gestart en gestopt;
- totale verstreken reistijd kan uit vertrek- en aankomsttijd worden berekend;
- gevaren tijd kan worden afgeleid uit beschikbare meetdata en/of geregistreerde vaarstatus;
- begin- en eindlocatie kunnen waar mogelijk uit positiegegevens worden voorgesteld;
- logstand, gevaren afstand en andere samenvattingswaarden kunnen waar mogelijk uit meetdata of de vorige reis worden afgeleid.

Het uitgangspunt is: **alles wat BootManager betrouwbaar kan weten of berekenen, wordt als voorstel ingevuld; de gebruiker houdt altijd de mogelijkheid om het te corrigeren of te overschrijven.**

---

## LOG-TRIP-AUTO-1: Reisgegevens automatisch voorinvullen

**Status:** Voorgesteld op 2026-06-06.

**User story:** Als gebruiker wil ik dat BootManager bij het starten, bewerken en afronden van een reis alle bekende of betrouwbaar afleidbare reisgegevens automatisch voorinvult, zodat ik zo weinig mogelijk dubbel hoef in te voeren en alleen uitzonderingen of correcties handmatig hoef vast te leggen.

### Functionele uitgangspunten

- Automatisch ingevulde waarden zijn voorstellen.
- De gebruiker kan iedere voorgestelde waarde wijzigen voordat de reis definitief wordt opgeslagen of afgerond.
- Handmatig gewijzigde waarden worden niet opnieuw stilzwijgend overschreven door automatische berekeningen.
- Het systeem maakt zichtbaar welke waarden automatisch zijn voorgesteld en welke handmatig zijn aangepast, wanneer dat zonder onnodige UI-drukte mogelijk is.
- Als een waarde niet betrouwbaar kan worden bepaald, blijft deze leeg of wordt duidelijk als schatting gemarkeerd.

### Automatisch voor te vullen gegevens

#### Bij het aanmaken of starten van een reis

- **Boot**: actieve/geselecteerde boot.
- **Reisnaam**: voorstel op basis van datum en eventueel vertrek- en bestemmingsplaats, bijvoorbeeld `Lemmer - Enkhuizen, 6 juni 2026`; gebruiker kan dit wijzigen.
- **Datum**: lokale boorddatum bij starten van de reis.
- **Vertrektijd**: lokale boordtijd waarop de gebruiker de reis start.
- **Van**:
  - laatst bekende haven/plaats van de vorige afgeronde reis, indien beschikbaar;
  - anders een voorstel op basis van actuele of recentste GPS-positie, wanneer locatievertaling beschikbaar is;
  - anders leeg.
- **Bootnaam**: afkomstig uit bootconfiguratie en niet opnieuw handmatig vereist.
- **Motoruren start**:
  - primair de motoruren-eindstand van de meest recente afgeronde reis voor dezelfde boot;
  - later eventueel uit een motorurensensor wanneer die beschikbaar en betrouwbaarder is;
  - gebruiker kan de voorgestelde stand corrigeren.
- **Logstand start**:
  - eindstand van de vorige afgeronde reis, indien aanwezig;
  - anders beschikbare log-/distance-meetwaarde;
  - anders leeg.
- **Bemanning**:
  - eventueel laatst gebruikte bemanningssamenstelling als voorstel;
  - gebruiker bevestigt of wijzigt deze.

#### Tijdens de reis

- Werk vertrek- of reistijden niet achteraf automatisch bij nadat de gebruiker ze handmatig heeft aangepast.
- Houd voldoende gegevens bij om bij afronding reistijd, vaartijd, afstand en samenvatting te kunnen berekenen.
- Gebruik UTC intern en lokale boordtijd in de UI.

#### Bij het stoppen of afronden van een reis

- **Aankomsttijd**: lokale boordtijd waarop de gebruiker de reis stopt of afrondt.
- **Naar**:
  - voorstel op basis van actuele/eindpositie of bekende haven/plaats;
  - gebruiker kan dit overschrijven.
- **Totale reisduur**: `aankomsttijd - vertrektijd`.
- **Gevaren tijd / vaartijd**:
  - berekend over perioden waarin de boot aantoonbaar voer;
  - indien nog geen betrouwbare vaarstatuslogica bestaat, eerste slice als verstreken tijd tussen vertrek en aankomst, duidelijk benoemd als reistijd;
  - latere verfijning mag motor-, snelheid-door-water-, SOG- of statusdata combineren om stilstand uit te sluiten.
- **Motoruren eind**:
  - voorstel op basis van motorurensensor als die beschikbaar is;
  - anders door gebruiker in te voeren;
  - nooit zonder betrouwbare bron verzinnen.
- **Motoruren gebruikt**: verschil tussen eind- en startstand, als beide aanwezig en geldig zijn.
- **Logstand eind**:
  - voorstel uit beschikbare log-/distance-meetwaarde;
  - anders handmatige invoer.
- **Gelogde/gevaren afstand**:
  - verschil tussen eind- en beginlogstand als beide beschikbaar zijn;
  - of berekend uit betrouwbare afstandsmetingen/positiegegevens;
  - bron en afronding moeten eenduidig zijn.
- **Brandstof**:
  - alleen automatisch voorstellen als voldoende betrouwbare tank- of verbruiksdata bestaat;
  - anders handmatig laten invullen.

### Definitie van tijdsvelden

Om verwarring te voorkomen worden de volgende begrippen apart behandeld:

- **Reisduur**: tijd tussen vertrek en aankomst, inclusief stilliggen tijdens de reis.
- **Vaartijd**: tijd waarin de boot daadwerkelijk onderweg was volgens een vastgelegde detectieregel.
- **Motorlooptijd**: tijd waarin de motor draaide, alleen wanneer een betrouwbare motorstatus of motorurenteller beschikbaar is.

BootManager mag deze begrippen niet als synoniemen tonen. In de eerste implementatieslice mag alleen reisduur automatisch worden berekend als vaartijd nog niet betrouwbaar kan worden vastgesteld.

### Betrouwbaarheid en bronprioriteit

Per automatisch veld wordt een vaste bronprioriteit gedocumenteerd. Voorbeelden:

1. expliciete betrouwbare sensorwaarde;
2. opgeslagen eindwaarde van de vorige afgeronde reis voor dezelfde boot;
3. berekende waarde uit meetdata;
4. laatst gebruikte gebruikerswaarde;
5. leeg laten.

Een lagere prioriteit mag een handmatig bevestigde of handmatig gewijzigde waarde niet overschrijven.

### UI-gedrag

- Velden zijn vooraf gevuld wanneer een voorstel beschikbaar is.
- Alle vooraf ingevulde velden blijven bewerkbaar.
- Een waarde die de gebruiker wijzigt, krijgt intern de status handmatig/overschreven.
- Een knop of actie zoals `Voorstellen opnieuw berekenen` mag worden toegevoegd, maar alleen na expliciete gebruikersactie.
- Bij afronden toont BootManager een compacte samenvatting van berekende tijden, afstanden en standen voordat de reis definitief wordt opgeslagen.
- Onzekere of geschatte waarden worden herkenbaar weergegeven, bijvoorbeeld met `Geschat` of een informatie-icoon.

### Architectuurregels

- Automatische voorinvulling en berekeningen horen in de Application-laag, niet rechtstreeks in Razor-componenten.
- Gebruik een aparte service of duidelijke feature-service voor tripvoorstellen, bijvoorbeeld `ILogbookTripSuggestionService`.
- De service retourneert voorstellen met waarde, bron en betrouwbaarheid/status.
- Bestaande handmatige waarden blijven leidend.
- Berekeningen zijn deterministisch en unit-testbaar.
- Tijden worden intern in UTC opgeslagen en via de bestaande boordtijdvoorziening lokaal weergegeven.
- De oplossing blijft .NET 8 gebruiken en sluit aan op de bestaande Clean Architecture- en featurestructuur.
- Nieuwe of aangepaste C#-code krijgt Nederlandse XML-documentatie waar relevant.

### Buiten scope eerste slice

- Geen automatische herkenning van havennamen als daarvoor nog geen betrouwbare lokale of externe locatievoorziening bestaat.
- Geen kunstmatige invulling van motoruren zonder sensor of vorige bekende eindstand.
- Geen complexe route-analyse om iedere korte stop of manoeuvre te classificeren.
- Geen stilzwijgende wijziging van reeds afgeronde reizen.
- Geen automatische definitieve afronding zonder gebruikersbevestiging.
- Geen AI-gegenereerde reisbeschrijving in deze story.

### Acceptatiecriteria

- Bij een nieuwe reis wordt de actieve boot automatisch geselecteerd en getoond.
- Datum en vertrektijd worden voorgesteld op basis van het startmoment in lokale boordtijd.
- Motoruren start worden, indien beschikbaar, gevuld met motoruren eind van de vorige afgeronde reis van dezelfde boot.
- Logstand start wordt, indien beschikbaar, gevuld vanuit de vorige eindstand of een betrouwbare actuele bron.
- Bij stoppen wordt aankomsttijd automatisch voorgesteld.
- Reisduur wordt correct berekend uit vertrek en aankomst.
- Vaartijd wordt alleen afzonderlijk ingevuld als daarvoor een gedocumenteerde betrouwbare detectieregel bestaat; anders blijft het veld leeg of wordt reisduur expliciet als zodanig getoond.
- Begin- en eindlocatie worden alleen voorgesteld wanneer een bruikbare bron beschikbaar is.
- Automatisch voorgestelde waarden zijn door de gebruiker te wijzigen.
- Na handmatige wijziging overschrijft een normale refresh of nieuwe meetdata de waarde niet.
- Definitief opslaan bewaart de door de gebruiker bevestigde waarden.
- Ontbrekende brondata veroorzaakt geen fout en levert geen verzonnen waarde op.
- Bestaande reizen en logboekregels blijven functioneren.
- Build en relevante unit-/integratietests slagen.

### Automatische testgevallen

- Nieuwe reis gebruikt actieve boot en huidige boorddatum/-tijd.
- Vorige afgeronde reis levert motoruren eind als nieuwe motoruren start.
- Een vorige reis van een andere boot wordt niet gebruikt.
- Geen vorige reis resulteert in een leeg motoruren-startveld.
- Reisduur wordt correct berekend over dezelfde dag en over middernacht.
- Ongeldige aankomst vóór vertrek wordt afgekeurd of duidelijk gevalideerd.
- Handmatig gewijzigde waarde blijft behouden wanneer voorstellen opnieuw worden geladen.
- Verschilberekening voor motoruren en logstand werkt alleen met geldige begin- en eindwaarden.
- Ontbrekende metingen geven een leeg voorstel en geen exception.
- UTC-opslag en lokale boordtijdweergave blijven correct.

### Handmatige acceptatietest

1. Rond een eerste reis af met bekende motoruren eind en logstand eind.
2. Maak voor dezelfde boot een nieuwe reis aan.
3. Controleer dat boot, datum, vertrektijd, motoruren start en logstand start waar mogelijk zijn ingevuld.
4. Wijzig motoruren start handmatig.
5. Refresh of laat nieuwe meetdata binnenkomen en controleer dat de handmatige waarde behouden blijft.
6. Start en stop de reis.
7. Controleer dat aankomsttijd en reisduur zijn ingevuld.
8. Controleer dat vaartijd niet ten onrechte gelijk wordt gesteld aan reisduur wanneer stilstand niet betrouwbaar wordt gedetecteerd.
9. Pas een automatisch voorgestelde eindwaarde aan en rond de reis af.
10. Maak opnieuw een reis aan en controleer dat de bevestigde eindstand van de vorige reis als nieuwe beginstand wordt gebruikt.

### Legacy-impact

- Verfijnt `Story 1 - Reis aanmaken en beheren` uit de actieve epic Digitaal Logboek.
- Ondersteunt het doel dat automatische bootdata wordt gecombineerd met handmatige aanvulling.
- Verbetert de reis-header en reis-samenvatting zonder de bestaande logboekregelworkflow te vervangen.
