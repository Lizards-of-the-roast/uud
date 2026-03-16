namespace Core.MainNavigation.RewardTrack;

public class RewardWebChange
{
	public enum StateTransition
	{
		BecomeAvailable,
		BecomeUnlocked,
		BecomeUnlocked_ColorOrb
	}

	public string trackName;

	public int ID;

	public StateTransition Transition;
}
