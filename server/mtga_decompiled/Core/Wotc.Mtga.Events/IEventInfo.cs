using System;
using System.Collections.Generic;
using Wizards.Arena.Enums.Event;
using Wizards.Mtga.FrontDoorModels;

namespace Wotc.Mtga.Events;

public interface IEventInfo
{
	string EventId { get; }

	string InternalEventName { get; }

	MDNEventState EventState { get; }

	MDNEFormatType FormatType { get; }

	DateTime StartTime { get; set; }

	DateTime LockedTime { get; set; }

	DateTime ClosedTime { get; set; }

	List<EventEntryFeeInfo> EntryFees { get; }

	List<EventTag> EventTags { get; }

	bool IsRanked { get; }

	bool UpdateQuests { get; }

	bool UpdateDailyWeeklyRewards { get; }

	bool IsArenaPlayModeEvent { get; }

	bool IsPreconEvent { get; }

	bool SkipDeckValidation { get; }

	bool AllowUncollectedCards { get; }

	bool IsAiOpponent { get; }
}
