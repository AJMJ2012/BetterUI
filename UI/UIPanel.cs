using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace BetterUI.UI;

public class NewUIPanel : UIPanel
{
	private Color _backgroundColor = new Color(26, 40, 89) * 0.8f;
	new public Color BackgroundColor { get { return _backgroundColor; } set { _backgroundColor = value; base.BackgroundColor = value; } }
	public Color MouseOverBorderColor = new(20, 30, 56);

	private Color _borderColor = new Color(13, 20, 44) * 0.8f;
	new public Color BorderColor { get { return _borderColor; } set { _borderColor = value; base.BorderColor = value; } }
	public Color MouseOverBackgroundColor = new(46, 60, 119);

	public NewUIPanel()
	{
		base.BorderColor = this.BorderColor;
		base.BackgroundColor = this.BackgroundColor;
		base.OnMouseOver += MouseOver;
		base.OnMouseOut += MouseOut;
	}

	private void MouseOver(UIMouseEvent evt, UIElement listeningElement)
	{
		base.BorderColor = this.MouseOverBorderColor;
		base.BackgroundColor = this.MouseOverBackgroundColor;
	}

	private void MouseOut(UIMouseEvent evt, UIElement listeningElement)
	{
		base.BorderColor = this.BorderColor;
		base.BackgroundColor = this.BackgroundColor;
	}
}