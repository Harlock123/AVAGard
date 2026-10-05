using System.Text.Json.Nodes;
using Telengard.Core.Content;
using Telengard.Core.Engine;
using Telengard.Core.Model;
using Telengard.Core.Randomness;
using Telengard.Infrastructure.Saves;
using Telengard.Infrastructure.Settings;

namespace Telengard.Core.Tests;

public sealed class SaveStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "tg-tests-" + Guid.NewGuid().ToString("N"));
    private SaveStore Store => new(_dir);
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static GameState SampleState()
    {
        var rng = new SplitMix64Random(77);
        var s = CharacterCreation.NewGame("Svenja", CharacterCreation.RollStats(rng), rng);
        s.Character.Items[Items.Sword] = 3;
        s.Character.Timers[Effect.Light] = 7;
        s.Character.Gold = 1234;
        s.Character.Experience = 5678;
        s.Followers[2] = new QueuedMonster(20, 4, 33);
        s.Explored.Add(GameState.PackRoom(25, 13, 1));
        s.X = 60; s.Y = 70; s.Z = 9;
        s.RandomState = 0xDEADBEEF;
        return s;
    }

    [Fact]
    public async Task SaveAndLoadRoundTripsEveryField()
    {
        var state = SampleState();
        await Store.SaveAsync(state, SaveKind.Manual, RulesPolicy.Classic(), Ct);
        var loaded = await Store.LoadAsync(Store.PathFor("Svenja", SaveKind.Manual), Ct);

        Assert.Equal(Preset.Classic, loaded.File.Preset);
        var a = SaveStore.Wrap(state, SaveKind.Manual, RulesPolicy.Classic()).State.GetRawText();
        var b = SaveStore.Wrap(loaded.State, SaveKind.Manual, RulesPolicy.Classic()).State.GetRawText();
        Assert.Equal(a, b);
        Assert.Equal("SVENJA", loaded.State.Character.Name);
        Assert.Equal(new QueuedMonster(20, 4, 33), loaded.State.Followers[2]);
    }

    [Fact]
    public async Task ResumedGameContinuesTheSameRandomSequence()
    {
        var state = SampleState();
        var rng = new SplitMix64Random(state.RandomState);
        double expected = rng.NextDouble();
        await Store.SaveAsync(state, SaveKind.Manual, RulesPolicy.Classic(), Ct);
        var loaded = await Store.LoadAsync(Store.PathFor("Svenja", SaveKind.Manual), Ct);
        Assert.Equal(expected, new SplitMix64Random(loaded.State.RandomState).NextDouble());
    }

    [Fact]
    public async Task TamperedOrTruncatedFilesAreRejectedWithAClearMessage()
    {
        await Store.SaveAsync(SampleState(), SaveKind.Manual, RulesPolicy.Classic(), Ct);
        var path = Store.PathFor("Svenja", SaveKind.Manual);
        var json = await File.ReadAllTextAsync(path, Ct);

        var e1 = Assert.Throws<SaveLoadException>(() => SaveStore.Parse(json[..(json.Length / 2)]));
        Assert.Contains("damaged", e1.Message);

        var node = JsonNode.Parse(json)!;
        node["State"]!["Character"]!["Gold"] = 999999;
        var e2 = Assert.Throws<SaveLoadException>(() => SaveStore.Parse(node.ToJsonString()));
        Assert.Contains("checksum", e2.Message);

        node = JsonNode.Parse(json)!;
        node["SchemaVersion"] = 99;
        Assert.Contains("newer version", Assert.Throws<SaveLoadException>(() => SaveStore.Parse(node.ToJsonString())).Message);

        Assert.Contains("not a Telengard save", Assert.Throws<SaveLoadException>(() => SaveStore.Parse("{}")).Message);
    }

    [Fact]
    public async Task ListingShowsDamagedFilesWithoutThrowing()
    {
        await Store.SaveAsync(SampleState(), SaveKind.Manual, RulesPolicy.Classic(), Ct);
        await File.WriteAllTextAsync(Path.Combine(_dir, "BROKEN.tgsave"), "{ not json", Ct);
        var list = Store.List();
        Assert.Equal(2, list.Count);
        Assert.Single(list, s => s.IsUsable);
        Assert.Single(list, s => !s.IsUsable && s.CharacterName == "BROKEN");
    }

    [Fact]
    public async Task DemoCharacterCannotBeSaved()
    {
        var rng = new SplitMix64Random(1);
        var demo = CharacterCreation.NewGame("", CharacterCreation.RollStats(rng), rng);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store.SaveAsync(demo, SaveKind.Manual, RulesPolicy.Classic(), Ct));
    }

    [Theory]
    [InlineData("SVENJA", false, true)]   // V4.18 default: the stored character survives death
    [InlineData("SVENJA", true, true)]    // manual rule: SV names survive
    [InlineData("BORIS", true, false)]    // manual rule: others are erased
    public async Task DeathRule(string name, bool manualRule, bool manualSaveSurvives)
    {
        var s = SampleState();
        s.Character.Name = name;
        var policy = RulesPolicy.Classic() with { ManualDeathRule = manualRule };
        await Store.SaveAsync(s, SaveKind.Manual, policy, Ct);
        await Store.SaveAsync(s, SaveKind.Suspend, policy, Ct);
        Store.OnDeath(name, policy);
        Assert.Equal(manualSaveSurvives, File.Exists(Store.PathFor(name, SaveKind.Manual)));
        Assert.False(File.Exists(Store.PathFor(name, SaveKind.Suspend)));
    }

    [Fact]
    public async Task SettingsRoundTripAndCorruptSettingsFallBack()
    {
        var path = Path.Combine(_dir, "settings.json");
        var store = new SettingsStore(path);
        var settings = new GameSettings { UiScale = 1.5, ModernDeadlineSeconds = null, HighContrast = true };
        settings.KeyBindings["North"] = ["W", "Up"];
        await store.SaveAsync(settings, Ct);
        var loaded = store.Load(out var problem);
        Assert.Null(problem);
        Assert.Equal(1.5, loaded.UiScale);
        Assert.Null(loaded.ModernDeadlineSeconds);
        Assert.Equal(["W", "Up"], loaded.KeyBindings["North"]);

        await File.WriteAllTextAsync(path, "garbage", Ct);
        var fallback = store.Load(out problem);
        Assert.NotNull(problem);
        Assert.Equal(1.0, fallback.UiScale);
    }
}
