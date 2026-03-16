using System;
using System.Collections.Generic;
using UnityEngine;
using Wizards.Mtga;

namespace Core.Shared.Code.DebugTools;

public class RemotePlayerPreferencesWatcher
{
	public Dictionary<string, string> GetRemotePlayerPrefs()
	{
		if (TryGetPlayerPrefDataProvider(out var dataProvider))
		{
			return dataProvider.GetAllPreferencesCopy();
		}
		return null;
	}

	public void OnDataChange(Action dataChangeAction)
	{
		if (TryGetPlayerPrefDataProvider(out var dataProvider))
		{
			PlayerPrefsDataProvider playerPrefsDataProvider = dataProvider;
			playerPrefsDataProvider.PreferenceDataChanged = (Action)Delegate.Remove(playerPrefsDataProvider.PreferenceDataChanged, dataChangeAction);
			PlayerPrefsDataProvider playerPrefsDataProvider2 = dataProvider;
			playerPrefsDataProvider2.PreferenceDataChanged = (Action)Delegate.Combine(playerPrefsDataProvider2.PreferenceDataChanged, dataChangeAction);
		}
	}

	public void SetRemotePreference(string key, string value)
	{
		if (TryGetPlayerPrefDataProvider(out var dataProvider))
		{
			dataProvider.SetPreference(key, value);
		}
	}

	public void RemoveRemotePreference(string key)
	{
		if (TryGetPlayerPrefDataProvider(out var dataProvider))
		{
			dataProvider.RemovePreference(key);
		}
	}

	private bool TryGetPlayerPrefDataProvider(out PlayerPrefsDataProvider dataProvider)
	{
		if (!Application.isPlaying)
		{
			dataProvider = null;
			return false;
		}
		dataProvider = Pantry.Get<PlayerPrefsDataProvider>();
		return dataProvider != null;
	}
}
