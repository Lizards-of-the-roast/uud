using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.GameState;

public class GameState_AbilityWordCount_Local : EvaluatorBase_Int
{
	public string AbilityWord;

	public override bool Execute(IBlackboard bb)
	{
		if (string.IsNullOrWhiteSpace(AbilityWord))
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
