using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.GameState.GameInfo;

public abstract class GameState_PlayerAbilityWordCount : EvaluatorBase_Int
{
	public string AbilityWord = string.Empty;

	public abstract Dictionary<string, int> AbilityWordCount(IBlackboard bb);

	public override bool Execute(IBlackboard bb)
	{
		if (bb == null)
		{
			return false;
		}
		MtgGameState gameState = bb.GameState;
		if (gameState == null)
		{
			return false;
		}
		MtgPlayer localPlayer = gameState.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}
		if (!gameState.PlayerToActiveAbilityWords.TryGetValue(localPlayer.InstanceId, out var value))
		{
			return false;
		}
		if (!value.TryGetValue(AbilityWord, out var value2))
		{
			return false;
		}
		return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, value2);
	}
}
