using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Player;

public class Player_PendingEffectAbilityIds : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Player != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Player.PendingEffects.Select((PendingEffectData x) => (int)x.AbilityId), MinCount, MaxCount);
		}
		return false;
	}
}
