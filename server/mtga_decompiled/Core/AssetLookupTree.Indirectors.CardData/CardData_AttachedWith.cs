using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.DuelScene;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_AttachedWith : IIndirector
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
		if (bb.CardData?.Instance == null || bb.GameState == null || bb.CardData.Instance.AttachedWithIds.Count == 0)
		{
			yield break;
		}
		foreach (uint attachedWithId in bb.CardData.Instance.AttachedWithIds)
		{
			if (bb.GameState.TryGetCard(attachedWithId, out var card))
			{
				GreClient.CardData.CardData cardData = CardDataExtensions.CreateWithDatabase(card, bb.CardDatabase);
				bb.SetCardDataExtensive(cardData);
				bb.CardHolderType = cardData.ZoneType.ToCardHolderType();
				yield return bb;
			}
		}
	}
}
