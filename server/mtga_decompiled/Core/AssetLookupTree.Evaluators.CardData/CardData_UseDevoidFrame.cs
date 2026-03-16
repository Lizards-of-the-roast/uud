using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.Extensions;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_UseDevoidFrame : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, CanUseDevoidFrame(bb.CardData));
	}

	private bool CanUseDevoidFrame(ICardDataAdapter cardData)
	{
		if (cardData == null)
		{
			return false;
		}
		if (!cardData.PrintingAbilityIds.Contains(151u))
		{
			return false;
		}
		if (CardUtilities.HasChangedColor(cardData))
		{
			return false;
		}
		return true;
	}
}
