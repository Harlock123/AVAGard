namespace Telengard.Core.Numerics;

/// <summary>
/// Bit-level model of Commodore 64 BASIC V2 floating point (Microsoft Binary Format, 40-bit),
/// as needed to reproduce the reference dungeon map exactly.
///
/// The accumulator (FAC) holds an excess-128 exponent, a 32-bit mantissa (top bit set when
/// normalised), a sign, and an 8-bit extension ("rounding") byte. Values loaded from memory or
/// pushed as a left operand have no extension byte. Behaviour reproduced here (see
/// docs/research/dungeon-generation.md for the evidence and ROM addresses):
///  * the left operand of every binary operator is rounded to 32 bits before it is pushed;
///  * addition aligns the smaller operand, truncating bits shifted below the extension byte;
///  * multiplication keeps 40 result bits (truncating) and normalises;
///  * INT truncates the 32-bit mantissa; assignment to an integer variable rounds, then truncates.
/// Only the non-negative paths needed by the map formula are exercised by tests.
/// </summary>
public readonly record struct CbmFloat(int Exponent, uint Mantissa, bool Negative, byte Extension)
{
    public static readonly CbmFloat Zero = default;

    public bool IsZero => Exponent == 0;

    public static CbmFloat FromPacked(byte e, byte m1, byte m2, byte m3, byte m4) =>
        e == 0 ? Zero : new CbmFloat(e, ((uint)(m1 | 0x80) << 24) | ((uint)m2 << 16) | ((uint)m3 << 8) | m4, (m1 & 0x80) != 0, 0);

    public static CbmFloat FromInt(long n)
    {
        if (n == 0) return Zero;
        bool neg = n < 0;
        ulong a = (ulong)Math.Abs(n);
        int bits = 64 - System.Numerics.BitOperations.LeadingZeroCount(a);
        if (bits > 32) throw new OverflowException("Integer too large for a 32-bit mantissa.");
        return new CbmFloat(128 + bits, (uint)(a << (32 - bits)), neg, 0);
    }

    public double ToDouble()
    {
        if (IsZero) return 0.0;
        double v = (Mantissa * 256.0 + Extension) / Math.Pow(2, 40) * Math.Pow(2, Exponent - 128);
        return Negative ? -v : v;
    }

    /// <summary>ROM ROUND: fold the extension byte into the 32-bit mantissa.</summary>
    public CbmFloat Round()
    {
        if (IsZero) return this;
        int e = Exponent;
        uint m = Mantissa;
        int ov = (Extension << 1) & 0xFF;
        if ((Extension & 0x80) != 0)
        {
            m++;
            if (m == 0)
            {
                m = 0x80000000;
                e++;
                ov >>= 1;
            }
        }
        return new CbmFloat(e, m, Negative, (byte)ov);
    }

    /// <summary>What FRMEVL pushes for a pending binary operator: rounded, no extension byte.</summary>
    public CbmFloat Pushed()
    {
        var r = Round();
        return r with { Extension = 0 };
    }

    /// <summary>Store to a floating-point variable (round, pack, reload).</summary>
    public CbmFloat Stored() => Pushed();

    private static CbmFloat Normalise(ulong m40, int e, bool neg)
    {
        if (m40 == 0) return Zero;
        int shift = 0;
        while ((m40 & (1UL << 39)) == 0)
        {
            m40 = (m40 << 1) & 0xFF_FFFF_FFFFUL;
            shift++;
        }
        if (shift >= e) return Zero;
        return new CbmFloat(e - shift, (uint)(m40 >> 8), neg, (byte)(m40 & 0xFF));
    }

    /// <summary>ARG + FAC, where <paramref name="arg"/> has already been pushed.</summary>
    public static CbmFloat Add(CbmFloat arg, CbmFloat fac)
    {
        if (fac.IsZero) return arg with { Extension = 0 };
        if (arg.IsZero) return fac;
        ulong a40 = (ulong)arg.Mantissa << 8;
        ulong f40 = ((ulong)fac.Mantissa << 8) | fac.Extension;
        int e;
        ulong big, small;
        bool bigNeg;
        if (arg.Exponent > fac.Exponent)
        {
            int d = arg.Exponent - fac.Exponent;
            f40 = d >= 64 ? 0 : f40 >> d;
            e = arg.Exponent; big = a40; small = f40; bigNeg = arg.Negative;
        }
        else
        {
            int d = fac.Exponent - arg.Exponent;
            a40 = d >= 64 ? 0 : a40 >> d;
            e = fac.Exponent; big = f40; small = a40; bigNeg = fac.Negative;
        }

        if (arg.Negative == fac.Negative)
        {
            ulong r = big + small;
            if ((r >> 40) != 0)
            {
                r >>= 1;
                e++;
            }
            return Normalise(r, e, bigNeg);
        }

        if (big >= small) return Normalise(big - small, e, bigNeg);
        return Normalise(small - big, e, !bigNeg);
    }

    /// <summary>ARG - FAC.</summary>
    public static CbmFloat Subtract(CbmFloat arg, CbmFloat fac) =>
        Add(arg, fac.IsZero ? fac : fac with { Negative = !fac.Negative });

    /// <summary>ARG * FAC: shift-and-add over FAC's 40 bits (extension byte first).</summary>
    public static CbmFloat Multiply(CbmFloat arg, CbmFloat fac)
    {
        if (fac.IsZero || arg.IsZero) return Zero;
        int e = arg.Exponent + fac.Exponent - 128;
        if (e <= 0) return Zero;
        ulong res = 0;
        uint ext = 0;
        ulong multiplier = ((ulong)fac.Mantissa << 8) | fac.Extension;
        for (int i = 0; i < 40; i++)
        {
            ulong carry = 0;
            if (((multiplier >> i) & 1) != 0)
            {
                res += arg.Mantissa;
                carry = res >> 32;
                res &= 0xFFFF_FFFFUL;
            }
            ext = (ext >> 1) | (uint)((res & 1) << 7);
            res = (res >> 1) | (carry << 31);
        }
        return Normalise((res << 8) | ext, e, arg.Negative ^ fac.Negative);
    }

    /// <summary>BASIC INT() (floor). Ignores and clears the extension byte.</summary>
    public CbmFloat Int()
    {
        if (IsZero) return Zero;
        if (Exponent >= 0xA0) return this;
        int sh = 0xA0 - Exponent;
        uint n = sh < 32 ? Mantissa >> sh : 0;
        if (!Negative) return FromInt(n);
        bool fractional = (sh < 32 ? (Mantissa & ((1u << sh) - 1)) != 0 : Mantissa != 0) || Extension != 0;
        return FromInt(-(long)(fractional ? n + 1 : n));
    }

    /// <summary>Assignment to an integer variable (e.g. H%=...): ROUND, then truncate toward zero.</summary>
    public int ToInt16Variable()
    {
        var r = Round();
        if (r.IsZero) return 0;
        if (r.Exponent >= 0x90) throw new OverflowException("?ILLEGAL QUANTITY");
        int sh = 0xA0 - r.Exponent;
        int n = sh >= 32 ? 0 : (int)(r.Mantissa >> sh); // C# masks shift counts; |value| < 1 must give 0
        return r.Negative ? -n : n;
    }

    public override string ToString() =>
        $"CbmFloat(e={Exponent:X2} m={Mantissa:X8} s={(Negative ? 1 : 0)} ov={Extension:X2} ~{ToDouble():R})";
}
