using Core.Code.Input;
using Core.Code.Promises;
using MTGA.KeyboardManager;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga;
using Wotc.Mtga.CustomInput;

namespace Wotc.Mtga.Login;

public class Panel : MonoBehaviour, IKeyDownSubscriber, IKeySubscriber, IAcceptActionHandler, INextActionHandler, IBackActionHandler
{
	[SerializeField]
	protected CustomButton _mainButton;

	protected LoginScene _loginScene;

	protected KeyboardManager _keyboardManager;

	protected IBILogger _biLogger;

	protected IActionSystem _actions;

	internal PanelType _backButtonPanelType;

	internal PanelType _panelType;

	protected virtual GameObject SelectOnLoad { get; }

	public PriorityLevelEnum Priority => PriorityLevelEnum.Wrapper;

	protected void EnableButton(bool enabled)
	{
		if ((bool)_mainButton)
		{
			_mainButton.Interactable = enabled;
		}
	}

	public virtual void Initialize(LoginScene controllerLogin, IActionSystem actions, KeyboardManager keyboardManager, IBILogger biLogger)
	{
		_loginScene = controllerLogin;
		base.gameObject.SetActive(value: false);
		_keyboardManager = keyboardManager;
		_actions = actions;
		_biLogger = biLogger;
	}

	public virtual void Show()
	{
		MainThreadDispatcher.Dispatch(delegate
		{
			_actions.PushFocus(this);
			base.gameObject.SetActive(value: true);
			EventSystem.current.SetSelectedGameObject(SelectOnLoad);
			_keyboardManager?.Subscribe(this);
		});
	}

	public virtual void Hide()
	{
		CustomInputModule.DeselectAll();
		MainThreadDispatcher.Dispatch(delegate
		{
			_actions.PopFocus(this);
			base.gameObject.SetActive(value: false);
			_keyboardManager?.Unsubscribe(this);
		});
	}

	public virtual void OnAccept()
	{
		EventSystem current = EventSystem.current;
		if (!(_loginScene == null) && !_loginScene.IsLoading && !(_mainButton == null) && !(current == null))
		{
			current.SetSelectedGameObject(_mainButton.gameObject);
			if (_mainButton.Interactable)
			{
				_mainButton.Click();
				EnableButton(enabled: false);
			}
		}
	}

	public virtual void OnNext()
	{
		EventSystem current = EventSystem.current;
		if (!(_loginScene == null) && !_loginScene.IsLoading && !(_mainButton == null) && !(current == null) && !(SelectOnLoad == null) && (current.currentSelectedGameObject == null || current.currentSelectedGameObject == _mainButton.gameObject))
		{
			current.SetSelectedGameObject(SelectOnLoad);
		}
	}

	public virtual void OnButton_GoBack()
	{
		_loginScene.LoadPanel(_backButtonPanelType);
	}

	public void OnGenericHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	public void onGenericClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	protected void onInputField(string args)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	public void OnButton_GoToHelp()
	{
		_loginScene.LoadPanel(PanelType.Help, _panelType);
	}

	protected virtual void OnDisable()
	{
		_keyboardManager?.Unsubscribe(this);
	}

	public virtual bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape)
		{
			OnButton_GoBack();
			return true;
		}
		return false;
	}

	public virtual void OnBack(ActionContext context)
	{
		OnButton_GoBack();
	}
}
