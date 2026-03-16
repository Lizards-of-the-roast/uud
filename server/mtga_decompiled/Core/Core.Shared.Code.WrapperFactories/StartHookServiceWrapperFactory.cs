using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Core.Shared.Code.WrapperFactories;

public class StartHookServiceWrapperFactory
{
	public static IStartHookServiceWrapper Create()
	{
		return new AwsStartHookServiceWrapper(Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS, Pantry.Get<IInventoryServiceWrapper>(), Pantry.Get<CosmeticsProvider>(), Pantry.Get<DeckDataProvider>(), Pantry.Get<ICustomTokenProvider>(), Pantry.Get<IAchievementDataProvider>());
	}
}
