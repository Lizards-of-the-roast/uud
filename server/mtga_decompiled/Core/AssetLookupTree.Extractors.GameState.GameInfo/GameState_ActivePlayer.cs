using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.GameState.GameInfo;

public class GameState_ActivePlayer : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb?.GameState?.ActivePlayer == null)
		{
			return false;
		}
		value = (int)bb.GameState.ActivePlayer.ClientPlayerEnum;
		return true;
	}
}
