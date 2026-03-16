using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Request;

public class Request_CastingTimeOption_OtherSelection : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Request is CastingTimeOption_ModalRequest castingTimeOption_ModalRequest && castingTimeOption_ModalRequest.OtherSelection.Count > 0)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)castingTimeOption_ModalRequest.OtherSelection[0]);
		}
		return false;
	}
}
