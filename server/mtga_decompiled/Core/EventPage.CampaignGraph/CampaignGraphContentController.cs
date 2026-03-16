using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Assets.Core.Meta.Utilities;
using Core.Code.Input;
using Core.Meta.MainNavigation.Store;
using Core.Meta.Quests;
using EventPage.CampaignGraph.ColorMastery;
using EventPage.Components;
using MTGA.KeyboardManager;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace EventPage.CampaignGraph;

public class CampaignGraphContentController : NavContentController
{
	[SerializeField]
	private Transform _templatesParent;

	[SerializeField]
	private PlayBladeV2 _playBlade;

	private EventContext _event;

	private EventTemplate _activeEventTemplate;

	private ICardRolloverZoom _rolloverZoomView;

	private Dictionary<string, EventTemplate> _instantiatedEventTemplates = new Dictionary<string, EventTemplate>(5);

	private IColorChallengeStrategy _strategy;

	private AssetLookupSystem _assetLookupSystem;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	private CosmeticsProvider _cosmetics;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private bool _noUpdatesAvailable = true;

	private string _currentEventTrack;

	public bool EventPageReady = true;

	public bool SkipEventPage;

	protected EventContext EventContext => Pantry.Get<EventManager>().ColorMasteryEventContext;

	public override NavContentType NavContentType => NavContentType.ChallengeEventLanding;

	public override bool IsReadyToShow
	{
		get
		{
			if (EventPageReady)
			{
				return _activeEventTemplate != null;
			}
			return false;
		}
	}

	public override bool SkipScreen => SkipEventPage;

	private string InstantiatedTemplateKey => _strategy.TemplateKey;

	public ContentControllerObjectives QuestProgressBar { get; private set; }

	public ContentControllerRewards RewardsPanel { get; private set; }

	public AssetLookupSystem AssetLookupSystem { get; private set; }

	public void Init(ContentControllerObjectives objectivesController, ContentControllerRewards rewardsController, ICardRolloverZoom cardRolloverZoomBase, AssetLookupSystem assetLookupSystem, KeyboardManager keyboardManager, IActionSystem actionSystem, CosmeticsProvider cosmetics, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		QuestProgressBar = objectivesController;
		RewardsPanel = rewardsController;
		_rolloverZoomView = cardRolloverZoomBase;
		AssetLookupSystem = assetLookupSystem;
		_keyboardManager = keyboardManager;
		_actionSystem = actionSystem;
		_cosmetics = cosmetics;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		if (_templatesParent == null)
		{
			_templatesParent = base.transform;
		}
		_strategy = Pantry.Get<IColorChallengeStrategy>();
		_assetLookupSystem = assetLookupSystem;
		_playBlade.Inject(actionSystem);
		_playBlade.OnShow = delegate
		{
			CampaignGraphTrackModule componentInChildren = GetComponentInChildren<CampaignGraphTrackModule>();
			if (componentInChildren != null)
			{
				componentInChildren.DeactivatePopups();
			}
		};
	}

	public IEnumerator Coroutine_ShowQuestBar()
	{
		Promise<PlayerProgressDailyWeekly> dailyWeeklyPromise = Pantry.Get<IQuestServiceWrapper>().GetPlayerProgressDailyWeekly();
		EventPageReady = false;
		yield return dailyWeeklyPromise.AsCoroutine();
		EventPageReady = true;
		int dailySequence = dailyWeeklyPromise.Result.dailySequence;
		int weeklySequence = dailyWeeklyPromise.Result.weeklySequence;
		RewardScheduleIntermediate rewardSchedule = WrapperController.Instance.RewardSchedule;
		int num = EventContext.PostMatchContext?.GamesWon ?? 0;
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
			_activeEventTemplate.DisableMainButton();
			QuestProgressBar.ShowQuestBar(list, list.Select((Client_QuestData q) => new RewardDisplayData(q.Reward, _cardDatabase.CardDataProvider, _cardViewBuilder.CardMaterialBuilder)).ToList(), TempRewardTranslation.ChestDescriptionToDisplayData(currentReward.Item2 ? currentReward.Item1.awardDescription : nextReward.awardDescription, _cardDatabase.CardDataProvider, _cardViewBuilder.CardMaterialBuilder), TempRewardTranslation.ChestDescriptionToDisplayData(currentReward2.Item2 ? currentReward2.Item1.awardDescription : nextReward2.awardDescription, _cardDatabase.CardDataProvider, _cardViewBuilder.CardMaterialBuilder), dailySequence, lastReward.wins, weeklySequence, lastReward2.wins, doUpdateBarAnimation: true, num, EventContext.PlayerEvent.EventInfo.UpdateDailyWeeklyRewards);
		}
		else if (EventContext.PlayerEvent.HasPrize(null) || RewardsPanel.Visible)
		{
			_activeEventTemplate.SetProgressBarState(EventPageStates.ClaimQuestRewards);
		}
		else
		{
			_activeEventTemplate.SetProgressBarState(EventPageStates.DisplayEvent);
		}
	}

	public void RefreshEvent()
	{
		if (EventContext.PlayerEvent is ColorChallengePlayerEvent colorChallengePlayerEvent)
		{
			string campaignGraphSelectedEvent = MDNPlayerPrefs.GetCampaignGraphSelectedEvent(WrapperController.Instance.AccountClient.AccountInformation?.PersonaID, colorChallengePlayerEvent.Name);
			_currentEventTrack = _strategy.SwitchTrack(campaignGraphSelectedEvent);
		}
		else
		{
			_currentEventTrack = _strategy.CurrentTrackName;
		}
		if (InstantiatedTemplateKey == "ColorChallenge")
		{
			if (_playBlade.ColorMasteryPanel != null)
			{
				_playBlade.ColorMasteryPanel.SetEvent(_strategy, _currentEventTrack, _assetLookupSystem);
			}
			else
			{
				_playBlade.InitColorMasteryPanel(_strategy, _currentEventTrack, _assetLookupSystem).SetOnItemClickedCallback(_colorMastery_OnClicked);
			}
		}
	}

	public override void OnBeginOpen()
	{
		EventPageReady = false;
		StartCoroutine(_coroutine_beginOpen());
	}

	private IEnumerator _coroutine_beginOpen()
	{
		SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: true);
		yield return _strategy.UpdateData();
		if (!_instantiatedEventTemplates.TryGetValue(InstantiatedTemplateKey, out var value))
		{
			value = InitEventTemplate(InstantiatedTemplateKey, EventContext, _cosmetics);
			OverlayModule[] componentsInChildren = base.gameObject.GetComponentsInChildren<OverlayModule>(includeInactive: true);
			if (componentsInChildren != null)
			{
				OverlayModule[] array = componentsInChildren;
				foreach (OverlayModule obj in array)
				{
					obj.SetZoomHandler(_rolloverZoomView);
					obj.OnDisabled = (Action)Delegate.Combine(obj.OnDisabled, new Action(_onOverlayModuleDisabled));
					obj.OnEnabled = (Action)Delegate.Combine(obj.OnEnabled, new Action(_onOverlayModuleEnabled));
				}
			}
		}
		ShowEventTemplate(value);
		if (_strategy.InPlayingMatchesModule)
		{
			StartCoroutine(_activeEventTemplate.PlayAnimation(EventTemplateAnimation.SetActive));
		}
		if (string.IsNullOrWhiteSpace(_currentEventTrack) || _strategy.CompletedGames == 0 || (_strategy.CurrentTrackCompleted && EventContext.PostMatchContext == null))
		{
			_playBlade.Show();
		}
		else
		{
			_playBlade.Hide();
		}
		EventPageReady = true;
		SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
	}

	private void _onOverlayModuleEnabled()
	{
		SceneLoader.GetSceneLoader().GetNavBar().SetHiddenState(hidden: true);
		_playBlade.Disable();
	}

	private void _onOverlayModuleDisabled()
	{
		if (_playBlade != null)
		{
			_playBlade.Enable();
		}
		NavBarController navBarController = SceneLoader.GetSceneLoader()?.GetNavBar();
		if (navBarController != null)
		{
			navBarController.SetHiddenState(hidden: false);
		}
	}

	private void _onChangeEventModule(string moduleName)
	{
		if (_activeEventTemplate != null)
		{
			_activeEventTemplate.gameObject.SetActive(value: false);
			_activeEventTemplate = null;
		}
		_strategy.SwitchTrack(moduleName);
		_currentEventTrack = moduleName;
		ShowEventTemplate(_instantiatedEventTemplates[InstantiatedTemplateKey]);
		StartCoroutine(_activeEventTemplate.PlayAnimation(EventTemplateAnimation.ModuleIntro));
		StartCoroutine(_activeEventTemplate.PlayAnimation(EventTemplateAnimation.Intro));
	}

	private void _colorMastery_OnClicked(string moduleName)
	{
		if (moduleName != _currentEventTrack)
		{
			_onChangeEventModule(moduleName);
		}
	}

	private EventTemplate InitEventTemplate(string templateKey, EventContext eventContext, CosmeticsProvider cosmetics)
	{
		EventTemplate eventTemplate = AssetLoader.Instantiate<EventTemplate>(AssetLookupSystem.GetPrefabPath<CampaignGraphEventTemplatePrefab, EventTemplate>(), _templatesParent);
		RectTransform component = eventTemplate.GetComponent<RectTransform>();
		component.offsetMin = new Vector2(0f, 0f);
		component.offsetMax = new Vector2(0f, -80f);
		_instantiatedEventTemplates.Add(templateKey, eventTemplate);
		eventTemplate.Init(AssetLookupSystem, _keyboardManager, _actionSystem, cosmetics, _cardDatabase, _cardViewBuilder);
		return eventTemplate;
	}

	private void ShowEventTemplate(EventTemplate eventTemplate)
	{
		if (eventTemplate != _activeEventTemplate && _activeEventTemplate != null)
		{
			_activeEventTemplate.Hide();
		}
		QuestProgressBar.OnRewardTicked -= OnQuestProgressBarObjectiveTicked;
		QuestProgressBar.OnBarFinishedAnimating -= OnQuestProgressBarFinishedAnimating;
		OnQuestProgressBarFinishedAnimating();
		_activeEventTemplate = eventTemplate;
		_activeEventTemplate.Show();
		QuestProgressBar.OnRewardTicked += OnQuestProgressBarObjectiveTicked;
		QuestProgressBar.OnBarFinishedAnimating += OnQuestProgressBarFinishedAnimating;
	}

	public override void OnBeginClose()
	{
		QuestProgressBar.OnRewardTicked -= OnQuestProgressBarObjectiveTicked;
		QuestProgressBar.OnBarFinishedAnimating -= OnQuestProgressBarFinishedAnimating;
		OnQuestProgressBarFinishedAnimating();
		_activeEventTemplate.Hide();
		_playBlade.Hide();
	}

	private void OnQuestProgressBarObjectiveTicked(RewardObjectiveContext context)
	{
		_noUpdatesAvailable = true;
		if (RewardsPanel.OnRewardBubbleTicked(context, OnQuestRewardsPanelClosed))
		{
			_noUpdatesAvailable = false;
			QuestProgressBar.SetInteractable(interactable: false);
		}
	}

	private void OnQuestProgressBarFinishedAnimating()
	{
		if (_noUpdatesAvailable && _activeEventTemplate != null && QuestProgressBar != null && QuestProgressBar.gameObject.activeSelf)
		{
			OnQuestRewardsPanelClosed();
		}
		_noUpdatesAvailable = true;
	}

	private void OnQuestRewardsPanelClosed()
	{
		QuestProgressBar.Hide();
		QuestProgressBar.SetInteractable(interactable: true);
		_activeEventTemplate.SetProgressBarState(EventPageStates.DisplayEvent);
	}
}
