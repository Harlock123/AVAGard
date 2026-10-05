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
| Sound | — | None yet | — | Silent; see KNOWN_ISSUES |
| Application icon | — | None yet | — | The Avalonia template logo was removed |

## Not used (deliberately)

- The original C64 program, loader, character set, sprites, SID music and effects, and title screen.
- The original manual's prose (only paraphrased in research notes).
- Any code or assets from existing fan ports (licenses missing or unclear; see SOURCE_LEDGER.md).
- The C64 ROMs. They were used only in a local research environment to generate test numbers.
