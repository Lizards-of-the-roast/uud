using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class CardInsertionPosition : EvaluatorBase_List<CardHolderBase.CardPosition>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardInsertionPosition != CardHolderBase.CardPosition.None)
		{
			return EvaluatorBase_List<CardHolderBase.CardPosition>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardInsertionPosition);
		}
		return false;
	}
}
