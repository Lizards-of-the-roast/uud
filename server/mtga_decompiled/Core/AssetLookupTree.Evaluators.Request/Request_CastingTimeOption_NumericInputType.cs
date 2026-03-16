using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Request;

public class Request_CastingTimeOption_NumericInputType : EvaluatorBase_List<NumericInputType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Request is CastingTimeOption_NumericInputRequest castingTimeOption_NumericInputRequest)
		{
			return EvaluatorBase_List<NumericInputType>.GetResult(ExpectedValues, Operation, ExpectedResult, castingTimeOption_NumericInputRequest.InputType);
		}
		return false;
	}
}
