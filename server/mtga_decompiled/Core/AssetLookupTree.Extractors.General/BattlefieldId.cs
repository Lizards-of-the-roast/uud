using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class BattlefieldId : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.BattlefieldId;
		return !string.IsNullOrWhiteSpace(bb.BattlefieldId);
	}
}
