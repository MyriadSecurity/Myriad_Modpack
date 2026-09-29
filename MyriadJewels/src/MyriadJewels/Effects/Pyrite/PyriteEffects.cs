using HarmonyLib;
using JetBrains.Annotations;
using Jewelcrafting;
using UnityEngine;

namespace MyriadJewels.Effects.Pyrite;

public static class LightningProc
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
		[AdditivePower] public float Chance;
	}
}

public static class SpiritHex
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
		[AdditivePower] public float Chance;
	}
}

public static class PhysicalBurst
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
		[AdditivePower] public float Chance;
	}
}

public static class StaminaSiphon
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
		[AdditivePower] public float Chance;
	}
}

public static class EitrSiphon
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
		[AdditivePower] public float Chance;
	}
}

public static class AdrenalSpike
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
		[AdditivePower] public float Chance;
	}
}

[HarmonyPatch(typeof(Character), nameof(Character.Damage))]
internal static class PyriteProcs
{
	[HarmonyPriority(Priority.LowerThanNormal)]
	private static void Prefix(HitData hit)
	{
		if (hit.GetAttacker() is not Player player) return;
		float cascade = player.GetEffectPower<Synergies.Cascade.Config>("Cascade").Power;
		float total = hit.m_damage.GetTotalDamage();
		float roll = Random.value;

		var lightning = player.GetEffectPower<LightningProc.Config>("Lightning Proc");
		if (lightning.Power > 0f && roll <= (lightning.Chance + cascade) / 100f)
			hit.m_damage.m_lightning += total * lightning.Power / 100f;

		var hex = player.GetEffectPower<SpiritHex.Config>("Spirit Hex");
		if (hex.Power > 0f && Random.value <= (hex.Chance + cascade) / 100f)
			hit.m_damage.m_spirit += total * hex.Power / 100f;

		var burst = player.GetEffectPower<PhysicalBurst.Config>("Physical Burst");
		if (burst.Power > 0f && Random.value <= (burst.Chance + cascade) / 100f)
		{
			float a = total * burst.Power / 100f;
			hit.m_damage.m_slash += a * 0.34f;
			hit.m_damage.m_blunt += a * 0.33f;
			hit.m_damage.m_pierce += a * 0.33f;
		}

		var eitr = player.GetEffectPower<EitrSiphon.Config>("Eitr Siphon");
		if (eitr.Power > 0f && Random.value <= (eitr.Chance + cascade) / 100f)
			player.AddEitr(eitr.Power);

		var adren = player.GetEffectPower<AdrenalSpike.Config>("Adrenal Spike");
		if (adren.Power > 0f && Random.value <= (adren.Chance + cascade) / 100f)
			SoftMods.AddAdrenaline(player, adren.Power);

		var warpath = player.GetEffectPower<Synergies.Warpath.Config>("Warpath");
		if (warpath.Power > 0f && (SoftMods.IsDualWielding(player) || SoftMods.IsTwoHandedNoShield(player)))
			SoftMods.AddAdrenaline(player, warpath.Power);
	}
}

[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.BlockAttack))]
internal static class PyriteStaminaSiphon
{
	private static void Postfix(Humanoid __instance, bool __result)
	{
		if (!__result || __instance is not Player player) return;
		var cfg = player.GetEffectPower<StaminaSiphon.Config>("Stamina Siphon");
		float cascade = player.GetEffectPower<Synergies.Cascade.Config>("Cascade").Power;
		if (cfg.Power <= 0f) return;
		if (Random.value > (cfg.Chance + cascade) / 100f) return;
		player.AddStamina(cfg.Power);
	}
}
