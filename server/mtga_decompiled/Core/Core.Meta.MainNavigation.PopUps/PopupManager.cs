using System;
using Core.Code.Input;
using MTGA.KeyboardManager;
using UnityEngine;
using Wizards.Mtga.Platforms;

namespace Core.Meta.MainNavigation.PopUps;

public class PopupManager : IKeyUpSubscriber, IKeySubscriber, IKeyDownSubscriber, IBackActionHandler, IDisposable
{
	private PopupBase? _activePopup;

	private readonly KeyboardManager _keyboardManager;

	private readonly IActionSystem _actionSystem;

	private SettingsMenuHost? _menuHost;

	public PriorityLevelEnum Priority => PriorityLevelEnum.Wrapper;

	public PopupManager(KeyboardManager keyboardManager, IActionSystem actionSystem)
	{
		_actionSystem = actionSystem;
		_keyboardManager = keyboardManager;
		_keyboardManager.Subscribe(this);
	}

	public void SetMenuHost(SettingsMenuHost menuHost)
	{
		_menuHost = menuHost;
	}

	public void RegisterPopup(PopupBase popup)
	{
		if (_activePopup == null)
		{
			_actionSystem.PushFocus(this);
		}
		_activePopup = popup;
	}

	public void UnregisterPopup(PopupBase popup)
	{
		if (_activePopup != null)
		{
			_actionSystem.PopFocus(this);
		}
		_activePopup = null;
	}

	public bool HasActivePopup()
	{
		return _activePopup != null;
	}

	public void CloseActivePopup()
	{
		if (HasActivePopup())
		{
			_activePopup?.Activate(activate: false);
		}
	}

	public void ToggleMenu()
	{
		if (_menuHost != null && _menuHost.IsOpen())
		{
			_menuHost.Close();
		}
		else
		{
			_menuHost?.Open();
		}
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape)
		{
			if (PlatformUtils.IsHandheld())
			{
				return HandleBackButton();
			}
			if (_menuHost != null && _menuHost.IsOpen())
			{
				ToggleMenu();
			}
			else if (_activePopup != null)
			{
				_activePopup.OnEscape();
			}
			else
			{
				ToggleMenu();
			}
			return true;
		}
		return false;
	}

	public bool HandleKeyUp(KeyCode curr, Modifiers mods)
	{
		if ((curr == KeyCode.Return || curr == KeyCode.KeypadEnter) && _activePopup != null)
		{
			_activePopup.OnEnter();
		}
		return false;
	}

	private bool HandleBackButton()
	{
		if (_activePopup != null)
		{
			_activePopup.OnEscape();
			return true;
		}
		return false;
	}

	public void Dispose()
	{
		_keyboardManager.Unsubscribe(this);
	}

	public void OnBack(ActionContext context)
	{
		HandleBackButton();
	}
}
