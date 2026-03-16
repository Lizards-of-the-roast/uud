using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Assets.Core.Shared.Code;
using Core.Code.PrizeWall;
using Core.Meta.MainNavigation.Store;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Loc;

namespace Core.MainNavigation.RewardTrack;

public class SetMasteryDataProvider
{
	private readonly ISetMasteryStrategy _strategy;

	public string CurrentBpName => _strategy?.CurrentBpName;

	public string CurrentPrizeWallId => _strategy?.CurrentPrizeWallId;

	public string PreviousBpName => _strategy?.PreviousBpName;

	public string PreviousPrizeWallId => _strategy?.PreviousPrizeWallId;

	public bool FailedInitializing => _strategy?.FailedInitializing ?? true;

	public event Action<Queue<LevelChange>> OnCurrentBpProgressUpdate
	{
		add
		{
			_strategy.OnCurrentBpProgressUpdate += value;
		}
		remove
		{
			_strategy.OnCurrentBpProgressUpdate -= value;
		}
	}

	public event Action<ClientRewardTierUpdate> OnCurrentBpRewardTierUpdate
	{
		add
		{
			_strategy.OnCurrentBpRewardTierUpdate += value;
		}
		remove
		{
			_strategy.OnCurrentBpRewardTierUpdate -= value;
		}
	}

	public IEnumerator Refresh()
	{
		return _strategy.Refresh().AsCoroutine();
	}

	public static SetMasteryDataProvider Create()
	{
		return new SetMasteryDataProvider(Pantry.Get<ISetMasteryStrategy>(), Pantry.Get<InventoryManager>());
	}

	private SetMasteryDataProvider(ISetMasteryStrategy strategy, InventoryManager inventoryManager)
	{
		_strategy = strategy;
		_strategy.ProcessTrackUpdate += ProcessTrackUpdate;
		_strategy.OnInventoryUpdate += OnUpdateInventory;
	}

	public void OnDestroy()
	{
		_strategy.ProcessTrackUpdate -= ProcessTrackUpdate;
		_strategy.OnInventoryUpdate -= OnUpdateInventory;
		_strategy.Destroy();
	}

	public Promise<ClientPlayerTrackUpdate> SpendOrbsOnNodes(string trackName, IEnumerable<int> nodeIds)
	{
		return _strategy.SpendOrbsOnNodes(trackName, nodeIds);
	}

	public Dictionary<int, OrbSlot> GetOrbSlotMap(string trackName)
	{
		return _strategy.GetTrack(trackName)?.OrbSlotById ?? new Dictionary<int, OrbSlot>();
	}

	public Queue<LevelChange> GetAndRemoveCachedBpLevelChanges()
	{
		Queue<LevelChange> cachedBpLevelChanges = _strategy.CachedBpLevelChanges;
		_strategy.CachedBpLevelChanges = null;
		return cachedBpLevelChanges;
	}

	public Queue<RewardWebChange> GetRewardWebChangeQueue(string trackName)
	{
		return _strategy.GetTrack(trackName)?.RewardWebChangeQueue ?? new Queue<RewardWebChange>();
	}

	public List<OrbInventoryChange> PopRewardedOrbsToDisplay(string trackName)
	{
		List<OrbInventoryChange> list = new List<OrbInventoryChange>();
		if (trackName != CurrentBpName)
		{
			return list;
		}
		foreach (OrbInventoryChange item in GetOrbInventoryChangeQueueAndClear(trackName))
		{
			if (item.oldGenericOrbAmount < item.currentGenericOrbAmount)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public Queue<OrbInventoryChange> GetOrbInventoryChangeQueueAndClear(string trackName)
	{
		ProgressionTrack track = _strategy.GetTrack(trackName);
		Queue<OrbInventoryChange> queue = new Queue<OrbInventoryChange>();
		if (track != null)
		{
			foreach (OrbInventoryChange item in track.OrbInventoryChangeQueue)
			{
				queue.Enqueue(item);
			}
			track.OrbInventoryChangeQueue.Clear();
		}
		return queue;
	}

	public int GetCurrentLevelIndex(string trackName)
	{
		return GetCurrentLevel(trackName)?.Index ?? 1;
	}

	public int GetCurrentXpProgress(string trackName)
	{
		return GetCurrentLevel(trackName)?.EXPProgressIfIsCurrent ?? 0;
	}

	public bool PlayerHitPremiumRewardTier(string trackName)
	{
		return DoesTrackHaveTier(trackName, 1);
	}

	private int GetCurrentTier(string trackName)
	{
		if (_strategy.GetTrack(trackName)?.RewardTiers?.Contains(1) != true)
		{
			return 0;
		}
		return 1;
	}

	public CurrentProgressionSummary GetCurrentProgressionSummary(string trackName, CardDatabase cardDatabase, CardMaterialBuilder cardMaterialBuilder)
	{
		int currentTier = GetCurrentTier(trackName);
		ProgressionTrackLevel currentLevel = GetCurrentLevel(trackName);
		bool shouldTease = false;
		ClientTrackRewardInfo clientTrackRewardInfo = currentLevel.ServerRewardTiers.ElementAtOrDefault(currentTier);
		if (currentTier == 0 && clientTrackRewardInfo?.chest == null && clientTrackRewardInfo != null && clientTrackRewardInfo.OrbsAwarded == 0)
		{
			ClientTrackRewardInfo clientTrackRewardInfo2 = currentLevel.ServerRewardTiers.ElementAtOrDefault(1);
			if (clientTrackRewardInfo2 != null)
			{
				clientTrackRewardInfo = clientTrackRewardInfo2;
				shouldTease = true;
			}
		}
		RewardDisplayData rewardDisplayDataForTrackReward = _strategy.GetRewardDisplayDataForTrackReward(trackName, clientTrackRewardInfo, cardDatabase, cardMaterialBuilder, HasTrackExpired(trackName));
		MTGALocalizedString mTGALocalizedString = "EPP/Level/XP";
		mTGALocalizedString.Parameters = new Dictionary<string, string> { 
		{
			"count",
			currentLevel.EXPProgressIfIsCurrent + "/" + currentLevel.ServerLevel.xpToComplete
		} };
		return new CurrentProgressionSummary
		{
			ShouldTease = shouldTease,
			CurrentReward = rewardDisplayDataForTrackReward,
			Tier = currentTier,
			ProgressText = mTGALocalizedString,
			LevelInfo = currentLevel
		};
	}

	public bool DoesTrackHaveTier(string trackName, int tier)
	{
		return _strategy.GetTrack(trackName)?.RewardTiers?.Contains(tier) == true;
	}

	public void PurchaseTrackUpgrade(int levelsToBePurchased = 1)
	{
		bool flag = PlayerHitPremiumRewardTier(CurrentBpName);
		string storeSubType = (flag ? "LevelUpgrade" : "RewardTierUpgrade");
		ProgressionTrack track = _strategy.GetTrack(CurrentBpName);
		if (flag && track.PlayerHitMaxLevel)
		{
			UnityEngine.Debug.LogError("Attempted purchase " + storeSubType + " for a completed track.");
			return;
		}
		if (WrapperController.Instance?.Store?.ProgressionTracks?.FirstOrDefault((StoreItem storeItem) => storeItem.SubType == storeSubType) == null)
		{
			UnityEngine.Debug.LogError("Attempted purchase of missing product of type " + storeSubType + ".");
			return;
		}
		_strategy.TrackDeltas.Clear();
		SceneLoader.GetSceneLoader().GetBattlePassPurchaseConfirmation().Activate(this, storeSubType == "LevelUpgrade", levelsToBePurchased);
	}

	public ProgressionTrackLevel GetCurrentLevel(string trackName)
	{
		ProgressionTrack track = _strategy.GetTrack(trackName);
		return Enumerable.ElementAtOrDefault(index: track?.CurrentLevelIndex ?? 0, source: track?.Levels?);
	}

	public bool IsEnabled(string trackName)
	{
		return _strategy.GetTrack(trackName)?.Enabled ?? false;
	}

	public DateTime GetTrackExpirationTime(string trackName)
	{
		return _strategy.GetTrack(trackName)?.ExpirationTime ?? default(DateTime);
	}

	public DateTime GetTrackExpirationWarningTime(string trackName)
	{
		return _strategy.GetTrack(trackName)?.ExpirationWarningTime ?? default(DateTime);
	}

	public bool HasTrackExpired(string trackName)
	{
		ProgressionTrack track = _strategy.GetTrack(trackName);
		if (!_strategy.FailedInitializing && track != null)
		{
			if (track.ExpirationTime != default(DateTime))
			{
				return track.ExpirationTime <= ServerGameTime.GameTime;
			}
			return false;
		}
		return true;
	}

	public int GetOrbCount(string trackName)
	{
		return _strategy.GetTrack(trackName)?.NumberOrbs ?? 0;
	}

	public List<ProgressionTrackLevel> GetLevelTracks(string trackName)
	{
		return _strategy.GetTrack(trackName)?.Levels ?? new List<ProgressionTrackLevel>();
	}

	public bool isWebEnabled(string trackName)
	{
		return _strategy.GetTrack(trackName)?.RewardWebStatus ?? false;
	}

	public string GetPrizeWallForTrack(string trackName)
	{
		return _strategy.GetTrack(trackName)?.PrizeWallId;
	}

	public bool PlayerHitMaxLevel(string trackName)
	{
		return _strategy.GetTrack(trackName)?.PlayerHitMaxLevel ?? false;
	}

	public int GetMaxNonRepeatLevel(string trackname)
	{
		return GetLevelTracks(trackname).Count((ProgressionTrackLevel o) => !o.IsRepeatable);
	}

	public List<RewardDisplayData> GetRewardDisplayDataForLevel(string trackName, ProgressionTrackLevel level, CardDatabase cardDatabase, CardMaterialBuilder cardMaterialBuilder, bool replaceXp = false)
	{
		List<RewardDisplayData> list = new List<RewardDisplayData>();
		foreach (ClientTrackRewardInfo serverRewardTier in level.ServerRewardTiers)
		{
			RewardDisplayData rewardDisplayDataForTrackReward = _strategy.GetRewardDisplayDataForTrackReward(trackName, serverRewardTier, cardDatabase, cardMaterialBuilder, replaceXp);
			list.Add(rewardDisplayDataForTrackReward);
		}
		return list;
	}

	public MTGALocalizedString GetTrackTitle(string trackName)
	{
		string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/BattlePass/" + trackName);
		if (localizedText != null)
		{
			return new MTGALocalizedString
			{
				Key = "MainNav/BattlePass/SetXMastery",
				Parameters = new Dictionary<string, string> { { "setName", localizedText } }
			};
		}
		return "MainNav/BattlePass/SetMastery";
	}

	public void ProcessTrackUpdate(ClientPlayerTrackUpdate trackUpdate)
	{
		if (trackUpdate == null)
		{
			UnityEngine.Debug.LogError("Invalid track update");
			return;
		}
		ProgressionTrack track = _strategy.GetTrack(trackUpdate.trackName);
		if (track == null)
		{
			UnityEngine.Debug.LogError("Processing track update for a track we don't have a definition for: " + trackUpdate.trackName);
			return;
		}
		ClientRewardWebDiff rewardWebDiff = trackUpdate.rewardWebDiff;
		OrbInventoryChange orbInventoryChange = new OrbInventoryChange
		{
			RecentlyUnlockedColors = new List<int>()
		};
		OrbCountDiff orbCountDiff = trackUpdate.orbCountDiff;
		bool flag = false;
		if (orbCountDiff != null)
		{
			track.NumberOrbs = orbCountDiff.currentOrbCount;
			orbInventoryChange.oldGenericOrbAmount = orbCountDiff.oldOrbCount;
			orbInventoryChange.currentGenericOrbAmount = orbCountDiff.currentOrbCount;
			flag = orbCountDiff.oldOrbCount != orbCountDiff.currentOrbCount;
		}
		if (rewardWebDiff != null)
		{
			foreach (int item in rewardWebDiff.currentUnlockedNodes.Except(rewardWebDiff.oldUnlockedNodes))
			{
				if (track.OrbSlotById.TryGetValue(item, out var value))
				{
					value.currentState = OrbSlot.OrbState.Unlocked;
				}
				track.RewardWebChangeQueue.Enqueue(new RewardWebChange
				{
					ID = item,
					Transition = RewardWebChange.StateTransition.BecomeUnlocked
				});
			}
			foreach (int item2 in rewardWebDiff.currentAvailableNodes.Except(rewardWebDiff.oldAvailableNodes))
			{
				if (track.OrbSlotById.TryGetValue(item2, out var value2))
				{
					value2.currentState = OrbSlot.OrbState.Available;
				}
				track.RewardWebChangeQueue.Enqueue(new RewardWebChange
				{
					ID = item2,
					Transition = RewardWebChange.StateTransition.BecomeAvailable
				});
			}
		}
		if (flag)
		{
			track.OrbInventoryChangeQueue.Enqueue(orbInventoryChange);
		}
		if (WrapperController.Instance != null && WrapperController.Instance.NavBarController != null)
		{
			WrapperController.Instance.NavBarController.RefreshMasteryDisplay();
		}
	}

	private void OnUpdateInventory(ClientInventoryUpdateReportItem update)
	{
		PAPA.StartGlobalCoroutine(DisplayRewards(update));
	}

	public MasteryPassError GetError<T>(Promise<T> requestHandle)
	{
		return _strategy.GetError(requestHandle);
	}

	private IEnumerator DisplayRewards(ClientInventoryUpdateReportItem update)
	{
		Stopwatch timeout = new Stopwatch();
		timeout.Start();
		yield return new WaitUntil(() => _strategy.TrackDeltas.Count > 0 || timeout.ElapsedMilliseconds > 2000);
		update.xpGained = 0;
		ContentControllerRewards rewardPanel = SceneLoader.GetSceneLoader()?.GetRewardsContentController();
		if (rewardPanel != null)
		{
			List<OrbInventoryChange> orbChanges = PopRewardedOrbsToDisplay(CurrentBpName);
			yield return rewardPanel.AddRewardedOrbsCoroutine(orbChanges);
			yield return rewardPanel.AddAndDisplayRewardsCoroutine(update, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
		}
	}

	public void OnTrackRewardTierUpdateReceived_AWS()
	{
		PAPA.StartGlobalCoroutine(RefreshThenUpdateTierReward_AWS());
	}

	private IEnumerator RefreshThenUpdateTierReward_AWS()
	{
		if (_strategy is AwsSetMasteryStrategy)
		{
			yield return Refresh();
			_strategy.OnTrackRewardTierUpdateReceived(null);
		}
	}

	public string PrizeWallHash()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(CurrentPrizeWallId);
		stringBuilder.Append(GetOrbCount(CurrentBpName));
		stringBuilder.Append(PreviousPrizeWallId);
		stringBuilder.Append(GetOrbCount(PreviousPrizeWallId));
		StringBuilder stringBuilder2 = new StringBuilder();
		using (SHA256 sHA = SHA256.Create())
		{
			byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
			foreach (byte b in array)
			{
				stringBuilder2.Append(b.ToString("x2"));
			}
		}
		return stringBuilder2.ToString();
	}

	public bool ShouldShowSetMasteryHeatPip(IAccountClient accountClient)
	{
		AccountInformation accountInformation = accountClient.AccountInformation;
		bool flag;
		if (accountInformation != null)
		{
			string text = PrizeWallHash();
			string lastSetMasteryNavViewed = MDNPlayerPrefs.GetLastSetMasteryNavViewed(accountInformation.PersonaID);
			flag = text == lastSetMasteryNavViewed;
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			return CanSpendOrbs();
		}
		return false;
	}

	public bool ShouldShowSetMasterOrbSpendHeat(IAccountClient accountClient)
	{
		AccountInformation accountInformation = accountClient.AccountInformation;
		bool flag;
		if (accountInformation != null)
		{
			string text = PrizeWallHash();
			string lastSetMasteryOrbSpendViewed = MDNPlayerPrefs.GetLastSetMasteryOrbSpendViewed(accountInformation.PersonaID);
			flag = text == lastSetMasteryOrbSpendViewed;
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			return CanSpendOrbs();
		}
		return false;
	}

	private bool CanSpendOrbs()
	{
		if (!CanSpendOrbsInBattlePass(CurrentBpName, CurrentPrizeWallId))
		{
			return CanSpendOrbsInBattlePass(PreviousBpName, PreviousPrizeWallId);
		}
		return true;
	}

	private bool CanSpendOrbsInBattlePass(string passName, string prizeWallId)
	{
		int orbCount = GetOrbCount(passName);
		if (orbCount == 0)
		{
			return false;
		}
		return PrizeWallDataProvider.PrizeWallHasAffordableItems(Pantry.Get<StoreManager>(), prizeWallId, orbCount);
	}
}
