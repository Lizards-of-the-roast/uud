using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_CastingTimeOptionCostType : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.CastingTimeOptions != null)
		{
			return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.CastingTimeOptions.Select((CastingTimeOption x) => x.CostType ?? null), MinCount, MaxCount);
		}
		return false;
	}
}
