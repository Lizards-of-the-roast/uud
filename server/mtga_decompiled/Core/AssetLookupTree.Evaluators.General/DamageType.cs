using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.General;

public class DamageType : EvaluatorBase_List<Wotc.Mtgo.Gre.External.Messaging.DamageType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.DamageType != Wotc.Mtgo.Gre.External.Messaging.DamageType.None)
		{
			return EvaluatorBase_List<Wotc.Mtgo.Gre.External.Messaging.DamageType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.DamageType);
		}
		return false;
	}
}
