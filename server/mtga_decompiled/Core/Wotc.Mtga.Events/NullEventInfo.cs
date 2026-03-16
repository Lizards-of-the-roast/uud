using System;
using System.Collections.Generic;
using Wizards.Arena.Enums.Event;
using Wizards.Mtga.FrontDoorModels;

namespace Wotc.Mtga.Events;

public class NullEventInfo : IEventInfo
{
	public string EventId { get; }

	public string InternalEventName { get; }

	public MDNEventState EventState { get; }

	public MDNEFormatType FormatType { get; }

	public DateTime StartTime { get; set; }

	public DateTime LockedTime { get; set; }

	public DateTime ClosedTime { get; set; }

	public List<EventEntryFeeInfo> EntryFees { get; }

	public List<EventTag> EventTags { get; }

	public bool IsRanked { get; }

	public bool UpdateQuests { get; }

	public bool UpdateDailyWeeklyRewards { get; }

	public bool IsArenaPlayModeEvent { get; }

	public bool IsPreconEvent { get; }

	public bool SkipDeckValidation { get; }

	public bool AllowUncollectedCards { get; }

	public bool IsAiOpponent { get; }
}
