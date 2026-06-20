# Current Codex Handoff

Updated: 2026-06-20.

## Rollen

- Claude Code programmeert uitsluitend vanuit een goedgekeurde story en implementation packet.
- Codex begeleidt proces, architectuur, story-scope, review, testen, documentatie en git/PR-flow.

## Repositorystatus

- Basisbranch: `master`.
- Actieve branch: `feature/pilot-inv-01-product-catalog`.
- `master` en `origin/master` staan op mergecommit `4a610da` voor `PILOT-LOC-04`.
- `PILOT-INV-01` is lokaal geïmplementeerd, getest en handmatig geaccepteerd; documentatie-, commit-, push- en PR-afronding lopen op de featurebranch.

## Actieve release

De leidende release is de **BootManager Holiday Pilot 2026**.

- bron: `.docs/releases/holiday-pilot-2026.md`;
- status: actief en leidend voor de eerstvolgende ontwikkelperiode;
- afgerond: `PILOT-SCAN-01`, `PILOT-AUTH-01`, `PILOT-LOC-01`, `PILOT-LOC-02`,
  `PILOT-LOC-03`, `PILOT-LOC-04`, `PILOT-INV-01`;
- actuele focus: administratieve afronding van `PILOT-INV-01`, daarna inventory-vervolg vanaf `PILOT-INV-02`.

Kies geen story buiten deze release, behalve bij een blocker, ontbrekende afhankelijkheid
of expliciete andere prioriteit van de gebruiker.

## Eerstvolgende actie

`PILOT-INV-01` is technisch gerealiseerd en door de gebruiker handmatig geaccepteerd.
De documentatie wordt nu bijgewerkt, waarna commit, push en een draft PR volgen.

Eerstvolgende inhoudelijke story na merge is `PILOT-INV-02` vanaf een schone actuele
`master`.

Let op bij vervolg op productcodes:

- handmatige gekoppelde code-invoer in `Voorraadbeheer > Producten` is aanwezig;
- direct barcode scannen binnen dat handmatige productformulier is nog geen afgeronde
  gebruikersflow en moet expliciet terugkomen in een latere scan-gerichte inventory-slice.

## Niet-standaard context

Lees `.codex/current-session-deferred-context.md` alleen wanneer historische
pilotdetails, scan-/QR-grondslagen, auth-/storage-samenvattingen, baseline-teststatus,
Raspberry Pi-/runtimecontext of post-vakantie vervolgvragen concreet relevant zijn.

## Documentatie

Bij iedere pilotstory blijven minimaal deze documenten onderling consistent:

- `README.md`;
- `.docs/releases/holiday-pilot-2026.md`;
- relevante actuele epic/userstory en `.docs/TODO.md`;
- geraakte legacy-userstories en `.docs/legacy-analysis/legacy-coverage-register.md`;
- deze handoff.

Storystatus, pilotvoortgang en eerstvolgende story moeten overeenkomen.
Documentatiewijzigingen worden na controle automatisch gecommit en naar de actuele
remote branch gepusht, tenzij de gebruiker expliciet anders vraagt of dit onveilig is.

## Handoffregel

Houd dit bestand als actuele momentopname. Bewaar alleen branchstatus, actieve release,
blockers, relevante productstatus en eerstvolgende actie. Historische implementatie-,
test-, PR- en acceptatiedetails horen in git, de release- of epicdocumentatie of
`.codex/current-session-deferred-context.md`.
