using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.GameState.GameInfo;

public class GameState_GameInfo_MatchState : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb?.GameState?.GameInfo == null)
		{
			return false;
		}
		value = (int)bb.GameState.GameInfo.MatchState;
		return true;
	}
}
