using Telengard.Core.Numerics;

namespace Telengard.Core.Dungeon;

/// <summary>
/// Computes the 16-bit "room word" for a dungeon coordinate exactly as the reference
/// Commodore 64 release (V4.18) does. The map is never stored: every room is derived
/// from its coordinates, so the dungeon is identical in every game.
///
/// Room word layout: bits 0-1 north wall, bits 2-3 west wall (0/1 open, 2 door, 3 solid),
/// bits 8+ raw feature code (0 = none). Coordinates 1 and 201 force solid border walls.
/// </summary>
public static class RoomWordGenerator
{
    // The three coordinate weights and multiplier, as packed by the C64 when it parses
    // the decimal constants 1.6915, 1.4278 and 1.2462 (bytes verified on the real ROM).
    private static readonly CbmFloat Xo = CbmFloat.FromPacked(0x81, 0x58, 0x83, 0x12, 0x6E);
    private static readonly CbmFloat Yo = CbmFloat.FromPacked(0x81, 0x36, 0xC2, 0x26, 0x82);
    private static readonly CbmFloat Zo = CbmFloat.FromPacked(0x81, 0x1F, 0x83, 0x7B, 0x4A);
    private static readonly CbmFloat W0 = CbmFloat.FromInt(4694);
    private static readonly CbmFloat Ten = CbmFloat.FromInt(10);
    private static readonly CbmFloat Fifteen = CbmFloat.FromInt(15);
    private static readonly CbmFloat One = CbmFloat.FromInt(1);

    public static int Compute(int x, int y, int z)
    {
        var fx = CbmFloat.FromInt(x);
        var fy = CbmFloat.FromInt(y);
        var fz = CbmFloat.FromInt(z);

        // q = x*XO + y*YO + z*ZO + (x+XO)*(y+YO)*(z+ZO), evaluated left to right.
        var t = CbmFloat.Multiply(fx.Pushed(), Xo);
        t = CbmFloat.Add(t.Pushed(), CbmFloat.Multiply(fy.Pushed(), Yo));
        t = CbmFloat.Add(t.Pushed(), CbmFloat.Multiply(fz.Pushed(), Zo));
        var a = CbmFloat.Add(fx.Pushed(), Xo);
        var b = CbmFloat.Add(fy.Pushed(), Yo);
        var ab = CbmFloat.Multiply(a.Pushed(), b);
        var c = CbmFloat.Add(fz.Pushed(), Zo);
        var abc = CbmFloat.Multiply(ab.Pushed(), c);
        var q = CbmFloat.Add(t.Pushed(), abc).Stored();

        // h = (q - INT(q)) * 4694, stored to an integer variable.
        var fraction = CbmFloat.Subtract(q.Pushed(), q.Int());
        int h = CbmFloat.Multiply(fraction.Pushed(), W0).ToInt16Variable();
        if (h >> 8 > 5) h &= 0xFF;

        if (h >> 8 > 0)
        {
            // feature = INT((q*10 - INT(q*10)) * 15 + 1); the subtraction mixes a rounded and an
            // unrounded product, so the result can (rarely) be 16.
            var p = CbmFloat.Multiply(q.Pushed(), Ten);
            var fraction2 = CbmFloat.Subtract(p.Pushed(), CbmFloat.Multiply(q.Pushed(), Ten).Int());
            var v = CbmFloat.Add(CbmFloat.Multiply(fraction2.Pushed(), Fifteen).Pushed(), One);
            int feature = v.Int().ToInt16Variable();
            h = (feature << 8) | (h & 0xFF);
        }

        if (x == 1 || x == 201) h |= 12;
        if (y == 1 || y == 201) h |= 3;
        return h;
    }
}
