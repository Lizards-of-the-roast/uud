using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasBecomeColorless : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, IsColorless(bb.CardData));
	}

	private bool IsColorless(ICardDataAdapter cardData)
	{
		if (cardData == null || cardData.Printing == null || cardData.Instance == null)
		{
			return false;
		}
		if (!CardUtilities.HasChangedColor(cardData))
		{
			return false;
		}
		if (cardData.Colors.Count > 0)
		{
			return false;
		}
		return true;
	}
}
