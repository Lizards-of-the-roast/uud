using System;
using Wizards.Mtga;

namespace Wotc.Mtga.Events;

public static class ColorChallengeStrategyFactory
{
	public static IColorChallengeStrategy Create()
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		if (currentEnvironment.HostPlatform == HostPlatform.AWS)
		{
			return new AwsColorChallengeStrategy();
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
