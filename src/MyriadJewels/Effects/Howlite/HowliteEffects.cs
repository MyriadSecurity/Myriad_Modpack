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

internal enum FamiliarKind
{
	Wolf,
	Crow
}

internal sealed class MyriadFamiliar : MonoBehaviour
{
	internal FamiliarKind Kind;
}

internal static class FamiliarController
{
	private const float WolfCooldownBase = 90f;
	private const float CrowCooldownBase = 75f;
	private const float MinCooldown = 20f;

	private static GameObject? _wolf;
	private static GameObject? _crow;
	private static float _wolfCooldownUntil;
	private static float _crowCooldownUntil;
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
			_wolf = Maintain(player, _wolf, FamiliarKind.Wolf, "Wolf", wolfPower, potency, 1f, ref _wolfCooldownUntil);
		else if (_wolf != null)
			_wolf = Despawn(_wolf, applyCooldown: false);

		if (crowPower > 0f)
			_crow = Maintain(player, _crow, FamiliarKind.Crow, "GiantCrow_bal", crowPower, potency, 0.5f, ref _crowCooldownUntil);
		else if (_crow != null)
			_crow = Despawn(_crow, applyCooldown: false);
	}

	internal static void DespawnAll(Player player)
	{
		if (_wolf != null)
		{
			LoseFamiliar(FamiliarKind.Wolf, player, applyCooldown: true);
			_wolf = Despawn(_wolf, applyCooldown: false);
		}
		if (_crow != null)
		{
			LoseFamiliar(FamiliarKind.Crow, player, applyCooldown: true);
			_crow = Despawn(_crow, applyCooldown: false);
		}
	}

	internal static void OnFamiliarDeath(Character familiar)
	{
		if (!familiar.TryGetComponent<MyriadFamiliar>(out MyriadFamiliar marker)) return;
		Player? player = Player.m_localPlayer;
		if (player == null || !player.IsOwner()) return;

		LoseFamiliar(marker.Kind, player, applyCooldown: true);
		if (marker.Kind == FamiliarKind.Wolf) _wolf = null;
		else _crow = null;
	}

	private static void LoseFamiliar(FamiliarKind kind, Player player, bool applyCooldown)
	{
		if (!applyCooldown) return;
		float cd = RespawnCooldown(player, kind == FamiliarKind.Wolf ? WolfCooldownBase : CrowCooldownBase);
		if (kind == FamiliarKind.Wolf) _wolfCooldownUntil = Time.time + cd;
		else _crowCooldownUntil = Time.time + cd;
	}

	private static float RespawnCooldown(Player player, float baseSeconds)
	{
		float reduce = player.GetEffectPower<SummonPotency.Config>("Summon Potency").Power
			+ player.GetEffectPower<Synergies.FamiliarBond.Config>("Familiar Bond").Power;
		return Mathf.Max(MinCooldown, baseSeconds * 100f / (100f + reduce));
	}

	internal static bool IsFamiliar(Character character) => character.GetComponent<MyriadFamiliar>() != null;

	internal static void Pulse(float seconds, float _)
	{
		_pulseUntil = Time.time + seconds;
	}

	internal static float PulseMul => Time.time < _pulseUntil ? 1.25f : 1f;

	internal static void TickFollow(Player player)
	{
		TickFollowOne(player, ref _wolf, FamiliarKind.Wolf, 2f, 0f);
		TickFollowOne(player, ref _crow, FamiliarKind.Crow, 3f, 1.5f);
	}

	private static void TickFollowOne(Player player, ref GameObject? familiar, FamiliarKind kind, float radius, float height)
	{
		if (familiar == null) return;
		if (!familiar)
		{
			LoseFamiliar(kind, player, applyCooldown: true);
			familiar = null;
			return;
		}

		Vector3 target = player.transform.position
			+ player.transform.forward * -radius
			+ Vector3.up * height;
		Character? ch = familiar.GetComponent<Character>();
		if (ch == null)
		{
			familiar.transform.position = Vector3.Lerp(familiar.transform.position, target, Time.deltaTime * 2f);
			return;
		}

		if (ch.GetComponent<MonsterAI>() is { } ai && ai.GetFollowTarget() != player.gameObject)
			ai.SetFollowTarget(player.gameObject);

		if (Vector3.Distance(ch.transform.position, player.transform.position) > 40f)
			ch.transform.position = target;
	}

	private static GameObject? Maintain(
		Player player,
		GameObject? existing,
		FamiliarKind kind,
		string prefab,
		float power,
		float potency,
		float sizeScale,
		ref float cooldownUntil)
	{
		if (existing != null && !existing)
			existing = null;
		if (existing != null) return existing;
		if (Time.time < cooldownUntil) return null;

		GameObject? template = ZNetScene.instance?.GetPrefab(prefab);
		if (template == null)
		{
			Plugin.Log.LogWarning($"MyriadJewels: familiar prefab '{prefab}' missing.");
			return null;
		}

		Vector3 pos = player.transform.position + player.transform.forward * 2f + Vector3.up * (sizeScale < 1f ? 1.5f : 0f);
		GameObject go = Object.Instantiate(template, pos, Quaternion.identity);
		go.AddComponent<MyriadFamiliar>().Kind = kind;
		if (go.GetComponent<Character>() is { } ch)
		{
			ch.m_faction = Character.Faction.Players;
			float scale = sizeScale * (1f + (power + potency) / 200f * PulseMul);
			ch.SetMaxHealth(ch.GetMaxHealth() * Mathf.Max(scale, 0.25f));
			ch.SetHealth(ch.GetMaxHealth());
			if (go.GetComponent<MonsterAI>() is { } ai)
			{
				ai.SetFollowTarget(player.gameObject);
				ai.m_alerted = true;
			}
		}

		if (go.GetComponent<CharacterDrop>() is { } characterDrop)
			characterDrop.m_dropsEnabled = false;
		foreach (DropOnDestroyed dropOnDestroyed in go.GetComponentsInChildren<DropOnDestroyed>(true))
			Object.Destroy(dropOnDestroyed);

		Plugin.Log.LogInfo($"Spawned familiar {prefab} (power={power}, potency={potency}).");
		return go;
	}

	private static GameObject? Despawn(GameObject? go, bool applyCooldown)
	{
		if (go != null)
		{
			if (applyCooldown && Player.m_localPlayer is { } player)
			{
				if (go.TryGetComponent<MyriadFamiliar>(out MyriadFamiliar marker))
					LoseFamiliar(marker.Kind, player, applyCooldown: true);
			}
			Object.Destroy(go);
		}
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

[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem), typeof(ItemDrop.ItemData), typeof(bool))]
internal static class FamiliarOnEquip
{
	private static void Postfix(Humanoid __instance)
	{
		if (__instance is Player { } player && player.IsOwner())
			FamiliarController.Ensure(player);
	}
}

[HarmonyPatch(typeof(Game), nameof(Game.Update))]
internal static class FamiliarFollowTick
{
	private static void Postfix()
	{
		Player? player = Player.m_localPlayer;
		if (player == null || !player.IsOwner()) return;
		FamiliarController.TickFollow(player);
	}
}

[HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
internal static class FamiliarOnDeath
{
	private static void Prefix(Character __instance)
	{
		if (!FamiliarController.IsFamiliar(__instance)) return;
		if (__instance.GetComponent<CharacterDrop>() is { } drop)
			drop.m_dropsEnabled = false;
		FamiliarController.OnFamiliarDeath(__instance);
	}
}

[HarmonyPatch(typeof(DropOnDestroyed), "OnDestroyed")]
internal static class FamiliarNoDropOnDestroyed
{
	private static bool Prefix(DropOnDestroyed __instance)
	{
		Character? ch = __instance.GetComponentInParent<Character>();
		return ch == null || !FamiliarController.IsFamiliar(ch);
	}
}

[HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
internal static class FamiliarNoCharacterDrop
{
	private static bool Prefix(CharacterDrop __instance)
	{
		Character? ch = __instance.GetComponent<Character>();
		return ch == null || !FamiliarController.IsFamiliar(ch);
	}
}

[HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
internal static class TrophyHunterOnKill
{
	private static void Prefix(Character __instance)
	{
		if (__instance is Player localPlayer && localPlayer.IsOwner())
		{
			FamiliarController.DespawnAll(localPlayer);
			return;
		}

		if (FamiliarController.IsFamiliar(__instance)) return;

		Player? player = Player.m_localPlayer;
		if (player == null) return;
		var cfg = player.GetEffectPower<TrophyHunter.Config>("Trophy Hunter");
		float moon = player.GetEffectPower<Synergies.MoonHunt.Config>("Moon Hunt").Power;
		float chance = cfg.Power + (EnvMan.IsNight() ? moon : 0f);
		if (chance <= 0f) return;
		if (Random.value > chance / 100f) return;
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
