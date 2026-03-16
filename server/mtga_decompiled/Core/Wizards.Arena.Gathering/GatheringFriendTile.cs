using System;
using AK.Wwise;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Challenge;
using MTGA.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Wizards.Arena.Gathering;

public class GatheringFriendTile : MonoBehaviour
{
	private enum AnimatorStates
	{
		Offline,
		Available,
		Busy,
		Away
	}

	public enum Context
	{
		SocialEntityList,
		ChatWindow
	}

	[Header("UI References")]
	[SerializeField]
	private TextMeshProUGUI _username;

	[SerializeField]
	private Localize _status;

	[SerializeField]
	private Button _buttonChallengeFriend;

	[SerializeField]
	private Button _buttonAcceptChallenge;

	[SerializeField]
	private Button _buttonCancelOutgoingChallenge;

	[SerializeField]
	private Button _buttonCancelIncomingChallenge;

	[SerializeField]
	private Animator _animator;

	[Header("Context Menu References")]
	[SerializeField]
	private GameObject _contextDropdownPrefab;

	[Header("Audio References")]
	[SerializeField]
	private AK.Wwise.Event _blockUnfriendAudioEvent;

	private SocialEntity _socialEntity;

	private ISocialManager _socialManager;

	private PVPChallengeController _challengeController;

	private Context _context;

	private bool _contextMenuActive;

	private FriendContextDropdown _friendContextDropdown;

	private static readonly int FriendStatus = Animator.StringToHash("FriendStatus");

	private static readonly int ChallengeOut = Animator.StringToHash("ChallengeOUT");

	private static readonly int ChallengeIn = Animator.StringToHash("ChallengeIN");

	private static readonly int FriendHasChatHistory = Animator.StringToHash("HasChatHistory");

	private static readonly int FriendHasUnreadChats = Animator.StringToHash("HasUnreadChat");

	private static readonly int IsChatTile = Animator.StringToHash("ChatItem");

	private static readonly int ChallengeEnabled = Animator.StringToHash("ChallengeENABLED");

	internal string FriendSocialId => _socialEntity.SocialId;

	private void Awake()
	{
		_socialManager = Pantry.Get<ISocialManager>();
		_challengeController = Pantry.Get<PVPChallengeController>();
	}

	private void OnEnable()
	{
		UpdateStatus();
	}

	public void Init(SocialEntity socialEntity)
	{
		if (socialEntity != null)
		{
			if (_socialEntity != null)
			{
				_socialEntity.OnStatusChanged -= SocialEntityOnStatusChanged;
			}
			_socialEntity = socialEntity;
			socialEntity.OnStatusChanged += SocialEntityOnStatusChanged;
			if (_username != null && !_username.text.Equals(_socialEntity.DisplayName, StringComparison.InvariantCulture))
			{
				_username.SetText(_socialEntity.DisplayName);
			}
			UpdateStatus();
		}
	}

	private void SocialEntityOnStatusChanged(PresenceStatus updatedPresenceStatus)
	{
		MainThreadDispatcher.Dispatch(UpdateStatus);
	}

	private void UpdateStatus()
	{
		if (base.isActiveAndEnabled)
		{
			if (_status == null || _animator == null)
			{
				Debug.LogError("Missing status and/or animator parameters!");
			}
			switch (_socialEntity?.Status ?? PresenceStatus.Offline)
			{
			case PresenceStatus.Offline:
				_status.SetText("Social/Friends/ConnectionState/Offline");
				_animator.SetInteger(FriendStatus, 0);
				break;
			case PresenceStatus.Away:
				_status.SetText("Social/Friends/ConnectionState/Away");
				_animator.SetInteger(FriendStatus, 3);
				break;
			case PresenceStatus.Busy:
				_status.SetText("Social/Friends/ConnectionState/Busy");
				_animator.SetInteger(FriendStatus, 2);
				break;
			case PresenceStatus.Available:
				_status.SetText("Social/Friends/ConnectionState/Online");
				_animator.SetInteger(FriendStatus, 1);
				break;
			}
		}
	}

	public void OnContextRightClick()
	{
		SetContextMenuActive(!_contextMenuActive);
	}

	private void SetContextMenuActive(bool active)
	{
		if (_contextMenuActive != active && !(_contextDropdownPrefab == null))
		{
			_contextMenuActive = active;
			if (active)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(_contextDropdownPrefab, base.gameObject.transform);
				_friendContextDropdown = gameObject.GetComponent<FriendContextDropdown>();
				_friendContextDropdown.Init(_socialEntity, showUnfriendButton: true);
			}
			else if (_friendContextDropdown != null)
			{
				UnityEngine.Object.Destroy(_friendContextDropdown.gameObject);
			}
		}
	}
}
