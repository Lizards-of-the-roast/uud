using GreClient.CardData;

namespace Wotc.Mtga;

public interface ICardBuilder<T> where T : BASE_CDC
{
	T CreateCDC(ICardDataAdapter cardData, bool isVisible = false);

	void DestroyCDC(T cdc);
}
