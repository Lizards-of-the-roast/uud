using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Assets.Core.Meta.Utilities;
using Core.Meta.MainNavigation.Store;
using Core.Meta.Quests;
using EventPage.Components;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Network.ServiceWrappers;

namespace EventPage;

public class EventPageQuestsController
{
	private ContentControllerObjectives _questBar;

	private ContentControllerRewards _rewardsPanel;

	private EventComponentManager _eventComponentManager;

	private CardDatabase _cardDatabase;

	private CardMaterialBuilder _cardMaterialBuilder;

	private bool _rewardFound = true;

	private Action _onRewardsPanelClosed;

	private AssetLookupSystem _assetLookupSystem;

	private ContentControllerObjectives QuestBar
	{
		get
		{
			if (!(_questBar == null))
			{
				return _questBar;
			}
			return _questBar = WrapperController.Instance.SceneLoader.GetObjectivesController();
		}
	}

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

	public EventPageQuestsController(Action onRewardsPanelClosed, EventComponentManager eventComponentManager, CardDatabase cardDatabase, CardMaterialBuilder cardMaterialBuilder, AssetLookupSystem assetLookupSystem)
	{
		_cardMaterialBuilder = cardMaterialBuilder;
		_cardDatabase = cardDatabase;
		_onRewardsPanelClosed = onRewardsPanelClosed;
		_eventComponentManager = eventComponentManager;
		_assetLookupSystem = assetLookupSystem;
	}

	public void OnEventPageOpen()
	{
		_rewardFound = true;
		OnQuestProgressBarFinishedAnimating();
		if (QuestBar != null)
		{
			QuestBar.OnRewardTicked += OnQuestProgressBarObjectiveTicked;
		}
		if (QuestBar != null)
		{
			QuestBar.OnBarFinishedAnimating += OnQuestProgressBarFinishedAnimating;
		}
	}

	public void OnEventPageClosed()
	{
		if (QuestBar != null)
		{
			QuestBar.OnRewardTicked -= OnQuestProgressBarObjectiveTicked;
		}
		if (QuestBar != null)
		{
			QuestBar.OnBarFinishedAnimating -= OnQuestProgressBarFinishedAnimating;
		}
		OnQuestProgressBarFinishedAnimating();
		_questBar = null;
		_rewardsPanel = null;
	}

	protected void OnQuestProgressBarObjectiveTicked(RewardObjectiveContext context)
	{
		ContentControllerRewards rewardsPanel = RewardsPanel;
		if ((object)rewardsPanel != null && rewardsPanel.OnRewardBubbleTicked(context, OnQuestRewardsPanelClosed))
		{
			_rewardFound = true;
			QuestBar?.SetInteractable(interactable: false);
		}
	}

	protected void OnQuestProgressBarFinishedAnimating()
	{
		if (!_rewardFound && QuestBar != null && QuestBar.gameObject.activeSelf)
		{
			OnQuestRewardsPanelClosed();
		}
		_rewardFound = false;
	}

	private void OnQuestRewardsPanelClosed()
	{
		HideQuestBar();
		_onRewardsPanelClosed?.Invoke();
		RewardsPanel?.UnregisterRewardsWillCloseCallback(OnQuestRewardsPanelClosed);
	}

	public void HideQuestBar()
	{
		QuestBar?.Hide();
		QuestBar?.SetInteractable(interactable: true);
	}

	public IEnumerator Coroutine_ShowQuestBar(EventContext eventContext)
	{
		Promise<PlayerProgressDailyWeekly> dailyWeeklyPromise = Pantry.Get<IQuestServiceWrapper>().GetPlayerProgressDailyWeekly();
		yield return dailyWeeklyPromise.AsCoroutine();
		if (dailyWeeklyPromise.Successful && ShowQuestBar(eventContext, dailyWeeklyPromise))
		{
			_eventComponentManager.SetProgressBarState(EventPageStates.ClaimQuestRewards);
		}
		else
		{
			_eventComponentManager.SetProgressBarState(EventPageStates.DisplayEvent);
		}
	}

	private bool ShowQuestBar(EventContext eventContext, Promise<PlayerProgressDailyWeekly> dailyWeeklyPromise)
	{
		int dailySequence = dailyWeeklyPromise.Result.dailySequence;
		int weeklySequence = dailyWeeklyPromise.Result.weeklySequence;
		RewardScheduleIntermediate rewardSchedule = WrapperController.Instance.RewardSchedule;
		int num = eventContext.PostMatchContext?.GamesWon ?? 0;
		(DailyWeeklyReward, bool) currentReward = RewardScheduleUtils.GetCurrentReward(rewardSchedule.dailyRewards, dailySequence, num);
		DailyWeeklyReward nextReward = RewardScheduleUtils.GetNextReward(rewardSchedule.dailyRewards, dailySequence);
		DailyWeeklyReward lastReward = RewardScheduleUtils.GetLastReward(rewardSchedule.dailyRewards);
		(DailyWeeklyReward, bool) currentReward2 = RewardScheduleUtils.GetCurrentReward(rewardSchedule.weeklyRewards, weeklySequence, num);
		DailyWeeklyReward nextReward2 = RewardScheduleUtils.GetNextReward(rewardSchedule.weeklyRewards, weeklySequence);
		DailyWeeklyReward lastReward2 = RewardScheduleUtils.GetLastReward(rewardSchedule.weeklyRewards);
		PostMatchClientUpdate postMatchClientUpdate = WrapperController.Instance.PostMatchClientUpdate;
		List<Client_QuestData> list;
		List<Client_QuestData> list2;
		if (postMatchClientUpdate != null && postMatchClientUpdate.questUpdate?.Count > 0)
		{
			list = new List<Client_QuestData>(WrapperController.Instance.PostMatchClientUpdate.questUpdate.Select((QuestData x) => new Client_QuestData(x)));
			list2 = list.Where((Client_QuestData q) => q.EndingProgress > q.StartingProgress).ToList();
		}
		else
		{
			list = new List<Client_QuestData>();
			list2 = new List<Client_QuestData>();
		}
		if (list2.Count > 0 || currentReward.Item2 || currentReward2.Item2)
		{
			QuestBar?.ShowQuestBar(list, list.Select((Client_QuestData q) => new RewardDisplayData(q.Reward, _cardDatabase.CardDataProvider, _cardMaterialBuilder)).ToList(), TempRewardTranslation.ChestDescriptionToDisplayData(currentReward.Item2 ? currentReward.Item1.awardDescription : nextReward.awardDescription, _cardDatabase.CardDataProvider, _cardMaterialBuilder), TempRewardTranslation.ChestDescriptionToDisplayData(currentReward2.Item2 ? currentReward2.Item1.awardDescription : nextReward2.awardDescription, _cardDatabase.CardDataProvider, _cardMaterialBuilder), dailySequence, lastReward.wins, weeklySequence, lastReward2.wins, doUpdateBarAnimation: true, num, eventContext.PlayerEvent.EventInfo.UpdateDailyWeeklyRewards);
			return true;
		}
		return false;
	}
}
