# Status and handoff (2026-10-05)

Environment used: Linux aarch64 (Arch), .NET SDK 10.0.400, Avalonia 12.1.3, xUnit v3 3.2.2.

## Milestones

| # | Milestone | State |
|---|---|---|
| 1 | Research dossier, source ledger, fidelity matrix, baseline | **Done.** C64 V4.18 chosen and documented |
| 2 | Runnable .NET 10 / Avalonia skeleton + headless test | **Done** |
| 3 | Character creation, dungeon rendering, movement, interaction | **Done** |
| 4 | Vertical slice: create → explore → monster → fight/cast/evade → reward → inn → save/resume → death/restart | **Done.** Every step is driven through the real UI by keyboard in headless tests (`VerticalSliceTests`, `SmokeTests`) and tested in the engine |
| 5 | Full monster, spell, progression, treasure and feature systems | **Engine complete** for V4.18 (all 20 monsters, 36 spells, all features). Presentation is basic (badges) |
| 6 | Modern usability and accessibility | **Mostly done**: deadline options, deliberate mode, pause, automap, coordinates, log, tooltips, key remapping, UI scale, high contrast, reduced motion, tutorial, resume points. **Missing:** sound, screen-reader review, high contrast for the side panels |
| 7 | Fidelity, persistence, packaging validation, docs | **Partly done.** Docs written, publish commands verified for 3 RIDs. **Not done:** Windows and macOS runs, installers and bundles, icon |

## Implemented / tested / observed

**Implemented:**
- Bit-exact V4.18 dungeon.
- The complete V4.18 rules engine:
  - turn order and character creation;
  - levels and level drain;
  - encounters, the 20 monsters and their specials;
  - combat, evasion and the follower queue;
  - all 36 spells;
  - treasure and items;
  - inn, elevator, pit, teleporter, stairs, altar, fountain, cube, throne and box;
  - storing, quitting, death and Raise Dead.
- Classic and Modern policies.
- Versioned, checksummed atomic saves and Modern resume points.
- Settings.
- The Avalonia UI: title, creation, game, settings, help, pause, death and tutorial.

**Tested (automated, 69 tests, all passing: 63 core, 6 UI):**
- **Map:** matches real-ROM output for 204,020 rooms (levels 1, 2, 10, 25, 50), plus model hashes for
  all 51 levels.
- **Engine flow:** stairs and inn, timeouts, stale input, DEMO, blocked moves, combat XP, save
  behaviour per preset, quitting, scroll of rescue.
- **Spells:** Pass Wall, level gating, wasted combat spells, durations, Word of Recall.
- **Simulation:** 2,000 random sessions × 2 presets with invariants, every prompt kind reached, and
  determinism.
- **Saves:** round trip, RNG continuity, corruption, newer versions, death rules, settings.
- **Deadlines:** fake-clock behaviour, re-arm, pause, end-to-end timeout to STAY.
- **UI (headless Avalonia):** title, creation by keyboard, play with keys until death, key-repeat
  suppression, resume point listed on the title screen, monster rendering in both presets, and the full
  vertical slice by keyboard (cast Light → stairs → inn → F1 store → title → read in → quit/die → saved
  copy survives → try again → title).

**Observed working:**
- Headless UI screenshots were inspected: title, game start, monster encounter (Classic and Modern),
  help overlay, death screen and inn.
- `dotnet run` and the published linux-arm64 build both start and stay up (8 s smoke runs, no errors).
- **A human has not yet played an interactive session on a real display. Do this first.**

**Not yet tested:**
- Windows and macOS execution.
- Interactive play on real hardware.
- Interface scales other than 100%.
- Settings changes taking effect in a running game.
- Classic attribute auto-cycling in the UI.
- The number-entry prompts in the UI (donation, cube, teleport).
- Disk-full and permission-error paths.

**Historically uncertain:** see [RESEARCH_SUMMARY.md](RESEARCH_SUMMARY.md) and
[KNOWN_ISSUES.md](KNOWN_ISSUES.md) (real-hardware timing, RND sequences, keyboard buffer, exact
wording, current rights holder).

**Deferred:** sound, monster pixel art, installers and bundles, an app icon, and a balance-analysis
simulation with a sensible player policy.

## Commands

```sh
dotnet build Telengard.slnx
dotnet test Telengard.slnx
dotnet run --project src/Telengard.Desktop
TG_SCREENSHOT_DIR=/tmp/shots dotnet test tests/Telengard.Desktop.Tests   # UI screenshots
dotnet publish src/Telengard.Desktop -c Release -r win-x64 --self-contained true -o out/win-x64
```

## Open decisions for the project owner

1. **Distribution and rights.** No statement clears public distribution of a recreation using the
   *Telengard* name. The rights holder is likely Hasbro (via Avalon Hill) and/or the Lawrence estate.
   Decide whether to seek permission, rename, or keep it private.
2. **Death rule default.** V4.18 behaviour (stored characters survive death) or the manual's SV rule.
   The current default is V4.18, with the SV rule optional.
3. **Sound backend.** Adding SoundFlow introduces a native dependency (miniaudio, MIT/public domain).

## Next concrete tasks

1. Run on **Windows**: `dotnet run`, then play through create → inn → store → read in → death.
   Record the results here.
2. Add sound: an `ISoundPlayer` implementation with synthesised cues.
3. Write a policy-driven balance simulation for Modern (a cautious bot that heads to the inn when hurt).
4. Add UI tests for the number-entry prompts and settings round-trips.
5. Package: an app icon, a macOS bundle, and a Windows zip or installer.
