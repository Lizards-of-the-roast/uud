using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_CatalogId : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		ICardDataAdapter cardData = bb.CardData;
		if (cardData == null)
		{
			return false;
		}
		MtgCardInstance instance = cardData.Instance;
		if (instance == null)
		{
			return false;
		}
		value = (int)instance.CatalogId;
		return true;
	}
}
