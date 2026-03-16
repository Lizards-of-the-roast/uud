using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_PresentationColor : EvaluatorBase_List<CardFrameKey>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<CardFrameKey>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.PresentationColor);
		}
		return false;
	}
}
