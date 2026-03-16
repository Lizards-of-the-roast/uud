using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.DuelScene;

namespace AssetLookupTree.Indirectors.ManaMovement;

public class ManaMovement_SourceCard : IIndirector
{
	private ICardDataAdapter _cardCache;

	private CardHolderType _holderCache;

	public void SetCache(IBlackboard bb)
	{
		_cardCache = bb.CardData;
		_holderCache = bb.CardHolderType;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataRaw(_cardCache);
		bb.CardHolderType = _holderCache;
		_cardCache = null;
		_holderCache = CardHolderType.None;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.ManaMovement.IsValid && bb.ManaMovement.Source is MtgCardInstance mtgCardInstance)
		{
			bb.SetCardDataExtensive(CardDataExtensions.CreateWithDatabase(mtgCardInstance, bb.CardDatabase));
			bb.CardHolderType = mtgCardInstance.Zone.Type.ToCardHolderType();
			yield return bb;
		}
	}
}
