# Telengard / Avalonia OSS & Asset Survey

Date checked: 2026-10-05. Everything below was checked against the live source on that date (GitHub API through `gh`, the nuget.org flat-container and registration APIs plus nuspec files, docs.avaloniaui.net, and the dotnet/runtime source). Where something is inferred and not confirmed, the text says so. This is not legal advice.

---

## 1. Existing Telengard ports, remakes and fan projects

"No LICENSE" means the default applies: all rights reserved. Code with no license is not open source and cannot be reused.

| Project | URL | Lang | Latest commit / tag | License (LICENSE file checked) | Follows which original | Contains original code/assets? | Use recommendation |
|---|---|---|---|---|---|---|---|
| Telengard Remastered (mirror by gondur) | https://github.com/gondur/Telengard | C64 BASIC + 6502 asm (CBM PRG Studio) | `0421203c6b` 2016-04-16, no tags | **None** (API 404, no LICENSE file) | C64 version. The release notes describe "Telengard Remastered v1.0/v1.1" by Eightbits (CSDb release 145192, 2016-02-10). | **Yes, derived.** `telengard.bas` is the modified original C64 BASIC. Its title screen prints "Avalon Hill presents ... (C)opyright 1983". It also ships `.d64` disk images. | **Do not use or copy.** At most it is a reference for how the C64 version behaved, and even that is a *modified* version, not the original. |
| Telengard_Remastered_1.1 (re-upload) | https://github.com/Abhisheksinha1506/Telengard_Remastered_1.1 | BASIC/asm | `570a031096` 2023-06-20 | LICENSE file says **GPL-3.0** | Same Eightbits C64 Remastered 1.1 | **Yes**, the same files as above | **Avoid.** A third party put GPL-3.0 on code derived from Avalon Hill's copyrighted work. That license grant is very likely invalid. |
| Internet Archive "Telengard Source Code" | https://archive.org/details/Telengard_Source_Code | BASIC (zip) | uploaded 2019-07-08 | No rights statement | "Version 1.1 - Remastered" (Eightbits zip) | Yes | Do not copy. |
| Internet Archive "Telengard Source Code Listing" | https://archive.org/details/Telengard_Source_Code_Listing | PDF scan | uploaded 2019-07-08 | No rights statement | Platform not stated | Yes (a listing) | Do not copy. |
| Vectrex port (Malban) | https://github.com/malbanGit/Telengard | C + 6809 asm | `22019b1903` 2026-09-02, no tags | **None** | README says "Original game by Daniel Michael Lawrence (1982)" and points to the C64 manual | **Likely derived** (my inference): `source/bank0/float40.c` emulates C64 BASIC 40-bit floats, which suggests a close port of the BASIC logic. Binaries (`.bin`) are included. | Do not copy. |
| elm-telengard | https://github.com/gbush235711/elm-telengard | Elm | `7e0c1232b1` 2014-04-28 | **MIT** (LICENSE.md) | C64 look (`c64.elm`, "emulated Commodore 64 display") | Includes `charset.png`, which is probably the C64 character set (likely ROM glyphs), and tile PNGs drawn in C64 style. The MIT grant covers the author's own code only. | Early-stage map viewer only. Do not reuse the charset or tiles. |
| mgritter/telengard (map reverse-engineering) | https://github.com/mgritter/telengard | C++ | `806ad8ca04` 2021-04-05 | **None** | C64 version (README: "reverse-engineer the algorithm used to procedurally generate the fifty levels") | Emulates C64 floating point to reproduce the original map hash. Includes `docs/*.html` level renders. | Read-only research reference. No license, so do not copy. |
| TelengardMapViewer (Dan Boris) | https://github.com/danlb2000/TelengardMapViewer | C# WinForms | `a3cd10b0a2` 2026-07-20, no tags | **MIT** (LICENSE.txt, file headers) | Header: "Based on the Atari version of Telengard, Avalon Hill (1982)" | **Clearly derived.** `CalculateRoom()` is a direct transliteration of the original level-generation formula (`q = x*XO + y*YO + level*ZO ...`, `LogicalAnd(q,0xFF)` and so on). The MIT license covers Boris's code, but the transliterated expression is still a copyright concern. | Do not copy its code. You may cite it as a behavioural cross-check, but verify against primary sources. |
| shaefer/telengard (homage) | https://github.com/shaefer/telengard | JavaScript (Node) | `d2d305dadf` 2024-09-13 | **None** | "homage ... room generation done like the original" | Third-party art: `art_license.txt` points to openclipart and nicubunu, plus named-artist monster images whose license is unclear. Also includes MP3/WAV SFX. | Avoid. No license and assets of mixed provenance. |
| shaefer/telengard2 | https://github.com/shaefer/telengard2 | JS (React/Redux) | `ffc1a6e689` 2018-02-20 | **None** | Loose homage | Own/stock images | Avoid. |
| Tallicia/TelengardXXXX ("Telengard at 40") | https://github.com/Tallicia/TelengardXXXX | Python | `eec9478042` 2022-04-06 | **None** | Not stated (2 files) | Unknown | Ignore. |
| JackSmack1971/telengard-reloaded | https://github.com/JackSmack1971/telengard-reloaded | C# (.NET 8) + Godot 4 | `317df32210` 2026-08-26 | **MIT** | "Reimagining", explicitly not a faithful recreation | No original code seen in the tree listing | Not useful for fidelity. Can be a .NET architecture reference. Do not treat its rules as original. |
| riverstory/telen-beta-01 | https://github.com/riverstory/telen-beta-01 | none | `c70f22c27d` 2020-06-08 | GPL-3.0 | n/a | Empty (only README and LICENSE) | Ignore. |
| Telengod (Ward Shrake) | https://orphanedgames.com/articles/Telengod/Telengod.html | C64 | 1985 and 2015 versions | No license stated | C64 character editor | Has its own source. The site also hosts the original disks and manual. | Use the manual and article for reference only. |
| Travis Baldree C64 remake (2002 freeware) | (was) http://buildingworlds.com/telengard/ | Windows | ~2002 | Freeware, no source | C64 | n/a | The URL now returns **HTTP 404**. Mentioned on Lawrence's page https://www.aquest.com/telen.htm |

Other notes:
- Dan Lawrence's own page (https://www.aquest.com/telen.htm) lists the platforms (PET, Apple ][+, TRS-80, Atari 400/800, C64, IBM PCjr/PC), offers the IBM-PC binaries, and says "Avalon Hill is no longer with us". It gives **no remake or redistribution permission**.
- I searched GitHub (`gh search repos telengard`, 11 hits), the web, GitLab and SourceForge. No GitLab or SourceForge projects turned up.
- **Bottom line:** none of these is a clean-room, permissively licensed implementation that faithfully follows the original. Several contain or transliterate the original code. Implement from your own notes and primary documentation (the manual and observed play), and do not copy code from any of these.

---

## 2. Avalonia 12 + .NET 10

### Package versions (nuget.org, checked 2026-10-05)

| Package | Latest stable | Published | TFMs in nuspec dependency groups | License | Notes |
|---|---|---|---|---|---|
| Avalonia | **12.1.3** | 2026-09-22 | net10.0, net8.0 (I also confirmed the `lib/net10.0` and `lib/net8.0` folders from the catalog entry) | MIT | GitHub release 12.1.3 tagged 2026-09-22 and marked Latest. Earlier 12.1 releases: 12.1.0 (07-09), 12.1.1 (07-29), 12.1.2 (09-02). The 11.3.x line continues (11.3.22). |
| Avalonia.Desktop | 12.1.3 | 2026-09-22 | net10.0, net8.0 | MIT | Depends on Avalonia.Win32, .X11, .Native, .Skia and **.HarfBuzz** |
| Avalonia.Themes.Fluent | 12.1.3 | | net10.0, net8.0 | MIT | |
| Avalonia.Themes.Simple | 12.1.3 | | net10.0, net8.0 | MIT | A lighter alternative theme |
| Avalonia.Fonts.Inter | 12.1.3 | | net10.0, net8.0 | MIT | Optional (`.WithInterFont()`) |
| Avalonia.Headless | 12.1.3 | | net10.0, net8.0 | MIT | |
| Avalonia.Headless.XUnit | 12.1.3 | | net10.0, net8.0 | MIT | **Depends on `xunit.v3.extensibility.core` >= 3.2.2, so it requires xUnit v3.** |
| Avalonia.Templates | 12.1.3 | 2026-09-22 | (dotnet new templates) | no license expression in the nuspec | `dotnet new install Avalonia.Templates::12.1.3` |
| AvaloniaUI.DiagnosticsSupport | 2.2.3 | 2026-06-21 | net10.0, net8.0 | **No license expression** in the nuspec. Copyright "AvaloniaUI OÜ". This is proprietary tooling. | Replaces Avalonia.Diagnostics. The Dev Tools need an AvaloniaUI Portal login. Community edition is free for non-commercial and individual use; Plus is EUR 299/yr. **Optional, debug only.** |
| Avalonia.Diagnostics | 11.3.22 (no 12.x exists) | | net6/net8/netstandard2.0 | MIT | **Removed in v12. Do not reference.** |
| CommunityToolkit.Mvvm | **8.4.2** | 2026-03-25 | net8.0, net8.0-windows10.0.17763, netstandard2.0/2.1 | MIT | Works on net10.0 through the net8.0 asset. |
| xunit.v3 | 4.0.1 | 2026-09-12 | net8.0, net472 | Apache-2.0 | The package is named "xunit.v3" but its version is 4.0.1. Avalonia.Headless.XUnit only requires extensibility.core >= 3.2.2. **Test this combination before pinning 4.x.** The fallback is the newest xunit.v3 3.x. |
| xunit.runner.visualstudio | 4.0.0 | 2026-08-15 | | Apache-2.0 | |
| Microsoft.NET.Test.Sdk | 18.10.1 | 2026-09-15 | | MIT | |

### Avalonia 12 breaking changes relevant to a new app
Source: https://docs.avaloniaui.net/docs/avalonia12-breaking-changes

- **.NET:** .NET Framework and netstandard support was dropped. .NET 8+ only, and **"The recommended target is .NET 10"**.
- **Compiled bindings are on by default.** `AvaloniaUseCompiledBindingsByDefault` is now true, so every `{Binding}` maps to `CompiledBinding`. Set `x:DataType` on views and DataTemplates.
- **Binding plugins removed.** The DataAnnotations validation plugin is now off by default. In v11 it had to be disabled by hand because it conflicted with CommunityToolkit.Mvvm; that is no longer needed.
- **Binding class hierarchy:** `IBinding` became `BindingBase`. In code, use `CompiledBinding.Create(...)` or `ReflectionBinding`.
- **Avalonia.Diagnostics removed.** Use `AvaloniaUI.DiagnosticsSupport` and `this.AttachDeveloperTools()` (or `WithDeveloperTools()`), guarded by `#if DEBUG`.
- **Text shaper:** HarfBuzz is configured by `UsePlatformDetect()`. If you call `UseSkia()` explicitly, you must also add `UseHarfBuzz()` and the `Avalonia.HarfBuzz` package.
- **Fonts:** Avalonia now has its own font parser. Only TTF/OTF are supported; Type 1 is dropped. This matters if you embed pixel fonts, which must be .ttf or .otf.
- **Focus:** `GotFocus`/`LostFocus` now take `FocusChangedEventArgs`. All focus handling goes through `FocusManager`/`IFocusManager`, and `KeyboardNavigationHandler.GetNext` became `FocusManager.GetNextElement`.
- **Dispatcher:** multiple dispatchers are supported. `DispatcherTimer` uses the *current* dispatcher, so create game-loop timers on the UI thread. `Dispatcher.InvokeAsync` now captures the ExecutionContext.
- **Window.WindowState** is a direct property and can no longer be set from styles.
- **Access keys** are triggered by the key symbol. `AccessText.AccessKey` is now `string?`.
- **Animations** stop on invisible controls. To keep the old behaviour, set `Animation.PlaybackBehavior = Always`.
- **Gestures** attached events moved to `InputElement`, so drop the `Gestures.` prefix.
- **Headless:** "xUnit.net support now targets version 3 (up from 2)". NUnit now targets 4.
- Direct2D1 and BinaryFormatter were removed. `Screen` is abstract.

### Custom rendering for a tile or grid view
Sources: https://docs.avaloniaui.net/docs/graphics-animation/custom-rendering, plus the API checked in the 12.1.3 source.

- **Recommended default:** subclass `Control` and override `public override void Render(DrawingContext context)`. Use `DrawRectangle`, `DrawLine`, `DrawImage`, `DrawText`/`FormattedText`, `DrawGeometry`, and `PushClip`/`PushTransform`/`PushRenderOptions`. In the static constructor, call `AffectsRender<T>(props...)` so property changes invalidate the control; otherwise call `InvalidateVisual()` when the model changes. This runs on the UI thread and is plenty for an 8x8 room viewport plus a text panel.
- **Pixel-crisp scaling:** `context.PushRenderOptions(new RenderOptions { BitmapInterpolationMode = BitmapInterpolationMode.None })`, or the `RenderOptions.BitmapInterpolationMode` attached property.
- **WriteableBitmap** (`Avalonia.Media.Imaging`): construct it with `new WriteableBitmap(PixelSize, Vector dpi, PixelFormat?, AlphaFormat?)` and write pixels through `using var fb = bmp.Lock()` (`ILockedFramebuffer`). Use it to compose an indexed-colour "screen" buffer (C64-style 320x200 framebuffer) and then `DrawImage` it scaled. This is good for an authentic retro look.
- **ICustomDrawOperation** via `context.Custom(...)` plus `ISkiaSharpApiLeaseFeature` gives direct SkiaSharp access. Only needed for advanced Skia features.
- **CompositionCustomVisualHandler** (render-thread animation) and **TopLevel.RequestAnimationFrame** (per-frame UI-thread callback) are for continuous animation. A turn-based game mostly needs `InvalidateVisual` plus a `DispatcherTimer` for the real-time "monster acts after N seconds" tick.

### Keyboard handling
Sources: https://docs.avaloniaui.net/docs/events/input-events, https://docs.avaloniaui.net/docs/input-interaction/mouse-and-keyboard-shortcuts, https://docs.avaloniaui.net/docs/concepts/input/focus

- `KeyDown`, `KeyUp` and `TextInput` fire on the **focused** element and bubble up. Override `OnKeyDown(KeyEventArgs e)` and use `e.Key`, `e.KeyModifiers` and `e.KeySymbol` (the produced character). Set `e.Handled = true` once handled.
- `TextInput` (`e.Text`) is better for single-letter commands such as Telengard's letter commands, because it follows the keyboard layout. `Key` is better for arrows and numpad.
- To intercept keys before child controls see them, use `AddHandler(InputElement.KeyDownEvent, handler, RoutingStrategies.Tunnel)`. Add `handledEventsToo: true` if needed.
- `KeyBindings` (a `KeyBinding` with `Gesture` and `Command`) map keys to ViewModel commands, but **only fire when the control or a descendant has focus**. Put them on the Window.
- The game view must set `Focusable="True"` and call `Focus()` on load, or the Window-level handler will not receive keys.

---

## 3. Retro-appropriate fonts, sounds and audio

### Fonts (google/fonts repo `ofl/*` directories, OFL.txt read for each)

| Font | Source | License (exact) | Provenance note | Recommendation |
|---|---|---|---|---|
| **Press Start 2P** | https://github.com/google/fonts/tree/main/ofl/pressstart2p | SIL OFL 1.1, "Copyright 2012 The Press Start 2P Project Authors (cody@zone38.net), with **Reserved Font Name "Press Start 2P"**" | Based on 1980s Namco arcade fonts ("Return of Ishtar"). It is not the C64 ROM. | Good 8x8 bitmap look. Keep the name unchanged if unmodified; if you modify it, rename it because of the RFN. |
| **VT323** | https://github.com/google/fonts/tree/main/ofl/vt323 | SIL OFL 1.1, "Copyright 2011, The VT323 Project Authors", no RFN | DEC VT320 terminal style | Very readable. A good choice for the message and status panel. |
| **Sixtyfour** | https://github.com/google/fonts/tree/main/ofl/sixtyfour | SIL OFL 1.1, "Copyright 2021 The Sixtyfour Project Authors (jenskutilek/homecomputer-fonts)" | Description: "a rework of some old pixel versions of the **Commodore 64** ... fonts the author created". Variable axes for scanline and bleed. Limited glyph set. | The closest C64 feel available under OFL. It is redrawn, not the ROM, but explicitly modelled on C64 glyphs. Low risk (typeface designs are generally not copyrightable in the US), but if you want zero ambiguity prefer Press Start 2P or VT323. |
| Silkscreen | https://github.com/google/fonts/tree/main/ofl/silkscreen | SIL OFL 1.1, "Copyright 2001 The Silkscreen Project Authors" | Original pixel design | Small UI labels. |
| IBM Plex Mono | https://github.com/google/fonts/tree/main/ofl/ibmplexmono | SIL OFL 1.1, "Copyright © 2017 IBM Corp. with **Reserved Font Name "Plex"**" | Modern | Fallback or debug font, not retro. |
| JetBrains Mono | https://github.com/google/fonts/tree/main/ofl/jetbrainsmono | SIL OFL 1.1, "Copyright 2020 The JetBrains Mono Project Authors" (variable `[wght]` TTF) | Modern | Fallback, not retro. |
| Others found (all OFL): Pixelify Sans, Share Tech Mono, Micro5, Tiny5, Jersey10, Workbench (Amiga-inspired) | google/fonts `ofl/*` | OFL 1.1 | | Optional. |

- **Avoid:** the actual Commodore 64 ROM character set and any font advertised as a ROM rip. Also avoid Style64 "C64 Pro" (custom license, not OFL; I did not verify the terms here, so treat it as not cleared).
- **OFL obligations:** ship `OFL.txt` with the app, for example under `Assets/Fonts/` and in an about/licences screen. Do not sell the font on its own. Respect any RFN when modifying.
- **Embedding in Avalonia** (https://docs.avaloniaui.net/docs/styling/custom-fonts):
  1. Add `<AvaloniaResource Include="Assets\**" />` to the csproj.
  2. Reference the font as `FontFamily="avares://AssemblyName/Assets/Fonts#Press Start 2P"`. The `#Family Name` must be the font's *internal* family name; if it is wrong, Avalonia silently falls back.
  3. Declare it once as an `Application.Resources` `FontFamily` resource. A `FontCollection` can also be registered via `.ConfigureFonts(...)` on the AppBuilder.
  4. v12 needs TTF/OTF.

### Sound effects (CC0)
- **Kenney** (https://kenney.nl/support): "all game assets on the asset pages are public domain licensed (CC0) ... free to use ... even in commercial projects". Attribution is optional, but do not use the Kenney logo. Relevant packs: Digital Audio (60), Interface Sounds (100), UI Audio (50), RPG Audio, Impact Sounds, Retro Sounds.
- **Procedural alternative (recommended, no asset licensing):** generate square, triangle and noise waves in code, with an envelope and pitch slides, in the style of sfxr/bfxr/jsfxr. A 16-bit PCM buffer at 44.1 kHz is a few lines of C#. You can play from memory or write .wav files at build time. This gives a C64/SID-like "beep and noise" feel with no third-party assets.

### Cross-platform audio for .NET (Avalonia has no audio API)

| Library | NuGet / version (checked) | License | Platforms | Notes |
|---|---|---|---|---|
| **SoundFlow** | SoundFlow 1.4.1 (2026-05-11), net8.0 | MIT (LSXPrime/SoundFlow) | Win, macOS, Linux, Android, iOS, FreeBSD | Uses the **miniaudio** backend (natives bundled). High-level API and easy to push generated PCM. **Good first choice.** |
| **Silk.NET.OpenAL** | 2.23.0 (2026-01-23) | MIT | All desktops | Needs the OpenAL Soft native, `Silk.NET.OpenAL.Soft.Native` 1.23.1, which is **LGPL-2.0-or-later**. Dynamic linking is fine, but ship the LGPL notice. Silk.NET 2.x targets net5/netstandard and runs on net10. |
| OpenTK.Audio.OpenAL | 4.9.4 (2025-03-17) | MIT | Desktop | Same OpenAL Soft native requirement. |
| **SDL3-CS** (edwardgushchin) | SDL3-CS 3.4.18 (2026-10-03), includes net10.0 | Zlib (repo LICENSE); SDL3 itself is zlib | All desktops | SDL audio streams. Heavier if used only for audio. |
| Silk.NET.SDL (SDL2) | 2.23.0 | MIT (bindings); SDL2 is zlib | All desktops | |
| NAudio | 3.1.0 (2026-09-07), net9.0 | MIT | **v3:** `NAudio.Core` is cross-platform. Playback backends are WASAPI/WaveOut/DirectSound/ASIO on Windows and **ALSA on Linux (NAudio.Alsa)**. **No macOS playback backend is listed.** | Fine on Windows and Linux; not a complete cross-platform answer. |
| OwnAudioSharp | 4.0.8, net10.0 | MIT (nuspec) | not verified | Not evaluated further. |

---

## 4. Per-user data paths in .NET (`Environment.GetFolderPath`)

Verified in the dotnet/runtime source `Environment.GetFolderPathCore.Unix.cs` (`release/8.0` and `release/10.0` are identical for these entries) and the documented Windows behaviour.

| SpecialFolder | Windows | Linux | macOS (.NET 8+) |
|---|---|---|---|
| `ApplicationData` | `%APPDATA%` (`C:\Users\u\AppData\Roaming`) | `$XDG_CONFIG_HOME`, else `~/.config` | `~/Library/Application Support` (NSApplicationSupportDirectory) |
| `LocalApplicationData` | `%LOCALAPPDATA%` (`C:\Users\u\AppData\Local`) | `$XDG_DATA_HOME`, else `~/.local/share` | `~/Library/Application Support` (same as above) |
| `UserProfile` | `C:\Users\u` | `$HOME` | `$HOME` |

- macOS changed in .NET 8 (dotnet/runtime PR #68610, "Fix some incorrect SpecialFolder entries for Unix", 2022). Before that, macOS used the XDG-style `~/.config` and `~/.local/share`.
- **Recommendation:**
  - Saves and characters: `Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.Create), "AVATelengard")`. That resolves to `%LOCALAPPDATA%\AVATelengard`, `~/.local/share/AVATelengard` and `~/Library/Application Support/AVATelengard`.
  - Settings: optionally `ApplicationData`, which is `~/.config/AVATelengard` on Linux.
  - Call `Directory.CreateDirectory` before writing, and write atomically (temp file, then `File.Move(overwrite:true)`).
  - For Linux sandboxed packages (Flatpak/Snap), XDG variables are remapped automatically.

---

## Recommended pins

```xml
<TargetFramework>net10.0</TargetFramework>
<PackageReference Include="Avalonia" Version="12.1.3" />
<PackageReference Include="Avalonia.Desktop" Version="12.1.3" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="12.1.3" />
<PackageReference Include="Avalonia.Fonts.Inter" Version="12.1.3" />   <!-- optional -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.2" />
<!-- optional, debug-only, proprietary: AvaloniaUI.DiagnosticsSupport 2.2.3 -->
<!-- audio (pick one): SoundFlow 1.4.1 (MIT, miniaudio) -->
<!-- tests -->
<PackageReference Include="Avalonia.Headless.XUnit" Version="12.1.3" />  <!-- needs xUnit v3 -->
<PackageReference Include="xunit.v3" Version="4.0.1" />                 <!-- verify; fallback latest 3.x -->
<PackageReference Include="xunit.runner.visualstudio" Version="4.0.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.10.1" />
```

Fonts: Press Start 2P or Sixtyfour for the dungeon view, VT323 for text. All are OFL 1.1; ship OFL.txt.
