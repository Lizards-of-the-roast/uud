using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.BrowserCardHeaders;

public class ActionSourceHeaders : ModalBrowserCardHeaderProvider.ISubProvider
{
	private readonly IClientLocProvider _clientLocProvider;

	private readonly IGreLocProvider _greLocProvider;

	private readonly ICardDataProvider _cardDataProvider;

	private readonly IGameStateProvider _gameStateProvider;

	public ActionSourceHeaders(IClientLocProvider clientLocProvider, IGreLocProvider greLocProvider, ICardDataProvider cardDataProvider, IGameStateProvider gameStateProvider)
	{
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
		_greLocProvider = greLocProvider ?? NullGreLocManager.Default;
		_cardDataProvider = cardDataProvider ?? NullCardDataProvider.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
	}

	public bool TryGetHeaderData(ICardDataAdapter cardModel, AbilityPrintingData abilityData, Action action, out ModalBrowserCardHeaderProvider.HeaderData headerData)
	{
		uint? num = action.SourceGrpId(_gameStateProvider.CurrentGameState);
		if (!num.HasValue)
		{
			headerData = ModalBrowserCardHeaderProvider.HeaderData.Null;
			return false;
		}
		uint value = num.Value;
		switch (value)
		{
		case 3u:
			headerData = new ModalBrowserCardHeaderProvider.HeaderData(useActionTypeHeader: true, _clientLocProvider.GetLocalizedText("Card/FaceDownCardTitle"));
			return true;
		case 2u:
			headerData = ModalBrowserCardHeaderProvider.HeaderData.Null;
			return true;
		default:
		{
			if (SourceIsCardPrinting(value, out var sourceCard))
			{
				headerData = new ModalBrowserCardHeaderProvider.HeaderData(useActionTypeHeader: true, _greLocProvider.GetLocalizedText(sourceCard.TitleId));
				return true;
			}
			headerData = ModalBrowserCardHeaderProvider.HeaderData.Null;
			return false;
		}
		}
	}

	private bool SourceIsCardPrinting(uint sourceId, out CardPrintingData sourceCard)
	{
		sourceCard = _cardDataProvider.GetCardPrintingById(sourceId);
		return sourceCard != null;
	}
}
