using UnityEngine.UI;

namespace Core.Meta.MainNavigation.PopoutMenu.Challenges;

public class ChallengeInviteWindowPlayer
{
	public string PlayerName { get; set; }

	public string PlayerId { get; set; }

	public string Status { get; set; }

	public Image AvatarImage { get; set; }

	public bool Invited { get; set; }
}
