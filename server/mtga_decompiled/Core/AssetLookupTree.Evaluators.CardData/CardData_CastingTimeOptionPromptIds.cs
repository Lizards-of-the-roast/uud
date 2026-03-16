using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_CastingTimeOptionPromptIds : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData?.CastingTimeOptions.Select((CastingTimeOption x) => (int)x.PromptId.GetValueOrDefault()), MinCount, MaxCount);
	}
}
