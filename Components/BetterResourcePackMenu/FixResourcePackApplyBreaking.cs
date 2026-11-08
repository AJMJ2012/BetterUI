using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader;

namespace BetterUI.Components.BetterResourcePackMenu;

// This is more of a band-aid fix to stop Terraria Ambiance from breaking the Resource Packs menu.
class FixResourcePackApplyBreaking : ModSystem
{
	static readonly MethodInfo UIResourcePackSelectionMenu_ApplyListChanges_Method = typeof(UIResourcePackSelectionMenu).GetMethod("ApplyListChanges", BindingFlags.NonPublic | BindingFlags.Instance);
	delegate void UIResourcePackSelectionMenu_ApplyListChanges_Delegate(UIResourcePackSelectionMenu self);
	public override void Load()
	{
		if (Main.netMode == NetmodeID.Server) return;
		MonoModHooks.Add(UIResourcePackSelectionMenu_ApplyListChanges_Method, Override_UIResourcePackSelectionMenu_ApplyListChanges);
	}

	static void Override_UIResourcePackSelectionMenu_ApplyListChanges(UIResourcePackSelectionMenu_ApplyListChanges_Delegate ApplyListChanges, UIResourcePackSelectionMenu self)
	{
		try
		{
			ApplyListChanges(self);
		}
		catch
		{
			Main.changeTheTitle = true;
			try
			{
				SoundEngine.Reload();
			}
			catch
			{
			}
		}
		return;
	}
}
