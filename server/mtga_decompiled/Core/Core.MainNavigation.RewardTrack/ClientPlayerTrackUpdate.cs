using Wizards.Mtga.FrontDoorModels;

namespace Core.MainNavigation.RewardTrack;

public class ClientPlayerTrackUpdate
{
	public string trackName;

	public string trackTier;

	public PlayerTrackDiff trackDiff;

	public ClientRewardWebDiff rewardWebDiff;

	public OrbCountDiff orbCountDiff;

	public ClientPlayerTrackUpdate(PlayerTrackUpdate azureModel)
	{
		trackName = azureModel.trackName;
		trackTier = azureModel.trackTier;
		if (azureModel?.trackDiff != null)
		{
			trackDiff = new PlayerTrackDiff(azureModel.trackDiff);
		}
		if (azureModel?.rewardWebDiff != null)
		{
			rewardWebDiff = new ClientRewardWebDiff(azureModel.rewardWebDiff);
		}
		orbCountDiff = new OrbCountDiff
		{
			currentOrbCount = azureModel.orbCountDiff.currentOrbCount,
			oldOrbCount = azureModel.orbCountDiff.oldOrbCount
		};
	}

	public ClientPlayerTrackUpdate(string trackName, ClientRewardWebDiff webDiff, OrbCountDiff orbDiff)
	{
		this.trackName = trackName;
		rewardWebDiff = webDiff;
		orbCountDiff = orbDiff;
	}
}
