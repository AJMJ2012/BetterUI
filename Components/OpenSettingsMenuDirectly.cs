using System.Reflection;
using Terraria;
using Terraria.GameInput;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;

namespace BetterUI.Components;

public class OpenSettingsMenuDirectly : ModSystem
{
	public override void PostUpdateInput()
	{
		if (!Main.gameMenu && BetterUI.instance.settingsMenuKey.JustPressed)
		{
			if (Main.mapFullscreen || PlayerInput.InBuildingMode || Main.ingameOptionsWindow || Main.inFancyUI || CaptureManager.Instance.Active || Main.LocalPlayer.talkNPC >= 0 || Main.LocalPlayer.sign >= 0 || Main.clothesWindow || Main.playerInventory)
			{
				Main.LocalPlayer.ToggleInv();
			}
			else
			{
				IngameOptions.Open();
			}
		}
		if (!Main.inFancyUI)
		{
			UserInterface.ActiveInstance.GetType().GetField("_history", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(UserInterface.ActiveInstance, new List<UIState>());
		}
	}
}