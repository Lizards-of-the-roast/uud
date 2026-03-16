namespace Wotc.Mtga.DuelScene;

public class NullDuelSceneStateController : IDuelSceneStateController
{
	public static readonly IDuelSceneStateController Default = new NullDuelSceneStateController();

	public void SetState(DuelSceneState state)
	{
	}

	public void SetState(bool? lockSceneTransitions = null, bool? firstGameStatePlaybackComplete = null, bool? gameComplete = null)
	{
	}
}
