using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class TargetIndex : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = bb.TargetIndex.GetValueOrDefault();
		return bb.TargetIndex.HasValue;
	}
}
