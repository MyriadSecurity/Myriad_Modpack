using HarmonyLib;
using JetBrains.Annotations;
using Jewelcrafting;
using UnityEngine;

namespace MyriadJewels.Effects.MossAgate;

public static class WellFedGrace
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

public static class Fieldcraft
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class Outrider
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class Craftmaster
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class ExpertRuneforger
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class ExpertAtgeir
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class ExpertUnarmed
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class ExpertSledge
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

[HarmonyPatch(typeof(Skills), nameof(Skills.GetSkillFactor))]
internal static class MossSkillBoosts
{
	private static void Postfix(Skills __instance, Skills.SkillType skillType, ref float __result)
	{
		Player? player = __instance.m_player;
		if (player == null) return;

		float rooted = player.GetEffectPower<Synergies.RootedCraft.Config>("Rooted Craft").Power;
		float fedBonus = 0f;
		if (IsWellFed(player))
		{
			fedBonus = player.GetEffectPower<WellFedGrace.Config>("Well Fed Grace").Power
				+ player.GetEffectPower<Synergies.GreenThumb.Config>("Green Thumb").Power;
		}

		float add = 0f;
		switch (skillType)
		{
			case Skills.SkillType.WoodCutting:
			case Skills.SkillType.Pickaxes:
				add += player.GetEffectPower<Fieldcraft.Config>("Fieldcraft").Power + rooted;
				break;
			case Skills.SkillType.Ride:
			case Skills.SkillType.Swim:
				add += player.GetEffectPower<Outrider.Config>("Outrider").Power + rooted;
				break;
			case Skills.SkillType.Polearms:
				add += player.GetEffectPower<ExpertAtgeir.Config>("Expert Atgeir").Power;
				break;
			case Skills.SkillType.Unarmed:
				add += player.GetEffectPower<ExpertUnarmed.Config>("Expert Unarmed").Power;
				break;
			case Skills.SkillType.Clubs:
				add += player.GetEffectPower<ExpertSledge.Config>("Expert Sledge").Power;
				break;
		}

		add += player.GetEffectPower<Craftmaster.Config>("Craftmaster").Power * 0.25f + rooted * 0.25f;
		add += player.GetEffectPower<ExpertRuneforger.Config>("Expert Runeforger").Power * 0.25f
			+ player.GetEffectPower<Synergies.RuneGrove.Config>("Rune Grove").Power * 0.25f;
		add += fedBonus * 0.25f;

		if (add > 0f) __result += add / 100f;
	}

	internal static bool IsWellFed(Player player)
	{
		foreach (Player.Food food in player.m_foods)
		{
			if (food.m_item != null) return true;
		}
		return false;
	}
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdateFood))]
internal static class FullLarderFoodDrain
{
	private static void Prefix(Player __instance)
	{
		float power = __instance.GetEffectPower<Synergies.FullLarder.Config>("Full Larder").Power;
		if (power <= 0f || !MossSkillBoosts.IsWellFed(__instance)) return;
		foreach (Player.Food food in __instance.m_foods)
			food.m_time = Mathf.Min(food.m_item.m_shared.m_foodBurnTime, food.m_time + power * 0.02f * Time.fixedDeltaTime);
	}
}
