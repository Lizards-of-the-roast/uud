using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Promises;
using Core.Meta.MainNavigation.PopoutMenu.Challenges;
using MTGA.Social;
using TMPro;
using UnityEngine;
using Wizards.Arena.Promises;
using cTMP;

namespace Wizards.Mtga.UnifiedChallenges;

public class ChallengeInviteWindowPopup : PopupBase
{
	[Header("Dismiss Button")]
	[SerializeField]
	private CustomButton _dismissButton;

	[Header("Opponent Username Parameters")]
	[SerializeField]
	protected GameObject _wizardsChallengeNameContainer;

	[SerializeField]
	protected TMP_InputField _opponentUsernameInputter;

	[SerializeField]
	protected cTMP_Dropdown _recentChallengesDropDown;

	[SerializeField]
	protected TMP_Text _opponentTitleLabel;

	[Header("Invited Entity List")]
	[SerializeField]
	protected ChallengeInviteWindowEntityInvited _invitedEntityPrefab;

	[SerializeField]
	protected Transform _invitedEntityListAnchor;

	[Header("Friend Entity List")]
	[SerializeField]
	protected ChallengeInviteWindowEntityPlayer _friendEntityPrefab;

	[SerializeField]
	protected Transform _friendEntityListAnchor;

	[Header("Invite Button")]
	[SerializeField]
	protected CustomButton _inviteButton;

	private static readonly int Disabled = Animator.StringToHash("Disabled");

	private Animator _inviteButtonAnimator;

	public Action DismissButtonPressed;

	public Action<ChallengeInviteWindowPlayerData> InviteButtonPressed;

	private ISocialManager _socialManager;

	private ChallengeInviteWindowPlayerData _challengeInviteWindowPlayerData;

	private List<ChallengeInviteWindowEntityInvited> _challengeInviteWindowEntityInvitedList;

	private List<ChallengeInviteWindowEntityPlayer> _challengeInviteWindowEntityPlayerList;

	private List<string> _recentChallengeHistory;

	private bool DisplayNameValidated;

	private string DropdownPlayerId;

	protected override void Awake()
	{
		base.Awake();
		_socialManager = Pantry.Get<ISocialManager>();
		_inviteButtonAnimator = _inviteButton.GetComponent<Animator>();
		SetDefaultUsername();
	}

	private void OnEnable()
	{
		_dismissButton?.OnClick.AddListener(OnDismissButtonPressed);
		_inviteButton?.OnClick.AddListener(OnInviteButtonPressed);
		_opponentUsernameInputter.onValueChanged.AddListener(OnDropdownTextUpdated);
		_opponentUsernameInputter.onEndEdit.AddListener(OnInputEndEdit);
		_wizardsChallengeNameContainer.gameObject.SetActive(value: true);
		_challengeInviteWindowEntityInvitedList = new List<ChallengeInviteWindowEntityInvited>();
		_challengeInviteWindowEntityPlayerList = new List<ChallengeInviteWindowEntityPlayer>();
		_challengeInviteWindowPlayerData = new ChallengeInviteWindowPlayerData();
		RefreshView();
	}

	private void OnDisable()
	{
		_dismissButton?.OnClick.RemoveAllListeners();
		_inviteButton?.OnClick.RemoveAllListeners();
		_opponentUsernameInputter.onValueChanged.RemoveAllListeners();
		_opponentUsernameInputter.onEndEdit.RemoveAllListeners();
		ClearTheLists();
	}

	public override void OnEscape()
	{
		OnDismissButtonPressed();
	}

	public override void OnEnter()
	{
		OnInviteButtonPressed();
	}

	protected override void Show()
	{
		base.Show();
	}

	protected override void Hide()
	{
		base.Hide();
	}

	public void SetDefaultUsername()
	{
		_opponentUsernameInputter.text = _socialManager?.LocalPlayer?.FullName;
	}

	public void RefreshView(ChallengeInviteWindowPlayerData challengeInviteWindowPlayerData = null)
	{
		_challengeInviteWindowPlayerData = challengeInviteWindowPlayerData ?? _challengeInviteWindowPlayerData;
		List<ChallengeInviteWindowPlayer> list = new List<ChallengeInviteWindowPlayer>();
		if (_challengeInviteWindowEntityPlayerList != null)
		{
			foreach (ChallengeInviteWindowEntityPlayer challengeInviteWindowEntityPlayer2 in _challengeInviteWindowEntityPlayerList)
			{
				if (challengeInviteWindowEntityPlayer2.Player.Invited)
				{
					list.Add(challengeInviteWindowEntityPlayer2.Player);
				}
			}
		}
		ClearTheLists();
		foreach (ChallengeInviteWindowPlayer invitedPlayer in _challengeInviteWindowPlayerData.InvitedPlayers)
		{
			ChallengeInviteWindowEntityInvited challengeInviteWindowEntityInvited = UnityEngine.Object.Instantiate(_invitedEntityPrefab, _invitedEntityListAnchor);
			challengeInviteWindowEntityInvited.Init(invitedPlayer);
			challengeInviteWindowEntityInvited.gameObject.SetActive(value: true);
			_challengeInviteWindowEntityInvitedList.Add(challengeInviteWindowEntityInvited);
		}
		foreach (ChallengeInviteWindowPlayer player in _challengeInviteWindowPlayerData.FriendPlayers)
		{
			ChallengeInviteWindowEntityPlayer challengeInviteWindowEntityPlayer = UnityEngine.Object.Instantiate(_friendEntityPrefab, _friendEntityListAnchor);
			if (list.Where((ChallengeInviteWindowPlayer p) => p.PlayerId == player.PlayerId).Any())
			{
				player.Invited = true;
			}
			challengeInviteWindowEntityPlayer.Init(player);
			challengeInviteWindowEntityPlayer.gameObject.SetActive(value: true);
			_challengeInviteWindowEntityPlayerList.Add(challengeInviteWindowEntityPlayer);
			challengeInviteWindowEntityPlayer.InviteToggleChanged = (Action<ChallengeInviteWindowPlayer>)Delegate.Combine(challengeInviteWindowEntityPlayer.InviteToggleChanged, new Action<ChallengeInviteWindowPlayer>(OnPlayerInviteToggled));
		}
		UpdateInviteButtonActive();
		_recentChallengeHistory = MDNPlayerPrefs.GetRecentChallenges(_accountClient?.AccountInformation?.DisplayName);
		_recentChallengesDropDown.onValueChanged.RemoveAllListeners();
		if (_recentChallengeHistory.Count > 0)
		{
			_recentChallengesDropDown.value = -1;
			_recentChallengesDropDown.gameObject.SetActive(value: true);
			_recentChallengesDropDown.ClearOptions();
			_recentChallengesDropDown.AddOptions(_recentChallengeHistory);
		}
		_recentChallengesDropDown.onValueChanged.AddListener(OnOpponentInput_DropDownItemSelected);
	}

	private void ClearTheLists()
	{
		while (_challengeInviteWindowEntityPlayerList.Count > 0)
		{
			ChallengeInviteWindowEntityPlayer challengeInviteWindowEntityPlayer = _challengeInviteWindowEntityPlayerList[0];
			challengeInviteWindowEntityPlayer.InviteToggleChanged = (Action<ChallengeInviteWindowPlayer>)Delegate.Remove(challengeInviteWindowEntityPlayer.InviteToggleChanged, new Action<ChallengeInviteWindowPlayer>(OnPlayerInviteToggled));
			UnityEngine.Object.Destroy(_challengeInviteWindowEntityPlayerList[0].gameObject);
			_challengeInviteWindowEntityPlayerList.RemoveAt(0);
		}
		while (_challengeInviteWindowEntityInvitedList.Count > 0)
		{
			UnityEngine.Object.Destroy(_challengeInviteWindowEntityInvitedList[0].gameObject);
			_challengeInviteWindowEntityInvitedList.RemoveAt(0);
		}
	}

	private void OnDismissButtonPressed()
	{
		DismissButtonPressed?.Invoke();
		Hide();
	}

	private Promise<string> ValidateDisplayName()
	{
		return _socialManager.GetPlayerIdFromFullPlayerName(_opponentUsernameInputter.text).ThenOnMainThread(delegate(Promise<string> result)
		{
			if (result.Successful)
			{
				DropdownPlayerId = result.Result;
			}
			else
			{
				DropdownPlayerId = null;
			}
		});
	}

	private bool DisplayNameFormatValidation()
	{
		DisplayNameValidated = _opponentUsernameInputter.text.Contains('#') && !string.Equals(_opponentUsernameInputter.text, _socialManager.LocalPlayer.FullName, StringComparison.CurrentCultureIgnoreCase) && !_challengeInviteWindowEntityInvitedList.Exists((ChallengeInviteWindowEntityInvited p) => string.Equals(p.Player.PlayerName, _opponentUsernameInputter.text, StringComparison.CurrentCultureIgnoreCase));
		return DisplayNameValidated;
	}

	private bool IsThereAValidInviteThatWeCanSendRightNow()
	{
		DisplayNameFormatValidation();
		if (!DisplayNameValidated)
		{
			return _challengeInviteWindowPlayerData.FriendPlayers.Where((ChallengeInviteWindowPlayer p) => p.Invited).Any();
		}
		return true;
	}

	private void UpdateInviteButtonActive()
	{
		bool flag = IsThereAValidInviteThatWeCanSendRightNow();
		_inviteButtonAnimator.SetBool(Disabled, !flag);
		_inviteButton.Interactable = flag;
	}

	private void OnInviteButtonPressed()
	{
		if (!IsThereAValidInviteThatWeCanSendRightNow())
		{
			return;
		}
		ValidateDisplayName().ThenOnMainThread(delegate(Promise<string> _)
		{
			if (DisplayNameValidated)
			{
				_challengeInviteWindowPlayerData.FriendPlayers.Add(new ChallengeInviteWindowPlayer
				{
					PlayerId = (_.Successful ? DropdownPlayerId : Guid.NewGuid().ToString()),
					PlayerName = _opponentUsernameInputter.text,
					Invited = true
				});
				MDNPlayerPrefs.AddRecentChallenge(_accountClient?.AccountInformation?.DisplayName, _opponentUsernameInputter.text);
			}
			_opponentUsernameInputter.text = null;
			InviteButtonPressed?.Invoke(_challengeInviteWindowPlayerData);
			OnDismissButtonPressed();
		});
	}

	private void OnPlayerInviteToggled(ChallengeInviteWindowPlayer player)
	{
		_challengeInviteWindowPlayerData.FriendPlayers.Where((ChallengeInviteWindowPlayer p) => p.PlayerId == player.PlayerId).ToList().ForEach(delegate(ChallengeInviteWindowPlayer p)
		{
			p.Invited = player.Invited;
		});
		UpdateInviteButtonActive();
	}

	private void OnDropdownTextUpdated(string value)
	{
		DisplayNameFormatValidation();
		UpdateInviteButtonActive();
	}

	private void OnOpponentInput_DropDownItemSelected(int val)
	{
		if (val > -1)
		{
			string text = _recentChallengeHistory[val];
			_opponentUsernameInputter.text = text;
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	private void OnInputEndEdit(string text)
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			OnInviteButtonPressed();
		}
	}
}
