using Telengard.Audio;
using Telengard.Core.Content;
using Telengard.Core.Engine;
using Telengard.Core.Model;
using Telengard.Core.Randomness;

namespace Telengard.Core.Tests;

/// <summary>Records what the director asks the engine to do (adapted from AVABand's tests).</summary>
internal sealed class RecordingAudioEngine : NullAudioEngine
{
    public List<string> Effects { get; } = [];
    public List<string> MusicStarted { get; } = [];
    public List<string?> Ambience { get; } = [];
    public override bool IsAvailable => true;
    public override void PlayEffect(string path, float gain = 1f) => Effects.Add(Path.GetFileName(path));
    public override void PlayMusic(string path, bool loop = true)
    {
        base.PlayMusic(path, loop);
        MusicStarted.Add(Path.GetFileName(path));
    }
    public override void PlayAmbience(string path)
    {
        base.PlayAmbience(path);
        Ambience.Add(Path.GetFileName(path));
    }
}

public class AudioTests
{
    private static string DesktopRoot
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AVAGard.slnx"))) dir = dir.Parent;
            return Path.Combine(dir!.FullName, "src", "Telengard.Desktop");
        }
    }

    private static string PacksRoot => Path.Combine(DesktopRoot, "soundpacks");
    private static SoundPack Retro => SoundPack.Load(Path.Combine(PacksRoot, "avagard-retro"));
    private static SoundPack Dubtrain => SoundPack.Load(Path.Combine(PacksRoot, "angband-dubtrain"));
    private static SoundPack Music => SoundPack.Load(Path.Combine(PacksRoot, "avagard-music"));

    private static (SoundDirector Director, RecordingAudioEngine Engine, List<string> Played) Setup(SoundPack? effects = null)
    {
        var engine = new RecordingAudioEngine();
        var director = new SoundDirector(engine)
        {
            Effects = effects ?? Retro,
            MusicPack = Music,
            AmbienceFolders = [Path.Combine(DesktopRoot, "ambience")],
        };
        var played = new List<string>();
        director.Played += played.Add;
        return (director, engine, played);
    }

    [Fact]
    public void BundledPacksLoadCleanly()
    {
        var problems = new List<string>();
        var packs = SoundPackCatalog.Discover([PacksRoot], problems);
        Assert.Empty(problems);
        Assert.Equal(["angband-dubtrain", "avagard-music", "avagard-retro"], packs.Select(p => p.Id).Order());
        Assert.All(["title", "town", "dungeon", "deep", "boss"], mood => Assert.True(Music.Music.ContainsKey(mood), mood));
    }

    [Fact]
    public void EveryCueHasASoundInTheRetroPack()
    {
        var (director, engine, played) = Setup();
        foreach (var cue in Enum.GetValues<SoundCue>()) director.Play(cue);
        Assert.Equal(Enum.GetValues<SoundCue>().Length, played.Count);
        Assert.Equal(played.Count, engine.Effects.Count);
    }

    [Fact]
    public void DubtrainPackCoversTheCombatCues()
    {
        var (director, _, played) = Setup(Dubtrain);
        foreach (var cue in new[] { SoundCue.Hit, SoundCue.Miss, SoundCue.Kill, SoundCue.Spell, SoundCue.Teleport, SoundCue.Death, SoundCue.Breath, SoundCue.LevelUp })
            director.Play(cue);
        Assert.Equal(["HIT", "MISS", "KILL", "SPELL", "TELEPORT", "DEATH", "BR_FIRE", "LEVEL"], played);
    }

    [Fact]
    public void DisabledEffectsAreSilent()
    {
        var (director, engine, _) = Setup();
        director.EffectsEnabled = false;
        director.Play(SoundCue.Hit);
        Assert.Empty(engine.Effects);
    }

    [Fact]
    public void MusicFollowsTheHero()
    {
        var (director, engine, _) = Setup();
        Assert.Equal("title", director.MusicMood);
        Assert.Equal("avagard_title.ogg", engine.MusicStarted.Last());

        director.SetLocation(3);
        Assert.Equal("dungeon", director.MusicMood);
        Assert.Contains(engine.MusicStarted.Last(), Music.Music["dungeon"]);

        director.SetLocation(25);
        Assert.Equal("deep", director.MusicMood);
        Assert.Contains(engine.MusicStarted.Last(), Music.Music["deep"]);

        director.SetMonster(Monsters.Dragon);
        Assert.Equal("boss", director.MusicMood);
        Assert.Equal("boss_theme.ogg", engine.MusicStarted.Last());
        director.SetMonster(0);
        Assert.Equal("deep", director.MusicMood);

        director.SetLocation(0);
        Assert.Equal("town", director.MusicMood);
        Assert.Contains(engine.MusicStarted.Last(), Music.Music["town"]);

        director.TitleScreen = true;
        Assert.Equal("avagard_title.ogg", engine.MusicStarted.Last());
    }

    [Fact]
    public void PlaylistMovesOnWhenATrackEnds()
    {
        var (director, engine, _) = Setup();
        director.SetLocation(2);
        var first = engine.MusicStarted.Last();
        engine.FinishMusic();
        Assert.NotEqual(first, engine.MusicStarted.Last());
        Assert.Contains(engine.MusicStarted.Last(), Music.Music["dungeon"]);
    }

    [Fact]
    public void AmbienceFollowsDepthAndStopsAtTheInn()
    {
        var (director, engine, _) = Setup();
        director.SetLocation(5);
        Assert.Equal("ambient-dungeon-shallow.ogg", Path.GetFileName(engine.CurrentAmbience));
        director.SetLocation(30);
        Assert.Equal("ambient-dungeon-deep.ogg", Path.GetFileName(engine.CurrentAmbience));
        director.SetLocation(45);
        Assert.Equal("ambient-dungeon-abyss.ogg", Path.GetFileName(engine.CurrentAmbience));
        director.SetLocation(0);
        Assert.Null(engine.CurrentAmbience);
    }

    [Fact]
    public void EveryBundledSoundDecodes()
    {
        var buffer = new short[4096];
        foreach (var pack in new[] { Retro, Music, Dubtrain })
            foreach (var file in pack.ReferencedFiles())
            {
                using var decoder = AudioDecoders.Open(pack.Resolve(file));
                Assert.True(decoder.Read(buffer) > 0, file);
                Assert.InRange(decoder.Channels, 1, 2);
            }
        foreach (var loop in Directory.GetFiles(Path.Combine(DesktopRoot, "ambience"), "*.ogg"))
        {
            using var decoder = AudioDecoders.Open(loop);
            Assert.True(decoder.Read(buffer) > 0, loop);
        }
    }

    [Fact]
    public void SoundNeverChangesTheGame()
    {
        static string Play(bool withSound)
        {
            var rng = new SplitMix64Random(31);
            var state = CharacterCreation.NewGame("QUIET", CharacterCreation.RollStats(rng), rng);
            state.Character.Hp = state.Character.MaxHp = 10_000;
            var game = new Game(state, RulesPolicy.Classic(), rng);
            SoundDirector? director = withSound ? Setup().Director : null;
            var log = new List<string>();
            void Feed(IEnumerable<GameEvent> events)
            {
                foreach (var e in events)
                {
                    if (e is SoundEvent s) director?.Play(s.Cue);
                    if (e is MessageEvent m) log.Add(m.Text);
                }
                director?.SetLocation(game.State.Z);
            }
            Feed(game.Start(true));
            for (int i = 0; i < 300 && game.CurrentPrompt is { } p; i++)
                Feed(game.Submit(p.Kind is PromptKind.Ready or PromptKind.Stairs ? PlayerInput.Of(Command.Stay)
                    : p.Kind == PromptKind.Combat ? PlayerInput.Of(Command.Fight) : PlayerInput.Of(Command.Other), p.Sequence));
            return string.Join("|", log) + game.State.Character.Experience;
        }
        Assert.Equal(Play(false), Play(true));
    }

    [Fact]
    public void OpenAlOpensOrFallsBackToSilence()
    {
        using var engine = OpenAlAudioEngine.CreateOrSilent(AudioBuffer.PeriodFrames("auto", VirtualMachine.Detect()));
        engine.SetVolumes(0f, 0f, 0f); // silent while testing
        engine.PlayEffect(Retro.Resolve(Retro.Sounds["HIT"][0]));
        engine.PlayMusic(Music.Resolve(Music.Music["title"][0]));
        Thread.Sleep(200);
        engine.StopMusic();
        if (!engine.IsAvailable) Assert.NotNull(((NullAudioEngine)engine).Reason);
    }
}
