using EventPage.Components.NetworkModels;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Events;

public class NPEPlayerEvent : BasicPlayerEvent
{
	private class DefaultFactory : IGenericFactory<EventInfoV3, ICourseInfoWrapper, NPEPlayerEvent>
	{
		public NPEPlayerEvent Create(EventInfoV3 eventInfo, ICourseInfoWrapper course)
		{
			return new NPEPlayerEvent(eventInfo, course, Pantry.Get<IEventsServiceWrapper>(), Pantry.Get<DeckDataProvider>(), Pantry.Get<FormatManager>());
		}
	}

	public new static readonly IGenericFactory<EventInfoV3, ICourseInfoWrapper, NPEPlayerEvent> CurrentFactory = new DefaultFactory();

	public int NextGame => _courseInfo.NPENextGameNumber;

	public NPEPlayerEvent(EventInfoV3 eventInfo, ICourseInfoWrapper course, IEventsServiceWrapper eventsServiceWrapper, DeckDataProvider deckDataProvider, FormatManager formatManager)
		: base(eventInfo, course, eventsServiceWrapper, deckDataProvider, formatManager)
	{
	}
}
