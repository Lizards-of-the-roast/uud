using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.PurchaseFlows;

public class PurchaseFlows : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.PurchaseFlow;
		return true;
	}
}
