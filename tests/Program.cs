using PeakSprintLock;

int count = 0;
void Check(bool value, string name) { if (!value) throw new Exception("FAIL " + name); Console.WriteLine("PASS " + name); count++; }
LockState Active() { var s = new LockState(); s.Step(true, true, false, true, null); return s; }
var state = new LockState();
Check(!state.Active, "starts off");
state.Step(true, false, false, true, null);
Check(!state.Active, "stationary toggle rejected");
state.Step(true, true, false, true, null);
Check(state.Active, "forward plus toggle activates");
state.Step(false, false, false, true, null);
Check(state.Active, "releasing keys keeps lock");
state.Step(true, false, false, true, null);
Check(!state.Active, "toggle cancels without forward");
state = Active(); state.Step(true, true, true, true, null);
Check(!state.Active, "backward wins over simultaneous toggle and forward");
foreach (var reason in new[] { "Input blocked", "Climbing", "Focus lost", "Character incapacitated", "Stamina empty", "Disabled", "Scene changed", "Plugin error", "Native input reset", "Pause input", "Crouch input" })
{
    state = Active(); state.Step(true, true, false, true, reason);
    Check(!state.Active && state.Reason == reason, "cancel: " + reason);
    state.Step(false, true, false, true, null);
    Check(!state.Active, "no auto-resume: " + reason);
}
state = new LockState(); state.Step(true, false, false, false, null);
Check(state.Active, "optional stationary activation");
LockState.Merge(0, 0, false, true, true, out var x, out var y, out var sprint);
Check(x == 0 && y == 1 && sprint, "forward and sprint injection");
foreach (float strafe in new[] { -1f, -.5f, 0f, .5f, 1f })
{
    LockState.Merge(strafe, 0, false, true, true, out x, out y, out sprint);
    Check(Math.Abs(x*x+y*y-1) < .00001 && Math.Sign(x)==Math.Sign(strafe) && y>0, "clamped strafe " + strafe);
}
LockState.Merge(.3f, -.4f, true, false, false, out x, out y, out sprint);
Check(x == .3f && y == -.4f && sprint, "inactive preserves physical input");
state=Active(); state.Cancel("Character changed");
Check(!state.Active, "character replacement resets lock");
state = Active();
Check(state.Active && !state.SprintLocked && state.Reason == "Auto walk", "W plus toggle selects walking");
state.Step(false, false, false, true, null, true);
Check(!state.SprintLocked, "pressing sprint later does not change captured walking mode");
LockState.Merge(0, 0, true, state.Active, state.SprintLocked, out x, out y, out sprint);
Check(y == 1 && !sprint, "walking output stays walking even with manual sprint input");
state.Step(true, false, false, true, null);
Check(!state.Active && !state.SprintLocked, "walking toggle off clears mode");
state.Step(true, true, false, true, null, true);
Check(state.Active && state.SprintLocked && state.Reason == "Auto run", "sprint plus W plus toggle selects running");
state.Step(false, false, false, true, null, false);
Check(state.SprintLocked, "releasing sprint preserves running mode");
LockState.Merge(0, 0, false, state.Active, state.SprintLocked, out x, out y, out sprint);
Check(y == 1 && sprint, "running continues after physical keys released");
foreach (var reason in new[] { "Climbing", "Focus lost", "Character incapacitated", "Stamina empty", "Scene changed", "Input blocked" })
{
    state = new LockState(); state.Step(true, true, false, true, null, true);
    state.Step(false, false, false, true, reason);
    Check(!state.Active && !state.SprintLocked, "running cancellation clears mode: " + reason);
    state.Step(true, true, false, true, null, false);
    Check(state.Active && !state.SprintLocked, "fresh walking activation does not inherit sprint: " + reason);
}
state = new LockState(); state.Step(true, true, true, true, null, true);
Check(!state.Active, "backward wins over sprint activation");
LockState.Merge(1, 0, false, true, false, out x, out y, out sprint);
Check(Math.Abs(x*x+y*y-1)<.00001 && !sprint, "walking diagonal limited to native speed");
TrailTests.Run(Check);
Console.WriteLine($"TOTAL {count} passed");
