namespace Telengard.Core.Randomness;

/// <summary>
/// Source of gameplay randomness. Injected so that tests and simulations are reproducible.
/// The dungeon map is NOT drawn from this source; it is derived from coordinates.
/// </summary>
public interface IRandomSource
{
    /// <summary>Returns a value in [0, 1), the domain of BASIC's RND(1).</summary>
    double NextDouble();

    /// <summary>Serializable generator state, so saves can resume the exact sequence.</summary>
    ulong State { get; set; }
}
