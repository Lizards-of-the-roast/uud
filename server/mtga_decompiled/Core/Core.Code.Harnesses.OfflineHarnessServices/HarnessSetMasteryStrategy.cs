using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.MainNavigation.RewardTrack;
using Wizards.Arena.Promises;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;

namespace Core.Code.Harnesses.OfflineHarnessServices;

public class HarnessSetMasteryStrategy : ISetMasteryStrategy
{
	public string CurrentBpName { get; }

	public string CurrentPrizeWallId { get; }

	public string PreviousBpName { get; }

	public string PreviousPrizeWallId { get; }

	public bool FailedInitializing { get; }

	public Queue<LevelChange> CachedBpLevelChanges { get; set; }

	public List<ClientInventoryUpdateReportItem> TrackDeltas { get; }

	public event Action<ClientInventoryUpdateReportItem> OnInventoryUpdate;

	public event Action<ClientPlayerTrackUpdate> ProcessTrackUpdate;

	public event Action<Queue<LevelChange>> OnCurrentBpProgressUpdate;

	public event Action<ClientRewardTierUpdate> OnCurrentBpRewardTierUpdate;

	public void OnTrackRewardTierUpdateReceived(RewardTierUpdate update)
	{
		throw new NotImplementedException();
	}

	public Task Refresh()
	{
		throw new NotImplementedException();
	}

	public ProgressionTrack GetTrack(string trackName)
	{
		return new ProgressionTrack
		{
			Name = "Test Track",
			ExtendedLevelRewardTiers = null,
			ExpirationTime = DateTime.Now + TimeSpan.FromDays(10.0),
			ExpirationWarningTime = default(DateTime),
			OrbInventoryChangeQueue = null,
			CurrentLevel = 0,
			CurrentLevelIndex = 0,
			MaxLevelIndex = 0,
			RewardTiers = null,
			TierCount = 0,
			NumberOrbs = 0,
			PlayerHitMaxLevel = false,
			Enabled = false,
			RewardWebStatus = false,
			NumberOfExtendedLevels = 0
		};
	}

	public MasteryPassError GetError<T>(Promise<T> requestHandle)
	{
		throw new NotImplementedException();
	}

	public RewardDisplayData GetRewardDisplayDataForTrackReward(string trackName, ClientTrackRewardInfo trackReward, CardDatabase cardDatabase, CardMaterialBuilder cardMaterialBuilder, bool replaceXp)
	{
		throw new NotImplementedException();
	}

	public Promise<ClientPlayerTrackUpdate> SpendOrbsOnNodes(string trackName, IEnumerable<int> nodeId)
	{
		throw new NotImplementedException();
	}

	public void Destroy()
	{
		throw new NotImplementedException();
	}
}
