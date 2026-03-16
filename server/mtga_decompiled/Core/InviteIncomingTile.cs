using System;
using Core.Code.Input;
using MTGA.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;

public class InviteIncomingTile : MonoBehaviour, IBackActionHandler
{
	[SerializeField]
	private TMP_Text _labelName;

	[SerializeField]
	private Image _imageAvatar;

	[SerializeField]
	private CustomButton _contextClickButton;

	[SerializeField]
	private Button _buttonAccept;

	[SerializeField]
	private RectTransform _contextMenu;

	[SerializeField]
	private float _contextMenuBottomMargin;

	[SerializeField]
	private Button _buttonReject;

	[SerializeField]
	private Button _buttonBlock;

	private Transform _dropdownRoot;

	private bool _contextMenuActive;

	public Action<Invite> Callback_Accept;

	public Action<Invite> Callback_Reject;

	public Action<Invite> Callback_Block;

	private IActionSystem _actionSystem;

	public Invite Invite { get; private set; }

	private void Awake()
	{
		_contextClickButton.OnClick.AddListener(OnContextClick);
		_contextClickButton.OnRightClick.AddListener(OnContextClick);
		_buttonAccept.onClick.AddListener(delegate
		{
			SetContextMenuActive(isActive: false);
			AudioManager.PlayAudio("sfx_ui_friends_click", base.gameObject);
			Callback_Accept?.Invoke(Invite);
		});
		_buttonReject.onClick.AddListener(delegate
		{
			SetContextMenuActive(isActive: false);
			AudioManager.PlayAudio("sfx_ui_friends_negate", base.gameObject);
			Callback_Reject?.Invoke(Invite);
		});
		_buttonBlock.onClick.AddListener(delegate
		{
			SetContextMenuActive(isActive: false);
			AudioManager.PlayAudio("sfx_ui_friends_negate", base.gameObject);
			Callback_Block?.Invoke(Invite);
		});
	}

	public void Init(Invite invite, Transform dropdownSortRoot, IActionSystem actionSystem)
	{
		base.gameObject.UpdateActive(active: true);
		Invite = invite;
		_labelName.text = invite.PotentialFriend.DisplayName;
		_dropdownRoot = dropdownSortRoot;
		_actionSystem = actionSystem;
	}

	private void SetAvatarImage(Sprite newAvatarImage)
	{
		_imageAvatar.sprite = newAvatarImage;
	}

	private void OnDisable()
	{
		SetContextMenuActive(isActive: false);
	}

	public void Cleanup()
	{
		SetContextMenuActive(isActive: false);
		Invite = null;
		Callback_Accept = null;
		Callback_Reject = null;
		Callback_Block = null;
	}

	public void OnContextClick()
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
		if (_contextMenuActive == isActive)
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
