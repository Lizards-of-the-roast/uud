using System;
using System.Collections.Generic;
using Core.Code.Input;
using DG.Tweening;
using MTGA.KeyboardManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class SystemMessageView : MonoBehaviour, IKeyDownSubscriber, IKeySubscriber, IBackActionHandler
{
	[Serializable]
	public class AnimationData
	{
		public float Duration = 0.25f;

		public Ease EaseMethod = Ease.InOutSine;

		public float Delay;
	}

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private Transform _messageRoot;

	[SerializeField]
	private TMP_Text _titleText;

	[SerializeField]
	private TMP_Text _messageText;

	[SerializeField]
	private TMP_Text _detailsText;

	[SerializeField]
	protected SystemMessageButtonView _buttonPrefab;

	[SerializeField]
	private AnimationData _fadeAnimationData;

	[SerializeField]
	private AnimationData _slideAnimationData;

	private readonly List<SystemMessageButtonView> _buttons = new List<SystemMessageButtonView>();

	private Action<SystemMessageView> _onUpdate;

	private Action _onHide;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	private SystemMessageButtonView _cancelButton;

	public bool IsOpen { get; set; }

	public PriorityLevelEnum Priority => PriorityLevelEnum.SystemMessage;

	private void Awake()
	{
		_canvasGroup.blocksRaycasts = false;
		_canvasGroup.alpha = 0f;
	}

	public void Show()
	{
		_keyboardManager?.Subscribe(this);
		if (!IsOpen)
		{
			_actionSystem?.PushFocus(this, IActionSystem.Priority.SystemMessage);
		}
		if (AudioManager.Instance != null)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_whoosh_03, base.gameObject);
		}
		IsOpen = true;
		_canvasGroup.blocksRaycasts = true;
		_canvasGroup.DOKill();
		_canvasGroup.DOFade(1f, _fadeAnimationData.Duration).SetEase(_fadeAnimationData.EaseMethod).SetDelay(_fadeAnimationData.Delay);
		_messageRoot.localPosition = new Vector3(0f - ((float)Screen.width + 100f), 0f, 0f);
		_messageRoot.DOLocalMove(Vector3.zero, _slideAnimationData.Duration).SetEase(_slideAnimationData.EaseMethod).SetDelay(_slideAnimationData.Delay);
	}

	public virtual void Hide()
	{
		_keyboardManager?.Unsubscribe(this);
		if (IsOpen)
		{
			_actionSystem?.PopFocus(this);
		}
		_cancelButton = null;
		foreach (SystemMessageButtonView button in _buttons)
		{
			button.SetInteractible(enabled: false);
		}
		_onUpdate = null;
		_canvasGroup.DOFade(0f, _fadeAnimationData.Duration).SetEase(_fadeAnimationData.EaseMethod).OnComplete(delegate
		{
			_canvasGroup.blocksRaycasts = false;
			IsOpen = false;
			_onHide?.Invoke();
		});
	}

	public void SetTitle(string text)
	{
		_titleText.text = text;
	}

	public void SetMessage(string text, int fontsize = -1)
	{
		_messageText.text = text;
		if (fontsize > 0)
		{
			_messageText.fontSize = fontsize;
		}
	}

	public void SetDetails(string text)
	{
		_detailsText.gameObject.UpdateActive(!string.IsNullOrEmpty(text));
		_detailsText.text = text;
	}

	public void CreateButtons(List<SystemMessageManager.SystemMessageButtonData> buttonData)
	{
		foreach (SystemMessageButtonView button in _buttons)
		{
			UnityEngine.Object.Destroy(button.gameObject);
		}
		_buttons.Clear();
		_cancelButton = null;
		foreach (SystemMessageManager.SystemMessageButtonData buttonDatum in buttonData)
		{
			HorizontalLayoutGroup componentInChildren = GetComponentInChildren<HorizontalLayoutGroup>();
			SystemMessageButtonView systemMessageButtonView = UnityEngine.Object.Instantiate(_buttonPrefab, componentInChildren.transform);
			systemMessageButtonView.Init(buttonDatum, HandleOnClick);
			if (buttonDatum.IsCancel || _cancelButton == null)
			{
				_cancelButton = systemMessageButtonView;
			}
			_buttons.Add(systemMessageButtonView);
		}
	}

	private void HandleOnClick(SystemMessageManager.SystemMessageButtonData capturedButton)
	{
		capturedButton.Callback?.Invoke();
		if (capturedButton.IsConfirm)
		{
			if (AudioManager.Instance != null)
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, AudioManager.Default);
			}
		}
		else if (AudioManager.Instance != null)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_cancel, AudioManager.Default);
		}
		if (capturedButton.HideOnClick)
		{
			Hide();
		}
	}

	private void Update()
	{
		_onUpdate?.Invoke(this);
	}

	public void SetOnUpdate(Action<SystemMessageView> updateAction)
	{
		_onUpdate = updateAction;
	}

	internal void Initialize(KeyboardManager keyboardManager, IActionSystem actionSystem, Action dismissCurrentMessage)
	{
		_keyboardManager = keyboardManager;
		_actionSystem = actionSystem;
		_onHide = dismissCurrentMessage;
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape)
		{
			if (_cancelButton != null)
			{
				_cancelButton.Click();
			}
			return true;
		}
		return false;
	}

	public void OnBack(ActionContext context)
	{
		if (_cancelButton != null)
		{
			_cancelButton.Click();
		}
	}
}
