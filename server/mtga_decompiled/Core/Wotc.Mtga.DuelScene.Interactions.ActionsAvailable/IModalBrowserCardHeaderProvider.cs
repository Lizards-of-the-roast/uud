using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public interface IModalBrowserCardHeaderProvider
{
	BrowserCardHeader.BrowserCardHeaderData GetBrowserCardInfo(ICardDataAdapter cardModel, Action action);
}
