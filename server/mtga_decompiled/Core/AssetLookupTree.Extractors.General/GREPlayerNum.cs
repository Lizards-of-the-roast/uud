using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class GREPlayerNum : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.GREPlayerNum != global::GREPlayerNum.Invalid)
		{
			value = (int)bb.GREPlayerNum;
			return true;
		}
		if (bb.Player != null)
		{
			value = (int)bb.Player.ClientPlayerEnum;
			return true;
		}
		value = 0;
		return false;
	}
}
