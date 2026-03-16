using System;
using System.Collections.Generic;
using Core.Code.Promises;
using UnityEngine;
using Wizards.Arena.Promises;
using Wotc.Mtga.Extensions;

namespace Wizards.Mtga.Npe;

[CreateAssetMenu(fileName = "NPE Progression Flag", menuName = "NPE Progression Flag")]
public class NpeProgressionFlag : ScriptableObject
{
	[Obsolete]
	private const string _oldAnimatorName = "NPEOnboardingStateMachineV2";

	private const string _defaultUser = "default";

	private bool? _cachedFlagStatus;

	[SerializeField]
	private string _flagName;

	[SerializeField]
	private bool _defaultFlagStatus = true;

	public string FlagName => _flagName;

	private string PlayerPrefsFlagKey => "NPEv2:" + _flagName;

	[Obsolete]
	private bool FlagHasBeenCompletedOnDevice()
	{
		string value = "NPEOnboardingStateMachineV2:" + _flagName;
		string text = Pantry.Get<IAccountClient>()?.AccountInformation?.PersonaID;
		if (string.IsNullOrEmpty(text))
		{
			text = "default";
			Debug.LogError("Failed to get UserId for NPE state machine flags. Some or all state machine flags will be stored/retrieved via the default user, specific only to this device.");
		}
		return ((IReadOnlyCollection<string>)(object)MDNPlayerPrefs.GetStateMachineFlags(text).Split(',')).Contains(value);
	}

	public Promise<bool> FlagHasBeenCompleted()
	{
		return Pantry.Get<PlayerPrefsDataProvider>().GetPreferenceBool(PlayerPrefsFlagKey).ThenOnMainThreadIfSuccess(delegate(bool completed)
		{
			if (!completed)
			{
				completed = FlagHasBeenCompletedOnDevice();
				if (completed)
				{
					MarkFlagCompletionStatus();
				}
			}
			_cachedFlagStatus = completed;
		})
			.ThenOnMainThreadIfError(delegate(Error error)
			{
				Debug.LogError($"Error retrieving NPE progression flag status for flag {_flagName}: {error.Message}\nError Code: {error.Code}\nException: {error.Exception}");
				_cachedFlagStatus = null;
			});
	}

	public bool GetCachedValue()
	{
		return _cachedFlagStatus ?? _defaultFlagStatus;
	}

	public bool? GetCachedFlagStatus()
	{
		return _cachedFlagStatus;
	}

	public void MarkFlagCompletionStatus()
	{
		PlayerPrefsDataProvider playerPrefsDataProvider = Pantry.Get<PlayerPrefsDataProvider>();
		_cachedFlagStatus = true;
		playerPrefsDataProvider.SetPreferenceBool(PlayerPrefsFlagKey, value: true);
	}
}
