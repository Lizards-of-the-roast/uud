using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wizards.Arena.Promises;
using Wizards.Unification.Models.PlayBlade;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wizards.Mtga;

public class PlayerPrefsDataProvider
{
	public static class PlayerPrefsKeys
	{
		public const string RecentGamesData = "RecentGamesData";

		public const string PlayBladeSelectionData = "PlayBladeSelectionData";

		public const string ViewedEventsData = "ViewedEventsData";

		public const string AutoDeclineFriendInvites = "AutoDeclineFriendInvites";

		public const string BlockNonFriendChallenges = "BlockNonFriendChallenges";
	}

	private readonly IPlayerPrefsServiceWrapper _playerPrefsService;

	private DTO_PlayerPreferences _playerPrefs;

	private bool _dirty;

	public Action PreferenceDataChanged;

	public bool Initialized { get; private set; }

	public static PlayerPrefsDataProvider Create()
	{
		return new PlayerPrefsDataProvider(Pantry.Get<IPlayerPrefsServiceWrapper>());
	}

	public async Task WaitForInitialized()
	{
		while (!Initialized)
		{
			await Task.Yield();
		}
	}

	public PlayerPrefsDataProvider(IPlayerPrefsServiceWrapper playerPrefsService)
	{
		_playerPrefsService = playerPrefsService;
	}

	public Promise<DTO_PlayerPreferences> Initialize()
	{
		return _playerPrefsService.GetPlayerPreferences().Then(delegate(Promise<DTO_PlayerPreferences> promise)
		{
			if (promise.Successful)
			{
				_playerPrefs = promise.Result ?? new DTO_PlayerPreferences();
				DTO_PlayerPreferences playerPrefs = _playerPrefs;
				if (playerPrefs.Preferences == null)
				{
					Dictionary<string, string> dictionary = (playerPrefs.Preferences = new Dictionary<string, string>());
				}
				Initialized = true;
				if (PreferenceDataChanged != null)
				{
					PreferenceDataChanged();
				}
			}
			else
			{
				PromiseExtensions.Logger.Error($"Failed to get player preferences: {promise.Error}");
			}
		});
	}

	public Dictionary<string, string> GetAllPreferencesCopy()
	{
		if (_playerPrefs?.Preferences != null)
		{
			return new Dictionary<string, string>(_playerPrefs.Preferences);
		}
		return null;
	}

	public Promise<DTO_PlayerPreferences> RemovePreference(string key)
	{
		if (!Initialized)
		{
			return Initialize().Then(delegate
			{
				RemovePreferenceAfterInitialization(key);
				return Save();
			});
		}
		RemovePreferenceAfterInitialization(key);
		return Save();
	}

	private void RemovePreferenceAfterInitialization(string key)
	{
		_dirty |= _playerPrefs.Preferences.Remove(key);
		if (_dirty && PreferenceDataChanged != null)
		{
			PreferenceDataChanged();
		}
	}

	public Promise<DTO_PlayerPreferences> SetPreference(string key, string value)
	{
		if (!Initialized)
		{
			return Initialize().Then(delegate
			{
				SetPreferencePostInitialization(key, value);
				return Save();
			});
		}
		SetPreferencePostInitialization(key, value);
		return Save();
	}

	private void SetPreferencePostInitialization(string key, string value)
	{
		if (value == null)
		{
			RemovePreferenceAfterInitialization(key);
			return;
		}
		_playerPrefs.Preferences.TryGetValue(key, out var value2);
		_playerPrefs.Preferences[key] = value;
		_dirty |= value2 != value;
		if (_dirty && PreferenceDataChanged != null)
		{
			PreferenceDataChanged();
		}
	}

	public Promise<string> GetPreference(string key)
	{
		if (Initialized)
		{
			return new SimplePromise<string>(GetPreferencePostInitialization(key));
		}
		return Initialize().Convert((DTO_PlayerPreferences _) => GetPreferencePostInitialization(key));
	}

	private string GetPreferencePostInitialization(string key)
	{
		string value = default(string);
		if (_playerPrefs?.Preferences?.TryGetValue(key, out value) != true)
		{
			return null;
		}
		return value;
	}

	public Promise<bool> GetPreferenceBool(string key)
	{
		return GetPreference(key).Convert(delegate(string s)
		{
			if (s == null)
			{
				return false;
			}
			if (s == "1")
			{
				return true;
			}
			return "true".Equals(s, StringComparison.InvariantCultureIgnoreCase) ? true : false;
		});
	}

	public Promise<DTO_PlayerPreferences> SetPreferenceBool(string key, bool value)
	{
		if (!value)
		{
			return RemovePreference(key);
		}
		return SetPreference(key, "1");
	}

	private Promise<DTO_PlayerPreferences> Save()
	{
		if (!Initialized)
		{
			string text = "Attempting to save player preference before data provider is initialized";
			SimpleLog.LogError(text);
			return new SimplePromise<DTO_PlayerPreferences>(new Error(0, text));
		}
		if (!_dirty)
		{
			return new SimplePromise<DTO_PlayerPreferences>(_playerPrefs);
		}
		return _playerPrefsService.SetPlayerPreferences(_playerPrefs.Preferences).IfSuccess(delegate
		{
			_dirty = false;
		}).IfError(delegate(Error e)
		{
			PromiseExtensions.Logger.Error("Failed to save player preferences tp service: " + e.Message);
		});
	}
}
