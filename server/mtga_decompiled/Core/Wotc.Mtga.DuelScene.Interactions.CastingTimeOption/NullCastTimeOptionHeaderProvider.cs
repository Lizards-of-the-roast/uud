namespace Wotc.Mtga.DuelScene.Interactions.CastingTimeOption;

public class NullCastTimeOptionHeaderProvider : ICastTimeOptionHeaderProvider
{
	public static ICastTimeOptionHeaderProvider Default = new NullCastTimeOptionHeaderProvider();

	public BrowserCardHeader.BrowserCardHeaderData GetCastTimeOptionHeader(MtgCastTimeOption castTimeOption)
	{
		return null;
	}
}
