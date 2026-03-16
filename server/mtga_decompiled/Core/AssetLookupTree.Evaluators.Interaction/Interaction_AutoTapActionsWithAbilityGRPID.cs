using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Interaction;

public class Interaction_AutoTapActionsWithAbilityGRPID : EvaluatorBase_Int
{
	public int Id;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.GreAction?.AutoTapSolution?.AutoTapActions == null)
		{
			return false;
		}
		return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, AutoTapActionsWithAbilityGRPIDCount(bb.GreAction.AutoTapSolution, Id));
	}

	private int AutoTapActionsWithAbilityGRPIDCount(AutoTapSolution autoTapSolution, int grpid)
	{
		int num = 0;
		foreach (AutoTapAction autoTapAction in autoTapSolution.AutoTapActions)
		{
			if (autoTapAction.AbilityGrpId == grpid)
			{
				num++;
			}
		}
		return num;
	}
}
