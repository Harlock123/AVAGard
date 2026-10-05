# Architecture

```
Telengard.Core            (no package dependencies)
├── Numerics/CbmFloat         bit-level C64 40-bit float (round-before-push, truncating add/mul)
├── Dungeon/                  RoomWordGenerator (V4.18 formula), DungeonMap (cached, decoded), LocalView (line of sight)
├── Content/                  monsters, items, spells (+ new descriptions), inn-name lists (newly written)
├── Model/                    Character, GameState (everything a save needs), CharacterCreation, Effect slots
├── Engine/                   Game (rules flow), Prompt/Command/PlayerInput, GameEvent, RulesPolicy
├── Randomness/               IRandomSource, SplitMix64Random (serializable state)
└── Timing/PromptDeadline     real-time pressure on an injected TimeProvider
Telengard.Infrastructure  → Core
├── Saves/                    SaveFile envelope (versioned, checksummed JSON), SaveStore (atomic, validated)
├── Settings/                 GameSettings, SettingsStore
└── Storage/                  UserDataPaths, AtomicFile
Telengard.Desktop (Avalonia 12, CommunityToolkit.Mvvm) → Core, Infrastructure
├── ViewModels/               Main (screen host), Title, Creation, Game, Settings, Help
├── Views/                    XAML views; MainWindow routes keys (tunnel, auto-repeat suppression)
├── Controls/                 DungeonView, AutomapView (custom Render; no per-room controls), Glyphs, Palette
├── Input/                    KeyMap (original keys + arrows/keypad, remappable), PromptPresenter
└── Services/                 AppServices, ISoundPlayer (silent for now)
```

## The engine

`Game` is the whole rules engine for one session. It is driven by explicit input:

1. `Start(StartPoint)` returns the events up to the first `Prompt`.
2. `Submit(PlayerInput, promptSequence)` returns the events up to the next prompt.

Input for a stale sequence number is ignored. So are a timeout on a prompt without a deadline, and
anything after the session has ended. This makes late timer callbacks and double clicks harmless.

The flow is written as a C# iterator state machine. Each private `Label` method corresponds to a
block of the V4.18 listing, cited by line number, so it can be reviewed against
`research/c64-mechanics.md` block by block. Prompts are `yield return`ed. Sub-flows use
`foreach (... ) yield return`. Transient encounter registers mirror the original variables
(`M`, `ML`, `MH`, `L`, `D`, `T`, `C`).

**Turn order** (line 2000 onwards): tick timers and regeneration → encounter (30%) → treasure (20%)
→ room feature → command prompt.

**Randomness:** a single injected `IRandomSource`. Its state is copied into `GameState.RandomState`
after every step, so saves resume the same sequence. The **map does not use randomness**:
`RoomWordGenerator` is a pure function of (x, y, z).

**Time:** game rules never read the clock. `PromptDeadline` wraps `TimeProvider.CreateTimer`. On
expiry it calls back with the prompt sequence, and the host submits `PlayerInput.Timeout`, which the
engine maps to that prompt's default (STAY, WAIT and so on). Tests use a manual `TimeProvider`.

**Policies:** `RulesPolicy` (Classic or Modern) holds only the deadline, save behaviour, suspend
snapshots, exploration tracking and the optional manual death rule. Gameplay code is shared. The
only engine branch on policy is `SaveEndsSession`, after a successful save.

**Events:** `MessageEvent`, `PauseEvent` (presentation pacing; never affects rules), `ClearMessagesEvent`,
`SoundEvent`, `MonsterAppeared`/`Gone`, `MovedEvent`, `SaveRequestedEvent`, `DiedEvent`, `SessionEndedEvent`.

## Ordering and input rules (desktop)

- `GameViewModel.Pump` plays events strictly in order. While it runs, game input is not accepted;
  a key press only shortens the remaining pauses.
- After the queue drains, the current prompt is shown. Its action buttons are built, and its
  deadline is armed if `Prompt.HasDeadline` and the policy has a deadline.
- `Submit` clears `_shown` before calling the engine, so a second click or key for the same prompt
  is dropped. `MainWindow` also suppresses keyboard auto-repeat (one press, one action).
- Saving: the engine yields a `Saving` prompt. The host writes the file on a worker thread, then
  answers `SaveSucceeded` or `SaveFailed`.
- Rendering never drives the simulation. `DungeonView` redraws when a new immutable
  `DungeonSnapshot` is assigned.

## Persistence

See [SAVE_FORMAT.md](SAVE_FORMAT.md). Writes are atomic (temp file + flush + rename). Loads validate
the format id, schema version, rules-content id, checksum and value ranges, and turn every failure
into a `SaveLoadException` with a clear message.

## Testing

- `RoomWordGeneratorTests`: real-ROM golden data (levels 1, 2, 10, 25 and 50 in full) and model
  hashes for all 51 levels.
- `EngineFlowTests`, `SpellTests`: scripted randomness (`ScriptedRandom`) for exact paths.
- `SimulationTests`: 2,000 seeded random-input sessions per preset. They check invariants and that
  every prompt kind is reachable, and confirm determinism.
- `SaveStoreTests`: round trip, RNG continuity, corruption, newer versions, death rules, settings.
- `PromptDeadlineTests`: fake-clock deadline, re-arm, pause, end-to-end timeout to STAY.
- `Telengard.Desktop.Tests`: headless Avalonia (Skia) UI flows with key presses and screenshots.
