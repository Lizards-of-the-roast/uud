using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class GameStatePlaybackStartedSignalArgs : SignalArgs
{
	public readonly MtgGameState GameState;

	public GameStatePlaybackStartedSignalArgs(object dispatcher, MtgGameState gameState)
		: base(dispatcher)
	{
		GameState = gameState;
	}
}
