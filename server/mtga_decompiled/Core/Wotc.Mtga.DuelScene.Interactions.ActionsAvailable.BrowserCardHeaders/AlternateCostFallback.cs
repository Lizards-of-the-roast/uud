using GreClient.CardData;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.BrowserCardHeaders;

public class AlternateCostFallback : ModalBrowserCardHeaderProvider.ISubProvider
{
	private readonly IClientLocProvider _locProvider;

	private readonly IGameStateProvider _gameStateProvider;

	public AlternateCostFallback(IClientLocProvider clientLocProvider, IGameStateProvider gameStateProvider)
	{
		_locProvider = clientLocProvider ?? NullLocProvider.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
	}

	public bool TryGetHeaderData(ICardDataAdapter cardModel, AbilityPrintingData abilityData, Action action, out ModalBrowserCardHeaderProvider.HeaderData headerData)
	{
		uint? num = action.SourceGrpId(_gameStateProvider.CurrentGameState);
		if (num.HasValue && num.Value == 0)
		{
			num = action.AlternativeGrpId;
		}
		if (!num.HasValue && abilityData == null)
		{
			headerData = ModalBrowserCardHeaderProvider.HeaderData.Null;
			return false;
		}
		headerData = new ModalBrowserCardHeaderProvider.HeaderData(useActionTypeHeader: true, _locProvider.GetLocalizedText("DuelScene/Browsers/BrowserCardInfo_AlternateCost"));
		return true;
	}
}
