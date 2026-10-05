using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Telengard.Core.Dungeon;
using Telengard.Core.Model;
using System.Globalization;

namespace Telengard.Desktop.Controls;

/// <summary>
/// Modern-mode aid (not in the original): a map of rooms you have stood in on the current
/// level, centred on the hero. Unvisited rooms stay dark.
/// </summary>
public sealed class AutomapView : Control
{
    public static readonly StyledProperty<DungeonSnapshot?> SnapshotProperty =
        AvaloniaProperty.Register<AutomapView, DungeonSnapshot?>(nameof(Snapshot));

    public static readonly StyledProperty<bool> HighContrastProperty =
        AvaloniaProperty.Register<AutomapView, bool>(nameof(HighContrast));

    public static readonly StyledProperty<int> RadiusProperty =
        AvaloniaProperty.Register<AutomapView, int>(nameof(Radius), 7);

    static AutomapView() => AffectsRender<AutomapView>(SnapshotProperty, HighContrastProperty, RadiusProperty);

    public DungeonSnapshot? Snapshot { get => GetValue(SnapshotProperty); set => SetValue(SnapshotProperty, value); }
    public bool HighContrast { get => GetValue(HighContrastProperty); set => SetValue(HighContrastProperty, value); }
    public int Radius { get => GetValue(RadiusProperty); set => SetValue(RadiusProperty, value); }

    private static readonly Typeface Mono = new(new FontFamily("avares://Telengard/Assets/Fonts#VT323"));

    public override void Render(DrawingContext dc)
    {
        var pal = HighContrast ? Palette.HighContrast : Palette.Retro;
        var bounds = new Rect(Bounds.Size);
        dc.FillRectangle(pal.Fog, bounds);
        var snap = Snapshot;
        if (snap is null || snap.Explored is null || snap.AtInn) return;
        var map = DungeonMap.Shared;
        int n = Radius * 2 + 1;
        double cell = Math.Floor(Math.Min(bounds.Width, bounds.Height) / n);
        if (cell < 4) return;
        double ox = (bounds.Width - cell * n) / 2, oy = (bounds.Height - cell * n) / 2;
        var wallPen = new Pen(pal.Brick, Math.Max(1.5, cell * 0.14));
        var doorPen = new Pen(pal.Door, Math.Max(1, cell * 0.1));

        for (int dy = -Radius; dy <= Radius; dy++)
            for (int dx = -Radius; dx <= Radius; dx++)
            {
                int x = snap.X + dx, y = snap.Y + dy;
                if (!DungeonMap.InBounds(x, y, snap.Z) || !snap.Explored.Contains(GameState.PackRoom(x, y, snap.Z))) continue;
                double px = ox + (dx + Radius) * cell, py = oy + (dy + Radius) * cell;
                dc.FillRectangle(pal.Floor, new Rect(px, py, cell, cell));
                Side(dc, map.NorthBits(x, y, snap.Z), new Point(px, py), new Point(px + cell, py), wallPen, doorPen);
                Side(dc, map.SouthBits(x, y, snap.Z), new Point(px, py + cell), new Point(px + cell, py + cell), wallPen, doorPen);
                Side(dc, map.WestBits(x, y, snap.Z), new Point(px, py), new Point(px, py + cell), wallPen, doorPen);
                Side(dc, map.EastBits(x, y, snap.Z), new Point(px + cell, py), new Point(px + cell, py + cell), wallPen, doorPen);
                var f = map.Features(x, y, snap.Z);
                if (!f.IsEmpty && cell >= 10)
                {
                    var t = new FormattedText(Glyphs.Letter(f.Own, f.HasStairsUp, snap.Z), CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight, Mono, cell * 0.8, pal.Feature);
                    dc.DrawText(t, new Point(px + (cell - t.Width) / 2, py + (cell - t.Height) / 2));
                }
            }

        var c = new Point(ox + (Radius + 0.5) * cell, oy + (Radius + 0.5) * cell);
        dc.DrawEllipse(pal.Player, null, c, cell * 0.28, cell * 0.28);
    }

    private static void Side(DrawingContext dc, int bits, Point a, Point b, IPen wall, IPen door)
    {
        if (bits == 3) dc.DrawLine(wall, a, b);
        else if (bits == 2)
        {
            var third = (b - a) / 3;
            dc.DrawLine(wall, a, a + third);
            dc.DrawLine(wall, b - third, b);
            dc.DrawLine(door, a + third, b - third);
        }
    }
}
