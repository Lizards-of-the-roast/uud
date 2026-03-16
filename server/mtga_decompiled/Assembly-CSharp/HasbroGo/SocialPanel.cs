using HasbroGo.Social.Models;
using TMPro;
using UnityEngine;

namespace HasbroGo;

public class SocialPanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI displayName;

	[SerializeField]
	private TextMeshProUGUI socialPrecenceStatus;

	[SerializeField]
	private GameObject sendFriendRequestButtonGO;

	public void UpdateDisplayName(string _displayName)
	{
		displayName.text = _displayName;
	}

	public void UpdatePresenceStatus(PlatformStatus status)
	{
		socialPrecenceStatus.text = status switch
		{
			PlatformStatus.Offline => "Offline", 
			PlatformStatus.Busy => "Busy", 
			_ => "Online", 
		};
		ShowSendFriendRequestButton(status != PlatformStatus.Offline);
	}

	private void ShowSendFriendRequestButton(bool show)
	{
		sendFriendRequestButtonGO.SetActive(show);
	}
}
