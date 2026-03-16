using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wizards.Arena.Promises;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;

namespace Core.MainNavigation.RewardTrack;

public interface ISetMasteryStrategy
{
	string CurrentBpName { get; }

	string CurrentPrizeWallId { get; }

	string PreviousBpName { get; }

	string PreviousPrizeWallId { get; }

	bool FailedInitializing { get; }

	Queue<LevelChange> CachedBpLevelChanges { get; set; }

	List<ClientInventoryUpdateReportItem> TrackDeltas { get; }

	event Action<ClientInventoryUpdateReportItem> OnInventoryUpdate;

	event Action<ClientPlayerTrackUpdate> ProcessTrackUpdate;

	event Action<Queue<LevelChange>> OnCurrentBpProgressUpdate;

	event Action<ClientRewardTierUpdate> OnCurrentBpRewardTierUpdate;

	void OnTrackRewardTierUpdateReceived(RewardTierUpdate update);

	Task Refresh();

	ProgressionTrack GetTrack(string trackName);

	MasteryPassError GetError<T>(Promise<T> requestHandle);

	RewardDisplayData GetRewardDisplayDataForTrackReward(string trackName, ClientTrackRewardInfo trackReward, CardDatabase cardDatabase, CardMaterialBuilder cardMaterialBuilder, bool replaceXp);

	Promise<ClientPlayerTrackUpdate> SpendOrbsOnNodes(string trackName, IEnumerable<int> nodeId);

	void Destroy();
}
