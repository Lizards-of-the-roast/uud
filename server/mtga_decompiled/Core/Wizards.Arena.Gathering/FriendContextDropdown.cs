using System.Collections.Generic;
using AK.Wwise;
using MTGA.Social;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;

namespace Wizards.Arena.Gathering;

public class FriendContextDropdown : MonoBehaviour
{
	[SerializeField]
	private Button unfriendButton;

	[SerializeField]
	private Button blockButton;

	[SerializeField]
	private AK.Wwise.Event blockUnfriendAudioEvent;

	private ISocialManager _socialManager;

	private SocialEntity _socialEntity;

	public static FriendContextDropdown Instance { get; private set; }

	private void Awake()
	{
		_socialManager = Pantry.Get<ISocialManager>();
		unfriendButton.onClick.AddListener(OnUnfriendClicked);
		blockButton.onClick.AddListener(OnBlockClicked);
		if (Instance != null && Instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			Instance = this;
		}
	}

	public void Init(SocialEntity socialEntity, bool showUnfriendButton = false)
	{
		_socialEntity = socialEntity;
		unfriendButton.gameObject.SetActive(showUnfriendButton);
	}

	private void OnBlockClicked()
	{
		AudioManager.PlayAudio(blockUnfriendAudioEvent.Name, base.gameObject);
		MTGALocalizedString obj = new MTGALocalizedString
		{
			Key = "DuelScene/ClientPrompt/Are_You_Sure_Title"
		};
		SystemMessageManager.ShowSystemMessage(message: new MTGALocalizedString
		{
			Key = "Social/Friends/Prompt/ConfirmBlock/Body",
			Parameters = new Dictionary<string, string> { { "displayName", _socialEntity.DisplayName } }
		}, title: obj, showCancel: true, onOK: delegate
		{
			_socialManager.AddBlock(_socialEntity);
		});
		Object.Destroy(base.gameObject);
	}

	private void OnUnfriendClicked()
	{
		AudioManager.PlayAudio(blockUnfriendAudioEvent.Name, base.gameObject);
		string title = new MTGALocalizedString
		{
			Key = "DuelScene/ClientPrompt/Are_You_Sure_Title"
		};
		string message = new MTGALocalizedString
		{
			Key = "Social/Friends/Prompt/RemoveFriend/Body",
			Parameters = new Dictionary<string, string> { { "displayName", _socialEntity.DisplayName } }
		};
		SystemMessageManager.ShowSystemMessage(title, message, showCancel: true, delegate
		{
			_socialManager.RemoveFriend(_socialEntity);
		});
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		unfriendButton.onClick.RemoveAllListeners();
		blockButton.onClick.RemoveAllListeners();
	}
}
