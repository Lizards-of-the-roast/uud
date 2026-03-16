using System;
using Core.Code.Harnesses.OfflineHarnessServices;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public class PlayerRankServiceWrapperFactory
{
	public static IPlayerRankServiceWrapper Create()
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		return currentEnvironment.HostPlatform switch
		{
			HostPlatform.AWS => Create(Pantry.Get<IFrontDoorConnectionServiceWrapper>()), 
			HostPlatform.Harness => new HarnessRankServiceProvider(), 
			_ => throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}"), 
		};
	}

	public static IPlayerRankServiceWrapper Create(IFrontDoorConnectionServiceWrapper fd)
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		if (currentEnvironment.HostPlatform == HostPlatform.AWS)
		{
			return new AwsPlayerRankServiceWrapper(fd.FDCAWS);
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
