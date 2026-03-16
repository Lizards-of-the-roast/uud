using Wotc;

namespace Wizards.Mtga.PlayBlade;

public class GoToEventPageSignalArgs : SignalArgs
{
	public string InternalEventName { get; private set; }

	public GoToEventPageSignalArgs(object dispatcher, string eventName)
		: base(dispatcher)
	{
		InternalEventName = eventName;
	}
}
