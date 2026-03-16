using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_Parent : IIndirector
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
		if (bb.CardData?.Parent != null)
		{
			GreClient.CardData.CardData cardData = new GreClient.CardData.CardData(bb.CardData.Parent, bb.CardDatabase.GetPrintingFromInstance(bb.CardData.Parent));
			bb.SetCardDataExtensive(cardData);
			bb.CardHolderType = cardData.ZoneType.ToCardHolderType();
			yield return bb;
		}
	}
}
