namespace Wotc.Mtga.DuelScene;

public class NullMatchSceneStateProvider : IMatchSceneStateProvider
{
	public static readonly IMatchSceneStateProvider Default = new NullMatchSceneStateProvider();

	public ObservableValue<MatchSceneState> CurrentState => new ObservableValue<MatchSceneState>();
}
