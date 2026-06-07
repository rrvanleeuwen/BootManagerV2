# Digitaal Logboek: Reisgegevens automatisch voorinvullen

**Status:** Overkoepelende backlogstory, voorgesteld op 2026-06-06 naar aanleiding van praktijkgebruik tijdens varen. Eerste implementatieslice `LOG-TRIP-AUTO-1A` geïmplementeerd en handmatig geaccepteerd op 2026-06-07.

Dit document is de canonieke, volledige scopebeschrijving. De korte functionele
samenvatting in `logbook-trip-autofill-functional.md` introduceert geen
afzonderlijke story of implementatiegoedkeuring.

## Relatie met bestaande epic

Deze story is een verfijning van `.docs/epics/digital-logbook.md`, in het bijzonder:

- `Story 1 - Reis aanmaken en beheren`;
- de reis-header en reis-samenvatting;
- het uitgangspunt dat automatische waarden mogen worden voorgesteld en dat de gebruiker deze kan overschrijven.

## Aanleiding

Bij het aanmaken en afronden van een reis vraagt BootManager verschillende gegevens die het systeem geheel of gedeeltelijk al kent. De gebruiker zou deze waarden niet opnieuw handmatig hoeven overnemen.

Voorbeelden:

- de bootnaam is bekend vanuit de actieve boot;
- actuele motoruren- en logstandwaarden kunnen vanuit het bootprofiel bewust in
  een reis worden overgenomen;
- vertrek- en aankomsttijd zijn bekend wanneer de reis in BootManager wordt gestart en gestopt;
- totale verstreken reistijd kan uit vertrek- en aankomsttijd worden berekend;
- gevaren tijd kan worden afgeleid uit beschikbare meetdata en/of geregistreerde vaarstatus;
- begin- en eindlocatie kunnen waar mogelijk uit positiegegevens worden voorgesteld;
- logstand, gevaren afstand en andere samenvattingswaarden kunnen waar mogelijk
  uit bevestigde tellerstanden of meetdata worden afgeleid.

Het uitgangspunt is: **alles wat BootManager betrouwbaar kan weten of berekenen, wordt als voorstel ingevuld; de gebruiker houdt altijd de mogelijkheid om het te corrigeren of te overschrijven.**

---

## LOG-TRIP-AUTO-1: Reisgegevens automatisch voorinvullen

**Status:** Voorgesteld op 2026-06-06.

**User story:** Als gebruiker wil ik dat BootManager bij het starten, bewerken en afronden van een reis alle bekende of betrouwbaar afleidbare reisgegevens automatisch voorinvult, zodat ik zo weinig mogelijk dubbel hoef in te voeren en alleen uitzonderingen of correcties handmatig hoef vast te leggen.

### LOG-TRIP-AUTO-1A: Bootstanden expliciet overnemen en voortschrijven

**Status:** Geïmplementeerd en handmatig geaccepteerd op 2026-06-07.

**User story:** Als eigenaar wil ik actuele motoruren- en logstanden bij de
bootinstellingen beheren en deze bewust met een knop of icoon in een nieuwe reis
overnemen, zodat tellerstanden niet stilzwijgend worden ingevuld, geldige hogere
reisstanden automatisch worden voortgeschreven en ik na vervanging van
apparatuur expliciet een lagere beginstand kan instellen.

#### Scope

- Breid het singleton `VesselProfile` uit met twee optionele, niet-negatieve
  actuele tellerstanden:
  - actuele motorurenstand;
  - actuele logstand in zeemijlen.
- Toon beide waarden in de bestaande groep `Boot` op `/settings`.
- Een gebruiker mag daar een initiële waarde invoeren of een bestaande waarde
  expliciet verlagen/resetten, bijvoorbeeld na vervanging van apparatuur.
- Een nieuwe reis toont bij `Motoruren start` en `Logstand start` een compacte
  knop of icoon om de actuele waarde uit de bootinstellingen over te nemen.
- De overname gebeurt alleen na die expliciete gebruikersactie; openen van het
  reisformulier overschrijft geen handmatige invoer.
- Bootnaam en vertrekdatum/-tijd mogen wel als gewone bekende defaults worden
  voorgesteld.
- Bij opslaan of afronden van een reis worden geldige tellerstanden
  voortgeschreven naar het bootprofiel:
  - motoruren gebruikt de hoogste geldige waarde van `EngineHoursStart` en
    `EngineHoursEnd`;
  - logstand gebruikt de hoogste geldige waarde van `LogstandStart` en
    `LogstandEnd`;
  - `LoggedMiles` is geen gebruikersinvoer maar wordt berekend als
    `LogstandEnd - LogstandStart`;
  - een `null`, lege, negatieve of `0`-waarde verlaagt of wist een bestaande
    positieve profielwaarde niet;
  - een lagere reiswaarde overschrijft een hogere profielwaarde niet;
  - een hogere reiswaarde wordt de nieuwe actuele profielwaarde.
- De maximumregel wordt incrementeel toegepast op de actuele profielwaarde en
  de reis die wordt opgeslagen. Historische reizen worden niet telkens opnieuw
  gescand, zodat een expliciete reset in Instellingen niet door oude reizen
  wordt teruggedraaid.
- Na een reset gebruikt een nieuwe reis de resetwaarde als bron voor de
  expliciete overname. Een later opgeslagen hogere reiswaarde schrijft deze
  weer voort.
- Bij afronden wordt aankomsttijd voorgesteld en reisduur berekend. Reisduur
  blijft onderscheiden van vaartijd en motorlooptijd.
- Bevestigde waarden blijven zichtbaar na herladen en in de printweergave.

#### Buiten scope

- GPS-/havenherkenning en reverse geocoding.
- Automatische tankniveau-/brandstofvoorstellen uit `FluidLevelMeasurements`.
- Motorurensensoren, `YDVLW`-interpretatie en bronvoorkeuren.
- Betrouwbare vaartijd- of motorlooptijddetectie.
- Automatische reisnaam en bemanningsvoorstellen.
- Een volledige tellerhistorie of auditlog van resets.
- Multi-vessel ondersteuning; de huidige installatie blijft single-vessel.

#### Acceptatiecriteria

- `/settings` toont optionele actuele motoruren- en logstandwaarden onder
  `Boot`.
- Beide waarden accepteren `null` of een niet-negatief decimaal getal.
- Een gebruiker kan in Instellingen bewust een lagere waarde opslaan als reset.
- Een nieuwe reis kopieert geen tellerstand zonder expliciete klik.
- De overnameknoppen vullen alleen hun eigen startveld met de actuele
  bootinstelling en overschrijven een handmatige waarde alleen na die bewuste
  klik.
- Ontbrekende bootinstellingen schakelen de betreffende overnameactie uit of
  tonen duidelijk dat geen waarde beschikbaar is.
- Opslaan van een reis met hogere geldige tellerstanden verhoogt de actuele
  bootinstellingen.
- `0`, `null`, negatieve en lagere reiswaarden overschrijven geen hogere
  actuele bootinstelling.
- Een handmatige reset in Instellingen blijft behouden zolang geen later
  opgeslagen reis een hogere geldige waarde aanlevert.
- Oude historische reizen worden na een reset niet gebruikt om de oude hogere
  stand opnieuw terug te zetten.
- `LogstandEnd` is een invoerveld en mag niet lager zijn dan `LogstandStart`.
- `LoggedMiles` is alleen-lezen en wordt berekend als
  `LogstandEnd - LogstandStart`.
- Aankomsttijd en reisduur worden correct voorgesteld/berekend, ook over
  middernacht.
- Reisduur wordt niet als vaartijd of motorlooptijd gelabeld.
- Bestaande reizen, logboekregels en printweergave blijven functioneren.
- De databasewijziging heeft een EF Core-migratie.
- Build en relevante unit-/integratietests slagen.

#### Automatische testgevallen

- Bootprofiel zonder tellerstanden retourneert `null` voor beide waarden.
- Bootprofiel accepteert initiële positieve tellerstanden.
- Expliciete Settings-update mag een lagere resetwaarde opslaan.
- Overnameactie kopieert de profielwaarde naar het juiste reisstartveld.
- Zonder overnameactie blijft een handmatig of leeg reisstartveld ongewijzigd.
- `EngineHoursEnd` hoger dan profielwaarde verhoogt de profielwaarde.
- Alleen `EngineHoursStart` aanwezig en hoger dan profielwaarde verhoogt de
  profielwaarde.
- `0`, `null`, negatief of lager motorurengetal verlaagt de profielwaarde niet.
- Een hogere `LogstandEnd` verhoogt de actuele logstand.
- `LoggedMiles` wordt correct berekend uit begin- en eindstand.
- Ontbrekende of negatieve `LoggedMiles` veroorzaakt geen ongeldige
  voortschrijving.
- Na een Settings-reset blijft de lagere waarde staan totdat een nieuwe
  opgeslagen reis een hogere waarde bevat.
- Historische hogere reizen worden niet opnieuw als bron gelezen na reset.
- Reisduur werkt op dezelfde dag en over middernacht.

#### Handmatige acceptatietest

1. Vul in `/settings` een initiële motorurenstand en logstand in.
2. Maak een nieuwe reis en controleer dat de startvelden niet automatisch zijn
   overschreven.
3. Activeer per veld het overnameknopje of icoon en controleer de gekopieerde
   waarden.
4. Wijzig een overgenomen waarde handmatig en sla de reis op.
5. Vul eindwaarden in die hoger zijn en controleer na opslaan dat `/settings`
   de hoogste geldige standen toont.
6. Sla een reis op met lege, `0` of lagere waarden en controleer dat de
   bootinstellingen niet dalen.
7. Zet in `/settings` beide waarden bewust lager om nieuwe apparatuur te
   simuleren.
8. Open een nieuwe reis, neem de resetwaarden expliciet over en controleer dat
   oude reizen de oude hogere standen niet terugzetten.
9. Rond de reis af en controleer aankomsttijd, reisduur, herladen en print.

#### Legacy-impact

- `US1.2 Bootinformatie bewerken`: blijft `Partial`, maar krijgt extra dekking
  voor actuele bootgebonden tellerstanden.
- `US5.3 Motoruren en brandstof in header`: blijft `Partial`; motoruren worden
  structureel beheerd, brandstofintegratie blijft open.
- `US5.6 Logboekheader invullen`: krijgt extra dekking door expliciete
  tellerstand-overname en tijddefaults.
- `US5.11 Statistieken en samenvatting`: blijft `Partial`; logverschil en
  reisduur vormen een basis.

#### Implementatie en verificatie

- `VesselProfile` bewaart de actuele motoruren- en logstandwaarden.
- Settings ondersteunt initiële waarden, lagere resets en leegmaken.
- Nieuwe reizen gebruiken bootnaam en actuele lokale boordtijd als defaults.
- Motoruren- en logstandstart worden alleen via een expliciete knop
  overgenomen.
- Reisopslag schrijft alleen geldige hogere tellerstanden voort.
- `LogstandEnd` is invoer; `LoggedMiles` wordt berekend als eind minus start.
- Reisduur wordt berekend en getoond in logboek en print.
- EF Core-migraties:
  - `AddVesselProfileCurrentMeters`;
  - `AddLogbookTripLogstandEnd`.
- `dotnet build BootManager.sln` geslaagd met 0 errors.
- Gerichte VesselProfile- en LogbookTrip-tests geslaagd: 19/19.
- `git diff --check` schoon.
- Gebruiker heeft de Settings-, overname-, maximum-, reset-, logstand-,
  gelogde-mijlen-, reisduur- en printflow handmatig getest en geaccepteerd.

GPS-/havenherkenning, tankniveaus, sensorgebaseerde motoruren/logstand,
vaartijddetectie, automatische reisnamen en bemanningsvoorstellen blijven
vervolgslices van `LOG-TRIP-AUTO-1`.

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
  - in de eerste slice alleen na een expliciete gebruikersactie uit de actuele
    bootinstelling;
  - later eventueel uit een motorurensensor wanneer die beschikbaar en betrouwbaarder is;
  - gebruiker kan de voorgestelde stand corrigeren.
- **Logstand start**:
  - in de eerste slice alleen na een expliciete gebruikersactie uit de actuele
    bootinstelling;
  - later eventueel een beschikbare log-/distance-meetwaarde;
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

1. expliciet bevestigde handmatige waarde;
2. actuele tellerstand uit het bootprofiel, na bewuste overnameactie;
3. later eventueel een expliciete betrouwbare sensorwaarde;
4. berekende waarde uit meetdata;
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
- Motoruren start en logstand start worden alleen na een expliciete
  gebruikersactie gevuld vanuit de actuele standen in het bootprofiel.
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
- Nieuwe reis kopieert tellerstanden alleen na activering van het bijbehorende
  overnameknopje of icoon.
- Actuele bootinstellingen zijn de bron; historische reizen worden niet direct
  als bron voor het reisformulier gelezen.
- Ontbrekende profielwaarde resulteert in een leeg startveld.
- Reisduur wordt correct berekend over dezelfde dag en over middernacht.
- Ongeldige aankomst vóór vertrek wordt afgekeurd of duidelijk gevalideerd.
- Handmatig gewijzigde waarde blijft behouden wanneer voorstellen opnieuw worden geladen.
- Verschilberekening voor motoruren en logstand werkt alleen met geldige begin- en eindwaarden.
- Ontbrekende metingen geven een leeg voorstel en geen exception.
- UTC-opslag en lokale boordtijdweergave blijven correct.

### Handmatige acceptatietest

1. Stel actuele motoruren- en logstandwaarden in bij `Instellingen > Boot`.
2. Maak een nieuwe reis aan en controleer boot, datum en vertrektijd.
3. Controleer dat tellerstanden pas na activering van het bijbehorende knopje
   of icoon worden overgenomen.
4. Wijzig een overgenomen waarde handmatig en controleer dat deze behouden
   blijft.
5. Sla hogere eindstanden op en controleer dat het bootprofiel wordt verhoogd.
6. Controleer dat lege, `0`, negatieve en lagere reiswaarden het bootprofiel
   niet verlagen.
7. Voer in Settings een lagere reset uit en controleer dat oude reizen deze
   waarde niet herstellen.
8. Start en stop de reis.
9. Controleer dat aankomsttijd en reisduur zijn ingevuld.
10. Controleer dat vaartijd niet ten onrechte gelijk wordt gesteld aan
    reisduur.

### Legacy-impact

- Verfijnt `Story 1 - Reis aanmaken en beheren` uit de actieve epic Digitaal Logboek.
- Ondersteunt het doel dat automatische bootdata wordt gecombineerd met handmatige aanvulling.
- Verbetert de reis-header en reis-samenvatting zonder de bestaande logboekregelworkflow te vervangen.
