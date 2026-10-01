using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Myriad.MapPinSelectorFix;

[BepInPlugin("myriad.mappinselectorfix", "Myriad Map Pin Selector Fix", "1.4.0")]
public class MapPinSelectorFixPlugin : BaseUnityPlugin
{
    internal static MapPinSelectorFixPlugin? Instance;
    internal static string LogPath = "";

    internal static ConfigEntry<bool> Enabled = null!;
    internal static ConfigEntry<float> OffsetX = null!;
    internal static ConfigEntry<float> OffsetY = null!;
    internal static ConfigEntry<float> Scale = null!;
    internal static ConfigEntry<bool> IncludePingPanel = null!;
    internal static ConfigEntry<bool> IncludeFilterPanel = null!;
    internal static ConfigEntry<bool> LogApplies = null!;

    private void Awake()
    {
        Instance = this;
        try
        {
            LogPath = Path.Combine(Paths.BepInExRootPath, "MapPinSelectorFix.log");
            File.WriteAllText(LogPath, $"[{DateTime.Now:O}] Awake reached (v1.4.0)\n");
        }
        catch { }

        Enabled = Config.Bind("PinSelector", "Enabled", true,
            "Apply manual position/scale to the large-map pin selector column.");
        OffsetX = Config.Bind("PinSelector", "OffsetX", -240f,
            "Horizontal offset in UI units from the game's default panel position. Negative moves left (toward the map). Positive moves right.");
        OffsetY = Config.Bind("PinSelector", "OffsetY", 0f,
            "Vertical offset in UI units from the game's default panel position. Positive moves up.");
        Scale = Config.Bind("PinSelector", "Scale", 1f,
            "Scale multiplier for the pin selector panels. 1 = default size.");
        IncludeFilterPanel = Config.Bind("PinSelector", "IncludeFilterPanel", true,
            "Also move/scale IconPanel2 (death/boss filter icons).");
        IncludePingPanel = Config.Bind("PinSelector", "IncludePingPanel", true,
            "Also move/scale IconPingPanel (ping icon).");
        LogApplies = Config.Bind("PinSelector", "LogApplies", false,
            "Write apply lines to BepInEx/MapPinSelectorFix.log.");

        OffsetX.SettingChanged += (_, __) => MapHooks.ApplyIfLargeOpen();
        OffsetY.SettingChanged += (_, __) => MapHooks.ApplyIfLargeOpen();
        Scale.SettingChanged += (_, __) => MapHooks.ApplyIfLargeOpen();
        Enabled.SettingChanged += (_, __) => MapHooks.ApplyIfLargeOpen();
        IncludeFilterPanel.SettingChanged += (_, __) => MapHooks.ApplyIfLargeOpen();
        IncludePingPanel.SettingChanged += (_, __) => MapHooks.ApplyIfLargeOpen();

        Invoke(nameof(InstallPatches), 0f);
    }

    private void InstallPatches()
    {
        try
        {
            var harmony = new Harmony("myriad.mappinselectorfix");
            var setMapMode = AccessTools.Method(typeof(Minimap), "SetMapMode");
            if (setMapMode == null)
            {
                FLog("FATAL: Minimap.SetMapMode not found");
                return;
            }
            harmony.Patch(setMapMode, postfix: new HarmonyMethod(typeof(MapHooks), nameof(MapHooks.SetMapModePostfix)));
            FLog($"InstallPatches OK  defaults OffsetX={OffsetX.Value} OffsetY={OffsetY.Value} Scale={Scale.Value}");
        }
        catch (Exception ex)
        {
            FLog("InstallPatches EX: " + ex);
        }
    }

    /// <summary>Invoked one frame after large map opens so other layout mods run first.</summary>
    public void ApplyNextFrame()
    {
        var map = Minimap.instance;
        if (map != null)
            MapHooks.Apply(map);
    }

    internal static void FLog(string msg)
    {
        var line = $"[{DateTime.Now:O}] {msg}";
        try
        {
            if (string.IsNullOrEmpty(LogPath))
                LogPath = Path.Combine(Paths.BepInExRootPath, "MapPinSelectorFix.log");
            File.AppendAllText(LogPath, line + "\n");
        }
        catch { }
    }
}

internal static class MapHooks
{
    private struct Baseline
    {
        public Vector2 Anchored;
        public Vector3 LocalScale;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 Pivot;
        public Vector2 SizeDelta;
    }

    private static readonly Dictionary<string, Baseline> Baselines = new Dictionary<string, Baseline>();
    private static bool _baselinesCaptured;

    public static void SetMapModePostfix(Minimap __instance, object mode)
    {
        int modeInt;
        try { modeInt = Convert.ToInt32(mode); }
        catch { return; }

        if (modeInt != 2 || __instance == null)
            return;

        if (MapPinSelectorFixPlugin.Instance != null)
            MapPinSelectorFixPlugin.Instance.Invoke(nameof(MapPinSelectorFixPlugin.ApplyNextFrame), 0f);
        else
            Apply(__instance);
    }

    public static void ApplyIfLargeOpen()
    {
        var map = Minimap.instance;
        if (map == null || map.m_largeRoot == null || !map.m_largeRoot.activeInHierarchy)
            return;
        Apply(map);
    }

    public static void Apply(Minimap map)
    {
        try
        {
            if (map.m_largeRoot == null) return;

            if (!MapPinSelectorFixPlugin.Enabled.Value)
            {
                RestoreBaselines(map);
                return;
            }

            CaptureBaselinesOnce(map);

            float ox = MapPinSelectorFixPlugin.OffsetX.Value;
            float oy = MapPinSelectorFixPlugin.OffsetY.Value;
            float scale = MapPinSelectorFixPlugin.Scale.Value;
            if (scale < 0.1f) scale = 0.1f;

            ApplyPanel(map, "IconPanel", ox, oy, scale, apply: true);
            ApplyPanel(map, "IconPanel2", ox, oy, scale, MapPinSelectorFixPlugin.IncludeFilterPanel.Value);
            ApplyPanel(map, "IconPingPanel", ox, oy, scale, MapPinSelectorFixPlugin.IncludePingPanel.Value);

            if (MapPinSelectorFixPlugin.LogApplies.Value)
                MapPinSelectorFixPlugin.FLog($"Applied Offset=({ox},{oy}) Scale={scale}");
        }
        catch (Exception ex)
        {
            MapPinSelectorFixPlugin.FLog("Apply EX: " + ex);
        }
    }

    private static void CaptureBaselinesOnce(Minimap map)
    {
        if (_baselinesCaptured) return;
        string[] names = { "IconPanel", "IconPanel2", "IconPingPanel" };
        foreach (var name in names)
        {
            var t = map.m_largeRoot!.transform.Find(name) as RectTransform;
            if (t == null) continue;
            Baselines[name] = new Baseline
            {
                Anchored = t.anchoredPosition,
                LocalScale = t.localScale,
                AnchorMin = t.anchorMin,
                AnchorMax = t.anchorMax,
                Pivot = t.pivot,
                SizeDelta = t.sizeDelta
            };
            MapPinSelectorFixPlugin.FLog(
                $"Baseline {name}: anchored={t.anchoredPosition} scale={t.localScale} anchor={t.anchorMin}-{t.anchorMax} size={t.sizeDelta}");
        }
        _baselinesCaptured = Baselines.Count > 0;
    }

    private static void ApplyPanel(Minimap map, string name, float ox, float oy, float scale, bool apply)
    {
        var t = map.m_largeRoot!.transform.Find(name) as RectTransform;
        if (t == null) return;
        if (!Baselines.TryGetValue(name, out var b)) return;

        t.anchorMin = b.AnchorMin;
        t.anchorMax = b.AnchorMax;
        t.pivot = b.Pivot;
        t.sizeDelta = b.SizeDelta;

        if (!apply)
        {
            t.anchoredPosition = b.Anchored;
            t.localScale = b.LocalScale;
            return;
        }

        t.gameObject.SetActive(true);
        t.anchoredPosition = b.Anchored + new Vector2(ox, oy);
        t.localScale = b.LocalScale * scale;
    }

    private static void RestoreBaselines(Minimap map)
    {
        if (!_baselinesCaptured || map.m_largeRoot == null) return;
        foreach (var kv in Baselines)
        {
            var t = map.m_largeRoot.transform.Find(kv.Key) as RectTransform;
            if (t == null) continue;
            var b = kv.Value;
            t.anchorMin = b.AnchorMin;
            t.anchorMax = b.AnchorMax;
            t.pivot = b.Pivot;
            t.anchoredPosition = b.Anchored;
            t.localScale = b.LocalScale;
            t.sizeDelta = b.SizeDelta;
        }
    }
}
