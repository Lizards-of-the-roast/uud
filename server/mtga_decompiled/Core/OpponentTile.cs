using System;
using Core.Code.Input;
using MTGA.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class OpponentTile : MonoBehaviour, IBackActionHandler
{
	private enum AnimatorStates
	{
		Offline,
		Available,
		Busy,
		Away
	}

	public enum Context
	{
		SocialEntityList,
		ChatWindow
	}

	[Header("Label References")]
	[SerializeField]
	private TMP_Text _labelName;

	[SerializeField]
	private Localize _labelStatus;

	[Header("Button References")]
	[SerializeField]
	private CustomButton _contextClickButton;

	[SerializeField]
	private RectTransform _contextMenu;

	[SerializeField]
	private float _contextMenuBottomMargin;

	[SerializeField]
	private RectTransform _buttonViewportTransform;

	[SerializeField]
	private Button _buttonAddFriend;

	[SerializeField]
	private Button _buttonBlock;

	[SerializeField]
	private Button _buttonUnblock;

	private Animator _animator;

	public Action<SocialEntity> Callback_OpenChat;

	public Action<SocialEntity> Callback_AddFriend;

	public Action<SocialEntity> Callback_BlockOpponent;

	public Action<SocialEntity> Callback_UnblockOpponent;

	private Context _context;

	private bool _contextMenuActive;

	private RectTransform _dropdownRoot;

	private static readonly int Status = Animator.StringToHash("FriendStatus");

	private static readonly int HasChatHistory = Animator.StringToHash("HasChatHistory");

	private static readonly int HasUnreadChats = Animator.StringToHash("HasUnreadChat");

	private static readonly int IsChatTile = Animator.StringToHash("ChatItem");

	private static readonly int IsBlocked = Animator.StringToHash("IsBlocked");

	private IActionSystem _actionSystem;

	public SocialEntity Opponent { get; private set; }

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_contextClickButton.OnClick.AddListener(OnButton_OpenChat);
		_contextClickButton.OnRightClick.AddListener(OnContextClick);
		_buttonAddFriend.onClick.AddListener(OnButton_AddFriend);
		_buttonBlock.onClick.AddListener(OnButton_Block);
		_buttonUnblock.onClick.AddListener(OnButton_Unblock);
	}

	private void OnEnable()
	{
		UpdateStatus();
	}

	public void Init(SocialEntity opponent, Context context, IActionSystem actionSystem, RectTransform dropdownRoot = null)
	{
		Opponent = opponent;
		_context = context;
		_dropdownRoot = dropdownRoot;
		_actionSystem = actionSystem;
		base.gameObject.UpdateActive(active: true);
		_labelName.text = Opponent?.DisplayName ?? "";
		UpdateStatus();
	}

	private void OnButton_AddFriend()
	{
		AudioManager.PlayAudio("sfx_ui_friends_negate", base.gameObject);
		Callback_AddFriend?.Invoke(Opponent);
	}

	private void OnButton_Block()
	{
		AudioManager.PlayAudio("sfx_ui_friends_negate", base.gameObject);
		Callback_BlockOpponent?.Invoke(Opponent);
	}

	private void OnButton_Unblock()
	{
		AudioManager.PlayAudio("sfx_ui_friends_negate", base.gameObject);
		SetContextMenuActive(isActive: false);
		Callback_UnblockOpponent?.Invoke(Opponent);
	}

	private void OnButton_OpenChat()
	{
		if (Opponent.IsChattable || Opponent.HasChatHistory)
		{
			AudioManager.PlayAudio("sfx_ui_friends_click", base.gameObject);
			Callback_OpenChat?.Invoke(Opponent);
		}
	}

	private void UpdateStatus()
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (Opponent.IsBlocked)
		{
			_labelStatus.SetText("NEEDS LOC: Blocked User");
			_animator.SetInteger(Status, 0);
			_animator.SetBool(IsBlocked, value: true);
			_buttonAddFriend.gameObject.SetActive(value: false);
			_buttonBlock.gameObject.SetActive(value: false);
			_buttonUnblock.gameObject.SetActive(value: true);
		}
		else
		{
			_animator.SetBool(IsBlocked, value: false);
			_buttonAddFriend.gameObject.SetActive(value: true);
			_buttonBlock.gameObject.SetActive(value: true);
			_buttonUnblock.gameObject.SetActive(value: false);
			if (Opponent.IsCurrentOpponent)
			{
				_labelStatus.SetText("NEEDS LOC: Current Opponent");
				if (Opponent.IsChattable)
				{
					_animator.SetInteger(Status, 1);
				}
				else
				{
					_animator.SetInteger(Status, 0);
				}
			}
			else
			{
				_labelStatus.SetText("NEEDS LOC: Previous Opponent");
				_animator.SetInteger(Status, 0);
			}
		}
		_animator.SetBool(HasUnreadChats, Opponent?.HasUnseenMessages ?? false);
		_animator.SetBool(HasChatHistory, Opponent?.HasChatHistory ?? false);
		_animator.SetBool(IsChatTile, _context == Context.ChatWindow);
	}

	private void OnDisable()
	{
		SetContextMenuActive(isActive: false);
	}

	public void Cleanup()
	{
		SetContextMenuActive(isActive: false);
		Opponent = null;
		Callback_OpenChat = null;
		Callback_AddFriend = null;
		Callback_BlockOpponent = null;
	}

	private void OnContextClick()
	{
		SetContextMenuActive(!_contextMenuActive);
	}

	private void Update()
	{
		if (!_contextMenuActive)
		{
			return;
		}
		if (CustomInputModule.PointerIsHeldDown() || CustomInputModule.IsRightClick())
		{
			if (!((RectTransform)base.transform).GetMouseOver() && !((RectTransform)_contextMenu.transform).GetMouseOver())
			{
				SetContextMenuActive(isActive: false);
			}
		}
		else if (!ActionSystemFactory.CaptureEscapeFeatureToggle && Input.GetKeyDown(KeyCode.Escape))
		{
			SetContextMenuActive(isActive: false);
		}
		else if (CustomInputModule.GetMouseScroll() != Vector2.zero)
		{
			SetContextMenuActive(isActive: false);
		}
	}

	private void SetContextMenuActive(bool isActive)
	{
		if (_contextMenuActive == isActive || _contextMenu == null)
		{
			return;
		}
		AudioManager.PlayAudio("sfx_ui_friends_click", base.gameObject);
		_contextMenuActive = isActive;
		_contextMenu.gameObject.UpdateActive(_contextMenuActive);
		if (_contextMenuActive)
		{
			_actionSystem?.PushFocus(this);
			Transform parent = (isActive ? _dropdownRoot : base.transform);
			_contextMenu.SetParent(parent, worldPositionStays: true);
			Vector2 anchoredPosition = _contextMenu.anchoredPosition;
			if (anchoredPosition.y - _contextMenu.rect.height < _contextMenuBottomMargin)
			{
				anchoredPosition.y -= anchoredPosition.y - _contextMenu.rect.height - _contextMenuBottomMargin;
				_contextMenu.anchoredPosition = anchoredPosition;
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(_buttonViewportTransform);
		}
		else
		{
			_contextMenu.SetParent(base.transform);
			_contextMenu.anchoredPosition = new Vector2(0f, 0f);
			_actionSystem?.PopFocus(this);
		}
	}

	public void OnBack(ActionContext context)
	{
		if (ActionSystemFactory.CaptureEscapeFeatureToggle)
		{
			SetContextMenuActive(isActive: false);
			context.Used = false;
		}
	}
}
