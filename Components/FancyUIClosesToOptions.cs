using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace BetterUI.Components;

class FancyUIClosesToOptions : ModSystem
{
 static readonly MethodInfo IngameFancyUI_Close_Method = typeof(IngameFancyUI).GetMethod("Close", BindingFlags.Public | BindingFlags.Static);
	delegate bool IngameFancyUI_Close_Delegate();

	public override void Load()
	{
		try { HookEndpointManager.Add(IngameFancyUI_Close_Method, Override_IngameFancyUI_Close); } catch { throw new Exception("Hook Failed"); }
	}

	public override void Unload()
	{
		try { HookEndpointManager.Remove(IngameFancyUI_Close_Method, Override_IngameFancyUI_Close); } catch { }
	}


	static void Override_IngameFancyUI_Close(IngameFancyUI_Close_Delegate Close)
	{
		Main.inFancyUI = false;
		SoundEngine.PlaySound(SoundID.MenuClose);
		Main.LocalPlayer.releaseInventory = false;
		Main.InGameUI.SetState(null);
		UILinkPointNavigator.Shortcuts.FANCYUI_SPECIAL_INSTRUCTIONS = 0;
		// TODO: Add check if options menu was open
		if (!Main.gameMenu)
		{
			Main.ingameOptionsWindow = true;
		}
	}
}