using MonoMod.RuntimeDetour.HookGen;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.UI.States;
using System;
using Terraria.ID;

namespace BetterUI.Components.BetterResourcePackMenu;

class PauseInResourcePackMenu : ModSystem
{
	static readonly MethodInfo Main_CanPauseGame_Method = typeof(Main).GetMethod("CanPauseGame", BindingFlags.NonPublic | BindingFlags.Static);
	delegate bool Main_CanPauseGame_Delegate();

	public override void Load()
	{
		if (Main.netMode == NetmodeID.Server) return;
		MonoModHooks.Add(Main_CanPauseGame_Method, Override_Main_CanPauseGame);
	}

	static bool Override_Main_CanPauseGame(Main_CanPauseGame_Delegate CanPauseGame)
	{
		bool flag = CanPauseGame();
		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			flag |= Main.InGameUI.IsVisible && (Main.InGameUI.CurrentState is UIResourcePackSelectionMenu || Main.InGameUI.CurrentState is UIResourcePackInfoMenu);
		}
		return flag;
	}
}
