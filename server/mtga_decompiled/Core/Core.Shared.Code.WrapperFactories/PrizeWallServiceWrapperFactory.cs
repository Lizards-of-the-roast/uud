using Core.Shared.Code.Network;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public static class PrizeWallServiceWrapperFactory
{
	public static IPrizeWallServiceWrapper Create()
	{
		return new PrizeWallServiceWrapper(Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS);
	}
}
