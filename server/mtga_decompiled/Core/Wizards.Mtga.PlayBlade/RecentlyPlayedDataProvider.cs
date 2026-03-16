using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Wizards.Arena.Promises;

namespace Wizards.Mtga.PlayBlade;

public class RecentlyPlayedDataProvider
{
	private readonly PlayerPrefsDataProvider _playerPrefsDataProvider;

	private List<RecentGamesData> _data = new List<RecentGamesData>();

	private bool _initialized;

	private const int SAVED_COUNT = 10;

	public static RecentlyPlayedDataProvider Create()
	{
		return new RecentlyPlayedDataProvider();
	}

	private RecentlyPlayedDataProvider()
	{
		_playerPrefsDataProvider = Pantry.Get<PlayerPrefsDataProvider>();
	}

	~RecentlyPlayedDataProvider()
	{
		_data = null;
	}

	private Promise<Unit> Initialize()
	{
		if (_initialized)
		{
			return new SimplePromise<Unit>();
		}
		return _playerPrefsDataProvider.GetPreference("RecentGamesData").Convert(delegate(string bladeSelectionJson)
		{
			if (bladeSelectionJson != null)
			{
				try
				{
					_data = JsonConvert.DeserializeObject<List<RecentGamesData>>(bladeSelectionJson);
				}
				catch (Exception arg)
				{
					SimpleLog.LogError(string.Format("Failed to deserialize player preferences data for ${0}: {1}", "RecentGamesData", arg));
					_data = new List<RecentGamesData>();
				}
			}
			else
			{
				_data = new List<RecentGamesData>();
			}
			_initialized = true;
			return Unit.Value;
		});
	}

	public Promise<List<RecentGamesData>> GetData()
	{
		if (!_initialized)
		{
			return Initialize().Convert((Unit _) => _data);
		}
		return new SimplePromise<List<RecentGamesData>>(_data);
	}

	public void AddRecentlyPlayedGame(string eventName, Guid deckId)
	{
		if (!_initialized)
		{
			Initialize().IfSuccess(delegate
			{
				AddRecentlyPlayedGamePostInit(eventName, deckId);
			});
		}
		else
		{
			AddRecentlyPlayedGamePostInit(eventName, deckId);
		}
	}

	public void AddRecentlyPlayedGamePostInit(string eventName, Guid deckId)
	{
		RecentGamesData recentGame = new RecentGamesData(eventName, deckId);
		_data.RemoveAll((RecentGamesData i) => i.EventName == recentGame.EventName);
		_data.Add(recentGame);
		int num = _data.Count - 10;
		int count = ((num > 0) ? num : 0);
		_data = _data.Skip(count).Take(10).ToList();
		string value = JsonConvert.SerializeObject(_data);
		_playerPrefsDataProvider.SetPreference("RecentGamesData", value);
	}

	public void RemoveRecentPlayedGame(RecentGamesData removeData)
	{
		if (_data.Remove(removeData))
		{
			string value = JsonConvert.SerializeObject(_data);
			_playerPrefsDataProvider.SetPreference("RecentGamesData", value);
		}
	}
}
