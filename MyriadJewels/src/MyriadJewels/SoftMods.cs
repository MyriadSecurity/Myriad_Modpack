using System.IO;
using System.Reflection;
using System.Text;

namespace MyriadJewels;

/// <summary>Soft reflection helpers for Battle Flow / DualMastery (no hard refs).</summary>
internal static class SoftMods
{
	private static Assembly? _battleFlow;
	private static Assembly? _dualMastery;
	private static bool _bfTried;
	private static bool _dmTried;

	internal static Assembly? BattleFlow
	{
		get
		{
			if (_bfTried) return _battleFlow;
			_bfTried = true;
			foreach (Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				if (a.GetName().Name == "BalrondBattleFlow")
				{
					_battleFlow = a;
					break;
				}
			}
			return _battleFlow;
		}
	}

	internal static Assembly? DualMastery
	{
		get
		{
			if (_dmTried) return _dualMastery;
			_dmTried = true;
			foreach (Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				if (a.GetName().Name == "BalrondDualMastery")
				{
					_dualMastery = a;
					break;
				}
			}
			return _dualMastery;
		}
	}

	internal static float GetAdrenaline(Player player)
	{
		try
		{
			Assembly? asm = BattleFlow;
			if (asm == null) return 0f;
			MethodInfo? m = asm.GetType("BalrondBattleFlow.AdrenalineRuntime")
				?.GetMethod("GetCurrentAdrenaline", BindingFlags.Public | BindingFlags.Static);
			if (m == null) return 0f;
			object? v = m.Invoke(null, new object[] { player });
			return v is float f ? f : 0f;
		}
		catch
		{
			return 0f;
		}
	}

	internal static void AddAdrenaline(Player player, float amount)
	{
		try
		{
			Assembly? asm = BattleFlow;
			if (asm == null) return;
			MethodInfo? m = asm.GetType("BalrondBattleFlow.AdrenalineRuntime")
				?.GetMethod("AddAdrenaline", BindingFlags.Public | BindingFlags.Static)
				?? asm.GetType("BalrondBattleFlow.AdrenalineRuntime")
					?.GetMethod("ModifyAdrenaline", BindingFlags.Public | BindingFlags.Static);
			m?.Invoke(null, new object[] { player, amount });
		}
		catch
		{
			// ignore
		}
	}

	internal static bool IsDualWielding(Player player)
	{
		try
		{
			Assembly? asm = DualMastery;
			if (asm == null) return false;
			MethodInfo? m = asm.GetType("BalrondDualWield.DualWieldMath")
				?.GetMethod("IsDualWieldingOneHandedWeapons", BindingFlags.Public | BindingFlags.Static);
			if (m == null) return false;
			object? v = m.Invoke(null, new object[] { player });
			return v is bool b && b;
		}
		catch
		{
			return false;
		}
	}

	internal static bool IsTwoHandedNoShield(Player player)
	{
		ItemDrop.ItemData? right = player.GetRightItem();
		if (right == null) return false;
		if (player.GetLeftItem() != null) return false;
		return right.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon;
	}

	internal static string DumpAsmHint()
	{
		var sb = new StringBuilder();
		sb.Append("BF=").Append(BattleFlow != null).Append(" DM=").Append(DualMastery != null);
		return sb.ToString();
	}
}
