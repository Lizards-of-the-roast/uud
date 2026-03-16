using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.DuelScene.Interactions.ModalBrowser;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.BrowserCardHeaders;

public class AbilityIdHeaders : ModalBrowserCardHeaderProvider.ISubProvider
{
	private readonly IClientLocProvider _locProvider;

	private readonly IGreLocProvider _greLocProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	public AbilityIdHeaders(IClientLocProvider clientLocProvider, IGreLocProvider greLocProvider, AssetLookupSystem assetLookupSystem)
	{
		_locProvider = clientLocProvider;
		_greLocProvider = greLocProvider;
		_assetLookupSystem = assetLookupSystem;
	}

	public bool TryGetHeaderData(ICardDataAdapter cardModel, AbilityPrintingData abilityData, Action action, out ModalBrowserCardHeaderProvider.HeaderData headerData)
	{
		headerData = ModalBrowserCardHeaderProvider.HeaderData.Null;
		IBlackboard blackboard = _assetLookupSystem.Blackboard;
		blackboard.Clear();
		blackboard.SetCardDataExtensive(cardModel);
		blackboard.Ability = abilityData;
		blackboard.GreAction = action;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<AbilityIdHeader> loadedTree))
		{
			AbilityIdHeader payload = loadedTree.GetPayload(blackboard);
			if (payload != null)
			{
				string header = (string.IsNullOrEmpty(payload.HeaderLocKey.Key) ? string.Empty : payload.HeaderLocKey.GetText(_locProvider, _greLocProvider).ToString());
				string subHeader = (string.IsNullOrEmpty(payload.SubheaderLocKey.Key) ? string.Empty : payload.SubheaderLocKey.GetText(_locProvider, _greLocProvider).ToString());
				headerData = new ModalBrowserCardHeaderProvider.HeaderData(header, subHeader, payload.UseActionTypeHeader);
			}
		}
		return !headerData.IsNull();
	}
}
