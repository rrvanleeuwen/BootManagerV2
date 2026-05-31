# Pi Database Analyse 2026-05-31

Status: uitgevoerd op feature branch `codex/pi-database-analysis`.

Bronbestand:

- Lokale analyse van Pi database-export `bootmanager-db-20260531.tar.gz`.
- Uitgepakte SQLite database: `.analysis/pi-db-20260531/bootmanager.db`.
- Analyse is read-only uitgevoerd. De database-export en `.analysis/` zijn lokale onderzoeksdata en worden niet gecommit.

## Samenvatting

De database bevat 444.454 ruwe `NetworkMessages` tussen `2026-05-29 12:18:21 UTC` en `2026-05-31 10:05:48 UTC`.

Belangrijkste conclusies:

- Bestaande interpreters vullen positie, beweging, heading, wind, snelheid door water en watertemperatuur.
- `DepthMeasurements` en `BatteryMeasurements` zijn leeg in deze export.
- Tankniveau is zeer waarschijnlijk aanwezig via `PCDIN` en `MXPGN` regels met PGN `01F211`, oftewel NMEA 2000 PGN `127505` Fluid Level.
- `YDVLW` bevat logstand/afstand door water en is een sterke kandidaat voor logboekvelden.
- `YDXDR` bevat yaw, pitch en roll; bruikbaar voor latere bewegings-/diagnostiek, maar minder belangrijk voor het logboek.
- `YDVTG`, `YDHDG`, `YDMWD`, `YDMDA`, `YDVWR` en `YDVWT` zijn raw-only alternatieven of aanvullingen op al bestaande metingen.
- De huidige `Source`-waarde is niet geschikt als stabiele fysieke apparaatidentiteit: dezelfde berichttypen komen allemaal langs dezelfde set Docker/UDP endpoints zoals `172.18.0.1:53784`, `172.18.0.1:39457`, enzovoort.

## Ruwe berichten

| MessageId | Aantal | Opmerking |
| --- | ---: | --- |
| `AIVDM` | 96.352 | AIS target berichten, raw-only |
| `YDMWV` | 39.303 | Wind, wordt al geinterpreteerd |
| `YDGSV` | 26.291 | GNSS satellietinformatie, raw-only |
| `AIVDO` | 24.873 | AIS eigen schip, raw-only |
| `YDHDG` | 19.794 | Magnetic heading, raw-only |
| `YDVTG` | 19.605 | COG/SOG, raw-only alternatief voor RMC |
| `PCDIN` | 12.578 | NMEA 2000 gatewaydata, bevat Fluid Level |
| `MXPGN` | 12.578 | NMEA 2000 gatewaydata, dubbele/parallelle Fluid Level data |
| `YDHDM` | 10.792 | Magnetic heading, wordt geinterpreteerd |
| `YDROT` | 10.792 | Rate of turn, raw-only |
| `YDVWR` | 10.791 | Apparent wind relative, raw-only |
| `YDXDR` | 10.791 | Transducer values: yaw, pitch, roll |
| `YDVLW` | 10.782 | Distance travelled/logstand, raw-only |
| `YDVHW` | 10.780 | Speed through water, wordt geinterpreteerd |
| `YDMDA` | 10.779 | Meteorologische composite; bevat wind |
| `YDMTW` | 10.779 | Water temperature, wordt geinterpreteerd |
| `YDMWD` | 10.779 | Wind direction/speed, raw-only |
| `YDVWT` | 10.779 | True wind relative, raw-only |
| `YDHDT` | 10.733 | True heading, wordt geinterpreteerd |
| `YDDTM` | 10.729 | Datum reference, raw-only |
| `YDGLL` | 10.721 | Positie, raw-only |
| `YDRMC` | 10.714 | Positie en motion, wordt geinterpreteerd |
| `YDGGA` | 10.712 | Positie, wordt geinterpreteerd |
| `YDVDR` | 10.691 | Set/drift, raw-only |
| `YDZDA` | 10.688 | Datum/tijd, raw-only |
| `YDGSA` | 8.868 | GNSS DOP/active satellites, raw-only |
| `YDRSA` | 546 | Rudder sensor angle, raw-only |
| `YDGST` | 430 | GNSS pseudorange error statistics, raw-only |
| `YDHTD` | 404 | Heading/track control data, raw-only |

## Measurement-tabellen

| Tabel | Aantal | Eerste UTC | Laatste UTC |
| --- | ---: | --- | --- |
| `PositionMeasurements` | 21.397 | 2026-05-29 12:19:02 | 2026-05-31 10:05:46 |
| `MotionMeasurements` | 10.686 | 2026-05-29 12:19:32 | 2026-05-31 10:05:46 |
| `HeadingMeasurements` | 21.525 | 2026-05-29 12:18:23 | 2026-05-31 10:05:47 |
| `WindMeasurements` | 39.303 | 2026-05-29 12:18:23 | 2026-05-31 10:05:44 |
| `DepthMeasurements` | 0 | - | - |
| `WaterTemperatureMeasurements` | 10.779 | 2026-05-29 12:18:27 | 2026-05-31 10:05:47 |
| `SpeedThroughWaterMeasurements` | 10.780 | 2026-05-29 12:18:27 | 2026-05-31 10:05:47 |
| `BatteryMeasurements` | 0 | - | - |

## Tankniveau / Fluid Level

`PCDIN` en `MXPGN` bevatten regels met `01F211`. Hex `01F211` komt overeen met PGN `127505`, Fluid Level. De payloads zijn in beide message-ids grotendeels dubbel aanwezig.

Voorbeelden:

```text
$PCDIN,01F211,000024F3,43,00A861DC050000FF*2D
$MXPGN,01F211,6843,00A861DC050000FF*60
$PCDIN,01F211,000024F3,43,104A11D0070000FF*56
$MXPGN,01F211,6843,104A11D0070000FF*1B
```

Decode-inferentie:

| Instance | Type | Capaciteit | Niveau in data | Opmerking |
| ---: | --- | ---: | --- | --- |
| 0 | Fuel | ca. 150 liter | meestal 100%, later payload `00FF7F...` | `0x7FFF` moet waarschijnlijk als onbekend/invalid worden behandeld, niet als 131,07%. |
| 0 | Water | ca. 200 liter | ca. 0-17,81% | Zichtbaar als `10....D0070000FF` payloads. |
| 1 | Water | ca. 200 liter | ca. 0,97-20,98% | Zichtbaar als `11....D0070000FF` payloads. |

Interpretatie:

- Er lijken minimaal drie tank-/fluid-kanalen zichtbaar: brandstof instance 0, water instance 0 en water instance 1.
- `PCDIN` en `MXPGN` lijken dezelfde PGN-data parallel te leveren. Een interpreter moet dubbele opslag voorkomen of een duidelijke prioriteit kiezen.
- Voor brandstof komt later `0x7FFF` voor als levelwaarde. Dat moet in de parser als onbekend/ongeldig behandeld worden totdat dit met de standaard of praktijkdata bevestigd is.

## Logstand / afstand

`YDVLW` is aanwezig met 10.782 regels en is raw-only.

Voorbeeld:

```text
$YDVLW,790.999,N,791.000,N*58
```

Dit is een sterke kandidaat voor logboekvelden rond logstand en afstand door water. De velden lijken totaal/cumulatief gelogde afstand in nautical miles te bevatten.

## Andere raw-only kandidaten

| Bericht | Voorbeeld | Kandidatuur |
| --- | --- | --- |
| `YDHDG` | `$YDHDG,33.6,,,,*62` | Magnetic heading; kan heading uitbreiden of als alternatief dienen voor HDM/HDT. |
| `YDVTG` | `$YDVTG,116.7,T,113.8,M,0.0,N,0.1,K,A*22` | COG/SOG alternatief naast RMC. |
| `YDMWD` | `$YDMWD,245.3,T,245.3,M,7.1,N,3.7,M*5B` | Ware wind richting/snelheid. |
| `YDMDA` | `$YDMDA,,I,,B,,C,20.9,C,,,,C,245.3,T,245.3,M,7.1,N,3.7,M*10` | Composite met o.a. wind; mogelijk overlap met MWD/MTW. |
| `YDVWR` | `$YDVWR,173.1,L,9.5,N,4.9,M,17.6,K*7F` | Apparent wind relative; mogelijk alternatief/uitbreiding op MWV. |
| `YDVWT` | `$YDVWT,150.1,L,7.2,N,3.7,M,13.3,K*79` | True wind relative. |
| `YDXDR` | `$YDXDR,A,33.75,D,Yaw,A,-1.75,D,Pitch,A,1.25,D,Roll*66` | Yaw/pitch/roll, nuttig voor diagnostiek of later motion-model. |
| `YDRSA` | `$YDRSA,-1.2,A,,V*4A` | Roerstand; interessant maar niet direct logboek-prioriteit. |
| `YDGST` | `$YDGST,055220.20,4.76,0.63,0.55,84.3,0.55,0.63,2.55*67` | GNSS kwaliteitsinformatie. |
| `AIVDM`/`AIVDO` | AIS `!` sentences | AIS-semantiek is aanwezig maar buiten deze analyse-slice. |

## Bronidentiteit

De huidige `Source` in raw en measurement-tabellen is in deze export geen betrouwbare fysieke apparaatidentiteit.

Voor vrijwel alle belangrijke berichttypes komen dezelfde zes sources voor:

```text
172.18.0.1:53784
172.18.0.1:39457
172.18.0.1:50523
172.18.0.1:58601
172.18.0.1:36683
172.18.0.1:52789
```

Dit patroon past bij Docker bridge plus wisselende UDP source ports. Daardoor kan BootManager bronvoorkeuren later niet alleen op `Source` baseren.

Aanbevolen bronidentiteit voor Story 8:

- Gebruik `Protocol`, talker-prefix, sentence/message-id en eventueel PGN als technische bronkenmerken.
- Bewaar bij NMEA 2000 gatewayberichten ook PGN, instance, fluid type en waar mogelijk gateway-/source-address informatie uit de sentence.
- Beschouw remote endpoint alleen als diagnostische metadata, niet als stabiele gebruikersbron.
- Laat gebruikers later een herkenbare naam koppelen aan een technische bron, bijvoorbeeld "GPS plotter", "AIS", "YDEN gateway", "Brandstoftank".

## Aanbevolen volgorde

1. Story 8: eerst bronidentiteit/bronvoorkeuren ontwerpen, omdat de huidige `Source`-kolom niet stabiel genoeg is.
2. Story 9 kandidaat 1: interpreter voor PGN 127505 Fluid Level via `PCDIN`/`MXPGN`, inclusief duplicate handling en invalid/unknown levelwaarden.
3. Daarna: interpreter voor `YDVLW` voor logstand/afstand.
4. Daarna pas: uitbreidingen voor `YDVTG`, `YDHDG`, windvarianten, roerstand of AIS.

## Open punten

- Motoruren zijn in deze export niet duidelijk zichtbaar als eenvoudig herkenbaar bericht.
- Diepte is in deze export niet aanwezig in interpreted measurements en er zijn geen `DBT`/`DPT` berichten aangetroffen in de onderzochte set.
- Batterij/spanning is niet aanwezig in `BatteryMeasurements`; de analyse heeft geen duidelijke battery-kandidaat uit de topberichten gevonden.
- De exacte PGN 127505 decode moet bij implementatie worden vastgelegd in unit tests met de echte payloadvoorbeelden hierboven.
