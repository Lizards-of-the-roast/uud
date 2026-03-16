using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_ActiveAbilityWord_ThresholdMet : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null || bb.CardData.ActiveAbilityWords.Count < 1)
		{
			return false;
		}
		if (bb.CardData.ActiveAbilityWords.TryGetUnmetThreshold(this, (EvaluatorBase_String evaluator, string abilityWord) => EvaluatorBase_String.GetResult(evaluator.ExpectedValue, evaluator.Operation, evaluator.ExpectedResult, abilityWord), out var thresholdAbilityWord))
		{
			return thresholdAbilityWord.Values.FirstOrDefault() >= thresholdAbilityWord.Threshold;
		}
		return false;
	}
}
