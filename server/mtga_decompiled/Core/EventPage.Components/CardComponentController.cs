using System.Collections.Generic;
using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;

namespace EventPage.Components;

public class CardComponentController : IComponentController
{
	private readonly EmblemComponent _component;

	private readonly CardDatabase _cardDatabase;

	private readonly CardViewBuilder _cardViewBuilder;

	public static IComponentController Create(EventComponent baseComponent, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		if (baseComponent is EmblemComponent component)
		{
			return new CardComponentController(component, cardDatabase, cardViewBuilder);
		}
		return null;
	}

	private CardComponentController(EmblemComponent component, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_component = component;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
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
		CardDisplayData cardDisplay = playerEvent.EventUXInfo.EventComponentData.CardDisplay;
		if (!playerEvent.InPlayingMatchesModule && cardDisplay != null)
		{
			_component.SetEmblems(new List<uint> { cardDisplay.GrpID }, EventEmblem.eCardType.SkinCard, _cardDatabase, _cardViewBuilder);
			active = true;
		}
		_component.gameObject.UpdateActive(active);
	}
}
