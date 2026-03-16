using System;
using Core.Code.Input;
using Core.Shared.Code.DebugTools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.AutoPlay;

[RequireComponent(typeof(Image))]
public class DebugInfoIMGUI : MonoBehaviour
{
	private Image _clickShield;

	private bool _debugGUIEnabled;

	private EventSystem _unityEventSystem;

	private DebugInfoIMGUIOnGui _debugInfoImguiOnGui;

	private IActionSystem _actionSystem;

	private Func<bool> _hasDebugRole;

	public void Init(PAPA papa, Func<bool> hasDebugRole)
	{
		_hasDebugRole = hasDebugRole;
		_debugInfoImguiOnGui = base.gameObject.AddComponent<DebugInfoIMGUIOnGui>();
		_debugInfoImguiOnGui.Init(papa);
		_debugInfoImguiOnGui.StayOpenToggled += OnStayOpenToggled;
		_actionSystem = papa.Actions;
		IActionSystem.Debug debugActions = _actionSystem.DebugActions;
		debugActions.DebugClose = (Action)Delegate.Combine(debugActions.DebugClose, new Action(OnDebugClose));
		IActionSystem.Debug debugActions2 = _actionSystem.DebugActions;
		debugActions2.DebugOpen = (Action)Delegate.Combine(debugActions2.DebugOpen, new Action(OnDebugOpen));
		IActionSystem.Debug debugActions3 = _actionSystem.DebugActions;
		debugActions3.DebugToggle = (Action)Delegate.Combine(debugActions3.DebugToggle, new Action(OnDebugToggle));
		_clickShield = GetComponent<Image>();
		UpdateState();
	}

	private void SetDebugToggleState(bool isOpen)
	{
		if (HasDebugRole() || Pantry.CurrentEnvironment.accountSystemEnvironment == EnvironmentType.PreProd || PerformanceCSVLogger.IsReporting || AutoPlayManager.CanRunAutoPlay())
		{
			_debugGUIEnabled = isOpen;
		}
		else
		{
			_debugGUIEnabled = false;
			_debugInfoImguiOnGui.DebugGUILocked = false;
		}
		UpdateState();
	}

	private bool HasDebugRole()
	{
		if (_hasDebugRole != null)
		{
			return _hasDebugRole();
		}
		return false;
	}

	private void UpdateState()
	{
		_debugInfoImguiOnGui.enabled = _debugGUIEnabled || _debugInfoImguiOnGui.DebugGUILocked;
		_clickShield.enabled = _debugInfoImguiOnGui.enabled;
	}

	private void OnStayOpenToggled(bool stayOpen)
	{
		UpdateState();
	}

	private void OnDebugOpen()
	{
		SetDebugToggleState(isOpen: true);
	}

	private void OnDebugClose()
	{
		SetDebugToggleState(isOpen: false);
	}

	private void OnDebugToggle()
	{
		SetDebugToggleState(!_debugGUIEnabled);
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		SetDebugToggleState(isOpen: false);
	}

	private void OnDestroy()
	{
		if (_actionSystem != null)
		{
			IActionSystem.Debug debugActions = _actionSystem.DebugActions;
			debugActions.DebugClose = (Action)Delegate.Remove(debugActions.DebugClose, new Action(OnDebugClose));
			IActionSystem.Debug debugActions2 = _actionSystem.DebugActions;
			debugActions2.DebugOpen = (Action)Delegate.Remove(debugActions2.DebugOpen, new Action(OnDebugOpen));
			IActionSystem.Debug debugActions3 = _actionSystem.DebugActions;
			debugActions3.DebugToggle = (Action)Delegate.Remove(debugActions3.DebugToggle, new Action(OnDebugToggle));
		}
		_hasDebugRole = null;
	}
}
