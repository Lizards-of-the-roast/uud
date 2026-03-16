namespace Core.Meta.MainNavigation.RewardTrack;

public class RewardTrackScreenWrapperCompassGuide : WrapperCompassGuide
{
	public ProgressionTrackPageContext TrackPageContext { get; }

	public RewardTrackScreenWrapperCompassGuide(ProgressionTrackPageContext trackPageContext)
	{
		TrackPageContext = trackPageContext;
	}
}
