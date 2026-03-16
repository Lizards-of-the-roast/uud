using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IGameStatePlaybackController
{
	void StartPlayback(MtgGameState gameState);

	void CompletePlayback(MtgGameState gameState);
}
