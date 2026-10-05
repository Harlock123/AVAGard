# Classic vs Modern

Both presets use **the same engine, dungeon, monsters, spells, treasure and probabilities**. Modern
changes how you interact with the game, not what happens in it, except for the timing rule, which is
disclosed below.

| | Classic | Modern |
|---|---|---|
| Dungeon, rules, probabilities | V4.18 | Identical |
| Time limit per decision | 3.7 s (reconstructed from the original's 400 keyboard polls) | Adjustable, 2–15 s (default 10 s), or **deliberate mode with no limit**. *Changing the limit changes the original real-time rule.* |
| DEMO character (empty name) | No limit, cannot be stored (original) | Same |
| Attribute rolling | A new set appears every 3.7 s or on any key; RETURN keeps it (original) | Reroll and Accept buttons; no automatic cycling |
| Storing (F1, at the prompt or in an inn) | Saves and returns to the title screen (original) | Saves and keeps playing |
| Resume after exit or crash | None. Only what you stored (original) | A resume point is written at every command prompt. Leaving mid-encounter resumes at the start of that turn |
| Death | The stored character is left untouched and can be read in again (V4.18 behaviour) | Same; the resume point is deleted |
| Optional manual death rule | Off by default. When on: erase the stored copy unless the name starts with "SV" (as the printed manual says) | Same option |
| Pause (Esc) | Available. **The original had no pause**; this is a disclosed convenience | Available |
| Message pauses | About 1.5 s beats, adjustable and skippable with any key | Shorter by default (0.35×), skippable |
| Status panel | Attributes, HP, SU, EX, GD, items, active effects (names only), followers as `Q` markers | Adds dungeon level, coordinates, turn count, effect durations, a follower distance text, tooltips |
| Automap | None (the original expected graph paper) | Visited rooms on the current level, walls, doors and feature letters |
| Log | Full log in the side panel | Same |
| Key remapping | Original keys, plus arrows and keypad | Dungeon commands remappable in Settings |
| Tooltips and spell descriptions | Available (presentation only) | Available |
| Sound and music | Same in both: new SID-style effects and chiptune by default. Original C64 sounds are not used | Same |

## Accommodations that touch timing or rules (disclosure)

1. **Pause** freezes the deadline. This is not in the original.
2. **Modern deadline and deliberate mode** change the time pressure.
3. **Keyboard type-ahead is not emulated.** Keys pressed while messages play only hurry the messages.
   On the C64 they would have queued up as moves.
4. **Randomness** is a modern generator. Distributions match the original; sequences cannot.
