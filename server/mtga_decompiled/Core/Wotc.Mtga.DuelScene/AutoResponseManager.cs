using System;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class AutoResponseManager : IGameplaySettingsProvider
{
	private const AutoPassOption AUTO_PASS_OPTION_DEFAULT = AutoPassOption.ResolveMyStackEffects;

	private readonly GameManager _gameManager;

	private readonly GreInterface _gre;

	private readonly SettingsMenuHost _settingsMenuHost;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IWorkflowProvider _workflowProvider;

	private readonly ISettingsMessageGenerator _settingsMessageGenerator;

	private SettingsMessage _pendingSettings;

	private SettingsMessage _currentSettings;

	private const ManaSelectionType AUTO_MANA_ON = ManaSelectionType.Auto;

	private const ManaSelectionType AUTO_MANA_OFF = ManaSelectionType.Manual;

	public bool FullControlDisabled => !FullControlEnabled;

	public bool FullControlEnabled => CurrentSettings.FullControlEnabled();

	public bool FullControlLocked => CurrentSettings.FullControlLocked();

	public bool AutoPassEnabled => CurrentSettings.AutoPassEnabled();

	public bool ResolveAllEnabled => CurrentSettings.ResolveAllEnabled();

	public bool AutoPayManaEnabled => CurrentSettings.AutoPayManaEnabled();

	public bool AutoSelectReplacementEffects => CurrentSettings.AutoSelectReplacementEffects();

	public GameplaySettings GameplaySettings => new GameplaySettings(CurrentSettings);

	public SettingsMessage CurrentSettings => _pendingSettings ?? _currentSettings;

	public event Action<SettingsMessage> SettingsUpdated;

	public AutoResponseManager(GameManager gameManager, GreInterface gre, IGameStateProvider gameStateProvider, IWorkflowProvider workflowProvider, ISettingsMessageGenerator settingsMessageGenerator, SettingsMenuHost settingsMenuHost)
	{
		_gameManager = gameManager;
		_gre = gre;
		_gameStateProvider = gameStateProvider;
		_workflowProvider = workflowProvider;
		_settingsMessageGenerator = settingsMessageGenerator ?? NullSettingsMessageGenerator.Default;
		_settingsMenuHost = settingsMenuHost;
		SetStartupSettings();
	}

	private void SetStartupSettings()
	{
		_gre.SetSettings(new SettingsMessage
		{
			ManaSelectionType = (MDNPlayerPrefs.AutoPayMana ? ManaSelectionType.Auto : ManaSelectionType.Manual),
			AutoSelectReplacementSetting = (MDNPlayerPrefs.AutoChooseReplacementEffects ? Setting.Enable : Setting.Disable),
			AutoPassOption = AutoPassOption.ResolveMyStackEffects,
			DefaultAutoPassOption = AutoPassOption.ResolveMyStackEffects
		});
	}

	public void UpdateSettings(SettingsMessage settingsMessage)
	{
		bool flag = _currentSettings.AutoPassEnabled();
		bool flag2 = settingsMessage.AutoPassEnabled();
		int num;
		if (_pendingSettings != null && _pendingSettings.AutoPassOption == settingsMessage.AutoPassOption)
		{
			num = ((_pendingSettings.DefaultAutoPassOption == settingsMessage.DefaultAutoPassOption) ? 1 : 0);
			if (num != 0)
			{
				_pendingSettings = null;
			}
		}
		else
		{
			num = 0;
		}
		_currentSettings = settingsMessage;
		this.SettingsUpdated?.Invoke(_currentSettings);
		if (num != 0 && !flag && flag2 && (_workflowProvider.GetCurrentWorkflow() ?? _workflowProvider.GetPendingWorkflow()) is IYieldWorkflow yieldWorkflow)
		{
			yieldWorkflow.OnAutoYieldEnabled();
		}
	}

	public void ToggleFullControl()
	{
		SetAutoPassOption(FullControlEnabled ? AutoPassOption.ResolveMyStackEffects : AutoPassOption.FullControl, AutoPassOption.ResolveMyStackEffects);
	}

	public void ToggleLockedFullControl()
	{
		if (!FullControlDisabled)
		{
			SetAutoPassOption(AutoPassOption.FullControl, FullControlLocked ? AutoPassOption.ResolveMyStackEffects : AutoPassOption.FullControl);
		}
	}

	public void ToggleForceLockedFullControl()
	{
		SetAutoPassOption(FullControlEnabled ? AutoPassOption.ResolveMyStackEffects : AutoPassOption.FullControl, FullControlLocked ? AutoPassOption.ResolveMyStackEffects : AutoPassOption.FullControl);
	}

	public void OnPhaseChanged()
	{
		if (FullControlEnabled && !FullControlLocked)
		{
			ResetAutoPassToDefault();
		}
	}

	private void ResetAutoPassToDefault()
	{
		SetAutoPassOption(AutoPassOption.ResolveMyStackEffects, AutoPassOption.ResolveMyStackEffects);
	}

	public void SetAutoSelectReplacementSetting(bool enabled)
	{
		if (_gre != null)
		{
			_gre.SetSettings(new SettingsMessage
			{
				AutoSelectReplacementSetting = (enabled ? Setting.Enable : Setting.Disable)
			});
			MDNPlayerPrefs.AutoChooseReplacementEffects = enabled;
		}
	}

	public void ToggleAutoPass()
	{
		if (!_settingsMenuHost.IsOpen() && !SocialUI.IsSendFriendInviteShowing())
		{
			AutoPassOption autoPassOption = AutoPassOption.ResolveMyStackEffects;
			if (!AutoPassEnabled)
			{
				autoPassOption = ((UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift)) ? AutoPassOption.Turn : AutoPassOption.UnlessOpponentAction);
			}
			SetAutoPassOption(autoPassOption, AutoPassOption.ResolveMyStackEffects);
		}
	}

	public void SetAutoPassOption(AutoPassOption autoPassOption, AutoPassOption defaultAutopassOption)
	{
		SettingsMessage settingsMessage = _settingsMessageGenerator.SetTurnAutoPass(_currentSettings, autoPassOption, defaultAutopassOption);
		if (settingsMessage != null)
		{
			_pendingSettings = settingsMessage;
			_gre.SetAutoPass(autoPassOption, defaultAutopassOption, _gameStateProvider.CurrentGameState.Value.GameWideTurn);
		}
	}

	public void SetManaAutoPayment(bool enabled)
	{
		if (_gre != null)
		{
			_gre.SetAutoPayMana(enabled ? ManaSelectionType.Auto : ManaSelectionType.Manual);
			MDNPlayerPrefs.AutoPayMana = enabled;
		}
	}

	public void SetResolveAll(bool enabled)
	{
		AutoPassOption stackAutoPassOption = (enabled ? AutoPassOption.ResolveAll : AutoPassOption.Clear);
		_gre.SetSettings(new SettingsMessage
		{
			StackAutoPassOption = stackAutoPassOption
		}, _gameStateProvider.CurrentGameState.Value.GameWideTurn);
		if (!enabled)
		{
			StyledButton buttonWithTag = _gameManager.UIManager.GetButtonWithTag(ButtonTag.ResolveAll);
			if (buttonWithTag != null)
			{
				buttonWithTag.ResetButton();
			}
		}
	}
}
