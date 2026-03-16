using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Extractors.GameState.StateBasedActions;

public class GameState_StateBasedActions_LossOfGameReason : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb != null)
		{
			MtgGameState gameState = bb.GameState;
			if (gameState != null)
			{
				_ = gameState.GameLossData;
				if (0 == 0)
				{
					value = (int)bb.GameState.GameLossData.Reason;
					return true;
				}
			}
		}
		return false;
	}
}
