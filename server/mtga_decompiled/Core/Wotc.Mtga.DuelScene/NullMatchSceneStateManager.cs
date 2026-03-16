namespace Wotc.Mtga.DuelScene;

public class NullMatchSceneStateManager : IMatchSceneStateManager, IMatchSceneStateProvider, IMatchSceneStateController
{
	public static readonly IMatchSceneStateManager Default = new NullMatchSceneStateManager();

	private static readonly IMatchSceneStateProvider _provider = NullMatchSceneStateProvider.Default;

	private static readonly IMatchSceneStateController _controller = NullMatchSceneStateController.Default;

	public ObservableValue<MatchSceneState> CurrentState => _provider.CurrentState;

	public void SetState(MatchSceneState state)
	{
		_controller.SetState(state);
	}
}
