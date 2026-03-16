using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AssetLookupTree;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Code.ClientFeatureToggle;
using Core.Code.Decks;
using Core.Code.Input;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation.Achievements;
using Core.Meta.MainNavigation.Challenge;
using Core.Meta.MainNavigation.NavBar;
using Core.Shared.Code.ClientModels;
using MTGA.KeyboardManager;
using ProfileUI;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.Arena.Enums.Store;
using Wizards.Mtga;
using Wizards.Mtga.Inventory;
using Wizards.Mtga.Models.ClientModels;
using Wizards.Mtga.Platforms;
using Wizards.Unification.Models.Graph;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;
using Wotc.Mtga.Wrapper.Draft;
using Wotc.Mtga.Wrapper.Mailbox;

public class NavBarController : MonoBehaviour, IKeyDownSubscriber, IKeySubscriber, IBackActionHandler
{
	public enum OnboardingState
	{
		None,
		HiddenBar,
		NoStore,
		FullBar,
		Home
	}

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[Header("Main Tabs")]
	public CustomButton HomeButton;

	private Animator HomeAnimator;

	public GameObject HomeButtonOverlay;

	public CustomButton ProfileButton;

	private Animator ProfileAnimator;

	public GameObject ProfileIndicator;

	public GameObject ProfileLock;

	public GameObject ProfileButtonOverlay;

	public CustomButton DecksButton;

	private Animator DecksAnimator;

	public GameObject DecksIndicator;

	public GameObject DecksLock;

	public GameObject DecksButtonOverlay;

	public CustomButton PacksButton;

	private Animator PacksAnimator;

	public GameObject PacksIndicator;

	public GameObject PacksLock;

	public GameObject PacksButtonOverlay;

	public CustomButton StoreButton;

	private Animator StoreAnimator;

	public GameObject StoreLock;

	public GameObject StoreIndicator;

	public GameObject StoreButtonOverlay;

	public CustomButton MasteryButton;

	private Animator MasteryAnimator;

	public GameObject MasteryIndicator;

	public GameObject MasteryLock;

	public GameObject MasteryButtonOverlay;

	public CustomButton AchievementsButton;

	private Animator AchievementsAnimator;

	public GameObject AchievementsIndicator;

	public GameObject AchievementsLock;

	public GameObject AchievementsButtonOverlay;

	[Header("Wildcard Object")]
	[SerializeField]
	private CustomButton _wildcardButton;

	[SerializeField]
	private TooltipTrigger _wildcardTooltip;

	[SerializeField]
	private Image _wildcardImage;

	[SerializeField]
	private Sprite _wildcardSpriteCommon;

	[SerializeField]
	private Sprite _wildcardSpriteUncommon;

	[SerializeField]
	private Sprite _wildcardSpriteRare;

	[SerializeField]
	private Sprite _wildcardSpriteMythic;

	[SerializeField]
	private GameObject _wildcardBonusEffectLevel1;

	[SerializeField]
	private GameObject _wildcardBonusEffectLevel2;

	[SerializeField]
	private GameObject _wildcardsAddedFXPrefab;

	[SerializeField]
	private Transform _wildcardsAddedFXContainer;

	[Header("Currency Objects")]
	public GameObject CurrenciesContainer;

	public CustomButton coinButton;

	[SerializeField]
	private TMP_Text _goldText;

	public CustomButton gemButton;

	[SerializeField]
	private TMP_Text _gemText;

	[SerializeField]
	private int _maxDisplayCurrency = int.MaxValue;

	[SerializeField]
	private NavBarTokenView _navBarTokenView;

	[Header("Vault Object")]
	public CustomButton VaultButton;

	[SerializeField]
	private GameObject _vaultContainer;

	[SerializeField]
	private Animator _vaultAnimator;

	[SerializeField]
	private TooltipTrigger _vaultTooltip;

	[Header("Other Buttons")]
	public CustomButton MailboxButton;

	[SerializeField]
	private GameObject _mailboxContainer;

	public CustomButton LearnButton;

	[SerializeField]
	private GameObject _learnContainer;

	public CustomButton OptionsButton;

	[SerializeField]
	private GameObject _optionsContainer;

	[Header("Other Objects")]
	public CustomToggle DeckViewToggle;

	[SerializeField]
	private GameObject _deckViewToggleContainer;

	public LayoutElement SettingsSpacer;

	public Image[] Backgrounds;

	public Transform RightSide;

	public Transform[] _rightSideChildOrder;

	public Transform SettingsOnlyTransform;

	private NavContentController _currentContentController;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	private IAccountClient _accountClient;

	private AssetLookupSystem _assetLookupSystem;

	private bool _firstRefresh = true;

	private bool _storeEnabled;

	private OnboardingState _onboardingState;

	private bool _hidden;

	private bool _systemExitDialogueShown;

	private bool _packsEnabled;

	private bool _profileEnabled;

	private bool _decksEnabled;

	private bool _achievementsEnabled;

	private Client_KillSwitchNotification _latestKillswitch;

	private ClientFeatureToggleDataProvider _featureToggleDataProvider;

	private PVPChallengeController _pvpChallengeController;

	private CustomTokenProvider _eventTokenProvider;

	private RectTransform _optionsButtonRectTransform;

	private static readonly int SelectedHash = Animator.StringToHash("Selected");

	private static readonly int ActiveHash = Animator.StringToHash("Active");

	private DeckBuilderLayoutState DeckBuilderLayoutState => Pantry.Get<DeckBuilderLayoutState>();

	public PriorityLevelEnum Priority => PriorityLevelEnum.BackButton;

	public CustomButton WildcardButton => _wildcardButton;

	private RectTransform OptionsButtonRectTransform
	{
		get
		{
			if (!_optionsButtonRectTransform)
			{
				_optionsButtonRectTransform = _optionsContainer.GetComponent<RectTransform>();
			}
			return _optionsButtonRectTransform;
		}
	}

	private void Awake()
	{
		_featureToggleDataProvider = Pantry.Get<ClientFeatureToggleDataProvider>();
		_featureToggleDataProvider.RegisterForToggleUpdates(AttemptSetAchievementsFeatureFromToggleLookup);
		AttemptSetAchievementsFeatureFromToggleLookup();
		_accountClient = Pantry.Get<IAccountClient>();
		_assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		_pvpChallengeController = Pantry.Get<PVPChallengeController>();
		HomeButton.OnClick.AddListener(HomeButton_OnClick);
		HomeAnimator = HomeButton.GetComponent<Animator>();
		ProfileButton.OnClick.AddListener(ProfileButton_OnClick);
		ProfileAnimator = ProfileButton.GetComponent<Animator>();
		DecksButton.OnClick.AddListener(DecksButton_OnClick);
		DecksAnimator = DecksButton.GetComponent<Animator>();
		StoreButton.OnClick.AddListener(StoreButton_OnClick);
		StoreAnimator = StoreButton.GetComponent<Animator>();
		PacksButton.OnClick.AddListener(PacksButton_OnClick);
		PacksAnimator = PacksButton.GetComponent<Animator>();
		MasteryButton.OnClick.AddListener(Mastery_OnClick);
		MasteryAnimator = MasteryButton.GetComponent<Animator>();
		AchievementsButton.OnClick.AddListener(AchievementsButton_OnClick);
		AchievementsAnimator = AchievementsButton.GetComponent<Animator>();
		LearnButton.OnClick.AddListener(LearnButton_OnClick);
		OptionsButton.OnClick.AddListener(OptionsButton_OnClick);
		MailboxButton.OnClick.AddListener(MailboxButton_OnClick);
		VaultButton.OnClick.AddListener(VaultButton_OnClick);
		coinButton.OnClick.AddListener(Coin_OnClick);
		gemButton.OnClick.AddListener(Gem_OnClick);
		_wildcardButton.OnClick.AddListener(WildCard_OnClick);
		HomeButton.OnMouseover.AddListener(NavButton_OnHover);
		ProfileButton.OnMouseover.AddListener(NavButton_OnHover);
		DecksButton.OnMouseover.AddListener(NavButton_OnHover);
		StoreButton.OnMouseover.AddListener(NavButton_OnHover);
		PacksButton.OnMouseover.AddListener(NavButton_OnHover);
		LearnButton.OnMouseover.AddListener(NavButton_OnHover);
		MasteryButton.OnMouseover.AddListener(NavButton_OnHover);
		AchievementsButton.OnMouseover.AddListener(NavButton_OnHover);
		OptionsButton.OnMouseover.AddListener(NavButton_OnHover);
		VaultButton.OnMouseover.AddListener(NavButton_OnHover);
		coinButton.OnMouseover.AddListener(Coin_OnHover);
		gemButton.OnMouseover.AddListener(Gem_OnHover);
		_wildcardButton.OnMouseover.AddListener(WildCard_OnHover);
		_keyboardManager = Pantry.Get<KeyboardManager>();
		if (_keyboardManager != null)
		{
			_keyboardManager.Subscribe(this);
		}
		_actionSystem = Pantry.Get<IActionSystem>();
		_actionSystem.PushFocus(this);
		DeckViewToggle.OnValueChanged.AddListener(DeckViewToggle_OnValueChanged);
		StartCoroutine(SubscribePackProgressMeterUpdate());
		Languages.LanguageChangedSignal.Listeners += OnLanguageChange;
		Pantry.Get<StoreManager>().OnStoreEnabledSet += EnableStore;
		Pantry.Get<CampaignGraphManager>().OnNodeStatesUpdated += OnNodeStatesUpdated;
	}

	private void Start()
	{
		_rightSideChildOrder = RightSide.GetComponentsInChildren<Transform>(includeInactive: true);
		_navBarTokenView.Init(Languages.ActiveLocProvider, _assetLookupSystem);
	}

	private void OnEnable()
	{
		EnableInput(inputEnabled: true);
	}

	private void OnDestroy()
	{
		_featureToggleDataProvider?.UnRegisterForToggleUpdates(AttemptSetAchievementsFeatureFromToggleLookup);
		HomeButton.OnClick.RemoveListener(HomeButton_OnClick);
		ProfileButton.OnClick.RemoveListener(ProfileButton_OnClick);
		DecksButton.OnClick.RemoveListener(DecksButton_OnClick);
		StoreButton.OnClick.RemoveListener(StoreButton_OnClick);
		PacksButton.OnClick.RemoveListener(PacksButton_OnClick);
		LearnButton.OnClick.RemoveListener(LearnButton_OnClick);
		OptionsButton.OnClick.RemoveListener(OptionsButton_OnClick);
		VaultButton.OnClick.RemoveListener(VaultButton_OnClick);
		coinButton.OnClick.RemoveListener(Coin_OnClick);
		gemButton.OnClick.RemoveListener(Gem_OnClick);
		_wildcardButton.OnClick.RemoveListener(WildCard_OnClick);
		MasteryButton.OnClick.RemoveListener(Mastery_OnClick);
		AchievementsButton.OnClick.RemoveListener(AchievementsButton_OnClick);
		HomeButton.OnMouseover.RemoveListener(NavButton_OnHover);
		ProfileButton.OnMouseover.RemoveListener(NavButton_OnHover);
		DecksButton.OnMouseover.RemoveListener(NavButton_OnHover);
		StoreButton.OnMouseover.RemoveListener(NavButton_OnHover);
		PacksButton.OnMouseover.RemoveListener(NavButton_OnHover);
		LearnButton.OnMouseover.RemoveListener(NavButton_OnHover);
		MasteryButton.OnMouseover.RemoveListener(NavButton_OnHover);
		AchievementsButton.OnMouseover.RemoveListener(NavButton_OnHover);
		OptionsButton.OnMouseover.RemoveListener(NavButton_OnHover);
		VaultButton.OnMouseover.RemoveListener(NavButton_OnHover);
		coinButton.OnMouseover.RemoveListener(Coin_OnHover);
		gemButton.OnMouseover.RemoveListener(Gem_OnHover);
		_wildcardButton.OnMouseover.RemoveListener(WildCard_OnHover);
		_keyboardManager?.Unsubscribe(this);
		_actionSystem?.PopFocus(this);
		DeckViewToggle.OnValueChanged.RemoveListener(DeckViewToggle_OnValueChanged);
		Languages.LanguageChangedSignal.Listeners -= OnLanguageChange;
		Pantry.Get<StoreManager>().OnStoreEnabledSet -= EnableStore;
		Pantry.Get<CampaignGraphManager>().OnNodeStatesUpdated -= OnNodeStatesUpdated;
	}

	private IEnumerator SubscribePackProgressMeterUpdate()
	{
		yield return new WaitUntil(() => WrapperController.Instance?.BonusPackManager != null);
		WrapperController.Instance.BonusPackManager.OnPacksPurchased += delegate
		{
			UpdateButtons();
		};
	}

	private void AttemptSetAchievementsFeatureFromToggleLookup()
	{
		AchievementsButton.GameObject().SetActive(AttemptAchievementsFeatureToggleLookup());
	}

	private bool AttemptAchievementsFeatureToggleLookup()
	{
		if (_featureToggleDataProvider.IsInitialized())
		{
			return _featureToggleDataProvider.GetToggleValueById("AchievementSceneStatus");
		}
		return false;
	}

	public void RefreshLocks(Client_KillSwitchNotification killswitch)
	{
		EnableProfile(killswitch != null && !killswitch.IsProfileSceneDisabled);
		EnableDecks(killswitch != null && !killswitch.IsDeckSceneDisabled);
		EnableBoosters(killswitch != null && !killswitch.IsBoosterDisabled);
		EnableAchievements(killswitch != null && !killswitch.IsAchievementSceneDisabled);
		_latestKillswitch = killswitch;
		RefreshCurrencyDisplay();
	}

	public void EnableStore(bool isEnabled)
	{
		_storeEnabled = isEnabled;
		StoreIndicator.UpdateActive(active: false);
		UpdateButtons();
	}

	public void EnableProfile(bool isEnabled)
	{
		_profileEnabled = isEnabled;
		ProfileIndicator.UpdateActive(active: false);
		UpdateButtons();
	}

	public void EnableBoosters(bool isEnabled)
	{
		_packsEnabled = isEnabled;
		PacksIndicator.UpdateActive(active: false);
		UpdateButtons();
	}

	public void EnableDecks(bool isEnabled)
	{
		_decksEnabled = isEnabled;
		UpdateButtons();
	}

	public void EnableAchievements(bool isEnabled)
	{
		_achievementsEnabled = isEnabled;
		AchievementsIndicator.UpdateActive(ShouldAchievementIndicatorBeActive());
		UpdateButtons();
	}

	public void EnableProfilePip()
	{
		ProfileIndicator.SetActive(value: true);
	}

	public void SetOnboardingState(OnboardingState onboardingState)
	{
		if (_onboardingState != onboardingState)
		{
			_onboardingState = onboardingState;
			UpdateButtons();
		}
	}

	public void SetHiddenState(bool hidden)
	{
		if (_hidden != hidden)
		{
			_hidden = hidden;
			UpdateButtons();
		}
	}

	public void UpdateCurrentScreenIndicator(NavContentController contentController)
	{
		_currentContentController = contentController;
		UpdateButtons();
	}

	public void EnableInput(bool inputEnabled)
	{
		_canvasGroup.blocksRaycasts = inputEnabled;
	}

	public void RefreshButtons()
	{
		UpdateButtons();
	}

	private async Task UpdateButtons()
	{
		bool active = !_hidden && _currentContentController != null;
		bool showHome = _onboardingState != OnboardingState.HiddenBar;
		bool flag = showHome && _onboardingState != OnboardingState.Home;
		bool flag2 = false;
		bool active2 = flag && AttemptAchievementsFeatureToggleLookup();
		PacksButtonOverlay.SetActive(value: false);
		HomeButtonOverlay.SetActive(value: false);
		ProfileButtonOverlay.SetActive(value: false);
		MasteryButtonOverlay.SetActive(value: false);
		DecksButtonOverlay.SetActive(value: false);
		StoreButtonOverlay.SetActive(value: false);
		AchievementsButtonOverlay.SetActive(value: false);
		HomeAnimator.SetBool(SelectedHash, value: false);
		ProfileAnimator.SetBool(SelectedHash, value: false);
		DecksAnimator.SetBool(SelectedHash, value: false);
		MasteryAnimator.SetBool(SelectedHash, value: false);
		PacksAnimator.SetBool(SelectedHash, value: false);
		StoreAnimator.SetBool(SelectedHash, value: false);
		AchievementsAnimator.SetBool(SelectedHash, value: false);
		DecksIndicator.SetActive(value: false);
		if (showHome && _currentContentController != null)
		{
			switch (_currentContentController.NavContentType)
			{
			case NavContentType.Home:
				HomeButtonOverlay.SetActive(value: true);
				HomeAnimator.SetBool(SelectedHash, value: true);
				if (((HomePageContentController)_currentContentController).HasChallengeScreenOpen())
				{
					flag = false;
					active2 = false;
				}
				break;
			case NavContentType.DeckListViewer:
				DecksButtonOverlay.SetActive(value: true);
				DecksAnimator.SetBool(SelectedHash, value: true);
				break;
			case NavContentType.DeckBuilder:
			{
				DecksButtonOverlay.SetActive(value: true);
				DecksAnimator.SetBool(SelectedHash, value: true);
				WrapperDeckBuilder wrapperDeckBuilder = (WrapperDeckBuilder)_currentContentController;
				if (wrapperDeckBuilder.IsSideboarding)
				{
					active = false;
				}
				if (wrapperDeckBuilder.IsEditingDeck && !wrapperDeckBuilder.IsReadOnly)
				{
					flag2 = true;
				}
				break;
			}
			case NavContentType.BoosterChamber:
				PacksButtonOverlay.SetActive(value: true);
				PacksAnimator.SetBool(SelectedHash, value: true);
				break;
			case NavContentType.Store:
				StoreButtonOverlay.SetActive(value: true);
				StoreAnimator.SetBool(SelectedHash, value: true);
				break;
			case NavContentType.Profile:
				ProfileButtonOverlay.SetActive(value: true);
				ProfileAnimator.SetBool(SelectedHash, value: true);
				break;
			case NavContentType.RewardTrack:
			case NavContentType.RewardTree:
				MasteryButtonOverlay.SetActive(value: true);
				MasteryAnimator.SetBool(SelectedHash, value: true);
				break;
			case NavContentType.Draft:
			{
				DraftContentController obj = (DraftContentController)_currentContentController;
				flag2 = !obj.IsForceVertical();
				flag = obj.DraftMode == DraftModes.BotDraft;
				active2 = flag && AttemptAchievementsFeatureToggleLookup();
				break;
			}
			case NavContentType.Achievements:
				AchievementsButtonOverlay.SetActive(value: true);
				AchievementsAnimator.SetBool(SelectedHash, value: true);
				break;
			}
		}
		bool active3 = flag && _onboardingState != OnboardingState.NoStore;
		base.gameObject.SetActive(active);
		HomeButton.gameObject.SetActive(showHome);
		ProfileButton.gameObject.SetActive(flag);
		MasteryButton.gameObject.SetActive(flag);
		AchievementsButton.gameObject.SetActive(active2);
		DecksButton.gameObject.SetActive(flag);
		StoreButton.gameObject.SetActive(active3);
		PacksButton.gameObject.SetActive(active3);
		_learnContainer.gameObject.SetActive(showHome);
		_deckViewToggleContainer.SetActive(flag2);
		if (flag2)
		{
			DeckViewToggle.Value = DeckBuilderLayoutState.LayoutInUse == DeckBuilderLayout.Column;
		}
		if (CurrenciesContainer != null)
		{
			CurrenciesContainer.SetActive(flag);
		}
		Image[] backgrounds = Backgrounds;
		for (int i = 0; i < backgrounds.Length; i++)
		{
			backgrounds[i].enabled = showHome;
		}
		SettingsSpacer.minWidth = ((!showHome) ? 20 : 200);
		StoreButton.Interactable = _storeEnabled;
		StoreLock.gameObject.SetActive(!_storeEnabled);
		ProfileButton.Interactable = _profileEnabled;
		ProfileLock.gameObject.SetActive(!_profileEnabled);
		DecksButton.Interactable = _decksEnabled;
		DecksLock.gameObject.SetActive(!_decksEnabled);
		PacksButton.Interactable = _packsEnabled;
		PacksLock.gameObject.SetActive(!_packsEnabled);
		SetMasteryDataProvider setMasteryDataProvider = Pantry.Get<SetMasteryDataProvider>();
		bool failedInitializing = setMasteryDataProvider.FailedInitializing;
		MasteryButton.Interactable = !failedInitializing;
		MasteryLock.gameObject.SetActive(failedInitializing);
		AchievementsButton.Interactable = _achievementsEnabled;
		AchievementsLock.gameObject.SetActive(!_achievementsEnabled);
		RefreshVaultProgress(WrapperController.Instance.InventoryManager.Inventory?.vaultProgress ?? 0.0);
		if (_firstRefresh && showHome)
		{
			StartCoroutine(Coroutine_RefreshCurrencyDisplay());
			_firstRefresh = false;
		}
		AccountInformation accountInformation = _accountClient.AccountInformation;
		if (accountInformation != null)
		{
			string combinedHash = WrapperController.Instance.Store.ListingsHash;
			if (_currentContentController is ContentController_StoreCarousel)
			{
				MDNPlayerPrefs.SetLastStoreViewed(accountInformation.PersonaID, combinedHash);
			}
			else if (_currentContentController is ProgressionTracksContentController || _currentContentController is RewardTreeController)
			{
				string value = setMasteryDataProvider.PrizeWallHash();
				MDNPlayerPrefs.SetLastSetMasteryNavViewed(accountInformation.PersonaID, value);
			}
			if (_currentContentController is ContentController_PrizeWall contentController_PrizeWall && contentController_PrizeWall.IsMasteryPrizeWall(setMasteryDataProvider))
			{
				string value2 = setMasteryDataProvider.PrizeWallHash();
				MDNPlayerPrefs.SetLastSetMasteryOrbSpendViewed(accountInformation.PersonaID, value2);
			}
			MasteryIndicator.UpdateActive(setMasteryDataProvider.ShouldShowSetMasteryHeatPip(_accountClient));
			string lastStoreViewed = MDNPlayerPrefs.GetLastStoreViewed(accountInformation.PersonaID);
			bool flag3 = await WrapperController.Instance.SparkyTourState.GetEligibleForStoreUpdates();
			bool flag4 = WrapperController.Instance != null && (WrapperController.Instance.BonusPackManager?.CanRedeem ?? false);
			StoreIndicator.UpdateActive(flag3 && (lastStoreViewed != combinedHash || flag4));
		}
		if (!showHome)
		{
			_firstRefresh = true;
			if (_optionsContainer.transform.parent != SettingsOnlyTransform)
			{
				_optionsContainer.transform.SetParent(SettingsOnlyTransform);
				_optionsContainer.transform.localPosition = Vector3.zero;
				OptionsButtonRectTransform.sizeDelta = new Vector2(50f, 70f);
			}
		}
		else
		{
			if (!(_optionsContainer.transform.parent != RightSide))
			{
				return;
			}
			_optionsContainer.transform.SetParent(RightSide, worldPositionStays: false);
			Transform[] rightSideChildOrder = _rightSideChildOrder;
			foreach (Transform transform in rightSideChildOrder)
			{
				if (!(transform == null) && transform.parent == RightSide)
				{
					transform.SetAsLastSibling();
				}
			}
		}
	}

	public void RefreshVaultProgress(double pct)
	{
		UpdateVaultDisplay(pct);
		bool flag = _onboardingState == OnboardingState.HiddenBar;
		if (pct >= 100.0 && !flag && WrapperController.Instance.NPEState.ActiveNPEGame == null)
		{
			_vaultContainer.gameObject.SetActive(value: true);
			if (_vaultAnimator.isActiveAndEnabled)
			{
				_vaultAnimator.SetBool(ActiveHash, value: true);
			}
		}
		else
		{
			if (_vaultAnimator.isActiveAndEnabled)
			{
				_vaultAnimator.SetBool(ActiveHash, value: false);
			}
			_vaultContainer.gameObject.SetActive(value: false);
		}
	}

	private void ChangeScreen(Action screenChangeAction)
	{
		HideInboxIfActive();
		if (_currentContentController != null)
		{
			_currentContentController.OnNavBarScreenChange(screenChangeAction);
		}
		else
		{
			screenChangeAction();
		}
	}

	private void HideInboxIfActive()
	{
		ContentControllerPlayerInbox playerInboxContentController = SceneLoader.GetSceneLoader().GetPlayerInboxContentController();
		if (playerInboxContentController != null && playerInboxContentController.isActiveAndEnabled)
		{
			playerInboxContentController.Hide();
		}
	}

	private void HomeButton_OnClick()
	{
		if (SceneLoader.GetSceneLoader().CurrentContentType != NavContentType.Home)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_home_open, base.gameObject);
			ChangeScreen(ChangeToHomePage);
			return;
		}
		(SceneLoader.GetSceneLoader().CurrentNavContent as HomePageContentController)?.HidePlayblade();
		PVPChallengeData activeCurrentChallengeData = _pvpChallengeController.GetActiveCurrentChallengeData();
		if (activeCurrentChallengeData != null)
		{
			_pvpChallengeController.LeaveChallenge(activeCurrentChallengeData.ChallengeId);
		}
		HideInboxIfActive();
	}

	private void ChangeToHomePage()
	{
		SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
	}

	private void VaultButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_home_open, base.gameObject);
		if (WrapperController.Instance.InventoryManager.Inventory.vaultProgress >= 100.0)
		{
			ChangeScreen(delegate
			{
				SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext
				{
					OpenVault = true
				}, forceReload: true);
			});
		}
	}

	private void PacksButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_packmanager_open, base.gameObject);
		ChangeScreen(delegate
		{
			SceneLoader.GetSceneLoader().GoToBoosterChamber("NavBar button");
		});
	}

	private void ProfileButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_packmanager_open, base.gameObject);
		ProfileIndicator.SetActive(value: false);
		ChangeScreen(delegate
		{
			SceneLoader.GetSceneLoader().GoToProfileScreen(SceneChangeInitiator.User, "NavBar button", ProfileScreenModeEnum.Unknown, RankType.Unknown, forceReload: false, alwaysInit: true);
		});
	}

	private void Mastery_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_packmanager_open, base.gameObject);
		ChangeScreen(delegate
		{
			SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
			sceneLoader.GoToProgressionTrackScene(new ProgressionTrackPageContext(null, NavContentType.None, sceneLoader.CurrentContentType), "NavBar button", forceReload: false, alwaysInit: true);
		});
	}

	private void AchievementsButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_packmanager_open, base.gameObject);
		ChangeScreen(delegate
		{
			SceneLoader.GetSceneLoader().GoToAchievementsScene("NavBar button");
		});
	}

	private void StoreButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_store_open, base.gameObject);
		ChangeScreen(delegate
		{
			SceneLoader.GetSceneLoader().GoToStore(StoreTabType.Featured, "NavBar button", forceReload: false, alwaysInit: true);
		});
	}

	private void DecksButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_packmanager_open, base.gameObject);
		ChangeScreen(delegate
		{
			SceneLoader.GetSceneLoader().GoToDeckManager();
		});
	}

	private void Coin_OnClick()
	{
		TryCloseAllObjectivePopups();
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_coins_tap, base.gameObject);
	}

	private void Gem_OnClick()
	{
		if (_onboardingState != OnboardingState.NoStore && !WrapperController.Instance.Store.StoreStatus.DisabledTags.Contains(EProductTag.Gems))
		{
			ChangeScreen(delegate
			{
				SceneLoader.GetSceneLoader().GoToStore(StoreTabType.Gems, "Gems Button on NavBar", forceReload: false, alwaysInit: true);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_gem_tap, base.gameObject);
			});
		}
	}

	private void Coin_OnHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_coins_rollover, base.gameObject);
	}

	private void Gem_OnHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_gem_rollover, base.gameObject);
	}

	private void WildCard_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_card_tap, base.gameObject);
	}

	private void WildCard_OnHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_card_rollover, base.gameObject);
	}

	private void NavButton_OnHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover_big, base.gameObject);
	}

	private void LearnButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		ChangeScreen(delegate
		{
			SceneLoader.GetSceneLoader().GoToLearnToPlay("NavBar button");
		});
	}

	private void OptionsButton_OnClick()
	{
		TryCloseAllObjectivePopups();
		WrapperController.Instance.SettingsMenuHost.Open();
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
	}

	private void MailboxButton_OnClick()
	{
		ContentControllerPlayerInbox playerInboxContentController = SceneLoader.GetSceneLoader().GetPlayerInboxContentController();
		if (playerInboxContentController != null)
		{
			if (playerInboxContentController.isActiveAndEnabled)
			{
				playerInboxContentController.Hide();
			}
			else
			{
				playerInboxContentController.Show();
				_mailboxContainer.GetComponentInChildren<NavBarMailController>().RefreshLetterCount();
				(_currentContentController as HomePageContentController)?.OnInboxOpened();
			}
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
	}

	private void TryCloseAllObjectivePopups()
	{
		HomePageContentController homePageContentController = _currentContentController as HomePageContentController;
		if (homePageContentController != null && homePageContentController.ObjectivesPanel != null)
		{
			homePageContentController.ObjectivesPanel.CloseAllObjectivePopups();
		}
	}

	public void PressedExitButton()
	{
		if ((bool)_currentContentController)
		{
			_currentContentController.OnNavBarExit(ExitAction);
		}
		else
		{
			ExitAction();
		}
		void ExitAction()
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
			SceneLoader.ApplicationQuit();
		}
	}

	private void UpdateWildcardCountAndGraphic(int commonWildcards, int uncommonWildcards, int rareWildcards, int mythicWildcards)
	{
		if (mythicWildcards > 0)
		{
			_wildcardImage.sprite = _wildcardSpriteMythic;
		}
		else if (rareWildcards > 0)
		{
			_wildcardImage.sprite = _wildcardSpriteRare;
		}
		else if (uncommonWildcards > 0)
		{
			_wildcardImage.sprite = _wildcardSpriteUncommon;
		}
		else
		{
			_wildcardImage.sprite = _wildcardSpriteCommon;
		}
	}

	private void UpdateWildcardTooltip(int commonWildcards, int uncommonWildcards, int rareWildcards, int mythicWildcards, double vaultProgress)
	{
		if (_wildcardTooltip != null)
		{
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/NavBar/WildcardsTooltip_Common", ("quantity", commonWildcards.ToString("N0")));
			string localizedText2 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/NavBar/WildcardsTooltip_Uncommon", ("quantity", uncommonWildcards.ToString("N0")));
			string localizedText3 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/NavBar/WildcardsTooltip_Rare", ("quantity", rareWildcards.ToString("N0")));
			string localizedText4 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/NavBar/WildcardsTooltip_MythicRare", ("quantity", mythicWildcards.ToString("N0")));
			NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
			numberFormatInfo.PercentPositivePattern = 1;
			numberFormatInfo.PercentDecimalDigits = 1;
			string localizedText5 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/NavBar/WildCardsTooltip_Vault", ("percent", (vaultProgress / 100.0).ToString("P", numberFormatInfo)));
			localizedText5 = "<style=\"VaultText\">" + localizedText5 + "</style>";
			_wildcardTooltip.TooltipData.Text = string.Join(Environment.NewLine, localizedText, localizedText2, localizedText3, localizedText4, localizedText5);
		}
	}

	private void AddWildcardEffect(int wildcardLevel)
	{
		switch (wildcardLevel)
		{
		case 1:
			_wildcardBonusEffectLevel1.SetActive(value: true);
			_wildcardBonusEffectLevel2.SetActive(value: false);
			break;
		case 2:
			_wildcardBonusEffectLevel1.SetActive(value: false);
			_wildcardBonusEffectLevel2.SetActive(value: true);
			break;
		default:
			_wildcardBonusEffectLevel1.SetActive(value: false);
			_wildcardBonusEffectLevel2.SetActive(value: false);
			break;
		}
	}

	private void UpdateVaultDisplay(double percent)
	{
		string item = (percent / 100.0).ToString("P1").FixCulture();
		if (_vaultTooltip != null)
		{
			_vaultTooltip.TooltipData.Text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/NavBar/VaultProgress_Tooltip", ("percent", item));
		}
	}

	public void RefreshMasteryDisplay()
	{
		SetMasteryDataProvider setMasteryDataProvider = Pantry.Get<SetMasteryDataProvider>();
		MasteryIndicator.UpdateActive(setMasteryDataProvider.ShouldShowSetMasteryHeatPip(_accountClient));
	}

	public void OnNodeStatesUpdated(ClientGraphDefinition clientGraphDef)
	{
		if (clientGraphDef.Type == Wizards.Arena.Enums.CampaignGraph.CampaignGraphType.Achievement)
		{
			StartCoroutine(RefreshAchievementsDisplay());
		}
	}

	public IEnumerator RefreshAchievementsDisplay()
	{
		CampaignGraphManager campaignGraphManager = Pantry.Get<CampaignGraphManager>();
		yield return new WaitUntil(() => campaignGraphManager.Ready);
		AchievementsIndicator.UpdateActive(ShouldAchievementIndicatorBeActive());
	}

	private bool ShouldAchievementIndicatorBeActive()
	{
		return Pantry.Get<IAchievementManager>().ClaimableAchievementCount > 0;
	}

	public void RefreshCodexNewPip()
	{
		_learnContainer.GetComponent<CodexNewPip>()?.EvaluatePip();
	}

	public void RefreshCurrencyDisplay()
	{
		ClientPlayerInventory inventory = Pantry.Get<InventoryManager>().Inventory;
		if (inventory != null)
		{
			_goldText.text = ((inventory.gold > _maxDisplayCurrency) ? $"{_maxDisplayCurrency:N0}+" : inventory.gold.ToString("N0"));
			_gemText.text = ((inventory.gems > _maxDisplayCurrency) ? $"{_maxDisplayCurrency:N0}+" : inventory.gems.ToString("N0"));
			bool flag = inventory.boosters != null && inventory.boosters.Exists((ClientBoosterInfo b) => b.count > 0);
			bool flag2 = _latestKillswitch?.IsBoosterDisabled ?? false;
			PacksIndicator.UpdateActive(!flag2 && flag);
			UpdateWildcardTooltip(inventory.wcCommon, inventory.wcUncommon, inventory.wcRare, inventory.wcMythic, inventory.vaultProgress);
			UpdateWildcardCountAndGraphic(inventory.wcCommon, inventory.wcUncommon, inventory.wcRare, inventory.wcMythic);
			List<Client_CustomTokenDefinitionWithQty> customTokensOfTypeWithQty = Pantry.Get<ICustomTokenProvider>().GetCustomTokensOfTypeWithQty(ClientTokenType.Event);
			_navBarTokenView.Init(Languages.ActiveLocProvider, _assetLookupSystem);
			_navBarTokenView.UpdateTokensTooltip(customTokensOfTypeWithQty);
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.gameObject.transform.GetComponent<RectTransform>());
			if (_firstRefresh)
			{
				base.gameObject.SetActive(value: false);
				base.gameObject.SetActive(value: true);
				_firstRefresh = false;
			}
			else if (base.gameObject.activeSelf)
			{
				StartCoroutine(Coroutine_RefreshCurrencyDisplay());
			}
			RefreshVaultProgress(inventory.vaultProgress);
		}
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape && PlatformUtils.IsHandheld())
		{
			return HandleBackButton();
		}
		return false;
	}

	public bool HandleBackButton()
	{
		if (_currentContentController == null)
		{
			ChangeScreen(delegate
			{
				SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
			});
			return true;
		}
		if (_currentContentController.NavContentType == NavContentType.Home)
		{
			PressedExitButton();
		}
		else
		{
			_currentContentController.OnHandheldBackButton();
		}
		return true;
	}

	public void ExitWithMessage()
	{
		if (!_systemExitDialogueShown)
		{
			_systemExitDialogueShown = true;
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
			SystemMessageManager.Instance.ShowOkCancel(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Confirm_ExitGame_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Confirm_ExitGame_Text"), delegate
			{
				PressedExitButton();
			}, delegate
			{
				_systemExitDialogueShown = false;
			});
		}
	}

	private IEnumerator Coroutine_RefreshCurrencyDisplay()
	{
		yield return new WaitForEndOfFrame();
		CurrenciesContainer.gameObject.SetActive(value: false);
		CurrenciesContainer.gameObject.SetActive(value: true);
	}

	private void DeckViewToggle_OnValueChanged()
	{
		DeckBuilderLayoutState.LayoutInUse = (DeckViewToggle.Value ? DeckBuilderLayout.Column : DeckBuilderLayout.List);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
	}

	public void RewardWildcard()
	{
		UnityEngine.Object.Instantiate(_wildcardsAddedFXPrefab, _wildcardsAddedFXContainer);
	}

	private void OnLanguageChange()
	{
		RefreshCurrencyDisplay();
	}

	public void OnBack(ActionContext context)
	{
		if (PlatformUtils.IsHandheld())
		{
			HandleBackButton();
		}
		else
		{
			context.Used = false;
		}
	}
}
