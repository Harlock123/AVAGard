# Fidelity baseline and matrix

## Reference version

**Commodore 64 disk release, program header `TELENGARD/64 V4.18`** (Avalon Hill Microcomputer Games,
1983; author Daniel Lawrence; manual credits "Orion Software").

Defining artifacts (research evidence, kept outside the repository; see `research/c64-mechanics.md` §0):

| Artifact | Identity |
|---|---|
| Disk image `Telengard_1983_Avalon_Hill.d64` (archive.org item `d64_Telengard_1983_Avalon_Hill`) | md5 `22da3702ca0124317f0157092e02858a` |
| Main program `DISK TELENGARD` (PRG, 32648 bytes) | md5 `c5c24dd50ceaf5312607179df12d9d7e` |
| Avalon Hill *Telengard Rules Manual*, 2nd ed. 3/83 (multi-platform, includes C64 key card) | mocagh.org scan; archive.org C64 copy |
| C64 BASIC ROM 901226-01 (float semantics only) | md5 `57af4ae21d4b705c2991d98ed5c1f7b8` |

### Why this version

1. The task brief names C64 as the default baseline, and it has the strongest evidence: the complete
   original program was recovered from a preserved disk image and decoded, so rules can be read
   directly instead of inferred.
2. Contemporary reviewers single out the C64 version as the best presented ("the most flair",
   COMPUTE! 9/83; Different Worlds 1985). It is the only version with colour, sprites and sound.
3. Its map formula has been executed on the genuine BASIC ROM, so the dungeon is reproducible bit-for-bit.

### How other releases differ (from evidence, not assumed)

| Release | Known difference | Source |
|---|---|---|
| Atari 8-bit | **Different map formula** (same constants, different expression), so a different dungeon. Time Stop skips *all* encounters. Combat formulas are the same. | Dan Boris's annotated Atari listing (archive.org) |
| Apple II, TRS-80, PET, CP/M | Text symbols (X, #, $, letter codes), no colour or sound. Different command keys for Store and the potions. | Manual key card; reviews |
| IBM PC (1985) | Rewritten in C by Lawrence; source lost. The addendum requires a lowercase "sv" prefix to recall dead characters. | 1985 addendum; aquest.com |
| "Telengard Remastered" (2016, fan) | Modified C64 BASIC (monster levels, hobbit, cloak). **Not original; never used as evidence.** | Its release notes |

This project is **not a hybrid**. Every rule follows V4.18. Where the manual disagrees with V4.18,
the code wins in Classic, and the disagreement is listed below.

## Evidence levels

- **V** verified from the V4.18 source, with listing lines cited in `research/c64-mechanics.md`.
- **VR** verified from source *and* by executing it on the real ROM.
- **M** stated in the manual only.
- **I** inferred (a reasonable reconstruction from indirect evidence).
- **A** reconstruction assumption or modern addition, clearly labelled.

## Matrix

"Classic" is the faithful preset. "Modern" shares every gameplay rule; it changes only timing,
saving and presentation, and only where listed.

| System | Original behaviour (V4.18) | Ev. | Classic | Modern | Verification | Uncertainty |
|---|---|---|---|---|---|---|
| Map generation | Each room is derived from (x,y,z) by a floating-point formula; nothing is stored | VR | Bit-exact port of the C64 40-bit float routines | Same map | Golden tests: real-ROM output for all of levels 1, 2, 10, 25 and 50 (204,020 rooms); model hashes for all 51 levels | None for the tested levels |
| Bounds | x,y 1–200, z 1–50, solid borders, no wrap | V | Same | Same | Unit tests | — |
| Walls and doors | 0/1 open, 2 door (passable, blocks sight), 3 solid | V | Same | Same | Unit tests | — |
| Features | 1 inn stairs (z1) or elevator up (z≥2); 2 pit; 3 teleporter; 4 stairs down; 5 altar; 6 fountain; 7 cube; 8 throne; 9 box. Stairs up are derived from the room above. Features never deplete. | V | Same | Same | Unit tests | — |
| Line of sight | 3×3 view; walls beyond a door or wall are hidden | V | Same | Same, plus optional automap of explored rooms (**A**, presentation only) | View tests | — |
| Character creation | 3d6 ×6 (STR INT WIS CON DEX CHR). Any key rerolls, RETURN accepts. HP=CON, SU=1. Start at (25,13,1) | V | Same | Same; reroll is an explicit button | Tests with fixed RNG | Manual says names ≤7 chars; code allows 30 (only 7 shown) |
| DEMO character | Empty name gives "DEMO": no timeouts, cannot be saved | V | Same | Same | Tests | — |
| Levels | Level up when EX ≥ 1000·2^LV, one level at a time, +1..CON HP, +LV SU. Level drain halves EX | V | Same | Same | Tests | — |
| Turn order | Tick timers and regen → 30% encounter → 20% treasure → room feature → command prompt | V | Same | Same | Engine tests | — |
| Input deadline | 400 keyboard polls, then the prompt's default (STAY, WAIT and so on) | V | 3.7 s per prompt (**I**: emulated 3.4–3.8 s; manual says "about 5") | Adjustable 2–15 s, or Deliberate (no deadline, disclosed as a rule-timing change) | Timer tests with a fake clock | Real-hardware seconds not measured |
| Monsters | 20 types, uniform at every depth. Level `INT(RND^1.5·(2z+2)+1)`, HP `INT(RND^.5·ML·M+1)`, XP `ML·M·10` | V | Same | Same | Statistical tests | — |
| Combat | To-hit and damage formulas, initiative from DEX, evade with d18 < DEX+boots, special attacks (drain, paralysis, breath, sword/whip) | V | Same | Same | Tests with scripted RNG | — |
| Friendly encounters | Elf heals, hobbit steals, dragon gift; plus a 5% random special | V | Same | Same | Tests | — |
| Following monsters | Evaded monsters queue behind you and attack when you STAY; their HP is re-rolled | V | Same | Same, plus a visible distance label | Tests | — |
| Spells | 36 spells, 6 levels; level n needs LV ≥ 3(n−1); cost = level; combat spells wasted outside melee | V | Same | Same, plus spell descriptions (**A**, newly written) | Tests per spell | Manual describes only levels 1–2 |
| Ring of Protection | **No effect in the code** (the manual says it absorbs damage) | V | No effect (code wins) | No effect, but the tooltip says it has none | Test | Manual/code conflict documented |
| Shield vs breath | No effect on breath (the manual implies it helps) | V | Same | Same | — | Manual/code conflict |
| Treasure | Coins (refuse to jewels), chests (traps, cobwebs), items 1–10. Items 1–7 are **replaced** by the new plus | V | Same | Same; confirmation shows the current vs new plus (presentation) | Tests | — |
| Inn | Gold becomes XP 1:1, level check, full heal, SU restored, timers cleared, queue cleared | V | Same | Same | Tests | — |
| Inn names | Three words from coordinate-derived indexes | V | Same formula with **newly written word lists** (**A**; the original lists are not copied) | Same | Test | Starting inn name differs from the original's |
| Altar, fountain, cube, throne, box, pit, teleporter | Probabilities and effects as specified | V | Same | Same | Tests per feature | — |
| Saving | F1 at the command prompt or in an Inn writes the character, then returns to the title. DEMO cannot be saved | V | Same: save and quit | Save without quitting, plus a suspend snapshot (**A**) | Save/load tests | — |
| Death | "Try again?" goes back to the title; **the save file is left untouched** | V | Same (default). Optional "manual death rule": delete the save on death unless the name starts with SV (**M**) | Same as Classic default | Tests | Manual and V4.18 disagree |
| Message pacing | Fixed delays of about 1.5 s | V | 1.5 s pauses, skippable with any key (disclosed) | Short pauses, skippable | — | — |
| Keyboard buffer | The C64 buffers keypresses (reviews mention accidental moves) | I | **Not emulated** (input during pacing is ignored) | Same | — | Disclosed deviation |
| Randomness | `RND` seeded from the clock | V | Seeded SplitMix64. Same distributions, not the same sequence | Same | — | The original RNG sequence is not reproduced |
| Sound and graphics | Sprites, custom charset, SID effects | V | Newly made retro-style presentation (**A**); no original assets | Same, with scale and contrast options | — | — |
