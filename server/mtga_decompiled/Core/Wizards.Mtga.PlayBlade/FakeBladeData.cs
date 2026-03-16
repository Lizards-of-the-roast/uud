using System.Collections.Generic;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Rank;
using Wizards.Unification.Models.PlayBlade;

namespace Wizards.Mtga.PlayBlade;

public class FakeBladeData : IBladeModel
{
	public List<BladeEventInfo> Events { get; set; }

	public Dictionary<PlayBladeQueueType, List<BladeQueueInfo>> Queues { get; set; }

	public List<DeckViewInfo> Decks { get; set; }

	public List<RankViewInfo> Ranks { get; set; }

	public List<RecentlyPlayedInfo> RecentlyPlayed { get; set; }

	public List<BladeEventFilter> EventFilters { get; set; }

	public List<string> ViewedEvents { get; }

	public bool UnviewedEventsExist()
	{
		return true;
	}
}
