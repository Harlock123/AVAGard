# Telengard (Commodore 64, "TELENGARD/64 V4.18") — mechanics specification

> Research document. Derived from a decoded listing of the original program that is kept
> **outside** this repository as research evidence (copyright Avalon Hill / Microcomputer Games;
> author Daniel Lawrence). Formulas are restated as pseudo-code with listing line numbers so the
> behaviour can be checked; the original source text is not reproduced. Research performed
> 2026-10-05 by a research sub-agent and reviewed against the listing by the lead developer.

All line numbers below refer to `listing_c64_DISK_TELENGARD.bas` (decoded by me from the
original disk image) unless prefixed `ATARI:` (Dan Boris's annotated DSKTEL.BAS listing).

## 0. Provenance of evidence

| Item | Source | Hash |
|---|---|---|
| C64 disk image | https://archive.org/download/d64_Telengard_1983_Avalon_Hill/Telengard_1983_Avalon_Hill.d64 (`dl/tel_c64.d64`) | md5 22da3702ca0124317f0157092e02858a, sha256 7eaf1173afb8a172c613f4ba9d5a0c9ee3bed42cabfa475b95aac0f962edab97 |
| `DISK TELENGARD` (main program, PRG, load $0801, 32648 bytes) | extracted with `tools/d64.py` → `c64files/DISK_TELENGARD.prg` | md5 c5c24dd50ceaf5312607179df12d9d7e |
| `D-TELENGARD` (loader: sprites, charset, ML helper at 38912-39091) | `c64files/D-TELENGARD.prg` | md5 a9e9056e5db8f5fbcf68ebcc29ad30cc |
| `DISK.CHAR.CREATE` (3rd-party char editor, "Jim Pendergraft 9/8/83" — NOT part of game) | `c64files/DISK.CHAR.CREATE.prg` | md5 ea8a6a67997d3fde241f445ed68b38f0 |
| `SHF.PLR`, `TANK.PLR`, `SAMUEL.PLR` | previous owners' save files (TANK is hacked: all stats 1000) | — |
| Decoded listings | `listing_c64_DISK_TELENGARD.bas` (926 lines incl. header), `listing_c64_D-TELENGARD.bas`, `listing_c64_DISK.CHAR.CREATE.bas` via my detokenizer `tools/detok.py` | — |
| Cross-check (Atari 8-bit, DSKTEL.BAS, commented by Dan Boris 2008) | https://archive.org/details/Telengard_Source_Code_Listing (`dl/listing_djvu.txt` OCR, `dl/listing.pdf`) | pdf md5 8683e4992262fa8e02a42ea074efca9d |
| C64 manual (OCR) | https://archive.org/details/Telengard_1983_Avalon_Hill (`dl/c64manual_djvu.txt`) | md5 158710be22871369b07e4e939ce897cf |
| Atari ATR (not decoded; Boris listing used instead) | `dl/tel_a8.atr` | md5 f241fd9fd3db78a46dee5abf119f8950 |
| C64 ROMs used only for ground-truth emulation | VICE-Team/svn-mirror `data/C64/basic-901226-01.bin`, `kernal-901227-03.bin` (`rom/`) | md5 57af4ae21d4b705c2991d98ed5c1f7b8 / 39065497630802346bce17963f13c092 (standard dumps) |

Program header (lines 10-40): `REM TELENGARD/64 V4.18`, `(C)COPYRIGHT 1981 BY MICROCOMPUTER GAMES`.
The loader (`D-TELENGARD`) prints "(C)OPYRIGHT 1983", installs sprites/charset and the small
ML block (raster/clear-screen helpers SYS38991/39004/39043, IRQ hook at 39082), then auto-LOADs/RUNs
`DISK TELENGARD`. Game logic is 100% in the BASIC program.

GitHub repositories consulted (all cloned under `repos/`, research only — nothing copied to the project):

| Repo (commit) | What | License |
|---|---|---|
| malbanGit/Telengard (22019b1) | Vectrex port in C; includes a 6809 MS-BASIC 40-bit float library to reproduce the C64 map | **no license file** (all rights reserved by default) |
| mgritter/telengard (806ad8c) | C++ C64 map reverse-engineering (uses IEEE double; notes it "doesn't give exactly the same results") | no license file |
| gondur/Telengard (0421203) & Abhisheksinha1506/Telengard_Remastered_1.1 (570a031) | "Telengard Remastered v1.1" — a MODIFIED C64 BASIC (CBM PRG Studio source). Changes monster level progression, hobbit steal, elven cloak (see its release notes) | gondur: none; Abhisheksinha1506 mirror: GPL-3.0 (re-upload, provenance unclear) |
| gbush235711/elm-telengard (7e0c123) | Elm map remake | MIT |
| Tallicia/TelengardXXXX (eec9478) | Python map using IEEE double | none |
| danlb2000/TelengardMapViewer (a3cd10b) | C# map viewer | MIT |
| shaefer/telengard, shaefer/telengard2 | JS homages | none |

The original game is commercial (Avalon Hill / Microcomputer Games, ©1981-83, author Daniel Lawrence);
the listing is evidence only.

---

## 1. Dungeon generation (verified-from-source + verified by running the real ROM)

### 1.1 Constants (listing line 1103, 1000-1009)
Coordinate weights XO=1.6915, YO=1.4278, ZO=1.2462 and multiplier W0=4694; helper functions
extract the north wall (`h AND 3`), west wall (`INT(h/4) AND 3`) and feature (`INT(h/256)`).
Exact packed 5-byte values after the C64 parses these decimals (read back from variable memory on the real ROM):

| var | packed bytes (exp, m1..m4) | 32-bit mantissa | value |
|---|---|---|---|
| XO | 81 58 83 12 6E | D883126E | 1.6914999997243285 |
| YO | 81 36 C2 26 82 | B6C22682 | 1.4278000006452203 |
| ZO | 81 1F 83 7B 4A | 9F837B4A | 1.246199999935925 |
| W0 | 8D 12 B0 00 00 | 92B00000 | 4694 |

### 1.2 Room word (listing lines 10010-10040)
Pseudo-code (the original BASIC is not reproduced here):
```
q   = x*XO + y*YO + z*ZO + (x+XO)*(y+YO)*(z+ZO)          // C64 MBF arithmetic, left-to-right
h   = int16( (q - floor(q)) * 4694 )                       // H%= : ROUND then truncate
if (h >> 8) > 5 : h &= 255                                 // no feature
if (h >> 8) > 0 :                                          // feature present (h>>8 in 1..5)
     f = floor( (q*10 - floor(q*10)) * 15 + 1 )            // 1..15 (rarely 16, see 1.5)
     h = (f << 8) | (h & 255)
if x==1 or x==201 : h |= 12        // west wall = 3
if y==1 or y==201 : h |= 3         // north wall = 3
```
Decoding (lines 1005, 6000-6020, 8150-8305, 10050-10085):
* `h & 3` = **north (up) wall** of room (x,y); `(h>>2) & 3` = **west (left) wall**.
  Value 0/1 = open, 2 = **door** (drawn wall with a gap: 10055-10060 pokes chr 162/163 in the middle), 3 = **solid wall**.
  Movement is blocked only by value 3 (`IF FNUP(L%)>2 ...`, 8150); doors are freely passable.
* South wall of (x,y) = north wall of (x,y+1) (`FNUP(P(2,1))`, 8200); east wall = west wall of (x+1,y) (`FNLF(P(1,2))`, 8300).
* `h >> 8` = feature code, reduced by **repeated subtraction of 9** (6010: `IF I>9 THEN I=I-9:GOTO6010`), NOT mod 10/9 in the arithmetic sense for 9 itself: 10→1 ... 15→6, 16→7, 9 stays 9.
* Bits 4-7 of h are meaningless (never read).

### 1.3 Feature codes (6014-6020, 6100-7185)
| code | meaning | line |
|---|---|---|
| 0 | nothing | 6014 |
| 1 | on level 1: **stairs up to an Inn** (6100-6102 forces J=4 → stairway with UP; going up at CZ=1 → CZ=0 → Inn 6103). On levels ≥2: "YOU FEEL HEAVY FOR A MOMENT" = **elevator up one level** (CZ-1), automatic | 6100-6235 |
| 2 | **pit** (on level 50 it acts as the level-1 elevator, i.e. up) | 6300 |
| 3 | **teleporter** | 6400 |
| 4 | **stairs down** (suppressed on level 50, 6013) | 6500 |
| 5 | **altar** | 6600 |
| 6 | **fountain** | 6700 |
| 7 | **gray misty cube** | 6800 |
| 8 | **throne** | 6900 |
| 9 | **small box with 4 coloured lights** | 7100 |

**Stairs up** are not stored: at 6000 the game computes the room word of the same (x,y) on the level above (`X=CX:Y=CY:Z=CZ-1:GOSUB10010:J=FNS(H%)`); if that room's reduced feature is 4 (stairs down) the current room has stairs **up** (6015 → 6500), and this *overrides* the current room's own feature (except that a down-stair I=4 is also offered). On level 1, J is forced to 0 (6005).

### 1.4 Bounds, wrap, persistence
* Valid player coordinates 1..200 for X and Y (8150-8305 refuse moving N at CY=1, S at CY=200, W at CX=1, E at CX=200), levels 1..50 (6013, 6300, 6430, 6817). **No wrap-around** for walking; the map borders are forced walls via the x/y=1 and 201 rules. The renderer evaluates rooms X,Y in CX-1..CX+2 (0..202) (15004).
* Teleporter arithmetic does wrap by subtracting 200 (6415-6420).
* **No feature persistence at all**: the room word is recomputed from (x,y,z) every time; there is no visited/removed-feature memory. Altars, thrones, boxes, fountains, cubes are permanently re-usable. `EX%(0..100)` is DIMmed, saved and loaded (1282, 8663) but never otherwise used in V4.18 (vestigial).
* Start/rescue/recall position: CX=25, CY=13, CZ=1 (1600, 8917, 27410). Room (25,13,1) has feature 1 → stairs up to the starting Inn.
* Inn name (6103-6109): `Z$(FNRD(CX*CY),0)+" "+Z$(FNRD(CX+CY),1)+" "+Z$(FNRD(CX*3+CY*7),2)` with FNRD = mod 10 and the word lists in DATA 1173-1175. The start inn (25,13) is "WORTHY MEADE INN". Every feature-1 room on level 1 is an inn.
* Measured feature density on level 1 (MBF model, interior 200x200): 27.2 % of rooms have a feature: codes 1:1584, 2:1607, 3:1615, 4:1591, 5:1355, 6:1313, 7:532, 8:528, 9:763, none:29112. Up-wall values 0/1/2/3 roughly 25 % each.

### 1.5 Floating-point semantics (critical for walls) — verified against the real ROM
Commodore BASIC V2 uses 5-byte Microsoft Binary Format: exponent byte (excess 128, 0 ⇒ value 0), 32-bit mantissa (implicit top bit; stored top bit = sign). In the accumulator (FAC, $61-$66) there is an extra **8-bit rounding byte FACOV ($70)**. Behaviour that matters here (transcribed from ROM 901226-01 disassembly, `tools/dis.py`):

1. **FRMEVL rounds the left operand before pushing it** (`$AE43 JSR $BC1B`), so for every binary op `a OP b`: ARG = round32(a), FAC = b with its 40-bit precision. Variables/literals loaded into FAC have ov=0.
2. **FADD** ($B86A): aligns the smaller operand by right shift; the shifted operand keeps 8 extra bits in the ov byte, lower bits are **truncated**; 40-bit add/subtract; renormalise pulling ov bits back.
3. **FMULT** ($BA2B): shift-and-add over the 40 bits of FAC (ov byte first, then mantissa LSB→MSB) times ARG's 32-bit mantissa, keeping 40 result bits (truncation), then normalise.
4. **INT** ($BCCC): truncation of the 32-bit mantissa (ov ignored, cleared) — floor for positives.
5. **Assignment to an integer variable** (`H%=`, LET at $A9C4): **ROUND (to 32 bits) then truncate** (QINT). Assignment to a float var (`Q=`): ROUND then pack.
6. `AND`/`OR` convert both sides with AYINT (truncate, range −32768..32767).
7. Numeric literals (`10`, `15`, `1`) are parsed at run time by FIN; integer literals are exact.

Evaluation order of 10010 (what must be emulated):
```
t1 = X*XO ; t = r(t1) + (Y*YO) ; t = r(t) + (Z*ZO)              // r() = round to 32 bits
a = r(X)+XO ; b = r(Y)+YO ; ab = r(a)*b ; c = r(Z)+ZO ; abc = r(ab)*c
Q = store( r(t) + abc )                                          // rounded & packed
fr = Q - INT(Q)              (exact)
H% = trunc( round32( r(fr) * W0 ) )
10025: p = Q*10 ; fr2 = r(p) - INT(p_unrounded) ; v = r( r(fr2)*15 ) + 1 ; k = INT(v)
```
**Precision issues**:
* For deep levels Q is large (x≈y≈200, z=50 gives Q≈2·10⁶, i.e. ~21 integer bits) leaving only ~11 fraction bits in the 32-bit mantissa, yet `frac*4694` needs ~12.2 bits — the low bits of H% (the **walls**, bits 0-3) are dominated by MBF rounding/truncation behaviour.
* IEEE double reproduces features almost always but walls diverge: measured mismatch rate of a naive double implementation vs the exact MBF model: z=1 1.6 %, z=10 6.9 %, z=25 ≈16 %, z=50 29.3 % of rooms (features ≤0.3 %). See validation table in §1.6.
* `INT(Q*10)` is applied to the *unrounded* product while the minuend is the *rounded* product, so `fr2` can be exactly 1.0 → f = 16 → reduced to 7 (cube). It really happens: room (90,40,50) has h = 0x1080 on the real ROM (first discovered because my test POKE overflowed with ?ILLEGAL QUANTITY exactly there).

### 1.6 Ground truth & reference implementation (in scratch)
* `tools/c64run.py`: headless 6502 + genuine C64 BASIC/KERNAL ROM; runs tokenized BASIC (lines 10010-10040 copied verbatim) and dumps results. `truth/c64rom_z{1,2,10,25,50}_y*.bin` = raw H% (uint16 LE) for X=0..201 (inner), Y ranges in the filename, produced by the real interpreter.
* `tools/mbf.py`: bit-level Python model of the ROM float routines + `room(x,y,z)`. `tools/validate.py` compares it with ROM output.
* `model_maps/mbf_model_zNN.bin`: model output for all levels Z=0..50, X,Y=0..201 (uint16 LE, row-major, Y outer) — usable as C# unit-test vectors.
* Validation result: see `truth/validation.txt` (summary in §9).

---

## 2. Character creation, levels (verified-from-source)
* Stats (1500-1515): six stats in order STR, INT, WIS, CON, DEX, CHR (`S(0..5)`, names `S$` 1015), each `3d6` (`Q=Q+INT(RND(1)*6+1)` three times). Any key other than RETURN (including the input timeout) **re-rolls all six**.
* 1520-1600: `HP=CON`, `CH=HP`, GD=0, TG=0, LV=1, EX=0, all items I(1..10)=0, all spell timers SF(1..11)=0, SU=1 (max spell units), CS=1 (current), CX=25, CY=13, CZ=1, new box combination (19800).
* Name (16200): up to 30 chars, A-Z/0-9 only; empty → `DEMO` (demo: **no input timeouts** (16010, 40020) and cannot be saved (8600)).
* RND is seeded `RND(-TI*SC)` (1000; SC=49151).
* Level/XP (18000-18080): level up when `EX >= 1000*2^LV` (LV2 at 2000, LV3 at 4000, LV4 at 8000 …). On level-up: LV+1, `+INT(RND*CON+1)` to both CH and HP, `CS+=LV`, `SU+=LV` (new LV), and EX is capped to `1000*2^LV - 1` (only one level per check). Level-down when `EX < 1000*2^(LV-1)` (except LV1 with EX≥0): `CS-=LV, SU-=LV` (old LV), LV-1, lose `INT(RND*CON+1)` HP (CH and HP); death if CH≤0 or LV=0.
* Spell level available: `C <= INT(LV/3)+1` and `C<7` (21002) → L1 at LV1-2, L2 at 3-5, L3 at 6-8, L4 at 9-11, L5 at 12-14, L6 at 15+.

## 3. Encounters and monsters (verified-from-source)
Each turn (2000): every `SF(i)` (i=1..11) decremented by 1; ring of regeneration `CH+=I(6)` (capped).
* **Encounter chance** (3000): 30 % (`IF RND(1)>.3 THEN 4000`). With Invisibility active, 80 % of those are cancelled (3001).
* **Type** (3005): `M=INT(RND*20+1)` — uniform over all 20 types at every depth. With Fear (SF(7)) active types 1-4 are re-rolled.
* Elven cloak (3006): if `INT(RND*20+1) <= I(4)` → "YOU HAVE NOT BEEN NOTICED", RETURN to approach (fight) else the monster is skipped (20900-20915).
* **Level** (3007): `ML = INT( RND^1.5 * (CZ*2+2) + 1 )` → 1..2·CZ+2, skewed low.
* Light (SF(3)) re-rolls undead 20 % of the time (3008).
* Time stop (SF(9)) skips encounters with monsters `M<16 AND M<>11` (3011).
* **Monster HP** (3015): `MH = INT( RND^0.5 * ML * M + 1 )`.
* **XP for kill** (3223): `E = ML*M*10`.

| M | name | undead (20700) | special |
|---|---|---|---|
| 1 | GNOLL | | |
| 2 | KOBOLD | | |
| 3 | SKELETON | yes | |
| 4 | HOBBIT | | steals an item unless `RND < .05*CHR` (3040-3060): items 1-7 taken entirely, 8-10 one unit |
| 5 | ZOMBIE | yes | |
| 6 | ORC | | |
| 7 | FIGHTER | | |
| 8 | MUMMY | yes | |
| 9 | ELF | | heals you fully if `RND <= .04*CHR` (3020-3037) |
| 10 | GHOUL | yes | paralysis chance .5 per hit (3345) |
| 11 | DWARF | | ignores Time Stop |
| 12 | TROLL | | |
| 13 | WRAITH | yes | level drain .1 per hit (3330) |
| 14 | OGRE | | |
| 15 | MINOTAUR | | |
| 16 | GIANT | | |
| 17 | SPECTER | yes | drain .2 (3335); Prot/Evil −6 to its to-hit |
| 18 | VAMPIRE | yes | drain .3 and paralysis .3 (3340); Prot/Evil −6 |
| 19 | DEMON | | 40 %: sword MB=4, damage ×3; else whip MB=2, ×2 (3350-3360); Prot/Evil −6 |
| 20 | DRAGON | | 70 %: claws MB=5, ×2; 30 %: **breath** `INT(RND*20*ML+1)`, always hits, halved ("partially dodge") unless `RND < .05*CON` (3365-3398 — note: higher CON ⇒ more often FULL damage, as written). Gift (3070-3080): if `RND*30 <= CHR` gives random item 1-7 raised by `INT(RND*(ML-I(I))+1)` if `I(I)<ML` |

Additionally 5 % of all encounters (3017) trigger one of the three specials (elf heal / hobbit steal / dragon gift) **regardless of monster type**.

### Combat (3090-3400, 3700-3795)
* **Initiative** (3090): monster attacks first if `RND > .5 + DEX*.02`.
* Prompt (3100): `F`ight / `C`ast / `E`vade (also F3/F5/F7). Timeout = "WAIT" → monster attacks (3115 → 3300). Other keys "NOT AMUSED".
* **Player to-hit** (3200): `INT(RND*20) + LV + I(1) + STR/2 (+4 if Strength spell)`; hits if ≥ 10.
* **Player damage** (3205): `INT(RND*8 + RND*LV*2 + I(1) + 1)` (+5 with Strength spell).
* **Monster to-hit** (3305): `INT(RND*20) + ML - I(2) - I(3) + MB` (armor, shield; −6 vs M 17-19 with Prot/Evil); hits if ≥ 10. After a miss → player's turn.
* **Monster damage** (3315): `INT( (RND*8 + RND*ML*2 + 1) * DB )`. Then drain check (`RND <= DR` and no Prot/Evil ⇒ `EX=INT(EX/2)` (LV1 ⇒ EX=−1 ⇒ effectively death), level check), else paralysis check (`RND <= PA` ⇒ monster attacks again).
* **Ring of Protection I(7) is never referenced** in combat (manual §: "absorbs hit points equal to its plus") — no effect in V4.18 code (also absent in Atari listing). Shield has no effect on dragon breath despite the manual.
* **Evade** (3700): `Q=INT(RND*18+1)`, succeed if `Q < DEX + I(5)` (boots). Then a random direction is tried in a fixed cyclic order starting at `FNR(4)` (N,S,W,E) skipping solid walls (value 3); if all solid → rooted. Astral walk: any direction. You move (8120) and the monster is pushed onto the "following monsters" queue (30100).
* Kill (3220): XP gain, level check, then 50 % immediate treasure roll (3900→4002) else normal 20 % treasure roll (4000).
* **Following monsters queue** M%/L%/H%(1..20) (30000-30200): on every move the current monster (or 0) is pushed to the front (30100); on STAY the front is popped and, if non-zero, fought (8350, 30000). After treasure, 50 % chance the queue is popped too (4910). Inn, teleport, rescue clear it (30200). Queue is shown as `Q` glyphs in column 22 and saved with the character.

## 4. Timed input (verified-from-source; timing measured in emulation)
Lines 16000-16015: the input routine polls the keyboard (GET) 400 times; if no key arrives it returns a synthetic "no key" code (CHR$(166)) which each prompt maps to its default action. The DEMO character loops forever instead (no timeout).
* Not jiffy based: **400 polls of GET**. With animation (AN≠0, 40000-40025) it is `INT(400/AR)` outer loops × AR GETs + one animation step each (AR=1..10 by feature).
* Measured with the real program running under ROM emulation with 60 Hz IRQ: **≈3.42-3.66 s per timeout on NTSC (≈3.55-3.80 s PAL)**, excluding VIC bad-line DMA (add ~5 %), i.e. ≈3.6-4 s real. The manual says "about 5 seconds". (Note: speed depends on variable-table length and line-search; inferred estimate, not hardware-measured.)
* Timeout results: title/menu re-prompt, stats re-roll, dungeon "S"tay (8000), combat "WAIT" (monster attacks), stairway "S", box "STOP", throne "IGNORE", cube → random level, pit "NO".
* Delays: `GOSUB19000` waits 90 jiffies (1.5 s) via `TI` (19000-19003); 19500 = `FOR Q=1TO100`.
* RUN/STOP disabled (60000) and handled at 16005.

## 5. Spells (21000-27615) — cost = spell level in units (`CS=CS-C`, 21035), spent even if wasted
Index `SP$((C-1)*6+S)`. D=1 in melee, D=0 in the dungeon. "Combat" spells cast outside melee are wasted (21800). Durations add to the timer (reset to 0 first if negative) and are measured in turns (decrement per turn at 2000).

| Lvl | # | Spell | Effect (line) |
|---|---|---|---|
|1|1|MAGIC MISSLE|combat; dmg `INT(RND*8+5)` (22105)|
|1|2|SLEEP|combat; undead immune; fails if `INT(RND*20+1) > INT`; asleep → RET to kill: 80 % kill (22220), other key → escape (3710)|
|1|3|CURE LIGHT WOUNDS|+`INT(RND*8+1)` HP|
|1|4|LIGHT|SF(3) += `INT(RND*11+5)`|
|1|5|TURN UNDEAD|combat; non-undead "insulted"; success if `RND < .05*WIS + .05*LV - .05*ML` → monster flees, XP awarded with ML halved (22525)|
|1|6|PROT/EVIL|SF(4) += `INT(RND*11+5)`|
|2|7|WEB|combat; fails if `INT(RND^2*20+ML) > INT`; RET to kill succeeds if `RND - ML/20 > .2`|
|2|8|LEVITATE|SF(5) += `INT(RND*21+5)`|
|2|9|CAUSE LIGHT WNDS|combat; `INT(RND*12+3)`|
|2|10|DETECT TRAPS|SF(2) += `INT(RND*21+5)` (90 % warning, 4011/4115)|
|2|11|CHARM|combat; undead immune; fails if `INT(RND^2*20+1) > CHR`; kill as WEB|
|2|12|STRENGTH|SF(1) += `INT(RND*21+5)` (+4 hit, +5 dmg)|
|3|13|LIGHTNING BOLT|combat; `INT(RND*6*LV+15)`|
|3|14|CURE SERIOUS WNDS|+`INT(RND*24+1)`|
|3|15|CONTINUAL LIGHT|SF(3) += `INT(RND*31+5)`|
|3|16|INVISIBILITY|SF(6) += `INT(RND*21+5)`|
|3|17|HOLD MONSTER|combat; fails if `INT(RND*20+ML) > INT`; RET to kill: `IF RND(1)>.2+ML*.03 THEN kill` (24520), i.e. kill chance `0.8-0.03*ML`|
|3|18|PHANTSML FORCES|combat; kill if `INT(RND*22+ML) < INT`|
|4|19|PASS WALL|not in melee; move one square in chosen direction ignoring walls (bounds kept)|
|4|20|FIREBALL|combat; `INT(RND*12*LV+15)`|
|4|21|CAUSE SERIOUS WND|combat; `INT(RND*32+10)`|
|4|22|FLESH TO STONE|combat; 60 % kill|
|4|23|FEAR|SF(7) += `INT(RND*30+5)` (no monsters 1-4)|
|4|24|FINGER OF DEATH|combat; undead "already dead"; kill if `RND > .3 + ML*.04 - LV*.03`|
|5|25|TELEPORT|not in melee; enter +N/−S, +E/−W, +UP/−DOWN; allowed if `SQR(dx²+dy²+(dz*5)²)-.1 <= LV*5`; must land in 1..200, ≤50, ≥1 (26105-26140)|
|5|26|ASTRAL WALK|SF(8) += `INT(RND*16+5)` (walk through walls; evade any direction)|
|5|27|POWER WORD KILL|combat; non-undead 80 % kill; undead immune|
|5|28|ICE STORM|combat; 60 damage|
|5|29|WALL OF FIRE|combat; 60 % monster gone (no XP), else it walks through taking `INT(RND*12+8)`|
|5|30|PLAGUE|combat; undead immune; immune if `RND > WIS*.05`; then kill if `RND*2 < CON*.03+1`, otherwise **backfires: you die** (26625)|
|6|31|TIME STOP|SF(9) += `FNR(20)+4`; in melee you leave the fight|
|6|32|RAISE DEAD|SF(10) += `INT(RND*40+5)`; on death (9000→27205): CON−1, SF(10)=0, revive with full HP if `RND <= CON*.06`|
|6|33|HOLY SYMBOL|combat; 90 % kill|
|6|34|WORD OF RECALL|not in melee; to 25,13,1 but **lose all items and carried gold** (27405)|
|6|35|RESTORATION|CH=HP|
|6|36|PRISMATIC WALL|combat; monster gone (no XP)|

Status timer abbreviations (15010): SF(1) STRG, 2 DTRP, 3 LGHT, 4 PROT, 5 LEVT, 6 INVS, 7 FEAR, 8 ASTW, 9 TMST, 10 RSED, 11 DRNK.

## 6. Treasure and items (4000-4910, 1105-1150)
* After a quiet turn: 20 % treasure roll (4000). After a kill: 50 % straight to treasure, else the 20 % roll.
* `L` here is CZ normally, but **ML after a fight** (3015 sets L=ML) — treasure scales with monster level after combat.
* Trap flag T: 15 % (4002). Detect Traps warns 90 %.
* 30 %: coins/valuables (4007): type `I=INT(RND*5)`: REFUSE, SILVER, GOLD, GEMS, JEWELS; trapped: `INT(RND*3*L+1)` dmg; value `J=INT(RND*I*L*200+1)` gold (refuse = 1 gold, and 20 % chance of an item after refuse).
* 70 %: 50 % an item directly (4200); 50 % a **chest** (4105): trapped → explodes `INT(RND*10*CZ+1)`; 10 % cobwebs; else `INT(RND*1000*L^2+1)` gold, then 50 % an item, each item followed by 50 % another (C=1 chain, 4240).
* **Items** (4200): `I=INT(RND*10+1)`, plus `J=INT(RND^.5*(L+1)+1)`. Items 1-7 (SWORD, ARMOR, SHIELD, ELVEN CLOAK, ELVEN BOOTS, RING OF REGENERATION, RING OF PROTECTION) are **replaced** by the new plus (even if lower: `I(I)=J`); 8-10 (SCROLL OF RESCUE, POTION OF HEALING, POTION OF STRENGTH) are counted. A trapped sword is "HOSTILE": `INT(RND*5+1)` damage, not taken. No inventory limit, one of each 1-7.
* Item effects: sword +hit/+dmg; armor & shield −monster to-hit; cloak surprise (1/20 per plus); boots +DEX for evade and pits; regen +plus HP/turn; protection none (see §3); scroll of rescue (F3) → 25,13,1 and **carried gold set to 0** (8917-8918); healing potion (F5) `+FNR(20)`; strength potion (F7) SF(1) += `10+INT(RND*20)`.

## 7. Room features (6100-7185)
* **Inn** (6103-6145): gold cashed: `EX=EX+GD`, `TG=TG+GD`, GD=0 (gold→XP 1:1; TG "in the safe" is only displayed/saved); level check (one level max); monster queue cleared; full HP; all SF timers cleared; CS=SU. Then RETURN = back to level 1 at the same x,y (CZ=1), F1 = save.
* **Elevator** (feature 1, levels ≥2): automatically CZ−1 ("YOU FEEL HEAVY").
* **Pit** (6300): levitating → choice; else if `INT(RND*20) > DEX+I(5)` you fall: `INT(RND*3*6+1)` (1-18) damage and CZ+1; otherwise asked whether to descend. On level 50 a pit = elevator up.
* **Teleporter** (6400-6485), exactly:
```
repeat:
  if (cx+cy) is even: cz -= 1; and if bit 1 of (cx+cy) is set: cz += 2     (6405, nested)
  cx = cx + cz*8 + cy*13 ;  cy = cy + cz*6 + cx*17   (uses the NEW cx)     (6410)
  while cx > 200: cx -= 200 ; while cy > 200: cy -= 200                    (6415-6420)
  if cz == 0: cz = 1 ; if cz > 50: cz = 50                                 (6425-6430)
while RND > 0.8   (20 % chance to chain)                                   (6435)
```
  (the second IF is on the THEN-branch of the first — only applies when CX+CY is even; mgritter's C++ port gets this wrong.) Monster queue cleared.
* **Stairs** (6500): U/7 up, D/1 down, S/5 stay (timeout = stay). Up from level 1 → Inn.
* **Altar** (6600): not worshipping: 30 % nothing, 70 % "DIRTY PAGAN TRASH!" → random **undead** attacks (normal ML). Worship → donate: amount must be ≤ GD and ≥ `50*CZ` else pagan attack. Then `GD-=C`; with probability `C/(GD_before)` (= `C/(GD+C)`) "YOU'VE BEEN HEARD": random SF(1..7) += `INT(RND*100*C/(GD+C)+1)`, else thanks.
* **Fountain** (6700-6786): colour C=1..5 (WHITE, GREEN, CLEAR, RED, BLACK). 60 % (6715-6745): with prob `1-.15C` heal `INT(RND*3*CZ+1)`; otherwise (prob `.15C`) a second roll: prob `.15C` poison `INT(RND*3*CZ+1)` damage, else XP change ±`INT(RND*500*CZ+1)` (50/50 gain/lose, 20100). 40 %: 60 % "refreshed"; else 50 % drunk SF(11) += 1..16 (random moves while >0), 25 % +`INT(RND*4*CZ+1)` current spell units (CS can exceed SU), 25 % **dispossessed** (all items lost).
* **Gray misty cube** (6800): RET to enter; 20 % random level 1..50; else type 1..50 (timeout → random; invalid → re-ask). X,Y unchanged.
* **Throne** (6900-7035): P/S/R/I. Each of P,S,R: 30 % "THE <random monster> KING RETURNS" (ML = `INT(RND*CZ*5)+5`). Pry: 40 % of rest jewels `INT(RND*1000*CZ+1)` else nothing. Sit: of rest 40 % nothing, 60 %×60 % teleport (same as 6403), else gong: 50 % `EX=INT(EX/2)`, 50 % if LV≤CZ `EX=1000*2^LV` (+1 level) else nothing. Read: if `RND < INT*.05` random stat ±1 (bounded 3..18) else nothing.
* **Box** (7100-7185): combination B(1..4) ∈ {1 RED,2 YELLOW,3 GREEN,4 BLUE}, per character (created at start and re-rolled after each opening, saved). Push R/G/Y/B in sequence; wrong button → `INT(RND*2*CZ+1)` damage and the attempt ends; all four right → `INT(RND*20000*CZ^2+1)` gold.

## 8. Commands, saving, death
* Dungeon keys (CM$ 1015, 8100-8120): W/8 north, X/2 south, A/4 west, D/6 east, S/5 stay, H help, Q quit, P replot, C cast, F1 save, F3 scroll of rescue, F5 healing potion, F7 strength potion (also CRSR-right=help, DEL=quit, CRSR-down=replot, "."=cast as written). Timeout = stay. Drunk: movement/stay keys replaced by `FNR(5)`.
* **Saving** (8600-8665): F1 anywhere in the dungeon or in the Inn; writes `"<NAME>.PLR"` SEQ: name, version 4.18, S(0..5), LV, GD, TG, EX, CH, HP, CX, CY, CZ, SU, CS, (I(i),SF(i)) i=1..10, SF(11), (M%,L%,H%) ×20, B(1..4), EX%(0..100); then `RUN` (back to title). Loading (1220-1300): CZ=0 → 1.
* **Quit** (8700) = death path. **Death** (9000): Raise Dead check, then "TRY AGAIN?" → `CLR:GOTO1000`; the save file is untouched (dead characters can be re-loaded).

## 9. Validation of the map model against the real ROM
Ground truth produced by running the original lines 10010-10040 on the genuine C64 BASIC ROM in a
6502 emulator. At the time of writing: the bit-level model matched the ROM on all 615 rooms of the start
area (level 1, y 0-14, x 0-40) and on level 50 up to the (90,40) edge case. Full-level ROM runs for
z = 1, 2, 10, 25, 50 were still in progress; see docs/research/dungeon-generation.md for status.
The C# port (`RoomWordGenerator`) is tested against the ROM start-area output and against the model's
output for every room of all 51 levels (0-50).

## 10. Cross-check with the Atari version (Dan Boris listing)
* Same combat/encounter/level formulas (ATARI: 3000-3007, 3090, 3200-3205, 3305-3315, 3700-3705, 4040, 4140, 6305, 6405-6410, 18000).
* Atari 3000 skips *all* encounters under Time Stop (`OR SF(NIN)>ZERO`); C64 instead only skips M<16, M≠11 (3011).
* **Atari map generator is different**: `Q=X*XO+Y*YO+Z*ZO+X*YO+Y*ZO+Z*XO : HI=Q AND 255 (USR) : Q=X*Y*ZO+Y*Z*XO+Z*X*YO : IF Q AND 3 THEN 10030 : Q=(Q/4) AND 15 : IF Q>9 THEN Q=Q-9 : HI=HI+Q*256` (ATARI 10010-10024) — same constants, different formula, so Atari dungeons differ from C64.
* Remastered v1.1 (gondur) changes minimum monster level, hobbit steal and elven cloak — not original.

## 11. Evidence/confidence table
| Mechanic | Evidence | Platform | Confidence |
|---|---|---|---|
| Map constants & formula | 1103, 10010-10040 | C64 | verified-from-source + ROM-executed |
| Wall/door semantics (0/1 open, 2 door, 3 wall) | 1005, 8150-8305, 10050-10085 | C64 | verified-from-source |
| Feature codes & reduction | 6000-6020, 6100-7185 | C64 | verified-from-source |
| Stairs-up derived from level above | 6000-6015 | C64 | verified-from-source |
| Bounds / no wrap / no persistence | 8150-8305, 10030-10035, whole listing | C64 | verified-from-source |
| MBF float details | ROM disassembly + emulation | C64 ROM 901226-01 | verified (model matches ROM, §9) |
| Character creation | 1500-1630 | C64 | verified-from-source |
| Level/XP table | 18000-18080 | C64 | verified-from-source |
| Monsters, specials, combat | 1020-1030, 3000-3400, 3700-3795, 20700 | C64 (Atari agrees) | verified-from-source |
| Input timeout (400 GETs) | 16000-16015, 40000-40025 | C64 | verified-from-source; seconds = inferred (emulated ≈3.4-3.8 s) |
| Spells | 1162-1172, 21000-27615 | C64 | verified-from-source |
| Treasure/items | 4000-4910, 8910-8960 | C64 | verified-from-source |
| Features (altar, fountain, cube, throne, box, pit, teleporter) | 6200-7185 | C64 | verified-from-source |
| Inn | 6103-6145 | C64 | verified-from-source |
| Save/death | 1220-1300, 8600-8710, 9000-9020 | C64 | verified-from-source |
| Ring of protection effect | absent in code | C64 + Atari | verified absent (manual disagrees) |
