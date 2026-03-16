using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_DigitalReleaseSet : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.CardData == null)
		{
			return false;
		}
		if (bb.CardData.DigitalReleaseSet == null)
		{
			return false;
		}
		value = bb.CardData.DigitalReleaseSet;
		return true;
	}
}
