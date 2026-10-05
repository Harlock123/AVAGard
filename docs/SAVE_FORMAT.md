# Save format (schema version 1)

Saves are UTF-8 JSON, written atomically (temporary file, flush, rename), in the per-user save
folder (see README).

| File | Meaning |
|---|---|
| `NAME.tgsave` | Stored character (F1). One per normalised name, like the original's `NAME.PLR` |
| `NAME.tgsuspend` | Modern resume point, rewritten at each command prompt. Deleted on death |

## Envelope

```json
{
  "Format": "avatelengard-save",
  "SchemaVersion": 1,
  "Content": "c64-v4.18",
  "AppVersion": "1.0.0.0",
  "Kind": "Manual",              // or "Suspend"
  "Preset": "Classic",           // or "Modern"
  "DeadlineSeconds": null,       // Modern deadline; null = deliberate (ignored for Classic)
  "ManualDeathRule": false,
  "SavedAt": "2026-10-05T12:00:00+00:00",
  "Checksum": "<sha256 of the compact JSON of State>",
  "State": { ... }
}
```

## State (`GameState`)

- `Character`:
  - `Name`, `Stats` [6]
  - `Level`, `Gold`, `Banked`, `Experience`
  - `Hp`, `MaxHp`, `SpellUnits`, `MaxSpellUnits`
  - `Items` [11]: slots 1–7 hold the plus; 8–10 hold counts
  - `Timers` [12]: effect turns, slots 1–11
  - `BoxCombination` [5]: slots 1–4
- `X`, `Y` (1–200) and `Z` (0 at an inn, otherwise 1–50).
- `Followers` [20]: `{Type, Level, Hp}`, nearest first.
- `RandomState`: the SplitMix64 state, so play resumes the same random sequence.
- `Explored`: packed `z<<16 | y<<8 | x` (Modern automap).
- `TurnCount`.

The dungeon itself is never saved. It is a pure function of the coordinates, and features are never
consumed (as in the original).

## Validation and errors

Loading checks, in order:

1. JSON is parseable.
2. The format id matches.
3. The schema version is not newer than the app supports ("made by a newer version").
4. The content id is known.
5. The checksum matches.
6. The state deserialises.
7. Array lengths and value ranges are valid (position, level, HP, name).

Every failure becomes a readable message. The title screen lists unreadable files instead of
hiding them.

## How saving interacts with each preset

| Event | Classic | Modern |
|---|---|---|
| F1 at the prompt or in an inn | Write `.tgsave`, return to title | Write `.tgsave`, continue |
| Command prompt reached | — | Write `.tgsuspend` (crash and exit recovery) |
| Death | `.tgsave` kept (V4.18). With the manual rule: deleted unless the name starts with SV | Same; `.tgsuspend` deleted |
| DEMO character | Cannot be stored | Same; no resume point |

**Crash recovery vs gameplay saves:** Classic has no automatic recovery. A crash loses progress since
the last F1, as on the original hardware, so recovery copies cannot undermine Classic death rules.
Modern's resume point is a convenience, documented in CLASSIC_VS_MODERN.md.

Future schema changes must bump `SchemaVersion` and add a migration in `SaveStore.Parse`.
