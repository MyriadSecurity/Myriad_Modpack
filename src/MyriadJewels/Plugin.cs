using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jewelcrafting;
using UnityEngine;

namespace MyriadJewels;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("org.bepinex.plugins.jewelcrafting", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
	public const string PluginGuid = "com.myriad.jewels";
	public const string PluginName = "MyriadJewels";
	public const string PluginVersion = "0.1.4";

	internal static ManualLogSource Log = null!;
	internal static Harmony Harmony = null!;

	private void Awake()
	{
		Log = Logger;
		if (!API.IsLoaded())
		{
			Log.LogError("Jewelcrafting API not loaded; MyriadJewels aborted.");
			return;
		}

		EffectRegistration.Register();
		Gems.Register();
		API.AddGemConfig(LoadEmbeddedYaml());

		Harmony = new Harmony(PluginGuid);
		foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
		{
			try
			{
				Harmony.CreateClassProcessor(type).Patch();
			}
			catch (Exception ex)
			{
				Log.LogError($"Harmony patch failed for {type.FullName}: {ex.Message}");
			}
		}
		Effects.Howlite.FamiliarController.HookRecalc();

		Log.LogInfo($"{PluginName} {PluginVersion} loaded (7 stones). SoftMods: {SoftMods.DumpAsmHint()}");
	}

	private static string LoadEmbeddedYaml()
	{
		Assembly asm = Assembly.GetExecutingAssembly();
		string? name = asm.GetManifestResourceNames()
			.FirstOrDefault(n => n.EndsWith("MyriadJewels.yaml", StringComparison.OrdinalIgnoreCase));
		if (name == null)
		{
			throw new InvalidOperationException("Embedded MyriadJewels.yaml not found.");
		}

		using Stream stream = asm.GetManifestResourceStream(name)!;
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}
}
