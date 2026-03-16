using System.Collections.Generic;

namespace Core.MainNavigation.RewardTrack;

public class OrbInventoryChange
{
	public string trackName;

	public int oldGenericOrbAmount;

	public int currentGenericOrbAmount;

	public List<int> RecentlyUnlockedColors;
}
