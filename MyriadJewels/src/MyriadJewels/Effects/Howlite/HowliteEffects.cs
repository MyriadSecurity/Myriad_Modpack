using System;
using System.Collections.Generic;
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
	internal long OwnerId;
	internal float TargetDamage;
}

internal static class FamiliarController
{
	private const float WolfCooldownBase = 90f;
	private const float CrowCooldownBase = 75f;
	private const float MinCooldown = 20f;
	private const string ZdoFamiliarKey = "Myriad.Familiar";
	private const string ZdoOwnerKey = "Myriad.FamiliarOwner";
	private const string ZdoDamageKey = "Myriad.FamiliarDamage";
	private const string ZdoScaleKey = "Myriad.FamiliarScale";
	private const string WolfPrefab = "Wolf_cub";
	private const string WolfPrefabFallback = "Wolf";
	private const string CrowPrefab = "CrowNestling_bal";
	private const string CrowPrefabFallback = "GiantCrow_bal";

	private static GameObject? _wolf;
	private static GameObject? _crow;
	private static float _wolfCooldownUntil;
	private static float _crowCooldownUntil;
	private static float _pulseUntil;
	private static float _nextSweep;

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
			_wolf = Maintain(player, _wolf, FamiliarKind.Wolf, wolfPower, potency, ref _wolfCooldownUntil);
		else
		{
			_wolf = Despawn(_wolf, applyCooldown: false);
			DestroyOwnedKind(player.GetPlayerID(), FamiliarKind.Wolf, except: null);
		}

		if (crowPower > 0f)
			_crow = Maintain(player, _crow, FamiliarKind.Crow, crowPower, potency, ref _crowCooldownUntil);
		else
		{
			_crow = Despawn(_crow, applyCooldown: false);
			DestroyOwnedKind(player.GetPlayerID(), FamiliarKind.Crow, except: null);
		}
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
		DestroyOwnedKind(player.GetPlayerID(), FamiliarKind.Wolf, except: null);
		DestroyOwnedKind(player.GetPlayerID(), FamiliarKind.Crow, except: null);
	}

	internal static void OnFamiliarDeath(Character familiar)
	{
		if (!TryGetFamiliar(familiar, out FamiliarKind kind, out long ownerId)) return;
		Player? player = Player.m_localPlayer;
		if (player == null || !player.IsOwner()) return;
		if (ownerId != 0 && ownerId != player.GetPlayerID()) return;
		if (ownerId == 0)
		{
			bool ours = (kind == FamiliarKind.Wolf && _wolf && familiar.gameObject == _wolf)
				|| (kind == FamiliarKind.Crow && _crow && familiar.gameObject == _crow);
			if (!ours) return;
		}

		LoseFamiliar(kind, player, applyCooldown: true);
		if (kind == FamiliarKind.Wolf) _wolf = null;
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

	internal static bool IsFamiliar(Character character) => TryGetFamiliar(character, out _, out _);

	internal static bool TryGetFamiliar(Character character, out FamiliarKind kind, out long ownerId)
	{
		kind = FamiliarKind.Wolf;
		ownerId = 0;
		if (!character) return false;

		bool fromZdo = false;
		ZNetView? nv = character.m_nview;
		if (nv != null && nv.IsValid())
		{
			ZDO zdo = nv.GetZDO();
			int coded = zdo.GetInt(ZdoFamiliarKey);
			if (coded > 0)
			{
				kind = coded == 2 ? FamiliarKind.Crow : FamiliarKind.Wolf;
				ownerId = zdo.GetLong(ZdoOwnerKey);
				fromZdo = true;
			}
		}

		if (character.TryGetComponent<MyriadFamiliar>(out MyriadFamiliar marker))
		{
			if (fromZdo)
			{
				marker.Kind = kind;
				if (ownerId != 0) marker.OwnerId = ownerId;
			}
			else
			{
				kind = marker.Kind;
				ownerId = marker.OwnerId;
			}
			return true;
		}

		return fromZdo;
	}

	internal static void TryBind(Character character)
	{
		if (!character || !TryGetFamiliar(character, out FamiliarKind kind, out long ownerId)) return;
		Dress(character, kind, ownerId);
	}

	internal static void Pulse(float seconds, float _)
	{
		_pulseUntil = Time.time + seconds;
	}

	internal static float PulseMul => Time.time < _pulseUntil ? 1.25f : 1f;

	internal static void TickFollow(Player player)
	{
		TickFollowOne(player, ref _wolf, FamiliarKind.Wolf, 2f, 0f);
		TickFollowOne(player, ref _crow, FamiliarKind.Crow, 3f, 1.5f);
		if (Time.time < _nextSweep) return;
		_nextSweep = Time.time + 2f;
		SweepOrphans();
		RetuneAlive(player);
	}

	private static void TickFollowOne(Player player, ref GameObject? familiar, FamiliarKind kind, float radius, float height)
	{
		if (!familiar)
		{
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

		Dress(ch, kind, player.GetPlayerID());
		if (ch.GetComponent<MonsterAI>() is { } ai)
		{
			if (ai.GetFollowTarget() != player.gameObject)
				ai.SetFollowTarget(player.gameObject);
			ai.m_alerted = true;
		}

		if (Vector3.Distance(ch.transform.position, player.transform.position) > 40f)
			TeleportFamiliar(ch, target);
	}

	private static void TeleportFamiliar(Character ch, Vector3 target)
	{
		ch.transform.position = target;
		if (ch.m_nview != null && ch.m_nview.IsValid() && ch.m_nview.IsOwner())
			ch.m_nview.GetZDO().SetPosition(target);
	}

	private static GameObject? Maintain(
		Player player,
		GameObject? existing,
		FamiliarKind kind,
		float power,
		float potency,
		ref float cooldownUntil)
	{
		if (existing != null && !existing)
			existing = null;
		if (existing != null)
		{
			ApplyStats(existing, player, kind, power, potency, fillHealth: false);
			return existing;
		}
		if (Time.time < cooldownUntil) return null;
		if (player.GetPlayerID() == 0L || ZNetScene.instance == null) return null;

		DestroyOwnedKind(player.GetPlayerID(), kind, except: null);

		string prefab = kind == FamiliarKind.Crow ? CrowPrefab : WolfPrefab;
		string fallback = kind == FamiliarKind.Crow ? CrowPrefabFallback : WolfPrefabFallback;
		GameObject? template = ZNetScene.instance?.GetPrefab(prefab)
			?? ZNetScene.instance?.GetPrefab(fallback);
		if (template == null)
		{
			Plugin.Log.LogWarning($"MyriadJewels: familiar prefab '{prefab}' / '{fallback}' missing.");
			return null;
		}

		ComputeStats(player, kind, power, potency, out float hp, out float damage, out float visualScale);
		Vector3 pos = player.transform.position + player.transform.forward * 2f
			+ Vector3.up * (kind == FamiliarKind.Crow ? 1.2f : 0f);
		GameObject go = UnityEngine.Object.Instantiate(template, pos, Quaternion.identity);
		StripRanchBehaviors(go);
		long ownerId = player.GetPlayerID();
		if (!MarkZdo(go, kind, ownerId, damage, visualScale))
		{
			Plugin.Log.LogWarning($"MyriadJewels: aborting {template.name} spawn, ZDO mark failed.");
			DestroyNetworked(go);
			return null;
		}
		ApplyStats(go, player, kind, power, potency, fillHealth: true);

		if (go.GetComponent<MonsterAI>() is { } ai)
		{
			ai.SetFollowTarget(player.gameObject);
			ai.m_alerted = true;
		}

		foreach (DropOnDestroyed dropOnDestroyed in go.GetComponentsInChildren<DropOnDestroyed>(true))
			UnityEngine.Object.DestroyImmediate(dropOnDestroyed);

		Plugin.Log.LogInfo($"Spawned familiar {template.name} hp={hp:0} dmg={damage:0.0} scale={visualScale:0.00} kit={KitPower(player):0} gem={power} pot={potency}.");
		return go;
	}

	private static void RetuneAlive(Player player)
	{
		float wolfPower = player.GetEffectPower<SpiritWolf.Config>("Spirit Wolf").Power;
		float crowPower = player.GetEffectPower<CombatCrow.Config>("Combat Crow").Power;
		float potency = player.GetEffectPower<SummonPotency.Config>("Summon Potency").Power
			+ player.GetEffectPower<Synergies.FamiliarBond.Config>("Familiar Bond").Power;
		if (wolfPower > 0f && _wolf)
			ApplyStats(_wolf, player, FamiliarKind.Wolf, wolfPower, potency, fillHealth: false);
		if (crowPower > 0f && _crow)
			ApplyStats(_crow, player, FamiliarKind.Crow, crowPower, potency, fillHealth: false);
	}

	private static void ApplyStats(GameObject go, Player player, FamiliarKind kind, float power, float potency, bool fillHealth)
	{
		if (!go) return;
		ComputeStats(player, kind, power, potency, out float hp, out float damage, out float visualScale);
		if (!MarkZdo(go, kind, player.GetPlayerID(), damage, visualScale)) return;
		if (go.GetComponent<Character>() is not { } ch) return;

		float oldMax = ch.GetMaxHealth();
		float oldHp = ch.GetHealth();
		ch.SetMaxHealth(hp);
		if (fillHealth)
			ch.SetHealth(hp);
		else if (hp > oldMax)
			ch.SetHealth(oldHp + (hp - oldMax));
		else
			ch.SetHealth(Mathf.Min(oldHp, hp));

		Dress(ch, kind, player.GetPlayerID());
	}

	private static float KitPower(Player player)
	{
		float armor = player.GetBodyArmor();
		float eitr = player.GetMaxEitr();
		return Mathf.Max(armor, eitr * 0.5f);
	}

	private static void ComputeStats(Player player, FamiliarKind kind, float power, float potency, out float hp, out float damage, out float visualScale)
	{
		float kit = KitPower(player);
		float curve = kit <= 100f ? kit / 100f : (100f + Mathf.Min((kit - 100f) * 0.25f, 20f)) / 100f;
		float gemMul = Mathf.Clamp(0.85f + (power - 15f) / 25f * 0.35f, 0.80f, 1.25f);
		float potMul = 1f + potency / 200f;
		float mul = gemMul * potMul;

		hp = (26f + 74f * curve) * mul;
		damage = (8f + 34f * curve) * mul;
		visualScale = 0.72f + 0.40f * curve + (gemMul - 1f) * 0.15f;
		if (kind == FamiliarKind.Crow)
		{
			hp *= 0.85f;
			damage *= 0.85f;
			visualScale *= 0.9f;
		}
		hp = Mathf.Clamp(hp, 16f, 125f);
		damage = Mathf.Clamp(damage, 5f, 50f);
		visualScale = Mathf.Clamp(visualScale, 0.65f, 1.18f);
	}

	private static void StripRanchBehaviors(GameObject go)
	{
		foreach (Growup growup in go.GetComponentsInChildren<Growup>(true))
			UnityEngine.Object.DestroyImmediate(growup);
		foreach (Procreation procreation in go.GetComponentsInChildren<Procreation>(true))
			UnityEngine.Object.DestroyImmediate(procreation);
		foreach (CharacterTimedDestruction timed in go.GetComponentsInChildren<CharacterTimedDestruction>(true))
			UnityEngine.Object.DestroyImmediate(timed);
	}

	private static bool MarkZdo(GameObject go, FamiliarKind kind, long ownerId, float damage, float visualScale)
	{
		ZNetView? nv = go.GetComponent<ZNetView>();
		if (nv == null || !nv.IsValid()) return false;
		if (!nv.IsOwner())
			nv.ClaimOwnership();
		if (!nv.IsOwner()) return false;

		ZDO zdo = nv.GetZDO();
		zdo.Persistent = false;
		nv.m_persistent = false;
		nv.m_syncInitialScale = true;
		zdo.Set(ZdoFamiliarKey, kind == FamiliarKind.Crow ? 2 : 1);
		zdo.Set(ZdoOwnerKey, ownerId);
		zdo.Set(ZdoDamageKey, damage);
		zdo.Set(ZdoScaleKey, visualScale);
		zdo.Set(ZDOVars.s_scaleScalarHash, visualScale);
		zdo.Set(ZDOVars.s_tamed, true);
		go.transform.localScale = Vector3.one * visualScale;
		return true;
	}

	internal static void ScaleOutgoingHit(HitData hit)
	{
		if (hit == null) return;
		Character? attacker = hit.GetAttacker();
		if (attacker == null) return;
		TryBind(attacker);
		if (!attacker.TryGetComponent<MyriadFamiliar>(out MyriadFamiliar fam))
		{
			if (IsFamiliar(attacker))
				hit.ApplyModifier(0.25f);
			return;
		}
		float total = hit.m_damage.GetTotalDamage();
		if (total <= 0.01f) return;
		if (fam.TargetDamage <= 0f)
		{
			hit.ApplyModifier(0.25f);
			return;
		}
		float target = fam.TargetDamage;
		Player? local = Player.m_localPlayer;
		if (local != null && fam.OwnerId == local.GetPlayerID())
			target *= PulseMul;
		float mul = target / total;
		hit.ApplyModifier(mul);
		hit.m_pushForce *= Mathf.Clamp(mul, 0.25f, 1f);
		hit.m_staggerMultiplier *= Mathf.Clamp(mul, 0.35f, 1f);
	}

	private static void Dress(Character ch, FamiliarKind kind, long ownerId)
	{
		if (!ch.TryGetComponent<MyriadFamiliar>(out MyriadFamiliar marker))
			marker = ch.gameObject.AddComponent<MyriadFamiliar>();
		marker.Kind = kind;
		marker.OwnerId = ownerId;

		ZNetView? nv = ch.m_nview;
		if (nv != null && nv.IsValid())
		{
			ZDO zdo = nv.GetZDO();
			float damage = zdo.GetFloat(ZdoDamageKey, marker.TargetDamage);
			if (damage > 0f) marker.TargetDamage = damage;
			float scale = zdo.GetFloat(ZdoScaleKey, 0f);
			if (scale > 0.1f)
				ch.transform.localScale = Vector3.one * scale;
		}

		ch.m_faction = Character.Faction.Players;
		ch.m_tamed = true;
		if (nv != null && nv.IsValid())
		{
			if (nv.IsOwner())
			{
				nv.GetZDO().Set(ZDOVars.s_tamed, true);
				if (!ch.IsTamed())
					ch.SetTamed(true);
			}
		}

		if (ch.GetComponent<CharacterDrop>() is { } characterDrop)
			characterDrop.m_dropsEnabled = false;

		Player? owner = FindOwner(ownerId);
		if (owner != null && ch.GetComponent<MonsterAI>() is { } ai && ai.GetFollowTarget() != owner.gameObject)
			ai.SetFollowTarget(owner.gameObject);
	}

	private static Player? FindOwner(long ownerId)
	{
		if (ownerId == 0) return null;
		Player? local = Player.m_localPlayer;
		if (local != null && local.GetPlayerID() == ownerId) return local;
		foreach (Player player in Player.GetAllPlayers())
		{
			if (player != null && player.GetPlayerID() == ownerId) return player;
		}
		return null;
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
			DestroyNetworked(go);
		}
		return null;
	}

	private static void DestroyOwnedKind(long ownerId, FamiliarKind kind, GameObject? except)
	{
		if (ownerId == 0) return;
		List<GameObject> doomed = new List<GameObject>();
		foreach (Character ch in Character.GetAllCharacters())
		{
			if (!ch || ch.gameObject == except) continue;
			if (!TryGetFamiliar(ch, out FamiliarKind found, out long foundOwner)) continue;
			if (found != kind || foundOwner != ownerId) continue;
			doomed.Add(ch.gameObject);
		}
		foreach (GameObject go in doomed)
			DestroyNetworked(go);
	}

	private static void SweepOrphans()
	{
		List<GameObject> doomed = new List<GameObject>();
		foreach (Character ch in Character.GetAllCharacters())
		{
			if (!ch) continue;
			TryBind(ch);
			if (!TryGetFamiliar(ch, out _, out long ownerId) || ownerId == 0) continue;
			if (FindOwner(ownerId) != null) continue;
			ZNetView? nv = ch.m_nview;
			if (nv == null || !nv.IsValid() || !nv.IsOwner()) continue;
			doomed.Add(ch.gameObject);
		}
		foreach (GameObject go in doomed)
			DestroyNetworked(go);
	}

	private static void DestroyNetworked(GameObject go)
	{
		if (!go) return;
		try
		{
			ZNetView? nv = go.GetComponent<ZNetView>();
			if (nv != null && nv.IsValid())
			{
				ZDO zdo = nv.GetZDO();
				zdo.Persistent = false;
				nv.m_persistent = false;
				if (!nv.IsOwner())
					nv.ClaimOwnership();
				if (nv.IsOwner() && ZNetScene.instance != null)
				{
					nv.Destroy();
					return;
				}
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning($"MyriadJewels: familiar despawn failed: {ex.Message}");
		}
		UnityEngine.Object.Destroy(go);
	}
}

[HarmonyPatch(typeof(Character), "Awake")]
internal static class FamiliarBindOnSpawn
{
	private static void Postfix(Character __instance) => FamiliarController.TryBind(__instance);
}

[HarmonyPatch(typeof(Character), nameof(Character.Damage))]
internal static class FamiliarOutgoingDamage
{
	private static void Prefix(HitData hit) => FamiliarController.ScaleOutgoingHit(hit);
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

[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem), typeof(ItemDrop.ItemData), typeof(bool))]
internal static class FamiliarOnUnequip
{
	private static void Postfix(Humanoid __instance)
	{
		if (__instance is Player { } player && player.IsOwner())
			FamiliarController.Ensure(player);
	}
}

[HarmonyPatch(typeof(Player), "OnDestroy")]
internal static class FamiliarOnPlayerDestroy
{
	private static void Prefix(Player __instance)
	{
		if (__instance == Player.m_localPlayer)
			FamiliarController.DespawnAll(__instance);
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
		if (UnityEngine.Random.value > chance / 100f) return;
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
