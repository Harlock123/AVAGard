# Dungeon generation: evidence and validation

The reference dungeon (C64 V4.18) is not stored. Each room's 16-bit word is computed from its
(x, y, z) coordinates with Commodore BASIC floating point. Pseudo-code and the room-word layout are
in [c64-mechanics.md §1](c64-mechanics.md).

## Why exact float emulation is required

The walls come from the lowest bits of `frac(q) × 4694`. On deep levels, `q` is in the millions, so
only about 11 bits of fraction survive in the C64's 32-bit mantissa. Rounding details then decide the
walls. A straightforward IEEE `double` port gives the wrong room word for **23,034 of 204,020** tested
rooms: 0.8–2.1% on level 1, rising to 16.8–42.7% on level 50. Almost all of the errors are in walls.

`Telengard.Core.Numerics.CbmFloat` therefore models the BASIC ROM's 40-bit routines at bit level:
operand rounding before push, truncating add and multiply, `INT`, and integer assignment.

## How the ground truth was produced (research environment, 2026-10-05)

1. The original program was extracted from the archive.org C64 disk image (md5 `22da3702…`) and
   detokenised.
2. A headless 6502 emulator ran the **genuine C64 BASIC/KERNAL ROMs** (901226-01 / 901227-03, used
   only to generate test data), executing the original room-word lines for every coordinate.
3. A bit-level Python model of the ROM float routines was compared against that output.

| Data | Rooms | Model mismatches |
|---|---|---|
| ROM, levels 1, 2, 10, 25, 50 (x, y 0..201) | 204,020 | **0** |
| ROM, start area (level 1, y 0..14, x 0..40) | 615 | 0 |

The real ROM also raises `?ILLEGAL QUANTITY` exactly where the model predicts a feature value of 16
at room (90,40,50).

## Tests in this repository

`tests/Telengard.Core.Tests/RoomWordGeneratorTests.cs`:

- `rom-zNN.bin.gz`: real-ROM room words for complete levels 1, 2, 10, 25 and 50. The C# port matches all of them.
- `rom-z01-y00-14-x00-40.bin`: the first ROM capture of the start area.
- `model-zNN.bin.gz` and `model-map-sha256.txt`: model output for levels 1 and 50 in full, plus
  SHA-256 hashes for every level 0–50.

The test vectors are numbers produced by running the formula. They contain no original program
text or ROM code.
