using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.GameState.GameInfo;

public class GameState_PlayerAbilityWordCount_Opponent : GameState_PlayerAbilityWordCount
{
	public override Dictionary<string, int> AbilityWordCount(IBlackboard bb)
	{
		if (!TryGetCount(bb.GameState, out var result))
		{
			return new Dictionary<string, int>();
		}
		return result;
	}

	private bool TryGetCount(MtgGameState gameState, out Dictionary<string, int> result)
	{
		result = null;
		if (gameState == null)
		{
			return false;
		}
		MtgPlayer opponent = gameState.Opponent;
		if (opponent == null)
		{
			return false;
		}
		return gameState.PlayerToActiveAbilityWords.TryGetValue(opponent.InstanceId, out result);
	}
}
