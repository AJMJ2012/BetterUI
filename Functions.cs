using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace BetterUI;

public static class Functions
{
	public static string GetName(this Mod mod)
		=> string.IsNullOrEmpty(mod.DisplayName?.Trim()) ? mod.Name : mod.DisplayName;
	public static string GetCleanName(this Mod mod) 
		=> string.IsNullOrEmpty(mod.DisplayNameClean?.Trim()) ? mod.Name : mod.DisplayNameClean;

	public static string GetName(this ModConfig config)
		=> string.IsNullOrEmpty(config.DisplayName.Value?.Trim()) || config.DisplayName.Value.StartsWith(string.Format("Mods.{0}", config.Mod.Name)) ? config.Name : config.DisplayName.Value;
}