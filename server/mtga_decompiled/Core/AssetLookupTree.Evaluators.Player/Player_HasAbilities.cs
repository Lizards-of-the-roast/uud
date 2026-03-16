using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.Player;

public class Player_HasAbilities : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Player != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Player.Abilities.Select((AbilityPrintingData x) => (int)x.Id), MinCount, MaxCount);
		}
		return false;
	}
}
