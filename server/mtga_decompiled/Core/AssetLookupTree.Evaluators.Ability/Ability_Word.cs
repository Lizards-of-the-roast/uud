using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_Word : EvaluatorBase_List<AbilityWord>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null)
		{
			return EvaluatorBase_List<AbilityWord>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Ability.AbilityWord);
		}
		return false;
	}
}
