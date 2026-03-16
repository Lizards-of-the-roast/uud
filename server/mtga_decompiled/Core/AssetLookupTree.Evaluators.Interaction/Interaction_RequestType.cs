using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Interaction;

public class Interaction_RequestType : EvaluatorBase_List<RequestType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Interaction != null)
		{
			return EvaluatorBase_List<RequestType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Interaction.Type);
		}
		return false;
	}
}
