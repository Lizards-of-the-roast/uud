using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Request;

public class Request_CastingTimeOption_GrpId : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Request is CastingTimeOption_NumericInputRequest castingTimeOption_NumericInputRequest)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, (int)castingTimeOption_NumericInputRequest.GrpId);
		}
		return false;
	}
}
