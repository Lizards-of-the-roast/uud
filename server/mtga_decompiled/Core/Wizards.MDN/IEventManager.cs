namespace Wizards.MDN;

public interface IEventManager
{
	EventContext GetEventContext(string internalEventName);
}
