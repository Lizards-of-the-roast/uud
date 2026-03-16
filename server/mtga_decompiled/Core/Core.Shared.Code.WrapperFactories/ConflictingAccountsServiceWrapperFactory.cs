using Core.Shared.Code.Network;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public static class ConflictingAccountsServiceWrapperFactory
{
	public static IConflictingAccountsServiceWrapper Create()
	{
		return Create(Pantry.Get<IFrontDoorConnectionServiceWrapper>(), Pantry.Get<IAccountClient>());
	}

	public static IConflictingAccountsServiceWrapper Create(IFrontDoorConnectionServiceWrapper fdc, IAccountClient accountClient)
	{
		return new ConflictingAccountsServiceWrapper(accountClient, fdc.FDCAWS);
	}
}
