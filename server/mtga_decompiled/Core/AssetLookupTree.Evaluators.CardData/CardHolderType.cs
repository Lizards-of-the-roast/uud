using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardHolderType : EvaluatorBase_List<global::CardHolderType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<global::CardHolderType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardHolderType);
	}
}
