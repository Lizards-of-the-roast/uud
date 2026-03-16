namespace Wotc.Mtga.DuelScene;

public interface IMatchSceneStateManager : IMatchSceneStateProvider, IMatchSceneStateController
{
	void SetState(MatchSceneManager.SubScene? subScene = null, bool? subSceneTransitioning = null, bool? lockSceneTransitions = null)
	{
		SetState(new MatchSceneState(CurrentState.Value, subScene, subSceneTransitioning, lockSceneTransitions));
	}
}
