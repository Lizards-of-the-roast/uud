namespace Wotc.Mtga.DuelScene;

public class NullCardHolderBuilder : ICardHolderBuilder
{
	public static readonly ICardHolderBuilder Default = new NullCardHolderBuilder();

	public ICardHolder CreateCardHolder(CardHolderType cardHolderType, GREPlayerNum owner)
	{
		return null;
	}

	public bool DestroyCardHolder(ICardHolder cardHolder)
	{
		return false;
	}
}
