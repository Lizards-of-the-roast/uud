public class ProgressionTrackPageContext
{
	public string TrackName;

	public bool PlayIntro;

	public NavContentType BackTarget;

	public NavContentType PreviousSceneForBI;

	public ProgressionTrackPageContext(string trackName, NavContentType backTarget, NavContentType previousSceneForBI, bool playIntro = false)
	{
		TrackName = trackName;
		BackTarget = backTarget;
		PreviousSceneForBI = previousSceneForBI;
		PlayIntro = playIntro;
	}
}
