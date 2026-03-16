using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_LayeredEffectSourceIds : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Instance.LayeredEffects.Select((LayeredEffectData x) => (int)x.SourceAbilityId), MinCount, MaxCount);
		}
		return false;
	}
}
