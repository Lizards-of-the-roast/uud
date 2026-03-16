using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IGameStateController
{
	void SetCurrentGameState(MtgGameState gameState);

	void SetLatestGameState(MtgGameState gameState);
}
