using Telengard.Infrastructure.Saves;
using Telengard.Infrastructure.Settings;

namespace Telengard.Desktop.Services;

/// <summary>Application-wide services, created once at startup and passed to view models.</summary>
public sealed class AppServices
{
    public required SaveStore Saves { get; init; }
    public required SettingsStore SettingsStore { get; init; }
    public required GameSettings Settings { get; set; }
    public TimeProvider Time { get; init; } = TimeProvider.System;
    public ISoundPlayer Sound { get; init; } = new SilentSoundPlayer();
    public string? StartupProblem { get; init; }

    public static AppServices CreateDefault()
    {
        var settingsStore = SettingsStore.ForCurrentUser();
        var settings = settingsStore.Load(out var problem);
        return new AppServices
        {
            Saves = SaveStore.ForCurrentUser(),
            SettingsStore = settingsStore,
            Settings = settings,
            StartupProblem = problem,
        };
    }

    /// <summary>For headless tests: everything under a temporary directory.</summary>
    public static AppServices CreateIsolated(string root) => new()
    {
        Saves = new SaveStore(Path.Combine(root, "saves")),
        SettingsStore = new SettingsStore(Path.Combine(root, "settings.json")),
        Settings = new GameSettings(),
    };

    public async Task SaveSettingsAsync()
    {
        try { await SettingsStore.SaveAsync(Settings); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { /* non-fatal; settings stay in memory */ }
    }
}
