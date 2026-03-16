using System.Linq;
using AK.Wwise;
using MTGA.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;

namespace Wizards.Arena.Gathering;

public class GatheringCornerIconControl : MonoBehaviour
{
	private enum AnimatorCornerIconState
	{
		Disabled,
		Offline,
		Online,
		Busy,
		HotChallengeOn,
		HotOn
	}

	[SerializeField]
	private Button _socialIconButton;

	[SerializeField]
	private GameObject _friendsListPrefab;

	[SerializeField]
	private Transform _friendsListParent;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private TMP_Text _onlineText;

	[Header("Audio References")]
	[SerializeField]
	private AK.Wwise.Event _clickOpenAudioEvent;

	[SerializeField]
	private AK.Wwise.Event _clickCloseAudioEvent;

	private GameObject _activeFriendsList;

	private ISocialManager _socialManager;

	private static readonly int IconStateHash = Animator.StringToHash("IconState");

	private static readonly int ToggleNumber = Animator.StringToHash("ShowNu");

	private void Awake()
	{
		_socialManager = Pantry.Get<ISocialManager>();
	}

	private void OnEnable()
	{
		_socialIconButton.onClick.AddListener(SocialIconButtonClick);
		_socialManager.LocalPresenceStatusChanged += OnLocalPresenceStatusChanged;
		UpdateIconState(_socialManager.LocalPlayer.Status);
		UpdateOnlineCount(showNum: true);
	}

	private void OnDisable()
	{
		_socialIconButton.onClick.RemoveListener(SocialIconButtonClick);
		_socialManager.LocalPresenceStatusChanged -= OnLocalPresenceStatusChanged;
	}

	private void OnApplicationQuit()
	{
		_socialManager?.Destroy();
	}

	private void SocialIconButtonClick()
	{
		if (_activeFriendsList == null)
		{
			_activeFriendsList = Object.Instantiate(_friendsListPrefab, _friendsListParent);
			AudioManager.PlayAudio(_clickOpenAudioEvent.Name, base.gameObject);
			UpdateOnlineCount(showNum: false);
		}
		else
		{
			Object.Destroy(_activeFriendsList);
			AudioManager.PlayAudio(_clickCloseAudioEvent.Name, base.gameObject);
			UpdateOnlineCount(showNum: true);
		}
	}

	private void OnLocalPresenceStatusChanged(PresenceStatus oldStatus, PresenceStatus newStatus)
	{
		UpdateIconState(newStatus);
	}

	private void UpdateIconState(PresenceStatus status)
	{
		if (!(_animator == null))
		{
			switch (status)
			{
			case PresenceStatus.Offline:
			case PresenceStatus.Away:
				_animator.SetInteger(IconStateHash, 1);
				break;
			case PresenceStatus.Busy:
				_animator.SetInteger(IconStateHash, 3);
				break;
			case PresenceStatus.Available:
				_animator.SetInteger(IconStateHash, 2);
				break;
			default:
				_animator.SetInteger(IconStateHash, 0);
				break;
			}
		}
	}

	private void UpdateOnlineCount(bool showNum)
	{
		_animator.SetBool(ToggleNumber, showNum);
		if (!_socialManager.Connected || _socialManager.LocalPlayer.Status == PresenceStatus.Offline)
		{
			_onlineText.text = string.Empty;
			return;
		}
		_onlineText.text = _socialManager.Friends.Count((SocialEntity f) => f.IsOnline).ToString();
	}
}
