using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterUI.Components.BetterResourcePackMenu;

class DraggableResourcePackItem : ModSystem
{
	static readonly MethodInfo UIResourcePackSelectionMenu_PopulatePackList_Method = typeof(UIResourcePackSelectionMenu).GetMethod("PopulatePackList", BindingFlags.NonPublic | BindingFlags.Instance);
	delegate void UIResourcePackSelectionMenu_PopulatePackList_Delegate(UIResourcePackSelectionMenu self);

	static ResourcePack currentItem = null;
	static Vector2 currentItemPos = Vector2.Zero;
	static bool updateList = false;
	static bool updateEnabled = false;
	static float currentScroll = 0f;

	public override void Load()
	{
		if (Main.netMode == NetmodeID.Server) return;
		MonoModHooks.Add(UIResourcePackSelectionMenu_PopulatePackList_Method, Override_UIResourcePackSelectionMenu_PopulatePackList);
	}

	// Maybe I should IL this...
	static void Override_UIResourcePackSelectionMenu_PopulatePackList(UIResourcePackSelectionMenu_PopulatePackList_Delegate PopulatePackList, UIResourcePackSelectionMenu self)
	{
		updateList = false;
		UIList _availablePacksList = self.GetType().GetField("_availablePacksList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self) as UIList;
		_availablePacksList.Clear();
		UIList _enabledPacksList = self.GetType().GetField("_enabledPacksList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self) as UIList;
		_enabledPacksList.Clear();
		self.GetType().GetMethod("CleanUpResourcePackPriority", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, null);
		ResourcePackList _packsList = self.GetType().GetField("_packsList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self) as ResourcePackList;
		IEnumerable<ResourcePack> enabledPacks = _packsList.EnabledPacks;
		IEnumerable<ResourcePack> disabledPacks = _packsList.DisabledPacks;
		int num = 0;
		foreach (ResourcePack item in disabledPacks)
		{
			UIResourcePack uiResourcePack = new(item, num)
			{
				Width = StyleDimension.FromPixelsAndPercent(0f, 1f)
			};
			uiResourcePack.Recalculate();

			uiResourcePack.OnLeftMouseDown += (evt, affectedElement) =>
			{
				currentItem = item;
				currentItemPos.X = affectedElement.GetDimensions().X * Main.UIScale;
				currentItemPos.Y = 0f;
				currentScroll = 0f;
			};

			uiResourcePack.OnUpdate += (affectedElement) =>
			{
				Vector2 offsetMousePos = (Main.MouseScreen * Main.UIScale) - currentItemPos;

				if (currentItem == item)
				{
					affectedElement.Top.Set(offsetMousePos.Y, 0f);
					float x = affectedElement.GetDimensions().Width * Main.UIScale;

					if (updateEnabled)
					{
						currentItemPos.X = affectedElement.GetDimensions().X * Main.UIScale;
						currentItemPos.Y = 0f;
						updateEnabled = false;
					}

					if (offsetMousePos.X > x + 20) // Right
					{
						currentItemPos.X += x;
						SoundEngine.PlaySound(SoundID.MenuTick);
						item.IsEnabled = true;
						item.SortingOrder = -1;
						updateList = true;
						updateEnabled = true;
					}
				}
			};

			uiResourcePack.OnLeftMouseUp += (evt, affectedElement) =>
			{
				currentItem = null;
				currentItemPos = Vector2.Zero;
				currentScroll = 0f;
			};

			UIElement uiElement = self.GetType().GetMethod("CreatePackToggleButton", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [item]) as UIElement;
			uiElement.OnUpdate += (affectedElement) => self.GetType().GetMethod("EnablePackUpdate", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [affectedElement]);
			uiElement.SetSnapPoint("ToggleToOn", num);
			uiResourcePack.ContentPanel.Append(uiElement);
			uiElement = self.GetType().GetMethod("CreatePackInfoButton", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [item]) as UIElement;
			uiElement.OnUpdate += (affectedElement) => self.GetType().GetMethod("SeeInfoUpdate", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [affectedElement]);
			uiElement.SetSnapPoint("InfoOff", num);
			uiResourcePack.ContentPanel.Append(uiElement);
			_availablePacksList.Add(uiResourcePack);
			num++;
		}
		num = 0;
		foreach (ResourcePack item in enabledPacks)
		{
			UIResourcePack uiResourcePack = new(item, num)
			{
				Width = StyleDimension.FromPixelsAndPercent(0f, 1f)
			};
			uiResourcePack.Recalculate();

			uiResourcePack.OnLeftMouseDown += (evt, affectedElement) =>
			{
				currentItem = item;
				float scrollPosition = (affectedElement.Parent.Parent.Parent.Children.Where(x => x.GetType() == typeof(UIScrollbar)).ToArray()[0] as UIScrollbar).ViewPosition;
				currentScroll = scrollPosition * Main.UIScale;
				currentItemPos.X = affectedElement.GetDimensions().X * Main.UIScale;
				currentItemPos.Y = affectedElement.GetDimensions().Y * Main.UIScale;
			};

			uiResourcePack.OnUpdate += (affectedElement) =>
			{
				if (currentItem == item)
				{
					float x = (affectedElement.GetDimensions().Width + 5) * Main.UIScale;
					float y = (affectedElement.GetDimensions().Height + 5) * Main.UIScale;

					float scrollPosition = (affectedElement.Parent.Parent.Parent.Children.Where(x => x.GetType() == typeof(UIScrollbar)).ToArray()[0] as UIScrollbar).ViewPosition;
					if (updateEnabled)
					{
						currentItemPos.X = affectedElement.GetDimensions().X * Main.UIScale;
						currentItemPos.Y = (affectedElement.GetDimensions().Y + scrollPosition) * Main.UIScale;
						updateEnabled = false;
					}
					float offsetScroll = (scrollPosition * Main.UIScale) - currentScroll;
					Vector2 offsetMousePos = (Main.MouseScreen * Main.UIScale) - currentItemPos;
					//Main.instance.MouseTextHackZoom(((int)offsetScroll).ToString() + "\n" + ((int)offsetMousePos.Y).ToString() + "\n" + (int)((offsetMousePos.Y + offsetScroll) / y));
					if (offsetMousePos.Y + offsetScroll > y) // Down
					{
						SoundEngine.PlaySound(SoundID.MenuTick);
						for (int i = 0; i < (int)((offsetMousePos.Y + offsetScroll) / y); i++)
						{
							self.GetType().GetMethod("OffsetResourcePackPriority", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [item, 1]);
							currentItemPos.Y += y;
						}
						updateList = true;
					}
					else if (offsetMousePos.Y + offsetScroll < -5) // Up
					{
						SoundEngine.PlaySound(SoundID.MenuTick);
						self.GetType().GetMethod("OffsetResourcePackPriority", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [item, -1]);
						currentItemPos.Y -= y;
						updateList = true;
					}
					if (offsetMousePos.X < -20) // Left
					{
						SoundEngine.PlaySound(SoundID.MenuTick);
						currentItemPos.X -= x;
						currentItemPos.Y = 0;
						item.IsEnabled = false;
						updateList = true;
						updateEnabled = true;
					}
				}
			};

			uiResourcePack.OnLeftMouseUp += (evt, affectedElement) =>
			{
				currentItem = null;
				currentItemPos = Vector2.Zero;
				currentScroll = 0f;
			};

			if (item.IsEnabled)
			{
				UIElement uiElement = self.GetType().GetMethod("CreatePackToggleButton", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [item]) as UIElement;
				uiElement.Left = new StyleDimension(0f, 0f);
				uiElement.Width = new StyleDimension(0f, 0.5f);
				uiElement.OnUpdate += (affectedElement) => self.GetType().GetMethod("DisablePackUpdate", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [affectedElement]);
				uiElement.SetSnapPoint("ToggleToOff", num);
				uiResourcePack.ContentPanel.Append(uiElement);
				uiElement = self.GetType().GetMethod("CreatePackInfoButton", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [item]) as UIElement;
				uiElement.OnUpdate += (affectedElement) => self.GetType().GetMethod("SeeInfoUpdate", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [affectedElement]);
				uiElement.Left = new StyleDimension(0f, 0.5f);
				uiElement.Width = new StyleDimension(0f, 1f / 6f);
				uiElement.SetSnapPoint("InfoOff", num);
				uiResourcePack.ContentPanel.Append(uiElement);
				uiElement = self.GetType().GetMethod("CreateOffsetButton", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [item, -1]) as UIElement;
				uiElement.Left = new StyleDimension(0f, 2f / 3f);
				uiElement.Width = new StyleDimension(0f, 1f / 6f);
				uiElement.SetSnapPoint("OrderUp", num);
				uiResourcePack.ContentPanel.Append(uiElement);
				uiElement = self.GetType().GetMethod("CreateOffsetButton", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [item, 1]) as UIElement;
				uiElement.Left = new StyleDimension(0f, 0.8333334f);
				uiElement.Width = new StyleDimension(0f, 1f / 6f);
				uiElement.SetSnapPoint("OrderDown", num);
				uiResourcePack.ContentPanel.Append(uiElement);
			}
			_enabledPacksList.Add(uiResourcePack);
			num++;
		}
		self.GetType().GetField("_availablePacksList", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(self, _availablePacksList);
		self.GetType().GetField("_enabledPacksList", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(self, _enabledPacksList);
		self.GetType().GetMethod("UpdateTitles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, null);

		_enabledPacksList.Parent.OnUpdate += (affectedElement) =>
		{
			if (updateList)
			{
				self.GetType().GetMethod("PopulatePackList", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, null);
			}
		};
	}
}
