using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.ZonePair;

public class ZonePair_FromZone : EvaluatorBase_List<ZoneType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<ZoneType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.ZonePair.FromZone);
	}
}
