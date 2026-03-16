using System;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public static class EventsServiceWrapperFactory
{
	public static IEventsServiceWrapper Create()
	{
		return Create(Pantry.Get<IFrontDoorConnectionServiceWrapper>(), Pantry.Get<IInventoryServiceWrapper>());
	}

	public static IEventsServiceWrapper Create(IFrontDoorConnectionServiceWrapper fd, IInventoryServiceWrapper inventoryServiceWrapper)
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		if (currentEnvironment.HostPlatform == HostPlatform.AWS)
		{
			return new AwsEventServiceWrapper(fd.FDCAWS, inventoryServiceWrapper);
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
