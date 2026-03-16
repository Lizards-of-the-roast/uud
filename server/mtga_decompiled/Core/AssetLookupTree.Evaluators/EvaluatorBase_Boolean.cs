using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators;

public abstract class EvaluatorBase_Boolean : IEvaluator
{
	public bool ExpectedResult = true;

	public abstract bool Execute(IBlackboard bb);

	public static bool GetResult(bool expectedResult, bool inValue)
	{
		return expectedResult == inValue;
	}
}
