using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Zone;

public abstract class ZoneEvaluatorBase_Int : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		MtgZone zone = GetZone(bb);
		if (zone != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, GetValue(zone));
		}
		return false;
	}

	protected abstract int GetValue(MtgZone zone);

	protected abstract MtgZone GetZone(IBlackboard bb);
}
