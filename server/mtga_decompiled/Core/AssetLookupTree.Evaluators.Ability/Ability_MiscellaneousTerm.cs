using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_MiscellaneousTerm : EvaluatorBase_List<MiscellaneousTerm>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null)
		{
			return EvaluatorBase_List<MiscellaneousTerm>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Ability.MiscellaneousTerms, MinCount, MaxCount);
		}
		return false;
	}
}
