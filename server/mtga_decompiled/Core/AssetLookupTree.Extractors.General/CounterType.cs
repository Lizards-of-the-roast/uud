using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class CounterType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.CounterType;
		return true;
	}
}
