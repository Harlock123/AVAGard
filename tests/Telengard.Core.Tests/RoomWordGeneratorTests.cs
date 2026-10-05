using System.IO.Compression;
using System.Security.Cryptography;
using Telengard.Core.Dungeon;

namespace Telengard.Core.Tests;

/// <summary>
/// Golden tests for the coordinate-derived map. Vectors come from two independent sources
/// (see docs/research/dungeon-generation.md):
///  * rom-*.bin: H% values produced by the genuine C64 BASIC ROM running the original formula;
///  * model-*: a bit-level Python model of the ROM float routines, itself checked against the ROM.
/// Both are little-endian uint16, row-major with Y outer.
/// </summary>
public class RoomWordGeneratorTests
{
    private static string Golden(string name) => Path.Combine(AppContext.BaseDirectory, "Golden", name);

    [Fact]
    public void StartRoomIsStairsUpToTheInn()
    {
        Assert.Equal(0x1F0, RoomWordGenerator.Compute(25, 13, 1));
    }

    [Fact]
    public void FeatureValueSixteenEdgeCaseMatchesRom()
    {
        // The real ROM yields feature 16 here because INT() is applied to an unrounded product.
        Assert.Equal(0x1080, RoomWordGenerator.Compute(90, 40, 50));
    }

    [Fact]
    public void MatchesRealRomOutputForStartArea()
    {
        var data = File.ReadAllBytes(Golden("rom-z01-y00-14-x00-40.bin"));
        int mismatches = 0;
        for (int y = 0; y <= 14; y++)
            for (int x = 0; x <= 40; x++)
            {
                int expected = BitConverter.ToUInt16(data, ((y * 41) + x) * 2);
                if (RoomWordGenerator.Compute(x, y, 1) != expected) mismatches++;
            }
        Assert.Equal(0, mismatches);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public void MatchesBitLevelModelForWholeLevel(int z)
    {
        using var gz = new GZipStream(File.OpenRead(Golden($"model-z{z:00}.bin.gz")), CompressionMode.Decompress);
        using var ms = new MemoryStream();
        gz.CopyTo(ms);
        var data = ms.ToArray();
        var bad = new List<string>();
        for (int y = 0; y <= 201; y++)
            for (int x = 0; x <= 201; x++)
            {
                int expected = BitConverter.ToUInt16(data, ((y * 202) + x) * 2);
                int actual = RoomWordGenerator.Compute(x, y, z);
                if (actual != expected && bad.Count < 10) bad.Add($"({x},{y},{z}) expected {expected:X4} got {actual:X4}");
            }
        Assert.Empty(bad);
    }

    [Fact]
    public void AllLevelsMatchModelHashes()
    {
        var lines = File.ReadAllLines(Golden("model-map-sha256.txt"));
        var buffer = new byte[202 * 202 * 2];
        foreach (var line in lines.Where(l => l.Length > 0))
        {
            var parts = line.Split(' ');
            int z = int.Parse(parts[0]);
            for (int y = 0; y <= 201; y++)
                for (int x = 0; x <= 201; x++)
                    BitConverter.TryWriteBytes(buffer.AsSpan(((y * 202) + x) * 2), (ushort)RoomWordGenerator.Compute(x, y, z));
            Assert.Equal(parts[1], Convert.ToHexStringLower(SHA256.HashData(buffer)));
        }
    }
}
