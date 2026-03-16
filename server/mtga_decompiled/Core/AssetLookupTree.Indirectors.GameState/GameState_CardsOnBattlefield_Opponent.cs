using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.GameState;

public class GameState_CardsOnBattlefield_Opponent : IIndirector
{
	private ICardDataAdapter _cacheCard;

	private CardHolderType _cacheHolder;

	public void SetCache(IBlackboard bb)
	{
		_cacheCard = bb.CardData;
		_cacheHolder = bb.CardHolderType;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataRaw(_cacheCard);
		bb.CardHolderType = _cacheHolder;
		_cacheCard = null;
		_cacheHolder = CardHolderType.None;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.GameState == null)
		{
			yield break;
		}
		bb.CardHolderType = CardHolderType.Battlefield;
		foreach (MtgCardInstance opponentBattlefieldCard in bb.GameState.OpponentBattlefieldCards)
		{
			bb.SetCardDataExtensive(CardDataExtensions.CreateWithDatabase(opponentBattlefieldCard, bb.CardDatabase));
			yield return bb;
		}
	}
}
