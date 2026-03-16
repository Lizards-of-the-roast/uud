using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.BadgeData;

public class BadgeData_ActivationWord : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.BadgeData == null)
		{
			return false;
		}
		value = bb.BadgeData.GetActivationWord();
		return true;
	}
}
