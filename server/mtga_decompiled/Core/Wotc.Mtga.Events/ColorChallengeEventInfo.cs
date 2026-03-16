using EventPage.Components.NetworkModels;

namespace Wotc.Mtga.Events;

public class ColorChallengeEventInfo : BasicEventInfo
{
	private string _internalEventName;

	private bool? _updateDailyWeeklyRewards;

	private bool? _isPreconEvent;

	public override string InternalEventName
	{
		get
		{
			return _internalEventName ?? base.InternalEventName;
		}
		set
		{
			_internalEventName = value;
		}
	}

	public override bool UpdateDailyWeeklyRewards
	{
		get
		{
			return _updateDailyWeeklyRewards ?? base.UpdateDailyWeeklyRewards;
		}
		set
		{
			_updateDailyWeeklyRewards = value;
		}
	}

	public override bool IsPreconEvent
	{
		get
		{
			return _isPreconEvent ?? base.IsPreconEvent;
		}
		set
		{
			_isPreconEvent = value;
		}
	}

	public ColorChallengeEventInfo(EventInfoV3 eventInfo)
		: base(eventInfo)
	{
	}
}
