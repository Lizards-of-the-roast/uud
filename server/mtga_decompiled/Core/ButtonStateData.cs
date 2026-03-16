using UnityEngine;

public class ButtonStateData
{
	public MTGALocalizedString LocalizedString;

	public string BrowserElementKey;

	public ButtonStyle.StyleType StyleType;

	public bool Enabled = true;

	public bool IsActive = true;

	public Sprite Sprite;

	public RectTransform ChildView;
}
