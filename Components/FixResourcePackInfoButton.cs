using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterUI.Components;

class FixResourcePackInfoButton : ModSystem
{
	static readonly MethodInfo UIResourcePackSelectionMenu_Click_Info_Method = typeof(UIResourcePackSelectionMenu).GetMethod("Click_Info", BindingFlags.NonPublic | BindingFlags.Instance);
	delegate void UIResourcePackSelectionMenu_Click_Info_Delegate(UIResourcePackSelectionMenu self, UIMouseEvent evt, UIElement listeningElement);
	static readonly MethodInfo UIResourcePackInfoMenu_GoBackClick_Method = typeof(UIResourcePackInfoMenu).GetMethod("GoBackClick", BindingFlags.NonPublic | BindingFlags.Instance);
	delegate void UIResourcePackInfoMenu_GoBackClick_Delegate(UIResourcePackInfoMenu self, UIMouseEvent evt, UIElement listeningElement);

	public override void Load()
	{
		try { HookEndpointManager.Add(UIResourcePackSelectionMenu_Click_Info_Method, Override_UIResourcePackSelectionMenu_Click_Info); } catch { throw new Exception("Hook Failed"); }
		try { HookEndpointManager.Add(UIResourcePackInfoMenu_GoBackClick_Method, Override_UIResourcePackInfoMenu_GoBackClick); } catch { throw new Exception("Hook Failed"); }
	}

	public override void Unload()
	{
		try { HookEndpointManager.Remove(UIResourcePackSelectionMenu_Click_Info_Method, Override_UIResourcePackSelectionMenu_Click_Info); } catch { }
		try { HookEndpointManager.Remove(UIResourcePackInfoMenu_GoBackClick_Method, Override_UIResourcePackSelectionMenu_Click_Info); } catch { }
	}

	static void Override_UIResourcePackSelectionMenu_Click_Info(UIResourcePackSelectionMenu_Click_Info_Delegate Click_Info, UIResourcePackSelectionMenu self, UIMouseEvent evt, UIElement listeningElement)
	{
		if (listeningElement is UIResourcePackInfoButton<string> uiResourcePackInfoButton)
		{
			SoundEngine.PlaySound(SoundID.MenuOpen);
			if (Main.gameMenu)
			{
				Main.MenuUI.SetState(new UIResourcePackInfoMenu(self, uiResourcePackInfoButton.ResourcePack));
			}
			else
			{
				Main.InGameUI.SetState(new UIResourcePackInfoMenu(self, uiResourcePackInfoButton.ResourcePack));
			}
		}
	}

	static void Override_UIResourcePackInfoMenu_GoBackClick(UIResourcePackInfoMenu_GoBackClick_Delegate GoBackClick, UIResourcePackInfoMenu self, UIMouseEvent evt, UIElement listeningElement)
	{
		UIResourcePackSelectionMenu _resourceMenu = self.GetType().GetField("_resourceMenu", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self) as UIResourcePackSelectionMenu;
		SoundEngine.PlaySound(SoundID.MenuClose);
		if (Main.gameMenu)
		{
			Main.MenuUI.SetState(_resourceMenu);
		}
		else
		{
			Main.InGameUI.SetState(_resourceMenu);
		}
	}
}
