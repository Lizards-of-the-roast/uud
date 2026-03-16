using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public static class InventoryManagerFactory
{
	public static InventoryManager Create()
	{
		return Create(Pantry.Get<IInventoryServiceWrapper>(), Pantry.Get<IMercantileServiceWrapper>());
	}

	private static InventoryManager Create(IInventoryServiceWrapper inventoryServiceWrapper, IMercantileServiceWrapper mercantileServiceWrapper)
	{
		return new InventoryManager(inventoryServiceWrapper, mercantileServiceWrapper);
	}
}
