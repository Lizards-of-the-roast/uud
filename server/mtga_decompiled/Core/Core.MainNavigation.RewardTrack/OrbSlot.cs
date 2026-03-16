namespace Core.MainNavigation.RewardTrack;

public class OrbSlot
{
	public enum OrbState
	{
		Unavailable,
		Available,
		Unlocked
	}

	public ClientRewardWebNodeInfo serverRewardNode;

	public OrbState currentState;
}
