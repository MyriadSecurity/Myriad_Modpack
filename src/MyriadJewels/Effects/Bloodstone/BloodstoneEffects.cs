using HarmonyLib;
using JetBrains.Annotations;
using Jewelcrafting;
using UnityEngine;

namespace MyriadJewels.Effects.Bloodstone;

public static class ManicCycle
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

public static class SurgeDetonate
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

public static class OverchargeLean
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

public static class HighWaterAnchor
{
	[PublicAPI]
	public struct Config
	{
		[InverseMultiplicativePercentagePower] public float Power;
	}
}

public static class CrestAmp
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

public static class TrinketResonance
{
	[PublicAPI]
	public struct Config
	{
		[AdditivePower] public float Power;
	}
}

[HarmonyPatch(typeof(Character), nameof(Character.Damage))]
internal static class BloodstoneCombat
{
	[HarmonyPriority(Priority.VeryLow)]
	private static void Prefix(HitData hit)
	{
		if (hit.GetAttacker() is not Player player) return;
		float adren = SoftMods.GetAdrenaline(player);
		float mul = 1f;

		float manic = player.GetEffectPower<ManicCycle.Config>("Manic Cycle").Power
			+ player.GetEffectPower<Synergies.ManicEngine.Config>("Manic Engine").Power;
		if (manic > 0f)
			SoftMods.AddAdrenaline(player, manic * 0.05f);

		float crest = player.GetEffectPower<CrestAmp.Config>("Crest Amp").Power
			+ player.GetEffectPower<Synergies.CrestSovereign.Config>("Crest Sovereign").Power;
		float threshold = 60f;
		if (player.GetEffectPower<Synergies.CrestSovereign.Config>("Crest Sovereign").Power > 0f)
			threshold = 45f;
		if (crest > 0f && adren >= threshold)
			mul += crest / 100f;

		float resonance = player.GetEffectPower<TrinketResonance.Config>("Trinket Resonance").Power;
		if (resonance > 0f && adren >= threshold)
			mul += resonance / 200f;

		float lean = player.GetEffectPower<OverchargeLean.Config>("Overcharge Lean").Power;
		if (lean > 0f && adren >= 90f)
			mul += lean / 100f;

		float detonate = player.GetEffectPower<SurgeDetonate.Config>("Surge Detonate").Power
			+ player.GetEffectPower<Synergies.ManicEngine.Config>("Manic Engine").Power;
		if (detonate > 0f && adren >= 95f)
		{
			mul += detonate / 100f;
			SoftMods.AddAdrenaline(player, -Mathf.Min(adren, 25f));
		}

		var opposed = player.GetEffectPower<Synergies.OpposedHearts.Config>("Opposed Hearts");
		if (opposed.Power > 0f && adren >= 90f)
			mul += opposed.Power / 100f;

		if (mul != 1f) hit.ApplyModifier(mul);
	}
}

[HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
internal static class BloodstoneEdgeDefense
{
	[HarmonyPriority(Priority.Low)]
	private static void Prefix(Character __instance, HitData hit)
	{
		if (__instance is not Player player) return;
		float adren = SoftMods.GetAdrenaline(player);
		var opposed = player.GetEffectPower<Synergies.OpposedHearts.Config>("Opposed Hearts");
		if (opposed.Power > 0f && adren < 30f)
		{
			float scale = (100f - Mathf.Clamp(opposed.Power * 0.5f, 0f, 20f)) / 100f;
			hit.ApplyModifier(scale);
		}

		float anchor = player.GetEffectPower<HighWaterAnchor.Config>("High-Water Anchor").Power
			+ player.GetEffectPower<Synergies.CrestSovereign.Config>("Crest Sovereign").Power;
		// Decay resist is modeled as small incoming DR while topped — soft stand-in without BF drain hooks
		if (anchor > 0f && adren >= 70f)
			hit.ApplyModifier((100f - Mathf.Clamp(anchor * 0.25f, 0f, 20f)) / 100f);
	}
}

/// <summary>Pack &amp; Pulse + Fused Pulse: listen for high-adren windows as a Surge proxy.</summary>
[HarmonyPatch(typeof(Player), nameof(Player.FixedUpdate))]
internal static class BloodstonePulseWatcher
{
	private static float _lastAdren;
	private static bool _surged;

	private static void Postfix(Player __instance)
	{
		if (!__instance.IsOwner()) return;
		float adren = SoftMods.GetAdrenaline(__instance);
		float fused = __instance.GetEffectPower<Synergies.FusedPulse.Config>("Fused Pulse").Power;
		if (fused > 0f && adren >= 70f)
			__instance.m_eitrRegenDelay = Mathf.Max(0.1f, __instance.m_eitrRegenDelay * (100f - fused) / 100f);

		var pack = __instance.GetEffectPower<Synergies.PackAndPulse.Config>("Pack & Pulse");
		if (pack.Power > 0f && adren >= 90f && _lastAdren < 90f && !_surged)
		{
			_surged = true;
			Howlite.FamiliarController.Pulse(6f, pack.Power);
			Howlite.FamiliarController.Ensure(__instance);
		}
		if (adren < 50f) _surged = false;
		_lastAdren = adren;
	}
}
