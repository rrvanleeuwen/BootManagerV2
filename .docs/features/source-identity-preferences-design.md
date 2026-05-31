# Source Identity And Source Preferences Design

Status: ontwerp voor NMEA Story 8, opgesteld op 2026-05-31.

## Doel

BootManager moet later per meetsoort een voorkeursbron kunnen toepassen, bijvoorbeeld voor positie, snelheid, heading, wind, tankniveau of logstand. Die bron mag niet worden afgeleid uit het UDP/YDEN transportendpoint. De YDEN gateway levert uiteindelijk alle data via UDP aan BootManager; dat endpoint zegt dus niets betrouwbaars over het fysieke apparaat dat de waarde heeft gemaakt.

De bronidentiteit moet worden afgeleid uit de inhoud en metadata van het NMEA-bericht zelf.

## Randvoorwaarde

Transportbron en databron zijn verschillende concepten:

- Transportbron: hoe het bericht BootManager bereikt, bijvoorbeeld UDP endpoint `172.18.0.1:53784`.
- Databron: welke technische NMEA-bron of sensor de waarde inhoudelijk vertegenwoordigt.

`NetworkMessage.Source` mag voorlopig nuttig blijven voor diagnose, maar mag niet als gebruikersbron of voorkeursbron worden gebruikt.

## Analysebasis

De Pi-databaseanalyse van 2026-05-31 laat zien dat vrijwel alle belangrijke berichttypes dezelfde set Docker/UDP endpoints gebruiken:

```text
172.18.0.1:53784
172.18.0.1:39457
172.18.0.1:50523
172.18.0.1:58601
172.18.0.1:36683
172.18.0.1:52789
```

Dat patroon past bij transportgedrag en niet bij fysieke bronidentiteit. Daarom moet Story 8 bronidentiteit op NMEA-inhoud baseren.

## Bronidentiteit Voor NMEA 0183

Een NMEA 0183 sentence heeft in de praktijk minimaal:

- talker-id, bijvoorbeeld `YD`, `AI`, `GP`;
- sentence type, bijvoorbeeld `RMC`, `GGA`, `MWV`, `HDM`, `HDT`;
- velden met meetwaarde, status en soms mode/quality.

Voor BootManager is de minimale technische bronkey:

```text
Protocol=NMEA0183
TalkerId=<talker>
SentenceType=<type>
```

Voorbeelden:

| Bericht | Technische bronkey | Meetsoort |
| --- | --- | --- |
| `$YDRMC` | `NMEA0183/YD/RMC` | positie, COG/SOG |
| `$YDGGA` | `NMEA0183/YD/GGA` | positie |
| `$YDMWV` | `NMEA0183/YD/MWV` | wind |
| `$YDHDM` | `NMEA0183/YD/HDM` | heading magnetic |
| `$YDHDT` | `NMEA0183/YD/HDT` | heading true |
| `!AIVDM` | `NMEA0183/AI/VDM` | AIS target data |
| `!AIVDO` | `NMEA0183/AI/VDO` | AIS own-vessel data |

Deze key is nog geen perfecte fysieke apparaatidentiteit. Een gateway kan meerdere onderliggende apparaten met dezelfde talker-prefix publiceren. Toch is dit betrouwbaarder dan UDP endpoint en voldoende als eerste technische bronidentiteit voor NMEA 0183.

### Aanvullende NMEA 0183 metadata

Waar beschikbaar kan BootManager later extra metadata vastleggen:

- fix quality of mode voor GPS-achtige sentences;
- statusvelden zoals valid/invalid;
- heading type: true/magnetic;
- wind type: apparent/true, relative/absolute;
- AIS MMSI of own-vessel indicator bij AIS verwerking.

Die extra metadata hoort niet altijd in de bronkey zelf. Vaak is het beter als bronkenmerk of kwaliteitskenmerk naast de bronkey.

## Bronidentiteit Voor NMEA 2000 Via Gateway Sentences

De Pi-analyse toont `PCDIN` en `MXPGN` berichten met PGN `01F211`, waarschijnlijk NMEA 2000 PGN `127505` Fluid Level.

Voor gateway-berichten is de minimale technische bronkey:

```text
Protocol=NMEA2000-Gateway
GatewaySentence=<PCDIN|MXPGN>
PGN=<pgn>
SourceAddress=<n2k source address when available>
Instance=<data instance when available>
SubType=<domain-specific subtype when available>
```

Voor PGN 127505 Fluid Level:

```text
Protocol=NMEA2000-Gateway
PGN=127505
SourceAddress=<source address from gateway sentence if available>
Instance=<fluid instance>
FluidType=<fuel|water|...>
```

Voorbeelden uit de Pi-analyse:

```text
$PCDIN,01F211,000024F3,43,00A861DC050000FF*2D
$MXPGN,01F211,6843,00A861DC050000FF*60
$PCDIN,01F211,000024F3,43,104A11D0070000FF*56
$MXPGN,01F211,6843,104A11D0070000FF*1B
```

Voor deze payloads is de belangrijke bronidentiteit niet `PCDIN` versus `MXPGN` op zichzelf, maar PGN plus inhoudelijke source/instance-velden. `PCDIN` en `MXPGN` lijken dezelfde onderliggende data parallel te publiceren. Een latere interpreter moet daarom duplicate handling of prioriteit toepassen.

## Conceptueel Model

Een toekomstig bronmodel kan uit drie lagen bestaan.

### 1. Transport Metadata

Alleen voor diagnose:

- remote endpoint;
- ingest listener;
- received timestamp;
- raw line.

Niet gebruiken voor gebruikersvoorkeuren.

### 2. Technical Data Source

Stabiele technische bron op basis van berichtinhoud:

- protocol;
- talker-id/sentence type of PGN;
- NMEA 2000 source address waar beschikbaar;
- instance/subtype waar beschikbaar;
- measurement kind.

Voorbeeldvelden:

```text
SourceKey
Protocol
TalkerId
SentenceType
Pgn
N2kSourceAddress
Instance
SubType
MeasurementKind
DisplayHint
LastSeenUtc
```

### 3. User Source Label And Preference

Gebruikerslaag:

- gebruikerslabel, bijvoorbeeld `GPS plotter`, `AIS`, `Windmeter`, `Brandstoftank`, `Watertank bakboord`;
- voorkeursbron per meetsoort;
- fallback-instellingen.

Voorbeeldvelden:

```text
MeasurementKind
PreferredSourceKey
FallbackPolicy
StaleAfterSeconds
UserLabel
```

## Bronvoorkeur Per Meetsoort

| Meetsoort | Mogelijke bronkeys | Opmerking |
| --- | --- | --- |
| Positie | `RMC`, `GGA`, later AIS own-vessel | Bronvoorkeur moet rekening houden met fix/status/recency. |
| COG/SOG | `RMC`, `VTG` | `VTG` is raw-only kandidaat; RMC bestaat al. |
| Heading | `HDT`, `HDM`, `HDG` | True heading kan functioneel voorkeur hebben boven magnetic, afhankelijk van UI/context. |
| Wind | `MWV`, `VWR`, `VWT`, `MWD`, `MDA` | Eerst bepalen of waarde apparent/true/relative/absolute is. |
| Snelheid door water | `VHW` | Bestaat al als interpreter. |
| Watertemperatuur | `MTW`, eventueel `MDA` | `MTW` bestaat al. |
| Diepte | `DBT`, `DPT` | Niet zichtbaar in de 2026-05-31 export. |
| Tankniveau | PGN `127505` Fluid Level | Sterke kandidaat voor Story 9. Bronkey moet instance/fluid type bevatten. |
| Logstand | `VLW` | Sterke kandidaat na Fluid Level. |
| AIS | `AIVDM`, `AIVDO` | AIS-semantiek later apart ontwerpen. |

## Fallbackgedrag

Een voorkeursbron kan tijdelijk geen actuele data leveren. BootManager moet dan voorspelbaar handelen:

1. Gebruik de voorkeursbron als de laatste waarde recent en geldig is.
2. Als de voorkeursbron stale of invalid is, gebruik een fallbackbron voor dezelfde meetsoort als die recent en geldig is.
3. Markeer de getoonde of voorgestelde waarde als fallback, zodat diagnose later mogelijk blijft.
4. Als geen bron recent en geldig is, toon geen actuele waarde en bewaar geen automatische suggestie alsof die live is.

Standaard kan `StaleAfterSeconds` per meetsoort verschillen:

| Meetsoort | Indicatieve stale grens |
| --- | ---: |
| Positie, COG/SOG, wind, heading | 10-30 seconden |
| Watertemperatuur, tankniveau, logstand | 60-300 seconden |
| Logboek-start/eindvoorstellen | Mag ouder zijn, maar timestamp moet zichtbaar blijven |

Exacte waarden moeten later per implementatiestory worden gekozen.

## UI-Richting

Settings moet later niet technische raw keys centraal zetten, maar herkenbare bronnen tonen:

- Meetsoort: `Positie`, `Heading`, `Wind`, `Tankniveau`, enzovoort.
- Huidige bronnen: gebruikerslabel plus technische hint, bijvoorbeeld `GPS plotter (YD/RMC)`.
- Voorkeur: primaire bron kiezen.
- Fallback: automatisch toestaan of blokkeren.
- Laatst gezien: timestamp en status.

Voor gevorderde diagnose mag de technische key zichtbaar zijn, maar niet als primaire UX-tekst.

## Gevolgen Voor Implementatie

Voor toekomstige codewijzigingen zijn waarschijnlijk nodig:

1. Een `SourceIdentity` of vergelijkbaar value object/service die uit parsed raw messages een technische bronkey maakt.
2. Opslag van bronmetadata bij nieuwe measurements of via een aparte source registry.
3. Interpreters moeten naast meetwaarde ook technische bronmetadata teruggeven.
4. Queryservices voor dashboard/logboek moeten bronvoorkeuren kunnen toepassen.
5. Settings-UI moet source labels en voorkeuren beheren.

## Vervolgstories

### Story 8A - SourceIdentity Value Object En Parsercontract

Ontwerp/implementeer een gedeelde bronkey voor NMEA 0183 en gateway/NMEA 2000 berichten, zonder UI.

### Story 8B - Source Registry En Last-Seen Overzicht

Sla ontdekte technische bronnen op met `LastSeenUtc`, measurement kind en display hint.

### Story 8C - Source Preferences Service

Implementeer voorkeursbronselectie en fallbackbeleid per meetsoort.

### Story 8D - Settings UI Voor Bronvoorkeuren

Maak een beheerbare UI waarin gebruikers bronlabels en voorkeuren kunnen instellen.

### Story 9 - Fluid Level Interpreter

Gebruik dit bronmodel bij de eerste nieuwe interpreter voor PGN `127505` Fluid Level via `PCDIN`/`MXPGN`.

## Open Ontwerpvragen

- Is de source address in `PCDIN` en `MXPGN` altijd betrouwbaar beschikbaar en consistent genoeg om in de bronkey op te nemen?
- Willen we `PCDIN` en `MXPGN` als twee gateway-sentencevormen behandelen of direct normaliseren naar dezelfde PGN-bron?
- Moet een bronkey per measurement kind verschillend zijn, of kan dezelfde technische bron meerdere measurement kinds leveren?
- Hoeveel automatische labels kunnen we zinvol genereren zonder de gebruiker verkeerde zekerheid te geven?
