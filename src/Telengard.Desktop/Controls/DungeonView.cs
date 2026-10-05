using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Telengard.Core.Content;
using Telengard.Core.Dungeon;

namespace Telengard.Desktop.Controls;

/// <summary>
/// Renders the 3×3 line-of-sight window around the hero (one custom-drawn control; no per-room
/// controls). The layout follows the original's idea: walls and doors only, the hero in the
/// centre room, the faced monster beside the hero, followers as markers on the right.
/// </summary>
public sealed class DungeonView : Control
{
    public static readonly StyledProperty<DungeonSnapshot?> SnapshotProperty =
        AvaloniaProperty.Register<DungeonView, DungeonSnapshot?>(nameof(Snapshot));

    public static readonly StyledProperty<bool> HighContrastProperty =
        AvaloniaProperty.Register<DungeonView, bool>(nameof(HighContrast));

    static DungeonView()
    {
        AffectsRender<DungeonView>(SnapshotProperty, HighContrastProperty);
    }

    public DungeonSnapshot? Snapshot
    {
        get => GetValue(SnapshotProperty);
        set => SetValue(SnapshotProperty, value);
    }

    public bool HighContrast
    {
        get => GetValue(HighContrastProperty);
        set => SetValue(HighContrastProperty, value);
    }

    private static readonly Typeface Mono = new(new FontFamily("avares://Telengard/Assets/Fonts#VT323"));

    public override void Render(DrawingContext dc)
    {
        var pal = HighContrast ? Palette.HighContrast : Palette.Retro;
        var bounds = new Rect(Bounds.Size);
        dc.FillRectangle(pal.Background, bounds);
        var snap = Snapshot;
        if (snap is null) return;

        double cell = Math.Floor(Math.Min(bounds.Width / 3.4, bounds.Height / 3.2));
        if (cell < 8) return;
        double ox = Math.Floor((bounds.Width - cell * 3) / 2 - cell * 0.15);
        double oy = Math.Floor((bounds.Height - cell * 3) / 2);
        Rect Room(int row, int col) => new(ox + col * cell, oy + row * cell, cell, cell);

        if (snap.AtInn)
        {
            DrawInn(dc, pal, Room(1, 1).Inflate(cell * 0.6), snap.InnName!);
            return;
        }

        if (snap.View is { } seen)
            foreach (var (row, col) in seen.VisibleRooms)
                dc.FillRectangle(pal.Floor, Room(row, col));

        double t = Math.Max(3, Math.Round(cell * 0.085));
        if (snap.View is { } view)
        {
            foreach (var w in view.Walls)
            {
                if (w.IsNorth && w.Col <= 2)
                    DrawWall(dc, pal, ox + w.Col * cell, oy + w.Row * cell - t / 2, cell, t, horizontal: true, w.Kind == WallKind.Door);
                else if (!w.IsNorth && w.Row <= 2)
                    DrawWall(dc, pal, ox + w.Col * cell - t / 2, oy + w.Row * cell, t, cell, horizontal: false, w.Kind == WallKind.Door);
            }

            foreach (var (row, col) in view.LitFeatures)
            {
                var r = Room(row, col).Deflate(cell * 0.38);
                dc.DrawRectangle(null, new Pen(pal.Lit, 2, new DashStyle([2, 2], 0)), r);
            }
        }

        // Centre room layout: hero on the left, the room's feature on the right (or small at the
        // bottom while a monster occupies the right half).
        var here = Room(1, 1);
        if (!snap.Features.IsEmpty)
        {
            var featureRect = snap.Monster is null
                ? new Rect(here.X + cell * 0.45, here.Y + cell * 0.18, cell * 0.5, cell * 0.64)
                : new Rect(here.X + cell * 0.08, here.Y + cell * 0.7, cell * 0.36, cell * 0.28);
            Glyphs.DrawFeature(dc, featureRect, snap.Features.Own, snap.Features.HasStairsUp, snap.Z, pal);
        }

        var heroRect = new Rect(here.X + cell * 0.05, here.Y + cell * 0.12, cell * 0.4, cell * 0.6);
        Glyphs.DrawHero(dc, heroRect, pal.Player, snap.Invisible ? 0.35 : 1.0);

        if (snap.Monster is { } m)
            DrawMonster(dc, pal, new Rect(here.X + cell * 0.46, here.Y + cell * 0.1, cell * 0.5, cell * 0.8), m.Type, m.Level);

        // Followers: one marker per occupied queue slot (nearest at the top), like the original's column.
        double fx = ox + cell * 3 + cell * 0.18;
        for (int i = 0; i < snap.FollowerDistances.Count; i++)
        {
            int d = snap.FollowerDistances[i];
            double fy = oy + (d - 1) * (cell * 3 / 20.0) + cell * 0.05;
            dc.DrawEllipse(pal.Living, null, new Point(fx, fy + cell * 0.06), cell * 0.06, cell * 0.06);
        }
    }

    private static void DrawWall(DrawingContext dc, Palette pal, double x, double y, double w, double h, bool horizontal, bool door)
    {
        var whole = new Rect(x, y, w, h);
        if (!door)
        {
            Bricks(dc, pal, whole, horizontal);
            return;
        }
        // A door: wall at both ends, an opening in the middle third with a frame line.
        if (horizontal)
        {
            Bricks(dc, pal, new Rect(x, y, w / 3, h), true);
            Bricks(dc, pal, new Rect(x + 2 * w / 3, y, w / 3, h), true);
            dc.DrawLine(new Pen(pal.Door, Math.Max(1.5, h / 3), new DashStyle([1.5, 1.5], 0)), new Point(x + w / 3, y + h / 2), new Point(x + 2 * w / 3, y + h / 2));
        }
        else
        {
            Bricks(dc, pal, new Rect(x, y, w, h / 3), false);
            Bricks(dc, pal, new Rect(x, y + 2 * h / 3, w, h / 3), false);
            dc.DrawLine(new Pen(pal.Door, Math.Max(1.5, w / 3), new DashStyle([1.5, 1.5], 0)), new Point(x + w / 2, y + h / 3), new Point(x + w / 2, y + 2 * h / 3));
        }
    }

    private static void Bricks(DrawingContext dc, Palette pal, Rect r, bool horizontal)
    {
        dc.FillRectangle(pal.Brick, r);
        var mortar = new Pen(pal.Mortar, 1);
        double brick = Math.Max(6, (horizontal ? r.Height : r.Width) * 2.2);
        if (horizontal)
        {
            dc.DrawLine(mortar, new Point(r.Left, r.Center.Y), new Point(r.Right, r.Center.Y));
            for (double bx = r.Left + brick; bx < r.Right; bx += brick)
                dc.DrawLine(mortar, new Point(bx, r.Top), new Point(bx, r.Center.Y));
            for (double bx = r.Left + brick / 2; bx < r.Right; bx += brick)
                dc.DrawLine(mortar, new Point(bx, r.Center.Y), new Point(bx, r.Bottom));
        }
        else
        {
            dc.DrawLine(mortar, new Point(r.Center.X, r.Top), new Point(r.Center.X, r.Bottom));
            for (double by = r.Top + brick; by < r.Bottom; by += brick)
                dc.DrawLine(mortar, new Point(r.Left, by), new Point(r.Center.X, by));
            for (double by = r.Top + brick / 2; by < r.Bottom; by += brick)
                dc.DrawLine(mortar, new Point(r.Center.X, by), new Point(r.Right, by));
        }
    }

    private static void DrawMonster(DrawingContext dc, Palette pal, Rect r, int type, int level)
    {
        bool undead = Monsters.IsUndead(type);
        var brush = undead ? pal.Undead : pal.Living;
        double s = Math.Min(r.Width, r.Height);
        var c = new Point(r.Center.X, r.Y + s * 0.42);
        // Undead are drawn with a square badge, the living with a round one (shape as well as colour).
        if (undead)
            dc.DrawRectangle(new SolidColorBrush(Colors.Black, 0.5), new Pen(brush, Math.Max(2, s * 0.06)), new Rect(c.X - s * .36, c.Y - s * .36, s * .72, s * .72), s * .08, s * .08);
        else
            dc.DrawEllipse(new SolidColorBrush(Colors.Black, 0.5), new Pen(brush, Math.Max(2, s * 0.06)), c, s * .38, s * .38);
        var name = Monsters.Name(type);
        var letter = new FormattedText(Monsters.Code(type), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Mono, s * 0.5, brush);
        dc.DrawText(letter, new Point(c.X - letter.Width / 2, c.Y - letter.Height / 2));
        var label = new FormattedText($"{name} L{level}", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Mono, Math.Max(11, s * 0.2), pal.Text);
        dc.DrawText(label, new Point(r.Center.X - label.Width / 2, r.Bottom - label.Height * 0.9));
    }

    private static void DrawInn(DrawingContext dc, Palette pal, Rect r, string name)
    {
        var pen = new Pen(pal.Feature, Math.Max(2, r.Width * 0.015));
        double w = r.Width, h = r.Height;
        var body = new Rect(r.X + w * .2, r.Y + h * .4, w * .6, h * .45);
        dc.FillRectangle(pal.Mortar, body);
        dc.DrawRectangle(null, pen, body);
        var roof = new StreamGeometry();
        using (var g = roof.Open())
        {
            g.BeginFigure(new Point(r.X + w * .12, r.Y + h * .42), true);
            g.LineTo(new Point(r.Center.X, r.Y + h * .12));
            g.LineTo(new Point(r.X + w * .88, r.Y + h * .42));
            g.EndFigure(true);
        }
        dc.DrawGeometry(pal.Brick, pen, roof);
        dc.FillRectangle(pal.Feature, new Rect(r.Center.X - w * .06, r.Y + h * .62, w * .12, h * .23));
        dc.FillRectangle(pal.FeatureAccent, new Rect(r.X + w * .28, r.Y + h * .5, w * .1, h * .08));
        dc.FillRectangle(pal.FeatureAccent, new Rect(r.X + w * .62, r.Y + h * .5, w * .1, h * .08));
        var text = new FormattedText(name, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Mono, Math.Max(14, w * 0.075), pal.Text);
        dc.DrawText(text, new Point(r.Center.X - text.Width / 2, r.Bottom - text.Height * 0.6));
    }
}
