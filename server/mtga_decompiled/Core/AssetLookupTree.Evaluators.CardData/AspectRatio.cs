using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class AspectRatio : EvaluatorBase_AspectRatioInRange
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_AspectRatioInRange.GetResult(bb.AspectRatio, AspectRatioRangeToCheck);
	}
}
