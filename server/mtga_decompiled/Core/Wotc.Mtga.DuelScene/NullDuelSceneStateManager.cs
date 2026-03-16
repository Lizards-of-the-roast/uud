namespace Wotc.Mtga.DuelScene;

public class NullDuelSceneStateManager : IDuelSceneStateManager, IDuelSceneStateProvider, IDuelSceneStateController
{
	public static readonly IDuelSceneStateManager Default = new NullDuelSceneStateManager();

	private static readonly IDuelSceneStateProvider _provider = NullDuelSceneStateProvider.Default;

	private static readonly IDuelSceneStateController _controller = NullDuelSceneStateController.Default;

	public ObservableValue<DuelSceneState> CurrentState => _provider.CurrentState;

	public void SetState(DuelSceneState state)
	{
		_controller.SetState(state);
	}

	public void SetState(bool? lockSceneTransitions = null, bool? firstGameStatePlaybackComplete = null, bool? gameComplete = null)
	{
		_controller.SetState(lockSceneTransitions, firstGameStatePlaybackComplete, gameComplete);
	}
}
