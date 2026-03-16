using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.GameState;

public class GameState_AbilityWordCount_Opponent : EvaluatorBase_Int
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
		MtgPlayer opponent = gameState.Opponent;
		if (opponent == null)
		{
			return false;
		}
		if (!gameState.PlayerToActiveAbilityWords.TryGetValue(opponent.InstanceId, out var value))
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
