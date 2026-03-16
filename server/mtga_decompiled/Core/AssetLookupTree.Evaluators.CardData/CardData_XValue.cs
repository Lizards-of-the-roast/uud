using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_XValue : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		ICardDataAdapter cardData = bb.CardData;
		if (cardData != null)
		{
			MtgCardInstance instance = cardData.Instance;
			if (instance != null && instance.ChooseXResult.HasValue)
			{
				return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.CardData.Instance.ChooseXResult.Value);
			}
		}
		return false;
	}
}
