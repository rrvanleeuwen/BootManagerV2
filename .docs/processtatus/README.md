# Processtatus

Deze map is de vaste overdrachtsplek tussen Claude Code en Codex tijdens een actieve
implementatiebranch.

## Regels

- Claude schrijft per branch naar `.docs/processtatus/<branch-map>/ClaudeStatus.md`.
- `<branch-map>` is de actuele branchnaam met iedere `/` vervangen door `-`, zodat de
  mapnaam geldig blijft op Windows.
- Claude zet in `ClaudeStatus.md` zijn volledige `Completion Notes`.
- Claude sluit altijd af met een aparte regel:

```text
Done: yyyy-MM-dd HH:mm
```

- Die `Done:`-regel betekent alleen dat Claude zijn implementatieronde heeft afgerond
  en dat Codex de review moet oppakken.
- `Done:` betekent nadrukkelijk niet dat de story geaccepteerd, afgerond of
  productierijp is.

## Codex-werkwijze

- Codex behandelt een nieuwe, nog niet verwerkte `Done:`-timestamp als reviewtrigger
  voor die branch.
- Codex gebruikt `ClaudeStatus.md` als eerste samenvattende handoff, maar controleert
  daarna altijd zelf diff, tests, write-set en acceptatiecriteria.
