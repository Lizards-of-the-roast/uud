using System;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public class PreferredPrintingServiceWrapperFactory
{
	public static IPreferredPrintingServiceWrapper Create()
	{
		return Create(Pantry.Get<IFrontDoorConnectionServiceWrapper>());
	}

	public static IPreferredPrintingServiceWrapper Create(IFrontDoorConnectionServiceWrapper fd)
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		if (currentEnvironment.HostPlatform == HostPlatform.AWS)
		{
			return new AwsPreferredPrintingServiceWrapper(fd.FDCAWS);
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
