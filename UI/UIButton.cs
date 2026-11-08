using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace BetterUI.UI;

public class UIButton : UIPanel
{
	private Color _borderColor = new Color(13, 20, 44) * 0.8f;
	private Color _backgroundColor = UICommon.DefaultUIBlueMouseOver;
	new public Color BorderColor { get { return _borderColor; } set { _borderColor = value; base.BorderColor = value; } }
	new public Color BackgroundColor { get { return _backgroundColor; } set { _backgroundColor = value; base.BackgroundColor = value; } }
	public Color MouseOverBorderColor = Colors.FancyUIFatButtonMouseOver;
	public Color MouseOverBackgroundColor = UICommon.DefaultUIBlue;

	public UIButton()
	{
		base.BorderColor = this.BorderColor;
		base.BackgroundColor = this.BackgroundColor;
		base.OnMouseOver += MouseOver;
		base.OnMouseOut += MouseOut;
	}

	private void MouseOver(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(in SoundID.MenuTick);
		base.BorderColor = this.MouseOverBorderColor;
		base.BackgroundColor = this.MouseOverBackgroundColor;
	}

	private void MouseOut(UIMouseEvent evt, UIElement listeningElement)
	{
		base.BorderColor = this.BorderColor;
		base.BackgroundColor = this.BackgroundColor;
	}
}