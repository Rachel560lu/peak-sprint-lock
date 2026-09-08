using System;
using System.Diagnostics;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PeakSprintLock;

[BepInPlugin(Id, "PEAK Sprint Lock", "0.2.0")]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Id = "dev.midor.peaksprintlock";
    internal static Plugin? Instance;
    internal readonly LockState State = new LockState();
    internal static AccessTools.FieldRef<CharacterMovement, bool> NativeSprintToggle = null!;
    internal ConfigEntry<bool> Enabled = null!, RequireForward = null!, ShowHud = null!, StopAtEmpty = null!, Diagnostics = null!;
    internal ConfigEntry<Key> ToggleKey = null!;
    private Harmony? harmony;
    private GameObject? driver;
    private CharacterInput? owner;
    private Vector2 rawMovement;
    private bool rawSprint, injected, faulted;
    private int toggleFrame = -1;
    private int sampleCount;
    private float nextTrace;
    private Func<Character, bool> outOfStamina = null!, canDoInput = null!;
    private Func<CharacterInput, Vector2> getMovement = null!;
    private string tracePath = "";

    private void Awake()
    {
        Instance = this;
        Enabled = Config.Bind("General", "Enabled", true, "Enable sprint lock.");
        ToggleKey = Config.Bind("Controls", "ToggleKey", Key.CapsLock, "Keyboard toggle key (Unity Input System name).");
        RequireForward = Config.Bind("Controls", "RequireForward", true, "Hold the game's forward action while activating.");
        ShowHud = Config.Bind("Display", "ShowHud", true, "Show a small status label.");
        StopAtEmpty = Config.Bind("Safety", "CancelWhenOutOfStamina", true, "Cancel running lock when the game reports no regular stamina; walking is unaffected.");
        Diagnostics = Config.Bind("Diagnostics", "Enabled", false, "Record one sample per second while active; no network data.");
        try
        {
            outOfStamina = AccessTools.MethodDelegate<Func<Character, bool>>(AccessTools.Method(typeof(Character), "OutOfRegularStamina"));
            canDoInput = AccessTools.MethodDelegate<Func<Character, bool>>(AccessTools.Method(typeof(Character), "CanDoInput"));
            getMovement = AccessTools.MethodDelegate<Func<CharacterInput, Vector2>>(AccessTools.Method(typeof(CharacterInput), "GetMovementInput"));
            NativeSprintToggle = AccessTools.FieldRefAccess<CharacterMovement, bool>("sprintToggleEnabled");
            var folder = Path.Combine(Paths.BepInExRootPath, "SprintLockDiagnostics");
            Directory.CreateDirectory(folder);
            tracePath = Path.Combine(folder, $"session-{Process.GetCurrentProcess().Id}-{DateTime.UtcNow:yyyyMMddTHHmmss}.log");
            Trace($"START version=0.2.0 gameMvid={typeof(Character).Module.ModuleVersionId} pluginMvid={typeof(Plugin).Module.ModuleVersionId}");
            harmony = new Harmony(Id);
            harmony.PatchAll(typeof(Plugin).Assembly);
            driver = new GameObject("PeakSprintLock.Runtime");
            DontDestroyOnLoad(driver);
            driver.AddComponent<RuntimeDriver>();
            driver.AddComponent<TrailGuide>().Initialize(Config, canDoInput);
            SceneManager.activeSceneChanged += SceneChanged;
        }
        catch (Exception ex) { Fail(ex); }
    }

    private void SceneChanged(Scene from, Scene to) { Cancel("Scene changed"); owner = null; Trace("SCENE " + to.name); }

    internal void Trace(string message)
    {
        Logger.LogInfo(message);
        if (tracePath.Length == 0) return;
        try { File.AppendAllText(tracePath, DateTime.UtcNow.ToString("O") + " " + message + Environment.NewLine); }
        catch { /* Diagnostics must never break input. */ }
    }

    internal void Fail(Exception ex)
    {
        Cancel("Plugin error");
        if (!faulted) { faulted = true; Logger.LogError(ex); Trace("FAULT " + ex.GetType().Name + ": " + ex.Message); }
    }

    internal void Cancel(string reason)
    {
        if (State.Active) Trace("OFF " + reason);
        State.Cancel(reason);
        Restore();
    }

    private void Restore()
    {
        if (injected && owner)
        {
            owner!.movementInput = rawMovement;
            owner.sprintIsPressed = rawSprint;
        }
        injected = false;
    }

    // Sample itself resets all inputs, so discard our ownership before the native reset.
    internal void BeforeSample(CharacterInput input)
    {
        if (owner == input) injected = false;
    }

    internal void AfterSample(CharacterInput input, bool allowed)
    {
        if (faulted) return;
        var character = Character.localCharacter;
        if (!character || character.input != input || !character.IsLocal) return;
        if (owner != input) { Cancel("Character changed"); owner = input; }
        rawMovement = input.movementInput; rawSprint = input.sprintIsPressed;
        if (++sampleCount == 1) Trace("FIRST_SAMPLE scene=" + SceneManager.GetActiveScene().name);
        var keyboard = Keyboard.current;
        var toggle = keyboard != null && ToggleKey.Value != Key.None && keyboard[ToggleKey.Value].wasPressedThisFrame && toggleFrame != Time.frameCount;
        if (toggle) toggleFrame = Time.frameCount;
        string? blocked = BlockReason(character, allowed);
        if (input.pauseWasPressed) blocked = "Pause input";
        if (input.crouchIsPressed || input.crouchToggleWasPressed) blocked = "Crouch input";
        if (StopAtEmpty.Value && (State.SprintLocked || (!State.Active && toggle && rawSprint)) && outOfStamina(character)) blocked = "Stamina empty";
        // Check the rebound backward action too: W+S can produce a zero movement vector.
        bool backward = rawMovement.y < -0.1f || (CharacterInput.action_moveBackward?.IsPressed() ?? false);
        bool previous = State.Active;
        State.Step(toggle, rawMovement.y > 0.1f, backward, RequireForward.Value, blocked, rawSprint);
        if (previous != State.Active || toggle) Trace((State.Active ? "ON " : "OFF ") + State.Reason);
        if (!State.Active) return;
        // Native Sample already normalized W+D. Read its rebound source again so
        // adding forward does not normalize an already-normalized diagonal twice.
        var physical = getMovement(input);
        LockState.Merge(physical.x, rawMovement.y, rawSprint, true, State.SprintLocked, out var x, out var y, out var sprint);
        input.movementInput = new Vector2(x, y);
        input.sprintIsPressed = sprint;
        injected = true;
        if (Diagnostics.Value && Time.unscaledTime >= nextTrace)
        {
            nextTrace = Time.unscaledTime + 1f;
            Trace($"SAMPLE frame={Time.frameCount} raw={rawMovement} output={input.movementInput} sprint={sprint} position={character.transform.position} stamina={character.data.currentStamina:F4} samples={sampleCount}");
        }
    }

    private string? BlockReason(Character c, bool allowed)
    {
        if (!Enabled.Value) return "Disabled";
        if (!Application.isFocused) return "Focus lost";
        if (!allowed) return "Input blocked";
        if (c.data.dead || c.data.passedOut || c.data.fullyPassedOut) return "Character incapacitated";
        if (c.data.isClimbingAnything) return "Climbing";
        if (StopAtEmpty.Value && State.SprintLocked && outOfStamina(c)) return "Stamina empty";
        return null;
    }

    internal void Tick()
    {
        if (!State.Active) return;
        var c = Character.localCharacter;
        if (!c || c.input != owner || !c.IsLocal) { Cancel("Character unavailable"); return; }
        var reason = BlockReason(c, canDoInput(c));
        if (reason != null) Cancel(reason);
        // Sample can be skipped by native spectator/god-camera branches.
    }

    internal void AfterReset(CharacterInput input)
    {
        if (owner != input || !injected) return;
        injected = false;
        Cancel("Native input reset");
    }

    internal void Draw()
    {
        if (!ShowHud.Value || !Character.localCharacter) return;
        var label = State.Active ? $"{(State.SprintLocked ? "AUTO RUN" : "AUTO WALK")}  |  {ToggleKey.Value} / Backward: stop" : $"Move Lock: OFF  |  Forward (+ Sprint) + {ToggleKey.Value}";
        GUI.Box(new Rect(16, 16, 480, 30), label);
    }

    private void OnDestroy()
    {
        Cancel("Plugin unloaded");
        SceneManager.activeSceneChanged -= SceneChanged;
        harmony?.UnpatchSelf();
        if (driver) Destroy(driver);
        Instance = null;
    }
}

public sealed class RuntimeDriver : MonoBehaviour
{
    private void Start() => Plugin.Instance?.Trace("FIRST_FRAME");
    private void Update() { try { Plugin.Instance?.Tick(); } catch (Exception ex) { Plugin.Instance?.Fail(ex); } }
    private void OnGUI() { try { Plugin.Instance?.Draw(); } catch (Exception ex) { Plugin.Instance?.Fail(ex); } }
    private void OnApplicationFocus(bool focus) { if (!focus) Plugin.Instance?.Cancel("Focus lost"); }
    private void OnApplicationPause(bool paused) { if (paused) Plugin.Instance?.Cancel("Paused"); }
    private void OnDisable() => Plugin.Instance?.Cancel("Runtime disabled");
}

[HarmonyPatch(typeof(CharacterInput), "Sample", new Type[] { typeof(bool) })]
internal static class SamplePatch
{
    private static void Prefix(CharacterInput __instance) => Plugin.Instance?.BeforeSample(__instance);
    private static void Postfix(CharacterInput __instance, bool __0)
    {
        try { Plugin.Instance?.AfterSample(__instance, __0); }
        catch (Exception ex) { Plugin.Instance?.Fail(ex); }
    }
}

[HarmonyPatch(typeof(CharacterInput), "ResetInput")]
internal static class ResetPatch
{
    private static void Postfix(CharacterInput __instance) => Plugin.Instance?.AfterReset(__instance);
}

// The game's separate sprint-toggle latch must not turn a walking lock into a run.
// Suppress it only during the native calculation, then restore its original state.
[HarmonyPatch(typeof(CharacterMovement), "SetMovementState")]
internal static class WalkingPatch
{
    internal struct Saved { public bool Applied, Toggle, Pressed; public CharacterInput? Input; }
    private static void Prefix(CharacterMovement __instance, ref Saved __state)
    {
        try
        {
            var plugin = Plugin.Instance;
            var c = Character.localCharacter;
            if (plugin == null || !plugin.State.Active || plugin.State.SprintLocked || !c || !c.IsLocal || c.refs.movement != __instance) return;
            __state = new Saved { Applied = true, Toggle = Plugin.NativeSprintToggle(__instance), Pressed = c.input.sprintToggleWasPressed, Input = c.input };
            Plugin.NativeSprintToggle(__instance) = false;
            c.input.sprintToggleWasPressed = false;
        }
        catch (Exception ex) { Plugin.Instance?.Fail(ex); }
    }
    private static void Finalizer(CharacterMovement __instance, Saved __state)
    {
        if (!__state.Applied) return;
        Plugin.NativeSprintToggle(__instance) = __state.Toggle;
        if (__state.Input) __state.Input!.sprintToggleWasPressed = __state.Pressed;
    }
}
