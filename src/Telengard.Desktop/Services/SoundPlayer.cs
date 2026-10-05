using Telengard.Core.Engine;

namespace Telengard.Desktop.Services;

/// <summary>
/// Sound output. Avalonia has no audio API; a cross-platform backend is deferred (see
/// docs/KNOWN_ISSUES.md). The game is fully playable without sound.
/// </summary>
public interface ISoundPlayer
{
    void Play(SoundCue cue, double volume);
}

public sealed class SilentSoundPlayer : ISoundPlayer
{
    public void Play(SoundCue cue, double volume) { }
}
