using System;
using Wizards.MDN;
using Wotc.Mtga.Events;

namespace EventPage.Components;

public class EventButtonComponentController : IComponentController
{
	private EventButtonComponent _component;

	public static IComponentController Create(EventComponent baseComponent, string serializedData, Action onClick)
	{
		if (baseComponent is EventButtonComponent component)
		{
			return new EventButtonComponentController(component, onClick);
		}
		return null;
	}

	private EventButtonComponentController(EventButtonComponent component, Action onClick)
	{
		_component = component;
		EventButtonComponent component2 = _component;
		component2.OnClick = (Action)Delegate.Combine(component2.OnClick, onClick);
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
	}

	public void Update(IPlayerEvent playerEvent)
	{
	}
}
