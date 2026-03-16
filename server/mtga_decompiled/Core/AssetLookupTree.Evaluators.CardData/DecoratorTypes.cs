using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class DecoratorTypes : EvaluatorBase_List<DecoratorType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.DecoratorTypes != null)
		{
			return EvaluatorBase_List<DecoratorType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.DecoratorTypes, MinCount, MaxCount);
		}
		return false;
	}
}
