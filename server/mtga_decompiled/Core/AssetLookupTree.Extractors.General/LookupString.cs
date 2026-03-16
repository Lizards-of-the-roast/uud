using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class LookupString : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.LookupString;
		return !string.IsNullOrWhiteSpace(bb.LookupString);
	}
}
