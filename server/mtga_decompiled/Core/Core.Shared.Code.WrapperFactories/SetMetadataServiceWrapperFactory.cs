using System;
using Core.Shared.Code.Network;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public class SetMetadataServiceWrapperFactory
{
	public static ISetMetadataServiceWrapper Create()
	{
		return Create(Pantry.Get<IFrontDoorConnectionServiceWrapper>());
	}

	public static ISetMetadataServiceWrapper Create(IFrontDoorConnectionServiceWrapper fd)
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		if (currentEnvironment.HostPlatform == HostPlatform.AWS)
		{
			return new AwsSetMetadataServiceWrapper(fd.FDCAWS);
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
