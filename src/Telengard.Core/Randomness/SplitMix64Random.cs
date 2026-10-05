namespace Telengard.Core.Randomness;

/// <summary>
/// Small, fully serializable PRNG (SplitMix64). This is a modern substitute for the
/// original machine's RND; gameplay randomness is not claimed to be sequence-identical.
/// </summary>
public sealed class SplitMix64Random(ulong seed) : IRandomSource
{
    public ulong State { get; set; } = seed;

    public double NextDouble() => (Next() >> 11) * (1.0 / (1UL << 53));

    private ulong Next()
    {
        ulong z = State += 0x9E3779B97F4A7C15UL;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
}
