using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace BetterUI;

public class ClientConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;
	public static ClientConfig Instance;

	[DefaultValue(false)]
	public bool ModConfig_Compact = false;

	[DefaultValue(false)]
	public bool ModConfig_SmallButtons = false;

	[DefaultValue(false)]
	public bool ModConfig_OldIcon = false;

	[DefaultValue(true)]
	public bool ModConfig_ColourButtons = true;

	[DefaultValue(false)]
	public bool ModConfig_AllMods = false;

}

public static class Config
{
	public static ClientConfig Client => ClientConfig.Instance;
}
