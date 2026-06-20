# PILOT-INV-05

Bron: `.docs/releases/holiday-pilot-2026.md`

### PILOT-INV-05 — Voorraad muteren en eenvoudige historie

**Storyzin**  
Als Owner of Crew wil ik voorraadverbruik, tellingen en correcties kunnen verwerken en
later in een eenvoudig logboek kunnen terugzien, zodat de werkelijke voorraad actueel
blijft zonder de context van product en locatie te verliezen.

**Waarom deze slice nu**  
Na catalogus, voorraadbasis, inruimen en terugvinden ontbreekt nog het dagelijks
bijhouden van voorraad wanneer producten gebruikt worden of aantallen niet meer kloppen.
Deze story voegt daarom zowel een fysieke verbruikflow op locatie als een
administratieve fallback toe, plus een eenvoudige historie voor controle achteraf.

**Scope**

- Deze story ondersteunt drie expliciete mutatietypes:
  - `Verbruik`
  - `Correctie`
  - `Telling`
- `Verbruik` verlaagt voorraad altijd op een expliciete product-locatieregel.
- De fysieke hoofdflow is:
  - product terugvinden;
  - naar de locatie gaan;
  - locatie scannen;
  - product scannen;
  - verbruikte hoeveelheid invoeren;
  - opslaan;
  - terugkeren naar het begin van de terugvind/verbruikflow.
- De administratieve fallback werkt zonder scannen.
- In die fallback kiest de gebruiker eerst een product en daarna een locatie.
- Als dat product maar op een actieve locatie ligt, kiest BootManager die locatie
  automatisch.
- Bij `Verbruik` voert de gebruiker de afname in.
- Bij `Telling` voert de gebruiker de feitelijk aanwezige nieuwe hoeveelheid in.
- Bij `Correctie` voert de gebruiker ook de feitelijk nieuwe hoeveelheid in.
- De gebruiker kan bij iedere mutatie een hele vrije optionele notitie toevoegen.
- Verbruik dat meer afneemt dan de actuele voorraad op die locatie wordt geblokkeerd.
- Als een mutatie de actieve voorraad van een product op een locatie op `0` brengt,
  verdwijnt de actieve voorraadregel van die locatie.
- De laatst gebruikte locatie van het product blijft daarbij als echte
  locatieverwijzing bewaard als verwachte locatie, zodat BootManager later nog kan tonen
  waar het product normaal hoort te liggen.
- Een aparte historiepagina toont alle voorraadmutaties.
- Die historiepagina toont standaard alle mutaties, nieuwste eerst.
- Een historieregel toont minimaal:
  - datum/tijd;
  - mutatietype;
  - productnaam;
  - gebied en locatienaam;
  - oude hoeveelheid;
  - nieuwe hoeveelheid;
  - gebruiker;
  - optionele notitie.

**Buiten scope**

- Negatieve voorraad.
- Mutaties zonder expliciete locatie.
- Inline historie op product- of locatiepagina's.
- Geavanceerde filters, export of rapportage op de historiepagina.
- Dashboardintegratie voor voorraadmutaties.
- Automatische verbruiksafleiding zonder expliciete gebruikersactie.

**Acceptatiecriteria**

1. Owner en Crew kunnen voorraadmutaties uitvoeren als `Verbruik`, `Correctie` of
   `Telling`.
2. De fysieke verbruikflow ondersteunt: product terugvinden, naar de locatie gaan,
   locatie scannen, product scannen, afname invoeren en opslaan.
3. Na afronding van die fysieke verbruikflow keert de gebruiker terug naar het begin van
   die terugvind/verbruikroute.
4. De administratieve fallback ondersteunt muteren zonder scannen door eerst een product
   en daarna een locatie te kiezen.
5. Als een product in die fallback maar op een actieve locatie ligt, kiest BootManager
   die locatie automatisch.
6. `Verbruik` vraagt om een afnamehoeveelheid; `Telling` en `Correctie` vragen om de
   nieuwe feitelijke hoeveelheid.
7. Een optionele vrije notitie kan bij iedere mutatie worden opgeslagen.
8. Verbruik boven de actuele voorraad op die locatie wordt duidelijk geblokkeerd.
9. Als een mutatie de actieve voorraad op `0` brengt, verdwijnt de actieve voorraadregel
   maar blijft de laatst gebruikte locatie van het product als verwachte locatie
   bewaard.
10. De aparte historiepagina toont alle mutaties standaard nieuwste eerst met minimaal
    datum/tijd, type, product, gebied + locatie, oude hoeveelheid, nieuwe hoeveelheid,
    gebruiker en optionele notitie.

**Legacy-impact**

- Dekt primair `US2.10` voorraad aanpassen en `US2.13` voorraadlogboek.
- Dekt ook de verbruikskant van `US2.20` voorraad verminderen via barcode, maar binnen
  de pilot nog in combinatie met expliciete locatiecontext en zonder bredere
  automatisering.
- Bouwt voort op `PILOT-INV-02` voor product-locatieregels en op `PILOT-INV-04` voor
  het terugvinden van producten voordat verbruik wordt geboekt.
- Laat geavanceerde filters, analyses en dashboardsignalering bewust open voor latere
  inventory-slices.

**Handmatige acceptatietest**

1. Zoek een product via de terugvindflow, ga naar de juiste locatie, scan daar de
   locatiecode en daarna de productcode.
2. Kies `Verbruik`, voer een afname in en sla op.
3. Controleer dat de voorraad op die locatie is verlaagd en dat de flow terugkeert naar
   het begin van de terugvind/verbruikroute.
4. Herhaal via de administratieve fallback zonder scannen: kies eerst een product en
   daarna een locatie, of laat de locatie automatisch kiezen als er maar een actief is.
5. Voer een `Telling` uit en controleer dat de nieuwe feitelijke hoeveelheid direct wordt
   opgeslagen.
6. Voer een `Correctie` uit met een andere nieuwe hoeveelheid en controleer dat ook deze
   wordt opgeslagen.
7. Voeg bij minstens een mutatie een vrije notitie toe en controleer dat die later in de
   historie zichtbaar is.
8. Probeer meer te verbruiken dan op de gekozen locatie aanwezig is; verwacht een
   duidelijke blokkade.
9. Verbruik een voorraadregel precies naar `0` en controleer dat de actieve regel
   verdwijnt, maar dat het product later nog zijn laatst gebruikte locatie als
   verwachte plek behoudt voor terugvinden of opnieuw inruimen.
10. Open de aparte historiepagina en controleer dat alle mutaties nieuwste eerst worden
    getoond met datum/tijd, type, product, gebied + locatie, oude hoeveelheid, nieuwe
    hoeveelheid, gebruiker en eventuele notitie.
