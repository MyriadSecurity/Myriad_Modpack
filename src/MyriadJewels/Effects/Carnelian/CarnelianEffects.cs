using HarmonyLib;
using JetBrains.Annotations;
using Jewelcrafting;
using UnityEngine;

namespace MyriadJewels.Effects.Carnelian;

public static class SecondWind
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class CombatBreath
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}

	[HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackStamina))]
	private static class ReduceAttackStamina
	{
		private static void Postfix(Attack __instance, ref float __result)
		{
			if (__instance.m_character is not Player player) return;
			float power = player.GetEffectPower<Config>("Combat Breath").Power
				+ player.GetEffectPower<Synergies.SecondLung.Config>("Second Lung").Power;
			if (power <= 0f) return;
			__result *= 100f / (100f + power);
		}
	}

	[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.BlockAttack))]
	private static class ReduceBlockStamina
	{
		private static void Prefix(Humanoid __instance)
		{
			if (__instance is not Player player) return;
			float power = player.GetEffectPower<Config>("Combat Breath").Power
				+ player.GetEffectPower<Synergies.SecondLung.Config>("Second Lung").Power;
			if (power <= 0f) return;
			__instance.m_blockStaminaDrain *= 100f / (100f + power);
		}
	}
}

public static class EitrReservoir
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}

	[HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
	private static class IncreaseMaxEitr
	{
		private static void Postfix(Player __instance, ref float eitr)
		{
			float power = __instance.GetEffectPower<Config>("Eitr Reservoir").Power;
			float bonus = __instance.GetEffectPower<Synergies.OrangeReservoir.Config>("Orange Reservoir").Power;
			eitr += power + bonus;
		}
	}
}

public static class ClearHead
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdateStats), new System.Type[0])]
internal static class CarnelianUpdateStats
{
	private static void Prefix(Player __instance)
	{
		float secondWind = __instance.GetEffectPower<SecondWind.Config>("Second Wind").Power
			+ __instance.GetEffectPower<Synergies.SecondLung.Config>("Second Lung").Power;
		if (secondWind > 0f)
			__instance.m_staminaRegenDelay = Mathf.Max(0.1f, __instance.m_staminaRegenDelay * (100f - secondWind) / 100f);

		float clearHead = __instance.GetEffectPower<ClearHead.Config>("Clear Head").Power
			+ __instance.GetEffectPower<Synergies.OrangeReservoir.Config>("Orange Reservoir").Power;
		if (clearHead > 0f)
			__instance.m_eitrRegenDelay = Mathf.Max(0.1f, __instance.m_eitrRegenDelay * (100f - clearHead) / 100f);
	}
}
