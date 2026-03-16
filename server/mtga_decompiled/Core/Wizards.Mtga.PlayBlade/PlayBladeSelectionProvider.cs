using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Wizards.Arena.Promises;
using Wizards.Unification.Models.PlayBlade;

namespace Wizards.Mtga.PlayBlade;

public class PlayBladeSelectionProvider : IPlayBladeSelectionProvider
{
	private bool _initialized;

	private BladeSelectionData _selectionData;

	private readonly PlayerPrefsDataProvider _playerPrefsDataProvider;

	private const string DefaultRankedPlayBladeQueueID = "StandardRanked";

	private const string DefaultUnrankedPlayBladeQueueID = "StandardUnranked";

	public PlayBladeSelectionProvider(PlayerPrefsDataProvider playerPrefsDataProvider)
	{
		_playerPrefsDataProvider = playerPrefsDataProvider;
	}

	public static PlayBladeSelectionProvider Create()
	{
		return new PlayBladeSelectionProvider(Pantry.Get<PlayerPrefsDataProvider>());
	}

	public BladeSelectionData GetDefaultSelectionData()
	{
		return new BladeSelectionData
		{
			findMatch = new FindMatchData
			{
				QueueId = "StandardUnranked",
				QueueIdForQueueType = new Dictionary<PlayBladeQueueType, string>
				{
					{
						PlayBladeQueueType.Ranked,
						"StandardRanked"
					},
					{
						PlayBladeQueueType.Unranked,
						"StandardUnranked"
					}
				},
				QueueType = PlayBladeQueueType.Unranked,
				UseBO3 = false
			},
			bladeType = BladeType.LastPlayed
		};
	}

	~PlayBladeSelectionProvider()
	{
		_selectionData = default(BladeSelectionData);
	}

	public BladeSelectionData GetSelection()
	{
		if (!_initialized)
		{
			Initialize();
		}
		return _selectionData;
	}

	private void Initialize()
	{
		if (_initialized)
		{
			return;
		}
		_playerPrefsDataProvider.GetPreference("PlayBladeSelectionData").Convert(delegate(string bladeSelectionJson)
		{
			if (bladeSelectionJson != null)
			{
				try
				{
					_selectionData = JsonConvert.DeserializeObject<BladeSelectionData>(bladeSelectionJson);
				}
				catch (Exception arg)
				{
					SimpleLog.LogError(string.Format("Failed to deserialize player preferences data for ${0}: {1}", "BladeSelectionData", arg));
					_selectionData = GetDefaultSelectionData();
				}
			}
			else
			{
				_selectionData = GetDefaultSelectionData();
			}
			if (string.IsNullOrEmpty(_selectionData.findMatch.QueueId))
			{
				_selectionData = GetDefaultSelectionData();
			}
			_selectionData.bladeType = BladeType.LastPlayed;
			_initialized = true;
			return Unit.Value;
		});
	}

	public void SetSelection(BladeSelectionData data)
	{
		if (!_initialized)
		{
			Initialize();
		}
		if (!_selectionData.Equals(data))
		{
			_selectionData = data;
			string value = JsonConvert.SerializeObject(data);
			_playerPrefsDataProvider.SetPreference("PlayBladeSelectionData", value);
		}
	}

	public void SetSelectedTab(BladeType bladeType)
	{
		if (!_initialized)
		{
			Initialize();
		}
		BladeSelectionData selectionData = _selectionData;
		selectionData.bladeType = bladeType;
		SetSelection(selectionData);
	}

	public bool IsEventBladeDeckSelected()
	{
		if (!_initialized)
		{
			Initialize();
		}
		return _selectionData.findMatch.DeckId != Guid.Empty;
	}
}
