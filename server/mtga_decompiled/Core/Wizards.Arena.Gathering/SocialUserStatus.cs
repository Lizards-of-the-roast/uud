using MTGA.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Wizards.Arena.Gathering;

public class SocialUserStatus : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _usernameDisplay;

	[SerializeField]
	private Localize _localizedUserStatus;

	[SerializeField]
	private Button _userStatusButton;

	[SerializeField]
	private GameObject _statusDropdownPrefab;

	[SerializeField]
	private Transform _statusDropdownParent;

	private GameObject _activeUserStatusDropdown;

	private ISocialManager _socialManager;

	private void Awake()
	{
		_socialManager = Pantry.Get<ISocialManager>();
	}

	private void Start()
	{
		_userStatusButton.onClick.AddListener(StatusDropdownClicked);
		_socialManager.LocalPlayer.OnStatusChanged += UpdateUserStatus;
	}

	private void OnDestroy()
	{
		_userStatusButton.onClick.RemoveAllListeners();
		_socialManager.LocalPlayer.OnStatusChanged -= UpdateUserStatus;
	}

	private void OnEnable()
	{
		UpdateUserDisplayName(_socialManager.LocalPlayer.DisplayName);
		UpdateUserStatus(_socialManager.LocalPlayer.Status);
	}

	private void StatusDropdownClicked()
	{
		if (_activeUserStatusDropdown == null)
		{
			_activeUserStatusDropdown = Object.Instantiate(_statusDropdownPrefab, _statusDropdownParent);
		}
		else
		{
			Object.Destroy(_activeUserStatusDropdown);
		}
	}

	private void UpdateUserDisplayName(string userDisplayName)
	{
		if (_usernameDisplay == null)
		{
			Debug.LogError("There is no user display name assigned.", this);
		}
		else
		{
			_usernameDisplay.SetText(userDisplayName);
		}
	}

	private void UpdateUserStatus(PresenceStatus updatedStatus)
	{
		if (_localizedUserStatus == null)
		{
			Debug.LogError("There is no localization object for the user status assigned.", this);
		}
		else
		{
			_localizedUserStatus.SetText(SocialEntity.PresenceLocalizationKey(updatedStatus));
		}
	}
}
