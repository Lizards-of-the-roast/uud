using System.Collections.Generic;
using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wotc.Mtga.Events;

namespace EventPage.Components;

public class BoosterPacksComponentController : IComponentController
{
	private BoosterPacksComponent _component;

	public BoosterPacksComponentController(BoosterPacksComponent component, BoosterPacksDisplayData data)
	{
		_component = component;
		List<uint> collationIds = data.CollationIds;
		if (collationIds != null && collationIds.Count > 0)
		{
			_component.SetMaterials(data.CollationIds);
		}
		else
		{
			_component.gameObject.SetActive(value: false);
		}
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
	}

	public void Update(IPlayerEvent playerEvent)
	{
	}
}
