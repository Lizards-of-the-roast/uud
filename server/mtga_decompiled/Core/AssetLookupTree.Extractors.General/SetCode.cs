using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class SetCode : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.SetCode;
		return !string.IsNullOrWhiteSpace(bb.SetCode);
	}
}
