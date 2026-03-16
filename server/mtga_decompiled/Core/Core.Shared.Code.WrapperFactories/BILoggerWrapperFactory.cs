using System;
using Wizards.Mtga;

namespace Core.Shared.Code.WrapperFactories;

public static class BILoggerWrapperFactory
{
	public static IBILogger Create()
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		return currentEnvironment.HostPlatform switch
		{
			HostPlatform.AWS => new BILogger(), 
			HostPlatform.Harness => new NullBILogger(), 
			_ => throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}"), 
		};
	}
}
