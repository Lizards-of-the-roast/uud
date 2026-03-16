using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Zone;

public abstract class ZoneEvaluatorBase_List<T> : EvaluatorBase_List<T>
{
	public override bool Execute(IBlackboard bb)
	{
		MtgZone zone = GetZone(bb);
		if (zone != null)
		{
			return EvaluatorBase_List<T>.GetResult(ExpectedValues, Operation, ExpectedResult, GetValue(zone));
		}
		return false;
	}

	protected abstract T GetValue(MtgZone zone);

	protected abstract MtgZone GetZone(IBlackboard bb);
}
