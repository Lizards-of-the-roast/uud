using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardHolder;

public class CardHolder_Type : EvaluatorBase_List<CardHolderType>
{
	public override bool Execute(IBlackboard bb)
	{
		CardHolderType inValue = bb.CardHolder?.CardHolderType ?? bb.CardHolderType;
		return EvaluatorBase_List<CardHolderType>.GetResult(ExpectedValues, Operation, ExpectedResult, inValue);
	}
}
