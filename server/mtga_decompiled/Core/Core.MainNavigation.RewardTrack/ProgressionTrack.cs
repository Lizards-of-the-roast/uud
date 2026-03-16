using System;
using System.Collections.Generic;
using Wizards.Mtga.FrontDoorModels;

namespace Core.MainNavigation.RewardTrack;

public class ProgressionTrack
{
	public string Name { get; set; }

	public Dictionary<int, OrbSlot> OrbSlotById { get; private set; } = new Dictionary<int, OrbSlot>();

	public List<ProgressionTrackLevel> Levels { get; private set; } = new List<ProgressionTrackLevel>();

	public List<TrackRewardInfo> ExtendedLevelRewardTiers { get; set; } = new List<TrackRewardInfo>();

	public DateTime ExpirationTime { get; set; }

	public DateTime ExpirationWarningTime { get; set; }

	public Queue<RewardWebChange> RewardWebChangeQueue { get; private set; } = new Queue<RewardWebChange>();

	public Queue<OrbInventoryChange> OrbInventoryChangeQueue { get; set; } = new Queue<OrbInventoryChange>();

	public int CurrentLevel { get; set; }

	public int CurrentLevelIndex { get; set; }

	public int MaxLevelIndex { get; set; }

	public HashSet<int> RewardTiers { get; set; }

	public int TierCount { get; set; }

	public int NumberOrbs { get; set; }

	public bool PlayerHitMaxLevel { get; set; }

	public bool Enabled { get; set; }

	public bool RewardWebStatus { get; set; }

	public int NumberOfExtendedLevels { get; set; }

	public string PrizeWallId { get; set; }
}
