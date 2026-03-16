using System.Collections.Generic;
using Wizards.Mtga.Decks;
using Wizards.Unification.Models.PlayBlade;

namespace Wizards.Mtga.PlayBlade;

public interface IBladeModel
{
	List<BladeEventInfo> Events { get; }

	Dictionary<PlayBladeQueueType, List<BladeQueueInfo>> Queues { get; }

	List<DeckViewInfo> Decks { get; }

	List<RecentlyPlayedInfo> RecentlyPlayed { get; }

	List<BladeEventFilter> EventFilters { get; }

	bool UnviewedEventsExist();
}
