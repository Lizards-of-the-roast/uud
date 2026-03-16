using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public static class GatheringServiceWrapperFactory
{
	public static IGatheringServiceWrapper Create()
	{
		return new GatheringServiceWrapper(Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS);
	}
}
