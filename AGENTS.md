# AGENTS.md

Notes for anyone, human or AI agent, changing this code. `README.md` covers playing, downloads and where files are
stored, and `docs/` holds the research, fidelity notes, save format and architecture; this file covers how to
build, test and change the code safely.

## What this is

AVAGard, a recreation of the 1982 dungeon crawler Telengard (the Commodore 64 version 4.18), in C# on .NET 10 and
Avalonia 12. The rules engine follows the original BASIC listing block by block, and the dungeon map is checked
against real-ROM data. Sound plays through OpenAL Soft, from sound-pack folders.

## Commands

```bash
dotnet build AVAGard.slnx                     # the SDK version is in global.json (10.0.400 or later)
dotnet test AVAGard.slnx                      # every test runs headless: no window opens
dotnet run --project src/Telengard.Desktop    # play it; --no-audio for silence
TG_SCREENSHOT_DIR=$PWD/shots dotnet test tests/Telengard.Desktop.Tests    # save screenshots of the UI tests
python3 tools/retro_audio.py <effects-dir> <music-dir>    # synthesise the retro effects and title music (needs ffmpeg)
```

- `Directory.Build.props` sets `net10.0`, nullable, implicit usings and `TreatWarningsAsErrors` for every project:
  a warning fails the build, so keep it at zero.
- Package versions are pinned centrally in `Directory.Packages.props`. `xunit.v3` stays at 3.2.2 to match
  `Avalonia.Headless.XUnit`.
- The executable is named `AVAGard` (`AssemblyName` in `Telengard.Desktop.csproj`).

## Layout

- `src/Telengard.Core/`: the rules, with no package dependencies. `docs/ARCHITECTURE.md` describes it in detail.
  - `Engine/Game*.cs`: the whole rules engine, an iterator state machine driven by `Start` and `Submit`. It
    yields `Prompt`s and `GameEvent`s.
  - `Dungeon/RoomWordGenerator.cs`: the map as a pure function of (x, y, z); `DungeonMap`, `LocalView`.
  - `Numerics/CbmFloat.cs`: bit-level C64 floating point. `Content/`: monsters, items, spells, inn names.
  - `Model/`: `Character`, `GameState` (everything a save needs). `Timing/PromptDeadline.cs`: real-time prompts.
  - `Randomness/`: `IRandomSource` and `SplitMix64Random`, whose state can be saved.
- `src/Telengard.Infrastructure/`: saves (`SaveFile`, `SaveStore`), settings, `UserDataPaths`, `AtomicFile`.
- `src/Telengard.Audio/`: the OpenAL engine (with a silent `NullAudioEngine`), decoders, sound packs and
  `SoundDirector`, ported from AVABand. `native/` holds the Windows `soft_oal.dll` builds.
- `src/Telengard.Desktop/`: the Avalonia app. `ViewModels/`, `Views/`, custom-drawn `Controls/` (`DungeonView`,
  `AutomapView`), `Input/KeyMap.cs`, `Services/AppServices.cs`. Sound packs ship as loose `soundpacks/` and
  `ambience/` folders copied next to the executable.
- `tests/Telengard.Core.Tests/`: engine, map (golden ROM data in `Golden/`), saves, timing and simulation tests.
- `tests/Telengard.Desktop.Tests/`: headless Avalonia UI flows.
- `tools/retro_audio.py`: standard-library Python that writes the `avagard-retro` effects and the title music.

## Rules that keep it working

- **The engine is driven by input, not time.** Game rules never read the clock; `PromptDeadline` uses an injected
  `TimeProvider`, and tests use `ManualTimeProvider`. Rendering never drives the simulation.
- **One random source.** Draw random numbers only from the injected `IRandomSource`. Its state is copied into
  `GameState` after every step, so saves resume the same sequence. The map uses no randomness at all.
- **Fidelity to the original.** Engine methods cite the V4.18 listing by line number. Check changes against
  `docs/research/c64-mechanics.md`, and keep the golden map tests passing.
- **Saves must load or fail clearly.** Writes are atomic (temp file, flush, rename). Loads validate format,
  version and checksum. See `docs/SAVE_FORMAT.md` before changing anything that is saved.
- **User files.** Saves go to `SpecialFolder.LocalApplicationData` and settings to `SpecialFolder.ApplicationData`,
  each in an `AVAGard` folder. Tests use temporary folders under `Path.GetTempPath()`, never the player's.
- **Audio.** OpenAL Soft through Silk.NET; without a device, or with `--no-audio`, the game stays silent.
  - `Directory.Build.targets` swaps in the official Windows `soft_oal.dll` from `src/Telengard.Audio/native/`,
    which needs no Visual C++ runtime.
  - `OpenAlConfig` writes the mixing period to a file named by `ALSOFT_CONF` (a larger one in a virtual machine)
    so the sound doesn't break up. A player's own `ALSOFT_CONF` is left alone.
- **Line endings.** Keep each file's existing line endings (at least one file is CRLF) so diffs show only real
  changes.

## Making a change

- Add or update tests with every rules change. `dotnet test` must pass, with no warnings.
- If the change shows on screen, save the UI screenshots with `TG_SCREENSHOT_DIR` and look at them.
- Update `README.md` and the relevant files in `docs/` (fidelity, known issues, save format) to match.
- New retro sounds go into `tools/retro_audio.py`; record the source and licence of other sounds in
  `docs/ASSET_PROVENANCE.md` and the pack's credits file.

## CI and releases

`.github/workflows/build.yml` builds and tests the solution on every push and pull request, using the SDK from
`global.json`. It then publishes self-contained single-file builds for win-x64/x86/arm64, linux-x64/arm64/arm and
osx-x64/arm64, and keeps them as workflow artifacts. Pushes to `main` replace the assets of the `latest`
pre-release, and a `v*` tag makes a proper release.
