using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_Owner : EvaluatorBase_List<global::GREPlayerNum>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<global::GREPlayerNum>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.OwnerNum);
		}
		return false;
	}
}
