using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterUI.Components.BetterResourcePackMenu;

class ResourcePackApplyButton : ModSystem
{
	static readonly MethodInfo UIResourcePackSelectionMenu_AddBackAndFolderButtons_Method = typeof(UIResourcePackSelectionMenu).GetMethod("AddBackAndFolderButtons", BindingFlags.NonPublic | BindingFlags.Instance);
	delegate void UIResourcePackSelectionMenu_AddBackAndFolderButtons_Delegate(UIResourcePackSelectionMenu self, UIElement outerContainer);

	public override void Load()
	{
		if (Main.netMode == NetmodeID.Server) return;
		MonoModHooks.Add(UIResourcePackSelectionMenu_AddBackAndFolderButtons_Method, Override_UIResourcePackSelectionMenu_AddBackAndFolderButtons);
	}

	// Maybe I should IL this...
	static void Override_UIResourcePackSelectionMenu_AddBackAndFolderButtons(UIResourcePackSelectionMenu_AddBackAndFolderButtons_Delegate AddBackAndFolderButtons, UIResourcePackSelectionMenu self, UIElement outerContainer)
	{
		UITextPanel<LocalizedText> uiTextPanel = new(Language.GetText("UI.Back"), 0.7f, large: true)
		{
			Width = new StyleDimension(-8f, 0.333f),
			Height = new StyleDimension(50f, 0f),
			VAlign = 1f,
			HAlign = 0f,
			Top = new StyleDimension(-45f, 0f)
		};
		uiTextPanel.OnMouseOver += (evt, listeningElement) => typeof(UIResourcePackSelectionMenu).GetMethod("FadedMouseOver", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, [evt, listeningElement]);
		uiTextPanel.OnMouseOut += (evt, listeningElement) => typeof(UIResourcePackSelectionMenu).GetMethod("FadedMouseOut", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, [evt, listeningElement]);
		uiTextPanel.OnLeftClick += (evt, listeningElement) => self.GetType().GetMethod("GoBackClick", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [evt, listeningElement]);
		uiTextPanel.SetSnapPoint("GoBack", 0);

		outerContainer.Append(uiTextPanel);
		UITextPanel<LocalizedText> uiTextPanel2 = new(Language.GetText("Apply Changes"), 0.7f, large: true)
		{
			Width = new StyleDimension(-8f, 0.333f),
			Height = new StyleDimension(50f, 0f),
			VAlign = 1f,
			HAlign = 0.5f,
			Top = new StyleDimension(-45f, 0f)
		};
		uiTextPanel2.OnMouseOver += (evt, listeningElement) => typeof(UIResourcePackSelectionMenu).GetMethod("FadedMouseOver", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, [evt, listeningElement]);
		uiTextPanel2.OnMouseOut += (evt, listeningElement) => typeof(UIResourcePackSelectionMenu).GetMethod("FadedMouseOut", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, [evt, listeningElement]);
		uiTextPanel2.OnLeftClick += (evt, listeningElement) =>
		{
			SoundEngine.PlaySound(SoundID.MenuOpen);
			self.GetType().GetMethod("ApplyListChanges", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, []);
			Main.SaveSettings();
			Main.instance.TilePaintSystem.Reset();
		};
		uiTextPanel2.SetSnapPoint("ApplyChances", 0);
		outerContainer.Append(uiTextPanel2);

		UITextPanel<LocalizedText> uiTextPanel3 = new(Language.GetText("GameUI.OpenFileFolder"), 0.7f, large: true)
		{
			Width = new StyleDimension(-8f, 0.333f),
			Height = new StyleDimension(50f, 0f),
			VAlign = 1f,
			HAlign = 1f,
			Top = new StyleDimension(-45f, 0f)
		};
		uiTextPanel3.OnMouseOver += (evt, listeningElement) => typeof(UIResourcePackSelectionMenu).GetMethod("FadedMouseOver", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, [evt, listeningElement]);
		uiTextPanel3.OnMouseOut += (evt, listeningElement) => typeof(UIResourcePackSelectionMenu).GetMethod("FadedMouseOut", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, [evt, listeningElement]);
		uiTextPanel3.OnLeftClick += (evt, listeningElement) => self.GetType().GetMethod("OpenFoldersClick", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, [evt, listeningElement]);
		uiTextPanel3.SetSnapPoint("OpenFolder", 0);
		outerContainer.Append(uiTextPanel3);
	}
}
