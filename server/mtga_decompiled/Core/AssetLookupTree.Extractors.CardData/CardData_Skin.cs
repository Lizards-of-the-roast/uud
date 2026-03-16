using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_Skin : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.CardData == null)
		{
			return false;
		}
		if (bb.CardData.SkinCode == null)
		{
			return false;
		}
		value = bb.CardData.SkinCode;
		return true;
	}
}
