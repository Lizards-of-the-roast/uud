using GreClient.CardData;

namespace Wotc.Mtga;

public class NullCardBuilder<T> : ICardBuilder<T> where T : BASE_CDC
{
	public static readonly ICardBuilder<T> Default = new NullCardBuilder<T>();

	public T CreateCDC(ICardDataAdapter cardData, bool isVisible = false)
	{
		return null;
	}

	public void DestroyCDC(T cdc)
	{
	}
}
