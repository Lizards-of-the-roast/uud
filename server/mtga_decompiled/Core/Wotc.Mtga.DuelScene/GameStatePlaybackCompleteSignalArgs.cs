using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class GameStatePlaybackCompleteSignalArgs : SignalArgs
{
	public readonly MtgGameState GameState;

	public GameStatePlaybackCompleteSignalArgs(object dispatcher, MtgGameState gameState)
		: base(dispatcher)
	{
		GameState = gameState;
	}
}
