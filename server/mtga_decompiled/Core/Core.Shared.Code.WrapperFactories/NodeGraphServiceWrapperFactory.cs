using System;
using AWS;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public class NodeGraphServiceWrapperFactory
{
	public static INodeGraphServiceWrapper Create()
	{
		return Create(Pantry.Get<IFrontDoorConnectionServiceWrapper>(), Pantry.Get<IInventoryServiceWrapper>());
	}

	public static INodeGraphServiceWrapper Create(IFrontDoorConnectionServiceWrapper fd, IInventoryServiceWrapper inventoryServiceWrapper)
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		if (currentEnvironment.HostPlatform == HostPlatform.AWS)
		{
			return new AwsNodeGraphServiceWrapper(fd.FDCAWS, inventoryServiceWrapper);
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
