using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga;

namespace Core.Meta.MainNavigation.PopoutMenu.Challenges;

public class ChallengeInviteWindowEntityPlayer : MonoBehaviour
{
	[SerializeField]
	private Toggle _inviteToggle;

	[SerializeField]
	private TMP_Text _inviteUserName;

	[SerializeField]
	private TMP_Text _inviteUserStatus;

	[SerializeField]
	private Image _inviteUserAvatar;

	[SerializeField]
	private Color _displayNameMainColor;

	[SerializeField]
	private Color _displayNameSubColor;

	public Action<ChallengeInviteWindowPlayer> InviteToggleChanged;

	public ChallengeInviteWindowPlayer Player { get; private set; }

	private void OnEnable()
	{
		_inviteToggle.onValueChanged.AddListener(OnInviteToggleChanged);
	}

	private void OnDisable()
	{
		_inviteToggle.onValueChanged.RemoveAllListeners();
	}

	public void Init(ChallengeInviteWindowPlayer player)
	{
		Player = player;
		if (player != null)
		{
			_inviteUserName.text = SharedUtilities.FormatDisplayName(player.PlayerName, _displayNameMainColor, _displayNameSubColor, 0u);
			_inviteUserStatus.text = Player.Status;
			_inviteToggle.isOn = Player.Invited;
		}
	}

	public bool GetShouldInvite()
	{
		return _inviteToggle.isOn;
	}

	public void SetInviteToggle(bool invite)
	{
		_inviteToggle.isOn = invite;
	}

	private void OnInviteToggleChanged(bool value)
	{
		Player.Invited = value;
		InviteToggleChanged?.Invoke(Player);
	}
}
