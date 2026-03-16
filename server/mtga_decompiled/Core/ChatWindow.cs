using System;
using System.Collections;
using System.Collections.Generic;
using Core.Code.Input;
using Core.Meta.MainNavigation.Challenge;
using MTGA.Social;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.Social;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class ChatWindow : MonoBehaviour
{
	[Header("Messages")]
	[SerializeField]
	private SocialMessagesView _messagesView;

	[Header("Chat Input")]
	[SerializeField]
	private LifecycleEvents _chatInput;

	[SerializeField]
	private TMP_InputField _chatInputField;

	[SerializeField]
	private Button _sendButton;

	[SerializeField]
	private TMP_InputField _prevChatInputField;

	[Header("Tooltips")]
	[SerializeField]
	private GameObject _chatTooltip;

	[SerializeField]
	private TMP_Text _chatTooltipText;

	[SerializeField]
	private float _showTooltipForSeconds = 2f;

	[Header("Conversations Dropdown")]
	[SerializeField]
	private RectTransform _headerHitArea;

	[SerializeField]
	private FriendTile _currentFriendTile;

	[SerializeField]
	private GameObject _dropdownParent;

	[SerializeField]
	private Transform _dropdownTileParent;

	[SerializeField]
	private Button _arrowDropdownButton;

	[SerializeField]
	private GameObject _arrowDropdownHighlight;

	[SerializeField]
	private TMP_Text _unseenCount;

	[SerializeField]
	private Image _unseenChallenge;

	[SerializeField]
	private Button _unseenBubble;

	[Header("Chat Window Prefabs")]
	[SerializeField]
	private FriendTile _friendTilePrefab;

	private SocialUI _socialUI;

	private ISocialManager _socialManager;

	private ChatManager _chatManager;

	private Animator _chatInputAnimator;

	private LayoutElement _chatInputLayout;

	private Localize _chatInputPlaceholder;

	private bool _dropdownVisible;

	private bool _chatTooltipIsHovered;

	private bool _disabledInput;

	private readonly List<FriendTile> _dropdownTiles = new List<FriendTile>();

	private static readonly int ErrorHash = Animator.StringToHash("Error");

	private IActionSystem _actionSystem;

	private PVPChallengeController _challengeController;

	private void Awake()
	{
		_sendButton.onClick.AddListener(SendChatMessage);
		_arrowDropdownButton.onClick.AddListener(DropdownButtonClicked);
		_unseenBubble.onClick.AddListener(ConversationsUnseenClicked);
		_currentFriendTile.Callback_OpenChat = FriendTileClicked;
		_chatInput.OnEnabled.AddListener(OnChatInputEnabled);
		_chatInputField.onValueChanged.AddListener(delegate
		{
			StartCoroutine(Coroutine_CheckLineCount());
		});
		_chatInputAnimator = _chatInput.GetComponent<Animator>();
		_chatInputLayout = _chatInput.GetComponent<LayoutElement>();
	}

	private void OnChatInputEnabled()
	{
		StartCoroutine(Coroutine_ActivateInputField());
		_chatInputAnimator.SetBool(ErrorHash, _disabledInput);
	}

	private void LateUpdate()
	{
		if (PlatformUtils.IsHandheld() && TouchScreenKeyboard.visible && _chatInputField.text.Contains(Environment.NewLine))
		{
			_chatInputField.text = _chatInputField.text.Replace(Environment.NewLine, "");
			if (EventSystem.current.currentSelectedGameObject == _chatInputField.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			TrySendMessage();
		}
	}

	public void Init(SocialUI socialUI, ISocialManager socialManager, PVPChallengeController challengeController, IActionSystem actionSystem)
	{
		if (_socialManager != null)
		{
			OnDestroy();
		}
		_challengeController = challengeController;
		_actionSystem = actionSystem;
		_socialManager = socialManager;
		_socialUI = socialUI;
		_chatManager = _socialManager.ChatManager;
		_socialManager.FriendPresenceChanged += FriendPresenceChanged;
		_socialManager.ChatEnabledChanged += ChatEnabledChanged;
		_chatManager.MessageAdded += MessageAdded;
		_chatManager.NotificationCancel += NotificationCancel;
		_socialUI.SubscribeToFriendChallenge(_currentFriendTile);
		_messagesView.Init(socialUI, socialManager, _challengeController);
	}

	public void Hide()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		_socialManager.FriendPresenceChanged -= FriendPresenceChanged;
		_socialManager.ChatEnabledChanged -= ChatEnabledChanged;
		_chatManager.MessageAdded -= MessageAdded;
		_chatManager.NotificationCancel -= NotificationCancel;
	}

	public void CloseWindow()
	{
		_socialUI.CloseChat();
	}

	private IEnumerator Coroutine_ActivateInputField()
	{
		yield return null;
		_chatInputField.ActivateInputField();
	}

	private IEnumerator Coroutine_CheckLineCount()
	{
		yield return null;
		if ((_chatInputField.textComponent.textInfo?.lineCount ?? 0) < 5)
		{
			_chatInputLayout.preferredHeight = -1f;
		}
		else
		{
			_chatInputLayout.preferredHeight = 170f;
		}
	}

	public void ConversationSelected(Conversation prevConversation, Conversation currentConversation)
	{
		string text = _chatInputField.text;
		if (!string.IsNullOrEmpty(text))
		{
			int num = Mathf.Max(0, _chatInputField.caretPosition);
			if (num < text.Length && text[num] == '\t')
			{
				text = text.Remove(num);
			}
		}
		prevConversation?.SaveMessageDraft(text);
		_chatInputField.text = currentConversation?.MessageDraft;
		UpdateDropdownVisible(show: false);
		CheckDisabled();
		UpdateCurrentFriend();
		UpdateFriendTileList();
		_chatInputField.ActivateInputField();
	}

	private void UpdateCurrentFriend()
	{
		_currentFriendTile.Init(_chatManager.CurrentConversation?.Friend, FriendTile.Context.ChatWindow, _actionSystem, _challengeController.ChallengePermissionState);
		_currentFriendTile.SetChallengeEnabled(value: true);
	}

	private void Update()
	{
		if (!_chatInput.IsEnabled || (!CustomInputModule.PointerIsHeldDown() && !CustomInputModule.IsRightClick()))
		{
			return;
		}
		bool mouseOver = _headerHitArea.GetMouseOver();
		if (!mouseOver && !((RectTransform)base.transform).GetMouseOver())
		{
			if (!PlatformUtils.IsHandheld())
			{
				_socialUI.Minimize();
			}
		}
		else if (_dropdownVisible && !mouseOver && !((RectTransform)_dropdownTileParent).GetMouseOver())
		{
			UpdateDropdownVisible(show: false);
		}
	}

	private void UpdateDropdownVisible(bool show)
	{
		_dropdownVisible = show;
		_dropdownParent.UpdateActive(_dropdownVisible);
		if (!_dropdownVisible)
		{
			return;
		}
		List<Conversation> sortedConversations = _chatManager.GetSortedConversations();
		int i = 0;
		foreach (Conversation item in sortedConversations)
		{
			if (item != _chatManager.CurrentConversation)
			{
				if (i >= _dropdownTiles.Count)
				{
					FriendTile friendTile = UnityEngine.Object.Instantiate(_friendTilePrefab, _dropdownTileParent);
					friendTile.SetChallengeEnabled(value: false);
					friendTile.Callback_OpenChat = FriendTileClicked;
					_socialUI.SubscribeToFriendChallenge(friendTile);
					_dropdownTiles.Add(friendTile);
				}
				_dropdownTiles[i].gameObject.UpdateActive(active: true);
				_dropdownTiles[i].Init(item.Friend, FriendTile.Context.ChatWindow, _actionSystem);
				i++;
			}
		}
		for (; i < _dropdownTiles.Count; i++)
		{
			_dropdownTiles[i].gameObject.UpdateActive(active: false);
		}
	}

	private void UpdateFriendTileList()
	{
		_arrowDropdownButton.gameObject.UpdateActive(_chatManager.Conversations.Count > 1 && !PlatformUtils.IsHandheld());
		int num = _chatManager.ConversationsUnseenCount();
		if (num > 0)
		{
			_arrowDropdownHighlight.UpdateActive(active: true);
			_unseenChallenge.gameObject.UpdateActive(active: false);
			_unseenCount.gameObject.UpdateActive(active: true);
			_unseenCount.text = new MTGALocalizedString
			{
				Key = "MainNav/General/Simple_Number",
				Parameters = new Dictionary<string, string> { 
				{
					"number",
					num.ToString()
				} }
			};
		}
		else if (_chatManager.IsChallengedInConversation())
		{
			_arrowDropdownHighlight.UpdateActive(active: true);
			_unseenChallenge.gameObject.UpdateActive(active: true);
			_unseenCount.gameObject.UpdateActive(active: false);
		}
		else
		{
			_arrowDropdownHighlight.UpdateActive(active: false);
			_unseenChallenge.gameObject.UpdateActive(active: false);
			_unseenCount.gameObject.UpdateActive(active: false);
		}
	}

	private bool CheckDisabled(bool showTooltip = false)
	{
		if (_chatInputPlaceholder == null)
		{
			_chatInputPlaceholder = _chatInputField.placeholder.GetComponent<Localize>();
		}
		if (!_socialManager.IsSocialEnabled)
		{
			_disabledInput = true;
			_chatTooltipText.SetText((MTGALocalizedString)"Social/Errors/ChatDisabled_Desc");
			_chatInputPlaceholder.SetText((MTGALocalizedString)"Social/Errors/ChatDisabled_Desc");
		}
		else if (_chatManager.CurrentConversation == null)
		{
			_disabledInput = true;
			_chatTooltipText.SetText((MTGALocalizedString)"Social/Friends/ConnectionState/Offline");
		}
		else if (!_chatManager.CurrentConversation.Friend.IsOnline)
		{
			_disabledInput = true;
			MTGALocalizedString mTGALocalizedString = new MTGALocalizedString
			{
				Key = "Social/Chat/FriendOfflineHint",
				Parameters = new Dictionary<string, string> { 
				{
					"name",
					_chatManager.CurrentConversation.Friend.DisplayName
				} }
			};
			_chatTooltipText.SetText(mTGALocalizedString);
			_chatInputPlaceholder.SetText(mTGALocalizedString);
		}
		else
		{
			_disabledInput = false;
			_chatInputPlaceholder.SetText((MTGALocalizedString)"Social/Chat/NewMessageHint");
		}
		if (_chatInputAnimator.isActiveAndEnabled)
		{
			_chatInputAnimator.SetBool(ErrorHash, _disabledInput);
		}
		if (_disabledInput && showTooltip)
		{
			StartCoroutine(Coroutine_ShowChatTooltipForSeconds(_showTooltipForSeconds));
		}
		return _disabledInput;
	}

	private void FriendPresenceChanged(SocialEntity friend)
	{
		if (friend.Equals(_chatManager.CurrentConversation?.Friend))
		{
			UpdateCurrentFriend();
			CheckDisabled();
		}
	}

	private void ChatEnabledChanged(bool chatEnabled)
	{
		CheckDisabled();
	}

	private void MessageAdded(SocialMessage message, Conversation conversation)
	{
		UpdateFriendTileList();
	}

	private void NotificationCancel(SocialMessage message)
	{
		UpdateFriendTileList();
	}

	public void TrySendMessage()
	{
		if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift) && _chatInput.IsEnabled)
		{
			int num = Mathf.Max(0, _chatInputField.caretPosition);
			string text = _chatInputField.text;
			if (!string.IsNullOrEmpty(text) && num < text.Length && text[num] == '\n')
			{
				_chatInputField.text = text.Remove(num);
			}
			if (num != text.Length)
			{
				_chatInputField.text = text.Replace("\n", "");
			}
			SendChatMessage();
		}
	}

	public void SendChatMessage()
	{
		string text = _chatInputField.text.RemoveSurrogatePairs();
		if (!string.IsNullOrWhiteSpace(text) && !CheckDisabled(showTooltip: true))
		{
			_chatInputField.text = string.Empty;
			if (_chatManager.SendChatMessage(text) != null)
			{
				AudioManager.PlayAudio("sfx_ui_instant_message_send", AudioManager.Default);
			}
		}
	}

	public void Unity_TooltipPointerEnter()
	{
		_chatTooltipIsHovered = true;
		CheckDisabled(showTooltip: true);
	}

	public void Unity_TooltipPointerExit()
	{
		_chatTooltipIsHovered = false;
		_chatTooltip.UpdateActive(active: false);
	}

	private void DropdownButtonClicked()
	{
		UpdateDropdownVisible(!_dropdownVisible && _chatManager.Conversations.Count > 1);
	}

	private void FriendTileClicked(SocialEntity friend)
	{
		UpdateDropdownVisible(!_dropdownVisible && _chatManager.Conversations.Count > 1);
		_chatManager.SelectConversation(friend);
	}

	private void ConversationsUnseenClicked()
	{
		_chatManager.SelectNextConversation(reverse: false, unseenOnly: true);
	}

	private IEnumerator Coroutine_ShowChatTooltipForSeconds(float seconds)
	{
		_chatTooltip.UpdateActive(active: true);
		yield return new WaitForSeconds(seconds);
		if (!_chatTooltipIsHovered)
		{
			_chatTooltip.UpdateActive(active: false);
		}
		CheckDisabled();
	}
}
