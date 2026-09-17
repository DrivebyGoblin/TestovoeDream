using System;

// Combines Unity focus/pause callbacks into one background interval.
public sealed class ApplicationSession
{
    private bool _hasFocus = true;
    private bool _isPaused;

    public DateTime? SuspendedAt { get; private set; }
    public bool IsSuspended => SuspendedAt.HasValue;
    public int ResumeCount { get; private set; }

    public DateTime? SetFocus(bool hasFocus, DateTime now)
    {
        _hasFocus = hasFocus;
        return UpdateState(now);
    }

    public DateTime? SetPause(bool isPaused, DateTime now)
    {
        _isPaused = isPaused;
        return UpdateState(now);
    }

    private DateTime? UpdateState(DateTime now)
    {
        if (!_hasFocus || _isPaused)
        {
            if (!IsSuspended) SuspendedAt = now;
            return null;
        }

        if (!IsSuspended) return null;

        DateTime? start = SuspendedAt;
        SuspendedAt = null;
        ResumeCount++;
        return start;
    }
}
