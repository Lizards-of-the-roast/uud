using Core.Shared.Code.Providers;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.ServiceFactories;

public class StaticContentControllerFactory
{
	public static StaticContentController Create()
	{
		return new StaticContentController(Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS);
	}
}
