# Legacy BootManager Scope Analysis

Status: eerste inventarisatie gestart (2026-05-25).

## Bronnen

- Legacy PDF: `.docs/legacy-input/BootmanagerEPICS.pdf` (lokale bron, niet ingecheckt)
- OCR-output: `.docs/legacy-input/BootmanagerEPICS.ocr.txt`
- Word-export per epic: `.docs/legacy-input/BootManager_*.docx`
- Oude GitHub-repo: `https://github.com/rrvanleeuwen/BootNETManager`
- Lokale read-only clone tijdens analyse: `C:\tmp\BootNETManager-legacy`

## Belangrijke Afspraak

De legacy PDF bevat ook technische informatie over architectuur, stackkeuzes en oude werkafspraken.
Die informatie is voor BootManagerV2 **niet leidend**.

Voor deze analyse telt alleen:

- functionele applicatiescope;
- epics;
- user stories;
- functionele acceptatiecriteria;
- vergelijking met huidige BootManagerV2-functionaliteit.

## OCR-kwaliteit

De PDF is grotendeels image-based. Gewone tekstextractie leverde alleen paginatitels op.
Daarom is OCR uitgevoerd op geëxporteerde pagina-afbeeldingen.
De gedeelde bron in git is daarom de OCR-output, niet de originele binary PDF.

Beperking:

- 56 van 68 pagina's konden direct als PNG worden geëxporteerd en ge-OCR'd.
- Enkele pagina's ontbreken in OCR, waarschijnlijk door PDF-image encoding.
- Ontbrekende of gedeeltelijke delen zijn in de analyse gemarkeerd als OCR-gap wanneer relevant.

Bekende OCR-gaps:

Geen bekende OCR-gaps meer voor de beschikbare Word-exportbestanden. Alle bekende gaten uit de OCR-analyse zijn met de Word-bronnen gecontroleerd of opgelost.

Opgelost via Word-bron:

- Epic 2 US2.1 en US2.2 zijn verwerkt uit `BootManager_Epic2_Inventarisbeheer.docx`.
- Epic 4 US4.1 is verwerkt uit `BootManager_Epic4_Documentbeheer.docx`; dezelfde Word-bron bevat aanvullend US4.13.
- Epic 5 US5.1 is verwerkt uit `BootManager_Epic5_Logboek.docx`.
- Epic 7 US7.1 t/m US7.8 zijn verwerkt uit `BootManager_Epic7_Dashboard.docx`.
- Epic 9 US9.1 t/m US9.5 zijn verwerkt uit `BootManager_Epic9_Integratie.docx`.

## Outputbestanden

- `scope-inventory.md` - ruwe legacy scope per epic.
- `mapped-epics.md` - mapping legacy scope naar huidige BootManagerV2-status.
- `legacy-coverage-register.md` - story-level afvinkregister voor legacy US-dekking.
- `implemented-or-obsolete.md` - legacy stories die al klaar, deels klaar of bewust niet meer relevant zijn.
- `proposed-backlog.md` - voorgestelde BootManagerV2 epics/user stories in huidige stijl.
- `word-source-progress.md` - voortgang per Word-exportbestand.

## Coverage Bijhouden

`legacy-coverage-register.md` is het primaire afvinkregister. Bij iedere afgeronde BootManagerV2-functionaliteit moet worden gecontroleerd welke legacy US-nummers geraakt worden. Werk daarna de status bij naar `Done`, `Partial`, `Open`, `Parked`, `Replaced` of `Obsolete`.

Dit register is bedoeld om dubbele analyse en dubbele user stories te voorkomen.

## Word-Verwerking

De PDF/OCR-analyse wordt vanaf 2026-05-25 gecontroleerd tegen Word-exportbestanden uit de originele OneNote-sectie.
Deze verwerking gebeurt bewust bestand voor bestand. Na ieder verwerkt bestand stopt Codex en vraagt expliciet akkoord voordat het volgende bestand wordt verwerkt.

## Oude Repo Observatie

De oude repo bevat vooral een vroege technische start:

- Boot entity en boot CRUD API.
- Opberglocatie entity.
- DTO's voor boot, opberglocatie en inventaris-vormen.
- MAUI/Blazor skeleton.

Voor BootManagerV2 is dit vooral functionele context. De oude architectuur of code-indeling wordt niet overgenomen.
