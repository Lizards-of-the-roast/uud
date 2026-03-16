using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_XValue : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		ICardDataAdapter cardData = bb.CardData;
		if (cardData != null)
		{
			MtgCardInstance instance = cardData.Instance;
			if (instance != null && instance.ChooseXResult.HasValue)
			{
				value = (int)bb.CardData.Instance.ChooseXResult.Value;
				return true;
			}
		}
		return false;
	}
}
