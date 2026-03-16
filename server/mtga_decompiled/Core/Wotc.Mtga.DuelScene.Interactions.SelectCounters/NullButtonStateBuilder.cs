namespace Wotc.Mtga.DuelScene.Interactions.SelectCounters;

public class NullButtonStateBuilder : IButtonStateBuilder
{
	public static readonly IButtonStateBuilder Default = new NullButtonStateBuilder();

	public ButtonStateData CreateButtonStateData(uint counterType, uint count, ButtonStyle.StyleType style, bool enabled, DuelSceneBrowserType browserType = DuelSceneBrowserType.Invalid)
	{
		return new ButtonStateData();
	}
}
