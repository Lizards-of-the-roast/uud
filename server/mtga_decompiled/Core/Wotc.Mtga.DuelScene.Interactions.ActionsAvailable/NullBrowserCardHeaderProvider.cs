using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class NullBrowserCardHeaderProvider : IModalBrowserCardHeaderProvider
{
	public static NullBrowserCardHeaderProvider Default => new NullBrowserCardHeaderProvider();

	public BrowserCardHeader.BrowserCardHeaderData GetBrowserCardInfo(ICardDataAdapter cardModel, Action action)
	{
		return null;
	}
}
