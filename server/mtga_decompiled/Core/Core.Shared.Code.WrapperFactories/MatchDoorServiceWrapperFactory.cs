using System;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public class MatchDoorServiceWrapperFactory
{
	public static IMatchdoorServiceWrapper Create()
	{
		return Create(Pantry.Get<IFrontDoorConnectionServiceWrapper>(), Pantry.Get<IInventoryServiceWrapper>());
	}

	public static IMatchdoorServiceWrapper Create(IFrontDoorConnectionServiceWrapper fd, IInventoryServiceWrapper inventoryServiceWrapper)
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		if (currentEnvironment.HostPlatform == HostPlatform.AWS)
		{
			return new AwsMatchdoorServiceWrapper(fd.FDCAWS, inventoryServiceWrapper);
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
