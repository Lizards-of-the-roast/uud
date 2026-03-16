using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasRegenerationPending : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (!EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData?.Instance != null && bb.CardData.Instance.RegenerateCount != 0))
		{
			if (bb.CardData?.Parent != null)
			{
				ICardDataAdapter cardData = bb.CardData;
				if (cardData == null)
				{
					return false;
				}
				return cardData.Parent.RegenerateCount != 0;
			}
			return false;
		}
		return true;
	}
}
