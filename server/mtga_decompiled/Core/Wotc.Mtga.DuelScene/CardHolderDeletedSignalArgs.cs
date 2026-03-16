namespace Wotc.Mtga.DuelScene;

public class CardHolderDeletedSignalArgs : SignalArgs
{
	public readonly ICardHolder CardHolder;

	public CardHolderDeletedSignalArgs(object dispatcher, ICardHolder cardHolder)
		: base(dispatcher)
	{
		CardHolder = cardHolder;
	}
}
