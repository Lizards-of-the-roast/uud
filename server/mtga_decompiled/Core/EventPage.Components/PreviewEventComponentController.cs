using System;
using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;

namespace EventPage.Components;

public class PreviewEventComponentController : IComponentController
{
	private CashTournamentComponent _component;

	private EventManager _eventManager;

	private PreviewEventWidgetData _data;

	public PreviewEventComponentController(CashTournamentComponent component, PreviewEventWidgetData data, EventManager eventManager, Action<string> onClick)
	{
		_component = component;
		_eventManager = eventManager;
		_data = data;
		_component.Initialize(data.LocKey);
		CashTournamentComponent component2 = _component;
		component2.OnClick = (Action)Delegate.Combine(component2.OnClick, (Action)delegate
		{
			onClick?.Invoke(data.LinkedEventName);
		});
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
		if (_eventManager.EventContexts.Find((EventContext c) => c.PlayerEvent.EventInfo.InternalEventName == _data.LinkedEventName) == null)
		{
			_component.gameObject.UpdateActive(active: false);
		}
	}

	public void Update(IPlayerEvent playerEvent)
	{
	}
}
