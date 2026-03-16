namespace Wotc.Mtga.DuelScene;

public interface IDuelSceneStateProvider
{
	ObservableValue<DuelSceneState> CurrentState { get; }

	bool AllowInput => CurrentState.Value.AllowInput;
}
