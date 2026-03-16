using Core.Shared.Code.Store.Steam;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga;
using Wizards.Mtga.Store;
using Wotc.Mtga.Loc;
using _3rdParty.Steam;

namespace Platforms;

public static class StoreFactory
{
	public static StoreManager PantryCreate()
	{
		return Create(Pantry.Get<IAccountClient>(), new UnityCrossThreadLogger(), Pantry.Get<IBILogger>(), Pantry.Get<StandaloneStoreConfig>(), Pantry.Get<IClientLocProvider>());
	}

	private static StoreManager Create(IAccountClient accountClient, Wizards.Arena.Client.Logging.ILogger logger, IBILogger biLogger, StandaloneStoreConfig storeConfig, IClientLocProvider locProvider)
	{
		switch (Application.platform)
		{
		case RuntimePlatform.OSXPlayer:
		case RuntimePlatform.WindowsPlayer:
			return BuildStandaloneStore();
		case RuntimePlatform.IPhonePlayer:
		case RuntimePlatform.Android:
			return BuildMobileStore();
		default:
			return new InactiveStoreManager(accountClient, logger, biLogger, locProvider);
		}
		StoreManager BuildMobileStore()
		{
			return new GeneralStoreManager(new IAPPurchaseController(logger), accountClient, logger, biLogger);
		}
		StoreManager BuildStandaloneStore()
		{
			if (Steam.Status == Steam.SteamStatus.Available)
			{
				return new GeneralStoreManager(new SteamPurchaseController(logger), accountClient, logger, biLogger);
			}
			if (storeConfig.DesiredStoreType == StandaloneStoreConfig.StandaloneStoreTypes.Xsolla)
			{
				return new XsollaStoreManager(accountClient, logger, biLogger);
			}
			return new InactiveStoreManager(accountClient, logger, biLogger, locProvider);
		}
	}
}
