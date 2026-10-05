namespace Telengard.Core.Timing;

/// <summary>
/// Real-time pressure for prompts. When armed, it calls back with the prompt's sequence number
/// once the deadline passes; the host then submits a timeout for that sequence (the engine
/// ignores it if the prompt has already been answered). Uses an injected <see cref="TimeProvider"/>
/// so tests can control time; game rules never read the clock.
/// </summary>
public sealed class PromptDeadline(TimeProvider time) : IDisposable
{
    private readonly object _gate = new();
    private ITimer? _timer;
    private long _sequence;
    private long _startedAt;
    private TimeSpan _duration;
    private Action<long>? _onExpired;
    private bool _paused;
    private TimeSpan _remainingWhenPaused;

    public bool IsArmed { get { lock (_gate) return _timer is not null || _paused; } }

    public void Arm(long sequence, TimeSpan duration, Action<long> onExpired)
    {
        lock (_gate)
        {
            DisposeTimer();
            _sequence = sequence;
            _duration = duration;
            _onExpired = onExpired;
            _paused = false;
            Start(duration);
        }
    }

    /// <summary>Restarts the full deadline (used while the player is typing a number).</summary>
    public void Restart()
    {
        lock (_gate)
        {
            if (_onExpired is null || _paused) return;
            DisposeTimer();
            Start(_duration);
        }
    }

    public void Cancel()
    {
        lock (_gate)
        {
            DisposeTimer();
            _onExpired = null;
            _paused = false;
        }
    }

    /// <summary>Modern pause: freezes the remaining time.</summary>
    public void Pause()
    {
        lock (_gate)
        {
            if (_timer is null || _paused) return;
            _remainingWhenPaused = RemainingUnlocked();
            DisposeTimer();
            _paused = true;
        }
    }

    public void Resume()
    {
        lock (_gate)
        {
            if (!_paused) return;
            _paused = false;
            Start(_remainingWhenPaused);
        }
    }

    /// <summary>Fraction of the deadline remaining (1 = just armed, 0 = expired), or null if not armed.</summary>
    public double? RemainingFraction
    {
        get
        {
            lock (_gate)
            {
                if (_paused) return _remainingWhenPaused / _duration;
                if (_timer is null || _duration <= TimeSpan.Zero) return null;
                return Math.Clamp(RemainingUnlocked() / _duration, 0, 1);
            }
        }
    }

    private TimeSpan _currentLength;

    private TimeSpan RemainingUnlocked()
    {
        var elapsed = time.GetElapsedTime(_startedAt);
        var left = _currentLength - elapsed;
        return left < TimeSpan.Zero ? TimeSpan.Zero : left;
    }

    private void Start(TimeSpan length)
    {
        _startedAt = time.GetTimestamp();
        _currentLength = length;
        long seq = _sequence;
        _timer = time.CreateTimer(_ => Fire(seq), null, length, Timeout.InfiniteTimeSpan);
    }

    private void Fire(long seq)
    {
        Action<long>? callback;
        lock (_gate)
        {
            if (seq != _sequence || _onExpired is null || _paused) return;
            callback = _onExpired;
            _onExpired = null;
            DisposeTimer();
        }
        callback(seq);
    }

    private void DisposeTimer()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void Dispose() => Cancel();
}
