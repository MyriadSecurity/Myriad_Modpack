using HarmonyLib;
using JetBrains.Annotations;
using Jewelcrafting;
using UnityEngine;

namespace MyriadJewels.Effects.Howlite;

public static class SpiritWolf
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

public static class CombatCrow
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

public static class SummonPotency
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

public static class TrophyHunter
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
		[InverseMultiplicativePercentagePower] public float Chance;
	}
}

internal static class FamiliarController
{
	private static GameObject? _wolf;
	private static GameObject? _crow;
	private static float _pulseUntil;

	internal static void HookRecalc()
	{
		API.OnEffectRecalc += () =>
		{
			Player? p = Player.m_localPlayer;
			if (p != null) Ensure(p);
		};
	}

	internal static void Ensure(Player player)
	{
		float wolfPower = player.GetEffectPower<SpiritWolf.Config>("Spirit Wolf").Power;
		float crowPower = player.GetEffectPower<CombatCrow.Config>("Combat Crow").Power;
		float potency = player.GetEffectPower<SummonPotency.Config>("Summon Potency").Power
			+ player.GetEffectPower<Synergies.FamiliarBond.Config>("Familiar Bond").Power;

		if (wolfPower > 0f)
			_wolf = Maintain(player, _wolf, "Wolf", wolfPower, potency);
		else
			_wolf = Despawn(_wolf);

		if (crowPower > 0f)
			_crow = Maintain(player, _crow, "Crow", crowPower, potency);
		else
			_crow = Despawn(_crow);
	}

	internal static void DespawnAll()
	{
		_wolf = Despawn(_wolf);
		_crow = Despawn(_crow);
	}

	internal static void Pulse(float seconds, float _)
	{
		_pulseUntil = Time.time + seconds;
	}

	internal static float PulseMul => Time.time < _pulseUntil ? 1.25f : 1f;

	private static GameObject? Maintain(Player player, GameObject? existing, string prefab, float power, float potency)
	{
		if (existing != null) return existing;
		GameObject? template = ZNetScene.instance?.GetPrefab(prefab);
		if (template == null)
		{
			Plugin.Log.LogWarning($"MyriadJewels: familiar prefab '{prefab}' missing.");
			return null;
		}

		Vector3 pos = player.transform.position + player.transform.forward * 2f;
		GameObject go = Object.Instantiate(template, pos, Quaternion.identity);
		if (go.GetComponent<Character>() is { } ch)
		{
			ch.m_faction = Character.Faction.Players;
			float scale = 1f + (power + potency) / 200f * PulseMul;
			ch.SetMaxHealth(ch.GetMaxHealth() * scale);
			ch.SetHealth(ch.GetMaxHealth());
		}
		Plugin.Log.LogInfo($"Spawned familiar {prefab} (power={power}, potency={potency}).");
		return go;
	}

	private static GameObject? Despawn(GameObject? go)
	{
		if (go != null) Object.Destroy(go);
		return null;
	}
}

[HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
internal static class FamiliarBootstrap
{
	private static void Postfix(Player __instance)
	{
		if (!__instance.IsOwner()) return;
		FamiliarController.Ensure(__instance);
	}
}

[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.OnDeath))]
internal static class FamiliarCleanup
{
	private static void Prefix(Humanoid __instance)
	{
		if (__instance is Player)
			FamiliarController.DespawnAll();
	}
}

[HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
internal static class TrophyHunterOnKill
{
	private static void Prefix(Character __instance)
	{
		Player? player = Player.m_localPlayer;
		if (player == null || __instance == player) return;
		var cfg = player.GetEffectPower<TrophyHunter.Config>("Trophy Hunter");
		float moon = player.GetEffectPower<Synergies.MoonHunt.Config>("Moon Hunt").Power;
		float chance = cfg.Power + (EnvMan.IsNight() ? moon : 0f);
		if (chance <= 0f) return;
		if (Random.value > chance / 100f) return;
		// Soft bonus: small heal/stamina reward standing in for trophy proc feedback
		player.AddStamina(Mathf.Clamp(chance * 0.25f, 1f, 10f));
	}
}

[HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetWeight))]
internal static class TrophyWeightReduce
{
	private static void Postfix(ItemDrop.ItemData __instance, ref float __result)
	{
		Player? player = Player.m_localPlayer;
		if (player == null) return;
		float reduce = player.GetEffectPower<TrophyHunter.Config>("Trophy Hunter").Chance;
		if (reduce <= 0f) return;
		if (__instance.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Material) return;
		if (__instance.m_shared.m_weight < 5f) return;
		__result *= (100f - Mathf.Clamp(reduce, 0f, 50f)) / 100f;
	}
}
