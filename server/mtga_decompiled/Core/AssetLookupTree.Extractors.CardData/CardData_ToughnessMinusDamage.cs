using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_ToughnessMinusDamage : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.CardData == null)
		{
			return false;
		}
		value = (int)(bb.CardData.Toughness.Value - bb.CardData.Damage);
		return true;
	}
}
