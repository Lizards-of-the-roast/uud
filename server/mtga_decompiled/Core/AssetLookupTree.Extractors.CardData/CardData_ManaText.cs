using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_ManaText : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.CardData == null)
		{
			return false;
		}
		if (bb.CardData.OldSchoolManaText == null)
		{
			return false;
		}
		value = bb.CardData.OldSchoolManaText;
		return true;
	}
}
