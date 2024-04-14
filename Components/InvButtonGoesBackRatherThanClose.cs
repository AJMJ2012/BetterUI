using Terraria.ID;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Cil;
using System;

namespace BetterUI.Components;


public class InvButtonGoesBackRatherThanClose : ModSystem
{
	static readonly MethodInfo Player_ToggleInv_Method = typeof(Player).GetMethod("ToggleInv", BindingFlags.Public | BindingFlags.Instance);
	delegate void Player_ToggleInv_Delegate(Player self);

	public override void Load()
	{
		try { HookEndpointManager.Modify<Player_ToggleInv_Delegate>(Player_ToggleInv_Method, IL_Player_ToggleInv); } catch { throw new Exception("IL Hook Failed"); }
	}

	public override void Unload()
	{
		try { HookEndpointManager.Unmodify<Player_ToggleInv_Delegate>(Player_ToggleInv_Method, IL_Player_ToggleInv); } catch { }
	}

	static void IL_Player_ToggleInv(ILContext il)
	{
		ILCursor c = new(il);
		/*
			// else if (Main.inFancyUI)
			// sequence point: hidden
			IL_0083: br IL_021c
			IL_0088: ldsfld bool Terraria.Main::inFancyUI
			IL_008d: brfalse.s IL_0099
			// IngameFancyUI.Close();
			IL_008f: call void Terraria.UI.IngameFancyUI::Close()
		*/
		if (!c.TryGotoNext(MoveType.After,
			i => i.MatchBr(out _),
			i => i.MatchLdsfld(typeof(Main), nameof(Main.inFancyUI)),
			i => i.MatchBrfalse(out _),
			i => i.MatchCall(typeof(IngameFancyUI), nameof(IngameFancyUI.Close))
		))
		{
			throw new Exception("IL Hook Failed");
		}

		c.Index--;
		c.Remove();
		c.EmitDelegate(() =>
		{
			if ((UserInterface.ActiveInstance.GetType().GetField("_history", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(UserInterface.ActiveInstance) as List<UIState>).Count > 1)
			{
				SoundEngine.PlaySound(SoundID.MenuClose);
				Main.InGameUI.GoBack();
			}
			else
			{
				IngameFancyUI.Close();
			}
		});
	}
}