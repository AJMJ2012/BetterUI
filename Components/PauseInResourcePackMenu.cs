using MonoMod.RuntimeDetour.HookGen;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.UI.States;
using System;

namespace BetterUI.Components;

class PauseInResourcePackMenu : ModSystem
{
	static readonly MethodInfo Main_CanPauseGame_Method = typeof(Main).GetMethod("CanPauseGame", BindingFlags.NonPublic | BindingFlags.Static);
	delegate bool Main_CanPauseGame_Delegate();

	public override void Load()
	{
		try { HookEndpointManager.Add(Main_CanPauseGame_Method, Override_Main_CanPauseGame); } catch { throw new Exception("Hook Failed"); }
	}

	public override void Unload()
	{
		try { HookEndpointManager.Remove(Main_CanPauseGame_Method, Override_Main_CanPauseGame); } catch { }
	}

	static bool Override_Main_CanPauseGame(Main_CanPauseGame_Delegate CanPauseGame)
	{
		bool flag = CanPauseGame();
		if (Main.netMode == 0)
		{
			flag |= Main.InGameUI.IsVisible && (Main.InGameUI.CurrentState is UIResourcePackSelectionMenu || Main.InGameUI.CurrentState is UIResourcePackInfoMenu);
		}
		return flag;
	}
}
