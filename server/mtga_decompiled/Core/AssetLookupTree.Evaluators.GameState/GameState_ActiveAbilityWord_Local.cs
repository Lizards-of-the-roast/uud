using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.GameState;

public class GameState_ActiveAbilityWord_Local : EvaluatorBase_String
{
	public string AbilityWord;

	public override bool Execute(IBlackboard bb)
	{
		if (string.IsNullOrWhiteSpace(ExpectedValue))
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
		return value.ContainsKey(ExpectedValue);
	}
}
