using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards.Text;

namespace AssetLookupTree.Extractors.CardText.Loyalty;

public class CardText_Loyalty_Valence : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.CardTextEntry is LoyaltyTextEntry loyaltyTextEntry)
		{
			value = (int)loyaltyTextEntry.GetValence();
			return true;
		}
		value = 0;
		return false;
	}
}
