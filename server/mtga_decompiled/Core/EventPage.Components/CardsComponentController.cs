using System;
using System.Collections.Generic;
using System.Linq;
using EventPage.Components.NetworkModels;
using GreClient.CardData;
using Wizards.MDN;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;

namespace EventPage.Components;

public class CardsComponentController : IComponentController
{
	private CardsComponent _component;

	public CardsComponentController(CardsComponent component, CardsDisplayData data, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, Action onClick)
	{
		_component = component;
		CardsComponent component2 = _component;
		component2.OnClick = (Action)Delegate.Combine(component2.OnClick, onClick);
		List<uint> grpIds = data.GrpIds;
		if (grpIds != null && grpIds.Count > 0)
		{
			_component.CreateCards(data.GrpIds.Select((uint g) => new CardData(null, cardDatabase.CardDataProvider.GetCardPrintingById(g))).ToArray(), data.HeaderLocKey, data.DescriptionLocKey, cardViewBuilder);
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
