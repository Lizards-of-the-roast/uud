using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards.Text;

namespace AssetLookupTree.Extractors.CardText;

public class CardText_EntryType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.CardTextEntry != null)
		{
			value = (int)bb.CardTextEntry.GetEntryType();
			return true;
		}
		value = 0;
		return false;
	}
}
