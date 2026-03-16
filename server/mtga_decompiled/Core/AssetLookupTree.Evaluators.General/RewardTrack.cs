using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class RewardTrack : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (!string.IsNullOrWhiteSpace(bb.RewardTrack))
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.RewardTrack);
		}
		return false;
	}
}
