using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Carousel;

public class Carousel_Name : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CarouselName != null)
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.CarouselName);
		}
		return false;
	}
}
