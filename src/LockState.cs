namespace PeakSprintLock;

// Pure policy: blocked/cancel always wins, and resuming never reactivates the lock.
public sealed class LockState
{
    public bool Active { get; private set; }
    public bool SprintLocked { get; private set; }
    public string Reason { get; private set; } = "Ready";

    public void Cancel(string reason) { Active = false; SprintLocked = false; Reason = reason; }

    public void Step(bool toggle, bool forward, bool backward, bool requireForward, string? blocked, bool sprintHeld = false)
    {
        if (blocked != null) { Cancel(blocked); return; }
        if (backward) { Cancel("Backward input"); return; }
        if (!toggle) return;
        if (Active) { Cancel("Toggle off"); return; }
        if (requireForward && !forward) { Reason = "Hold forward to activate"; return; }
        Active = true;
        SprintLocked = sprintHeld;
        Reason = SprintLocked ? "Auto run" : "Auto walk";
    }

    public static void Merge(float x, float y, bool sprint, bool active, bool sprintLocked,
        out float resultX, out float resultY, out bool resultSprint)
    {
        resultX = x; resultY = y; resultSprint = sprint;
        if (!active) return;
        resultY = 1f;
        var length = (float)System.Math.Sqrt(x * x + 1f);
        resultX /= length; resultY /= length;
        resultSprint = sprintLocked;
    }
}
