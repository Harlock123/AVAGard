# Source ledger

All sources were accessed on **2026-10-05**. The full history, manual and review ledger (S1–S39:
manual scans, Lawrence's website and interview, 1982–1993 magazine reviews, Wikipedia, MobyGames,
Lemon64, fan sites) is in
[research/history-and-manual.md §(a)](research/history-and-manual.md). Each entry records what was
actually inspected, the claims it supports, its confidence and its licensing notes. This file adds
the technical and implementation sources.

## Primary technical evidence (research only; nothing copied into the repository)

| # | Title / item | Org | URL | Type | Platform | What was inspected | Supports | Confidence | Licensing |
|---|---|---|---|---|---|---|---|---|---|
| T1 | `Telengard_1983_Avalon_Hill.d64` (md5 22da3702…) | Avalon Hill (preserved by archive.org) | https://archive.org/details/d64_Telengard_1983_Avalon_Hill | Disk image | C64 | Extracted all files; decoded `DISK TELENGARD` (V4.18), the loader, a 3rd-party character editor and leftover saves | Every rule in research/c64-mechanics.md | High | © Avalon Hill / Microcomputer Games 1981–83. Used as evidence only |
| T2 | Telengard source code listing (Atari DSKTEL.BAS, annotated) | Dan Boris (2008) | https://archive.org/details/Telengard_Source_Code_Listing | Annotated listing (PDF, OCR) | Atari 8-bit | Formulas for combat, encounters and map | Atari cross-check; Atari map differs from the C64 one | High | Contains original code; evidence only |
| T3 | Atari tech page | Dan Boris | http://atarihq.com/danb/Telengard.shtml | Technical analysis | Atari | Full page | 50×200×200 dungeon, room-code meanings | High | — |
| T4 | C64 BASIC ROM 901226-01, KERNAL 901227-03 | Commodore (dumps from the VICE svn mirror) | https://github.com/VICE-Team/svn-mirror | ROM images | C64 | Disassembly of the float routines; ran the original map lines in a 6502 emulator | Bit-exact float semantics; 204,020-room ground truth | High | Copyrighted ROMs, used only locally to generate numbers. The ROMs are **not** in the repository |
| T5 | Telengard Rules Manual (C64 copy) | Avalon Hill | https://archive.org/details/Telengard_1983_Avalon_Hill | Manual scan (OCR) | C64 | Key card, manual text | Commands, intended item effects | Medium-high | © Avalon Hill; paraphrased only |

## Existing ports and remakes examined (none used as a source of truth; no code copied)

| Project | URL / commit | License | Notes |
|---|---|---|---|
| malbanGit/Telengard (Vectrex) | github, 22019b1 | none (all rights reserved) | Emulates C64 floats; reduces feature values >9 differently from V4.18 |
| mgritter/telengard (C++) | github, 806ad8c | none | IEEE-double map, which its README says is inexact; teleporter nested-IF bug |
| danlb2000/TelengardMapViewer (C#) | github, a3cd10b | MIT | Atari-based formula, so not the C64 map |
| gbush235711/elm-telengard | github, 7e0c123 | MIT | Map remake; may ship ROM-derived glyphs |
| Tallicia/TelengardXXXX (Python) | github, eec9478 | none | IEEE-double map |
| gondur/Telengard; Abhisheksinha1506/Telengard_Remastered_1.1 | github | none / GPL-3.0 (re-upload; provenance doubtful) | "Remastered 1.1", a *modified* game; excluded as evidence |
| shaefer/telengard, telengard2 | github | none | JS homages |
| JackSmack1971/telengard-reloaded | github | MIT | A reimagining; not relevant to fidelity |

## Implementation references

| Item | URL | Used for |
|---|---|---|
| Avalonia 12 docs and release notes | https://docs.avaloniaui.net, https://github.com/AvaloniaUI/Avalonia/releases | API choices: custom `Render`, headless testing, compiled bindings |
| nuget.org package metadata (nuspecs) | https://api.nuget.org | Pinned versions, target frameworks and licenses |
| .NET `Environment.SpecialFolder` mapping (runtime PR #68610) | https://github.com/dotnet/runtime | Per-user paths |
| google/fonts VT323 | https://github.com/google/fonts/tree/main/ofl/vt323 | Font file and OFL text |

Full survey, with more detail: [research/open-source-and-licensing.md](research/open-source-and-licensing.md).
