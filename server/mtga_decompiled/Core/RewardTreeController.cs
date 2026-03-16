using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Assets.Core.Meta.Utilities;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation.Store;
using Core.Meta.Quests;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Arena.Promises;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

public class RewardTreeController : NavContentController
{
	private ContentControllerObjectives ObjectivesPanel;

	private ContentControllerRewards RewardsPanel;

	public EPPDeckUpgradeController DeckUpgrade;

	[SerializeField]
	private CustomButton _backButton;

	[SerializeField]
	private List<RewardTreeView> _rewardTreePrefabs;

	[SerializeField]
	private EPPRewardWebHanger _hanger;

	[SerializeField]
	private GameObject _orangeButton;

	[SerializeField]
	private GameObject _outlineButton;

	[SerializeField]
	private Localize _titleLabel;

	[SerializeField]
	private GameObject _orbInventory;

	[SerializeField]
	private Localize _orbInventoryLabel;

	[SerializeField]
	private Localize _orbFractionLabel;

	[SerializeField]
	private List<GameObject> _orbsToAnimate;

	[SerializeField]
	private bool _useLargeOrbs;

	private RewardTreeView _activeView;

	private readonly List<RewardTreeView> _treeViews = new List<RewardTreeView>();

	private RewardTreePageContext _context;

	private SetMasteryDataProvider _masteryPassProvider;

	private DateTime _sceneActivationForBI;

	private IQuestServiceWrapper _questServiceWrapper;

	private IBILogger _logger;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private CardMaterialBuilder _cardMaterialBuilder;

	private AssetLookupSystem _assetLookupSystem;

	public override NavContentType NavContentType => NavContentType.RewardTree;

	public bool PauseAnimation
	{
		get
		{
			if (_activeView != null)
			{
				return _activeView.PauseAnimation;
			}
			return false;
		}
		set
		{
			if ((bool)_activeView)
			{
				_activeView.PauseAnimation = value;
			}
		}
	}

	private void Awake()
	{
		_backButton.OnClick.AddListener(ClickBackButton);
	}

	private void OnDestroy()
	{
		_backButton.OnClick.RemoveListener(ClickBackButton);
	}

	public void SetContext(RewardTreePageContext context)
	{
		_context = context;
	}

	public override void OnBeginOpen()
	{
		ObjectivesPanel.OnRewardTicked += OnRewardTicked;
		ObjectivesPanel.OnBarFinishedAnimating += OnObjectivesFinishedAnimating;
		string trackName = _context?.TrackName ?? _masteryPassProvider.CurrentBpName;
		_activeView = _treeViews.Find((RewardTreeView v) => v.TrackName == trackName);
		if (_activeView == null)
		{
			RewardTreeView rewardTreeView = _rewardTreePrefabs.Find((RewardTreeView v) => v.TrackName == trackName);
			if (rewardTreeView == null)
			{
				SimpleLog.LogPreProdError("Could not find a RewardTreeView prefab for track " + trackName + ", please check the RewardTreeController's serialized fields!");
				return;
			}
			_activeView = UnityEngine.Object.Instantiate(rewardTreeView, base.transform, worldPositionStays: false);
			_activeView.transform.SetSiblingIndex(0);
			_treeViews.Add(_activeView);
		}
		_activeView.InitializeRewardTreeView(_cardDatabase, _cardViewBuilder, _cardMaterialBuilder, _useLargeOrbs, _orbsToAnimate);
		_titleLabel.SetText(_masteryPassProvider.GetTrackTitle(trackName));
		_activeView.OnShowHanger += ShowHanger;
		_activeView.OnClearHanger += ClearHanger;
		_activeView.OnUpdateOrangeButton += _orangeButton.SetActive;
		_activeView.OnUpdateOutlineButton += _outlineButton.SetActive;
		_activeView.OnUpdateOrbPicks += UpdateOrbPicks;
		_activeView.OnUpdateOrbInvNumber += UpdateOrbInvNumber;
		_activeView.OnUpdateOrbPlacedNumber += UpdateOrbFractionalText;
		foreach (RewardTreeView treeView in _treeViews)
		{
			Wotc.Mtga.Extensions.GameObjectExtensions.UpdateActive(active: treeView == _activeView, go: treeView.gameObject);
		}
		base.OnBeginOpen();
	}

	public override void OnBeginClose()
	{
		ObjectivesPanel.OnRewardTicked -= OnRewardTicked;
		ObjectivesPanel.OnBarFinishedAnimating -= OnObjectivesFinishedAnimating;
		if (_activeView != null)
		{
			_activeView.OnShowHanger -= ShowHanger;
			_activeView.OnClearHanger -= ClearHanger;
			_activeView.OnUpdateOrangeButton -= _orangeButton.SetActive;
			_activeView.OnUpdateOutlineButton -= _outlineButton.SetActive;
			_activeView.OnUpdateOrbPicks -= UpdateOrbPicks;
			_activeView.OnUpdateOrbInvNumber -= UpdateOrbInvNumber;
			_activeView.OnUpdateOrbPlacedNumber -= UpdateOrbFractionalText;
		}
		OnObjectivesFinishedAnimating();
		DeckUpgrade.gameObject.UpdateActive(active: false);
		EventSystem.current.SetSelectedGameObject(null);
		OnRewardWebDoneBeingViewed(_context?.PreviousSceneForBI, (_activeView == null) ? null : _activeView.TrackName);
		base.OnBeginClose();
	}

	public override void OnFinishOpen()
	{
		if (_context.PostMatchContext != null)
		{
			StartCoroutine(Coroutine_ShowQuestBar());
		}
		else
		{
			AfterShowingRewardsOnSceneEntrance();
		}
		base.OnFinishOpen();
	}

	public void Init(ContentControllerObjectives objectivesPanel, ContentControllerRewards rewardsPanel, ICardRolloverZoom zoomHandler, IBILogger logger, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, AssetLookupSystem assetLookupSystem)
	{
		_questServiceWrapper = Pantry.Get<IQuestServiceWrapper>();
		_masteryPassProvider = Pantry.Get<SetMasteryDataProvider>();
		ObjectivesPanel = objectivesPanel;
		RewardsPanel = rewardsPanel;
		_logger = logger;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_cardMaterialBuilder = cardMaterialBuilder;
		_hanger.SetZoomHandler(zoomHandler);
		_assetLookupSystem = assetLookupSystem;
		foreach (RewardTreeView treeView in _treeViews)
		{
			treeView.InitializeRewardTreeView(_cardDatabase, _cardViewBuilder, _cardMaterialBuilder, _useLargeOrbs);
		}
	}

	public void ClickOrangeButton()
	{
		_activeView.AutoPick();
	}

	public void ClickOutlineButton()
	{
		_activeView.SuggestPick();
	}

	private void OnRewardTicked(RewardObjectiveContext context)
	{
		if (base.gameObject.activeInHierarchy && RewardsPanel.OnRewardBubbleTicked(context, AfterShowingRewardsOnSceneEntrance))
		{
			ObjectivesPanel.SetInteractable(interactable: false);
		}
	}

	private void ShowHanger(EPP_OrbSlotView orbView)
	{
		_hanger.InspectOrbSlot(orbView);
	}

	private void UpdateOrbPicks(int numberOrbPicks)
	{
		CustomButton component = _orangeButton.GetComponent<CustomButton>();
		string text = ((numberOrbPicks <= 1) ? "EPP/RewardWeb/ConfirmPick" : "EPP/RewardWeb/ConfirmPicks");
		component.SetLocText(text);
	}

	private void UpdateOrbInvNumber(int numberOrbs)
	{
		_orbInventory.SetActive(numberOrbs > 0);
		_orbInventoryLabel.SetText("MainNav/General/Simple_Number", new Dictionary<string, string> { 
		{
			"number",
			numberOrbs.ToString()
		} });
	}

	public void UpdateOrbFractionalText(MTGALocalizedString text)
	{
		_orbFractionLabel.SetText(text);
	}

	private void ClearHanger()
	{
		_hanger.DisengageOrbSlot();
	}

	private void AfterShowingRewardsOnSceneEntrance()
	{
		ObjectivesPanel.Hide();
		ObjectivesPanel.SetInteractable(interactable: true);
		if (_activeView != null)
		{
			_activeView.InitDisplay(RewardsPanel, DeckUpgrade);
		}
	}

	private void OnObjectivesFinishedAnimating()
	{
		if (!RewardsPanel.Visible)
		{
			AfterShowingRewardsOnSceneEntrance();
		}
	}

	private IEnumerator Coroutine_ShowQuestBar()
	{
		Promise<PlayerProgressDailyWeekly> dailyWeeklyPromise = _questServiceWrapper.GetPlayerProgressDailyWeekly();
		yield return dailyWeeklyPromise.AsCoroutine();
		List<Client_QuestData> list = new List<Client_QuestData>(WrapperController.Instance.PostMatchClientUpdate.questUpdate.Select((QuestData x) => new Client_QuestData(x)));
		int dailySequence = dailyWeeklyPromise.Result.dailySequence;
		int weeklySequence = dailyWeeklyPromise.Result.weeklySequence;
		int num = ((_context.PostMatchContext != null && _context.PostMatchContext.GamesWon > 0) ? _context.PostMatchContext.GamesWon : 0);
		RewardScheduleIntermediate rewardSchedule = WrapperController.Instance.RewardSchedule;
		(DailyWeeklyReward, bool) currentReward = RewardScheduleUtils.GetCurrentReward(rewardSchedule.dailyRewards, dailySequence, num);
		DailyWeeklyReward nextReward = RewardScheduleUtils.GetNextReward(rewardSchedule.dailyRewards, dailySequence);
		DailyWeeklyReward lastReward = RewardScheduleUtils.GetLastReward(rewardSchedule.dailyRewards);
		(DailyWeeklyReward, bool) currentReward2 = RewardScheduleUtils.GetCurrentReward(rewardSchedule.weeklyRewards, weeklySequence, num);
		DailyWeeklyReward nextReward2 = RewardScheduleUtils.GetNextReward(rewardSchedule.weeklyRewards, weeklySequence);
		DailyWeeklyReward lastReward2 = RewardScheduleUtils.GetLastReward(rewardSchedule.weeklyRewards);
		ObjectivesPanel.ShowQuestBar(list, list.Select((Client_QuestData q) => new RewardDisplayData(q.Reward, _cardDatabase.CardDataProvider, _cardMaterialBuilder)).ToList(), TempRewardTranslation.ChestDescriptionToDisplayData(currentReward.Item2 ? currentReward.Item1.awardDescription : nextReward.awardDescription, _cardDatabase.CardDataProvider, _cardMaterialBuilder), TempRewardTranslation.ChestDescriptionToDisplayData(currentReward2.Item2 ? currentReward2.Item1.awardDescription : nextReward2.awardDescription, _cardDatabase.CardDataProvider, _cardMaterialBuilder), dailySequence, lastReward.wins, weeklySequence, lastReward2.wins, doUpdateBarAnimation: true, num, _context.PostMatchContext == null || _context.PostMatchContext.MatchesOfThisEventTypeCanAffectDailyWeeklyWins);
	}

	public void ClickBackButton()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_back, base.gameObject);
		if (_context.PostMatchContext != null)
		{
			if (_context.EventContext != null)
			{
				SceneLoader.GetSceneLoader().GoToEventScreen(_context.EventContext);
			}
			else
			{
				SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
			}
		}
		else
		{
			ProgressionTrackPageContext trackPageContext = new ProgressionTrackPageContext(null, NavContentType.None, NavContentType.RewardTree);
			SceneLoader.GetSceneLoader().GoToProgressionTrackScene(trackPageContext, "From Web back button");
		}
	}

	private void OnRewardWebDoneBeingViewed(string fromScene, string trackName)
	{
		DateTime utcNow = DateTime.UtcNow;
		TimeSpan duration = utcNow - _sceneActivationForBI;
		_sceneActivationForBI = utcNow;
		_logger.Send(ClientBusinessEventType.ProgressionRewardWebViewed, new ProgressionRewardWebViewed
		{
			FromSceneName = fromScene,
			TrackName = trackName,
			Duration = duration,
			EventTime = utcNow
		});
	}

	public void TEST_DeckUpgrade()
	{
		if (SceneLoader.GetSceneLoader().CurrentContentType == NavContentType.RewardTree)
		{
			TEST_DeckUpgradeComplete();
			return;
		}
		SceneLoader.GetSceneLoader().SceneLoaded += TEST_DeckUpgradeComplete;
		SceneLoader.GetSceneLoader().GoToRewardTreeScene(new RewardTreePageContext(null, null, null, NavContentType.None));
	}

	private void TEST_DeckUpgradeComplete()
	{
		SceneLoader.GetSceneLoader().SceneLoaded -= TEST_DeckUpgradeComplete;
		OrbSlot[] array = _masteryPassProvider.GetOrbSlotMap(_activeView.TrackName).Values.ToArray();
		OrbSlot[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			_ = array2[i];
			int num = UnityEngine.Random.Range(0, array.Length);
			UpgradePacket upgradePacket = array[num].serverRewardNode.upgradePacket;
			if (upgradePacket != null)
			{
				DeckUpgrade.DisplayUpgrade(upgradePacket, "Blue Mastery 1", _cardDatabase, _cardViewBuilder, _cardMaterialBuilder, testing: true);
				return;
			}
		}
		DeckUpgrade.DisplayUpgrade(new UpgradePacket
		{
			cardsAdded = new List<uint> { 67726u, 67726u, 67726u, 69111u, 67756u, 67756u },
			targetDeckDescription = "Decks/Precon/Precon_July_W"
		}, "Blue Mastery 1", _cardDatabase, _cardViewBuilder, _cardMaterialBuilder);
	}

	public override void OnHandheldBackButton()
	{
		ClickBackButton();
	}
}
