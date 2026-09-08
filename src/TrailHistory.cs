using System;
using System.Collections.Generic;

namespace PeakSprintLock;

// Engine-independent history. Coordinates stay in 3D, including climbs and jumps.
public sealed class TrailHistory
{
    public readonly struct Point
    {
        public readonly float X, Y, Z;
        public readonly double Time;
        public readonly bool BreakBefore;
        public Point(float x, float y, float z, double time, bool breakBefore)
        { X = x; Y = y; Z = z; Time = time; BreakBefore = breakBefore; }
    }

    public const double Lifetime = 45, SampleInterval = .1, MaxGap = 1;
    public const int Capacity = 600;
    private readonly List<Point> points = new List<Point>(Capacity);
    public IReadOnlyList<Point> Points { get; }
    private Point previous;
    private bool observed, pendingBreak = true;
    private double lastClock = double.NegativeInfinity;
    public TrailHistory() { Points = points.AsReadOnly(); }

    public void Clear()
    { points.Clear(); observed = false; pendingBreak = true; lastClock = double.NegativeInfinity; }

    public void Prune(double now)
    {
        if (!Finite(now)) return;
        if (now < lastClock) Clear();
        lastClock = now;
        int remove = 0;
        while (remove < points.Count && now - points[remove].Time >= Lifetime) remove++;
        if (remove > 0) points.RemoveRange(0, remove);
    }

    public bool Observe(double now, float x, float y, float z)
    {
        if (!Finite(now) || !Finite(x) || !Finite(y) || !Finite(z))
        { pendingBreak = true; observed = false; return false; }
        Prune(now);
        var point = new Point(x, y, z, now, false);
        if (observed)
        {
            if (now - previous.Time > MaxGap || DistanceSquared(point, previous) > 64)
                pendingBreak = true;
        }
        previous = point;
        observed = true;
        if (points.Count > 0 && !pendingBreak)
        {
            var last = points[points.Count - 1];
            if (now - last.Time < SampleInterval) return false;
            // Keep a stationary heartbeat so the end does not disappear while waiting.
            if (DistanceSquared(point, last) < .04 && now - last.Time < .5) return false;
        }
        points.Add(new Point(x, y, z, now, pendingBreak || points.Count == 0));
        pendingBreak = false;
        if (points.Count > Capacity) points.RemoveRange(0, points.Count - Capacity);
        return true;
    }

    public static float Opacity(double age) => (float)Math.Max(0, Math.Min(1, 1 - age / Lifetime));

    // Newest first; singleton segments have nothing to draw. Reuse the caller's list.
    public void GetSegments(List<(int Start, int Count)> output, int limit = 32)
    {
        output.Clear();
        int end = points.Count;
        while (end > 0 && output.Count < limit)
        {
            int start = end - 1;
            while (start > 0 && !points[start].BreakBefore) start--;
            if (end - start >= 2) output.Add((start, end - start));
            end = start;
        }
    }
    private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    private static double DistanceSquared(Point a, Point b)
    { double x = (double)a.X - b.X, y = (double)a.Y - b.Y, z = (double)a.Z - b.Z; return x*x + y*y + z*z; }

    // Highest forward alignment wins; range and cone reject behind-camera candidates.
    public static bool EligibleTarget(float distance, float forwardDot)
        => Finite(distance) && Finite(forwardDot) && distance > .1f && distance <= 60 && forwardDot >= .94f;
}
