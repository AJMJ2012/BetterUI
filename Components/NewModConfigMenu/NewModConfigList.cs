using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.GameContent.UI.States;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using BetterUI.UI;
using Terraria.Localization;
using ReLogic.Content;
using MonoMod.Cil;

namespace BetterUI.Components.NewModConfigMenu;

internal class NewModConfigList : ModSystem
{
	static readonly Assembly ModLoader_Assembly = typeof(ModLoader).Assembly;
	static readonly Type UIModConfigList_Type = ModLoader_Assembly.GetType("Terraria.ModLoader.Config.UI.UIModConfigList");
	static readonly Type UIModItem_Type = ModLoader_Assembly.GetType("Terraria.ModLoader.UI.UIModItem");
	static readonly Type UIModConfig_Type = ModLoader_Assembly.GetType("Terraria.ModLoader.Config.UI.UIModConfig");
	static readonly Type Interface_Type = ModLoader_Assembly.GetType("Terraria.ModLoader.UI.Interface");

	static readonly MethodInfo UIModConfigList_OnInitialize_Method = UIModConfigList_Type.GetMethod("OnInitialize", BindingFlags.Public | BindingFlags.Instance);
	delegate void UIModConfigList_OnInitialize_Delegate(object self);
	static readonly MethodInfo UIModConfigList_OnActivate_Method = UIModConfigList_Type.GetMethod("OnActivate", BindingFlags.Public | BindingFlags.Instance);
	delegate void UIModConfigList_OnActivate_Delegate(object self);
	static readonly MethodInfo UIModConfigList_Unload_Method = UIModConfigList_Type.GetMethod("Unload", BindingFlags.NonPublic | BindingFlags.Instance);
	delegate void UIModConfigList_Unload_Delegate(object self);

	static readonly MethodInfo UIModItem_OpenConfig_Method = UIModItem_Type.GetMethod("OpenConfig", BindingFlags.NonPublic | BindingFlags.Instance);
	delegate void UIModItem_OpenConfig_Delegate(object self, UIMouseEvent evt, UIElement listeningElement);

	static readonly MethodInfo UIModConfig_OnActivate_Method = UIModConfig_Type.GetMethod("OnActivate", BindingFlags.Public | BindingFlags.Instance);

	public override void Load()
	{
		if (Main.netMode == NetmodeID.Server) return;
		MonoModHooks.Add(UIModConfigList_OnInitialize_Method, Override_UIModConfigList_OnInitialize);
		MonoModHooks.Add(UIModConfigList_OnActivate_Method, Override_UIModConfigList_OnActivate);
		MonoModHooks.Add(UIModConfigList_Unload_Method, Override_UIModConfigList_Unload);

		MonoModHooks.Add(UIModItem_OpenConfig_Method, Override_UIModItem_OpenConfig);

		MonoModHooks.Modify(UIModConfig_OnActivate_Method, IL_UIModConfig_OnActivate);

		Rebuild_UIModConfigList();
	}

	public override void Unload()
	{
		if (Main.netMode == NetmodeID.Server) return;
		// Hooks will be unloaded before this is called
		Rebuild_UIModConfigList();
	}

	// Rebuild the panel UI when the mod unloads or loads later.
	static void Rebuild_UIModConfigList()
	{
		FieldInfo Interface_modConfigList_Field = Interface_Type.GetField("modConfigList", BindingFlags.NonPublic | BindingFlags.Static);
		object Interface_modConfigList_Object = Interface_modConfigList_Field.GetValue(null);
		(Interface_modConfigList_Object as UIState).RemoveAllChildren();
		UIModConfigList_OnInitialize_Method.Invoke(Interface_modConfigList_Object, null);
	}

	internal static bool keepSelectedMod = false;

	// Old panel. Don't want the new one.
	static void Override_UIModConfigList_OnInitialize(UIModConfigList_OnInitialize_Delegate OnInitialize, object self)
	{
		// Reflection References
		FieldInfo uIElement_Field = self.GetType().GetField("uIElement", BindingFlags.NonPublic | BindingFlags.Instance);
		FieldInfo uIPanel_Field = self.GetType().GetField("uIPanel", BindingFlags.NonPublic | BindingFlags.Instance);
		FieldInfo modList_Field = self.GetType().GetField("modList", BindingFlags.NonPublic | BindingFlags.Instance);
		FieldInfo buttonB_Field = self.GetType().GetField("backButton", BindingFlags.NonPublic | BindingFlags.Instance);
		UIElement uIElement = uIElement_Field.GetValue(self) as UIElement;
		UIPanel uIPanel = uIPanel_Field.GetValue(self) as UIPanel;
		UIList modList = modList_Field.GetValue(self) as UIList;
		UITextPanel<LocalizedText> buttonB = buttonB_Field.GetValue(self) as UITextPanel<LocalizedText>;

		uIElement = new UIElement();
		uIElement.Width.Set(0f, 0.8f);
		uIElement.MaxWidth.Set(600f, 0f);
		uIElement.Top.Set(220f, 0f);
		uIElement.Height.Set(-220f, 1f);
		uIElement.HAlign = 0.5f;
		uIPanel = new UIPanel();
		uIPanel.Width.Set(0f, 1f);
		uIPanel.Height.Set(-110f, 1f);
		uIPanel.BackgroundColor = UICommon.MainPanelBackground;
		uIPanel.PaddingTop = 0f;
		uIElement.Append(uIPanel);
		modList = [];
		modList.Width.Set(-25f, 1f);
		modList.Height.Set(-50f, 1f);
		modList.Top.Set(50f, 0f);
		modList.ListPadding = 5f;
		uIPanel.Append(modList);
		UIScrollbar uIScrollbar = new();
		uIScrollbar.SetView(100f, 1000f);
		uIScrollbar.Height.Set(-50f, 1f);
		uIScrollbar.Top.Set(50f, 0f);
		uIScrollbar.HAlign = 1f;
		uIPanel.Append(uIScrollbar);
		modList.SetScrollbar(uIScrollbar);
		UITextPanel<LocalizedText> uIHeaderTexTPanel = new(Language.GetText("tModLoader.ModConfiguration"), 0.8f, large: true);
		uIHeaderTexTPanel.HAlign = 0.5f;
		uIHeaderTexTPanel.Top.Set(-35f, 0f);
		uIHeaderTexTPanel.SetPadding(15f);
		uIHeaderTexTPanel.BackgroundColor = UICommon.DefaultUIBlue;
		uIElement.Append(uIHeaderTexTPanel);
		buttonB = new UITextPanel<LocalizedText>(Language.GetText("UI.Back"), 0.7f, large: true);
		buttonB.Width.Set(-10f, 0.5f);
		buttonB.Height.Set(50f, 0f);
		buttonB.VAlign = 1f;
		buttonB.HAlign = 0.5f;
		buttonB.Top.Set(-45f, 0f);
		buttonB.WithFadedMouseOver();
		buttonB.OnLeftClick += delegate
		{
			SoundEngine.PlaySound(SoundID.MenuClose);
			keepSelectedMod = false;
			if (Main.gameMenu)
			{
				Main.menuMode = 10000;
			}
			else
			{
				IngameFancyUI.Close();
			}
		};
		uIElement.Append(buttonB);
		uIElement.RecalculateChildren();
		uIElement.Recalculate();

		uIElement_Field.SetValue(self, uIElement);
		uIPanel_Field.SetValue(self, uIPanel);
		modList_Field.SetValue(self, modList);
		buttonB_Field.SetValue(self, buttonB);

		(self as UIState).Append(uIElement);
		return;
	}

	// Only want the mod list to update. Not the config list.
	static void Override_UIModConfigList_OnActivate(UIModConfigList_OnActivate_Delegate OnActivate, object self)
	{
		// Reflection References
		FieldInfo modList_Field = self.GetType().GetField("modList", BindingFlags.NonPublic | BindingFlags.Instance);
		UIList modList = modList_Field.GetValue(self) as UIList;
		FieldInfo ModToSelectOnOpen_Field = self.GetType().GetField("ModToSelectOnOpen", BindingFlags.Public | BindingFlags.Instance);
		Mod ModToSelectOnOpen = ModToSelectOnOpen_Field.GetValue(self) as Mod;
		FieldInfo selectedMod_Field = self.GetType().GetField("selectedMod", BindingFlags.NonPublic | BindingFlags.Instance);
		Mod selectedMod = selectedMod_Field.GetValue(self) as Mod;

		modList?.Clear();
		if (!keepSelectedMod) selectedMod = null;
		if (ModToSelectOnOpen != null)
		{
			selectedMod = ModToSelectOnOpen;
			ModToSelectOnOpen = null;
		}

		modList_Field.SetValue(self, modList);
		selectedMod_Field.SetValue(self, selectedMod);
		ModToSelectOnOpen_Field.SetValue(self, ModToSelectOnOpen);

		PopulateMods(self);
		return;
	}

	static void Override_UIModConfigList_Unload(UIModConfigList_Unload_Delegate Unload, object self)
	{
		keepSelectedMod = false;
		Unload(self);
		return;
	}

	// Keep selected mod when opening config from mods list
	static void Override_UIModItem_OpenConfig(UIModItem_OpenConfig_Delegate OpenConfig, object self, UIMouseEvent evt, UIElement listeningElement)
	{
		keepSelectedMod = true;
		OpenConfig(self, evt, listeningElement);
		return;
	}

	// Remove `ModToSelectOnOpen` from UIModConfig.OnActivate
	static void IL_UIModConfig_OnActivate(ILContext il)
	{
		ILCursor c = new(il);
		/*
			// Interface.modConfigList.ModToSelectOnOpen = mod;
			// sequence point: (line 428, col 3) to (line 428, col 51) in D:\a\tModLoader\tModLoader\src\tModLoader\Terraria\ModLoader\Config\UI\UIModConfig.cs
			IL_0000: ldsfld class Terraria.ModLoader.Config.UI.UIModConfigList Terraria.ModLoader.UI.Interface::modConfigList
			IL_0005: ldarg.0
			IL_0006: ldfld class Terraria.ModLoader.Mod Terraria.ModLoader.Config.UI.UIModConfig::mod
			IL_000b: stfld class Terraria.ModLoader.Mod Terraria.ModLoader.Config.UI.UIModConfigList::ModToSelectOnOpen
		*/
		if (!c.TryGotoNext(MoveType.Before,
			i => i.MatchLdsfld(Interface_Type.GetField("modConfigList", BindingFlags.NonPublic | BindingFlags.Static)),
			i => i.MatchLdarg(0),
			i => i.MatchLdfld(UIModConfig_Type.GetField("mod", BindingFlags.NonPublic | BindingFlags.Instance)),
			i => i.MatchStfld(UIModConfigList_Type.GetField("ModToSelectOnOpen", BindingFlags.Public | BindingFlags.Instance))
		))
		{
			throw new Exception("IL Sequence not found");
		}
		c.RemoveRange(4);
	}

	static bool compact => Config.Client.ModConfig_Compact;
	static bool altMode => Config.Client.ModConfig_SmallButtons;
	static bool oldIcon => Config.Client.ModConfig_OldIcon;
	static bool colorButtons => Config.Client.ModConfig_ColourButtons;
	static bool allMods => Config.Client.ModConfig_AllMods;

	// The new mod list.
	static void PopulateMods(object self)
	{
		// Reflection References
		IDictionary<Mod, List<ModConfig>> Configs = (IDictionary<Mod, List<ModConfig>>)typeof(ConfigManager).GetField("Configs", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
		FieldInfo modList_Field = self.GetType().GetField("modList", BindingFlags.NonPublic | BindingFlags.Instance);
		UIList modList = modList_Field.GetValue(self) as UIList;
		FieldInfo selectedMod_Field = self.GetType().GetField("selectedMod", BindingFlags.NonPublic | BindingFlags.Instance);
		Mod selectedMod = selectedMod_Field.GetValue(self) as Mod;

		float baseSize = compact ? 40f : 80f;
		float buttonSize = 40f;
		float padding = compact ? 5f : 10f;
		float halfpadding = padding / 2f;
		float iconDiffOffset = 5f;
		float iconDiffOffset2 = compact ? 2.5f : 5f;

		int i = 0;
		List<Mod> list = ModLoader.Mods.Where(mod => selectedMod != null ? mod == selectedMod : mod.Name != "ModLoader").ToList();
		list.Sort((x, y) => x.GetCleanName().CompareTo(y.GetCleanName()));
		foreach (Mod mod in list)
		{
			Configs.TryGetValue(mod, out List<ModConfig> modConfigs);
			if (allMods || selectedMod != null || !allMods && modConfigs?.Count > 0)
			{
				UISortableElement container = new(i++);
				container.Width.Set(0f, 1f);
				container.Height.Set(baseSize + baseSize, 0f);
				container.SetPadding(0f);

				NewUIPanel panel = new();

				panel.BackgroundColor = new Color(26, 40, 89) * 0.8f;
				panel.BorderColor = new Color(13, 20, 44) * 0.8f;
				panel.MouseOverBackgroundColor = new Color(46, 60, 119);
				panel.MouseOverBorderColor = new Color(20, 30, 56);

				panel.SetPadding(halfpadding);
				panel.Width.Set(0f, 1f);
				panel.Height.Set(baseSize + panel.PaddingTop + panel.PaddingBottom, 0f);
				float bottomOffset = panel.Height.Pixels - panel.PaddingTop - panel.PaddingBottom;
				float leftOffset = baseSize + panel.PaddingLeft;

				Texture2D iconBackgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Slot_Back").Value;
				UIImage iconBackground = new(iconBackgroundTexture);
				iconBackground.Top.Set(iconDiffOffset, 0f);
				iconBackground.Left.Set(iconDiffOffset, 0f);
				if (compact)
				{
					iconBackground.Width.Set(iconBackground.Width.Pixels / 2f, 0f);
					iconBackground.Height.Set(iconBackground.Height.Pixels / 2f, 0f);
					iconBackground.ScaleToFit = true;
				}
				panel.Append(iconBackground);

				// TODO: Cache Icons
				try
				{
					TmodFile modFile = (TmodFile)mod.GetType().GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(mod);
					using (modFile.Open())
					{
						using Stream s = modFile.GetStream("icon.png");
						Texture2D iconTexture = Main.Assets.CreateUntracked<Texture2D>(s, ".png").Value;
						UIImage icon = new(iconTexture);
						icon = new UIImage(iconTexture);
						icon.Top.Set(iconBackground.Top.Pixels - iconDiffOffset2, 0f);
						icon.Left.Set(iconBackground.Left.Pixels - iconDiffOffset2, 0f);
						icon.Width.Set(baseSize, 0f);
						icon.Height.Set(baseSize, 0f);
						icon.ScaleToFit = true;
						panel.Append(icon);
					}
				}
				catch
				{
					Texture2D iconBorderTexture = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Slot_Front").Value;
					UIImage iconBorder = new(iconBorderTexture);
					iconBorder.Top.Set(iconBackground.Top.Pixels, 0f);
					iconBorder.Left.Set(iconBackground.Left.Pixels, 0f);
					if (compact)
					{
						iconBorder.Width.Set(iconBorderTexture.Width / 2f, 0f);
						iconBorder.Height.Set(iconBorderTexture.Height / 2f, 0f);
						iconBorder.ScaleToFit = true;
					}
					panel.Append(iconBorder);

					Texture2D iconTexture = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Locked").Value;
					UIImage icon = new(iconTexture);
					if (compact)
					{
						icon.Width.Set(iconTexture.Width / 2f, 0f);
						icon.Height.Set(iconTexture.Height / 2f, 0f);
						icon.ScaleToFit = true;
					}
					icon.Top.Set(iconBorder.Top.Pixels + (iconBorder.Height.Pixels / 2f) - (icon.Height.Pixels / 2f), 0f);
					icon.Left.Set(iconBorder.Top.Pixels + (iconBorder.Width.Pixels / 2f) - (icon.Width.Pixels / 2f), 0f);
					panel.Append(icon);
				}

				UIElement subContainer = new();
				subContainer.SetPadding(halfpadding);
				subContainer.Width.Set(-leftOffset, 1f);
				subContainer.Height.Set(0f, 1f);
				subContainer.Left.Set(leftOffset, 0f);
				float bottomOffset2 = bottomOffset - subContainer.PaddingTop - subContainer.PaddingBottom;

				UIText text = new(mod.GetName());
				if (compact)
				{
					float textScale = (40f + padding + padding - FontAssets.MouseText.Value.MeasureString(text.Text).Y) / 2f;
					text.Top.Set(textScale, 0f);
				}
				subContainer.Append(text);

				if (!compact)
				{
					UIHorizontalSeparator separator = new();
					separator.Width.Set(0f, 1f);
					separator.Height.Set(5f, 0f);
					separator.Top.Set(20f, 0f);
					separator.Color = UICommon.DefaultUIBlue;
					subContainer.Append(separator);
				}

				if (modConfigs?.Count > 0)
				{
					List<ModConfig> clientConfigs = modConfigs.Where(a => a.Mode == ConfigScope.ClientSide).ToList();
					List<ModConfig> serverConfigs = modConfigs.Where(a => a.Mode == ConfigScope.ServerSide).ToList();

					bool longConfigNames = modConfigs.Any(a => a.GetName().Length > 25f);

					int j = 0;
					int k = 0;
					for (int x = 0; x < 2; x++)
					{
						List<ModConfig> configs;
						if (compact)
						{
							configs = x == 1 ? clientConfigs : x == 0 ? serverConfigs : [];
						}
						else
						{
							configs = x == 0 ? clientConfigs : x == 1 ? serverConfigs : [];
						}
						if (altMode && !compact)
						{
							configs.Reverse();
						}
						int l = 0;
						foreach (ModConfig config in configs)
						{
							l++;
							string configDisplayName = config.GetName();
							UIButton<string> button = new("");

							if (colorButtons && config.Mode == ConfigScope.ServerSide)
							{
								button.BorderColor = new Color(button.BorderColor.B, button.BorderColor.G, button.BorderColor.R, button.BorderColor.A);
								button.BackgroundColor = new Color(button.BackgroundColor.B, button.BackgroundColor.G, button.BackgroundColor.R, button.BackgroundColor.A);
								button.HoverPanelColor = new Color(button.HoverPanelColor.B, button.HoverPanelColor.G, button.HoverPanelColor.R, button.HoverPanelColor.A);
							}

							if (altMode || compact)
							{
								button.Width.Set(buttonSize, 0f);
								button.Height.Set(buttonSize, 0f);
								if (config.Mode == ConfigScope.ClientSide)
								{
									button.Top.Set(bottomOffset2 - button.Height.Pixels + ((button.Height.Pixels + subContainer.PaddingBottom) * (j / 5)), 0f);
									button.Left.Set((button.Width.Pixels + subContainer.PaddingLeft) * (j++ % 5), 0f);
									if (compact)
									{
										button.Top.Set(0f, 0f);
										button.Left.Set(halfpadding - ((button.Width.Pixels + subContainer.PaddingRight) * ((k++ % 5) + 1)), 1f);
									}
								}
								else
								{
									button.Top.Set(bottomOffset2 - button.Height.Pixels + ((button.Height.Pixels + subContainer.PaddingBottom) * (k / 5)), 0f);
									if (compact)
									{
										button.Top.Set(0f, 0f);
									}
									button.Left.Set(halfpadding - ((button.Width.Pixels + subContainer.PaddingRight) * ((k++ % 5) + 1)), 1f);
								}
							}
							else
							{
								if ((config.Mode == ConfigScope.ClientSide && l > serverConfigs.Count) || (config.Mode == ConfigScope.ServerSide && l > clientConfigs.Count) || longConfigNames)
								{
									button.Width.Set(0f, 1f);
									button.Height.Set(buttonSize, 0f);
									button.Top.Set(bottomOffset2 - button.Height.Pixels + ((button.Height.Pixels + subContainer.PaddingBottom) * ((config.Mode == ConfigScope.ClientSide || longConfigNames) ? j++ : k++)), 0f);
								}
								else
								{
									button.Width.Set(-subContainer.PaddingLeft / 2f, 0.5f);
									button.Height.Set(buttonSize, 0f);
									if (config.Mode == ConfigScope.ClientSide)
									{
										button.Left.Set(0f, 0f);
										button.Top.Set(bottomOffset2 - button.Height.Pixels + ((button.Height.Pixels + subContainer.PaddingBottom) * j++), 0f);
									}
									else
									{
										button.Left.Set(subContainer.PaddingLeft / 2f, 0.5f);
										button.Top.Set(bottomOffset2 - button.Height.Pixels + ((button.Height.Pixels + subContainer.PaddingBottom) * k++), 0f);
									}
								}
								button.PaddingLeft = buttonSize;
								button.SetText(configDisplayName);
								button.TextOriginX = 0f;
								button.UseInnerDimensions = true;
							}

							if (altMode || compact)
							{
								button.OnUpdate += delegate
								{
									if (button.IsMouseHovering)
									{
										Main.instance.MouseText(configDisplayName);
									}
								};
							}

							button.OnLeftClick += delegate
							{
								SoundEngine.PlaySound(SoundID.MenuOpen);
								FieldInfo modConfigField = Interface_Type.GetField("modConfig", BindingFlags.NonPublic | BindingFlags.Static);
								object modConfig = modConfigField.GetValue(null);
								MethodInfo SetModMethod = modConfig.GetType().GetMethod("SetMod", BindingFlags.NonPublic | BindingFlags.Instance);
								SetModMethod.Invoke(modConfig, [mod, config, false, null, null, true]);
								modConfigField.SetValue(null, modConfig);
								if (Main.gameMenu)
								{
									Main.menuMode = 10024;
								}
								else
								{
									Main.InGameUI.SetState((UIState)modConfigField.GetValue(null));
								}
							};

							Asset<Texture2D> buttonTexture;
							UIElement buttonIcon;
							if (oldIcon)
							{
								Main.instance.LoadItem(ItemID.Cog);
								buttonTexture = TextureAssets.Item[ItemID.Cog];
								buttonIcon = new UIImage(buttonTexture)
								{
									MarginLeft = -buttonTexture.Value.Width / 2f
								};
							}
							else
							{
								buttonTexture = UICommon.ConfigSideIndicatorTexture;
								Rectangle textureFrame = buttonTexture.Frame(2, 1, (config.Mode == ConfigScope.ServerSide) ? 1 : 0);
								buttonIcon = new UIImageFramed(buttonTexture, textureFrame)
								{
									MarginLeft = -buttonTexture.Value.Width / 4f
								};
							}
							if (altMode || compact)
								buttonIcon.Left.Set(0f, 0.5f);
							else
								buttonIcon.Left.Set(-buttonSize / 2f, 0);
							buttonIcon.Top.Set(-buttonTexture.Value.Height / 2f, 0.5f);
							button.Append(buttonIcon);

							subContainer.Append(button);
						}

					}
				}
				panel.Append(subContainer);

				float totalHeight = subContainer.Height.Pixels - subContainer.PaddingTop - subContainer.PaddingBottom;
				foreach (UIElement child in subContainer.Children)
				{
					totalHeight = Math.Max(totalHeight, child.Top.Pixels + child.Height.Pixels);
				}
				subContainer.Height.Set(totalHeight + subContainer.PaddingTop + subContainer.PaddingBottom, 0f);

				totalHeight = panel.Height.Pixels - panel.PaddingTop - panel.PaddingBottom;
				foreach (UIElement child in panel.Children)
				{
					totalHeight = Math.Max(totalHeight, child.Top.Pixels + child.Height.Pixels);
				}

				panel.Height.Set(totalHeight + panel.PaddingTop + panel.PaddingBottom, 0f);
				container.Height.Set(panel.Height.Pixels, 0f);

				container.Append(panel);

				modList.Add(container);
			}
		}
		modList_Field.SetValue(self, modList);
		return;
	}
}