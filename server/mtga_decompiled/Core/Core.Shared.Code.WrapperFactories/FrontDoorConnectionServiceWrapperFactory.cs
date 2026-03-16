using System;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public static class FrontDoorConnectionServiceWrapperFactory
{
	public static IFrontDoorConnectionServiceWrapper Create()
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		if (currentEnvironment.HostPlatform == HostPlatform.AWS)
		{
			return new AwsFrontDoorConnectionServiceWrapper();
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
