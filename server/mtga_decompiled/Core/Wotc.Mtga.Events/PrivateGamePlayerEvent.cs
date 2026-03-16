using EventPage.Components.NetworkModels;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Events;

public class PrivateGamePlayerEvent : BasicPlayerEvent
{
	private class DefaultFactory : IGenericFactory<EventInfoV3, ICourseInfoWrapper, PrivateGamePlayerEvent>
	{
		public PrivateGamePlayerEvent Create(EventInfoV3 eventInfo, ICourseInfoWrapper course)
		{
			return new PrivateGamePlayerEvent(eventInfo, course, Pantry.Get<IEventsServiceWrapper>(), Pantry.Get<DeckDataProvider>(), Pantry.Get<FormatManager>());
		}
	}

	public new static readonly IGenericFactory<EventInfoV3, ICourseInfoWrapper, PrivateGamePlayerEvent> CurrentFactory = new DefaultFactory();

	public PrivateGamePlayerEvent(EventInfoV3 eventInfo, ICourseInfoWrapper course, IEventsServiceWrapper eventsServiceWrapper, DeckDataProvider deckDataProvider, FormatManager formatManager)
		: base(eventInfo, course, eventsServiceWrapper, deckDataProvider, formatManager)
	{
		eventInfo.EventUXInfo.EventComponentData.TitleRankText = new LocalizedTextData
		{
			LocKey = "Events/Event_Title_DirectGame"
		};
		eventInfo.EventUXInfo.EventComponentData.DescriptionText = new LocalizedTextData
		{
			LocKey = "Events/Event_Title_DirectGame"
		};
	}
}
