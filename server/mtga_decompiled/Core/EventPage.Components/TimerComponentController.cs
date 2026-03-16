using System;
using Wizards.MDN;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;

namespace EventPage.Components;

public class TimerComponentController : IComponentController
{
	private TimerComponent _component;

	public static IComponentController Create(EventComponent baseComponent, Action onTimerEnded)
	{
		if (baseComponent is TimerComponent component)
		{
			return new TimerComponentController(component, onTimerEnded);
		}
		return null;
	}

	public TimerComponentController(TimerComponent component, Action onTimerEnded)
	{
		_component = component;
		_component.OnTimerEnded = onTimerEnded;
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
	}

	public void Update(IPlayerEvent playerEvent)
	{
		bool active = false;
		switch (playerEvent.GetTimerState())
		{
		case EventTimerState.Preview:
			_component.Preview(playerEvent.EventInfo.StartTime, new MTGALocalizedString
			{
				Key = "MainNav/EventPage/EventStartTimer"
			});
			active = true;
			break;
		case EventTimerState.Unjoined_LockingSoon:
			_component.LockingSoon(playerEvent.EventInfo.LockedTime, new MTGALocalizedString
			{
				Key = "MainNav/EventPage/SignUpEndTimer"
			});
			active = true;
			break;
		case EventTimerState.Joined_ClosingSoon:
			_component.EndingSoon(playerEvent.EventInfo.ClosedTime, new MTGALocalizedString
			{
				Key = "MainNav/EventPage/EventEndTimer"
			});
			active = true;
			break;
		case EventTimerState.ClosedAndCompleted:
			_component.Closed(new MTGALocalizedString
			{
				Key = "MainNav/EventPage/EventEndTimer"
			});
			active = true;
			break;
		}
		_component.gameObject.UpdateActive(active);
	}
}
