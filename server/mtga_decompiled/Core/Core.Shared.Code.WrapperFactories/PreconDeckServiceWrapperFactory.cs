using System;
using Core.Code.Harnesses.OfflineHarnessServices;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public class PreconDeckServiceWrapperFactory
{
	public static IPreconDeckServiceWrapper Create()
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		return currentEnvironment.HostPlatform switch
		{
			HostPlatform.AWS => new AwsPreconDeckServiceWrapper(Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS), 
			HostPlatform.Harness => new HarnessPreconServiceWrapper(), 
			_ => throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}"), 
		};
	}
}
