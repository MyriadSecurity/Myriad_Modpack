using Jewelcrafting;
using UnityEngine;

namespace MyriadJewels;

internal static class Gems
{
	internal static void Register()
	{
		API.AddGems("Howlite", "howlite", new Color(0.92f, 0.93f, 0.90f, 1f));
		API.AddGems("Carnelian", "carnelian", new Color(0.82f, 0.28f, 0.14f, 1f));
		API.AddGems("Bloodstone", "bloodstone", new Color(0.35f, 0.12f, 0.14f, 1f));
		API.AddGems("Pyrite", "pyrite", new Color(0.85f, 0.72f, 0.22f, 1f));
		API.AddGems("Hematite", "hematite", new Color(0.35f, 0.36f, 0.40f, 1f));
		API.AddGems("Tourmaline", "tourmaline", new Color(0.72f, 0.18f, 0.42f, 1f));
		API.AddGems("Moss Agate", "mossagate", new Color(0.35f, 0.55f, 0.28f, 1f));
	}
}
