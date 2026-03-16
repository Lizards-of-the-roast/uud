using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Request;

public class Request_SelectN_IdType : EvaluatorBase_List<IdType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Request == null)
		{
			return false;
		}
		if (!(bb.Request is SelectNRequest selectNRequest))
		{
			return false;
		}
		return EvaluatorBase_List<IdType>.GetResult(ExpectedValues, Operation, ExpectedResult, selectNRequest.IdType);
	}
}
