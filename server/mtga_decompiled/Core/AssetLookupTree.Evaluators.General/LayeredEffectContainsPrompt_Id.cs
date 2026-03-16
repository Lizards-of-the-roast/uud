using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.General;

public class LayeredEffectContainsPrompt_Id : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.LayeredEffects != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.LayeredEffects.Select((LayeredEffectData x) => (int)x.PromptId), MinCount, MaxCount);
		}
		return false;
	}
}
