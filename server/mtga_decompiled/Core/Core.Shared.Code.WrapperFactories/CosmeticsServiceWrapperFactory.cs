using System;
using Core.Code.Harnesses.OfflineHarnessServices;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public class CosmeticsServiceWrapperFactory
{
	public static ICosmeticsServiceWrapper Create()
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		ICosmeticsServiceWrapper cosmeticsServiceWrapper = null;
		return currentEnvironment.HostPlatform switch
		{
			HostPlatform.AWS => new AwsCosmeticsServiceWrapper(Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS), 
			HostPlatform.Harness => new HarnessCosmeticsServiceWrapper(), 
			_ => throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}"), 
		};
	}

	public static ICosmeticsServiceWrapper Create(IFrontDoorConnectionServiceWrapper fd)
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		ICosmeticsServiceWrapper cosmeticsServiceWrapper = null;
		if (currentEnvironment.HostPlatform == HostPlatform.AWS)
		{
			return new AwsCosmeticsServiceWrapper(fd.FDCAWS);
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
