namespace Wotc.Mtga.DuelScene;

public interface IDuelSceneStateController
{
	void SetState(DuelSceneState state);

	void SetState(bool? lockSceneTransitions = null, bool? firstGameStatePlaybackComplete = null, bool? gameComplete = null);
}
