using System.Collections.Generic;
using HasbroGo.Social.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HasbroGo;

public class SocialBust : MonoBehaviour
{
	[SerializeField]
	private GameObject NameAndOnlineFriendCountContainer;

	[SerializeField]
	private TextMeshProUGUI onlineFriendsCount;

	[SerializeField]
	private TextMeshProUGUI displayName;

	[SerializeField]
	private Sprite onlinePresencIconSprite;

	[SerializeField]
	private Sprite offlinePresencIconSprite;

	[SerializeField]
	private Sprite busyPresencIconSprite;

	[SerializeField]
	private Image bustIcon;

	private readonly Dictionary<PlatformStatus, Sprite> socialPresenceSpriteMap = new Dictionary<PlatformStatus, Sprite>();

	public void Awake()
	{
		socialPresenceSpriteMap.Add(PlatformStatus.Playing, onlinePresencIconSprite);
		socialPresenceSpriteMap.Add(PlatformStatus.Offline, offlinePresencIconSprite);
		socialPresenceSpriteMap.Add(PlatformStatus.Busy, busyPresencIconSprite);
		UpdateDisplayName(string.Empty);
		if (!HasbroGoSDKManager.Instance.HasbroGoSdk.AccountsService.IsLoggedIn())
		{
			UpdateDisplayName("Login to see friends");
		}
	}

	public void OnEnable()
	{
		ResetView();
	}

	public void ResetView()
	{
		ShowNameAndOnlineFriendCountContainer(show: true);
		ShowOnlineFriendCount(show: true);
	}

	public void ShowNameAndOnlineFriendCountContainer(bool show)
	{
		NameAndOnlineFriendCountContainer.SetActive(show);
	}

	public void ShowOnlineFriendCount(bool show)
	{
		onlineFriendsCount.gameObject.SetActive(show);
	}

	public void UpdatePresenceIcon(PlatformStatus status)
	{
		if (socialPresenceSpriteMap.TryGetValue(status, out var value))
		{
			bustIcon.sprite = value;
		}
		else
		{
			bustIcon.sprite = onlinePresencIconSprite;
		}
	}

	public void UpdateDisplayName(string _displayName)
	{
		displayName.text = _displayName;
	}

	public void UpdateOnlineFriendsCount(int numOnlineFriends)
	{
		onlineFriendsCount.text = numOnlineFriends.ToString();
	}
}
