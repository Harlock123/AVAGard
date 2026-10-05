# Third-party notices

## Shipped with the application

| Component | Version | License | Use | Source |
|---|---|---|---|---|
| Avalonia, Avalonia.Desktop, Avalonia.Themes.Fluent, Avalonia.Skia, Avalonia.HarfBuzz, Avalonia.Win32, Avalonia.X11, Avalonia.Native, Avalonia.FreeDesktop(.AtSpi), Avalonia.Remote.Protocol | 12.1.3 | MIT | UI framework | https://github.com/AvaloniaUI/Avalonia |
| Avalonia.Fonts.Inter (Inter typeface) | 12.1.3 | MIT (package); Inter font under SIL OFL 1.1 | UI font | https://github.com/AvaloniaUI/Avalonia, https://rsms.me/inter |
| Avalonia.Angle.Windows.Natives | 2.1.27548.20260419 | BSD-3-Clause style (ANGLE Project Authors) | Windows GPU backend | https://github.com/AvaloniaUI/angle |
| SkiaSharp (+ NativeAssets) | 3.119.4 | MIT (Skia: BSD-3-Clause) | Rendering | https://github.com/mono/SkiaSharp |
| HarfBuzzSharp (+ NativeAssets) | 8.3.1.3 | MIT (HarfBuzz: MIT/"Old MIT") | Text shaping | https://github.com/mono/SkiaSharp |
| MicroCom.Runtime | 0.11.6 | MIT | Avalonia interop | https://github.com/kekekeks/MicroCom |
| Tmds.DBus.Protocol | 0.94.1 | MIT | Linux desktop integration | https://github.com/tmds/Tmds.DBus |
| CommunityToolkit.Mvvm | 8.4.2 | MIT | MVVM helpers | https://github.com/CommunityToolkit/dotnet |
| **VT323** font (`src/Telengard.Desktop/Assets/Fonts/VT323-Regular.ttf`) | google/fonts commit 7085eb8 | **SIL Open Font License 1.1**; reserved font name none listed; © 2011 The VT323 Project Authors (Peter Hull) | Retro display font | https://github.com/google/fonts/tree/main/ofl/vt323 (license text: `Assets/Fonts/VT323-OFL.txt`, shipped alongside) |
| .NET runtime (self-contained builds) | 10.0.x | MIT | Runtime | https://github.com/dotnet/runtime |

Obligations: keep the copyright and license notices of the MIT/BSD components with redistributed
binaries (the NuGet packages contain them), and ship the OFL text with the VT323 font (it is
embedded next to it). The OFL allows bundling with software, including commercial software, but the
font may not be sold on its own.

## Build and test only (not shipped)

| Component | Version | License |
|---|---|---|
| Avalonia.BuildServices (build telemetry; opt out with `AVALONIA_TELEMETRY_OPTOUT=1`) | 11.3.2 | MIT |
| xunit.v3, xunit.runner.visualstudio | 3.2.2 / 3.1.5 | Apache-2.0 |
| Microsoft.NET.Test.Sdk | 18.10.1 | MIT |
| Avalonia.Headless, Avalonia.Headless.XUnit | 12.1.3 | MIT |

## Telengard itself

*Telengard* © Avalon Hill / Microcomputer Games (1981–85); program by Daniel M. Lawrence. Avalon
Hill was acquired by Hasbro in 1998. This project is an independent reimplementation from documented
behaviour and is not affiliated with or endorsed by any rights holder. No original code, artwork,
music, sound or manual text is included. Short factual names (monster, spell and item names as
printed in the manual) are used to describe the gameplay. See docs/ASSET_PROVENANCE.md, and the
"Rights" sections of docs/research/history-and-manual.md, for what is and is not known about current
rights. **Nothing in this repository establishes that the project may be distributed publicly.**
