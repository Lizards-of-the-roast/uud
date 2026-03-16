using Wizards.Mtga.FrontDoorModels;

namespace Core.MainNavigation.RewardTrack;

public class ClientRewardTierUpdate
{
	public int[] tiersAdded;

	public OrbCountDiff orbCountDiff;

	public ClientInventoryUpdateReportItem inventoryDelta;
}
