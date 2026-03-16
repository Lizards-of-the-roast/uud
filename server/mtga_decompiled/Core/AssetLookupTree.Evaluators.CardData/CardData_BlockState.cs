using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_BlockState : EvaluatorBase_List<BlockState>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance != null)
		{
			return EvaluatorBase_List<BlockState>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Instance.BlockState);
		}
		return false;
	}
}
