using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasLoyalty : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, HasLoyalty(bb.CardData));
	}

	private bool HasLoyalty(ICardDataAdapter cardData)
	{
		if (cardData == null)
		{
			return false;
		}
		if (!cardData.PrintedTypes.Contains(CardType.Planeswalker))
		{
			return false;
		}
		MtgCardInstance instance = cardData.Instance;
		if (instance == null || !instance.Loyalty.HasValue)
		{
			return !string.IsNullOrEmpty(cardData.Printing.Toughness.RawText);
		}
		return true;
	}
}
