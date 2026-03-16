using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.General;

public class SyntheticEventType : EvaluatorBase_List<Wotc.Mtgo.Gre.External.Messaging.SyntheticEventType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<Wotc.Mtgo.Gre.External.Messaging.SyntheticEventType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.SyntheticEvent);
	}
}
