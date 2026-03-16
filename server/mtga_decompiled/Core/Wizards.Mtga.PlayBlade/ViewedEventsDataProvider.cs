using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Promises;
using Newtonsoft.Json;
using Wizards.Arena.Promises;

namespace Wizards.Mtga.PlayBlade;

public class ViewedEventsDataProvider
{
	private bool _initialized;

	private List<string> _viewedEvents;

	private PlayerPrefsDataProvider _playerPrefsDataProvider;

	private List<string> _inFlightViewedEvents;

	public ViewedEventsDataProvider(PlayerPrefsDataProvider playerPrefsDataProvider)
	{
		_playerPrefsDataProvider = playerPrefsDataProvider;
	}

	public static ViewedEventsDataProvider Create()
	{
		return new ViewedEventsDataProvider(Pantry.Get<PlayerPrefsDataProvider>());
	}

	public Promise<Unit> Initialize()
	{
		if (_initialized)
		{
			return new SimplePromise<Unit>(Unit.Value);
		}
		return _playerPrefsDataProvider.GetPreference("ViewedEventsData").ThenOnMainThread(delegate(string viewedEvents)
		{
			if (!string.IsNullOrEmpty(viewedEvents))
			{
				try
				{
					_viewedEvents = JsonConvert.DeserializeObject<List<string>>(viewedEvents);
				}
				catch (Exception arg)
				{
					SimpleLog.LogError($"Failed to deserialize player preferences data for viewed events: {arg}");
					throw;
				}
			}
			else
			{
				_viewedEvents = new List<string>();
			}
			_inFlightViewedEvents = ((_viewedEvents == null) ? new List<string>() : new List<string>(_viewedEvents));
			_initialized = true;
		}).Convert((string _) => Unit.Value);
	}

	public Promise<Unit> AddViewedEvents(IEnumerable<string> eventIds)
	{
		if (!_initialized)
		{
			return Initialize().Then(delegate
			{
				AddViewedEventInternal(eventIds);
			});
		}
		AddViewedEventInternal(eventIds);
		return new SimplePromise<Unit>(Unit.Value);
		void AddViewedEventInternal(IEnumerable<string> eventIdinternal)
		{
			foreach (string eventId in eventIds)
			{
				if (_inFlightViewedEvents.Contains(eventId))
				{
					break;
				}
				_inFlightViewedEvents.Add(eventId);
			}
		}
	}

	public Promise<bool> HasBeenViewed(string eventId)
	{
		if (!_initialized)
		{
			return Initialize().Convert((Unit _) => _viewedEvents.Contains(eventId));
		}
		return new SimplePromise<bool>(_viewedEvents.Contains(eventId));
	}

	public Promise<Unit> TrimViewedEvents(List<string> activeEvents)
	{
		if (!_initialized)
		{
			return Initialize().ThenOnMainThread((Action)delegate
			{
				TrimViewedEventsInternal();
			});
		}
		TrimViewedEventsInternal();
		return new SimplePromise<Unit>(Unit.Value);
		void TrimViewedEventsInternal()
		{
			List<string> list = new List<string>();
			foreach (string viewedEvent in _viewedEvents)
			{
				if (activeEvents.Contains(viewedEvent))
				{
					list.Add(viewedEvent);
				}
			}
			_viewedEvents = list;
			_playerPrefsDataProvider.SetPreference("ViewedEventsData", JsonConvert.SerializeObject(_viewedEvents));
		}
	}

	public void SaveViewedEvents()
	{
		_viewedEvents = _inFlightViewedEvents.ToList();
		_playerPrefsDataProvider.SetPreference("ViewedEventsData", JsonConvert.SerializeObject(_viewedEvents));
	}
}
