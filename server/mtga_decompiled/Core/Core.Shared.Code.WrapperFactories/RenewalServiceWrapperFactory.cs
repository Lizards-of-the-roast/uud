using System;
using Core.Shared.Code.Network;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public static class RenewalServiceWrapperFactory
{
	public static IRenewalServiceWrapper Create()
	{
		return Create(Pantry.Get<IFrontDoorConnectionServiceWrapper>());
	}

	public static IRenewalServiceWrapper Create(IFrontDoorConnectionServiceWrapper fd)
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		return currentEnvironment.HostPlatform switch
		{
			HostPlatform.AWS => new AwsRenewalServiceWrapper(fd.FDCAWS), 
			HostPlatform.Unknown => throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}"), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
