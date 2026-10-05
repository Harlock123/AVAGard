# Research summary (2026-10-05)

## What Telengard is

- A single-player, real-time dungeon crawler. Daniel M. Lawrence derived it from his mainframe game
  *DND* (DECsystem-10 / PDP-11, 1976–79) and rewrote it in BASIC on a Commodore PET in 1978.
- He ported it himself to the Apple II, TRS-80 and Atari 800. Avalon Hill's Microcomputer Games
  division licensed and published it (1982–83: Apple, TRS-80, PET, Atari, C64; CP/M 1984; IBM PC
  1985, a C rewrite).
- The dungeon is computed from coordinates rather than stored, because 8K–32K machines had no room
  for maps. Real-time play originally stopped players hogging shared terminals.
- **There is no story, quest or win condition.** In Lawrence's words, "In 8k of memory there was no
  room for a story." The manual's framing is a few lines: adventurers descend into a harsh dungeon;
  few return, with tales of horror and treasure. This recreation adds no narrative. Its newly written
  text (help, tooltips) is marked as such.

## Reference version and evidence

**Commodore 64 V4.18** is the reference. The complete original BASIC program was recovered from a
preserved disk image (archive.org) and decoded in a research environment. It is evidence only and is
**not** included in this repository.

- **Dungeon map (VR):** reproduced **bit-exactly**. A model of the C64's 40-bit floating point was
  validated against the genuine BASIC ROM on 204,020 rooms with zero mismatches. Plain `double`
  arithmetic would get 0.8–43% of rooms wrong, depending on depth.
- **Other rules (V):** character creation, levels, encounters, combat, all 36 spells, treasure,
  features, inns, saving and death all come straight from the listing. Line references are in
  [research/c64-mechanics.md](research/c64-mechanics.md).
- **Manual (M):** the full Avalon Hill rules manual (2nd ed., 3/83) was read. It supplies the key
  card, the intended meaning of attributes and items, and the atmosphere.
- **Cross-check:** Dan Boris's annotated Atari listing shows the same combat formulas but a
  *different* map formula. The dungeons of different ports are therefore not interchangeable.

## Notable findings (code vs. manual)

| Topic | Manual says | V4.18 code does | This project (Classic) |
|---|---|---|---|
| Ring of Protection | absorbs damage each round | no effect at all (Atari too) | no effect, disclosed in its tooltip |
| Shield | turns some dragon fire | no effect on breath | no effect |
| Time limit | "about 5 seconds" | 400 keyboard polls (≈3.4–3.8 s, emulated) | 3.7 s |
| Recalling dead characters | only names beginning "SV" | save file untouched; any stored character can be reloaded | code behaviour by default; manual rule optional |
| Name length | 7 characters | up to 30 (7 shown) | 30, 7 shown in the status panel title |
| Stairs | U/D commands | stairs prompt automatically when you enter or stay in the room | as code |
| Spells 3–6 | names only | effects fully specified | as code; descriptions newly written |

## Unresolved fidelity questions

1. **Exact time per prompt on real hardware.** It is measured only in emulation, and real VIC-II DMA
   adds about 5%. Classic uses 3.7 s.
2. **Original RND sequence.** It is seeded from the clock, so exact sequences are not reproducible.
   Distributions are faithful; sequences are not.
3. **Keyboard type-ahead.** The C64 buffers up to 10 keys, which reviews blame for accidental moves.
   This is not emulated.
4. **Exact on-screen wording, the title and music sequence, and sprite art** are not reproduced. All
   of it is replaced with new presentation.
5. **Who holds the rights today.** Hasbro (via Avalon Hill) and/or the Lawrence estate. No public
   statement was found. See [research/history-and-manual.md §B7](research/history-and-manual.md).

Coverage and gaps are listed in [SOURCE_LEDGER.md](SOURCE_LEDGER.md) and
[research/history-and-manual.md §D](research/history-and-manual.md).
