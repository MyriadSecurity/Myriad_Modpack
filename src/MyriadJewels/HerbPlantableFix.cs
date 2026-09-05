using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace MyriadJewels;

/// <summary>
/// Amazing Nature sage/mint have real world pickables (<c>Pickable_*_bal</c>) and separate
/// walk-over ItemDrops (<c>*_bal</c>). PlantEverything ExtraResources can attach the Piece
/// to the ItemDrop instead, so the cultivator plants a pickup rather than an E-harvest plant.
/// Rebind cultivator tables to the pickable prefabs after other mods finish init.
/// </summary>
[HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
internal static class HerbPlantableFix
{
	private static readonly HerbDef[] Herbs =
	{
		new("Pickable_Sage_bal", "Sage_bal", "Sage", "Plant sage to grow more pickable sage."),
		new("Pickable_Mint_bal", "Mint_bal", "Mint", "Plant mint to grow more pickable mint."),
	};

	private readonly struct HerbDef
	{
		public readonly string PickableName;
		public readonly string ItemName;
		public readonly string PieceName;
		public readonly string PieceDescription;

		public HerbDef(string pickableName, string itemName, string pieceName, string pieceDescription)
		{
			PickableName = pickableName;
			ItemName = itemName;
			PieceName = pieceName;
			PieceDescription = pieceDescription;
		}
	}

	[HarmonyPriority(Priority.Last)]
	private static void Postfix(ZNetScene __instance)
	{
		__instance.StartCoroutine(CoFix());
	}

	private static IEnumerator CoFix()
	{
		for (int i = 0; i < 180; i++)
		{
			if (TryFix()) yield break;
			yield return null;
		}

		Plugin.Log.LogWarning("MyriadJewels: could not rebind sage/mint cultivator pickables.");
	}

	private static bool TryFix()
	{
		ZNetScene? zns = ZNetScene.instance;
		ObjectDB? odb = ObjectDB.instance;
		if (!zns || !odb) return false;

		List<PieceTable> tables = GetCultivatorTables(odb, zns);
		if (tables.Count == 0) return false;

		bool allFound = true;
		foreach (HerbDef herb in Herbs)
		{
			if (!FixHerb(zns, odb, tables, herb)) allFound = false;
		}

		return allFound;
	}

	private static bool FixHerb(ZNetScene zns, ObjectDB odb, List<PieceTable> tables, HerbDef herb)
	{
		GameObject item = odb.GetItemPrefab(herb.ItemName);
		GameObject? pickable = FindPickablePrefab(zns, herb.PickableName, item);
		if (!pickable || !item) return false;

		Pickable pick = pickable.GetComponent<Pickable>();
		if (!pick.m_itemPrefab) pick.m_itemPrefab = item;

		EnsureInZNetScene(zns, pickable);
		if (!pickable.GetComponent<Piece>()) EnsurePiece(pickable, item, zns, herb);

		foreach (PieceTable table in tables)
		{
			SwapCultivatorPrefab(table, herb.ItemName, pickable);
		}

		return true;
	}

	private static GameObject? FindPickablePrefab(ZNetScene zns, string pickableName, GameObject item)
	{
		GameObject fromScene = zns.GetPrefab(pickableName);
		if (IsWorldPickable(fromScene, item)) return fromScene;

		foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
		{
			if (!go || go.name != pickableName) continue;
			if (go.scene.IsValid() && !string.IsNullOrEmpty(go.scene.name)) continue;
			if (IsWorldPickable(go, item)) return go;
		}

		return null;
	}

	private static bool IsWorldPickable(GameObject go, GameObject item)
	{
		if (!go || go == item) return false;
		if (!go.GetComponent<Pickable>()) return false;
		return !go.GetComponent<ItemDrop>();
	}

	private static void EnsureInZNetScene(ZNetScene zns, GameObject pickable)
	{
		int hash = StableHash(pickable.name);
		if (zns.m_namedPrefabs.TryGetValue(hash, out GameObject existing) && existing == pickable)
			return;
		zns.m_namedPrefabs[hash] = pickable;
		if (!zns.m_prefabs.Contains(pickable)) zns.m_prefabs.Add(pickable);
	}

	private static void EnsurePiece(GameObject pickable, GameObject item, ZNetScene zns, HerbDef herb)
	{
		ItemDrop itemDrop = item.GetComponent<ItemDrop>();
		if (!itemDrop) return;

		Piece piece = pickable.AddComponent<Piece>();
		piece.m_name = herb.PieceName;
		piece.m_description = herb.PieceDescription;
		piece.m_category = Piece.PieceCategory.Misc;
		piece.m_cultivatedGroundOnly = false;
		piece.m_groundOnly = true;
		piece.m_groundPiece = true;
		piece.m_canBeRemoved = true;
		piece.m_targetNonPlayerBuilt = false;
		piece.m_icon = itemDrop.m_itemData.GetIcon();
		piece.m_resources = new[]
		{
			new Piece.Requirement
			{
				m_resItem = itemDrop,
				m_amount = 1,
				m_recover = false,
			},
		};

		Piece? placeFx = zns.GetPrefab("RaspberryBush")?.GetComponent<Piece>();
		if (placeFx) piece.m_placeEffect = placeFx.m_placeEffect;
	}

	private static void SwapCultivatorPrefab(PieceTable table, string itemName, GameObject pickable)
	{
		string pickableName = pickable.name;
		bool hasPickable = false;
		for (int i = table.m_pieces.Count - 1; i >= 0; i--)
		{
			GameObject go = table.m_pieces[i];
			if (!go) continue;
			string name = go.name.Replace("(Clone)", "").Trim();
			bool isItem = name == itemName || (go.GetComponent<ItemDrop>() && !go.GetComponent<Pickable>() && (name == itemName || name == pickableName));
			if (isItem)
			{
				if (hasPickable)
				{
					table.m_pieces.RemoveAt(i);
				}
				else
				{
					table.m_pieces[i] = pickable;
					hasPickable = true;
					Plugin.Log.LogInfo($"MyriadJewels: cultivator {itemName} rebound to {pickableName}");
				}
			}
			else if (name == pickableName)
			{
				if (hasPickable) table.m_pieces.RemoveAt(i);
				else
				{
					table.m_pieces[i] = pickable;
					hasPickable = true;
				}
			}
		}

		if (!hasPickable) table.m_pieces.Add(pickable);
	}

	private static List<PieceTable> GetCultivatorTables(ObjectDB odb, ZNetScene zns)
	{
		var tables = new List<PieceTable>();
		foreach (string name in new[] { "Cultivator", "BlackMetalCultivator_bal" })
		{
			GameObject? tool = odb.GetItemPrefab(name) ?? zns.GetPrefab(name);
			PieceTable? table = tool?.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_buildPieces;
			if (table && !tables.Contains(table)) tables.Add(table);
		}

		return tables;
	}

	private static int StableHash(string str)
	{
		int num = 5381;
		int num2 = 5381;
		for (int i = 0; i < str.Length; i++)
		{
			if ((i & 1) == 0) num = ((num << 5) + num) ^ str[i];
			else num2 = ((num2 << 5) + num2) ^ str[i];
		}

		return num + (num2 * 1566083941);
	}
}
