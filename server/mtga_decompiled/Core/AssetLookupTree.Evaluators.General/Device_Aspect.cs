using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class Device_Aspect : EvaluatorBase_AspectRatioInRange
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_AspectRatioInRange.GetResult(bb.AspectRatio, AspectRatioRangeToCheck);
	}
}
