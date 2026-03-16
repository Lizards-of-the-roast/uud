using System;
using Core.Code.Harnesses.OfflineHarnessServices;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;
using _3rdParty.Steam;

namespace Core.Shared.Code.WrapperFactories;

public class MercantileServiceWrapperFactory
{
	public static IMercantileServiceWrapper Create()
	{
		Client_RMTPlatform platform = Client_RMTPlatform.Xsolla;
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			platform = Client_RMTPlatform.Apple;
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			platform = Client_RMTPlatform.Android;
		}
		else if (Steam.Status == Steam.SteamStatus.Available)
		{
			platform = Client_RMTPlatform.Steam;
		}
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		switch (currentEnvironment.HostPlatform)
		{
		case HostPlatform.AWS:
		{
			IFrontDoorConnectionServiceWrapper frontDoorConnectionServiceWrapper = Pantry.Get<IFrontDoorConnectionServiceWrapper>();
			return new AwsMercantileServiceWrapper(inventoryServiceWrapper: Pantry.Get<IInventoryServiceWrapper>(), listingsProvider: Pantry.Get<ListingsProvider>(), cosmeticsProvider: Pantry.Get<CosmeticsProvider>(), fdc: frontDoorConnectionServiceWrapper.FDCAWS, platform: platform);
		}
		case HostPlatform.Harness:
			return new HarnessMercantileServiceWrapper();
		default:
			throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
		}
	}

	public static IMercantileServiceWrapper Create(IFrontDoorConnectionServiceWrapper fd, IInventoryServiceWrapper inventory, ListingsProvider listingsProvider, CosmeticsProvider cosmeticsProvider, Client_RMTPlatform platform)
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		if (currentEnvironment.HostPlatform == HostPlatform.AWS)
		{
			return new AwsMercantileServiceWrapper(fd.FDCAWS, inventory, listingsProvider, cosmeticsProvider, platform);
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
