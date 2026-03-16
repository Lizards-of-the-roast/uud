using System;
using Core.Shared.Code.Network;
using Wizards.Mtga;

namespace Core.Shared.Code.WrapperFactories;

public class SetDataServiceWrapperFactory
{
	public static ISetDataServiceWrapper Create()
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		_ = currentEnvironment.HostPlatform;
		_ = 1;
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
