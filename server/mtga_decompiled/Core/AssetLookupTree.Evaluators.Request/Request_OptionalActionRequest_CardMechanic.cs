using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Request;

public class Request_OptionalActionRequest_CardMechanic : EvaluatorBase_List<CardMechanicType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Request is OptionalActionMessageRequest optionalActionMessageRequest)
		{
			return EvaluatorBase_List<CardMechanicType>.GetResult(ExpectedValues, Operation, ExpectedResult, optionalActionMessageRequest.Mechanics, MinCount, MaxCount);
		}
		return false;
	}
}
