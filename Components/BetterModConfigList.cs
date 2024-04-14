using MonoMod.RuntimeDetour.HookGen;
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
using DALib.Functions;
using Terraria.GameContent.UI.States;
using Terraria.Audio;
using ReLogic.Content;
using Microsoft.Xna.Framework;
using BetterUI.UI;

namespace BetterUI.Components;

class BetterModConfigList : ModSystem
{
	static readonly MethodInfo UIModConfigList_Populate_Method = typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.Config.UI.UIModConfigList").GetMethod("Populate", BindingFlags.NonPublic | BindingFlags.Instance);
	delegate bool UIModConfigList_Populate_Delegate(object self);

	public override void Load()
	{
		try { HookEndpointManager.Add(UIModConfigList_Populate_Method, Override_UIModConfigList_Populate); } catch { throw new Exception("Hook Failed"); }
	}

	public override void Unload()
	{
		try { HookEndpointManager.Remove(UIModConfigList_Populate_Method, Override_UIModConfigList_Populate); } catch { }
	}

	static void Override_UIModConfigList_Populate(UIModConfigList_Populate_Delegate Populate, object self)
	{
		int i = 0;
		IDictionary<Mod, List<ModConfig>> Configs = (IDictionary<Mod, List<ModConfig>>)(typeof(ConfigManager).GetField("Configs", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

		bool compact = true;
		bool altMode = false;
		float baseSize = compact ? 40f : 80f;
		float padding = compact ? 5f : 10f;
		float halfpadding = padding / 2f;
		float iconDiffOffset = 5f;
		float iconDiffOffset2 = compact ? 2.5f : 5f;

		foreach (KeyValuePair<Mod, List<ModConfig>> item in Configs.OrderBy(a => a.Key.DisplayName))
		{
			UISortableElement container = new UISortableElement(i++);
			container.Width.Set(0f, 1f);
			container.Height.Set(baseSize + baseSize, 0f);
			container.SetPadding(0f);

			NewUIPanel panel = new NewUIPanel();

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
			UIImage iconBackground = new UIImage(iconBackgroundTexture);
			iconBackground.Top.Set(iconDiffOffset, 0f);
			iconBackground.Left.Set(iconDiffOffset, 0f);
			if (compact)
			{
				iconBackground.Width.Set(iconBackground.Width.Pixels / 2f, 0f);
				iconBackground.Height.Set(iconBackground.Height.Pixels / 2f, 0f);
				iconBackground.ScaleToFit = true;
			}
			panel.Append(iconBackground);

			try
			{
				TmodFile modFile = (TmodFile)(item.Key.GetType().GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(item.Key));
				using (modFile.Open())
				{
					using Stream s = modFile.GetStream("icon.png");
					Texture2D iconTexture = Main.Assets.CreateUntracked<Texture2D>(s, ".png").Value;
					UIImage icon = new UIImage(iconTexture);
					icon = new UIImage(iconTexture);
					icon.Top.Set(iconBackground.Top.Pixels - iconDiffOffset2, 0f);
					icon.Left.Set(iconBackground.Left.Pixels - iconDiffOffset2, 0f);
					icon.Width.Set(baseSize, 0f);
					icon.Height.Set(baseSize, 0f);
					icon.ScaleToFit = true;
					panel.Append(icon);
				}
			}
			catch {
				Texture2D iconBorderTexture = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Slot_Front").Value;
				UIImage iconBorder = new UIImage(iconBorderTexture);
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
				UIImage icon = new UIImage(iconTexture);
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

			UIElement subContainer = new UIElement();
			subContainer.SetPadding(halfpadding);
			subContainer.Width.Set(-leftOffset, 1f);
			subContainer.Height.Set(0f, 1f);
			subContainer.Left.Set(leftOffset, 0f);
			float bottomOffset2 = bottomOffset - subContainer.PaddingTop - subContainer.PaddingBottom;

			UIText text = new UIText(item.Key.DisplayName);
			if (compact)
			{
				float textScale = ((40f + padding + padding) - FontAssets.MouseText.Value.MeasureString(text.Text).Y) / 2f;
				text.Top.Set(textScale, 0f);
			}
			subContainer.Append(text);

			if (!compact)
			{
				UIHorizontalSeparator separator = new UIHorizontalSeparator();
				separator.Width.Set(0f, 1f);
				separator.Height.Set(5f, 0f);
				separator.Top.Set(20f, 0f);
				separator.Color = UICommon.DefaultUIBlue;
				subContainer.Append(separator);
			}

            List<ModConfig> clientConfigs = item.Value.Where(a => a.Mode == ConfigScope.ClientSide).ToList();
            List<ModConfig> serverConfigs = item.Value.Where(a => a.Mode == ConfigScope.ServerSide).ToList();

            int j = 0;
			int k = 0;

			for (int x = 0; x < 2; x++)
			{
				List<ModConfig> configs = x == 0 ? clientConfigs : x == 1 ? serverConfigs : new List<ModConfig>();
				if (compact)
				{
					configs = x == 1 ? clientConfigs : x == 0 ? serverConfigs : new List<ModConfig>();
				}

                if (altMode && !compact)
                {
                    configs.Reverse();
                }

                int l = 0;
                foreach (ModConfig config in configs)
				{
					l++;
					string configDisplayName = ((LabelAttribute)Attribute.GetCustomAttribute(config.GetType(), typeof(LabelAttribute)))?.Label ?? config.Name;

					UIButton button = new UIButton();
					if (config.Mode == ConfigScope.ServerSide)
					{
						button.BorderColor = button.BorderColor.ShiftHue(120);
						button.BackgroundColor = button.BackgroundColor.ShiftHue(120);
						button.MouseOverBackgroundColor = button.MouseOverBackgroundColor.ShiftHue(120);
					}

					if (altMode || compact)
					{
						button.Width.Set(40f, 0f);
						button.Height.Set(40f, 0f);
						if (config.Mode == ConfigScope.ClientSide)
						{
                            button.Top.Set(bottomOffset2 - button.Height.Pixels + (button.Height.Pixels + subContainer.PaddingBottom) * (j / 5), 0f);
							button.Left.Set((button.Width.Pixels + subContainer.PaddingLeft) * (j++ % 5), 0f);
							if (compact)
							{
								button.Top.Set(0f, 0f);
								button.Left.Set(halfpadding - (button.Width.Pixels + subContainer.PaddingRight) * ((k++ % 5) + 1), 1f);
							}
						}
						else
                        {
                            button.Top.Set(bottomOffset2 - button.Height.Pixels + (button.Height.Pixels + subContainer.PaddingBottom) * (k / 5), 0f);
							if (compact)
							{
								button.Top.Set(0f, 0f);
							}
							button.Left.Set(halfpadding - (button.Width.Pixels + subContainer.PaddingRight) * ((k++ % 5) + 1), 1f);
                        }
					}
					else
					{
						if ((config.Mode == ConfigScope.ClientSide && l > serverConfigs.Count) || (config.Mode == ConfigScope.ServerSide && l > clientConfigs.Count))
						{
                            button.Width.Set(0f, 1f);
                            button.Height.Set(40f, 0f);
                            button.Top.Set(bottomOffset2 - button.Height.Pixels + (button.Height.Pixels + subContainer.PaddingBottom) * (config.Mode == ConfigScope.ClientSide ? j++ : k++), 0f);
                        }
                        else {
							button.Width.Set(-subContainer.PaddingLeft / 2f, 0.5f);
							button.Height.Set(40f, 0f);
							if (config.Mode == ConfigScope.ClientSide)
                            {
                                button.Left.Set(0f, 0f);
                                button.Top.Set(bottomOffset2 - button.Height.Pixels + (button.Height.Pixels + subContainer.PaddingBottom) * j++, 0f);
                            }
							else
							{
                                button.Left.Set(subContainer.PaddingLeft / 2f, 0.5f);
                                button.Top.Set(bottomOffset2 - button.Height.Pixels + (button.Height.Pixels + subContainer.PaddingBottom) * k++, 0f);
                            }
						}
						button.Recalculate();
						CalculatedStyle dimensions = button.GetDimensions();

						UIText text2 = new UIText(configDisplayName);
						text2.Left.Set(40f - padding, 0f);
						float text2Scale = Math.Min(((float)(dimensions.Width / 4f) - 40f - padding - button.PaddingLeft - button.PaddingRight) / FontAssets.MouseText.Value.MeasureString(text2.Text).X, 1f);
						text2.SetText(text2.Text, text2Scale, false);
						text2.Top.Set((4f / text2Scale) - 4f, 0f);
						button.Append(text2);
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

					button.OnClick += delegate
					{
						SoundEngine.PlaySound(SoundID.MenuOpen);
						FieldInfo modConfigField = typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.UI.Interface").GetField("modConfig", BindingFlags.NonPublic | BindingFlags.Static);
						object modConfig = modConfigField.GetValue(null);
						MethodInfo SetModMethod = modConfig.GetType().GetMethod("SetMod", BindingFlags.NonPublic | BindingFlags.Instance);
						SetModMethod.Invoke(modConfig, new object[] { item.Key, config });
						modConfigField.SetValue(null, modConfig);
						Main.InGameUI.SetState((UIState)(modConfigField.GetValue(null)));
					};

					Main.instance.LoadItem(ItemID.Cog);
					Texture2D buttonTexture = TextureAssets.Item[ItemID.Cog].Value;
					UIImage buttonIcon = new UIImage(buttonTexture);
					if (altMode || compact)
						buttonIcon.Left.Set(-buttonTexture.Width / 2f, 0.5f);
					buttonIcon.Top.Set(-buttonTexture.Height / 2f, 0.5f);
					button.Append(buttonIcon);
					subContainer.Append(button);
                }

				panel.Append(subContainer);
			}

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

			FieldInfo modListField = typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.Config.UI.UIModConfigList").GetField("modList", BindingFlags.NonPublic | BindingFlags.Instance);
			UIList modList = (UIList)modListField.GetValue(self);
			modList.Add(container);
			modListField.SetValue(self, modList);
		}
		return;
	}
}



