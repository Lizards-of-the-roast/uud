using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullGameStateManager : IGameStateManager, IGameStateController, IGameStateProvider
{
	public static readonly IGameStateManager Default = new NullGameStateManager();

	private static readonly IGameStateProvider _provider = NullGameStateProvider.Default;

	private static readonly IGameStateController _controller = NullGameStateController.Default;

	public ObservableReference<MtgGameState> CurrentGameState => _provider.CurrentGameState;

	public ObservableReference<MtgGameState> LatestGameState => _provider.LatestGameState;

	public void SetCurrentGameState(MtgGameState gameState)
	{
		_controller.SetCurrentGameState(gameState);
	}

	public void SetLatestGameState(MtgGameState gameState)
	{
		_controller.SetCurrentGameState(gameState);
	}
}
