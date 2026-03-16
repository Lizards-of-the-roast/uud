namespace Wotc.Mtga.DuelScene;

public class NullDuelSceneStateProvider : IDuelSceneStateProvider
{
	public static IDuelSceneStateProvider Default = new NullDuelSceneStateProvider();

	public ObservableValue<DuelSceneState> CurrentState => null;
}
