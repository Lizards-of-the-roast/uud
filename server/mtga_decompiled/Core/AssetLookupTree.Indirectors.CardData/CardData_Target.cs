using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.DuelScene;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_Target : IIndirector
{
	private ICardDataAdapter _cacheCardData;

	private CardHolderType _cacheHolderType;

	private MtgPlayer _cachePlayer;

	public void SetCache(IBlackboard bb)
	{
		_cacheCardData = bb.CardData;
		_cacheHolderType = bb.CardHolderType;
		_cachePlayer = bb.Player;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataRaw(_cacheCardData);
		bb.CardHolderType = _cacheHolderType;
		bb.Player = _cachePlayer;
		_cacheCardData = null;
		_cacheHolderType = CardHolderType.None;
		_cachePlayer = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			yield break;
		}
		foreach (MtgEntity target in bb.CardData.Targets)
		{
			if (!(target is MtgCardInstance instance))
			{
				if (target is MtgPlayer player)
				{
					bb.SetCardDataRaw(_cacheCardData);
					bb.CardHolderType = _cacheHolderType;
					bb.Player = player;
					yield return bb;
				}
			}
			else
			{
				GreClient.CardData.CardData cardData = CardDataExtensions.CreateWithDatabase(instance, bb.CardDatabase);
				bb.SetCardDataExtensive(cardData);
				bb.CardHolderType = cardData.ZoneType.ToCardHolderType();
				bb.Player = _cachePlayer;
				yield return bb;
			}
		}
	}
}
