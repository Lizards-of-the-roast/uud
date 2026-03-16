using AssetLookupTree.Blackboard;
using Wizards.Mtga.Platforms;

namespace AssetLookupTree.Evaluators;

public abstract class EvaluatorBase_AspectRatioInRange : IEvaluator
{
	public AspectRatioRange AspectRatioRangeToCheck;

	public abstract bool Execute(IBlackboard bb);

	public static bool GetResult(float aspectRatio, AspectRatioRange aspectRatioRange)
	{
		return aspectRatioRange.Contains(aspectRatio);
	}
}
