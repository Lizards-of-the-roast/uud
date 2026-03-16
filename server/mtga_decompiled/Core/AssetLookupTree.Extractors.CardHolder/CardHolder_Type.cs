using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardHolder;

public class CardHolder_Type : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)(bb.CardHolder?.CardHolderType ?? bb.CardHolderType);
		return true;
	}
}
