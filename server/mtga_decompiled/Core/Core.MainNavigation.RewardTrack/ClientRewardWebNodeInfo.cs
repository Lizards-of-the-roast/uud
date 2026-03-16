using System.Collections.Generic;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Unification.Models.Player;

namespace Core.MainNavigation.RewardTrack;

public class ClientRewardWebNodeInfo
{
	public int id;

	public string unlockQuestMetric;

	public int unlockMetricCount;

	public ClientChestDescription chest;

	public UpgradePacket upgradePacket;

	public List<int> childIds;

	public ClientRewardWebNodeInfo()
	{
	}

	public ClientRewardWebNodeInfo(RewardWebNodeInfo info)
	{
		id = info.id;
		unlockQuestMetric = info.unlockQuestMetric;
		unlockMetricCount = info.unlockMetricCount;
		chest = info.chest;
		upgradePacket = info.upgradePacket;
		childIds = new List<int>();
		if (info.childIds == null)
		{
			return;
		}
		foreach (int childId in info.childIds)
		{
			childIds.Add(childId);
		}
	}
}
