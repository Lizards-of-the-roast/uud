using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.DuelScene.Interactions.ModalBrowser;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.BrowserCardHeaders;

public class AbilityReferenceTypeHeaders : ModalBrowserCardHeaderProvider.ISubProvider
{
	private readonly IClientLocProvider _clientLocProvider;

	private readonly IGreLocProvider _greLocProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	public AbilityReferenceTypeHeaders(IClientLocProvider clientLocProvider, IGreLocProvider greLocProvider, AssetLookupSystem assetLookupSystem)
	{
		_clientLocProvider = clientLocProvider;
		_greLocProvider = greLocProvider;
		_assetLookupSystem = assetLookupSystem;
	}

	public bool TryGetHeaderData(ICardDataAdapter cardModel, AbilityPrintingData abilityData, Action action, out ModalBrowserCardHeaderProvider.HeaderData headerData)
	{
		if (abilityData != null && _assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ReferenceTypeHeader> loadedTree))
		{
			IBlackboard blackboard = _assetLookupSystem.Blackboard;
			blackboard.Clear();
			blackboard.Ability = abilityData;
			ReferenceTypeHeader payload = loadedTree.GetPayload(blackboard);
			if (payload != null && payload.LocKey != null)
			{
				string text = payload.LocKey.GetText(_clientLocProvider, _greLocProvider);
				if (!string.IsNullOrEmpty(text))
				{
					headerData = new ModalBrowserCardHeaderProvider.HeaderData(text);
					return true;
				}
			}
		}
		headerData = ModalBrowserCardHeaderProvider.HeaderData.Null;
		return false;
	}
}
