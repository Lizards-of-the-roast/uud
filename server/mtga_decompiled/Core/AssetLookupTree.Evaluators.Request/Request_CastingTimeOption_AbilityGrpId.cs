using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Request;

public class Request_CastingTimeOption_AbilityGrpId : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Request is CastingTimeOption_ModalRequest castingTimeOption_ModalRequest)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, (int)castingTimeOption_ModalRequest.AbilityGrpId);
		}
		return false;
	}
}
