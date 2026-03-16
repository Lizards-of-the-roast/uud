using Core.Code.Harnesses.OfflineHarnessServices;
using Core.Shared.Code.Network;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public static class PlayerInboxServiceWrapperFactory
{
	public static IPlayerInboxServiceWrapper Create()
	{
		if (MDNPlayerPrefs.DEBUG_MockPlayerInboxService && AllowPlayerInboxMockService())
		{
			return new HarnessPlayerInboxServiceWrapper();
		}
		IFrontDoorConnectionServiceWrapper frontDoorConnectionServiceWrapper = Pantry.Get<IFrontDoorConnectionServiceWrapper>();
		return new PlayerInboxServiceWrapper(inventoryServiceWrapper: Pantry.Get<IInventoryServiceWrapper>(), fdcAws: frontDoorConnectionServiceWrapper.FDCAWS);
	}

	public static bool AllowPlayerInboxMockService()
	{
		if (!Application.isEditor)
		{
			return Pantry.Get<IAccountClient>()?.AccountInformation?.HasRole_Debugging() == true;
		}
		return true;
	}
}
