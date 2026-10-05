namespace Telengard.Core.Tests;

/// <summary>A TimeProvider whose clock only moves when the test calls <see cref="Advance"/>.</summary>
public sealed class ManualTimeProvider : TimeProvider
{
    private long _ticks;
    private readonly List<ManualTimer> _timers = [];

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    public override long GetTimestamp() => _ticks;

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var t = new ManualTimer(this, callback, state, _ticks + dueTime.Ticks);
        _timers.Add(t);
        return t;
    }

    public void Advance(TimeSpan by)
    {
        _ticks += by.Ticks;
        foreach (var t in _timers.ToArray())
            if (!t.Disposed && t.Due <= _ticks)
            {
                t.Disposed = true;
                t.Callback(t.State);
            }
    }

    private sealed class ManualTimer(ManualTimeProvider owner, TimerCallback callback, object? state, long due) : ITimer
    {
        public TimerCallback Callback { get; } = callback;
        public object? State { get; } = state;
        public long Due { get; set; } = due;
        public bool Disposed { get; set; }
        public bool Change(TimeSpan dueTime, TimeSpan period) { Due = owner._ticks + dueTime.Ticks; return true; }
        public void Dispose() => Disposed = true;
        public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
    }
}
