namespace Wotc.Mtga.DuelScene.Interactions;

public class NullButtonDataProvider : IButtonDataProvider
{
	public static readonly IButtonDataProvider Default = new NullButtonDataProvider();

	public string GetLocKey()
	{
		return string.Empty;
	}

	public string GetSfx()
	{
		return string.Empty;
	}

	public ButtonStyle.StyleType GetStyle()
	{
		return ButtonStyle.StyleType.None;
	}
}
