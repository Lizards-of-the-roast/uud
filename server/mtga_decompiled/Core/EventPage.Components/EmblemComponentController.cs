using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;

namespace EventPage.Components;

public class EmblemComponentController : IComponentController
{
	private EmblemComponent _component;

	public EmblemComponentController(EmblemComponent component, EmblemDisplayData data, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_component = component;
		_component.SetEmblems(data.EmblemIDs, EventEmblem.eCardType.Emblem, cardDatabase, cardViewBuilder);
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
