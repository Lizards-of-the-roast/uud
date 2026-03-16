using System;
using System.Collections.Generic;
using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wotc.Mtga.Events;

namespace EventPage.Components;

public class ViewCardPoolComponentController : IComponentController
{
	private ViewCardPoolComponent _component;

	public ViewCardPoolComponentController(ViewCardPoolComponent component, ViewCardPoolWidgetData data, Action<List<uint>> onClicked)
	{
		_component = component;
		_component.OnClicked = delegate
		{
			onClicked?.Invoke(data.CardPool);
		};
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
