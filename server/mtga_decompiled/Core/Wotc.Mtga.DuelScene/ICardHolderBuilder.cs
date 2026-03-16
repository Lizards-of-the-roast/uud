namespace Wotc.Mtga.DuelScene;

public interface ICardHolderBuilder
{
	ICardHolder CreateCardHolder(CardHolderType cardHolderType, GREPlayerNum owner);

	bool DestroyCardHolder(ICardHolder cardHolder);
}
