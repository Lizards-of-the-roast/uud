using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.GameState.GameInfo;

public class GameState_GameInfo_Stage : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb?.GameState?.GameInfo == null)
		{
			return false;
		}
		value = (int)bb.GameState.GameInfo.Stage;
		return true;
	}
}
