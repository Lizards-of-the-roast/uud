using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_RelevantZones : EvaluatorBase_List<ZoneType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null)
		{
			return EvaluatorBase_List<ZoneType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Ability.RelevantZones, MinCount, MaxCount);
		}
		return false;
	}
}
