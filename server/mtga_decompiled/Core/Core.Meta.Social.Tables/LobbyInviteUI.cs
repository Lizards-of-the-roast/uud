using System.Collections.Generic;
using System.Linq;
using MTGA.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;

namespace Core.Meta.Social.Tables;

public class LobbyInviteUI : MonoBehaviour
{
	[SerializeField]
	private TMP_Dropdown FriendsDropdown;

	[SerializeField]
	private Button InviteButton;

	private string _currentLobbyId;

	private ILobbyController _lobbyController;

	public void UpdateLobbyInfo(string lobbyId)
	{
		_currentLobbyId = lobbyId;
		UpdateDropdownOptions();
	}

	private void Start()
	{
		InviteButton.onClick.AddListener(OnInviteClicked);
		EnsureControllerRefs();
	}

	private void EnsureControllerRefs()
	{
		if (_lobbyController == null)
		{
			_lobbyController = Pantry.Get<ILobbyController>();
		}
	}

	private void OnEnable()
	{
		UpdateDropdownOptions();
	}

	private void OnFriendPresenceChange(SocialEntity friend)
	{
		UpdateDropdownOptions();
	}

	private void UpdateDropdownOptions()
	{
		FriendsDropdown.options.Clear();
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		if (!string.IsNullOrEmpty(_currentLobbyId))
		{
			EnsureControllerRefs();
			list = (from playerName in _lobbyController.GetInvitableFriends(_currentLobbyId)
				select new TMP_Dropdown.OptionData
				{
					text = playerName
				}).ToList();
		}
		if (list.Count > 0)
		{
			FriendsDropdown.interactable = true;
			InviteButton.interactable = true;
		}
		else
		{
			list.Add(new TMP_Dropdown.OptionData
			{
				text = "No invitable friends"
			});
			FriendsDropdown.interactable = false;
			InviteButton.interactable = false;
		}
		FriendsDropdown.options = list;
	}

	private void OnInviteClicked()
	{
		Pantry.Get<ILobbyController>().SendLobbyInvite(_currentLobbyId, FriendsDropdown.captionText.text);
	}

	private void OnDestroy()
	{
		InviteButton.onClick.RemoveListener(OnInviteClicked);
	}
}
