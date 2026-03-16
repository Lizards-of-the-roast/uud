namespace Wotc.Mtga.DuelScene;

public interface IMatchSceneStateProvider
{
	ObservableValue<MatchSceneState> CurrentState { get; }

	MatchSceneManager.SubScene SubScene => CurrentState.Value.SubScene;

	bool LockSceneTransitions => CurrentState.Value.LockSceneTransitions;
}
