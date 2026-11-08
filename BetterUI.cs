using System.Diagnostics;
using System;
using Terraria.ModLoader;
using System.Windows.Forms;
using Terraria;
using Terraria.ID;

namespace BetterUI;
public class BetterUI : Mod
{
	public static BetterUI instance;
	public ModKeybind resourcePackKey;
	public ModKeybind settingsMenuKey;

	public override void Load()
	{
		instance = this;
		if (Main.netMode == NetmodeID.Server) return;
		resourcePackKey = KeybindLoader.RegisterKeybind(this, "OpenResourcePacksMenu", "F12");
		settingsMenuKey = KeybindLoader.RegisterKeybind(this, "OpenSettingMenu", "Escape");
	}

	public override void Unload()
	{
		instance = null;
	}
}