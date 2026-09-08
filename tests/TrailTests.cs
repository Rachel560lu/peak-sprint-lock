using PeakSprintLock;

static class TrailTests
{
    public static void Run(Action<bool, string> check)
    {
        var h = new TrailHistory();
        check(h.Observe(0, 0, 0, 0) && h.Points[0].BreakBefore, "trail starts a new segment");
        check(!h.Observe(.05, 1, 0, 0), "sampling throttles ordinary movement");
        check(h.Observe(.11, 1, 2, 3) && h.Points[1].Y == 2 && !h.Points[1].BreakBefore, "climbing retains 3D coordinates");
        check(!h.Observe(.3, 1.01f, 2, 3), "small network jitter does not add points");
        check(h.Observe(.7, 1, 2, 3), "stationary heartbeat preserves endpoint");
        h.Observe(.71, 30, 2, 3);
        check(h.Points[h.Points.Count-1].BreakBefore, "teleport breaks even inside sample interval");
        h.Observe(2, 31, 2, 3);
        check(h.Points[h.Points.Count-1].BreakBefore, "missing observations break route");
        int before = h.Points.Count;
        check(!h.Observe(2.1, float.NaN, 0, 0) && h.Points.Count == before, "invalid position rejected");
        h.Observe(2.2, 32, 2, 3);
        check(h.Points[h.Points.Count-1].BreakBefore, "invalid position cannot bridge segments");
        check(!h.Observe(double.PositiveInfinity, 0, 0, 0), "invalid timestamp rejected");
        h.Observe(1, 1, 1, 1);
        check(h.Points.Count == 1 && h.Points[0].BreakBefore, "clock rewind resets history");
        h.Prune(46);
        check(h.Points.Count == 0, "points expire at lifetime boundary");
        h.Clear();
        for (int i = 0; i < 2000; i++) h.Observe(i * .001, i * 20, 0, 0);
        check(h.Points.Count == TrailHistory.Capacity, "teleport storm respects memory cap");
        check(h.Points[0].X == 1400 * 20, "capacity removes oldest points");
        h.Clear();
        check(h.Points.Count == 0, "clearing removes target history");
        h.Observe(0, 0, 0, 0);
        check(h.Points[0].BreakBefore, "new target cannot connect to old target");
        h.Clear();
        for (int i = 0; i < 1000; i++) h.Observe(i * .11, i * .3f, 0, 0);
        check(h.Points.Count <= 410 && h.Points[0].Time > 64, "long run retains only recent window");
        check(TrailHistory.Opacity(0) == 1 && TrailHistory.Opacity(22.5) == .5f && TrailHistory.Opacity(45) == 0, "age fades linearly");
        check(TrailHistory.Opacity(-1) == 1 && TrailHistory.Opacity(100) == 0, "opacity is bounded");
        check(TrailHistory.EligibleTarget(10, 1), "forward nearby target eligible");
        check(!TrailHistory.EligibleTarget(61, 1) && !TrailHistory.EligibleTarget(10, -.5f), "distant and behind-camera targets rejected");
        check(!TrailHistory.EligibleTarget(0, 1) && !TrailHistory.EligibleTarget(float.NaN, 1), "invalid target geometry rejected");
        var segments = new List<(int Start, int Count)>();
        h.Clear();
        h.Observe(0, 0, 0, 0); h.Observe(.2, 1, 0, 0);
        h.Observe(.4, 100, 0, 0); h.Observe(.6, 101, 0, 0);
        h.GetSegments(segments);
        check(segments.Count == 2 && segments[0] == (2, 2) && segments[1] == (0, 2), "renderer ranges never bridge a teleport");
        h.GetSegments(segments, 1);
        check(segments.Count == 1 && segments[0].Start == 2, "renderer limit prioritizes newest route");
        h.Observe(.8, 200, 0, 0);
        h.GetSegments(segments);
        check(segments.Count == 2, "singleton after teleport is not drawn");
        h.Prune(45.3);
        h.GetSegments(segments);
        check(segments.Count == 1 && segments[0] == (0, 2), "expiry reindexes remaining segment correctly");
        h.Clear(); h.GetSegments(segments);
        check(segments.Count == 0, "clear removes all renderer ranges");
        for (int i = 0; i < 100; i++)
        { h.Observe(i * .3, i * 100, 0, 0); h.Observe(i * .3 + .15, i * 100 + 1, 0, 0); }
        h.GetSegments(segments);
        check(segments.Count == 32 && segments[0].Start == 198, "many discontinuities respect renderer cap");
    }
}
