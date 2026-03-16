using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Utility;

public class Utility_And : Utility_Compound
{
	public override bool Execute(IBlackboard bb)
	{
		bool flag = true;
		foreach (IEvaluator nestedEvaluator in NestedEvaluators)
		{
			flag &= nestedEvaluator.Execute(bb);
		}
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, flag);
	}
}
