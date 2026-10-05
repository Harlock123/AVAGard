# TELENGARD (Daniel Lawrence / Avalon Hill Microcomputer Games, 1982): history, versions, manual, atmosphere

Research date: 2026-10-05. Scope: history, versions, manual content, reception, atmosphere, rights. Detailed formulas and source code are covered by another agent; this report records only mechanics the manual or reviews state.

Conventions:
- **[MANUAL]** means read directly in the Avalon Hill *Telengard Rules Manual* (2nd edition, 3/83, © 1982) scan.
- **[PRIMARY-ARTICLE]** means read directly in a period magazine scan or OCR text (1982–1993), or in Lawrence's own words.
- **[SECONDARY]** means a later summary (Wikipedia, MobyGames, blogs, fan comments).
- Manual prose is paraphrased. Quotes are short and used only as evidence.

---

## (a) SOURCE LEDGER

All accessed 2026-10-05. "Inspected" means I actually read the text listed. Nothing in this report comes from memory alone.

| # | Title | Author/Org | URL | Type | Platform/version | What I inspected | Claims supported | Confidence | Licensing notes |
|---|---|---|---|---|---|---|---|---|---|
| S1 | *Telengard Rules Manual* (24 pp., "2nd Edition", "3/83", © 1982) | The Avalon Hill Game Co. / Microcomputer Games div. | https://www.mocagh.org/ah/telengard-manual.pdf | Primary: original manual scan (PDF) | Multi-system (Apple, Atari 800, C64, PET, TRS-80) | Full text via pdftotext, all 24 pages. Rendered pp. 7–8 as images to check the key table | All manual sections in §B2 | High (OCR checked against page images for the command keys) | © Avalon Hill 1982, now Hasbro. Paraphrased here, short quotes only |
| S2 | Commodore C64 Manual: Telengard (1983) | Avalon Hill (scan on Internet Archive) | https://archive.org/details/Telengard_1983_Avalon_Hill | Primary: manual scan (djvu OCR text) | C64 copy of the same multi-system manual, 2nd ed. | OCR text, grepped for edition and C64 keys | Same text as S1. C64 function keys on the back-cover card | Medium-high (OCR noisy) | Same as S1 |
| S3 | TRS-80 Manual: Telengard | Avalon Hill (IA scan) | https://archive.org/details/Telengard_19xx_Avalon_Hill | Primary: manual scan (OCR) | TRS-80 copy, 2nd ed. 3/83 | OCR grep | Same text. C64 F-keys confirmed (F1/F7/F5/F3) | Medium-high | Same |
| S4 | Atari Game Manual: Telengard | IA scan | https://archive.org/details/agm_Telengard | Primary: manual scan (OCR) | Atari copy | OCR grep only | Same text and tables. Edition line OCRs as "3nd edition" (probably 2nd or 3rd, unclear) | Medium | Same |
| S5 | Telengard Rules Addendum, IBM PC & PCjr versions | Avalon Hill, © 1985 (form "Z-3658 8/85") | https://archive.org/details/TelengardRulesAddendumIbmPcPcjrVersions1985 | Primary: addendum scan (OCR, 1 page) | IBM PC / PCjr (1985) | Full OCR text | PC loading (type TELEN), lowercase "sv" rule for recallable dead characters | High | © AH 1985 |
| S6 | Telengard poster (22x17") | Avalon Hill 1982 (IA) | https://archive.org/details/telengardposteravalonhill1982 | Primary: item metadata only | DOS and C64 boxes per uploader | Metadata description only. Poster OCR was empty | A poster came in the box | Medium | — |
| S7 | "Telengard" page, aquest.com | Daniel M. Lawrence | http://www.aquest.com/telen.htm (read via web.archive.org 2014 snapshot) | Primary: author's own statement | All | Full page text | DND origins, PET 1978, 8K→32K, ports, IBM-PC C rewrite with lost source, free download of PC binaries | High that it is Lawrence's statement. Dates inside it are inconsistent (see §C) | Lawrence offered the PC binaries for download. No licence text |
| S8 | "Author" page, aquest.com | Daniel M. Lawrence (later edited by someone else) | http://www.aquest.com/author.htm (archive 2014) | Primary: autobiography | — | Full page text | Born 1958-04-22, died 2010-06-07, Purdue 1976–80, Lafayette IN, MicroEMACS, Adventure Quest, no AH employment | High. One later-added line says "passing in 2014", which conflicts with the date given on the same page | — |
| S9 | "Interview with Daniel M. Lawrence, CRPG Pioneer and Author of Telengard" | Matt Barton, *Armchair Arcade*, 2007-06-22 | http://armchairarcade.com/neo/node/1366 (archive 2014) | Primary: interview | — | Full Q&A text | DND 1976. Telengard on PET 1978. Ported to Atari 800, Apple II+ and TRS-80 before AH licensing. Seeded procedural dungeon. Reason for real-time play. No story because of 8K. AH approached him at a convention. He was never an AH employee | High | Short quotes only |
| S10 | "Review: Telengard" | Tony Roberts, *COMPUTE!* #40, Sept 1983, p.176 | https://www.atarimagazines.com/compute/issue40/review_telengard.php | Primary: contemporary review (transcribed) | Atari, Apple, PET, C64 (C64 preferred) | Full text | ~5 s timer, 20 monsters, 36 spells, stats on the right side of the screen, inns pause the game, 2–3 key buffer, $23 tape / $28 disk | High | — |
| S11 | "Route 80: The Road to TRS-80 Gaming" (Telengard review) | Dick McGrath, *Computer Gaming World* 3.3, May–June 1983, pp.35, 43 | https://archive.org/details/Computer_Gaming_World_Issue_3.3 (also cgwmuseum.org) | Primary: review (OCR) | TRS-80 ("System: Many") | Full review text plus an AH ad in the same issue | 3d6 stats, SV naming, hybrid fighter/mage, no shop, 50 levels, line-of-sight display, unpredictable features, unexplained high-level spells, BASIC is modifiable | High | — |
| S12 | AH advertisement in CGW 3.3 | Avalon Hill | same as S11 | Primary: ad | C64 plus other systems | Ad text | Quotes Playboy (Feb 1983). "50-level dungeon", "sprite graphics and sound" on C64, platform/RAM/price list | High | — |
| S13 | "The Commodore Key" (C-64 section) | Edward Curtis, CGW 3.4 (1983) | https://archive.org/details/Computer_Gaming_World_Issue_3.4 | Primary (OCR) | C64 | Telengard paragraph | Play is the same as on other machines. Graphics on the C64 are "much improved", mostly through sprites | High | — |
| S14 | AH ad "Telengard: How low can you go?" | Avalon Hill, CGW 3.5 (1983) | https://archive.org/details/Computer_Gaming_World_Issue_3.5 | Primary: ad (OCR) | C64, Atari, TRS-80, PET, Apple | Ad text | 50 levels, "hundreds of" chambers per level, cassette $23 / 48K disk $28, RAM requirements | High | — |
| S15 | "The Commodore Key" (RPG roundup) | Roy Wagner, CGW 4.3, June 1984, p.38 | https://archive.org/details/Computer_Gaming_World_Issue_4.3 | Primary (OCR) | C64 | Telengard paragraph | "50 levels, each with 40,000 rooms (so they say)", "ten different monsters" (an error), wall drawing is slow, very fast-paced | High that this is what he wrote. Some of its claims are wrong | — |
| S16 | Capsule review, *The Space Gamer* #58 (Dec 1982) | Norman Banduch, Steve Jackson Games | https://archive.org/details/Space_Gamer_58 | Primary (OCR) | 48K Apple II, TRS-80 I/III disk, $28 | Full capsule plus a mail-order ad | Earliest dated review found. Flat graphics (x, #, $), slow redraw, key errors, no pause, "curiously addictive" | High | — |
| S17 | Game review, *Different Worlds* #40 (Jul–Aug 1985) | Troy Christensen | https://archive.org/details/different-worlds-40 | Primary (OCR) | C64 (compared with Apple) | Full review | C64 has red-brick walls, yellow gold "globs" and detailed monsters. Box held a 24-pp manual, a poster and a disk. Sleuth order numbers. Very large plusses on items | High | — |
| S18 | "An avalanche from Avalon Hill" | Brian J. Murphy, *Creative Computing* 9:9, Sept 1983, p.192 | https://www.atarimagazines.com/creative/v9n9/192_An_avalanche_from_Avalon_.php | Primary (transcribed) | Text version (X, letter codes, #). Platform not named | Telengard section | Credits "Dan Lawrence and Mike O'Brien". Stats flash about every 3 s. Start below the "Worthy Meade Inn". Status in the upper-right quarter. 1 spell unit at start. Compared with Wizardry | High that it is what he wrote. The O'Brien credit is uncorroborated | — |
| S19 | "Computer Role-playing Games: Now and Beyond" | Scorpia, CGW #38, Jun–Jul 1987, p.28 | https://archive.org/details/Computer_Gaming_World_Issue_38 | Primary (OCR) | CompuServe "Castle Telengard" | Passage | Editor's note: AH's Telengard was a "later commercial product based on the original public domain program" | Medium. The editor's note conflicts with Lawrence's account | — |
| S20 | "Public Domain Computer Role-Playing Games" | Gerald Graef, CGW #92, Mar 1992, p.64 | https://archive.org/details/Computer_Gaming_World_Issue_92 | Primary (OCR) | — | Opening paragraph | Telengard and Apshai were "quickly overshadowed" by Wizardry and Ultima | High | — |
| S21 | "Scorpia's Magic Scroll of Games" | Scorpia, CGW #111, Oct 1993 | https://archive.org/details/Computer_Gaming_World_Issue_111 | Primary (OCR) | Apple | Telengard entry | D&D influences (3–18, gold counts only after extraction, one level at a time, spell names). "Pretty dated" today, but "hot stuff" back then | High | — |
| S22 | "Communications: Adventures On-Line" | *Antic* 3:7, Nov 1984 | https://www.atarimagazines.com/v3n7/online.html | Primary | CompuServe | Passage | "Castle Telengard" on CompuServe, GO GAM-320 | High | — |
| S23 | "Online with START: Summer Fun" | Gregg Pearlman, *START* 3:1, Summer 1988 | https://www.atarimagazines.com/startv3n1/onlinewithstart.html | Primary | CompuServe | Passage | Castle Telengard still listed under GO FRPGAM in 1988 | High | — |
| S24 | Wikipedia: "Telengard" | Wikipedia contributors | https://en.wikipedia.org/wiki/Telengard | Secondary | All | Full article and reference list (via WebFetch summary) | Overview, list of ports, citations to S7–S21 | Medium (I verified most of its citations myself) | CC BY-SA |
| S25 | Wikipedia: "DND (video game)" | Wikipedia | https://en.wikipedia.org/wiki/DND_(video_game) | Secondary | DND | WebFetch summary | DND 1977 on PDP-10. DEC ordered DND removed in Sept 1983 to avoid litigation from Telengard's publisher. R.O. Software PC port 1984 | Medium (I did not trace its sources) | CC BY-SA |
| S26 | Wikipedia: "Avalon Hill" | Wikipedia | https://en.wikipedia.org/wiki/Avalon_Hill | Secondary | — | WebFetch summary | Hasbro bought AH on 1998-08-04 for $6M (trademarks, copyrights, Avalon Hill Software). Placed under Hasbro Interactive. AH excluded from the 2001 Infogrames sale. Moved to WotC in 2004, then Hasbro Gaming in 2021 | Medium | CC BY-SA |
| S27 | The Unofficial DND Home Page | Justin (site maintainer), c.2002 | http://dnd.lunaticsworld.com/ (archive 2002-11-06) | Secondary: fan history | DND, Telengard, Castle Telengard | Full text | DND lineage. Telengard was algorithmic, one class. Port list incl. Ohio Scientific and CPC. AH "does not wish Telengard to be distributed online". DND made freely distributable by grant of the author. R.O. Software dispute | Medium | DND free in unmodified form "by grant of the author" (per the site). Telengard not free |
| S28 | MobyGames: Telengard and its Releases page | MobyGames | https://www.mobygames.com/game/3392/telengard/ (archive 2025-04-27) and /releases/ (archive 2026-02-26) | Secondary database | All | Overview, credits, trivia, one user review, release list | Platform/date list incl. FM-7, PC-88, PC-98 (Kiya) and a UK C64 release (Eclipse 1984). Credits copied from the manual. Its trivia line is wrong (see §C) | Medium | — |
| S29 | Telengard review (C64) | Jonathan Quick, Lemon64, 2002-03-15, plus user comments | https://www.lemon64.com/review/telengard/171 (archive 2024) | Secondary: retrospective and fan comments | C64 | Full review and comments | C64 sprites and custom charset. Throne has music. Organ music, wind and bell at the title. Slow BASIC | Low-medium (memories) | — |
| S30 | Lemon64 game page | Lemon64 | https://www.lemon64.com/game/telengard (archive 2024) | Secondary | C64 | Page text | Released 1983. Lists joystick port 2 as control (questionable, since the manual describes keyboard only) | Low-medium | — |
| S31 | "Telengard" (Atari 8-bit tech page) | Dan Boris | http://atarihq.com/danb/Telengard.shtml | Secondary: technical analysis | Atari 8-bit disk | Full page | 50 levels of 200x200. Disk file list (DSKTEL.BAS, TELINI.DAT, TELEN.SET...). Room-code meanings 1–9 (Inn, Pit, Teleporter, Stairway, Altar, Fountain, Cube, Throne, Box) | High (derived from the code) | Formulas are left to the other agent |
| S32 | "Game 6: Telengard (1982)" | The CRPG Addict, March 2010 | http://crpgaddict.blogspot.com/2010/03/game-6-telengard.html | Secondary: play-through | DOS 1985 | WebFetch summary | 5-second auto-action, no pause, save deleted on death, suicide key | Medium | — |
| S33 | "Telengard (Commodore PET, 1981), Preserved" | Archeogaming blog, Jan 2013 | http://archeogaming.blogspot.com/2013/01/telengard-commodore-pet-1981-preserved.html | Secondary | PET cassette (side 2 of the boxed tape) | WebFetch summary | The PET version was dumped from the boxed C64/PET tape. Uses basic symbols. Title says "1981" without evidence | Medium | — |
| S34 | "Avalon Hill's Telengard for Z-90 or H/Z-100 with CP/M-85 and MBASIC" | Bill Loguidice, *Armchair Arcade*, 2012-07-25 | http://www.armchairarcade.com/neo/node/4827 (archive 2016) | Secondary: hands-on with a sealed copy | Heath/Zenith CP/M-85 | Full text | Official AH CP/M port exists. Needs MBASIC. Files TELEN/GARD. 5-second auto-move | High | — |
| S35 | Daniel M. Lawrence obituary/tribute | le_trombone (LiveJournal), 2010-06-09. Search snippet of the Hartzler-Clapper funeral home obituary | https://le-trombone.livejournal.com/182656.html ; https://www.clapperfuneralservices.com/guestbook/5182630 | Secondary | — | LiveJournal via WebFetch. Funeral-home text only as a search-result snippet (not fetched) | Died 2010-06-07, age 52, Lafayette. Known for MicroEMACS, Telengard and Adventure Quest. Telengard "most-copied" | Medium-high | — |
| S36 | "Telengod" article | Ward Shrake, Orphaned Computers & Game Systems | https://orphanedgames.com/articles/Telengod/Telengod.html | Secondary | C64 | WebFetch summary | 1985 character-editor utility for C64 save files. Three C64 dumps (A–C) | Low-medium (the summary was thin) | — |
| S37 | 10 Minute Amiga Retro Cast blog, Oct 2020 | 10marc.com | http://www.10marc.com/2020/10/telengard-for-cbm-pet-commodore-64-and.html | Secondary | PET/C64/VIC-20 | WebFetch summary | VIC-20 version is a fan adaptation by "Neo-Rio" | Medium | — |
| S38 | Grognardia "Retrospective: Telengard" | James Maliszewski, Nov 2021 | http://grognardia.blogspot.com/2021/11/retrospective-telengard.html | Secondary: memoir | Atari 400 | WebFetch summary | Atmosphere and reception. Says "36 monsters", which is wrong (it's 36 spells) | Low-medium | — |
| S39 | Gamasutra / Game Developer, "History of CRPGs Pt 1" | Matt Barton, 2007-02-23 | https://www.gamedeveloper.com/design/the-history-of-computer-role-playing-games-part-1-the-early-years-1980-1983- | Secondary | — | WebFetch summary | Says "published 1982 for the PET". 2M rooms, 20 monsters, 36 spells. "Pure dungeon crawler" | Medium | — |

Sources I tried that failed or were thin: lemon64.com and mobygames.com return a 403 or Cloudflare page to live fetches (I used archived copies). c64-wiki.com has no Telengard page. A WebFetch of retroreviews.org failed on its TLS certificate. The aquest.com and DND-page links (angelfire tribute, thelogbook) were not retrievable. Antic, Softline, Zzap!64 and Byte reviews of Telengard: none found (see §D).

---

## (b) FINDINGS BY TOPIC

### B1. Premise, setting, atmosphere

- **[MANUAL] S1 §1.0–1.1:** The manual opens with a short second-person welcome. Many adventurers have gone down into "the harsh dungeon" and never returned. A few came back from "the cruel labyrinth" with tales of horror and despair, and also of great treasure and magic. The game is a "computerized fantasy role-playing game": you play one character who repeatedly descends into Telengard, fights monsters, gathers gold and enchanted items, gets stronger and learns new spells. There is no further story, no named villain and no stated goal beyond surviving and growing stronger. The manual's tone is jokey ("Suicide is painless in TELENGARD"; "your shopping bag is big"; "Never fight a Dragon!"; "Heroes die young.").
- **[MANUAL] S1 §6.4:** Inns sit above the dungeon (on the surface), above every "up" circular stairway on dungeon level 1. One joke: the "Bank Accounts" of all Inns are linked by computer.
- **[PRIMARY-ARTICLE] S11 (CGW 1983):** McGrath opens with the "ominous grey ruins of an ancient castle" near the town of Telengard. This is his own color, not the manual's.
- **[PRIMARY-ARTICLE] S18 (Creative Computing 1983):** The hero starts at the foot of a staircase up to the **"Worthy Meade Inn"**. That inn name is not in the manual. It presumably appears on screen in the reviewed version (unverified).
- **[PRIMARY-ARTICLE] S12/S14 (AH ads):** Ad copy reuses the manual's intro ("Will you return from the cruel labyrinth…"). It calls the game a "realtime fantasy role-playing game" with "a shifting collection of hideous monsters with unpredictable behavior patterns". The C64 ad mentions "sprite graphics and sound".
- **[SECONDARY] S29 (Lemon64 comments, low reliability):** C64 players remember eerie organ music, a desolate wind and a gothic bell that shook the screen at the start. The jewelled throne plays music. Colors sometimes cycle. Loading music on tape. Tape loading was very slow.
- **[PRIMARY] S9 (Lawrence):** There was no story because "In 8k of memory there was no room for a story."

### B2. What the MANUAL says (S1; 2nd ed. 3/83; same text in S2–S4)

**Front matter.** It is titled "TELENGARD Rules Manual". The cover line reads "TELENGARD IS AVALON HILL'S TRADEMARK NAME FOR ITS MICROCOMPUTER [game]". The credit is "GAME DESIGN: Orion Software". **Daniel Lawrence is not named anywhere in the manual.** Fourteen playtesters are listed. The copyright line reads "© The Avalon Hill Game Company 1982 … Computer Program and Audio Visual Display Copyrighted". There is an unauthorized-reproduction warning, and the STUMP joke under "Questions on Play". Contents: 1 Introduction, 2 Attributes, 3 Commands, 4 Spells, 5 Monsters, 6 Features, 7 Treasures, 8 Hints, 9 Multi-player suggestions, 10–11 Loading.

**Start-up and character creation (§1.2–1.22):**
- The opening menu offers (S)tart a new character or (R)ead in an old one.
- On a new character, the screen cycles through sets of six attributes, each 3–18. A new set appears "periodically". Pressing any letter key fast-forwards, and RETURN/ENTER accepts the set. There is no point-buy.
- Names can be up to 7 characters. Longer names are cut off.
- Typing **DEMO** as the name turns off real time. The manual recommends this for learning (§8.1).
- Reading in an old character: you type the name. The character reappears where it was saved. A character saved in a surface Inn is placed directly below that Inn.
- A **dead** character can be recalled only if it was saved at least once **and its name begins with "SV"**. It comes back with the stats from its last save. (S5, IBM 1985: the "sv" must be lowercase, or the file is erased on recall.)
- The player is shown as **X** on TRS-80, Apple and PET, and as a graphic symbol on Atari and C64.

**Real time (§1.4, 1.51, 7.2):** You have "about 5 seconds" to key in a command "before the computer will do one for you". The default is STAY. Treasure pickup also has a 5-second window, after which "LEAVE IT" appears. Pressing H repeatedly can stall the game in the ACTION phase. During spellcasting the monster stays "mesmerized" as long as you keep striking keys after choosing the level (§4.4).

**Turn structure (§1.5):** Each Player Turn has an **ACTION Phase**, marked by a right-pointing cursor arrow below the dungeon graphic, then an **ENCOUNTER Phase** if the square holds a monster, feature or treasure. You can have an encounter without moving.

**Attributes (§2.1–2.17):** The six attributes range 3–18, and higher is better.
- STR: mainly success in combat.
- INT: important for casting spells.
- WIS: important for healing spells and spells against Undead.
- CON: "very important". It sets starting HP.
- DEX: mainly running away (evasion) and avoiding pitfalls.
- CHR: mainly how certain creatures react.
- Experience Level (LVL) is not an attribute and is "virtually unlimited".

**Hit points (§2.2):**
- A level 1 character's HP equals CON.
- HP are shown as remaining/total.
- A night at a surface Inn restores all HP.
- A Ring of Regeneration restores HP equal to its plus each turn in the dungeon.
- Characters gain HP with each level.

**Experience (§2.3):**
- XP comes from each gold piece brought **out** of the dungeon and from each monster killed or "turned".
- Fountains or thrones can sometimes grant XP, at risk.
- You advance **only one level at a time**, however much XP you have.
- XP table: L2 2,000; L3 4,000; L4 8,000; L5 16,000; L6 32,000; L7 64,000; L8 128,000; L9 256,000; L10 512,000; doubling after that.
- Monster XP formula (§5.1): (monster level) × (monster rank × 10).

**Spell Units (§2.4):**
- A spell costs as many SU as its level (level 1 costs 1, level 3 costs 3, and so on).
- SU are shown as remaining/total. You gain more SU with levels.
- SU are fully restored at an Inn. Fountains can also give SU, at risk.

**Gold (§6.4, 7.4):**
- Gold-value treasure (silver, gold, gems, jewels) earns XP equal to its value, but **only when deposited at an Inn**. Deposits happen automatically on entering an Inn.
- Gold is found in rubbish piles, treasure chests or lying about.
- A Scroll of Rescue forces you to drop all gold-value treasure.

**Inns (§6.4):**
- They are on the surface, above every up-stairway on level 1.
- Staying overnight restores all HP and SU and deposits all gold.
- A character "may advance to a higher Experience Level" there (the manual's wording).
- You re-enter the dungeon with RETURN/ENTER.
- Store (save) is a command (§3.1). The manual does not say outright that saving is only possible at an Inn. Reviews (S10, S15) describe saving at Inns. **[Gap: whether Store works anywhere is not settled by the manual. S10 and S15 imply Inn only.]**

**Commands (§3, back-cover card; key table checked against page images):**

| Command | Apple/Atari | TRS-80 | C64 | PET |
|---|---|---|---|---|
| Cast spell (both phases) | C | C | C | C |
| Replot (shows active duration spells under the map) | P | P | P | P |
| Help (lists ACTION commands; repeat to stall) | H | H | H | H |
| Stay (the default if you are too slow) | S | S | S | S |
| Quit ("Suicide is painless") | Q | Q | Q | Q |
| Store (save to disk/tape) | CTRL-S | CLEAR | **F1** | HOME |
| Strength potion | CTRL-P | G | **F7** | + |
| Heal potion | CTRL-H | U | **F5** | (symbol, unclear in scan) |
| Rescue scroll | CTRL-R | R | **F3** | / |
| Move W / N / E / S | A / W / D / X | same | same | same |
| Up / Down stairs | U / D | same | same | same |
| Fight / Evade / Cast (encounter) | F / E / C | same | same | same |
| Pick up item | RETURN | ENTER | RETURN | RETURN |

The movement keys cluster around S. The keypad can be used (5 = Stay, 8 = North). Fight, Evade and Cast sit just to the right of W, D and X.
- **Walls and doors:** thin lines are doors you can pass through. Thick lines are walls you cannot pass without spells.
- **After evading:** an **@** sign left of the stat chart shows how many squares away the evaded monster is.
- **Uncontrolled outcomes (§3.21):** sometimes you get no chance to act, because something is stolen, the monster strikes first, or you receive a gift. "Not all monsters are enemies."

**Spells (§4):**
- There are 6 spell levels with 6 spells each (36 total). Combat spells are marked with an asterisk.
- Experience level needed for each spell level: 1→L1, 2→L3, 3→L6, 4→L9, 5→L12, 6→L15.
- Casting: press C, choose the level (1–6), then the spell (1–6). INT and WIS affect success.
- The manual describes only levels 1–2. Higher spells are deliberately left for players to discover.

Level 1:
- *Magic Missile (damage).
- *Sleep (no effect on Undead; can work even on high-level Living monsters; "monsters sometimes sleep lightly").
- Cure Light Wounds.
- Light (duration; shows Features in nearby squares as white squares).
- *Turn Undead (WIS-based; Undead only).
- Protection from Evil (duration; prevents level drain by Specter or Vampire).

Level 2:
- *Web.
- Levitate (duration; avoids pits, which are "more dangerous the deeper you go").
- *Cause Light Wounds.
- Detect Traps (duration).
- *Charm (Undead may resist).
- Strength (duration; like the potion).

Names only, from the card:
- L3: *Lightning Bolt, Cure Serious Wounds, Continual Light, Invisibility, *Hold Monster, *Phantasmal Force.
- L4: Pass Wall, *Fireball, *Cause Serious Wounds, *Flesh to Stone, Fear, *Finger of Death.
- L5: Teleport, Astral Walk, *Power Word Kill, *Ice Storm, Wall of Fire, *Plague.
- L6: Time Stop, Raise Dead, *Holy Symbol, Word of Recall, Restoration, *Prismatic Wall.

Asterisk placement on the L4–L6 card is partly garbled in OCR, so treat those combat flags as approximate.

**Monsters (§5):**
- There are 20 monsters, listed by rank from weakest to strongest.
- Each has an experience level, shown on encounter together with its type. The manual mentions a "level 76 Wraith".
- Type: (U) Undead, (L) Living. Undead don't sleep and usually can't be charmed.
- On TRS-80, Apple, Atari and PET every monster is shown as **#**. The C64 has a unique symbol for each.

| Rank | Monster | Manual's short description (paraphrased) |
|---|---|---|
| 1 | Gnoll (L) | Weak dungeon lowlife |
| 2 | Kobold (L) | Gnoll relative with a cranial horn; hamstrings victims |
| 3 | Skeleton (U) | Mindless, attacks relentlessly |
| 4 | Hobbit (L) | Nimble; would rather steal a magic item than fight |
| 5 | Zombie (U) | Fresh corpse, a bit cannier than a skeleton |
| 6 | Orc (L) | Sly man-eater |
| 7 | Fighter (L) | Mercenary who kills you and steals gold and valuables |
| 8 | Mummy (U) | Deadlier than a zombie |
| 9 | Elf (L) | Good fighter, skilled thief |
| 10 | Ghoul (U) | Corpse-eater; its touch paralyzes |
| 11 | Dwarf (L) | Axe-wielder; picks pockets |
| 12 | Troll (L) | Avoid altogether |
| 13 | Wraith (U) | Drains XP and level |
| 14 | Ogre (L) | Merciless |
| 15 | Minotaur (L) | Deadly breath |
| 16 | Giant (L) | Club-wielder |
| 17 | Specter (U) | Drains level |
| 18 | Vampire (U) | Paralyzes; drains blood and level |
| 19 | Demon (L) | Whip and flaming sword, "the whip's worse" |
| 20 | Dragon (L) | Breath weapon; "Never fight a Dragon!" |

**Features (§6):**
- Features are stationary. You investigate them during the ENCOUNTER phase, in real time.
- Light spells reveal them in adjacent squares.
- TRS-80, Apple and PET show them as abbreviations. Atari and C64 use graphics: §6.5 gives the Atari symbol key, and the C64 symbols are called "self explanatory".

| Feature | Abbreviation | What the manual says |
|---|---|---|
| Grey Misty Cube | GMC | Transports you to a dungeon floor of your choice, "sometimes goes awry". Entered with RETURN. §9.5 says a few exist on level 1 |
| Elevator | ELE | One-way ride up one floor |
| Altar | ALT | "Penny pinchers and pagans beware!" |
| Circular Stairway | ( or / (OCR unclear) | Up/down depending on its vertical connections. One sits beneath each Inn |
| Pit | PIT | Drops you to the next lower level. Levitate avoids it |
| Gem-encrusted Throne | THR | Several options, at your own risk |
| Teleportal | TPT | Automatic, one-way teleport to another location. Hard to map |
| Fountain | FTN | Liquids of many colors with varied effects |
| Small Box with Buttons | SAF | Open it by pushing colored buttons in the right sequence |
| Inn | — | See above |

- **Not in the manual:** dungeon dimensions (the "200x200" figure and even "50 levels" are absent from the rules text). There is also no separate "cube" other than the Grey Misty Cube. Fifty levels comes from the ads and reviews (S12, S14, S10, S11). 200x200 comes from S31 (code analysis) and the CGW 4.3 "40,000 rooms (so they say)" (S15).

**Treasures (§7):**
- Shown as **$**, except on the C64, which shows "a glittering cache".
- No carrying limit. Sometimes several treasures appear in a row.
- Magic items with a plus **replace** the one you carry, even if the new one is worse. A Sword+2 replaces a Sword+5.
- Items:
  - Sword: more damage.
  - Armor: protection.
  - Shield: like armor; turns some dragon flame.
  - Elven Cloak: camouflage that may let you surprise monsters.
  - Elven Boots: stealth, better DEX, more nimble in combat.
  - Ring of Regeneration: HP back per turn equal to its plus.
  - Ring of Protection: absorbs HP equal to its plus each combat round.
  - Scroll of Rescue: returns you to the foot of your starting Inn's stairway. Only usable in the Ready state, not in combat. You drop all gold.
  - Potion of Giant Strength: temporary super-strength.
  - Potion of Healing: restores some HP.
- Potions and scrolls are counted in the stats.
- The manual does **not** mention elixirs or amulets. Treasure chests are mentioned as a source of gold.

**Hints (§8):**
- Play as DEMO first.
- Pick high CON. STR, WIS and INT also matter.
- Stay near the Inns until level 2.
- Don't fight dragons.
- Learn Undead vs Living.
- Watch your HP.

**Multi-player suggestions (§9):**
- Treasure Hunt (e.g., ½ hour).
- High Warrior (¾ hour).
- Campaign variation.
- Beast Slayer.
- Third/Second Level Warrior.
- Defying Death (survive on level 25, using a level-1 Grey Misty Cube).
- Magic Extractor.
- Mapping the Depths ("use graph paper").
- Discoverer.

**Loading (§10–11):** These sections show how the product was packaged.
- One cassette carried Atari (3 parts) and TRS-80 on side 1, and C64 (2 parts) and PET (2 parts) on side 2.
- The C64 tape shows the logo and **"CREATING MONSTERS"** while loading. The main part is called "CAS TELENGARD".
- Disks: Apple boots itself. TRS-80 Model I boots; Model III needs conversion, then RUN"TEL". Atari needs the BASIC cartridge, then RUN"D:TELEN.BAS", and the disk must stay in the drive.
- IBM PC (S5, 1985): type TELEN.

### B3. Release history and versions

**DND (mainframe predecessor):**
- [PRIMARY S7] Lawrence wrote DND in BASIC under TOPS-10 on the DECsystem-10 at BOCES Spencerport, NY, in summer 1976 and 1977. He ported it to Purdue's PDP-11 RSTS/E in 1977–79, where it was very popular (blamed for dropouts, and it annoyed the computing staff). DEC invited him to Maynard, MA, to port it to the DECSystem-20 (TOPS-20).
- [PRIMARY S9] In the interview: "DND was written in 1976". It grew from player suggestions at Purdue's ENAD building, and "That summer I took it home and ported it to the Decsystem-10". The order differs from S7 (see §C).
- [PRIMARY S9] He had not seen the PLATO *dnd* (Whisenhunt & Wood, 1975), but testers may have passed on ideas.
- [SECONDARY S27] DND was later freely distributable in unmodified form "by grant of the author". A 1984 PC port of a DEC Pascal DND by "Bill" (R.O. Software) was sold as shareware. Lawrence regarded it as piracy.
- [SECONDARY S25] Wikipedia says DEC ordered DND removed from its computers in Sept 1983 to avoid litigation from Telengard's publisher. I did not verify this.

**Telengard (micro rewrite):**
- [PRIMARY S7, S9] In 1978 Lawrence bought a PET 2001 (about $1,200) and rewrote DND in BASIC as TELENGARD. Early versions fit in 8K; the final one ran "barely inside" 32K.
- [PRIMARY S9] There was no room for stored maps, so he derived each location from X/Y/Z using math involving prime numbers. Real time came from DND's terminal room, where it stopped players from hogging terminals. Before Avalon Hill licensed it, he had ported it himself to the Atari 800, Apple II+ and TRS-80.
- [PRIMARY S9] He did not work for AH. AH saw the game at a gaming convention where he was running competitions and demos, and asked to sell it.
- [PRIMARY S7] The Apple and TRS-80 ports were quick because of similar Microsoft 8K BASIC. Atari was harder because of its string handling. The IBM-PC version was rewritten **in C**, and that source was later lost; Lawrence kept the binaries and offered them for download.
- [SECONDARY S27] The micro version was simplified: one class, one algorithmic dungeon, streamlined rules.

**Commercial platforms (evidence for each):**

| Platform | Evidence | Date evidence | Notes |
|---|---|---|---|
| Apple II (48K disk) | S1 manual, S16, S12, S14, S17, S28 | Space Gamer Dec 1982 review covers Apple/TRS-80 disk, "Published 1982" | Text display (X, #, $, letter codes) |
| TRS-80 Model I/III (32K tape, 48K disk) | S1, S11, S16, S28 | 1982 (S16) | CGW 1983 TRS-80 review. BASIC is modifiable |
| Commodore PET 2001 (32K tape) | S1, S14, S17, S33 | MobyGames 1982. Archeogaming says "1981" (unsupported) | On side 2 of the multi-system cassette. Text/symbol graphics |
| Atari 400/800 (40K tape, 48K disk) | S1 (with its own symbol key), S12, S14 | MobyGames Apr 1983 | Custom character set (S31 lists TELEN.SET) |
| Commodore 64 (tape and disk) | S1, S10, S12, S13, S17 | 1983. CGW ad and reviews from mid-1983 | Sprites, colors, sound. The only version with a unique symbol per monster |
| CP/M (Heath/Zenith Z-90, H/Z-100, CP/M-85 + MBASIC) | S34, S28 | MobyGames 1984 | Rare. Text graphics |
| IBM PC / PCjr | S5 (1985 addendum), S7, S28, S32 | 1985 | Rewritten in C by Lawrence. S27 says two PC versions were sold together (monochrome and CGA) |
| Japan: FM-7, PC-88, PC-98 (Kiya Overseas) | S28 only | 1983–84 | Not verified beyond MobyGames |
| UK C64 (Eclipse Software) | S28 only | 1984 | Not verified |
| Online "Castle Telengard" (CompuServe) | S22, S23, S19, S27 | Listed 1984 (GO GAM-320) and 1988 | Relationship to Lawrence unclear. S27 says "licensed and ported". The CGW editor in S19 calls AH's game "based on the original public domain program". Unresolved |
| VIC-20 | S37, S29 (Neo-Rio comment), S24 (cites a 2020 YouTube video for the 24K need) | 2020s | **Fan port, not an AH release** as far as I can find |
| Ohio Scientific, Amstrad CPC | S27 only | — | Unverified |
| Windows 2002–05 remake (Travis Baldree, buildingworlds.com) | S7, S28, S29 | 2002–2005 | Fan remake of the C64 version |
| C64 "Telengard Remastered 1.1" (CSDb 2016) | S24, IA | 2016 | Fan release |

**Known differences between ports:**
- Graphics: TRS-80, Apple and PET use X for the player, # for monsters, $ for treasure, and letter codes for features (S1, S16, S18). Atari uses a custom graphic symbol set (S1 §6.5). The C64 uses sprites and a custom charset: a unique image per monster, red-brick walls, yellow gold "globs", and a "glittering cache" for treasure (S1, S13, S17, S29).
- Sound: the C64 has sound and music (S12, S29).
- Command keys differ per machine (table above).
- The dungeon is the same on every machine according to the reviews ("It plays the same on any computer", S10; S13). Dan Boris's Atari analysis gives 50x200x200 (S31). The Lemon64 comments (S29) and the MobyGames TRS-80 user review (S28) also describe the map as fixed between runs.
- Speed: everyone complains that the BASIC versions redraw slowly (S15, S16, S17, S29). The PC version was in C (S7).
- The manual's 5-second limit is the same for all. The DOS version is described the same way (S32).
- Box contents: rulebook, disk or tape, a poster (C64 and DOS per S6; S17 confirms a poster in the C64 box), separate loading instructions, and TRS-80 conversion instructions (S16).

**Prices (S10, S11, S12, S14, S16):** $23 cassette, $28 disk.

### B4. Contemporary and later reviews

- **The Space Gamer #58, Dec 1982 (Norman Banduch)** [S16], Apple/TRS-80:
  - "curiously addictive", but "severely flawed in its design and program execution".
  - Flat graphics. The redraw is "maddeningly slow".
  - The keys cause mistakes (E vs D, S vs X), and slow text makes you press keys too early or too late.
  - No pause. Monster difficulty is unevenly matched: his 4th-level fighter was killed by a high-level dragon at the foot of the beginner's tavern stairs.
  - Little variety: level-45 monsters are just meaner versions of level-1 ones.
  - Verdict: "could have been a good game, but is marred by poor programming and lack of polish"; wait for a second edition.
- **CGW 3.3, May–June 1983 (Dick McGrath, "Route 80")** [S11]:
  - The game "borrowed heavily" from D&D, and he looked for a credit to Gygax.
  - Rolling stats is tedious, and you can't enter your own stats.
  - The SV- naming convention produces unpronounceable names.
  - The character is a fighter/mage hybrid with no class choice. You start with an economy sword, armor and shield, one spell, and no money.
  - There is no store to spend gold in. Gold is auto-deposited at the inn.
  - Solo play. 50 levels. Overhead view limited to line of sight, which confuses mapping.
  - Feature effects change between visits. 20 monsters. 36 spells; the booklet names but doesn't describe the higher ones.
  - The more he played, the more he liked it. It has the hook that brings you back.
  - Because it is BASIC, he promised printing code in the next column.
- **Creative Computing, Sept 1983 (Brian J. Murphy)** [S18]:
  - "Last, and best" of four AH titles reviewed. Credits Dan Lawrence and Mike O'Brien.
  - Graphics are rudimentary (X, letter codes, #), but the action is "fast and furious".
  - Stat sets flash for about 3 seconds each.
  - Starts below the "Worthy Meade Inn", with status in the upper-right quarter of the screen. 1 SU at start.
  - Mentions treasure chests, cloaks (his text says "clocks", probably a typo), friendly monsters that give gifts or heal, fountains, misty cubes, altars, pits, elevators.
  - Not as good as Wizardry, but easy to play and a good introduction for newcomers.
- **COMPUTE! #40, Sept 1983 (Tony Roberts)** [S10]:
  - Covers Atari, Apple, PET and C64. Plays the same on all; the C64 "has the most flair".
  - Stats, treasures, gold and XP sit on the right side of the screen.
  - About 5 seconds per move. Inns pause the game.
  - A 2–3 character keyboard buffer can cause accidental moves.
  - Treasure chests can hold 10,000 gp or a poisonous spider.
  - "Can tie you up in knots and rob you of your sleep."
- **CGW 3.4, 1983 (Edward Curtis, Commodore Key)** [S13]: C64 graphics are "much improved", mainly through sprites. The sudden appearance of different creatures and treasures adds a lot.
- **CGW 4.3, June 1984 (Roy Wagner)** [S15], C64:
  - Very similar to Temple of Apshai, but with 50 levels × 40,000 rooms "(so they say)".
  - Fast-paced: messages have to be read quickly. Wall drawing is slow. You can save at any inn.
  - Contains errors ("ten different monsters"; stats "distributing up to 18 points").
- **Different Worlds #40, Jul–Aug 1985 (Troy Christensen)** [S17], C64:
  - "Wildly exciting and totally infectious".
  - C64 graphics are rich and colorful. Apple graphics are "non-existant" (keyboard symbols).
  - He counts "at least 50,000 rooms".
  - Slow scrolling. Magic items with very high plusses (+30 swords "commonly found") don't always work as described.
  - Strongly recommended for C64 owners. Mentions the poster in the box.
- **Playboy, Feb 1983** (as quoted in the AH ad, S12): "one of the most powerful examples of the challenging diversions your computer can offer these days." I did not see the original.
- **CGW #38, 1987 (Scorpia)** [S19]: describes "Castle Telengard" as an early, primitive hack-and-slash with no goal.
- **CGW #92, 1992 (Graef)** [S20]: Telengard was quickly overshadowed by Wizardry and Ultima.
- **CGW #111, Oct 1993 (Scorpia)** [S21]: an early "hack'n'slash with no main goal", clearly D&D-derived, dated compared with the Gold Box games, "Back then, however, it was hot stuff".
- Not found: Softline, Antic (except the CompuServe mention), Byte, Zzap!64, Commodore User. Wikipedia also cites AllGame (Earl Green, 4/5) and Appelcline's *Designers & Dragons*. I did not read either.

### B5. Daniel Lawrence's own statements

- **S9 (2007):** He played D&D. DND evolved from player suggestions. He did not know the PLATO dnd. He explains the procedural generation. Telengard was played locally in 1978 and ported before AH licensed it. Real time was meant to stop terminal hogging. 8K left no room for a story; today a story should be dynamic, driven by NPC motives. "Memory, Memory, Memory… one very dense hunk of BASIC". He was never an AH employee.
- **S7 (website):** "an ancient computer game which I wrote in 1982" (sic), then the DND/PET history. Alas, Avalon Hill "is no longer with us". He claims it predates Temple of Apshai and Wizardry and inspired later games. The IBM-PC C source is lost, and he offers the binaries.
- **S8:** Purdue 1976–80. He co-designed the tabletop RPG *Adventure Quest*. He adapted and maintained **MicroEMACS** for many years. He worked on the KnowledgeMan/GURU 4GL products. He lived in Lafayette, IN.
- **S35:** Died 7 June 2010, age 52, Lafayette, IN. A friend's tribute calls Telengard "definitely the most-copied (many times without credit being given)".

### B6. C64 version: screen, colors, messages (as recorded)

- Map: an overhead view of the immediate surroundings. Thin lines are doors and thick lines are walls (S1). Only what the character can see is drawn (S11).
- Colors: red-brick walls, bright yellow gold "globs", detailed and unique monster images (S17). Sprites and custom charset (S13, S29). Some color cycling (S29, fan memory).
- Status display: on the right of the screen (S10), or "upper righthand quarter" (S18, platform not stated). It shows the six attributes plus LVL, HP (cur/max), SU (cur/max), EXP, gold and items (S1, S10, S18). The @ distance indicator appears left of the stat chart after an evade (S1).
- Prompts and messages from the manual: a right-pointing arrow cursor marks the Ready state. "WHICH LEVEL?" when casting. "LEAVE IT" for skipped treasure. "IT MISSED…" (flavor in the Elven Boots entry). Load screens show "CREATING MONSTERS" and "FOUND CAS TELENGARD". The CRPG Addict (DOS) quotes encounter prompts in the form "You see a level 3 dragon. (F)ight, (c)ast, or (e)vade?" and "(u)p, (d)own, or (s)tay" on stairs (S32).
- Monsters are shown with type and level on encounter (S1 §5.2, S10).
- Sound and music: wind, bell, organ title music, throne music (S29, fan memories only, not verified in a primary source).
- Lemon64 lists joystick port 2 as the control (S30). This contradicts the keyboard-only manual. Treat it as unverified.

### B7. Copyright, trademark and rights

- [MANUAL S1] "© The Avalon Hill Game Company 1982 … Computer Program and Audio Visual Display Copyrighted". TELENGARD is called AH's "trademark name". [S5] The IBM addendum is © 1985 AH.
- [PRIMARY S9] Lawrence says AH "licensed" the game. He was not an employee. The actual split of rights between Lawrence (or "Orion Software") and AH is **not documented** in what I read.
- [SECONDARY S26] Hasbro bought Avalon Hill on 4 Aug 1998 for $6M, including trademarks, copyrights, divisions and Avalon Hill Software. It was placed under Hasbro Interactive. AH was excluded when Hasbro Interactive was sold to Infogrames in 2001. AH moved to Wizards of the Coast in 2004 and to Hasbro Gaming in 2021. So whatever rights AH held now presumably sit with **Hasbro**. I found no specific statement about the Telengard title.
- [S27, c.2002] The DND fan page says AH (that is, Hasbro-era AH) "does not wish Telengard to be distributed online". It thanks AH for answering questions about a product "they haven't sold for over ten years".
- [S7] Lawrence himself offered the IBM-PC binaries as a free download from aquest.com. There is no licence text, and that was not a public-domain grant.
- **No statement found that Telengard is freeware or public domain.** The CGW 1987 editor's "original public domain program" remark (S19) refers to the online/early program and is not authoritative. Any reimplementation should treat the name and the manual text as possibly still protected.

---

## (c) DISAGREEMENTS BETWEEN SOURCES

1. **When Telengard was written.** Lawrence's site (S7) says "which I wrote in 1982". The same page, and the interview (S9), say 1978 on the PET. The DND fan page (S27) says "1980-1985". Archeogaming (S33) says PET 1981. Most likely 1978 for the private version and 1982 for the AH release.
2. **Order of DND platforms.** S7: written on the DEC-10 at BOCES in summer 1976/77, then ported to Purdue's RSTS/E in 1977–79. S9: it evolved at Purdue's ENAD as a freshman, "That summer I took it home and ported it to the Decsystem-10". S25 (Wikipedia DND): 1977, PDP-10. S27: "1972?-1978", TOPS-10 then RSTS.
3. **Who wrote DND.** MobyGames trivia (S28) says Telengard was a port of "Gary Whisenhunt's and Ray Wood's DND". This contradicts Lawrence (S9), who says DND was his own and he never saw the PLATO dnd. MobyGames is very likely wrong.
4. **Credits.** The manual (S1) and MobyGames credit "Orion Software". Creative Computing (S18) credits "Dan Lawrence and Mike O'Brien". Lawrence is not named in the manual. Who Mike O'Brien is, and whether Orion Software was Lawrence's label, is unresolved.
5. **Dungeon size.** The ads (S12, S14) say 50 levels with "hundreds of" chambers per level. CGW 4.3 (S15) says 40,000 rooms per level "(so they say)". Different Worlds (S17) says "at least one thousand rooms" per level. Dan Boris's code analysis (S31) and MobyGames (S28) give 200×200×50, about 2 million. The manual gives no figure.
6. **Number of monsters.** The manual and most reviews say 20. Wagner (S15) says "ten". Grognardia (S38) says "36 monsters", confusing them with the 36 spells.
7. **Character creation.** The manual says random 3–18 sets, cycled and accepted with RETURN. Wagner (S15) claims you distribute up to 18 points. Creative Computing says each set flashes for "exactly three seconds". The manual just says "periodically".
8. **XP for level 5.** The manual says L5 needs 16,000. Creative Computing (S18) says "The fifth level requires 32,000", which is the manual's L6 value. This may be an off-by-one in how he counted.
9. **Platforms.** Wikipedia (S24) lists a VIC-20 version needing 24K. The only evidence I found is a 2020s fan port (S37, S29). The DND page (S27) lists Ohio Scientific and CPC ports, with no corroboration. MobyGames lists Japanese ports that nothing else I read mentions.
10. **Saving.** The manual lists Store as a general ACTION-phase command (key per machine), without saying "inn only". COMPUTE! (S10) and CGW 4.3 (S15) say you save at inns. The CRPG Addict and Different Worlds don't settle it.
11. **Lawrence's death year.** The author page (S8) gives "Died: June 7, 2010" and the obituary (S35) agrees. A later-added sentence on the same page says "passing in 2014", which is an error.
12. **Relation to "Castle Telengard" (CompuServe).** S27 says Telengard was licensed and ported to CompuServe. The CGW 1987 editor (S19) says AH's Telengard was a later commercial product based on the "original public domain program". Lawrence's own accounts don't mention CompuServe.

## (d) GAPS NOT RESOLVED

- I could not see the C64 screen directly, so I have no pixel-exact layout or color list. Descriptions come from reviews and fan memories. The MobyGames screenshot gallery was not fetched (blocked).
- The exact C64 on-screen message strings (beyond those in the manual), the inn screen, and the title/music sequence are unverified.
- Whether "Store" works outside Inns. The manual is ambiguous.
- Effects of the level 3–6 spells, and of fountains, thrones, altars and boxes. The manual deliberately leaves these out; see the source-code agent.
- No record of a first-edition manual (1982) to compare with the 2nd edition (3/83). One Atari copy's edition line is illegible in OCR ("3nd").
- The original Playboy (Feb 1983) review and the AllGame review were not read. Softline, Antic, Byte, Zzap!64 and Commodore User reviews were not found.
- The DEC removal of DND in Sept 1983 (S25) was not verified in a primary source.
- Who owns the Telengard copyright today: Hasbro (via AH), or Lawrence's estate for the underlying program. No public statement found, and no freeware declaration exists.
- The Japanese (Kiya) and UK (Eclipse) releases rest on MobyGames alone.
- The identity of Orion Software and of Mike O'Brien.
