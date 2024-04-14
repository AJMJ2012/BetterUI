using Terraria;
using Terraria.GameContent.UI.States;
using Terraria.Initializers;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterUI.Components;

public class OpenResourcePackMenuDirectly : ModSystem
{
	public override void PostUpdateInput()
	{
		if (!Main.gameMenu && !Main.inFancyUI && !Main.ingameOptionsWindow && BetterUI.instance.resourcePackKey.JustPressed)
		{
			IngameFancyUI.CoverNextFrame();
			Main.playerInventory = false;
			Main.editChest = false;
			Main.npcChatText = "";
			Main.inFancyUI = true;
			Main.InGameUI.SetState(new UIResourcePackSelectionMenu(new ForceResourcePackReload(), Main.AssetSourceController, AssetInitializer.CreateResourcePackList(Main.instance.Services)));
		}
	}
}