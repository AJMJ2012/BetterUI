using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BetterUI.Components;

class FixOptionsMenuClose : ModSystem
{
	static readonly MethodInfo IngameOptions_Close_Method = typeof(IngameOptions).GetMethod("Close", BindingFlags.Public | BindingFlags.Static);
	delegate void IngameOptions_Close_Delegate();

	public override void Load()
	{
		if (Main.netMode == NetmodeID.Server) return;
		MonoModHooks.Add(IngameOptions_Close_Method, Override_IngameOptions_Close);
	}

	static void Override_IngameOptions_Close(IngameOptions_Close_Delegate Close)
	{
		Close();
		Main.inFancyUI = false;
		Main.playerInventory = false;
	}
}
