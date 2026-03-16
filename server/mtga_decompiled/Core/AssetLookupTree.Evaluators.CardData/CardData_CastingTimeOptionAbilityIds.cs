using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_CastingTimeOptionAbilityIds : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.CastingTimeOptions.Select((CastingTimeOption x) => (int)x.AbilityId.GetValueOrDefault()), MinCount, MaxCount);
		}
		return false;
	}
}
