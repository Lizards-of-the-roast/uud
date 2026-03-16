namespace Wotc.Mtga.DuelScene;

public interface ICardViewManager : ICardViewProvider, ICardViewController
{
	uint GetCardUpdatedId(uint id);

	uint GetCardPreviousId(uint id);
}
