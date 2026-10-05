using Avalonia;
using Avalonia.Media;
using Telengard.Core.Dungeon;

namespace Telengard.Desktop.Controls;

/// <summary>Simple vector glyphs drawn for this project (no original artwork is used).</summary>
public static class Glyphs
{
    public static string Letter(FeatureKind f, bool upStairs, int z) => f switch
    {
        _ when upStairs && f == FeatureKind.StairsDown => "⇕",
        _ when upStairs => "↑",
        FeatureKind.InnStairsOrElevator => z == 1 ? "I" : "E",
        FeatureKind.Pit => "P",
        FeatureKind.Teleporter => "T",
        FeatureKind.StairsDown => "↓",
        FeatureKind.Altar => "A",
        FeatureKind.Fountain => "F",
        FeatureKind.GrayMistyCube => "C",
        FeatureKind.Throne => "H",
        FeatureKind.Box => "B",
        _ => "",
    };

    public static string Name(FeatureKind f, bool upStairs, int z) => f switch
    {
        _ when upStairs && f == FeatureKind.StairsDown => "Stairway up and down",
        _ when upStairs => "Stairway up",
        FeatureKind.InnStairsOrElevator => z == 1 ? "Stairway up to an inn" : "Elevator (up one level)",
        FeatureKind.Pit => "Pit",
        FeatureKind.Teleporter => "Teleporter",
        FeatureKind.StairsDown => "Stairway down",
        FeatureKind.Altar => "Altar",
        FeatureKind.Fountain => "Fountain",
        FeatureKind.GrayMistyCube => "Gray misty cube",
        FeatureKind.Throne => "Throne",
        FeatureKind.Box => "Small box",
        _ => "",
    };

    /// <summary>Draws a feature icon centred in <paramref name="r"/>.</summary>
    public static void DrawFeature(DrawingContext dc, Rect r, FeatureKind f, bool upStairs, int z, Palette pal)
    {
        var pen = new Pen(pal.Feature, Math.Max(1.5, r.Width * 0.05));
        var accent = new Pen(pal.FeatureAccent, Math.Max(1.5, r.Width * 0.05));
        double cx = r.Center.X, cy = r.Center.Y, s = Math.Min(r.Width, r.Height) * 0.32;

        bool innStairs = f == FeatureKind.InnStairsOrElevator && z == 1;
        if (upStairs || innStairs || f == FeatureKind.StairsDown)
        {
            for (int i = 0; i < 4; i++)
            {
                double y = cy - s + i * s / 2;
                double x0 = cx - s + i * s / 2;
                dc.DrawLine(pen, new Point(x0, y), new Point(x0, y + s / 2));
                dc.DrawLine(pen, new Point(x0, y + s / 2), new Point(x0 + s / 2, y + s / 2));
            }
            if (upStairs || innStairs) Arrow(dc, accent, cx + s * 0.9, cy, s * 0.6, up: true);
            if (f == FeatureKind.StairsDown) Arrow(dc, accent, cx - s * 0.9, cy, s * 0.6, up: false);
            return;
        }

        switch (f)
        {
            case FeatureKind.InnStairsOrElevator:
                dc.DrawRectangle(null, pen, new Rect(cx - s * .7, cy - s, s * 1.4, s * 2));
                Arrow(dc, accent, cx, cy, s * 0.8, up: true);
                break;
            case FeatureKind.Pit:
                dc.DrawEllipse(Brushes.Black, pen, new Point(cx, cy), s, s * 0.55);
                dc.DrawEllipse(null, accent, new Point(cx, cy), s * 0.6, s * 0.3);
                break;
            case FeatureKind.Teleporter:
                for (int i = 1; i <= 3; i++) dc.DrawEllipse(null, i % 2 == 0 ? accent : pen, new Point(cx, cy), s * i / 3, s * i / 3);
                break;
            case FeatureKind.Altar:
                dc.DrawRectangle(null, pen, new Rect(cx - s, cy, s * 2, s * 0.7));
                dc.DrawLine(accent, new Point(cx, cy - s), new Point(cx, cy));
                dc.DrawLine(accent, new Point(cx - s * .4, cy - s * .65), new Point(cx + s * .4, cy - s * .65));
                break;
            case FeatureKind.Fountain:
                dc.DrawEllipse(null, pen, new Point(cx, cy + s * .5), s, s * .35);
                dc.DrawLine(accent, new Point(cx, cy + s * .4), new Point(cx, cy - s * .7));
                dc.DrawEllipse(null, accent, new Point(cx, cy - s * .8), s * .25, s * .2);
                break;
            case FeatureKind.GrayMistyCube:
                var mist = new SolidColorBrush(Colors.LightGray, 0.35);
                dc.DrawRectangle(mist, new Pen(Brushes.LightGray, pen.Thickness), new Rect(cx - s, cy - s, s * 2, s * 2));
                dc.DrawLine(new Pen(Brushes.LightGray, 1), new Point(cx - s, cy - s), new Point(cx - s * .5, cy - s * 1.4));
                dc.DrawLine(new Pen(Brushes.LightGray, 1), new Point(cx + s, cy - s), new Point(cx + s * 1.5, cy - s * 1.4));
                break;
            case FeatureKind.Throne:
                dc.DrawRectangle(null, pen, new Rect(cx - s * .6, cy - s, s * 1.2, s * 1.1));
                dc.DrawRectangle(null, pen, new Rect(cx - s * .9, cy + s * .1, s * 1.8, s * .5));
                dc.DrawLine(pen, new Point(cx - s * .8, cy + s * .6), new Point(cx - s * .8, cy + s));
                dc.DrawLine(pen, new Point(cx + s * .8, cy + s * .6), new Point(cx + s * .8, cy + s));
                dc.DrawEllipse(pal.FeatureAccent, null, new Point(cx, cy - s * .6), s * .15, s * .15);
                break;
            case FeatureKind.Box:
                dc.DrawRectangle(null, pen, new Rect(cx - s * .8, cy - s * .5, s * 1.6, s));
                IBrush[] lights = [Brushes.Red, Brushes.Yellow, Brushes.LimeGreen, Brushes.DodgerBlue];
                for (int i = 0; i < 4; i++)
                    dc.DrawEllipse(lights[i], null, new Point(cx - s * .55 + i * s * .37, cy), s * .12, s * .12);
                break;
        }
    }

    private static void Arrow(DrawingContext dc, IPen pen, double x, double y, double len, bool up)
    {
        double d = up ? -1 : 1;
        dc.DrawLine(pen, new Point(x, y - d * len / 2), new Point(x, y + d * len / 2));
        dc.DrawLine(pen, new Point(x, y + d * len / 2), new Point(x - len / 3, y + d * len / 6));
        dc.DrawLine(pen, new Point(x, y + d * len / 2), new Point(x + len / 3, y + d * len / 6));
    }

    /// <summary>A small adventurer figure.</summary>
    public static void DrawHero(DrawingContext dc, Rect r, IBrush brush, double opacity)
    {
        using var _ = dc.PushOpacity(opacity);
        double s = Math.Min(r.Width, r.Height);
        var pen = new Pen(brush, Math.Max(2, s * 0.08), lineCap: PenLineCap.Round);
        var c = r.Center;
        dc.DrawEllipse(brush, null, new Point(c.X, c.Y - s * .28), s * .12, s * .12);
        dc.DrawLine(pen, new Point(c.X, c.Y - s * .15), new Point(c.X, c.Y + s * .12));
        dc.DrawLine(pen, new Point(c.X, c.Y + s * .12), new Point(c.X - s * .14, c.Y + s * .36));
        dc.DrawLine(pen, new Point(c.X, c.Y + s * .12), new Point(c.X + s * .14, c.Y + s * .36));
        dc.DrawLine(pen, new Point(c.X - s * .18, c.Y - s * .05), new Point(c.X + s * .18, c.Y - s * .05));
        dc.DrawLine(new Pen(Brushes.Silver, Math.Max(1.5, s * .05)), new Point(c.X + s * .18, c.Y - s * .05), new Point(c.X + s * .3, c.Y - s * .35));
    }
}
