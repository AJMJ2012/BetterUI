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
		try { HookEndpointManager.Add(IngameOptions_Close_Method, Override_IngameOptions_Close); } catch { throw new Exception("Hook Failed"); }
	}

	public override void Unload()
	{
		try { HookEndpointManager.Remove(IngameOptions_Close_Method, Override_IngameOptions_Close); } catch { }
	}

	static void Override_IngameOptions_Close(IngameOptions_Close_Delegate Close)
	{
		Close();
		Main.inFancyUI = false;
		Main.playerInventory = false;
	}
}
