using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_Tag : EvaluatorBase_List<MetaDataTag>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null)
		{
			return EvaluatorBase_List<MetaDataTag>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Ability.Tags, MinCount, MaxCount);
		}
		return false;
	}
}
