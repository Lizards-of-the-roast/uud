using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullGameStateProvider : IGameStateProvider
{
	public static readonly IGameStateProvider Default = new NullGameStateProvider();

	public ObservableReference<MtgGameState> CurrentGameState => new ObservableReference<MtgGameState>();

	public ObservableReference<MtgGameState> LatestGameState => new ObservableReference<MtgGameState>();
}
