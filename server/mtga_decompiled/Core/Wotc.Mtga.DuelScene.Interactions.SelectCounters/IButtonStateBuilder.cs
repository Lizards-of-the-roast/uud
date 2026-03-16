namespace Wotc.Mtga.DuelScene.Interactions.SelectCounters;

public interface IButtonStateBuilder
{
	ButtonStateData CreateButtonStateData(uint counterType, uint count, ButtonStyle.StyleType style, bool enabled, DuelSceneBrowserType browserType = DuelSceneBrowserType.Invalid);
}
