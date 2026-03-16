using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Qualification;

public class Qualification_HasDetailAffector : EvaluatorBase_Boolean
{
	public string DetailKey;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.Qualification.HasValue && bb.Qualification.Value.AffectedId != 0 && bb.Qualification.Value.Details.TryGetValue(DetailKey, out var value) && uint.TryParse(value, out var result))
		{
			return bb.Qualification.Value.AffectedId == result;
		}
		return false;
	}
}
