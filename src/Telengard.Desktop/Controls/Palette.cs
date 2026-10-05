using Avalonia.Media;

namespace Telengard.Desktop.Controls;

/// <summary>Colours for the dungeon views. Information is never conveyed by colour alone.</summary>
public sealed record Palette(
    IBrush Background, IBrush Floor, IBrush Brick, IBrush Mortar, IBrush Door, IBrush Player, IBrush Living,
    IBrush Undead, IBrush Feature, IBrush FeatureAccent, IBrush Text, IBrush Fog, IBrush Lit)
{
    public static readonly Palette Retro = new(
        Background: new SolidColorBrush(Color.Parse("#0B0B14")),
        Floor: new SolidColorBrush(Color.Parse("#14142A")),
        Brick: new SolidColorBrush(Color.Parse("#9C4A33")),
        Mortar: new SolidColorBrush(Color.Parse("#5E2A1D")),
        Door: new SolidColorBrush(Color.Parse("#D9A066")),
        Player: new SolidColorBrush(Color.Parse("#A5A1FF")),
        Living: new SolidColorBrush(Color.Parse("#E07A4F")),
        Undead: new SolidColorBrush(Color.Parse("#9FD3A0")),
        Feature: new SolidColorBrush(Color.Parse("#EDE65F")),
        FeatureAccent: new SolidColorBrush(Color.Parse("#6ABFD9")),
        Text: new SolidColorBrush(Color.Parse("#E8E6D9")),
        Fog: new SolidColorBrush(Color.Parse("#07070C")),
        Lit: new SolidColorBrush(Color.Parse("#F2F2F2")));

    public static readonly Palette HighContrast = new(
        Background: Brushes.Black,
        Floor: new SolidColorBrush(Color.Parse("#101010")),
        Brick: Brushes.White,
        Mortar: new SolidColorBrush(Color.Parse("#808080")),
        Door: Brushes.Yellow,
        Player: Brushes.Cyan,
        Living: new SolidColorBrush(Color.Parse("#FF8C00")),
        Undead: new SolidColorBrush(Color.Parse("#7CFC00")),
        Feature: Brushes.Yellow,
        FeatureAccent: Brushes.Cyan,
        Text: Brushes.White,
        Fog: Brushes.Black,
        Lit: Brushes.White);
}
