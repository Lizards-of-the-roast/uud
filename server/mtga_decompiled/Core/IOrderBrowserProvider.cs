using Wotc.Mtgo.Gre.External.Messaging;

public interface IOrderBrowserProvider : IBasicBrowserProvider, ICardBrowserProvider, IDuelSceneBrowserProvider, IBrowserHeaderProvider
{
	string GetLeftOrderText();

	string GetRightOrderText();

	OrderingContext GetOrderingContext();

	OrderIndicator.ArrowDirection GetArrowDirection();
}
