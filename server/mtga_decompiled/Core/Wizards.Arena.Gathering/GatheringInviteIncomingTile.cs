using AK.Wwise;
using MTGA.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;

namespace Wizards.Arena.Gathering;

public class GatheringInviteIncomingTile : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField]
	private TextMeshProUGUI _username;

	[SerializeField]
	private Button _buttonAccept;

	[SerializeField]
	private Button _buttonReject;

	[Header("Context Menu References")]
	[SerializeField]
	private GameObject _contextDropdownPrefab;

	[Header("Audio References")]
	[SerializeField]
	private AK.Wwise.Event _acceptAudioEvent;

	[SerializeField]
	private AK.Wwise.Event _rejectAudioEvent;

	private bool _contextMenuActive;

	private FriendContextDropdown _friendContextDropdown;

	private ISocialManager _socialManager;

	public Invite Invite { get; private set; }

	private void Awake()
	{
		_socialManager = Pantry.Get<ISocialManager>();
		_buttonAccept.onClick.AddListener(OnAccept);
		_buttonReject.onClick.AddListener(OnReject);
	}

	public void Init(Invite invite)
	{
		Invite = invite;
		_username.text = invite.PotentialFriend.DisplayName;
	}

	private void OnAccept()
	{
		AudioManager.PlayAudio(_acceptAudioEvent.Name, base.gameObject);
		_socialManager.AcceptFriendInviteIncoming(Invite.PotentialFriend);
	}

	private void OnReject()
	{
		AudioManager.PlayAudio(_rejectAudioEvent.Name, base.gameObject);
		_socialManager.DeclineFriendInviteIncoming(Invite);
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
				GameObject gameObject = Object.Instantiate(_contextDropdownPrefab, base.gameObject.transform);
				_friendContextDropdown = gameObject.GetComponent<FriendContextDropdown>();
				_friendContextDropdown.Init(Invite.PotentialFriend);
			}
			else if (_friendContextDropdown != null)
			{
				Object.Destroy(_friendContextDropdown.gameObject);
			}
		}
	}

	private void OnDestroy()
	{
		Invite = null;
		_buttonAccept.onClick.RemoveListener(OnAccept);
		_buttonReject.onClick.RemoveListener(OnReject);
	}
}
