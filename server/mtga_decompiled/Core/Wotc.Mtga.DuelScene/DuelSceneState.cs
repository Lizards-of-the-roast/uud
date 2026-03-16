namespace Wotc.Mtga.DuelScene;

public readonly struct DuelSceneState
{
	public readonly MatchSceneManager.SubScene MatchScene_SubScene;

	public readonly bool MatchScene_SubSceneTransitioning;

	public readonly bool MatchScene_LockSceneTransitions;

	public readonly bool FirstGameStatePlaybackComplete;

	public readonly bool GameComplete;

	public bool AllowInput
	{
		get
		{
			if (MatchScene_SubScene == MatchSceneManager.SubScene.DuelScene && !MatchScene_SubSceneTransitioning)
			{
				return FirstGameStatePlaybackComplete;
			}
			return false;
		}
	}

	private DuelSceneState(MatchSceneManager.SubScene subScene, bool subSceneTransitioning, bool lockSceneTransitions, bool firstGameStatePlaybackComplete, bool gameComplete)
	{
		MatchScene_SubScene = subScene;
		MatchScene_SubSceneTransitioning = subSceneTransitioning;
		MatchScene_LockSceneTransitions = lockSceneTransitions;
		FirstGameStatePlaybackComplete = firstGameStatePlaybackComplete;
		GameComplete = gameComplete;
	}

	public DuelSceneState(DuelSceneState other, bool? lockSceneTransitions = null, bool? firstGameStatePlaybackComplete = null, bool? gameComplete = null)
		: this(other.MatchScene_SubScene, other.MatchScene_SubSceneTransitioning, lockSceneTransitions ?? other.MatchScene_LockSceneTransitions, firstGameStatePlaybackComplete ?? other.FirstGameStatePlaybackComplete, gameComplete ?? other.GameComplete)
	{
	}

	public DuelSceneState(MatchSceneState matchSceneState, DuelSceneState duelSceneState)
		: this(matchSceneState.SubScene, matchSceneState.SubSceneTransitioning, matchSceneState.LockSceneTransitions, duelSceneState.FirstGameStatePlaybackComplete, duelSceneState.GameComplete)
	{
	}
}
