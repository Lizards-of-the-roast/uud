using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_FakeStyleCard : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.IsFakeStyleCard);
		}
		return false;
	}
}
