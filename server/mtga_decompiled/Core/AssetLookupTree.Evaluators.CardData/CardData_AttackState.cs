using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_AttackState : EvaluatorBase_List<AttackState>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance != null)
		{
			return EvaluatorBase_List<AttackState>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Instance.AttackState);
		}
		return false;
	}
}
