using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_Decorators : EvaluatorBase_List<DecoratorType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			return !ExpectedResult;
		}
		if (bb.CardData.Instance == null)
		{
			return !ExpectedResult;
		}
		MtgCardInstance instance = bb.CardData.Instance;
		if (instance.Decorators == null || instance.Decorators.Count == 0)
		{
			return !ExpectedResult;
		}
		return EvaluatorBase_List<DecoratorType>.GetResult(ExpectedValues, Operation, ExpectedResult, instance.Decorators.Values.SelectMany((HashSet<DecoratorType> x) => x), MinCount, MaxCount);
	}
}
