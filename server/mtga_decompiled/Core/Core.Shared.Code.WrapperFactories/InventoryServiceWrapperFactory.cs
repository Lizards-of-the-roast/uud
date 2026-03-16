using System;
using Core.Code.Harnesses.OfflineHarnessServices;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Core.Shared.Code.WrapperFactories;

public class InventoryServiceWrapperFactory
{
	public static IInventoryServiceWrapper Create()
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		return currentEnvironment.HostPlatform switch
		{
			HostPlatform.AWS => Create(Pantry.Get<IFrontDoorConnectionServiceWrapper>(), Pantry.Get<CosmeticsProvider>(), Pantry.Get<DeckDataProvider>()), 
			HostPlatform.Harness => new HarnessInventoryServiceWrapper(), 
			_ => throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}"), 
		};
	}

	public static IInventoryServiceWrapper Create(IFrontDoorConnectionServiceWrapper fd, CosmeticsProvider cosmeticsProvider, DeckDataProvider deckDataProvider)
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		if (currentEnvironment.HostPlatform == HostPlatform.AWS)
		{
			return new AwsInventoryServiceWrapper(fd.FDCAWS, cosmeticsProvider, deckDataProvider);
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
