using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Player;

public class Player_LayeredEffectSourceIds : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Player != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Player.LayeredEffects.Select((LayeredEffectData x) => (int)x.SourceAbilityId), MinCount, MaxCount);
		}
		return false;
	}
}
