using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IGameStateProvider
{
	ObservableReference<MtgGameState> CurrentGameState { get; }

	ObservableReference<MtgGameState> LatestGameState { get; }
}
