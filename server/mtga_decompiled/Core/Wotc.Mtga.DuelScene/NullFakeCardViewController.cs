using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public class NullFakeCardViewController : IFakeCardViewController
{
	public static readonly IFakeCardViewController Default = new NullFakeCardViewController();

	public DuelScene_CDC CreateFakeCard(string key, ICardDataAdapter cardData, bool isVisible = false)
	{
		return null;
	}

	public bool DeleteFakeCard(string key)
	{
		return false;
	}
}
