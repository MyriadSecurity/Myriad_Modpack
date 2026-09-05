using HarmonyLib;
using UnityEngine;

namespace MyriadJewels;

/// <summary>
/// Mayehm custom factions skip vanilla <see cref="BaseAI.IsEnemy(Character, Character)"/>,
/// and this Valheim build no longer moves tames onto the Players faction. Tamed Bjorn
/// (and other commandable tames) then never treat hostiles as enemies.
/// Restore tamed-vs-hostile after other IsEnemy prefixes.
/// </summary>
[HarmonyPatch(typeof(BaseAI), nameof(BaseAI.IsEnemy), typeof(Character), typeof(Character))]
internal static class TamedCombatIsEnemy
{
	[HarmonyPriority(Priority.Last)]
	private static void Postfix(Character a, Character b, ref bool __result)
	{
		if (!a || !b || a == b) return;

		bool tamedA = a.IsTamed();
		bool tamedB = b.IsTamed();
		if (tamedA == tamedB) return;

		Character other = tamedA ? b : a;
		if (other is Player) { __result = false; return; }

		Character.Faction faction = other.GetFaction();
		if (faction is Character.Faction.Players or Character.Faction.PlayerSpawned)
		{
			__result = false;
			return;
		}

		__result = true;
	}
}
