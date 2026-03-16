using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Qualification;

public class Qualification_HasDetailPair : EvaluatorBase_Boolean
{
	public string DetailKey;

	public string DetailValue;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.Qualification.HasValue && bb.Qualification.Value.Details.TryGetValue(DetailKey, out var value))
		{
			return DetailValue == value;
		}
		return false;
	}
}
