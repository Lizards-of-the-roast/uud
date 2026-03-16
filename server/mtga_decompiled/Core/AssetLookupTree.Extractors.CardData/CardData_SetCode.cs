using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_SetCode : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.CardData == null)
		{
			return false;
		}
		if (bb.CardData.ExpansionCode == null)
		{
			return false;
		}
		value = bb.CardData.ExpansionCode;
		return true;
	}
}
