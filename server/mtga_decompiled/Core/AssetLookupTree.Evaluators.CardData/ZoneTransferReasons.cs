using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class ZoneTransferReasons : EvaluatorBase_List<ZoneTransferReason>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<ZoneTransferReason>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.ZoneTransferReason);
	}
}
