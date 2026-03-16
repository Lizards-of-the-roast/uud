using System;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public class ActiveMatchesServiceWrapperFactory
{
	public static IActiveMatchesServiceWrapper Create()
	{
		return Create(Pantry.Get<IFrontDoorConnectionServiceWrapper>());
	}

	public static IActiveMatchesServiceWrapper Create(IFrontDoorConnectionServiceWrapper fd)
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		if (currentEnvironment.HostPlatform == HostPlatform.AWS)
		{
			return new AwsActiveMatchesServiceWrapper(fd.FDCAWS);
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
