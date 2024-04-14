using System.Diagnostics;
using System;
using Terraria.ModLoader;
using System.Windows.Forms;

namespace BetterUI;
public class BetterUI : Mod
{
	public static BetterUI instance;
	public ModKeybind resourcePackKey;
	public ModKeybind settingsMenuKey;

	public override void Load()
	{
		instance = this;
		resourcePackKey = KeybindLoader.RegisterKeybind(this, "Open Resource Packs Menu", "F12");
		settingsMenuKey = KeybindLoader.RegisterKeybind(this, "Open Setting Menu (Rebind Inventory)", "Escape");
	}

	public override void Unload()
	{
		instance = null;
		MessageBox.Show("Due to tModLoader not unloading certain IL edits from " + (this.DisplayName) + ", tModLoader needs to be restarted.\n\nIf you are attempting to join a server, it is best you skip mod loading before joining.\nHold the 'Shift' key at the splash screen until you are at the main menu.\n\nPress OK to restart.", "tModLoader - " + this.DisplayName);
		Process.Start(Environment.ProcessPath, Environment.CommandLine);
		Environment.Exit(0);
	}
}