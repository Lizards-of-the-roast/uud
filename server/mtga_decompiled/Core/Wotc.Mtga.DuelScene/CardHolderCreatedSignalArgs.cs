namespace Wotc.Mtga.DuelScene;

public class CardHolderCreatedSignalArgs : SignalArgs
{
	public readonly ICardHolder CardHolder;

	public CardHolderCreatedSignalArgs(object dispatcher, ICardHolder cardHolder)
		: base(dispatcher)
	{
		CardHolder = cardHolder;
	}
}
