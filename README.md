# AVAGard: a Telengard recreation in .NET 10 and Avalonia

**AVAGard** is an independent, **unofficial** recreation of *Telengard* (Daniel Lawrence; Avalon
Hill, 1982–83).
It is a real-time dungeon crawl through a 50-level, 200×200-room dungeon. The dungeon is generated
from its coordinates and is the same in every game.

- **Classic** reproduces the Commodore 64 release (V4.18): its rules, its bit-exact dungeon and its
  real-time pressure.
- **Modern** uses the same rules and dungeon with a friendlier interface: adjustable or no time limit,
  an automap, coordinates, a full log, tooltips and an automatic resume point.

Sound and music play through OpenAL (the audio layer is shared with the author's AVABand). The
default pack is new SID-style effects with an original chiptune title theme. A CC0 dungeon music
pack and the Dubtrain effects pack are also included. Run with `--no-audio` for silence.

This project is not affiliated with or endorsed by the rights holders. No original program code,
artwork, music or manual text is included. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)
and [docs/ASSET_PROVENANCE.md](docs/ASSET_PROVENANCE.md). The name and branding live in a few
strings and can be replaced.

## Requirements

- .NET SDK 10.0.400 or a newer 10.0 feature band. `global.json` pins it with `latestFeature` roll-forward.
- Windows 10+, Linux (X11 or XWayland) or macOS. These are the platforms Avalonia 12 supports.
  See *Validation status* for what has actually been tested.

## Build, run, test

```sh
dotnet build AVAGard.slnx
dotnet run --project src/Telengard.Desktop
dotnet test AVAGard.slnx
```

UI tests run headlessly, with no window. To save screenshots of the UI states they drive:

```sh
TG_SCREENSHOT_DIR=/tmp/shots dotnet test tests/Telengard.Desktop.Tests
```

Avalonia's build tooling (`Avalonia.BuildServices`) sends anonymous **build-time** telemetry. To opt
out, set `AVALONIA_TELEMETRY_OPTOUT=1`. The game itself never uses the network.

## Publish

Self-contained single-folder builds:

```sh
dotnet publish src/Telengard.Desktop -c Release -r win-x64     --self-contained true -o out/win-x64
dotnet publish src/Telengard.Desktop -c Release -r linux-x64   --self-contained true -o out/linux-x64
dotnet publish src/Telengard.Desktop -c Release -r linux-arm64 --self-contained true -o out/linux-arm64
dotnet publish src/Telengard.Desktop -c Release -r osx-arm64   --self-contained true -o out/osx-arm64
```

The executable is `AVAGard` (`AVAGard.exe` on Windows). Sound packs ship as loose folders next to
it (`soundpacks/`, `ambience/`). Players can add their own packs under the settings folder. macOS app-bundle packaging and code
signing are not set up yet (see docs/KNOWN_ISSUES.md).

## Where things are stored

| | Windows | Linux | macOS |
|---|---|---|---|
| Saves | `%LOCALAPPDATA%\AVAGard\saves` | `$XDG_DATA_HOME/AVAGard/saves` (default `~/.local/share/...`) | `~/Library/Application Support/AVAGard/saves` |
| Settings | `%APPDATA%\AVAGard\settings.json` | `$XDG_CONFIG_HOME/AVAGard/settings.json` (default `~/.config/...`) | `~/Library/Application Support/AVAGard/settings.json` |

## Repository layout

```
src/Telengard.Core            rules engine, dungeon generator, C64 float emulation (no dependencies)
src/Telengard.Infrastructure  save files, settings, per-user paths, atomic writes
src/Telengard.Audio           OpenAL engine, decoders, sound packs, music and sound director (from AVABand)
src/Telengard.Desktop         Avalonia application (MVVM, custom-drawn dungeon view)
tests/Telengard.Core.Tests    engine, map (real-ROM golden data), saves, timing, simulations
tests/Telengard.Desktop.Tests headless UI tests
docs/                         research, fidelity matrix, architecture, guides
tools/retro_audio.py          synthesises the retro effects and the AVAGard music (stdlib Python + ffmpeg)
```

## Documentation

- [docs/STATUS.md](docs/STATUS.md): milestone status, validation record and handoff notes
- [docs/FIDELITY.md](docs/FIDELITY.md): reference version, baseline decision and fidelity matrix
- [docs/RESEARCH_SUMMARY.md](docs/RESEARCH_SUMMARY.md) and [docs/SOURCE_LEDGER.md](docs/SOURCE_LEDGER.md)
- [docs/research/](docs/research): the history and manual dossier, the C64 mechanics specification,
  dungeon-generation evidence, and the open-source and licensing survey
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)
- [docs/CLASSIC_VS_MODERN.md](docs/CLASSIC_VS_MODERN.md)
- [docs/PLAYER_GUIDE.md](docs/PLAYER_GUIDE.md): controls and how to play
- [docs/SAVE_FORMAT.md](docs/SAVE_FORMAT.md)
- [docs/KNOWN_ISSUES.md](docs/KNOWN_ISSUES.md): known issues and unverified historical behaviour
- [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) and [docs/ASSET_PROVENANCE.md](docs/ASSET_PROVENANCE.md)

## Validation status (honest summary)

- Built and tested on **Linux arm64 (Arch VM), .NET SDK 10.0.400**: all automated tests pass, the app
  and the published linux-arm64 build start without errors, and OpenAL opens the VM's audio device.
- **Windows has not been run or tested yet.** win-x64 and osx-arm64 builds publish successfully
  (cross-compiled), but neither has been launched. No cross-platform claim is made beyond that.
