using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_ReasonFaceDownFromSourceAbilityId : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.CardData.Instance.FaceDownState.ReasonFaceDownSourceAbilityGrpId);
		}
		return false;
	}
}
