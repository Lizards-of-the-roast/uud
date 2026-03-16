namespace Wotc.Mtga.DuelScene.Interactions.CastingTimeOption;

public interface ICastTimeOptionHeaderProvider
{
	BrowserCardHeader.BrowserCardHeaderData GetCastTimeOptionHeader(MtgCastTimeOption castTimeOption);

	bool TryGetCastTimeOptionHeader(MtgCastTimeOption castTimeOption, out BrowserCardHeader.BrowserCardHeaderData cardHeaderData)
	{
		cardHeaderData = GetCastTimeOptionHeader(castTimeOption);
		return cardHeaderData != null;
	}
}
