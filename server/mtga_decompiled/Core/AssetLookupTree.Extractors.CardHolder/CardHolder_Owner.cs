using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardHolder;

public class CardHolder_Owner : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)(bb.CardHolder?.PlayerNum ?? bb.GREPlayerNum);
		return true;
	}
}
