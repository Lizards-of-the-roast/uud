using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Utility;

public class Utility_Or : Utility_Compound
{
	public override bool Execute(IBlackboard bb)
	{
		bool flag = NestedEvaluators.Count == 0;
		foreach (IEvaluator nestedEvaluator in NestedEvaluators)
		{
			flag |= nestedEvaluator.Execute(bb);
		}
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, flag);
	}
}
