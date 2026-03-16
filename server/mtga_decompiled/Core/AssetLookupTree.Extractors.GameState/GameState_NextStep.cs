using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.GameState;

public class GameState_NextStep : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb?.GameState == null)
		{
			return false;
		}
		value = (int)bb.GameState.NextStep;
		return true;
	}
}
