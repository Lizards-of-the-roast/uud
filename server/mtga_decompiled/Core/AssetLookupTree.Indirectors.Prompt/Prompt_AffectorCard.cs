using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.DuelScene;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Indirectors.Prompt;

public class Prompt_AffectorCard : IIndirector
{
	private const string CARD_ID = "CardId";

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
		if (bb.Prompt != null && bb.GameState != null && bb.CardDataProvider != null && bb.Prompt.Parameters.Count > 0 && bb.Prompt.Parameters[0].ParameterName == "CardId" && bb.Prompt.Parameters[0].Type == ParameterType.Number)
		{
			int numberValue = bb.Prompt.Parameters[0].NumberValue;
			if (bb.GameState.TryGetCard((uint)numberValue, out var card))
			{
				bb.SetCardDataExtensive(CardDataExtensions.CreateWithDatabase(card, bb.CardDatabase));
				bb.CardHolderType = card.Zone.Type.ToCardHolderType();
				yield return bb;
			}
		}
	}
}
