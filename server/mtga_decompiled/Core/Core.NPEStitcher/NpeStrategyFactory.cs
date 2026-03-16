using System;
using Wizards.Mtga;

namespace Core.NPEStitcher;

public static class NpeStrategyFactory
{
	public static INpeStrategy Create()
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		if (currentEnvironment.HostPlatform == HostPlatform.AWS)
		{
			return new NodeNpeStrategy();
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
