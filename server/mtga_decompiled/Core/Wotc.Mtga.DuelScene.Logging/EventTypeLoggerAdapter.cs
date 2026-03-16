using Core.BI;
using Wizards.Mtga;

namespace Wotc.Mtga.DuelScene.Logging;

public class EventTypeLoggerAdapter : IBILoggerAdapter
{
	private readonly BIEventType _eventType;

	public EventTypeLoggerAdapter(BIEventType eventType)
	{
		_eventType = eventType;
	}

	public void Log(params (string, string)[] payload)
	{
		_eventType.SendWithDefaults(payload);
	}
}
