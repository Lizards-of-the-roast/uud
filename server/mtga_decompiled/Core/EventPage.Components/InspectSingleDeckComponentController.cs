using System;
using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;

namespace EventPage.Components;

public class InspectSingleDeckComponentController : IComponentController
{
	private readonly InspectSingleDeckComponent _component;

	public InspectSingleDeckComponentController(InspectSingleDeckComponent component, Action<Client_Deck> onClick)
	{
		InspectSingleDeckComponentController inspectSingleDeckComponentController = this;
		_component = component;
		InspectSingleDeckComponent component2 = _component;
		component2.OnClick = (Action)Delegate.Combine(component2.OnClick, (Action)delegate
		{
			onClick?.Invoke(inspectSingleDeckComponentController._component.Deck);
		});
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
		bool active = false;
		InspectSingleDeckWidgetData inspectSingleDeckWidget = eventContext.PlayerEvent.EventUXInfo.EventComponentData.InspectSingleDeckWidget;
		if (inspectSingleDeckWidget?.Deck != null)
		{
			_component.Init(DeckServiceWrapperHelpers.ToClientModel(inspectSingleDeckWidget.Deck));
			active = true;
		}
		_component.gameObject.UpdateActive(active);
	}

	public void Update(IPlayerEvent playerEvent)
	{
	}
}
