using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.Arena.Enums.Store;
using Wizards.Arena.Promises;
using Wizards.Models;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Unification.Models;
using Wizards.Unification.Models.Graph;
using Wizards.Unification.Models.Player;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.MainNavigation.RewardTrack;

public class AwsSetMasteryStrategy : ISetMasteryStrategy
{
	private readonly InventoryManager _inventoryManager;

	private readonly CampaignGraphManager _graphManager;

	private ProgressionTrack _currentBpTrack;

	private ProgressionTrack _previousBpTrack;

	private bool _isRefreshing;

	public bool FailedInitializing { get; private set; }

	public Queue<LevelChange> CachedBpLevelChanges { get; set; }

	public List<ClientInventoryUpdateReportItem> TrackDeltas { get; private set; } = new List<ClientInventoryUpdateReportItem>();

	public string CurrentBpName => _currentBpTrack?.Name;

	public string CurrentPrizeWallId => _currentBpTrack?.PrizeWallId;

	public string PreviousBpName => _previousBpTrack?.Name;

	public string PreviousPrizeWallId => _previousBpTrack?.PrizeWallId;

	public event Action<ClientInventoryUpdateReportItem> OnInventoryUpdate;

	public event Action<ClientPlayerTrackUpdate> ProcessTrackUpdate;

	public event Action<Queue<LevelChange>> OnCurrentBpProgressUpdate;

	public event Action<ClientRewardTierUpdate> OnCurrentBpRewardTierUpdate;

	public AwsSetMasteryStrategy(InventoryManager inventoryManager, CampaignGraphManager campaignGraphManager)
	{
		_inventoryManager = inventoryManager;
		_graphManager = campaignGraphManager;
		_inventoryManager.Subscribe(InventoryUpdateSource.CampaignGraphTieredRewardNode, OnLevelUpInventoryUpdate, null, publish: false);
		_inventoryManager.Subscribe(InventoryUpdateSource.BattlePassLevelUp, OnCampaignGraphPurchaseNodeUpdate, null, publish: false);
		_inventoryManager.Subscribe(InventoryUpdateSource.CampaignGraphPurchaseNode, OnCampaignGraphPurchaseNodeUpdate, null, publish: false);
		_inventoryManager.Subscribe(InventoryUpdateSource.CustomerSupportGrant, OnInventoryWithXpUpdate, null, publish: false);
		_inventoryManager.Subscribe(InventoryUpdateSource.MercantileChestPurchase, OnInventoryWithXpUpdate, null, publish: false);
		_inventoryManager.Subscribe(InventoryUpdateSource.DailyWins, OnInventoryWithXpUpdate, null, publish: false);
		_inventoryManager.Subscribe(InventoryUpdateSource.WeeklyWins, OnInventoryWithXpUpdate, null, publish: false);
	}

	public void Destroy()
	{
		_inventoryManager.UnSubscribe(InventoryUpdateSource.CampaignGraphTieredRewardNode, OnLevelUpInventoryUpdate);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.BattlePassLevelUp, OnCampaignGraphPurchaseNodeUpdate);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.CampaignGraphPurchaseNode, OnCampaignGraphPurchaseNodeUpdate);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.CustomerSupportGrant, OnInventoryWithXpUpdate);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.MercantileChestPurchase, OnInventoryWithXpUpdate);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.DailyWins, OnInventoryWithXpUpdate);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.WeeklyWins, OnInventoryWithXpUpdate);
	}

	public async Task Refresh()
	{
		await new Until(() => !_isRefreshing).WithTimeout(TimeSpan.FromSeconds(20.0)).AsTask;
		if (_isRefreshing)
		{
			Debug.LogWarning("Attempting to refresh mastery pass that's already being refreshed");
			return;
		}
		_isRefreshing = true;
		await new Until(() => _graphManager.Initialized).WithTimeout(TimeSpan.FromSeconds(20.0)).AsTask;
		if (!_graphManager.Initialized)
		{
			Debug.LogError("[AwsProgressionManager] Node Graph Manager failed to initialize!");
			FailedInitializing = true;
			_isRefreshing = false;
			return;
		}
		List<ClientGraphDefinition> list = (from def in (await _graphManager.GetDefinitions()).Values
			where def.Type == Wizards.Arena.Enums.CampaignGraph.CampaignGraphType.SetMastery
			orderby (!(def.EndTime == DateTime.MinValue)) ? def.EndTime : DateTime.MaxValue descending, def.StartTime descending
			select def).ToList();
		if (list.Count == 0)
		{
			Debug.LogError("[AwsProgressionManager] Graph Definitions for SetMastery is null or empty");
			FailedInitializing = true;
			_isRefreshing = false;
			return;
		}
		ClientGraphDefinition currTrackDef = list[0];
		ClientGraphDefinition prevTrackDef = ((list.Count > 1) ? list[1] : null);
		_graphManager.Update(currTrackDef);
		await new Until(() => _graphManager.Ready).AsTask;
		Dictionary<string, ClientNodeState> dictionary = _graphManager.GetState(currTrackDef).NodeStates;
		if (dictionary == null)
		{
			Debug.LogError("[NPE] Node Graph Manager returned null state!");
			dictionary = new Dictionary<string, ClientNodeState>();
		}
		bool flag = ProcessRewardUpgradeIfNeeded(currTrackDef, dictionary, currTrackDef?.Configuration?.SetMasteryConfiguration?.RewardTierNodeId);
		string text = currTrackDef?.Configuration?.SetMasteryConfiguration?.RenewalTierNodeId;
		if (text != null)
		{
			flag |= ProcessRewardUpgradeIfNeeded(currTrackDef, dictionary, text);
		}
		if (flag)
		{
			await new Until(() => _graphManager.Ready).AsTask;
			dictionary = _graphManager.GetState(currTrackDef).NodeStates;
		}
		if (ProcessTieredRewardsIfNeeded(currTrackDef, dictionary))
		{
			await new Until(() => _graphManager.Ready).AsTask;
			dictionary = _graphManager.GetState(currTrackDef).NodeStates;
			_currentBpTrack = InitializeProgression(_currentBpTrack, currTrackDef, dictionary);
		}
		else
		{
			_currentBpTrack = InitializeProgression(_currentBpTrack, currTrackDef, dictionary);
		}
		if (prevTrackDef != null && GetOrbCountForMasteryGraph(prevTrackDef) > 0)
		{
			_graphManager.Update(prevTrackDef);
			await new Until(() => _graphManager.Ready).AsTask;
			Dictionary<string, ClientNodeState> nodeStates = _graphManager.GetState(prevTrackDef).NodeStates;
			if (ProcessTieredRewardsIfNeeded(prevTrackDef, nodeStates))
			{
				await new Until(() => _graphManager.Ready).AsTask;
				nodeStates = _graphManager.GetState(prevTrackDef).NodeStates;
				_previousBpTrack = InitializeProgression(_previousBpTrack, prevTrackDef, nodeStates);
			}
			else
			{
				_previousBpTrack = InitializeProgression(_previousBpTrack, prevTrackDef, nodeStates);
			}
			await new Until(() => _graphManager.Ready).AsTask;
		}
		else
		{
			_previousBpTrack = null;
		}
		WrapperController.Instance?.NavBarController?.RefreshMasteryDisplay();
		_isRefreshing = false;
	}

	public ProgressionTrack GetTrack(string trackName)
	{
		if (string.IsNullOrEmpty(trackName))
		{
			return null;
		}
		if (CurrentBpName == trackName)
		{
			return _currentBpTrack;
		}
		if (PreviousBpName == trackName)
		{
			return _previousBpTrack;
		}
		return null;
	}

	public RewardDisplayData GetRewardDisplayDataForTrackReward(string trackName, ClientTrackRewardInfo trackReward, CardDatabase cardDatabase, CardMaterialBuilder cardMaterialBuilder, bool replaceXp)
	{
		return TempRewardTranslation.ChestDescriptionToDisplayData(trackReward?.chest, cardDatabase.CardDataProvider, cardMaterialBuilder, replaceXp);
	}

	public Promise<ClientPlayerTrackUpdate> SpendOrbsOnNodes(string trackName, IEnumerable<int> nodeIds)
	{
		return GetTrackGraph(trackName).AsPromise().Then(delegate(Promise<ClientGraphDefinition> p)
		{
			ClientGraphDefinition result = p.Result;
			List<string> nodeIds2;
			try
			{
				Dictionary<int, string> invertedOrbMappings = result.Configuration.SetMasteryConfiguration.OrbMappings.ToDictionary((KeyValuePair<string, int> _) => _.Value, (KeyValuePair<string, int> _) => _.Key);
				nodeIds2 = (from nodeId in nodeIds
					select (!invertedOrbMappings.TryGetValue(nodeId, out var value)) ? null : value into _
					where _ != null
					select _).ToList();
			}
			catch
			{
				SimpleLog.LogError("SpendOrbOnNode: Could not invert OrbMappings for orb track " + trackName);
				nodeIds2 = new List<string>();
			}
			return spendOrbOnNode(trackName, result, nodeIds2);
		});
	}

	public MasteryPassError GetError<T>(Promise<T> requestHandle)
	{
		if (!requestHandle.Successful)
		{
			return MasteryPassError.Other;
		}
		return MasteryPassError.None;
	}

	private async Task<ClientGraphDefinition> GetTrackGraph(string trackName)
	{
		(await _graphManager.GetDefinitions()).TryGetValue(trackName, out var value);
		return value;
	}

	private Promise<ClientPlayerTrackUpdate> spendOrbOnNode(string trackName, ClientGraphDefinition trackGraph, List<string> nodeIds)
	{
		ProgressionTrack track = GetTrack(trackName);
		if (trackGraph == null)
		{
			throw new Exception("No track definition for " + trackName + " while spending orb.");
		}
		List<ClientNodeDefinition> list = new List<ClientNodeDefinition>();
		foreach (string nodeId in nodeIds)
		{
			if (!trackGraph.Nodes.TryGetValue(nodeId, out var value))
			{
				throw new Exception("No node definition for " + nodeId + " found in track " + trackName + ".");
			}
			list.Add(value);
		}
		Dictionary<string, ClientNodeState> graphState = _graphManager.GetState(trackGraph).NodeStates;
		ClientRewardWebDiff diff = new ClientRewardWebDiff(trackGraph, graphState);
		int oldOrbCount = track.NumberOrbs;
		PurchasePayload purchasePayload = new PurchasePayload
		{
			Currency = Wizards.Arena.Enums.Store.PurchaseCurrency.CustomToken,
			CustomTokenId = getOrbCurrencyKey(trackGraph)
		};
		return _graphManager.PurchaseNodes(trackGraph, list, purchasePayload).Convert(delegate
		{
			diff.ConfigureCurrent(trackGraph, graphState);
			string orbCurrencyKey = getOrbCurrencyKey(trackGraph);
			int currentOrbCount = oldOrbCount - 1;
			if (_inventoryManager.Inventory.CustomTokens.TryGetValue(orbCurrencyKey, out var value2))
			{
				currentOrbCount = value2;
			}
			return new ClientPlayerTrackUpdate(trackName, diff, new OrbCountDiff
			{
				oldOrbCount = oldOrbCount,
				currentOrbCount = currentOrbCount
			});
		});
	}

	private bool ProcessRewardUpgradeIfNeeded(ClientGraphDefinition masteryGraph, Dictionary<string, ClientNodeState> graphState, string rewardTierNodeId)
	{
		if (string.IsNullOrEmpty(rewardTierNodeId))
		{
			Debug.LogError("AwsSetMasteryStrategy.Invalid Mastery Graph Configuration.");
			return false;
		}
		if (!masteryGraph.Nodes.TryGetValue(rewardTierNodeId, out var value))
		{
			Debug.LogError("AwsSetMasteryStrategy.Unable to find " + rewardTierNodeId + " in graph nodes");
			return false;
		}
		if (!graphState.TryGetValue(rewardTierNodeId, out var _))
		{
			Debug.LogError("AwsSetMasteryStrategy.Unable to find " + rewardTierNodeId + " in graphState");
			return false;
		}
		bool result = false;
		foreach (DTO_NodePurchaseOption purchaseOption in value.Configuration.PurchaseNodeConfig.PurchaseOptions)
		{
			if (purchaseOption.Currency == Wizards.Arena.Enums.Store.PurchaseCurrency.CustomToken && _inventoryManager.Inventory.CustomTokens.TryGetValue(purchaseOption.CustomTokenId, out var value3) && value3 >= purchaseOption.Price)
			{
				PurchasePayload purchasePayload = new PurchasePayload
				{
					Currency = Wizards.Arena.Enums.Store.PurchaseCurrency.CustomToken,
					CustomTokenId = purchaseOption.CustomTokenId
				};
				_graphManager.PurchaseNode(masteryGraph, value, purchasePayload);
				result = true;
			}
		}
		return result;
	}

	private bool ProcessTieredRewardsIfNeeded(ClientGraphDefinition masteryGraph, Dictionary<string, ClientNodeState> graphState)
	{
		Dictionary<string, ClientNodeDefinition> nodes = masteryGraph.Nodes;
		List<string> list = new List<string>();
		foreach (var (text2, clientNodeState2) in graphState)
		{
			if (nodes[text2].Type == Wizards.Arena.Enums.CampaignGraph.NodeType.TieredReward && clientNodeState2.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
			{
				list.Add(text2);
			}
		}
		if (list.Count > 0)
		{
			_graphManager.ProcessNodes(masteryGraph, list);
			return true;
		}
		return false;
	}

	private static HashSet<int> GetRewardTiers(ClientGraphDefinition masteryGraphDef, IReadOnlyDictionary<string, ClientNodeState> graphState)
	{
		HashSet<int> rewardTiers = new HashSet<int> { 0 };
		ClientSetMasteryGraphConfiguration obj = masteryGraphDef.Configuration?.SetMasteryConfiguration;
		AddRewardTierIfCompleted(obj?.RewardTierNodeId, 1);
		AddRewardTierIfCompleted(obj?.RenewalTierNodeId, 2);
		return rewardTiers;
		void AddRewardTierIfCompleted(string tierName, int tierNum)
		{
			if (!string.IsNullOrEmpty(tierName) && graphState.TryGetValue(tierName, out var value) && value != null && value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed)
			{
				rewardTiers.Add(tierNum);
			}
		}
	}

	private static string getOrbCurrencyKey(ClientGraphDefinition masteryGraph)
	{
		if (masteryGraph == null)
		{
			Debug.LogError("[AwsProgressionManager] Unable to get Custom Orb Token Key, no Graph Meta data!");
			return null;
		}
		if (string.IsNullOrEmpty(masteryGraph.Configuration?.SetMasteryConfiguration?.CustomTokenOrbKey))
		{
			Debug.LogError("[AwsProgressionManager] Unable to get Custom Orb Token Key from Graph Meta data: GraphId: " + masteryGraph.Id);
			return null;
		}
		return masteryGraph.Configuration.SetMasteryConfiguration.CustomTokenOrbKey;
	}

	private ProgressionTrack InitializeProgression(ProgressionTrack oldRef, ClientGraphDefinition masteryGraph, Dictionary<string, ClientNodeState> graphState)
	{
		ProgressionTrack progressionTrack = new ProgressionTrack
		{
			OrbInventoryChangeQueue = ((oldRef != null) ? new Queue<OrbInventoryChange>(oldRef.OrbInventoryChangeQueue) : new Queue<OrbInventoryChange>())
		};
		List<ProgressionTrackLevel> progressionLevelTrack = GetProgressionLevelTrack(masteryGraph, graphState);
		progressionTrack.RewardTiers = GetRewardTiers(masteryGraph, graphState);
		foreach (KeyValuePair<int, OrbSlot> item in GetRewardWebDictionary(masteryGraph, graphState))
		{
			progressionTrack.OrbSlotById[item.Key] = item.Value;
		}
		progressionTrack.Levels.AddRange(progressionLevelTrack);
		progressionTrack.Enabled = true;
		progressionTrack.Name = masteryGraph.Id;
		progressionTrack.ExpirationTime = masteryGraph.Configuration?.SetMasteryConfiguration?.TrackEndTime ?? masteryGraph.EndTime;
		progressionTrack.ExpirationWarningTime = masteryGraph.EndRevealTime;
		ClientGraphConfiguration configuration = masteryGraph.Configuration;
		progressionTrack.RewardWebStatus = configuration != null && configuration.SetMasteryConfiguration?.OrbMappings?.Count > 0;
		progressionTrack.TierCount = masteryGraph.Configuration?.SetMasteryConfiguration?.RewardTiers?.Count ?? 2;
		progressionTrack.PrizeWallId = masteryGraph.Configuration?.SetMasteryConfiguration?.PrizeWallId;
		int currentLevelIndex = 0;
		int num = 0;
		foreach (KeyValuePair<string, ClientNodeState> item2 in graphState)
		{
			var (nodeId, clientNodeState2) = (KeyValuePair<string, ClientNodeState>)(ref item2);
			if (clientNodeState2.Status != Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
			{
				continue;
			}
			ProgressionTrackLevel progressionTrackLevel = progressionLevelTrack.FirstOrDefault((ProgressionTrackLevel o) => o.LevelId == nodeId);
			if (progressionTrackLevel == null)
			{
				continue;
			}
			currentLevelIndex = progressionTrackLevel.Index;
			num = progressionTrackLevel.RawLevel;
			if (oldRef == null || oldRef.CurrentLevel == num || oldRef.CurrentLevelIndex < 0 || oldRef.CurrentLevelIndex >= oldRef.Levels.Count)
			{
				break;
			}
			ProgressionTrackLevel progressionTrackLevel2 = oldRef.Levels[oldRef.CurrentLevelIndex];
			if (progressionTrackLevel2 != null)
			{
				if (CachedBpLevelChanges == null)
				{
					Queue<LevelChange> queue = (CachedBpLevelChanges = new Queue<LevelChange>());
				}
				CachedBpLevelChanges.Enqueue(new LevelChange
				{
					Level = progressionTrackLevel2
				});
			}
			break;
		}
		foreach (KeyValuePair<int, OrbSlot> item3 in GetRewardWebDictionary(masteryGraph, graphState))
		{
			progressionTrack.OrbSlotById[item3.Key] = item3.Value;
		}
		string orbCurrencyKey = getOrbCurrencyKey(masteryGraph);
		if (_inventoryManager.Inventory.CustomTokens.TryGetValue(orbCurrencyKey, out var value))
		{
			progressionTrack.NumberOrbs = value;
		}
		progressionTrack.NumberOfExtendedLevels = 50000;
		progressionTrack.MaxLevelIndex = progressionLevelTrack.Count;
		progressionTrack.CurrentLevel = num;
		progressionTrack.CurrentLevelIndex = currentLevelIndex;
		progressionTrack.PlayerHitMaxLevel = false;
		return progressionTrack;
	}

	private Dictionary<int, OrbSlot> GetRewardWebDictionary(ClientGraphDefinition masteryGraph, Dictionary<string, ClientNodeState> graphState)
	{
		Dictionary<string, int> orbMappings = masteryGraph.Configuration.SetMasteryConfiguration.OrbMappings;
		Dictionary<string, ClientNodeDefinition> nodes = masteryGraph.Nodes;
		if (orbMappings == null || orbMappings.Count == 0)
		{
			return new Dictionary<int, OrbSlot>(0);
		}
		Dictionary<int, OrbSlot> dictionary = new Dictionary<int, OrbSlot>(orbMappings.Count);
		foreach (KeyValuePair<string, int> item in orbMappings)
		{
			OrbSlot orbSlot = new OrbSlot();
			if (graphState.TryGetValue(item.Key, out var value))
			{
				orbSlot.currentState = ((value.Status != Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed) ? OrbSlot.OrbState.Available : OrbSlot.OrbState.Unlocked);
			}
			else
			{
				orbSlot.currentState = OrbSlot.OrbState.Unavailable;
			}
			orbSlot.serverRewardNode = new ClientRewardWebNodeInfo
			{
				id = item.Value
			};
			if (!nodes.TryGetValue(item.Key, out var value2))
			{
				continue;
			}
			orbSlot.serverRewardNode.childIds = new List<int>();
			foreach (string child in value2.Children)
			{
				if (!nodes.TryGetValue(child, out var value3))
				{
					continue;
				}
				int value4;
				if (value3.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.AutomaticPayout)
				{
					ClientChestDescription clientChestDescription = value3.Configuration?.AutomaticPayoutNode?.ChestDescription;
					if (clientChestDescription != null)
					{
						orbSlot.serverRewardNode.chest = ServiceWrapperHelpers.ToClientChestDescription(clientChestDescription);
					}
				}
				else if (orbMappings.TryGetValue(value3.Id, out value4))
				{
					orbSlot.serverRewardNode.childIds.Add(value4);
				}
			}
			dictionary[item.Value] = orbSlot;
		}
		return dictionary;
	}

	private void OnInventoryWithXpUpdate(ClientInventoryUpdateReportItem update)
	{
		if ((update == null || update.xpGained != 0) && SceneManager.GetSceneByName("MainNavigation").isLoaded)
		{
			PAPA.StartGlobalCoroutine(RefreshThenProcessProgressUpdate());
		}
	}

	private async Task<bool> DidSpendMasteryOrb(ClientInventoryUpdateReportItem update)
	{
		string orbCurrencyKey = getOrbCurrencyKey(await GetTrackGraph(CurrentBpName));
		int result;
		if (orbCurrencyKey != null)
		{
			result = ((update != null && update.delta?.customTokenDelta?.FirstOrDefault((CustomTokenDeltaInfo o) => o.id == orbCurrencyKey)?.delta < 0) ? 1 : 0);
		}
		else
		{
			result = 0;
		}
		return (byte)result != 0;
	}

	private void OnCampaignGraphPurchaseNodeUpdate(ClientInventoryUpdateReportItem update)
	{
		OnCampaignGraphPurchaseNodeUpdateAsync(update);
	}

	private async Task OnCampaignGraphPurchaseNodeUpdateAsync(ClientInventoryUpdateReportItem update)
	{
		if (!(await DidSpendMasteryOrb(update)))
		{
			this.OnInventoryUpdate?.Invoke(update);
		}
	}

	private void OnLevelUpInventoryUpdate(ClientInventoryUpdateReportItem update)
	{
		OnLevelUpInventoryUpdateAsync(update);
	}

	private async Task OnLevelUpInventoryUpdateAsync(ClientInventoryUpdateReportItem update)
	{
		if (CurrentBpName == null)
		{
			Debug.LogError($"[AwsSetMasteryStrategy] Calling level up inventory update async with no current BP name. Strategy refreshing is: {_isRefreshing}");
			await Refresh();
		}
		ClientGraphDefinition clientGraphDefinition = await GetTrackGraph(CurrentBpName);
		if (clientGraphDefinition == null)
		{
			throw new Exception("No track definition for " + CurrentBpName + " while level up inventory update.");
		}
		string orbCurrencyKey = getOrbCurrencyKey(clientGraphDefinition);
		if (update?.delta?.customTokenDelta != null)
		{
			CustomTokenDeltaInfo customTokenDeltaInfo = update.delta.customTokenDelta.FirstOrDefault((CustomTokenDeltaInfo o) => o.id == orbCurrencyKey);
			if (customTokenDeltaInfo != null)
			{
				Dictionary<string, ClientNodeState> nodeStates = _graphManager.GetState(clientGraphDefinition).NodeStates;
				ClientRewardWebDiff webDiff = new ClientRewardWebDiff(clientGraphDefinition, nodeStates);
				_inventoryManager.Inventory.CustomTokens.TryGetValue(orbCurrencyKey, out var value);
				int oldOrbCount = value - customTokenDeltaInfo.delta;
				ClientPlayerTrackUpdate obj = new ClientPlayerTrackUpdate(CurrentBpName, webDiff, new OrbCountDiff
				{
					oldOrbCount = oldOrbCount,
					currentOrbCount = value
				});
				TrackDeltas.Add(update);
				this.ProcessTrackUpdate?.Invoke(obj);
			}
		}
		this.OnInventoryUpdate?.Invoke(update);
	}

	private IEnumerator RefreshThenProcessProgressUpdate()
	{
		yield return Refresh().AsCoroutine();
		this.OnCurrentBpProgressUpdate?.Invoke(new Queue<LevelChange>());
	}

	private List<ClientTrackRewardInfo> GetClientTrackRewardForTieredNode(ClientNodeDefinition tieredNode)
	{
		List<ClientTrackRewardInfo> list = new List<ClientTrackRewardInfo>(tieredNode.Configuration.TieredNode.ChestDescriptions.Count);
		foreach (ClientChestDescription value in tieredNode.Configuration.TieredNode.ChestDescriptions.Values)
		{
			list.Add(new ClientTrackRewardInfo
			{
				chest = ((value != null) ? ServiceWrapperHelpers.ToClientChestDescription(value) : null)
			});
		}
		return list;
	}

	private int GetOrbCountForMasteryGraph(ClientGraphDefinition masteryGraph)
	{
		string orbCurrencyKey = getOrbCurrencyKey(masteryGraph);
		_inventoryManager.Inventory.CustomTokens.TryGetValue(orbCurrencyKey, out var value);
		return value;
	}

	private List<ProgressionTrackLevel> GetProgressionLevelTrack(ClientGraphDefinition masteryGraph, Dictionary<string, ClientNodeState> graphState)
	{
		List<ProgressionTrackLevel> list = new List<ProgressionTrackLevel>();
		Dictionary<string, ClientNodeDefinition> nodes = masteryGraph.Nodes;
		if (string.IsNullOrEmpty(masteryGraph.Configuration?.SetMasteryConfiguration?.StartingLevelNodeId))
		{
			FailedInitializing = true;
			Debug.LogError("[AwsProgressionManager] Unable to get starting node from configuration for graph: " + masteryGraph.Id);
			return null;
		}
		string startingLevelNodeId = masteryGraph.Configuration.SetMasteryConfiguration.StartingLevelNodeId;
		Queue<ClientNodeDefinition> queue = new Queue<ClientNodeDefinition>();
		int num = 0;
		if (nodes.TryGetValue(startingLevelNodeId, out var value))
		{
			queue.Enqueue(value);
			while (queue.Count > 0)
			{
				ClientNodeDefinition clientNodeDefinition = queue.Dequeue();
				int xpToComplete = clientNodeDefinition?.Configuration?.ProgressNode?.Threshold ?? 1000;
				bool flag = clientNodeDefinition.Id == masteryGraph?.Configuration?.SetMasteryConfiguration?.RepeatingLevelNodeId;
				int eXPProgressIfIsCurrent = 0;
				int num2 = num + 1;
				if (graphState.TryGetValue(clientNodeDefinition.Id, out var value2))
				{
					eXPProgressIfIsCurrent = value2.ProgressNodeState?.CurrentProgress ?? 0;
					if (flag)
					{
						num2 += value2.ProgressNodeState?.CompletionCount ?? 0;
					}
				}
				ProgressionTrackLevel progressionTrackLevel = new ProgressionTrackLevel
				{
					IsRepeatable = flag,
					LevelId = clientNodeDefinition.Id,
					Index = num,
					RawLevel = num2,
					EXPProgressIfIsCurrent = eXPProgressIfIsCurrent,
					ServerLevel = new ClientTrackLevelInfo
					{
						isTentpole = flag,
						xpToComplete = xpToComplete,
						isPageStarter = (num == 0)
					}
				};
				List<ClientTrackRewardInfo> list2 = new List<ClientTrackRewardInfo>();
				foreach (string child in clientNodeDefinition.Children)
				{
					if (nodes.TryGetValue(child, out var value3))
					{
						if (value3.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.TieredReward && value3.Configuration?.TieredNode?.ChestDescriptions != null)
						{
							list2.AddRange(GetClientTrackRewardForTieredNode(value3));
						}
						else if (value3.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.Progress)
						{
							queue.Enqueue(value3);
						}
					}
					else
					{
						Debug.LogWarning("[AwsProgressionManager] Unable to get node from map. There's most likely a problem in the graph configuration and a mismatch in node id");
					}
				}
				progressionTrackLevel.ServerRewardTiers = list2;
				list.Add(progressionTrackLevel);
				num++;
			}
		}
		return list;
	}

	public void OnTrackRewardTierUpdateReceived(RewardTierUpdate update)
	{
		this.OnCurrentBpRewardTierUpdate?.Invoke(null);
	}
}
