using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class DamageAmount : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = bb.DamageAmount.GetValueOrDefault();
		return bb.DamageAmount.HasValue;
	}
}
