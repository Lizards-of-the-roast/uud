using System;
using Core.Code.Harnesses.OfflineHarnessServices;
using Wizards.Mtga;
using Wotc.Mtga.Events;

namespace Core.MainNavigation.RewardTrack;

public static class SetMasteryStrategyFactory
{
	public static ISetMasteryStrategy Create()
	{
		return Create(Pantry.Get<InventoryManager>());
	}

	public static ISetMasteryStrategy Create(InventoryManager invMan)
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		return currentEnvironment.HostPlatform switch
		{
			HostPlatform.AWS => new AwsSetMasteryStrategy(invMan, Pantry.Get<CampaignGraphManager>()), 
			HostPlatform.Harness => new HarnessSetMasteryStrategy(), 
			_ => throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}"), 
		};
	}
}
