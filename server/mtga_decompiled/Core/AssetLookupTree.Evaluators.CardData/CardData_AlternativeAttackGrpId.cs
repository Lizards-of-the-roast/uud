using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_AlternativeAttackGrpId : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance != null && bb.GameState != null && bb.GameState.AttackInfo.TryGetValue(bb.CardData.InstanceId, out var value))
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)value.AlternativeGrpId);
		}
		return false;
	}
}
