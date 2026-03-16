using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Assets.Core.Meta.Utilities;
using Assets.Core.Shared.Code;
using Core.Code.Familiar;
using Core.Code.Input;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Challenge;
using Core.Meta.MainNavigation.Notifications;
using Core.Meta.MainNavigation.PopUps;
using Core.Meta.MainNavigation.Store;
using Core.Meta.NewPlayerExperience.Graph;
using Core.Meta.Quests;
using Core.Shared.Code.ClientModels;
using MTGA.KeyboardManager;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Arena.Enums.Store;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Models;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.PlayBlade;
using Wizards.Unification.Models.Events;
using Wizards.Unification.Models.PlayBlade;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;
using Wotc.Mtga.Wrapper;

public class HomePageContentController : NavContentController
{
	private ContentControllerRewards RewardsPanel;

	[SerializeField]
	private Animator _homeBillboardAnimator;

	public GameObject PlaybladeParentGO;

	public MainButton MainButton;

	public Animator HomeAnimator;

	public Transform RightBillboardContainer;

	public HomePageBillboard BillboardPrefab;

	[SerializeField]
	private int maxBillboards = 3;

	public HomeCarouselController CarouselController;

	private PopupNotificationManager _popupNotificationManager;

	private PlayBladeConfigDataProvider _playBladeConfigProvider;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	private HomePageContext Context;

	private Promise<PlayerProgressDailyWeekly> _dailyWeeklyQuestPromise;

	private List<HomePageBillboard> _rightBillboards = new List<HomePageBillboard>();

	private bool _waitingForEventData;

	private Queue<EventPayoutData> _eventPayout = new Queue<EventPayoutData>();

	private SeasonPayoutData _seasonPayoutData;

	private Coroutine _notificationPopups;

	private bool _active;

	private PlayBladeV3 _playBladeV3;

	private PlayBladeDataProvider _playBladeDataProvider;

	private IPlayBladeSelectionProvider _playBladeSelectionProvider;

	private PlayBladeConfigDataProvider _playBladeConfigDataProvider;

	private AssetLookupSystem _assetLookupSystem;

	private IAccountClient _accountClient;

	private QuestDataProvider _questDataProvider;

	private List<Client_QuestData> _npeQuests;

	private InventoryManager _invManager;

	private IInventoryServiceWrapper _inventoryServiceWrapper;

	private CosmeticsProvider _cosmeticsProvider;

	private IBILogger _biLogger;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private CardMaterialBuilder _cardMaterialBuilder;

	private RecentlyPlayedDataProvider _recentlyPlayedDataProvider;

	private EventManager _eventManager;

	private ICustomTokenProvider _customTokenProvider;

	private PopupManager _popupManager;

	private ViewedEventsDataProvider _viewedEventsDataProvider;

	private ISetMetadataProvider _setMetadataProvider;

	private PVPChallengeController _pvpChallengeController;

	public override NavContentType NavContentType => NavContentType.Home;

	public ContentControllerObjectives ObjectivesPanel { get; private set; }

	public HomePageState HomePageState { get; private set; }

	public bool IsEventBladeActive
	{
		get
		{
			if (_playBladeV3 != null)
			{
				return _playBladeV3.gameObject.activeInHierarchy;
			}
			return false;
		}
	}

	public PlayBladeController ChallengeBladeController { get; private set; }

	public override bool IsReadyToShow => !_waitingForEventData;

	public bool IsEventBladeDeckSelected()
	{
		return _playBladeSelectionProvider.IsEventBladeDeckSelected();
	}

	public void Init(ContentControllerObjectives objectivesPanel, ContentControllerRewards rewardsPanel, ISocialManager socialManager, AssetLookupSystem assetLookupSystem, IAccountClient accountClient, InventoryManager inventoryManager, KeyboardManager keyboardManager, IActionSystem actionSystem, CosmeticsProvider cosmetics, NavBarController navBar, IFrontDoorConnectionServiceWrapper frontDoorConnectionServiceWrapper, IBILogger biLogger, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, EventManager eventManager, Action<BladeEventInfo> playBladeQueueSelected, Action<BladeEventFilter> playBladeFilterSelected, ICustomTokenProvider customTokenProvider, PopupManager popupManager, ISetMetadataProvider setMetadataProvider)
	{
		_questDataProvider = Pantry.Get<QuestDataProvider>();
		_inventoryServiceWrapper = Pantry.Get<IInventoryServiceWrapper>();
		_popupNotificationManager = Pantry.Get<PopupNotificationManager>();
		_playBladeConfigProvider = Pantry.Get<PlayBladeConfigDataProvider>();
		_pvpChallengeController = Pantry.Get<PVPChallengeController>();
		_cosmeticsProvider = cosmetics;
		_biLogger = biLogger;
		_invManager = inventoryManager;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_cardMaterialBuilder = cardMaterialBuilder;
		_assetLookupSystem = assetLookupSystem;
		_accountClient = accountClient;
		_keyboardManager = keyboardManager;
		_actionSystem = actionSystem;
		_eventManager = eventManager;
		_customTokenProvider = customTokenProvider;
		_popupManager = popupManager;
		_setMetadataProvider = setMetadataProvider;
		ObjectivesPanel = objectivesPanel;
		RewardsPanel = rewardsPanel;
		InitPlayBlade(objectivesPanel, socialManager, playBladeQueueSelected, playBladeFilterSelected);
	}

	private void InitPlayBlade(ContentControllerObjectives objectivesPanel, ISocialManager socialManager, Action<BladeEventInfo> playBladeQueueSelected, Action<BladeEventFilter> playBladeFilterSelected)
	{
		string prefabPath = _assetLookupSystem.GetPrefabPath<PlayBladePrefab, PlayBladeController>();
		ChallengeBladeController = AssetLoader.Instantiate<PlayBladeController>(prefabPath, PlaybladeParentGO.transform);
		ChallengeBladeController.gameObject.SetActive(value: false);
		ChallengeBladeController.SetHomeAnimator(HomeAnimator);
		ChallengeBladeController.DeckSelector.Init(objectivesPanel, _assetLookupSystem, ChallengeBladeController);
		ChallengeBladeController.SetSocialClient(socialManager);
		ChallengeBladeController.OnPlayBladeVisibilityChanged += HandlePlayBladeVisibilityChanged;
		_recentlyPlayedDataProvider = Pantry.Get<RecentlyPlayedDataProvider>();
		_viewedEventsDataProvider = Pantry.Get<ViewedEventsDataProvider>();
		DeckDataProvider deckDataProvider = Pantry.Get<DeckDataProvider>();
		Pantry.Get<IColorChallengeStrategy>();
		_playBladeDataProvider = new PlayBladeDataProvider(_eventManager, _assetLookupSystem, deckDataProvider, Pantry.Get<IPlayerRankServiceWrapper>()?.CombinedRank, WrapperController.Instance.SparkyTourState, _recentlyPlayedDataProvider, _invManager.Inventory, _viewedEventsDataProvider);
		_playBladeSelectionProvider = Pantry.Get<IPlayBladeSelectionProvider>();
		_playBladeConfigDataProvider = Pantry.Get<PlayBladeConfigDataProvider>();
		string prefabPath2 = _assetLookupSystem.GetPrefabPath<PlayBladeV3Prefab, PlayBladeV3>();
		_playBladeV3 = AssetLoader.Instantiate<PlayBladeV3>(prefabPath2, PlaybladeParentGO.transform);
		_playBladeV3.Initialize(new MetaCardBuilder(WrapperController.Instance.CardViewBuilder), _playBladeSelectionProvider, JoinMatchMaking, EditDeck, GoToEventScreen, playBladeQueueSelected, playBladeFilterSelected, WrapperController.Instance.UnityObjectPool, _assetLookupSystem);
		_playBladeV3.gameObject.SetActive(value: false);
		_playBladeV3.OnVisibilityChanged += HandlePlayBladeVisibilityChanged;
	}

	public void UpdatePlaybladeNPEState(NPEOnboarding.EventTileVisuals newVisualState)
	{
	}

	private void HandlePlayBladeVisibilityChanged()
	{
		if (!IsEventBladeActive)
		{
			PlayBladeController challengeBladeController = ChallengeBladeController;
			if ((object)challengeBladeController != null && challengeBladeController.PlayBladeVisualState == PlayBladeController.PlayBladeVisualStates.Hidden)
			{
				CarouselController?.Resume();
				return;
			}
		}
		CarouselController?.Pause();
	}

	private void OnDestroy()
	{
		foreach (HomePageBillboard rightBillboard in _rightBillboards)
		{
			rightBillboard.GetComponent<CustomButton>().OnClick.RemoveAllListeners();
			rightBillboard.GetComponent<CustomButton>().OnMouseover.RemoveAllListeners();
		}
		ObjectivesPanel.OnBarFinishedAnimating -= OnObjectivesFinishedAnimating;
		ObjectivesPanel.OnQuestSwapClicked -= OnQuestSwapClicked;
		ObjectivesPanel.OnRewardTicked -= OnRewardTicked;
		if (RewardsPanel != null)
		{
			RewardsPanel.UnregisterRewardsWillCloseCallback(OnRewardsClicked);
		}
		if (_invManager != null)
		{
			_invManager.UnSubscribe(InventoryUpdateSource.MercantilePurchase, OnInventoryUpdated);
			_invManager.UnSubscribe(InventoryUpdateSource.OpenChest, OnInventoryUpdated);
			_invManager.UnSubscribe(InventoryUpdateSource.MercantileChestPurchase, OnInventoryUpdated);
		}
		_invManager = null;
		if (_playBladeV3 != null)
		{
			_playBladeV3.OnVisibilityChanged -= HandlePlayBladeVisibilityChanged;
		}
		if (ChallengeBladeController != null)
		{
			ChallengeBladeController.OnPlayBladeVisibilityChanged -= HandlePlayBladeVisibilityChanged;
		}
	}

	public override void Activate(bool active)
	{
		_active = active;
		ObjectivesPanel.Hide();
		if (active)
		{
			StartCoroutine(Coroutine_BeginLoad());
			WrapperController.Instance.DecksManager.GetAllDecks();
			if (Context != null && Context.OpenVault)
			{
				StartCoroutine(OpenVaultCoroutine());
			}
			_invManager.Subscribe(InventoryUpdateSource.MercantilePurchase, OnInventoryUpdated);
			_invManager.Subscribe(InventoryUpdateSource.OpenChest, OnInventoryUpdated);
			_invManager.Subscribe(InventoryUpdateSource.MercantileChestPurchase, OnInventoryUpdated);
			ObjectivesPanel.OnBarFinishedAnimating += OnObjectivesFinishedAnimating;
			ObjectivesPanel.OnRewardTicked += OnRewardTicked;
			ObjectivesPanel.OnQuestSwapClicked += OnQuestSwapClicked;
			if (!string.IsNullOrWhiteSpace(Context?.AFKDraft))
			{
				ShowKickedOutOfDraftPopup(Context.AFKDraft);
				Context.AFKDraft = null;
			}
		}
		else
		{
			ChallengeBladeController.gameObject.SetActive(value: false);
			RewardsPanel.Clear();
			ObjectivesPanel.OnBarFinishedAnimating -= OnObjectivesFinishedAnimating;
			ObjectivesPanel.OnRewardTicked -= OnRewardTicked;
			ObjectivesPanel.OnQuestSwapClicked -= OnQuestSwapClicked;
			if (_invManager != null)
			{
				_invManager.UnSubscribe(InventoryUpdateSource.MercantilePurchase, OnInventoryUpdated);
				_invManager.UnSubscribe(InventoryUpdateSource.OpenChest, OnInventoryUpdated);
				_invManager.UnSubscribe(InventoryUpdateSource.MercantileChestPurchase, OnInventoryUpdated);
			}
		}
		MainButton.gameObject.SetActive(active);
		HomePageState state = (active ? HomePageState.Normal : HomePageState.Inactive);
		UpdateHomePageState(state);
	}

	private void OnInventoryUpdated(ClientInventoryUpdateReportItem update)
	{
		CarouselController.Refresh();
	}

	private void ShowKickedOutOfDraftPopup(string eventName)
	{
		SystemMessageManager.Instance.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("Draft/RemovedForInactivity_Title"), Languages.ActiveLocProvider.GetLocalizedText("Draft/RemovedForInactivity_Description"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_Cancel"), null, Languages.ActiveLocProvider.GetLocalizedText("Draft/Autopicking_Button"), delegate
		{
			EventContext eventContext = _eventManager.EventContexts.FirstOrDefault((EventContext e) => e.PlayerEvent.EventInfo.InternalEventName == eventName);
			if (eventContext != null)
			{
				SceneLoader.GetSceneLoader().GoToEventScreen(eventContext);
			}
		});
	}

	public void SetContext(HomePageContext context)
	{
		Context = context;
	}

	private void OnRewardTicked(RewardObjectiveContext context)
	{
		RewardsPanel.OnRewardBubbleTicked(context, OnRewardsClicked);
	}

	private void OnQuestSwapClicked(Guid questId)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Quest/Confirm_Swap_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Quest/Confirm_Swap_Text"), showCancel: true, delegate
		{
			OnConfirmQuestSwap(questId);
		});
	}

	private void OnConfirmQuestSwap(Guid id)
	{
		StartCoroutine(Coroutine_SwapQuest(id));
	}

	private void OnConfirmQuestRefresh()
	{
		StartCoroutine(Coroutine_GetNewQuests(isReturningFromGame: false));
	}

	private IEnumerator Coroutine_GetNewQuests(bool isReturningFromGame)
	{
		yield return _questDataProvider.RefreshQuestData();
		yield return _questDataProvider.RefreshDailyWeeklyQuests();
		_npeQuests = null;
		List<Client_QuestData> allQuests = GetAllQuests();
		yield return Coroutine_ShowQuestBar(allQuests, isReturningFromGame);
	}

	private List<Client_QuestData> GetAllQuests()
	{
		List<Client_QuestData> quests = _questDataProvider.GetQuests();
		if (_npeQuests != null)
		{
			quests.AddRange(_npeQuests);
		}
		return quests;
	}

	public override void OnFinishOpen()
	{
		if (Context != null && Context.PostMatchContext?.PostMatchClientUpdate != null)
		{
			_questDataProvider.UpdateQuestsFromPostMatch(Context.PostMatchContext?.PostMatchClientUpdate);
		}
		List<Client_QuestData> allQuests = GetAllQuests();
		ShowNotificationPopups();
		StartCoroutine(Coroutine_ShowQuestBar(allQuests, Context != null && Context.PostMatchContext != null));
		List<KeyValuePair<Guid, PVPChallengeData>> list = _pvpChallengeController.GetAllChallenges()?.Where((KeyValuePair<Guid, PVPChallengeData> challenge) => challenge.Value.ChallengePlayers.ContainsKey(challenge.Value.LocalPlayerId)).ToList();
		if (list != null && list.Any())
		{
			ChallengeBladeController.ViewFriendChallenge(list[0].Value.ChallengeId);
		}
		WrapperController.Instance.SceneLoader.SpawnNPEOnboarding();
	}

	private void OnObjectivesFinishedAnimating()
	{
		WrapperController.Instance.PostMatchClientUpdate = null;
		SceneLoader.GetSceneLoader().GetNavBar().RefreshCodexNewPip();
	}

	public void OnRightBillboardHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	public void OnPlayHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover_big, base.gameObject);
	}

	private void OnRewardsClicked()
	{
	}

	public void OnPlayButton()
	{
		ObjectivesPanel?.CloseAllObjectivePopups();
		_playBladeV3.Show();
	}

	public void OnInboxOpened()
	{
		ObjectivesPanel?.CloseAllObjectivePopups();
		HidePlayblade();
	}

	public override void OnBeginClose()
	{
		if ((bool)_playBladeV3)
		{
			_playBladeV3.Hide(doMainNavAudio: false);
		}
		ChallengeBladeController.Hide();
		base.OnBeginClose();
	}

	public void HidePlayblade()
	{
		if ((bool)_playBladeV3)
		{
			_playBladeV3.Hide(doMainNavAudio: true);
		}
		if (ChallengeBladeController.PlayBladeVisualState != PlayBladeController.PlayBladeVisualStates.Hidden)
		{
			ChallengeBladeController.Hide();
		}
	}

	private void ClearBillboards()
	{
		foreach (HomePageBillboard rightBillboard in _rightBillboards)
		{
			UnityEngine.Object.Destroy(rightBillboard.gameObject);
		}
		_rightBillboards.Clear();
	}

	private void RenderBillboardEvents()
	{
		ClearBillboards();
		List<EventContext> eventContexts = _eventManager.EventContexts;
		ClientPlayerInventory inv = WrapperController.Instance.InventoryManager.Inventory;
		List<PlayBladeQueueEntry> playBladeConfig = _playBladeConfigProvider.GetPlayBladeConfig();
		List<Client_CustomTokenDefinitionWithQty> customTokensOfTypeWithQty = _customTokenProvider.GetCustomTokensOfTypeWithQty(ClientTokenType.Event);
		List<EventContext> orderedEventContexts = (from x in eventContexts
			where x.PlayerEvent.ShowInPlayblade(inv)
			where x.PlayerEvent.EventUXInfo.DisplayPriority >= 0 && !string.IsNullOrEmpty(x.PlayerEvent.EventUXInfo.PublicEventName)
			where x.PlayerEvent.EventInfo.EventState == MDNEventState.ForceActive || x.PlayerEvent.CourseData.CurrentModule != PlayerEventModule.Join || !(x.PlayerEvent.EventInfo.LockedTime <= ServerGameTime.GameTime)
			where x.PlayerEvent.EventUXInfo.HasEventPage || playBladeConfig.Exists((PlayBladeQueueEntry b) => b.EventNameBO1 == x.PlayerEvent.EventInfo.InternalEventName || b.EventNameBO3 == x.PlayerEvent.EventInfo.InternalEventName)
			select x).ToList();
		List<ClientDynamicFilterTag> orderedFilterTags = (from x in _eventManager.DynamicFilterTags
			where x.BillboardPriority > 0
			where _playBladeV3.GetFilterForID(x.TagId) != null
			select x).ToList();
		List<BillboardData> priorityBillboards = EventHelper.GetPriorityBillboards(orderedEventContexts, orderedFilterTags, customTokensOfTypeWithQty, maxBillboards);
		CombinedRankInfo combinedRank = Pantry.Get<IPlayerRankServiceWrapper>().CombinedRank;
		foreach (BillboardData item in priorityBillboards)
		{
			HomePageBillboard homePageBillboard = UnityEngine.Object.Instantiate(BillboardPrefab, RightBillboardContainer);
			_rightBillboards.Add(homePageBillboard);
			homePageBillboard.SetEvent(_assetLookupSystem, item, _playBladeV3, combinedRank);
		}
	}

	private IEnumerator Coroutine_ShowQuestBar(List<Client_QuestData> quests, bool isReturningFromGame)
	{
		if (_dailyWeeklyQuestPromise == null)
		{
			_dailyWeeklyQuestPromise = _questDataProvider.RefreshDailyWeeklyQuests();
		}
		yield return _dailyWeeklyQuestPromise.AsCoroutine();
		PlayerProgressDailyWeekly dailyWeeklyProgress = _questDataProvider.GetDailyWeeklyProgress();
		if (dailyWeeklyProgress == null)
		{
			Debug.LogWarning("No Daily/Weekly progress.");
		}
		int num = dailyWeeklyProgress?.dailySequence ?? 0;
		int num2 = dailyWeeklyProgress?.weeklySequence ?? 0;
		bool flag = Context != null && (Context.PostMatchContext == null || Context.PostMatchContext.MatchesOfThisEventTypeCanAffectDailyWeeklyWins);
		bool flag2 = flag && num != 0;
		bool flag3 = flag && num2 != 0;
		int num3 = ((Context != null && Context.PostMatchContext != null && Context.PostMatchContext.GamesWon > 0) ? Context.PostMatchContext.GamesWon : 0);
		bool flag4 = isReturningFromGame && num3 > 0;
		RewardScheduleIntermediate rewardSchedule = WrapperController.Instance.RewardSchedule;
		(DailyWeeklyReward, bool) currentReward = RewardScheduleUtils.GetCurrentReward(rewardSchedule.dailyRewards, num, num3);
		DailyWeeklyReward nextReward = RewardScheduleUtils.GetNextReward(rewardSchedule.dailyRewards, num);
		DailyWeeklyReward lastReward = RewardScheduleUtils.GetLastReward(rewardSchedule.dailyRewards);
		(DailyWeeklyReward, bool) currentReward2 = RewardScheduleUtils.GetCurrentReward(rewardSchedule.weeklyRewards, num2, num3);
		DailyWeeklyReward nextReward2 = RewardScheduleUtils.GetNextReward(rewardSchedule.weeklyRewards, num2);
		DailyWeeklyReward lastReward2 = RewardScheduleUtils.GetLastReward(rewardSchedule.weeklyRewards);
		if (quests != null)
		{
			bool flag5 = quests.Where((Client_QuestData q) => q.EndingProgress > q.StartingProgress).ToList().Count > 0 || (flag4 && Context.PostMatchContext != null && Context.PostMatchContext.MatchesOfThisEventTypeCanAffectDailyWeeklyWins && (currentReward.Item1.wins > 0 || currentReward2.Item1.wins > 0));
			RewardDisplayData dailyReward = TempRewardTranslation.ChestDescriptionToDisplayData(((flag4 && flag2) ? nextReward.awardDescription : currentReward.Item1.awardDescription) ?? nextReward.awardDescription, _cardDatabase.CardDataProvider, _cardMaterialBuilder);
			RewardDisplayData weeklyReward = TempRewardTranslation.ChestDescriptionToDisplayData(((flag4 && flag3) ? nextReward2.awardDescription : currentReward2.Item1.awardDescription) ?? nextReward2.awardDescription, _cardDatabase.CardDataProvider, _cardMaterialBuilder);
			ObjectivesPanel.ShowQuestBar(quests.ToList(), quests.Select((Client_QuestData q) => new RewardDisplayData(q.Reward, _cardDatabase.CardDataProvider, _cardMaterialBuilder)).ToList(), dailyReward, weeklyReward, num, lastReward.wins, num2, lastReward2.wins, flag5, isReturningFromGame ? num3 : 0, flag, OnConfirmQuestRefresh);
			if (isReturningFromGame && !flag5)
			{
				OnObjectivesFinishedAnimating();
			}
		}
		if (Context != null && Context.InitialBladeState == PlayBladeController.PlayBladeVisualStates.Challenge)
		{
			ObjectivesPanel.AnimateOutro();
		}
	}

	public void ShowBladeAndSelect(string publicEventName)
	{
		EventContext eventContext = _eventManager.EventContexts.FirstOrDefault((EventContext evt) => evt.PlayerEvent.EventUXInfo.PublicEventName == publicEventName);
		if (eventContext == null)
		{
			SimpleLog.LogError("Could not find event context by public name: " + publicEventName);
			return;
		}
		PlayBladeQueueEntry playBladeQueueEntry = _playBladeConfigDataProvider.GetPlayBladeConfig().FirstOrDefault((PlayBladeQueueEntry c) => c.EventNameBO1 == eventContext.PlayerEvent.EventInfo.InternalEventName || c.EventNameBO3 == eventContext.PlayerEvent.EventInfo.InternalEventName);
		if (playBladeQueueEntry == null)
		{
			SimpleLog.LogError("Could not find PlayBlade queue for event: " + eventContext.PlayerEvent.EventInfo.InternalEventName);
			return;
		}
		BladeSelectionData selection = _playBladeSelectionProvider.GetSelection();
		selection.bladeType = Wizards.Mtga.PlayBlade.BladeType.FindMatch;
		selection.findMatch.DeckId = Guid.Empty;
		selection.findMatch.UseBO3 = false;
		selection.findMatch.QueueType = playBladeQueueEntry.QueueType;
		selection.findMatch.QueueId = playBladeQueueEntry.Id;
		if (selection.findMatch.QueueIdForQueueType == null)
		{
			selection.findMatch.QueueIdForQueueType = new Dictionary<PlayBladeQueueType, string>();
		}
		selection.findMatch.QueueIdForQueueType[playBladeQueueEntry.QueueType] = playBladeQueueEntry.Id;
		_playBladeSelectionProvider.SetSelection(selection);
		_playBladeV3.Show();
	}

	public void ShowPlayBladeEventsAndFilter(string dynamicFilter)
	{
		_playBladeV3.ShowEventsTabAndFilter(dynamicFilter);
	}

	public void UpdateHomePageState(HomePageState state)
	{
		if (!base.gameObject.activeSelf)
		{
			HomePageState = HomePageState.Inactive;
			ObjectivesPanel.gameObject.SetActive(value: false);
			return;
		}
		HomePageState = state;
		ObjectivesPanel.SetInteractable(HomePageState == HomePageState.Normal);
		SceneLoader.GetSceneLoader().SetSocialVisible(HomePageState != HomePageState.NotificationFlow);
		MainButton.gameObject.UpdateActive(_active);
		MainButton.SetVisualState();
	}

	private void ShowNotificationPopups()
	{
		if (base.gameObject.activeSelf)
		{
			if (_notificationPopups != null)
			{
				StopCoroutine(_notificationPopups);
			}
			_notificationPopups = StartCoroutine(Coroutine_NotificationPopups());
			UpdateHomePageState(HomePageState.NotificationFlow);
		}
	}

	public override void OnNavBarScreenChange(Action screenChangeAction)
	{
		PVPChallengeData pVPChallengeData = _pvpChallengeController?.GetActiveCurrentChallengeData();
		if (pVPChallengeData != null)
		{
			_pvpChallengeController.LeaveChallenge(pVPChallengeData.ChallengeId, confirm: true, screenChangeAction);
		}
		else
		{
			screenChangeAction();
		}
	}

	public bool HasChallengeScreenOpen()
	{
		return _pvpChallengeController?.GetActiveCurrentChallengeData() != null;
	}

	private Promise<NewMatchCreatedConfig> GetActiveMatch()
	{
		return Pantry.Get<IActiveMatchesServiceWrapper>().GetActiveMatches().Convert((List<NewMatchCreatedConfig> p) => p?.FirstOrDefault());
	}

	private void ReconnectToMatch(NewMatchCreatedConfig activeMatch)
	{
		WrapperController.EnableLoadingIndicator(enabled: true);
		string eventId = activeMatch.eventId;
		Matchmaking matchmaking = Pantry.Get<Matchmaking>();
		EventContext eventContext = Pantry.Get<EventManager>().EventContexts.FirstOrDefault((EventContext e) => e.PlayerEvent.MatchMakingName == eventId);
		matchmaking.LaunchFromReconnect();
		WrapperController.EnableLoadingIndicator(enabled: false);
		matchmaking.JoinMatchFromReconnect(activeMatch, eventContext);
	}

	private IEnumerator Coroutine_SwapQuest(Guid id)
	{
		ObjectivesPanel.SetInteractable(interactable: false);
		List<Client_QuestData> oldQuests = GetAllQuests();
		yield return _questDataProvider.SwapPlayerQuest(id.ToString()).IfSuccess(delegate(Promise<List<Client_QuestData>> p)
		{
			MainThreadDispatcher.Instance.Add(delegate
			{
				handleQuestsSuccess(id, p, oldQuests);
			});
		});
		ObjectivesPanel.SetInteractable(interactable: true);
	}

	private void handleQuestsSuccess(Guid id, Promise<List<Client_QuestData>> promise, List<Client_QuestData> oldQuests)
	{
		Client_QuestData client_QuestData = promise.Result.FirstOrDefault((Client_QuestData newQ) => !oldQuests.Exists((Client_QuestData oldQ) => oldQ.Id == newQ.Id));
		if (client_QuestData != null)
		{
			ObjectivesPanel.ReplaceQuest(id, client_QuestData, new RewardDisplayData(client_QuestData.Reward, _cardDatabase.CardDataProvider, _cardMaterialBuilder));
		}
	}

	private IEnumerator Coroutine_BeginLoad()
	{
		_waitingForEventData = true;
		StartCoroutine(_eventManager.Coroutine_GetEventsAndCourses());
		yield return new WaitUntil(() => !_eventManager.RefreshingEventContexts);
		if (!ChallengeBladeController.IsInitialized)
		{
			ChallengeBladeController.Initialize(ObjectivesPanel, _keyboardManager, _actionSystem, _assetLookupSystem, _cardViewBuilder);
		}
		IPlayerRankServiceWrapper rankInfo = Pantry.Get<IPlayerRankServiceWrapper>();
		_playBladeDataProvider.SetRankInfo(rankInfo?.CombinedRank);
		BladeData bladeData = _playBladeDataProvider.GetBladeData();
		yield return new WaitUntil(() => bladeData.Initialized);
		_playBladeV3.SetData(bladeData);
		if (Context != null)
		{
			switch (Context.InitialBladeState)
			{
			case PlayBladeController.PlayBladeVisualStates.Challenge:
				ObjectivesPanel.AnimateOutro();
				break;
			case PlayBladeController.PlayBladeVisualStates.Events:
				_playBladeV3.Show();
				break;
			}
		}
		yield return _questDataProvider.RefreshQuestData().AsCoroutine();
		CarouselController.Refresh();
		if (_dailyWeeklyQuestPromise == null)
		{
			_dailyWeeklyQuestPromise = _questDataProvider.RefreshDailyWeeklyQuests();
		}
		yield return _dailyWeeklyQuestPromise.AsCoroutine();
		_npeQuests = null;
		if (rankInfo != null)
		{
			Promise<EventAndSeasonPayouts> eventAndSeasonPayoutsPromise = rankInfo.GetEventAndSeasonPayouts();
			_seasonPayoutData = null;
			yield return eventAndSeasonPayoutsPromise.AsCoroutine();
			if (eventAndSeasonPayoutsPromise.Successful && eventAndSeasonPayoutsPromise.Result != null)
			{
				foreach (EventPayoutData eventPayout in eventAndSeasonPayoutsPromise.Result.eventPayouts)
				{
					_eventPayout.Enqueue(eventPayout);
				}
				SeasonPayoutData seasonPayout = eventAndSeasonPayoutsPromise.Result.seasonPayout;
				if (seasonPayout != null)
				{
					_seasonPayoutData = seasonPayout;
					Promise<CombinedRankInfo> rankHandle = rankInfo.GetPlayerRankInfo().IfSuccess(delegate(Promise<CombinedRankInfo> promise)
					{
						MainThreadDispatcher.Instance.Add(delegate
						{
							Pantry.Get<IPlayerRankServiceWrapper>().CombinedRank = promise.Result;
						});
					});
					SeasonAndRankDataProvider seasonAndRankDataProvider = Pantry.Get<SeasonAndRankDataProvider>();
					Promise<Client_SeasonAndRankInfo> seasonHandle = seasonAndRankDataProvider.Refresh();
					if (seasonPayout.inventoryInfoWithNoDeltas != null)
					{
						_inventoryServiceWrapper.OnInventoryInfoUpdated_AWS(seasonPayout.inventoryInfoWithNoDeltas);
					}
					yield return new WaitUntil(() => rankHandle.IsDone && seasonHandle.IsDone);
				}
			}
		}
		RenderBillboardEvents();
		_waitingForEventData = false;
	}

	private IEnumerator OpenVaultCoroutine()
	{
		ClientInventoryUpdateReportItem vaultInventoryUpdate = null;
		Action<ClientInventoryUpdateReportItem> onVaultInventoryUpdated = delegate(ClientInventoryUpdateReportItem u)
		{
			vaultInventoryUpdate = u;
		};
		WrapperController.Instance.InventoryManager.Subscribe(InventoryUpdateSource.CompleteVault, onVaultInventoryUpdated);
		Promise<bool> promise = _inventoryServiceWrapper.CompleteVault();
		yield return promise.AsCoroutine();
		yield return new WaitUntil(() => vaultInventoryUpdate != null);
		WrapperController.Instance.InventoryManager.UnSubscribe(InventoryUpdateSource.CompleteVault, onVaultInventoryUpdated);
		if (promise.Successful)
		{
			yield return RewardsPanel.AddAndDisplayRewardsCoroutine(vaultInventoryUpdate, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Landing/Vault_Rewards_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
		}
	}

	private IEnumerator Coroutine_NotificationPopups()
	{
		Debug.Log("BEGIN home page notification flow");
		yield return Coroutine_NotificationPopups_ClaimVouchers();
		yield return Coroutine_NotificationPopups_SeasonPayouts();
		yield return Coroutine_NotificationPopups_SparkRankUnlocked();
		yield return Coroutine_NotificationPopups_EventPayout();
		AccountInformation account = _accountClient.AccountInformation;
		yield return Coroutine_NotificationPopups_MythicQualify(account);
		yield return Coroutine_NotificationPopups_BannedCardAnnounce();
		yield return Coroutine_NotificationPopups_SetAnnounce();
		yield return Coroutine_NotificationPopups_MoZTutorial();
		RenewalManager renewalManager = WrapperController.Instance.RenewalManager;
		yield return Coroutine_NotificationPopups_RotationPreview(renewalManager, account);
		yield return Coroutine_NotificationPopups_LoginGrants();
		yield return Coroutine_NotificationPopups_CampaignGraphAutomaticPayouts();
		yield return Coroutine_NotificationPopups_RenewalPopup(renewalManager);
		yield return StartCoroutine(_popupNotificationManager.ShowPopupsCoroutine());
		Debug.Log("END home page notification flow");
		_notificationPopups = null;
		UpdateHomePageState(HomePageState.Normal);
	}

	private IEnumerator Coroutine_NotificationPopups_ClaimVouchers()
	{
		List<ClientInventoryUpdateReportItem> ts = WrapperController.Instance.InventoryManager.FetchExistingUpdates(InventoryUpdateSource.RedeemVoucher);
		if (WrapperController.Instance.DebugFlag.VouchersPopup && ts.Count == 0)
		{
			ts.Add(RenewalPopup.TEST_CreateTestInventoryUpdate());
		}
		bool redeemVoucherDismiss;
		if (ts.Count > 0)
		{
			redeemVoucherDismiss = false;
			RewardsPanel.RegisterRewardWillCloseCallback(DismissRedeemVoucherPresentation);
			yield return RewardsPanel.AddAndDisplayRewardsCoroutine(ts, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/PreorderRewards_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
			yield return new WaitUntil(() => redeemVoucherDismiss);
		}
		void DismissRedeemVoucherPresentation()
		{
			redeemVoucherDismiss = true;
		}
	}

	private IEnumerator Coroutine_NotificationPopups_SeasonPayouts()
	{
		if (WrapperController.Instance.DebugFlag.SeasonPayoutPopup && _seasonPayoutData == null)
		{
			_seasonPayoutData = ContentControllerRewardsTestUtils.TEST_CreateSeasonPayoutData();
		}
		if (_seasonPayoutData != null)
		{
			bool dismissed = false;
			RewardsPanel.RegisterRewardWillCloseCallback(delegate
			{
				dismissed = true;
			});
			yield return RewardsPanel.DisplayEndOfSeasonCoroutine(_seasonPayoutData);
			_seasonPayoutData = null;
			yield return new WaitUntil(() => dismissed);
		}
	}

	private IEnumerator Coroutine_NotificationPopups_SparkRankUnlocked()
	{
		if (MDNPlayerPrefs.GetSparkRankRewardShown(_accountClient.AccountInformation?.AccountID))
		{
			yield break;
		}
		Task<bool> sparkQueueOpenedTask = Pantry.Get<NewPlayerExperienceStrategy>().SparkQueueOpened;
		yield return new WaitUntil(() => sparkQueueOpenedTask.IsCompleted);
		if (!sparkQueueOpenedTask.Result)
		{
			yield break;
		}
		IPlayerRankServiceWrapper playerRankServiceWrapper = Pantry.Get<IPlayerRankServiceWrapper>();
		CombinedRankInfo combinedRankInfo = playerRankServiceWrapper?.CombinedRank;
		if (combinedRankInfo == null && playerRankServiceWrapper != null)
		{
			Promise<CombinedRankInfo> playerRankPromise = playerRankServiceWrapper.GetPlayerRankInfo();
			yield return new WaitUntil(() => playerRankPromise.IsDone);
			combinedRankInfo = (playerRankPromise.Successful ? playerRankPromise.Result : null);
		}
		if (combinedRankInfo == null)
		{
			yield break;
		}
		MDNPlayerPrefs.SetSparkRankRewardShown(_accountClient.AccountInformation.AccountID, newValue: true);
		if (combinedRankInfo.constructedClass == RankingClassType.Spark)
		{
			yield return RewardsPanel.DisplaySparkRankUnlockCoroutine();
			yield return new WaitUntil(() => !RewardsPanel.Visible);
		}
	}

	private IEnumerator Coroutine_NotificationPopups_EventPayout()
	{
		if (WrapperController.Instance.DebugFlag.EventPayoutPopup && _eventPayout.Count == 0)
		{
			_eventPayout.Enqueue(ContentControllerRewardsTestUtils.TEST_CreateEventPayoutData());
		}
		while (_eventPayout.Count > 0)
		{
			bool dismissed = false;
			EventPayoutData publicEvent = _eventPayout.Dequeue();
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText("Events/Event_Title_" + publicEvent.PublicEventName);
			string localizedText2 = Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Event_Payout", ("publicEventName", localizedText));
			SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("Events/Event_Completed"), localizedText2, showCancel: false, delegate
			{
				dismissed = true;
			});
			yield return new WaitUntil(() => dismissed);
			if (publicEvent.delta != null && !publicEvent.delta.All((ClientInventoryUpdateReportItem _) => _.IsEmpty()))
			{
				RewardsPanel.RegisterRewardWillCloseCallback(delegate
				{
					dismissed = true;
				});
				yield return RewardsPanel.AddAndDisplayRewardsCoroutine(publicEvent.delta, Languages.ActiveLocProvider.GetLocalizedText("Events/Event_Completed"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
			}
			yield return new WaitUntil(() => !RewardsPanel.Visible);
		}
	}

	private IEnumerator Coroutine_NotificationPopups_MythicQualify(AccountInformation account)
	{
		if (account == null)
		{
			yield break;
		}
		bool flag = account.HasRole_InvitationalQualified();
		bool shownQualifierBadge = MDNPlayerPrefs.GetShownQualifierBadge(account.PersonaID);
		if ((flag && !shownQualifierBadge) || WrapperController.Instance.DebugFlag.MythicQualifyPopup)
		{
			bool dismissed = false;
			RewardsPanel.RegisterRewardWillCloseCallback(delegate
			{
				dismissed = true;
			});
			yield return RewardsPanel.AddMythicQualifierBadgeCoroutine();
			yield return RewardsPanel.AddAndDisplayRewardsCoroutine(Array.Empty<ClientInventoryUpdateReportItem>(), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/MythicQualifier_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/MythicQualifier_Subtitle"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Vault/DraftProgressOkay"));
			MDNPlayerPrefs.SetShownQualifierBadge(account.PersonaID, newValue: true);
			yield return new WaitUntil(() => dismissed);
		}
		else if (!flag && shownQualifierBadge)
		{
			MDNPlayerPrefs.SetShownQualifierBadge(account.PersonaID, newValue: false);
		}
	}

	private IEnumerator Coroutine_NotificationPopups_BannedCardAnnounce()
	{
		if (!WrapperController.Instance.DebugFlag.BannedPopup && !WrapperController.Instance.SparkyTourState.BannedCardsPopupUnlocked)
		{
			yield break;
		}
		BannedCardPopup bannedCardPopup = SceneLoader.GetSceneLoader().GetBannedCardPopup();
		if (!(bannedCardPopup != null))
		{
			yield break;
		}
		bool dismissed = false;
		bannedCardPopup.Setup(delegate
		{
			dismissed = true;
		});
		if (WrapperController.Instance.DebugFlag.BannedPopup || bannedCardPopup.ShouldBeShown)
		{
			bannedCardPopup.Activate(activate: true);
			_popupManager.RegisterPopup(bannedCardPopup);
			yield return new WaitUntil(() => dismissed);
			_popupManager.UnregisterPopup(bannedCardPopup);
		}
		SceneLoader.GetSceneLoader().DestroyPopup<BannedCardPopup>();
	}

	private IEnumerator Coroutine_NotificationPopups_SetAnnounce()
	{
		bool num = RemoteSettings.GetBool("ui.SetAnnouncement.enabled", defaultValue: true);
		bool flag = SceneLoader.GetSceneLoader().HasSeenNewSetAnnouncement();
		bool setAnnounceTrailerUnlocked = WrapperController.Instance.SparkyTourState.SetAnnounceTrailerUnlocked;
		if ((num && !flag && setAnnounceTrailerUnlocked) || WrapperController.Instance.DebugFlag.SetAnnouncePopup)
		{
			SetAnnouncementController setAnnouncement = SceneLoader.GetSceneLoader().GetSetAnnouncementController();
			if (_setMetadataProvider.IsSetPublished(setAnnouncement.NewSetId.GetName()))
			{
				setAnnouncement.Activate(activate: true);
				_popupManager.RegisterPopup(setAnnouncement);
				yield return new WaitUntil(() => !setAnnouncement.IsShowing);
				setAnnouncement.SetAnnouncmentSeen();
				_popupManager.UnregisterPopup(setAnnouncement);
			}
			SceneLoader.GetSceneLoader().DestroyPopup<SetAnnouncementController>();
		}
		else
		{
			SceneLoader.GetSceneLoader().DestroyPopup<SetAnnouncementController>();
		}
	}

	private IEnumerator Coroutine_NotificationPopups_MoZTutorial()
	{
		if ((SceneLoader.GetSceneLoader().HasSeenMOZTutorialPopup() || !PlatformUtils.IsHandheld()) && !WrapperController.Instance.DebugFlag.MOZTutorialPopup)
		{
			yield break;
		}
		MOZTutorialPopup mOZTutorialPopup = SceneLoader.GetSceneLoader().GetMOZTutorialPopup();
		if (mOZTutorialPopup != null)
		{
			bool dismissed = false;
			mOZTutorialPopup.Init(WrapperController.Instance.CardViewBuilder, WrapperController.Instance.CardDatabase, _accountClient.AccountInformation?.PersonaID, delegate
			{
				dismissed = true;
			});
			mOZTutorialPopup.Activate(activate: true);
			yield return new WaitUntil(() => dismissed);
			SceneLoader.GetSceneLoader().DestroyPopup<MOZTutorialPopup>();
		}
	}

	private IEnumerator Coroutine_NotificationPopups_RotationPreview(RenewalManager renewalManager, AccountInformation account)
	{
		if (!ShouldShowRotationNotificationPopup(renewalManager, account))
		{
			yield break;
		}
		RotationPreviewPopup rotationPreview = SceneLoader.GetSceneLoader().GetRotationPreviewPopup();
		rotationPreview.Init(delegate
		{
			MDNPlayerPrefs.SetRotationEducationViewed(account.PersonaID, renewalManager.GetCurrentRenewalId(), value: true);
		});
		yield return new WaitUntil(() => !rotationPreview.IsShowing);
		SceneLoader.GetSceneLoader().DestroyPopup<RotationPreviewPopup>();
		if (renewalManager.IsUpcomingRenewalAvailable())
		{
			RenewalPreviewPopup renewalPreview = SceneLoader.GetSceneLoader().GetRenewalPreviewPopup();
			renewalPreview.Init();
			yield return new WaitUntil(() => !renewalPreview.IsShowing);
			SceneLoader.GetSceneLoader().DestroyPopup<RenewalPreviewPopup>();
		}
	}

	private bool ShouldShowRotationNotificationPopup(RenewalManager renewalManager, AccountInformation account)
	{
		if (account == null)
		{
			return false;
		}
		if (renewalManager.IsActiveRenewalAvailable())
		{
			return true;
		}
		if (renewalManager.IsCurrentRenewalUpcoming() && !MDNPlayerPrefs.GetRotationEducationViewed(account.PersonaID, renewalManager.GetCurrentRenewalId()))
		{
			return true;
		}
		return false;
	}

	private IEnumerator Coroutine_NotificationPopups_LoginGrants()
	{
		List<ClientInventoryUpdateReportItem> loginGrantInventoryUpdates = WrapperController.Instance.InventoryManager.FetchExistingUpdates(InventoryUpdateSource.LoginGrant);
		if (WrapperController.Instance.DebugFlag.LoginGrantPopup && loginGrantInventoryUpdates.Count == 0)
		{
			loginGrantInventoryUpdates.Add(RenewalPopup.TEST_CreateTestInventoryUpdate());
		}
		while (loginGrantInventoryUpdates.Count > 0)
		{
			bool loginGrantDismissed = false;
			ClientInventoryUpdateReportItem t = loginGrantInventoryUpdates[0];
			loginGrantInventoryUpdates.RemoveAt(0);
			RewardsPanel.RegisterRewardWillCloseCallback(DismissLoginGrantPresentation);
			yield return RewardsPanel.AddAndDisplayRewardsCoroutine(t, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
			yield return new WaitUntil(() => loginGrantDismissed);
			void DismissLoginGrantPresentation()
			{
				loginGrantDismissed = true;
			}
		}
	}

	private IEnumerator Coroutine_NotificationPopups_CampaignGraphAutomaticPayouts()
	{
		List<ClientInventoryUpdateReportItem> campaignGraphAutomaticPayoutsInventoryUpdates = WrapperController.Instance.InventoryManager.FetchExistingUpdates(InventoryUpdateSource.CampaignGraphAutomaticPayoutNode);
		while (campaignGraphAutomaticPayoutsInventoryUpdates.Count > 0)
		{
			bool campaignGraphAutomaticPayoutDismissed = false;
			ClientInventoryUpdateReportItem t = campaignGraphAutomaticPayoutsInventoryUpdates[0];
			campaignGraphAutomaticPayoutsInventoryUpdates.RemoveAt(0);
			RewardsPanel.RegisterRewardWillCloseCallback(DismissCampaignGraphAutomaticPayoutsPresentation);
			yield return RewardsPanel.AddAndDisplayRewardsCoroutine(t, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title_StoreAcquired"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
			yield return new WaitUntil(() => campaignGraphAutomaticPayoutDismissed);
			void DismissCampaignGraphAutomaticPayoutsPresentation()
			{
				campaignGraphAutomaticPayoutDismissed = true;
			}
		}
	}

	private IEnumerator Coroutine_NotificationPopups_RenewalPopup(RenewalManager renewalManager)
	{
		if (renewalManager.IsActiveRenewalAvailable())
		{
			RenewalPopup renewalPopup = SceneLoader.GetSceneLoader().GetRenewalPopup();
			renewalPopup.Init(_assetLookupSystem, _cardDatabase, _cardViewBuilder, null, _keyboardManager);
			yield return new WaitUntil(() => !renewalPopup.IsShowing);
			SceneLoader.GetSceneLoader().DestroyPopup<RenewalPopup>();
		}
	}

	private void JoinMatchMaking(string internalEventName, Guid deckId)
	{
		DeckDataProvider deckDataProvider = Pantry.Get<DeckDataProvider>();
		Client_Deck deck = deckDataProvider.GetDeckForId(deckId);
		if (deck == null)
		{
			return;
		}
		EventContext eventContext = null;
		EAiBotMatchType eAiBotMatchType = EAiBotMatchTypeUtil.GetBotMatchType(internalEventName);
		Matchmaking matchmaking = WrapperController.Instance.Matchmaking;
		DecksManager deckManager = WrapperController.Instance.DecksManager;
		if (eAiBotMatchType != EAiBotMatchType.Unknown)
		{
			if (Pantry.Get<FormatManager>().GetSafeFormat(deck.Summary.Format).UseRebalancedCards)
			{
				eAiBotMatchType = EAiBotMatchType.Rebalanced;
			}
			eventContext = _eventManager.GetEventContext(eAiBotMatchType.GetEventName());
			BotTool botTool = Pantry.Get<BotTool>();
			IBotMatchServiceWrapper botMatchServiceWrapper = Pantry.Get<IBotMatchServiceWrapper>();
			PAPA.StartGlobalCoroutine(Coroutine_PlayBotGame(botTool, matchmaking, deckManager, botMatchServiceWrapper, deckDataProvider, eventContext, deck));
			_recentlyPlayedDataProvider.AddRecentlyPlayedGame(eventContext.PlayerEvent.EventInfo.EventId, deckId);
			AudioManager.PlayAudio(WwiseEvents.match_making_find_match, base.gameObject);
			return;
		}
		GetActiveMatch().ThenOnMainThread(delegate(Promise<NewMatchCreatedConfig> promise)
		{
			if (promise.Successful && promise.Result != null)
			{
				ReconnectToMatch(promise.Result);
			}
			else
			{
				eventContext = _eventManager.GetEventContext(internalEventName);
				PAPA.StartGlobalCoroutine(Coroutine_JoinLadderEvent(matchmaking, deckManager, eventContext, deck));
				_recentlyPlayedDataProvider.AddRecentlyPlayedGame(eventContext.PlayerEvent.EventInfo.EventId, deckId);
				AudioManager.PlayAudio(WwiseEvents.match_making_find_match, base.gameObject);
			}
		});
	}

	private IEnumerator Coroutine_PlayBotGame(BotTool botTool, Matchmaking matchmaking, DecksManager decksManager, IBotMatchServiceWrapper botMatchServiceWrapper, DeckDataProvider deckDataProvider, EventContext selectedEvent, Client_Deck deck)
	{
		BotControlManager.SetUpBotTool(botTool, _assetLookupSystem);
		WrapperController.EnableLoadingIndicator(enabled: true);
		matchmaking.SetExpectedEvent(selectedEvent);
		WrapperDeckUtilities.setLastPlayed(deck);
		Client_Deck deckToSubmit = WrapperDeckUtilities.GetSubmitDeck(deck, decksManager);
		yield return selectedEvent.PlayerEvent.DeckFormattedForEventSubmission(deckToSubmit).IfSuccess(delegate(Promise<Client_Deck> promise)
		{
			deckToSubmit = promise.Result;
		}).IfError(delegate(Promise<Client_Deck> promise)
		{
			deckToSubmit = null;
			Debug.LogError(promise.Error.Message);
			WrapperController.EnableLoadingIndicator(enabled: false);
		})
			.AsCoroutine();
		if (deckToSubmit == null)
		{
			Debug.LogError("Error joining bot match");
			yield break;
		}
		EAiBotMatchType botMatchType = EAiBotMatchTypeUtil.GetBotMatchType(selectedEvent.PlayerEvent.EventInfo.EventId);
		if (botMatchType == EAiBotMatchType.Unknown)
		{
			Debug.LogError("Invalid bot match event name supplied.");
			yield break;
		}
		Promise<string> handle = botMatchServiceWrapper.AIBotMatch(DeckInfoV3.FromDeckInfo(DeckServiceWrapperHelpers.ToAzureModel(deckToSubmit)), botTool.DeckHeuristic.GUID, botMatchType);
		yield return handle.AsCoroutine();
		WrapperController.EnableLoadingIndicator(enabled: false);
		if (handle.Successful)
		{
			matchmaking.SetupBotMatch(selectedEvent, LoadSceneMode.Additive);
			yield break;
		}
		Debug.LogError(handle.Error.Message);
		Utils.GetDeckSubmissionErrorMessages(handle.Error, out var errTitle, out var errText);
		SystemMessageManager.Instance.ShowOk(errTitle, errText);
		if (deckDataProvider != null && handle.Error.Code == 7000)
		{
			deckDataProvider.MarkDirty();
			deckDataProvider.GetAllDecks();
		}
		SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
	}

	private IEnumerator Coroutine_JoinLadderEvent(Matchmaking matchMaking, DecksManager decksManager, EventContext eventContext, Client_Deck deck)
	{
		WrapperController.EnableLoadingIndicator(enabled: true);
		if (eventContext.PlayerEvent.CourseData.CurrentModule != PlayerEventModule.Join)
		{
			Promise<ICourseInfoWrapper> drop = eventContext.PlayerEvent.DropFromEvent();
			yield return drop.AsCoroutine();
			if (!drop.Successful)
			{
				if (drop.ErrorSource != ErrorSource.Debounce)
				{
					frontDoorPromiseFailed(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Drop_Error_Text"));
				}
				yield break;
			}
		}
		Promise<ICourseInfoWrapper> joinAndPay = eventContext.PlayerEvent.JoinAndPay(EventEntryCurrencyType.None, "");
		yield return joinAndPay.AsCoroutine();
		if (!joinAndPay.Successful)
		{
			if (joinAndPay.ErrorSource != ErrorSource.Debounce)
			{
				frontDoorPromiseFailed(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Queue_Join_Error_Text"));
			}
			yield break;
		}
		Promise<Client_Deck> submitDeck = null;
		WrapperDeckUtilities.setLastPlayed(deck);
		Client_Deck deckToSubmit = WrapperDeckUtilities.GetSubmitDeck(deck, decksManager);
		if (deckToSubmit != null)
		{
			submitDeck = eventContext.PlayerEvent.SubmitEventDeck(deckToSubmit);
			yield return submitDeck.AsCoroutine();
		}
		if (deckToSubmit == null || !submitDeck.Successful)
		{
			Error error = submitDeck?.Error ?? new Error(-1, "Submitted event deck is null");
			Debug.LogError(error.Message);
			Utils.GetDeckSubmissionErrorMessages(error, out var errTitle, out var errText);
			frontDoorPromiseFailed(errTitle, errText);
			SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
			yield break;
		}
		eventContext.PlayerEvent.CourseData.CourseDeck = deck;
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_play_match_start.EventName, AudioManager.Default);
		matchMaking.SetExpectedEvent(eventContext);
		eventContext.PlayerEvent.JoinNewMatchQueue().ThenOnMainThread(delegate(Promise<string> p)
		{
			WrapperController.EnableLoadingIndicator(enabled: false);
			if (p.Successful)
			{
				matchMaking.SetupEventMatch(eventContext);
			}
			else
			{
				Debug.LogError("Error joining ladder event queue");
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Join_Error_Text"));
			}
		});
		static void frontDoorPromiseFailed(string title, string message)
		{
			WrapperController.EnableLoadingIndicator(enabled: false);
			SystemMessageManager.Instance.ShowOk(title, message);
		}
	}

	private void loadDeckFailed(DecksManager decksManager)
	{
		List<SystemMessageManager.SystemMessageButtonData> list = new List<SystemMessageManager.SystemMessageButtonData>();
		SystemMessageManager.SystemMessageButtonData item = new SystemMessageManager.SystemMessageButtonData
		{
			Text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_OK"),
			Callback = delegate
			{
				HidePlayblade();
				decksManager.ForceRefreshCachedDecks();
			}
		};
		list.Add(item);
		SystemMessageManager.Instance.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_InvalidDeck"), Languages.ActiveLocProvider.GetLocalizedText("DuelScene/Browsers/Submit_Deck_Invalid"), list);
		WrapperController.EnableLoadingIndicator(enabled: false);
	}

	private void EditDeck(Guid deckId, string eventId, string eventFormat, bool isInvalidForEventFormat)
	{
		Client_Deck deckForId = Pantry.Get<DeckDataProvider>().GetDeckForId(deckId);
		deckForId.Summary.Name = Utils.GetLocalizedDeckName(deckForId.Summary.Name);
		EventContext evt = ((eventId != null) ? _eventManager.GetEventContext(eventId) : null);
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		Wizards.Mtga.FrontDoorModels.DeckInfo deck = DeckServiceWrapperHelpers.ToAzureModel(deckForId);
		bool isInvalidForEventFormat2 = isInvalidForEventFormat;
		sceneLoader.GoToDeckBuilder(new DeckBuilderContext(deck, evt, sideboarding: false, firstEdit: false, DeckBuilderMode.DeckBuilding, ambiguousFormat: false, default(Guid), null, null, null, cachingEnabled: false, isPlayblade: true, eventFormat, isInvalidForEventFormat2));
	}

	private void GoToEventScreen(string internalEventName)
	{
		EventContext eventContext = _eventManager.GetEventContext(internalEventName);
		if (eventContext != null)
		{
			SceneLoader.GetSceneLoader().GoToEventScreen(eventContext, reloadIfAlreadyLoaded: false, SceneLoader.NavMethod.PlayButton);
		}
	}
}
