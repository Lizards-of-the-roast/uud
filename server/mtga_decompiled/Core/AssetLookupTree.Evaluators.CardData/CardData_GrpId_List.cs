using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_GrpId_List : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			return false;
		}
		return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, (int)CardData_GrpId.GetGrpId(bb.CardData));
	}
}
