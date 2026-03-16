using Wotc.Mtga.DuelScene.UXEvents;

namespace Wotc.Mtga.DuelScene;

public class FamiliarRequestHandler : IUpdate
{
	private readonly HeadlessClient _familiarClient;

	private readonly UXEventQueue _eventQueue;

	private readonly IGameStateProvider _gameStateProvider;

	public FamiliarRequestHandler(HeadlessClient familiarClient, IContext context)
	{
		_familiarClient = familiarClient;
		_eventQueue = context.Get<UXEventQueue>() ?? new UXEventQueue();
		_gameStateProvider = context.Get<IGameStateProvider>() ?? NullGameStateProvider.Default;
	}

	public void OnUpdate(float time)
	{
		if (_familiarClient.PendingInteraction != null && _familiarClient.StrategyEnabled && !_eventQueue.IsRunning && GameStatesAreInSync())
		{
			_familiarClient.HandleRequest();
		}
	}

	private bool GameStatesAreInSync()
	{
		return _gameStateProvider.CurrentGameState.Value == _gameStateProvider.LatestGameState.Value;
	}
}
