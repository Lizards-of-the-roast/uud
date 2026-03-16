using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Code.ClientFeatureToggle;
using EventPage.Components.NetworkModels;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Format;
using Wizards.Unification.Models.Events;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wizards.MDN;

public class EventManager : IEventManager
{
	private readonly IEventsServiceWrapper _eventsServiceWrapper;

	private readonly IAccountClient _accountClient;

	private IColorChallengeStrategy _colorChallengeStrategy;

	private CardDatabase _cardDatabase;

	private DateTime _lastEventContextRefresh;

	public List<EventContext> EventContexts { get; } = new List<EventContext>();

	public List<ClientDynamicFilterTag> DynamicFilterTags { get; private set; } = new List<ClientDynamicFilterTag>();

	public Dictionary<string, EventContext> EventsByInternalName { get; } = new Dictionary<string, EventContext>();

	public EventContext PrivateGameEventContext => EventContexts.FirstOrDefault((EventContext e) => e.PlayerEvent is PrivateGamePlayerEvent);

	public bool RefreshingEventContexts { get; private set; }

	public EventContext ColorMasteryEventContext => EventContexts.FirstOrDefault((EventContext _) => _.PlayerEvent is ColorChallengePlayerEvent);

	public ColorChallengePlayerEvent ColorMasteryEvent => ColorMasteryEventContext?.PlayerEvent as ColorChallengePlayerEvent;

	public EventManager(IEventsServiceWrapper serviceWrapper, IAccountClient accountClient)
	{
		_eventsServiceWrapper = serviceWrapper;
		_accountClient = accountClient;
	}

	public void Inject(CardDatabase cardDatabase)
	{
		_cardDatabase = cardDatabase;
	}

	private static IPlayerEvent GetPlayerEvent(EventPage.Components.NetworkModels.EventInfoV3 eventInfo, ICourseInfoWrapper courseInfo)
	{
		IGenericFactory<EventPage.Components.NetworkModels.EventInfoV3, ICourseInfoWrapper, IPlayerEvent> genericFactory = (eventInfo.InternalEventName.Contains("DirectGame") ? PrivateGamePlayerEvent.CurrentFactory : (eventInfo.InternalEventName.Contains("NPE") ? NPEPlayerEvent.CurrentFactory : ((eventInfo.InternalEventName == "ColorChallenge") ? ((IGenericFactory<EventPage.Components.NetworkModels.EventInfoV3, ICourseInfoWrapper, IPlayerEvent>)ColorChallengePlayerEvent.CurrentFactory) : ((IGenericFactory<EventPage.Components.NetworkModels.EventInfoV3, ICourseInfoWrapper, IPlayerEvent>)((!FormatUtilities.IsLimited(eventInfo.FormatType)) ? BasicPlayerEvent.CurrentFactory : LimitedPlayerEvent.CurrentFactory)))));
		return genericFactory.Create(eventInfo, courseInfo);
	}

	public EventContext GetEventContext(string internalEventName)
	{
		EventContext value = null;
		if (internalEventName == "ColorChallenge")
		{
			value = ColorMasteryEventContext;
		}
		else
		{
			EventsByInternalName.TryGetValue(internalEventName, out value);
		}
		return value;
	}

	public IEnumerator Coroutine_GetEventsAndCourses()
	{
		return Coroutine_GetEventsAndCourses(null);
	}

	public IEnumerator Coroutine_GetEventsAndCourses(Action<bool> callback)
	{
		if (EventContexts.Count != 0 && _lastEventContextRefresh > DateTime.Now - TimeSpan.FromSeconds(10.0))
		{
			yield break;
		}
		bool requestsSucceeded = true;
		RefreshingEventContexts = true;
		Dictionary<string, EventContext> previousEventContexts = EventsByInternalName.ToDictionary((KeyValuePair<string, EventContext> kvp) => kvp.Key, (KeyValuePair<string, EventContext> kvp) => kvp.Value);
		EventContexts.Clear();
		EventsByInternalName.Clear();
		bool showAllEvents = Pantry.Get<ClientFeatureToggleDataProvider>().GetToggleValueById("ShowAllActiveEvents");
		Promise<List<EventPage.Components.NetworkModels.EventInfoV3>> eventsPromise = _eventsServiceWrapper?.GetAllEvents(_cardDatabase.CardDataProvider, showAllEvents);
		if (eventsPromise != null)
		{
			yield return eventsPromise.AsCoroutine();
			if (eventsPromise.Successful)
			{
				List<EventPage.Components.NetworkModels.EventInfoV3> events = eventsPromise.Result;
				Promise<List<ClientDynamicFilterTag>> filterPromise = _eventsServiceWrapper.GetFilterTags(showAllEvents);
				yield return filterPromise.AsCoroutine();
				if (filterPromise.Successful)
				{
					DynamicFilterTags = filterPromise.Result;
				}
				else
				{
					requestsSucceeded = false;
				}
				List<EventPage.Components.NetworkModels.EventInfoV3> list = new List<EventPage.Components.NetworkModels.EventInfoV3>();
				foreach (EventPage.Components.NetworkModels.EventInfoV3 item in events)
				{
					bool flag = item.AllowedCountryCodes != null && item.AllowedCountryCodes.Any();
					bool flag2 = item.ExcludedCountryCodes != null && item.ExcludedCountryCodes.Any();
					if (!flag && !flag2)
					{
						list.Add(item);
					}
					else if (flag && item.AllowedCountryCodes.Contains(_accountClient.AccountInformation.CountryCode))
					{
						list.Add(item);
					}
					else if (!flag && flag2 && !item.ExcludedCountryCodes.Contains(_accountClient.AccountInformation.CountryCode))
					{
						list.Add(item);
					}
				}
				events = list;
				Promise<List<ICourseInfoWrapper>> coursesPromise = _eventsServiceWrapper.GetEventCourses();
				yield return coursesPromise.AsCoroutine();
				if (coursesPromise.Successful)
				{
					List<ICourseInfoWrapper> result = coursesPromise.Result;
					result = _eventsServiceWrapper.CreateCoursesForJoinState(result, events);
					foreach (EventPage.Components.NetworkModels.EventInfoV3 evt in events)
					{
						ICourseInfoWrapper courseInfoWrapper = result.FirstOrDefault((ICourseInfoWrapper c) => c.InternalEventName == evt.InternalEventName);
						if (courseInfoWrapper != null || !(evt.InternalEventName != "DirectGame"))
						{
							EventContext value;
							PostMatchContext postMatchContext = (previousEventContexts.TryGetValue(evt.InternalEventName, out value) ? value.PostMatchContext : null);
							EventContext eventContext = new EventContext
							{
								PlayerEvent = GetPlayerEvent(evt, courseInfoWrapper),
								PostMatchContext = postMatchContext
							};
							EventContexts.Add(eventContext);
							EventsByInternalName.Add(evt.InternalEventName, eventContext);
						}
					}
					_lastEventContextRefresh = DateTime.Now;
				}
				else
				{
					requestsSucceeded = false;
				}
			}
			else if (!eventsPromise.Successful)
			{
				_ = (int)eventsPromise.Error;
			}
		}
		else
		{
			requestsSucceeded = false;
		}
		RefreshingEventContexts = false;
		callback?.Invoke(requestsSucceeded);
	}
}
