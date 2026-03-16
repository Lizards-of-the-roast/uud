using System.Collections.Generic;
using Core.Code.Promises;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Inventory;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.PlayBlade;
using Wotc.Mtga.Events;
using Wotc.Mtga.Providers;

[RequireComponent(typeof(Animator))]
public class NPEOnboarding : MonoBehaviour
{
	public enum EventTileVisuals
	{
		AllLocked,
		FreePlayUnlocked,
		RankedUnlocked,
		ReadyToUnlock,
		Normal
	}

	[SerializeField]
	private SparkyController _sparkyPrefab;

	[SerializeField]
	private float _sparkyScale = 1f;

	[SerializeField]
	private Canvas _Canvas;

	[Header("Debug")]
	[SerializeField]
	private bool _useDebug;

	[SerializeField]
	private int _mockColorMasteryEventsFinished;

	[SerializeField]
	private bool _mockSkipTourWasPressed;

	private SparkyController _sparky;

	private Animator _stateMachine;

	private SparkyTourState _npeOnboardingState;

	private SceneLoader _sceneLoader;

	private CampaignGraphManager _campaignGraphManager;

	private InventoryManager _inventoryManager;

	private TooltipSystem _tooltipSystem;

	private CosmeticsProvider _cosmetics;

	private AccountInformation _accountInfo;

	private bool? _inGame;

	private NavContentType _loadedContentType;

	private bool? _homeScreenReady;

	private bool? _eventBladeShown;

	private bool? _rewardsPanelShown;

	private bool? _upgradeDeckShown;

	private StoreTabType _storeTab;

	private bool? _tooltipActive;

	private bool? _deckSelected;

	private bool _avatarSelected;

	private bool? _boosterOpenInChamber;

	private int? _gold;

	private int? _gems;

	private int? _packs;

	private int? _finishedColorMasteryEvents;

	private bool? _clientForcedToBeUnlocked;

	private static readonly int InGame = Animator.StringToHash("InGame");

	private static readonly int InGameChanged = Animator.StringToHash("InGameChanged");

	private static readonly int HomePageLoaded = Animator.StringToHash("HomePageLoaded");

	private static readonly int HomePageReady = Animator.StringToHash("HomePageReady");

	private static readonly int EventBladeShown = Animator.StringToHash("EventBladeShown");

	private static readonly int EventBladeChanged = Animator.StringToHash("EventBladeChanged");

	private static readonly int RewardsPopupShown = Animator.StringToHash("RewardsPopupShown");

	private static readonly int StorePacksSubPageShown = Animator.StringToHash("StorePacksSubPageShown");

	private static readonly int TooltipActive = Animator.StringToHash("TooltipActive");

	private static readonly int DeckSelected = Animator.StringToHash("DeckSelected");

	private static readonly int BoosterOpenInChamber = Animator.StringToHash("BoosterOpenInChamber");

	private static readonly int AvatarSelected = Animator.StringToHash("AvatarSelected");

	private static readonly int Gold = Animator.StringToHash("Gold");

	private static readonly int Gems = Animator.StringToHash("Gems");

	private static readonly int PacksIncreased = Animator.StringToHash("PacksIncreased");

	private static readonly int PacksDecreased = Animator.StringToHash("PacksDecreased");

	private static readonly int Packs = Animator.StringToHash("Packs");

	private static readonly int FinishedColorMasteryEvents = Animator.StringToHash("FinishedColorMasteryEvents");

	private static readonly int LevelChanged = Animator.StringToHash("LevelChanged");

	private static readonly int ClientForcedToBeUnlocked = Animator.StringToHash("ClientForcedToBeUnlocked");

	private static readonly int UpgradeDeckShown = Animator.StringToHash("UpgradeDeckShown");

	private static readonly int SparkyArrived = Animator.StringToHash("SparkyArrived");

	private static readonly int SparkySaid = Animator.StringToHash("SparkySaid");

	private static readonly int HomePageShown = Animator.StringToHash("HomePageShown");

	private static readonly int ProfilePageShown = Animator.StringToHash("ProfilePageShown");

	private static readonly int StorePageShown = Animator.StringToHash("StorePageShown");

	private static readonly int ProductPageShown = Animator.StringToHash("ProductPageShown");

	private static readonly int PacksPageShown = Animator.StringToHash("PacksPageShown");

	private static readonly int EventPageShown = Animator.StringToHash("EventPageShown");

	private static readonly int ColorChallengeShown = Animator.StringToHash("ColorChallengeShown");

	private static readonly int LearnPageShown = Animator.StringToHash("LearnPageShown");

	private static readonly int RewardTrackPageShown = Animator.StringToHash("RewardTrackPageShown");

	private static readonly int RewardWebPageShown = Animator.StringToHash("RewardWebPageShown");

	private static readonly int PageChanged = Animator.StringToHash("PageChanged");

	private static readonly int IsHandheld = Animator.StringToHash("IsHandheld");

	private static readonly int QueueSelected = Animator.StringToHash("QueueSelected");

	private static readonly int SelectedQueueIsAlchemy = Animator.StringToHash("SelectedQueueIsAlchemy");

	private static readonly int SelectedQueueIsBotMatch = Animator.StringToHash("SelectedQueueIsBotMatch");

	private static readonly int SelectedQueueIsBrawl = Animator.StringToHash("SelectedQueueIsBrawl");

	private static readonly int SelectedQueueIsHistoric = Animator.StringToHash("SelectedQueueIsHistoric");

	private static readonly int SelectedQueueIsRanked = Animator.StringToHash("SelectedQueueIsRanked");

	private static readonly int SelectedQueueIsStandard = Animator.StringToHash("SelectedQueueIsStandard");

	private static readonly int SelectedQueueIsExplorer = Animator.StringToHash("SelectedQueueIsExplorer");

	private static readonly int OpenDualColorPreconEvent = Animator.StringToHash("OpenDualColorPreconEvent");

	private static readonly int OpenSparkQueue = Animator.StringToHash("OpenSparkQueue");

	private static readonly int GraduateSparkRank = Animator.StringToHash("GraduateSparkRank");

	private static readonly int OpenSparkyDeckDuel = Animator.StringToHash("OpenSparkyDeckDuel");

	private static readonly int CloseSparkyDeckDuel = Animator.StringToHash("CloseSparkyDeckDuel");

	private static readonly Dictionary<string, Dictionary<string, int>> _animatorMilestones = new Dictionary<string, Dictionary<string, int>> { 
	{
		"NewPlayerExperience",
		new Dictionary<string, int>
		{
			{ "OpenDualColorPreconEvent", OpenDualColorPreconEvent },
			{ "OpenSparkQueue", OpenSparkQueue },
			{ "GraduateSparkRank", GraduateSparkRank },
			{ "OpenSparkyDeckDuel", OpenSparkyDeckDuel },
			{ "CloseSparkyDeckDuel", CloseSparkyDeckDuel }
		}
	} };

	private static NPEOnboarding _npeOnboarding;

	public bool Pause
	{
		get
		{
			return _stateMachine.enabled;
		}
		set
		{
			_stateMachine.enabled = !value;
		}
	}

	private void Awake()
	{
		_npeOnboarding = this;
		base.gameObject.name = base.gameObject.name.Replace("(Clone)", "");
		_stateMachine = GetComponent<Animator>();
		_sparky = Object.Instantiate(_sparkyPrefab, base.transform);
		_sparky.OnArrived += OnArrived;
		_sparky.OnSaid += OnSaid;
		_sparky.gameObject.SetActive(value: false);
		if (_Canvas != null)
		{
			_Canvas.worldCamera = CurrentCamera.Value;
		}
		_stateMachine.SetBool(IsHandheld, PlatformUtils.IsHandheld());
	}

	public static void RestoreAnimatorStateMachinePersistentFlags()
	{
		StateMachineFlagSMB.RestorePersistentFlags(_npeOnboarding._stateMachine);
	}

	private void OnEnable()
	{
		_tooltipSystem = Pantry.Get<TooltipSystem>();
		_sparky.Pause = false;
		_inGame = null;
		_loadedContentType = NavContentType.None;
		_homeScreenReady = null;
		_eventBladeShown = null;
		_rewardsPanelShown = null;
		_upgradeDeckShown = null;
		_storeTab = StoreTabType.None;
		_tooltipActive = null;
		_deckSelected = null;
		_avatarSelected = false;
		_boosterOpenInChamber = null;
		_gold = null;
		_gems = null;
		_packs = null;
		_finishedColorMasteryEvents = null;
		_clientForcedToBeUnlocked = null;
		StateMachineFlagSMB.RestorePersistentFlags(_stateMachine);
		if (_npeOnboardingState == null)
		{
			_stateMachine.enabled = false;
		}
		if (_sceneLoader != null)
		{
			_sceneLoader.SceneLoaded -= UpdatePagesShown;
			_sceneLoader.SceneLoaded += UpdatePagesShown;
			_sceneLoader.PlayBladeQueueSelected -= PlayBladeQueueSelected;
			_sceneLoader.PlayBladeQueueSelected += PlayBladeQueueSelected;
			UpdatePagesShown();
		}
		if (_campaignGraphManager != null)
		{
			_campaignGraphManager.OnUpdateMilestoneStates -= OnUpdateMilestoneStates;
			_campaignGraphManager.OnUpdateMilestoneStates += OnUpdateMilestoneStates;
		}
	}

	private void OnDisable()
	{
		if (_sceneLoader != null)
		{
			_sceneLoader.SceneLoaded -= UpdatePagesShown;
			_sceneLoader.PlayBladeQueueSelected -= PlayBladeQueueSelected;
		}
		if (_campaignGraphManager != null)
		{
			_campaignGraphManager.OnUpdateMilestoneStates -= OnUpdateMilestoneStates;
		}
		SMBehaviour.DeactivateStateMachine(_stateMachine);
	}

	public void Initialize(AccountInformation accountInfo, InventoryManager inventoryManager, SparkyTourState sparkyTourState, SceneLoader sceneLoader, CosmeticsProvider cosmetics, CampaignGraphManager campaignGraphManager)
	{
		_accountInfo = accountInfo;
		_inventoryManager = inventoryManager;
		_npeOnboardingState = sparkyTourState;
		_sceneLoader = sceneLoader;
		_campaignGraphManager = campaignGraphManager;
		_cosmetics = cosmetics;
		_sceneLoader.SceneLoaded -= UpdatePagesShown;
		_sceneLoader.SceneLoaded += UpdatePagesShown;
		_sceneLoader.PlayBladeQueueSelected -= PlayBladeQueueSelected;
		_sceneLoader.PlayBladeQueueSelected += PlayBladeQueueSelected;
		_campaignGraphManager.OnUpdateMilestoneStates -= OnUpdateMilestoneStates;
		_campaignGraphManager.OnUpdateMilestoneStates += OnUpdateMilestoneStates;
		InitializeMilestoneStates();
		_stateMachine.enabled = true;
	}

	private void Update()
	{
		if (Application.isEditor)
		{
			if (Input.GetKeyDown(KeyCode.F6))
			{
				MDNPlayerPrefs.ClearSelectedDeckId(_accountInfo?.PersonaID, "Play");
			}
			if (Input.GetKeyDown(KeyCode.F8))
			{
				base.gameObject.SetActive(value: false);
				base.gameObject.SetActive(value: true);
			}
		}
		if (_npeOnboardingState == null || !_npeOnboardingState.StateLoaded)
		{
			return;
		}
		if (_inGame != _sceneLoader.IsInDuelScene)
		{
			_inGame = _sceneLoader.IsInDuelScene;
			_stateMachine.SetBool(InGame, _inGame.Value);
			UpdatePagesShown();
			_stateMachine.SetTrigger(InGameChanged);
		}
		if (_inGame == true)
		{
			return;
		}
		if (_loadedContentType != _sceneLoader.CurrentContentType && !_sceneLoader.IsLoading)
		{
			_loadedContentType = _sceneLoader.CurrentContentType;
			_stateMachine.SetBool(HomePageLoaded, _sceneLoader.CurrentContentType == NavContentType.Home);
		}
		bool isHomeScreenReady = _sceneLoader.GetIsHomeScreenReady();
		if (_homeScreenReady != isHomeScreenReady)
		{
			_homeScreenReady = isHomeScreenReady;
			_stateMachine.SetBool(HomePageReady, _homeScreenReady.Value);
		}
		bool homeEventBladeShown = _sceneLoader.GetHomeEventBladeShown();
		if (_eventBladeShown != homeEventBladeShown)
		{
			_eventBladeShown = homeEventBladeShown;
			_stateMachine.SetBool(EventBladeShown, _eventBladeShown.Value);
			_stateMachine.SetTrigger(EventBladeChanged);
		}
		bool homeDeckSelected = _sceneLoader.GetHomeDeckSelected();
		if (_deckSelected != homeDeckSelected)
		{
			_deckSelected = homeDeckSelected;
			_stateMachine.SetBool("DeckSelected", _deckSelected.Value);
		}
		bool flag = _sceneLoader.GetObjectivesController().IsAnimating || _sceneLoader.GetRewardsContentController().isActiveAndEnabled;
		if (_rewardsPanelShown != flag)
		{
			_rewardsPanelShown = flag;
			_stateMachine.SetBool(RewardsPopupShown, _rewardsPanelShown.Value);
		}
		StoreTabType storeCurrentTab = _sceneLoader.GetStoreCurrentTab();
		if (_storeTab != storeCurrentTab)
		{
			_storeTab = storeCurrentTab;
			_stateMachine.SetBool(StorePacksSubPageShown, _storeTab == StoreTabType.Packs);
			_stateMachine.SetBool(ProductPageShown, _storeTab == StoreTabType.Packs);
		}
		bool flag2 = (bool)_tooltipSystem && (_tooltipSystem.IsDisplaying || _sceneLoader.GetObjectivesController().IsPopupActive);
		if (_tooltipActive != flag2)
		{
			_tooltipActive = flag2;
			_stateMachine.SetBool(TooltipActive, _tooltipActive.Value);
		}
		bool boosterOpenInChamber = _sceneLoader.GetBoosterOpenInChamber();
		if (_boosterOpenInChamber != boosterOpenInChamber)
		{
			_boosterOpenInChamber = boosterOpenInChamber;
			_stateMachine.SetBool(BoosterOpenInChamber, _boosterOpenInChamber.Value);
		}
		if (!_avatarSelected && !string.IsNullOrWhiteSpace(_cosmetics.PlayerAvatarSelection))
		{
			_avatarSelected = true;
			_stateMachine.SetBool(AvatarSelected, _avatarSelected);
		}
		ClientPlayerInventory inventory = _inventoryManager.Inventory;
		if (_gold != inventory?.gold)
		{
			_gold = inventory.gold;
			_stateMachine.SetInteger(Gold, _gold.Value);
		}
		if (_gems != inventory?.gems)
		{
			_gems = inventory.gems;
			_stateMachine.SetInteger(Gems, _gems.Value);
		}
		int num = 0;
		if (inventory?.boosters != null)
		{
			foreach (ClientBoosterInfo booster in inventory.boosters)
			{
				num += booster.count;
			}
		}
		if (_packs != num)
		{
			if (_packs.HasValue)
			{
				if (_packs < num)
				{
					_stateMachine.SetTrigger(PacksIncreased);
				}
				else
				{
					_stateMachine.SetTrigger(PacksDecreased);
				}
			}
			_packs = num;
			_stateMachine.SetInteger(Packs, _packs.Value);
		}
		int num2 = 0;
		num2 = (_useDebug ? _mockColorMasteryEventsFinished : _npeOnboardingState.FinishedColorMasteryEvents);
		if (_finishedColorMasteryEvents != num2)
		{
			_finishedColorMasteryEvents = num2;
			_stateMachine.SetInteger(FinishedColorMasteryEvents, _finishedColorMasteryEvents.Value);
			_stateMachine.SetTrigger(LevelChanged);
		}
		bool flag3 = (_useDebug ? _mockSkipTourWasPressed : _npeOnboardingState.ClientForcedToUnlock);
		if (_clientForcedToBeUnlocked != flag3)
		{
			_clientForcedToBeUnlocked = flag3;
			_stateMachine.SetBool(ClientForcedToBeUnlocked, _clientForcedToBeUnlocked.Value);
			_stateMachine.SetTrigger(LevelChanged);
		}
		bool rewardTreeUpgradeDeckShown = _sceneLoader.GetRewardTreeUpgradeDeckShown();
		if (_upgradeDeckShown != rewardTreeUpgradeDeckShown)
		{
			_upgradeDeckShown = rewardTreeUpgradeDeckShown;
			_stateMachine.SetBool(UpgradeDeckShown, _upgradeDeckShown.Value);
		}
		_sparky.transform.localScale = Vector3.one * _sparkyScale;
	}

	private void OnArrived()
	{
		_stateMachine.SetTrigger(SparkyArrived);
	}

	private void OnSaid()
	{
		_stateMachine.SetTrigger(SparkySaid);
	}

	private void UpdatePagesShown()
	{
		_homeScreenReady = false;
		_loadedContentType = NavContentType.None;
		_stateMachine.SetBool(HomePageReady, value: false);
		_stateMachine.SetBool(HomePageLoaded, value: false);
		_stateMachine.SetBool(HomePageShown, value: false);
		_stateMachine.SetBool(ProfilePageShown, value: false);
		_stateMachine.SetBool(StorePageShown, value: false);
		_stateMachine.SetBool(ProductPageShown, value: false);
		_stateMachine.SetBool(PacksPageShown, value: false);
		_stateMachine.SetBool(ColorChallengeShown, value: false);
		_stateMachine.SetBool(EventPageShown, value: false);
		_stateMachine.SetBool(LearnPageShown, value: false);
		_stateMachine.SetBool(RewardTrackPageShown, value: false);
		_stateMachine.SetBool(RewardWebPageShown, value: false);
		if (_inGame == false)
		{
			NavContentType currentContentType = _sceneLoader.CurrentContentType;
			_stateMachine.SetBool(HomePageShown, currentContentType == NavContentType.Home);
			_stateMachine.SetBool(ProfilePageShown, currentContentType == NavContentType.Profile);
			_stateMachine.SetBool(StorePageShown, currentContentType == NavContentType.Store);
			_stateMachine.SetBool(PacksPageShown, currentContentType == NavContentType.BoosterChamber);
			_stateMachine.SetBool(EventPageShown, currentContentType == NavContentType.EventLanding);
			_stateMachine.SetBool(ColorChallengeShown, currentContentType == NavContentType.ChallengeEventLanding);
			_stateMachine.SetBool(LearnPageShown, currentContentType == NavContentType.LearnToPlay);
			_stateMachine.SetBool(RewardTrackPageShown, currentContentType == NavContentType.RewardTrack);
			_stateMachine.SetBool(RewardWebPageShown, currentContentType == NavContentType.RewardTree);
		}
		_stateMachine.SetTrigger(PageChanged);
	}

	private void PlayBladeQueueSelected(BladeEventInfo selectedBladeEventInfo)
	{
		_stateMachine.SetBool(SelectedQueueIsAlchemy, selectedBladeEventInfo.Format?.IsAlchemy ?? false);
		_stateMachine.SetBool(SelectedQueueIsBotMatch, selectedBladeEventInfo.IsBotMatch);
		_stateMachine.SetBool(SelectedQueueIsBrawl, selectedBladeEventInfo.Format?.FormatIncludesCommandZone ?? false);
		_stateMachine.SetBool(SelectedQueueIsHistoric, selectedBladeEventInfo.Format?.IsHistoric ?? false);
		_stateMachine.SetBool(SelectedQueueIsRanked, selectedBladeEventInfo.IsRanked);
		_stateMachine.SetBool(SelectedQueueIsStandard, selectedBladeEventInfo.Format?.IsStandard ?? false);
		_stateMachine.SetBool(SelectedQueueIsExplorer, selectedBladeEventInfo.Format?.IsExplorer ?? false);
		_stateMachine.SetTrigger(QueueSelected);
	}

	public void CompleteOnboarding()
	{
		Object.Destroy(base.gameObject);
	}

	private void InitializeMilestoneStates()
	{
		foreach (string key in _animatorMilestones.Keys)
		{
			if (_campaignGraphManager.TryGetState(key, out var state))
			{
				OnUpdateMilestoneStates(key, state.MilestoneStates);
			}
		}
	}

	private void OnUpdateMilestoneStates(string campaignGraphId, Dictionary<string, bool> milestoneStates)
	{
		MainThreadDispatcher.Dispatch(delegate
		{
			OnUpdateMilestoneStatesInner(campaignGraphId, milestoneStates);
		});
	}

	private void OnUpdateMilestoneStatesInner(string campaignGraphId, Dictionary<string, bool> milestoneStates)
	{
		if (!_animatorMilestones.TryGetValue(campaignGraphId, out var value))
		{
			return;
		}
		foreach (KeyValuePair<string, int> item in value)
		{
			milestoneStates.TryGetValue(item.Key, out var value2);
			_stateMachine.SetBool(item.Value, value2);
		}
	}
}
