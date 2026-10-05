# Asset provenance inventory

| Asset | Path | Origin | License | Notes |
|---|---|---|---|---|
| VT323 font | `src/Telengard.Desktop/Assets/Fonts/VT323-Regular.ttf` | google/fonts, `ofl/vt323`, commit 7085eb8 (2026-10-05). sha256 `cf4de751…8594d` | SIL OFL 1.1 | License text shipped as `VT323-OFL.txt` |
| Inter font | NuGet `Avalonia.Fonts.Inter` 12.1.3 | Avalonia project | MIT package; Inter is OFL 1.1 | UI text |
| Dungeon graphics (walls, doors, stairs, pit, teleporter, altar, fountain, cube, throne, box, inn, hero figure, monster badges) | `src/Telengard.Desktop/Controls/*.cs` | Drawn in code for this project | Project's own | No original sprites or charset used |
| Monster codes and colours | `Monsters.Code`, `Palette` | New | Project's own | Undead use square badges, the living use round ones (shape, not just colour) |
| Inn name word lists | `src/Telengard.Core/Content/InnNames.cs` | Newly written | Project's own | The original's lists are deliberately not copied, so names differ from the original |
| Messages, prompts, help text, spell descriptions, tooltips | Engine and Desktop sources | Newly written, paraphrasing the meaning of the original messages | Project's own | Not copied from the original program or manual |
| Monster, spell and item names | `Content/*.cs` | Names as printed in the 1982 manual | Factual names | "Magic Missile" is spelled correctly (the program has "MISSLE") |
| Golden test data `tests/Telengard.Core.Tests/Golden/*.bin(.gz)` | Generated | Numbers produced by running the map formula (genuine ROM, or the float model) | Project's own data | Contains no program text or ROM code |
| Retro sound effects (25 WAVs) | `src/Telengard.Desktop/soundpacks/avagard-retro` | Synthesised by `tools/retro_audio.py` (SID-style voices, no samples) | CC0 | Default effects pack |
| Title and dungeon music: "Beneath the Inn", "Graph Paper" | `soundpacks/avagard-music/avagard_*.ogg` | New compositions rendered by `tools/retro_audio.py` | CC0 | Not based on the original game's music |
| Other music (16 tracks) | `soundpacks/avagard-music` | AVABand's cc0-dungeon-music pack (OpenGameArt CC0, plus AVABand's own boss theme) | CC0 | Credits in the pack's `CREDITS.txt`. AVABand's title theme is deliberately not used |
| Ambience loops (3) | `src/Telengard.Desktop/ambience` | AVABand (OpenGameArt CC0) | CC0 | `ambience/CREDITS.md` |
| Dubtrain effects pack | `soundpacks/angband-dubtrain` | Angband 4.2 `lib/sounds`, via AVABand | CC BY 4.0 | Optional; attribution in THIRD_PARTY_NOTICES and the pack's LICENSE.txt |
| OpenAL Soft Windows DLLs | `src/Telengard.Audio/native/win-x64`, `win-x86` | Official OpenAL Soft 1.23.1 release binaries, unchanged (via AVABand; sha256 of the release zip in `native/README.md`) | LGPL-2.0-or-later | Loaded at run time |
| Application icon | — | None yet | — | The Avalonia template logo was removed |

## Not used (deliberately)

- The original C64 program, loader, character set, sprites, SID music and effects, and title screen.
- The original manual's prose (only paraphrased in research notes).
- Any code or assets from existing fan ports (licenses missing or unclear; see SOURCE_LEDGER.md).
- The C64 ROMs. They were used only in a local research environment to generate test numbers.
