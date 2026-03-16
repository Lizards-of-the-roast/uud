using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.DuelScene;

namespace AssetLookupTree.Indirectors.LinkInfo;

public class LinkInfo_AffectedCard : IIndirector
{
	private ICardDataAdapter _cacheCardData;

	private CardHolderType _cacheHolderType;

	public void SetCache(IBlackboard bb)
	{
		_cacheCardData = bb.CardData;
		_cacheHolderType = bb.CardHolderType;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataRaw(_cacheCardData);
		bb.CardHolderType = _cacheHolderType;
		_cacheCardData = null;
		_cacheHolderType = CardHolderType.None;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.LinkInfo.AffectedId != 0 && bb.CardDataProvider != null && bb.GameState != null && bb.GameState.TryGetCard(bb.LinkInfo.AffectedId, out var card))
		{
			bb.SetCardDataExtensive(CardDataExtensions.CreateWithDatabase(card, bb.CardDatabase));
			bb.CardHolderType = card.Zone.Type.ToCardHolderType();
			yield return bb;
		}
	}
}
