using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Player;

public class Player_SeatId : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.Player != null)
		{
			value = (int)bb.Player.InstanceId;
			return true;
		}
		value = 0;
		return false;
	}
}
