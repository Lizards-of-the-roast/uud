using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.GameState;

public class GameState_Phase : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb?.GameState == null)
		{
			return false;
		}
		value = (int)bb.GameState.CurrentPhase;
		return true;
	}
}
