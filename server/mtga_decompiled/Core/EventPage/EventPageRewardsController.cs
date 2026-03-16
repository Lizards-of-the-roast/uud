using System;
using System.Collections.Generic;
using Core.Meta.MainNavigation.Store;
using Wizards.Models;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Loc;

namespace EventPage;

public class EventPageRewardsController
{
	private List<ClientInventoryUpdateReportItem> _inventoryUpdates = new List<ClientInventoryUpdateReportItem>();

	private InventoryManager _inventoryManager;

	private ContentControllerRewards _rewardsPanel;

	private readonly Action _onRewardsPanelClosed;

	private ContentControllerRewards RewardsPanel
	{
		get
		{
			if (!(_rewardsPanel == null))
			{
				return _rewardsPanel;
			}
			return _rewardsPanel = WrapperController.Instance.SceneLoader.GetRewardsContentController();
		}
	}

	public bool HasPendingRewards => _inventoryUpdates.Count > 0;

	public EventPageRewardsController(InventoryManager inventoryManager, Action onRewardsPanelClosed)
	{
		_inventoryManager = inventoryManager;
		_onRewardsPanelClosed = onRewardsPanelClosed;
	}

	public void OnEventPageOpen()
	{
		RewardsPanel?.Clear();
		_inventoryUpdates.Clear();
		_inventoryManager.Subscribe(InventoryUpdateSource.EventReward, OnInventoryUpdated, null, publish: false);
		_inventoryManager.Subscribe(InventoryUpdateSource.EntryReward, GetInventoryUpdatesAndDisplay, null, publish: false);
		_inventoryManager.Subscribe(InventoryUpdateSource.EventEntryReward, GetInventoryUpdatesAndDisplay);
	}

	private void OnInventoryUpdated(ClientInventoryUpdateReportItem update)
	{
		_inventoryUpdates.Add(update);
	}

	private void GetInventoryUpdatesAndDisplay(ClientInventoryUpdateReportItem update)
	{
		RewardsPanel?.RegisterRewardWillCloseCallback(OnRewardPanelClosed);
		RewardsPanel?.AddAndDisplayRewardsCoroutine(update, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
	}

	public void OnEventPageClosed()
	{
		_inventoryManager.UnSubscribe(InventoryUpdateSource.EventReward, OnInventoryUpdated);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.EntryReward, GetInventoryUpdatesAndDisplay);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.EventEntryReward, GetInventoryUpdatesAndDisplay);
		_rewardsPanel = null;
	}

	public void ShowRewardsPanel()
	{
		if (_inventoryUpdates.Count > 0)
		{
			RewardsPanel?.RegisterRewardWillCloseCallback(OnRewardPanelClosed);
			RewardsPanel?.AddAndDisplayRewardsCoroutine(_inventoryUpdates, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
			_inventoryUpdates.Clear();
		}
	}

	public void OnRewardPanelClosed()
	{
		_onRewardsPanelClosed?.Invoke();
		RewardsPanel?.UnregisterRewardsWillCloseCallback(OnRewardPanelClosed);
	}
}
