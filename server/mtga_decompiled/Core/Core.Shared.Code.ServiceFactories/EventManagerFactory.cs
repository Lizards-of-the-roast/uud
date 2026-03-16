using Wizards.MDN;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.ServiceFactories;

public class EventManagerFactory
{
	public static EventManager Create()
	{
		return new EventManager(Pantry.Get<IEventsServiceWrapper>(), Pantry.Get<IAccountClient>());
	}
}
