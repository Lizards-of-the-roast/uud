using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Core.BI;
using Core.Code.Promises;
using Core.Meta.MainNavigation.PopUps;
using Core.Shared.Code.PVPChallenge;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.PrivateGame;
using Wizards.Mtga.PrivateGame.Challenges;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Wrapper.Draft;

public class UnifiedChallengeBladeWidget : PlayBladeWidget
{
	[Header("Settings Parameters")]
	[SerializeField]
	private Animator _settingsAnimator;

	[SerializeField]
	private Spinner_OptionSelector _challengeTypeSpinner;

	[SerializeField]
	private Spinner_OptionSelector _bestOfSpinner;

	[SerializeField]
	private Spinner_OptionSelector _startingPlayerSpinner;

	[SerializeField]
	private Spinner_OptionSelector _deckTypeSpinner;

	[SerializeField]
	private GameObject _fakeOptionB01;

	[SerializeField]
	private GameObject _fakeOption60Card;

	[SerializeField]
	protected TMP_Text TournamentSettingsText;

	[Header("Opponent Parameters")]
	[SerializeField]
	private TMP_Text _opponentNameText;

	[SerializeField]
	private GameObject _deckLock;

	[Header("Timer Parameters")]
	[SerializeField]
	private DraftTimer _timer;

	[SerializeField]
	private AudioEvent _timerPulseEvent;

	[SerializeField]
	private AudioEvent _timerCriticalPulseEvent;

	[Header("Buttons")]
	[SerializeField]
	private GameObject _mainButtonGlow;

	[Header("Challenge Status")]
	[SerializeField]
	private Localize _challengeStatusText;

	private static readonly int ANIMATOR_EXPAND_HASH = Animator.StringToHash("Expand");

	private static readonly int ANIMATOR_TOURNAMENT_HASH = Animator.StringToHash("Tournament");

	private static readonly int ANIMATOR_LOCKED_HASH = Animator.StringToHash("Locked");

	private EventContext _eventContext;

	private PopupManager _popUpManager;

	private bool _isChallengeSettingsLocked;

	private Dictionary<int, bool> _timerStatusBySecond = new Dictionary<int, bool>();

	public UnityEvent UEvOnShow = new UnityEvent();

	private System.Timers.Timer _countdownTimer;

	private bool IsChallengeSettingsLocked
	{
		get
		{
			return _isChallengeSettingsLocked;
		}
		set
		{
			_isChallengeSettingsLocked = value;
			_settingsAnimator.SetBool(ANIMATOR_LOCKED_HASH, value);
		}
	}

	private void Awake()
	{
		InitChallengeController();
		_mainButton.OnClick.AddListener(OnMainOrSecondaryButtonClicked);
		_secondaryButton.OnClick.AddListener(OnMainOrSecondaryButtonClicked);
		_selectDeckSlot.OnClick.AddListener(OnSelectDeckClicked);
		_challengeTypeSpinner.onValueChanged.RemoveAllListeners();
		_challengeTypeSpinner.ClearOptions();
		_challengeTypeSpinner.AddOptions(ChallengeSpinnerMatchTypes.Labels.Select((string _) => Languages.ActiveLocProvider.GetLocalizedText(_)).ToList());
		_challengeTypeSpinner.SelectOption(ChallengeSpinnerMatchTypes.DefaultIndex);
		_challengeTypeSpinner.onValueChanged.AddListener(OnChallengeTypeChanged);
		_deckTypeSpinner.onValueChanged.RemoveAllListeners();
		_deckTypeSpinner.ClearOptions();
		_deckTypeSpinner.AddOptions(ChallengeSpinnerDeckTypes.Labels.Select((string _) => Languages.ActiveLocProvider.GetLocalizedText(_)).ToList());
		_deckTypeSpinner.SelectOption(0);
		_deckTypeSpinner.onValueChanged.AddListener(OnDeckTypeChanged);
		_bestOfSpinner.onValueChanged.RemoveAllListeners();
		_bestOfSpinner.ClearOptions();
		_bestOfSpinner.AddOptions(new List<string>
		{
			Languages.ActiveLocProvider.GetLocalizedText("MainNav/PrivateGame/Settings/ChallengeFormat_BestOf1"),
			Languages.ActiveLocProvider.GetLocalizedText("MainNav/PrivateGame/Settings/ChallengeFormat_BestOf3")
		});
		_bestOfSpinner.SelectOption(0);
		_bestOfSpinner.onValueChanged.AddListener(OnBestOfSpinnerChanged);
		_startingPlayerSpinner.onValueChanged.RemoveAllListeners();
		_startingPlayerSpinner.ClearOptions();
		_startingPlayerSpinner.AddOptions(new List<string>
		{
			Languages.ActiveLocProvider.GetLocalizedText("MainNav/PrivateGame/Settings/StartingPlayer_Random"),
			Languages.ActiveLocProvider.GetLocalizedText("MainNav/PrivateGame/Settings/StartingPlayer_Challenger"),
			Languages.ActiveLocProvider.GetLocalizedText("MainNav/PrivateGame/Settings/StartingPlayer_Opponent")
		});
		_startingPlayerSpinner.SelectOption(0);
		_startingPlayerSpinner.onValueChanged.AddListener(OnStartingPlayerChanged);
	}

	private void Start()
	{
		if (TryGetCurrentChallengeData(out var challengeData) && challengeData.ChallengePlayers.ContainsKey(challengeData.LocalPlayerId))
		{
			_playBlade.HideDeckSelector();
		}
	}

	private void OnEnable()
	{
		_eventContext = Pantry.Get<EventManager>().PrivateGameEventContext;
		_popUpManager = Pantry.Get<PopupManager>();
		_settingsAnimator.SetBool(ANIMATOR_EXPAND_HASH, value: true);
		_settingsAnimator.SetBool(ANIMATOR_LOCKED_HASH, IsChallengeSettingsLocked);
		SetSpinnersFromChallenge();
		_challengeController.RegisterForChallengeChanges(OnChallengeDataChanged);
		if (_popUpManager.HasActivePopup())
		{
			_popUpManager.CloseActivePopup();
		}
	}

	private void OnDisable()
	{
		CurrentChallengeId = Guid.Empty;
		_challengeController.UnRegisterForChallengeChanges(OnChallengeDataChanged);
	}

	public void OnChallengeDataChanged(PVPChallengeData data)
	{
		if (CurrentChallengeId == data.ChallengeId)
		{
			UpdateView();
			if (data.Status == ChallengeStatus.Removed || _challengeController.GetChallengeData(data.ChallengeId) == null || (!data.ChallengePlayers.ContainsKey(data.LocalPlayerId) && !data.Invites.ContainsKey(data.LocalPlayerId)))
			{
				_playBlade.Hide();
			}
		}
	}

	public override void SetChallengeDeck(Client_Deck deck, DeckFormat deckFormat)
	{
		_challengeController.SetDeckForChallenge(CurrentChallengeId, deck?.Id ?? Guid.Empty);
	}

	private void UpdateView()
	{
		if (TryGetCurrentChallengeData(out var challengeData))
		{
			IsChallengeSettingsLocked = challengeData.ChallengeOwnerId != challengeData.LocalPlayerId || challengeData.Status == ChallengeStatus.Starting;
			if (challengeData.Status != ChallengeStatus.None)
			{
				_opponentNameText.text = challengeData.OpponentFullName;
				SetSpinnersFromChallenge();
				UpdateDeck(challengeData);
			}
			_timer.gameObject.SetActive(challengeData.Status == ChallengeStatus.Starting);
			UpdateButton(challengeData);
		}
	}

	private void UpdateDeck(PVPChallengeData challengeData)
	{
		bool flag = challengeData.GetLocalPlayer().DeckId != Guid.Empty;
		if (flag)
		{
			Client_Deck deck = WrapperController.Instance.DecksManager.GetDeck(challengeData.LocalPlayer.DeckId);
			if (deck != null)
			{
				SetDeck(deck, GetDeckFormat());
			}
			flag = deck != null;
		}
		else
		{
			SetDeck(null, null);
		}
		if (!base.SelectedDeckView.IsUnityNull())
		{
			base.SelectedDeckView.gameObject.UpdateActive(flag);
			if (challengeData.Status == ChallengeStatus.Setup)
			{
				base.SelectedDeckView.SetIsUnavailable(unavailable: false);
			}
		}
		if (!PlatformUtils.IsHandheldNon4x3())
		{
			_selectDeckSlot.gameObject.UpdateActive(!flag);
		}
	}

	private void UpdateCountdown(Guid challengeId)
	{
		PVPChallengeData challengeData = _challengeController.GetChallengeData(challengeId);
		if (challengeData == null || !(challengeData.MatchLaunchDateTime != DateTime.MinValue))
		{
			return;
		}
		TimeSpan timeSpan = challengeData.MatchLaunchDateTime - DateTime.Now;
		_timer.UpdateTime((float)timeSpan.TotalSeconds, challengeData.MatchLaunchCountdown);
		bool num = _challengeController.IsChallengeLocked(challengeId);
		float num2 = (float)timeSpan.TotalSeconds / (float)challengeData.MatchLaunchCountdown * 10f;
		float num3 = Mathf.Abs(num2 % 1f);
		int key = Mathf.FloorToInt(num2 - num3);
		bool flag = false;
		if (!_timerStatusBySecond.ContainsKey(key))
		{
			_timerStatusBySecond.Add(key, value: true);
			flag = true;
		}
		if (num)
		{
			if (flag)
			{
				AudioManager.PlayAudio(_timerPulseEvent, _timer.gameObject);
			}
			UpdateMainOrSecondaryButton(main: false, interactable: false, "MainNav/Challenges/MainButton/StartingMatch", "Starting");
			UpdateChallengeStatusText("");
			base.SelectedDeckView.SetIsUnavailable(unavailable: true);
			if (PlatformUtils.IsHandheldNon4x3())
			{
				_selectDeckSlot.gameObject.SetActive(value: false);
			}
		}
		else
		{
			if (flag)
			{
				AudioManager.PlayAudio(_timerCriticalPulseEvent, _timer.gameObject);
			}
			UpdateMainOrSecondaryButton(main: false, interactable: true, "MainNav/Challenges/MainButton/Cancel", "Cancel");
			UpdateChallengeStatusText("MainNav/Challenges/MainButtonDescription/Cancel", "Starting");
			base.SelectedDeckView.SetIsUnavailable(unavailable: false);
			if (PlatformUtils.IsHandheldNon4x3())
			{
				_selectDeckSlot.gameObject.SetActive(value: true);
			}
		}
	}

	private void UpdateButton(PVPChallengeData challengeData)
	{
		if (challengeData.Status == ChallengeStatus.Starting && _countdownTimer == null)
		{
			_countdownTimer = new System.Timers.Timer(30.0);
			UpdateCountdown(challengeData.ChallengeId);
			_countdownTimer.Elapsed += delegate
			{
				if (challengeData.Status == ChallengeStatus.Starting)
				{
					MainThreadDispatcher.Instance.Add(delegate
					{
						UpdateCountdown(challengeData.ChallengeId);
					});
				}
				else
				{
					_timerStatusBySecond.Clear();
					_countdownTimer.Enabled = false;
					_countdownTimer.Stop();
					_countdownTimer = null;
				}
			};
			_countdownTimer.Enabled = true;
			_countdownTimer.Start();
		}
		else if (challengeData.Status != ChallengeStatus.Starting && _countdownTimer != null)
		{
			_countdownTimer.Enabled = false;
			_countdownTimer.Stop();
			_countdownTimer = null;
		}
		if (_mainButtonGlow != null)
		{
			_mainButtonGlow.gameObject.UpdateActive(active: false);
		}
		if (PlatformUtils.IsHandheldNon4x3())
		{
			if (_playBlade.DeckSelector.IsShowing)
			{
				_selectDeckSlot.gameObject.SetActive(value: false);
			}
			else
			{
				_selectDeckSlot.gameObject.SetActive(value: true);
			}
		}
		switch (challengeData.Status)
		{
		case ChallengeStatus.None:
			UpdateMainOrSecondaryButton(main: true, interactable: true, "MainNav/FriendChallenge/Accept_Button");
			break;
		case ChallengeStatus.Setup:
		{
			if (!_playBlade.DeckSelector.IsShowing && challengeData.ChallengePlayers.Count < 2)
			{
				UpdateMainOrSecondaryButton(main: true, interactable: false, "MainNav/Challenges/MainButton/Waiting", "Waiting");
				if (!challengeData.Invites.Exists((KeyValuePair<string, ChallengeInvite> invite) => invite.Value.Status == InviteStatus.Sent))
				{
					UpdateChallengeStatusText("MainNav/Challenges/MainButton/NoInvitesDescription", "Invite an opponent");
				}
				else
				{
					UpdateChallengeStatusText("MainNav/Challenges/MainButtonDescription/Waiting", "Waiting for an opponent");
				}
				return;
			}
			if (!_challengeController.CheckLocalPlayerDeckValid(challengeData.ChallengeId))
			{
				UpdateMainOrSecondaryButton(main: true, interactable: false, "MainNav/Challenges/InvalidDeck", "Invalid Deck");
				UpdateChallengeStatusText("MainNav/Challenges/MainButtonDescription/SelectDeck", "Select a deck");
				return;
			}
			if (_playBlade.DeckSelector.IsShowing)
			{
				UpdateMainOrSecondaryButton(main: false, interactable: true, "MainNav/Challenges/MainButton/SelectDeck", "Select Deck");
				UpdateChallengeStatusText("MainNav/Challenges/MainButtonDescription/SelectDeck", "Create or choose a valid deck");
				break;
			}
			ChallengePlayer localPlayer = challengeData.LocalPlayer;
			if (localPlayer != null && localPlayer.PlayerStatus == PlayerStatus.Ready)
			{
				UpdateMainOrSecondaryButton(main: false, interactable: true, "MainNav/Challenges/MainButton/Unready", "Not Ready");
				UpdateChallengeStatusText("MainNav/Challenges/MainButtonDescription/Unready", "Waiting for all players to select valid deck and ready");
			}
			else
			{
				UpdateMainOrSecondaryButton(main: true, interactable: true, "MainNav/Challenges/MainButton/Ready", "Ready");
				UpdateChallengeStatusText("MainNav/Challenges/MainButtonDescription/Ready", "Waiting for all players to select valid deck and ready");
			}
			break;
		}
		}
		if (!_challengeController.AllPlayersReady(challengeData) || challengeData.Status == ChallengeStatus.Starting)
		{
			return;
		}
		if (challengeData.ChallengeOwnerId == challengeData.LocalPlayerId)
		{
			UpdateMainOrSecondaryButton(main: true, interactable: true, "MainNav/Challenges/MainButton/StartMatch", "Play");
			UpdateChallengeStatusText("");
			AudioManager.PlayAudio("sfx_ui_unified_challenges_loop", AudioManager.Default);
			if (_mainButtonGlow != null)
			{
				_mainButtonGlow.gameObject.UpdateActive(active: true);
			}
		}
		else
		{
			UpdateChallengeStatusText("MainNav/Challenges/MainButtonDescription/WaitingForHost", "Waiting for Host to start game");
		}
	}

	private void UpdateMainOrSecondaryButton(bool main, bool interactable, string key, string fallbackText = "")
	{
		if (main)
		{
			_secondaryButton.gameObject.UpdateActive(active: false);
			_mainButton.gameObject.UpdateActive(active: true);
			_mainButton.Interactable = interactable;
			_mainButtonText.SetText(key, null, fallbackText);
		}
		else
		{
			_mainButton.gameObject.UpdateActive(active: false);
			_secondaryButton.gameObject.UpdateActive(active: true);
			_secondaryButton.Interactable = interactable;
			_secondaryButtonText.SetText(key, null, fallbackText);
		}
	}

	private void UpdateChallengeStatusText(string key, string fallbackText = " ")
	{
		_challengeStatusText.SetText(key, null, fallbackText);
	}

	public override void Show()
	{
		base.gameObject.UpdateActive(active: true);
		if (_objectivesController.AnimatorShowing)
		{
			_objectivesController.AnimateOutro();
		}
		_playBlade.DeckSelector.SetShouldAnimateObjectiveTrack(isVisible: false);
		if (UEvOnShow != null)
		{
			UEvOnShow.Invoke();
		}
	}

	public override bool ProceedWithHide()
	{
		if (TryGetCurrentChallengeData(out var challengeData) && challengeData.ChallengePlayers.ContainsKey(challengeData.LocalPlayerId) && !_challengeController.LeaveChallenge(challengeData.ChallengeId))
		{
			return false;
		}
		return true;
	}

	public override bool Hide()
	{
		if (TryGetCurrentChallengeData(out var challengeData) && challengeData.ChallengePlayers.ContainsKey(challengeData.LocalPlayerId))
		{
			return false;
		}
		if (!_objectivesController.AnimatorShowing)
		{
			_objectivesController.AnimateIntro();
		}
		_playBlade.DeckSelector.SetShouldAnimateObjectiveTrack(isVisible: true);
		AudioManager.PlayAudio("sfx_ui_unified_challenges_loop_stop", AudioManager.Default);
		return base.Hide();
	}

	protected override void SetDeck(Client_Deck deck, DeckFormat deckFormat)
	{
		base.SetDeck(deck, deckFormat);
		if (!base.IsDeckSelected && !PlatformUtils.IsHandheldNon4x3())
		{
			_selectDeckSlot.gameObject.UpdateActive(active: false);
		}
		else if (PlatformUtils.IsHandheldNon4x3())
		{
			_selectDeckHandheldText.SetText("MainNav/Challenges/MainButton/SelectDeck");
		}
		else
		{
			base.SelectedDeckView.SetToolTipLocString("MainNav/HomePage/EventBlade/Tooltip_ChooseDeck");
		}
	}

	public void ViewFriendChallenge(string playerId)
	{
		PVPChallengeData challengeData = _challengeController.GetChallengeData(playerId);
		if (challengeData != null)
		{
			ViewFriendChallenge(challengeData.ChallengeId);
		}
	}

	public void ViewFriendChallenge(Guid challengeId)
	{
		PVPChallengeData challengeData = _challengeController.GetChallengeData(challengeId);
		if (challengeData != null)
		{
			CurrentChallengeId = challengeData.ChallengeId;
			_challengeController.SetDeckForChallengeFromPrefs(challengeData.ChallengeId);
			UpdateView();
		}
	}

	private void SetSpinnersFromChallenge()
	{
		if (TryGetCurrentChallengeData(out var challengeData))
		{
			SetChallengeOption_StartingPlayer(challengeData.StartingPlayer);
			SetChallengeOption_BestOfSettings(challengeData.IsBestOf3);
			SetChallengeOption_ChallengeType(challengeData.MatchType);
		}
	}

	private WhoPlaysFirst GetChallengeOption_StartingPlayer()
	{
		return _startingPlayerSpinner.ValueIndex switch
		{
			0 => WhoPlaysFirst.Random, 
			1 => WhoPlaysFirst.LocalPlayer, 
			2 => WhoPlaysFirst.Opponent, 
			_ => WhoPlaysFirst.Random, 
		};
	}

	private void SetChallengeOption_StartingPlayer(WhoPlaysFirst startingPlayer)
	{
		_startingPlayerSpinner.onValueChanged.RemoveAllListeners();
		switch (startingPlayer)
		{
		case WhoPlaysFirst.Random:
			_startingPlayerSpinner.ValueIndex = 0;
			break;
		case WhoPlaysFirst.LocalPlayer:
			_startingPlayerSpinner.ValueIndex = 1;
			break;
		case WhoPlaysFirst.Opponent:
			_startingPlayerSpinner.ValueIndex = 2;
			break;
		}
		_startingPlayerSpinner.onValueChanged.AddListener(OnStartingPlayerChanged);
	}

	private void SetChallengeOption_BestOfSettings(bool bestOf3)
	{
		_bestOfSpinner.onValueChanged.RemoveAllListeners();
		_bestOfSpinner.ValueIndex = (bestOf3 ? 1 : 0);
		_bestOfSpinner.onValueChanged.AddListener(OnBestOfSpinnerChanged);
	}

	private ChallengeMatchTypes GetChallengeOption_Mode()
	{
		return PrivateGameUtils.GetGameModeFromSpinners(_challengeTypeSpinner.ValueIndex, _deckTypeSpinner.ValueIndex);
	}

	private void SetChallengeOption_ChallengeType(ChallengeMatchTypes challengeMatchType)
	{
		_deckTypeSpinner.onValueChanged.RemoveAllListeners();
		_challengeTypeSpinner.onValueChanged.RemoveAllListeners();
		ChallengeTypeInfo challengeTypeInfo = ChallengeUtils.MatchTypeToInfo[challengeMatchType];
		ChallengeSpinnerMatchType challengeSpinnerMatchType = (ChallengeSpinnerMatchType)ChallengeSpinnerMatchTypes.All[challengeTypeInfo.MatchType];
		ChallengeSpinnerDeckType? challengeSpinnerDeckType = (challengeTypeInfo.DeckType.HasValue ? ((ChallengeSpinnerDeckType?)ChallengeSpinnerDeckTypes.All[challengeTypeInfo.DeckType.Value]) : ((ChallengeSpinnerDeckType?)null));
		_settingsAnimator.SetBool(ANIMATOR_TOURNAMENT_HASH, challengeSpinnerMatchType.IsTournament);
		_challengeTypeSpinner.ValueIndex = (int)challengeTypeInfo.MatchType;
		if (challengeTypeInfo.DeckType.HasValue)
		{
			_deckTypeSpinner.ValueIndex = (int)challengeTypeInfo.DeckType.Value;
		}
		_bestOfSpinner.gameObject.SetActive(!challengeSpinnerMatchType.IsTournament && (challengeSpinnerDeckType?.AllowBo3 ?? false));
		_fakeOptionB01.SetActive(!challengeSpinnerMatchType.IsTournament && challengeSpinnerDeckType.HasValue && !challengeSpinnerDeckType.GetValueOrDefault().AllowBo3);
		_deckTypeSpinner.gameObject.SetActive(!challengeSpinnerMatchType.IsTournament);
		switch (challengeMatchType)
		{
		case ChallengeMatchTypes.DirectGameTournamentHistoric:
			_deckTypeSpinner.ValueIndex = 0;
			break;
		case ChallengeMatchTypes.DirectGameTournamentAlchemy:
			_deckTypeSpinner.ValueIndex = 3;
			break;
		}
		if (challengeSpinnerMatchType.IsTournament)
		{
			TournamentSettingsText.text = Languages.ActiveLocProvider.GetLocalizedText(challengeSpinnerMatchType.TournamentText);
		}
		else
		{
			_fakeOption60Card.SetActive(value: false);
		}
		_deckTypeSpinner.onValueChanged.AddListener(OnDeckTypeChanged);
		_challengeTypeSpinner.onValueChanged.AddListener(OnChallengeTypeChanged);
	}

	public override DeckFormat GetDeckFormat()
	{
		return WrapperController.Instance.FormatManager.GetSafeFormat(PrivateGameUtils.GetDeckTypeFromSpinners(_challengeTypeSpinner.ValueIndex, _deckTypeSpinner.ValueIndex));
	}

	protected override void OnSelectDeckClicked()
	{
		_playBlade.OnSelectDeckClicked(_eventContext, GetDeckFormat(), null);
		if (TryGetCurrentChallengeData(out var challengeData))
		{
			_challengeController.SetLocalPlayerStatus(challengeData.ChallengeId, PlayerStatus.NotReady);
			UpdateButton(challengeData);
		}
		BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengeScreenDeckSelectorOpened.ToString()), ("ChallengeId", challengeData.ChallengeId.ToString()));
	}

	protected override void OnMainOrSecondaryButtonClicked()
	{
		if (!TryGetCurrentChallengeData(out var challengeData))
		{
			return;
		}
		if (_challengeController.AllPlayersReady(challengeData) && challengeData.Status != ChallengeStatus.Starting && challengeData.ChallengeOwnerId == challengeData.LocalPlayerId)
		{
			AudioManager.PlayAudio("sfx_ui_unified_challenges_burst", AudioManager.Default);
			AudioManager.PlayAudio("sfx_ui_unified_challenges_loop_stop", AudioManager.Default);
			BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengeReadyFlowComplete.ToString()), ("ChallengeId", challengeData.ChallengeId.ToString()), ("ChallengeStatus", challengeData.Status.ToString()), ("MatchLaunchCountdown", challengeData.MatchLaunchCountdown.ToString()), ("MatchLaunchDateTime", challengeData.MatchLaunchDateTime.ToString()), ("MatchType", challengeData.MatchType.ToString()));
			_challengeController.LaunchChallenge(challengeData.ChallengeId);
			return;
		}
		switch (challengeData.Status)
		{
		case ChallengeStatus.None:
			_challengeController.AcceptChallengeInvite(challengeData.ChallengeId);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
			break;
		case ChallengeStatus.Setup:
			if (_playBlade.DeckSelector.IsShowing)
			{
				_playBlade.HideDeckSelector();
				if (TryGetCurrentChallengeData(out var challengeData2))
				{
					UpdateButton(challengeData2);
				}
			}
			else
			{
				ChallengePlayer localPlayer = challengeData.LocalPlayer;
				if (localPlayer != null && localPlayer.PlayerStatus == PlayerStatus.Ready)
				{
					_challengeController.SetLocalPlayerStatus(challengeData.ChallengeId, PlayerStatus.NotReady);
				}
				else
				{
					_challengeController.SetLocalPlayerStatus(challengeData.ChallengeId, PlayerStatus.Ready);
				}
			}
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
			break;
		case ChallengeStatus.Starting:
			if (!_challengeController.IsChallengeLocked(challengeData.ChallengeId))
			{
				_challengeController.SetLocalPlayerStatus(challengeData.ChallengeId, PlayerStatus.NotReady);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_cancel, base.gameObject);
			}
			break;
		}
	}

	private void OnChallengeTypeChanged(int index, string value)
	{
		ChallengeSpinnerMatchType challengeSpinnerMatchType = (ChallengeSpinnerMatchType)ChallengeSpinnerMatchTypes.All[index];
		ChallengeSpinnerDeckType challengeSpinnerDeckType = (ChallengeSpinnerDeckType)ChallengeSpinnerDeckTypes.All[_deckTypeSpinner.ValueIndex];
		bool flag = false;
		_settingsAnimator.SetBool(ANIMATOR_TOURNAMENT_HASH, challengeSpinnerMatchType.IsTournament);
		_deckTypeSpinner.gameObject.SetActive(!challengeSpinnerMatchType.IsTournament);
		_bestOfSpinner.gameObject.SetActive(!challengeSpinnerMatchType.IsTournament && challengeSpinnerDeckType.AllowBo3);
		_fakeOptionB01.SetActive(!challengeSpinnerMatchType.IsTournament && !challengeSpinnerDeckType.AllowBo3);
		if (challengeSpinnerMatchType.IsTournament)
		{
			TournamentSettingsText.text = Languages.ActiveLocProvider.GetLocalizedText(challengeSpinnerMatchType.TournamentText);
			flag = true;
		}
		else
		{
			_fakeOption60Card.SetActive(value: false);
		}
		if (TryGetCurrentChallengeData(out var challengeData))
		{
			_challengeController.SetGameSettings(challengeData.ChallengeId, GetChallengeOption_Mode(), challengeData.StartingPlayer, flag || challengeData.IsBestOf3);
		}
		RefreshDeckSelector(allowRefresh: true);
	}

	private void OnDeckTypeChanged(int index, string value)
	{
		ChallengeSpinnerMatchType challengeSpinnerMatchType = (ChallengeSpinnerMatchType)ChallengeSpinnerMatchTypes.All[_challengeTypeSpinner.ValueIndex];
		ChallengeSpinnerDeckType challengeSpinnerDeckType = (ChallengeSpinnerDeckType)ChallengeSpinnerDeckTypes.All[index];
		_fakeOptionB01.SetActive(!challengeSpinnerMatchType.IsTournament && !challengeSpinnerDeckType.AllowBo3);
		_bestOfSpinner.gameObject.SetActive(!challengeSpinnerMatchType.IsTournament && challengeSpinnerDeckType.AllowBo3);
		if (TryGetCurrentChallengeData(out var challengeData))
		{
			if (!challengeSpinnerDeckType.AllowBo3 && _bestOfSpinner.ValueIndex == 1)
			{
				challengeData.IsBestOf3 = false;
			}
			_challengeController.SetGameSettings(challengeData.ChallengeId, GetChallengeOption_Mode(), challengeData.StartingPlayer, challengeData.IsBestOf3);
		}
		RefreshDeckSelector(allowRefresh: true);
	}

	private void OnBestOfSpinnerChanged(int index, string value)
	{
		if (TryGetCurrentChallengeData(out var challengeData))
		{
			_challengeController.SetGameSettings(challengeData.ChallengeId, challengeData.MatchType, challengeData.StartingPlayer, _bestOfSpinner.ValueIndex == 1);
		}
	}

	private void OnStartingPlayerChanged(int index, string value)
	{
		if (TryGetCurrentChallengeData(out var challengeData))
		{
			_challengeController.SetGameSettings(challengeData.ChallengeId, challengeData.MatchType, GetChallengeOption_StartingPlayer(), challengeData.IsBestOf3);
		}
	}

	private void RefreshDeckSelector(bool allowRefresh)
	{
		_playBlade.ShowDeckSelector(_eventContext, GetDeckFormat(), null, allowRefresh);
		if (TryGetCurrentChallengeData(out var challengeData))
		{
			_challengeController.SetLocalPlayerStatus(challengeData.ChallengeId, PlayerStatus.NotReady);
			UpdateButton(challengeData);
		}
	}
}
