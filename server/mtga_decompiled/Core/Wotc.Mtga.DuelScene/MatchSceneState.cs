namespace Wotc.Mtga.DuelScene;

public readonly struct MatchSceneState
{
	public readonly MatchSceneManager.SubScene SubScene;

	public readonly bool SubSceneTransitioning;

	public readonly bool LockSceneTransitions;

	public MatchSceneState(MatchSceneManager.SubScene subScene, bool subSceneTransitioning, bool lockSceneTransitions)
	{
		SubScene = subScene;
		SubSceneTransitioning = subSceneTransitioning;
		LockSceneTransitions = lockSceneTransitions;
	}

	public MatchSceneState(MatchSceneState other, MatchSceneManager.SubScene? subScene = null, bool? sceneTransitioning = null, bool? lockSceneTransitions = null)
		: this(subScene ?? other.SubScene, sceneTransitioning ?? other.SubSceneTransitioning, lockSceneTransitions ?? other.LockSceneTransitions)
	{
	}
}
