using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.GameState.GameInfo;

public class GameState_DecidingPlayer : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb?.GameState?.DecidingPlayer == null)
		{
			return false;
		}
		value = (int)bb.GameState.DecidingPlayer.ClientPlayerEnum;
		return true;
	}
}
