using System.Collections.Generic;

namespace Wotc.Mtga.Events;

public static class IPlayerEventEventExtensions
{
	public static (bool AllowUncollectedCards, IReadOnlyList<uint> CardPool) DeckValidationEventData(this IPlayerEvent @event)
	{
		return (AllowUncollectedCards: @event?.EventInfo?.AllowUncollectedCards == true, CardPool: @event?.CourseData?.CardPool);
	}
}
