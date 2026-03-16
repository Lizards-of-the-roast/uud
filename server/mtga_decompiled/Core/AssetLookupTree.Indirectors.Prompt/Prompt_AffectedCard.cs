using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.DuelScene;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Indirectors.Prompt;

public class Prompt_AffectedCard : IIndirector
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
		if (bb.Prompt != null && bb.GameState != null && bb.CardDataProvider != null && bb.Prompt.Parameters.Count > 1)
		{
			PromptParameter promptParameter = bb.Prompt.Parameters.Skip(1).FirstOrDefault((PromptParameter x) => x.ParameterName == "CardId" && x.Type == ParameterType.Number && x.NumberValue != 0);
			if (promptParameter != null && bb.GameState.TryGetCard((uint)promptParameter.NumberValue, out var card))
			{
				bb.SetCardDataExtensive(CardDataExtensions.CreateWithDatabase(card, bb.CardDatabase));
				bb.CardHolderType = card.Zone.Type.ToCardHolderType();
				yield return bb;
			}
		}
	}
}
