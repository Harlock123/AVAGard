using Telengard.Core.Randomness;

namespace Telengard.Core.Tests;

public class RandomSourceTests
{
    [Fact]
    public void SameSeedProducesSameSequence()
    {
        var a = new SplitMix64Random(42);
        var b = new SplitMix64Random(42);
        for (int i = 0; i < 100; i++)
            Assert.Equal(a.NextDouble(), b.NextDouble());
    }

    [Fact]
    public void RestoringStateResumesSequence()
    {
        var a = new SplitMix64Random(7);
        a.NextDouble();
        var saved = a.State;
        var expected = a.NextDouble();
        var b = new SplitMix64Random(0) { State = saved };
        Assert.Equal(expected, b.NextDouble());
    }

    [Fact]
    public void ValuesAreInUnitInterval()
    {
        var r = new SplitMix64Random(1);
        for (int i = 0; i < 10_000; i++)
        {
            var v = r.NextDouble();
            Assert.InRange(v, 0.0, 0.9999999999999999);
        }
    }
}
