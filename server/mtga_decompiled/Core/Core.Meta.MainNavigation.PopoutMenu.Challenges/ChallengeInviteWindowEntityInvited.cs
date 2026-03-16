using TMPro;
using UnityEngine;
using Wotc.Mtga;

namespace Core.Meta.MainNavigation.PopoutMenu.Challenges;

public class ChallengeInviteWindowEntityInvited : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _invitedUserName;

	[SerializeField]
	private Color _displayNameMainColor;

	[SerializeField]
	private Color _displayNameSubColor;

	public ChallengeInviteWindowPlayer Player { get; private set; }

	public void Init(ChallengeInviteWindowPlayer player)
	{
		Player = player;
		_invitedUserName.text = SharedUtilities.FormatDisplayName(Player.PlayerName, _displayNameMainColor, _displayNameSubColor, 0u);
	}
}
