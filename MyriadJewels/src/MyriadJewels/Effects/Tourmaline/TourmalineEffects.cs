using HarmonyLib;
using JetBrains.Annotations;
using Jewelcrafting;
using UnityEngine;

namespace MyriadJewels.Effects.Tourmaline;

public static class TwinBladeAptitude
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class OffHandCatchUp
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class PairedRhythm
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}

	internal static int HitCount;
	internal static float Until;
}

public static class ClawRake
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

public static class KickThunder
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

public static class SledgeQuake
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

public static class GreatWeaponPoise
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

public static class GreatWeaponEconomy
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class GreatWeaponSpecial
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

public static class GreatWeaponCommit
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}

	internal static float Until;
}

[HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackStamina))]
internal static class TourmalineStamina
{
	private static void Postfix(Attack __instance, ref float __result)
	{
		if (__instance.m_character is not Player player) return;
		if (!SoftMods.IsTwoHandedNoShield(player)) return;
		float power = player.GetEffectPower<GreatWeaponEconomy.Config>("Great Weapon Economy").Power;
		if (power <= 0f) return;
		__result *= 100f / (100f + power);
	}
}

[HarmonyPatch(typeof(Character), nameof(Character.Damage))]
internal static class TourmalineDamage
{
	[HarmonyPriority(Priority.Last)]
	private static void Prefix(HitData hit)
	{
		if (hit.GetAttacker() is not Player player) return;
		float mul = 1f;

		if (SoftMods.IsDualWielding(player))
		{
			PairedRhythm.HitCount++;
			float rhythm = player.GetEffectPower<PairedRhythm.Config>("Paired Rhythm").Power
				+ player.GetEffectPower<Synergies.TwinTempest.Config>("Twin Tempest").Power;
			if (rhythm > 0f && PairedRhythm.HitCount >= 4)
			{
				PairedRhythm.HitCount = 0;
				PairedRhythm.Until = Time.time + 3f;
			}
			if (Time.time < PairedRhythm.Until)
				mul += rhythm / 100f;

			float off = player.GetEffectPower<OffHandCatchUp.Config>("Off-Hand Catch-Up").Power
				+ player.GetEffectPower<Synergies.TwinTempest.Config>("Twin Tempest").Power;
			if (off > 0f) mul += off / 200f;
		}

		if (SoftMods.IsTwoHandedNoShield(player))
		{
			float oath = player.GetEffectPower<Synergies.GreatOath.Config>("Great Oath").Power;
			float special = player.GetEffectPower<GreatWeaponSpecial.Config>("Great Weapon Special").Power + oath;
			float commit = player.GetEffectPower<GreatWeaponCommit.Config>("Great Weapon Commit").Power + oath;
			if (hit.m_skill == Skills.SkillType.Axes)
				mul += player.GetEffectPower<KickThunder.Config>("Kick Thunder").Power / 100f;
			if (hit.m_skill == Skills.SkillType.Clubs)
				mul += player.GetEffectPower<SledgeQuake.Config>("Sledge Quake").Power / 100f;
			if (hit.m_skill == Skills.SkillType.Unarmed)
				mul += player.GetEffectPower<ClawRake.Config>("Claw Rake").Power / 100f;
			if (special > 0f)
				mul += special / 200f;
			if (Time.time < GreatWeaponCommit.Until)
				mul += commit / 100f;
			else if (commit > 0f && hit.m_staggerMultiplier > 1f)
				GreatWeaponCommit.Until = Time.time + 2.5f;
		}

		if (mul != 1f) hit.ApplyModifier(mul);
	}
}

[HarmonyPatch(typeof(Character), nameof(Character.GetStaggerTreshold))]
internal static class TourmalinePoise
{
	private static void Postfix(Character __instance, ref float __result)
	{
		if (__instance is not Player player) return;
		if (!SoftMods.IsTwoHandedNoShield(player)) return;
		float power = player.GetEffectPower<GreatWeaponPoise.Config>("Great Weapon Poise").Power;
		if (power <= 0f) return;
		__result *= 1f + power / 100f;
	}
}

[HarmonyPatch(typeof(Skills), nameof(Skills.GetSkillFactor))]
internal static class TourmalineDualSkill
{
	private static void Postfix(Skills __instance, Skills.SkillType skillType, ref float __result)
	{
		Player? player = __instance.m_player;
		if (player == null) return;

		// Soft DualWield skill: name-based hash match when DualMastery present
		float twin = player.GetEffectPower<TwinBladeAptitude.Config>("Twin Blade Aptitude").Power;
		if (twin > 0f && SoftMods.DualMastery != null && SoftMods.IsDualWielding(player))
			__result += twin / 100f;
	}
}
