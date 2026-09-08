using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PeakSprintLock;

// Display-only feature: never writes movement, camera rotation or network state.
public sealed class TrailGuide : MonoBehaviour
{
    private readonly TrailHistory history = new TrailHistory();
    private readonly List<LineRenderer> lines = new List<LineRenderer>();
    private readonly List<(int Start, int Count)> segments = new List<(int, int)>(32);
    private ConfigEntry<bool> feature = null!;
    private ConfigEntry<Key> key = null!;
    private Character? target;
    private Character? localOwner;
    private Material? material;
    private double nextRender;
    private bool failed;
    private string status = "OFF";
    private Func<Character, bool> canInput = null!;

    internal void Initialize(ConfigFile config, Func<Character, bool> inputAllowed)
    {
        feature = config.Bind("Trail", "Enabled", true, "Enable experimental display-only teammate trails. Not gameplay verified.");
        key = config.Bind("Trail", "ToggleKey", Key.F6, "Look toward a teammate and press to start their trail; press again to clear. No automatic movement.");
        canInput = inputAllowed;
        SceneManager.activeSceneChanged += SceneChanged;
    }

    private void SceneChanged(Scene from, Scene to) => Clear("OFF");
    private void Clear(string reason)
    {
        target = null; history.Clear(); status = reason;
        foreach (var line in lines) if (line) line.enabled = false;
    }

    private void Update()
    {
        if (failed || feature == null) return;
        try { Tick(); }
        catch (Exception ex)
        {
            failed = true; Clear("Trail unavailable");
            Plugin.Instance?.Trace("TRAIL_FAULT " + ex);
        }
    }

    private void Tick()
    {
        var local = Character.localCharacter;
        if (local != localOwner) { Clear("OFF"); localOwner = local; }
        if (!feature.Value || Plugin.Instance == null || !Plugin.Instance.Enabled.Value || !local || !local.IsLocal || !Application.isFocused ||
            Time.timeScale == 0 || local.data.dead || local.data.passedOut || local.data.fullyPassedOut || !canInput(local))
        { Clear("OFF"); return; }
        var camera = Camera.main;
        if (!camera) { Clear("Camera unavailable"); return; }
        var keyboard = Keyboard.current;
        if (keyboard != null && key.Value != Key.None && keyboard[key.Value].wasPressedThisFrame)
        {
            if (target) { Clear("OFF"); return; }
            float bestDot = -1, bestDistance = float.PositiveInfinity;
            foreach (var candidate in Character.AllCharacters)
            {
                if (!candidate || candidate == local || candidate.IsLocal || candidate.data.dead || candidate.data.passedOut || candidate.data.fullyPassedOut) continue;
                Vector3 offset = candidate.transform.position - camera.transform.position;
                float distance = offset.magnitude, dot = Vector3.Dot(camera.transform.forward, offset.normalized);
                if (TrailHistory.EligibleTarget(distance, dot) && (dot > bestDot || (dot == bestDot && distance < bestDistance)))
                { target = candidate; bestDot = dot; bestDistance = distance; }
            }
            history.Clear();
            status = target ? "Recording" : "No teammate near crosshair";
        }
        if (!target) { if (history.Points.Count > 0) Clear("Target unavailable"); return; }
        if (!Character.AllCharacters.Contains(target) || target.data.dead || target.data.passedOut || target.data.fullyPassedOut)
        { Clear("Target unavailable"); return; }
        double now = Time.unscaledTime;
        var position = target.transform.position;
        history.Observe(now, position.x, position.y, position.z);
        if (now >= nextRender) { nextRender = now + .1; Render(now); }
    }

    private LineRenderer GetLine(int index)
    {
        if (material == null)
        {
            var shader = Shader.Find("Sprites/Default");
            if (!shader) throw new InvalidOperationException("Trail shader unavailable");
            material = new Material(shader);
        }
        while (lines.Count <= index)
        {
            var go = new GameObject("PeakSprintLock.TrailSegment");
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = material; line.useWorldSpace = true;
            line.widthMultiplier = .055f; line.numCapVertices = 2;
            lines.Add(line);
        }
        return lines[index];
    }

    private void Render(double now)
    {
        // Split at every discontinuity; cap pooled renderers even under repeated teleports.
        var points = history.Points;
        history.GetSegments(segments);
        int used = 0;
        foreach (var segment in segments)
        {
            int start = segment.Start, end = start + segment.Count;
            var line = GetLine(used++);
            line.positionCount = end - start;
            for (int i = start; i < end; i++)
                line.SetPosition(i - start, new Vector3(points[i].X, points[i].Y + .08f, points[i].Z));
            line.startColor = new Color(.55f, .95f, .45f, TrailHistory.Opacity(now - points[start].Time));
            line.endColor = new Color(.55f, .95f, .45f, TrailHistory.Opacity(now - points[end - 1].Time));
            line.enabled = true;
        }
        for (int i = used; i < lines.Count; i++) lines[i].enabled = false;
    }

    private void OnGUI()
    {
        if (feature == null || !feature.Value || Plugin.Instance == null || !Plugin.Instance.ShowHud.Value || !Character.localCharacter) return;
        string label = target ? $"TRAIL: {target.characterName} | {Vector3.Distance(Character.localCharacter.transform.position, target.transform.position):F0}m | {key.Value}: clear" : $"Trail: {status} | Look at teammate + {key.Value}";
        GUI.Box(new Rect(16, 50, 560, 30), label);
    }
    private void OnApplicationFocus(bool focused) { if (!focused) Clear("OFF"); }
    private void OnApplicationPause(bool paused) { if (paused) Clear("OFF"); }
    private void OnDisable() => Clear("OFF");
    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= SceneChanged;
        foreach (var line in lines) if (line) Destroy(line.gameObject);
        if (material) Destroy(material);
    }
}
