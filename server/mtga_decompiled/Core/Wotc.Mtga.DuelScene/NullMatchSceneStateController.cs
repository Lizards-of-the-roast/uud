namespace Wotc.Mtga.DuelScene;

public class NullMatchSceneStateController : IMatchSceneStateController
{
	public static readonly IMatchSceneStateController Default = new NullMatchSceneStateController();

	public void SetState(MatchSceneState state)
	{
	}
}
