using HarmonyLib;
using JetBrains.Annotations;
using Jewelcrafting;
using UnityEngine;

namespace MyriadJewels.Effects.Hematite;

public static class AddSlash
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
		[AdditivePower] public float Chance;
	}
}

public static class AddBlunt
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
		[AdditivePower] public float Chance;
	}
}

public static class AddPierce
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
		[AdditivePower] public float Chance;
	}
}

public static class AddLightning
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
		[AdditivePower] public float Chance;
	}
}

public static class AddSpirit
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
		[AdditivePower] public float Chance;
	}
}

public static class AddChopPick
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
		[AdditivePower] public float Chance;
	}
}

public static class ResistSlash
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class ResistBlunt
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class ResistPierce
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class ResistFire
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class ResistFrost
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class ResistLightning
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class ResistPoison
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class ResistSpirit
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

[HarmonyPatch(typeof(Character), nameof(Character.Damage))]
internal static class HematiteTypedDamage
{
	private static void Prefix(HitData hit)
	{
		if (hit.GetAttacker() is not Player player) return;

		float cascade = player.GetEffectPower<Synergies.Cascade.Config>("Cascade").Power;
		float total = hit.m_damage.GetTotalDamage();
		float rollPhys = Random.value;
		float rollElem = Random.value;

		var slash = player.GetEffectPower<AddSlash.Config>("Add Slash");
		if (slash.Power > 0f && rollPhys <= (slash.Chance + cascade) / 100f)
			hit.m_damage.m_slash += total * slash.Power / 100f;

		var blunt = player.GetEffectPower<AddBlunt.Config>("Add Blunt");
		if (blunt.Power > 0f && rollPhys <= (blunt.Chance + cascade) / 100f)
			hit.m_damage.m_blunt += total * blunt.Power / 100f;

		var pierce = player.GetEffectPower<AddPierce.Config>("Add Pierce");
		if (pierce.Power > 0f && rollPhys <= (pierce.Chance + cascade) / 100f)
			hit.m_damage.m_pierce += total * pierce.Power / 100f;

		var lightning = player.GetEffectPower<AddLightning.Config>("Add Lightning");
		if (lightning.Power > 0f && rollElem <= (lightning.Chance + cascade) / 100f)
			hit.m_damage.m_lightning += total * lightning.Power / 100f;

		var spirit = player.GetEffectPower<AddSpirit.Config>("Add Spirit");
		if (spirit.Power > 0f && rollElem <= (spirit.Chance + cascade) / 100f)
			hit.m_damage.m_spirit += total * spirit.Power / 100f;

		var tool = player.GetEffectPower<AddChopPick.Config>("Add Chop Pick");
		if (tool.Power > 0f && rollPhys <= (tool.Chance + cascade) / 100f)
		{
			float a = total * tool.Power / 100f;
			hit.m_damage.m_chop += a;
			hit.m_damage.m_pickaxe += a;
		}
	}
}

[HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
internal static class HematiteResists
{
	private static void Prefix(Character __instance, HitData hit)
	{
		if (__instance is not Player player) return;
		if (hit.GetAttacker() is not { } attacker || attacker == __instance) return;

		float lattice = player.GetEffectPower<Synergies.IronLattice.Config>("Iron Lattice").Power;

		Scale(ref hit.m_damage.m_slash, player.GetEffectPower<ResistSlash.Config>("Resist Slash").Power + lattice);
		Scale(ref hit.m_damage.m_blunt, player.GetEffectPower<ResistBlunt.Config>("Resist Blunt").Power + lattice);
		Scale(ref hit.m_damage.m_pierce, player.GetEffectPower<ResistPierce.Config>("Resist Pierce").Power + lattice);
		Scale(ref hit.m_damage.m_fire, player.GetEffectPower<ResistFire.Config>("Resist Fire").Power + lattice);
		Scale(ref hit.m_damage.m_frost, player.GetEffectPower<ResistFrost.Config>("Resist Frost").Power + lattice);
		Scale(ref hit.m_damage.m_lightning, player.GetEffectPower<ResistLightning.Config>("Resist Lightning").Power + lattice);
		Scale(ref hit.m_damage.m_poison, player.GetEffectPower<ResistPoison.Config>("Resist Poison").Power + lattice);
		Scale(ref hit.m_damage.m_spirit, player.GetEffectPower<ResistSpirit.Config>("Resist Spirit").Power + lattice);
	}

	private static void Scale(ref float value, float resistPower)
	{
		if (value <= 0f || resistPower <= 0f) return;
		value *= (100f - Mathf.Clamp(resistPower, 0f, 90f)) / 100f;
	}
}
