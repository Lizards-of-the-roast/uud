using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class CardInsertionPosition : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.CardInsertionPosition;
		return bb.CardInsertionPosition != CardHolderBase.CardPosition.None;
	}
}
