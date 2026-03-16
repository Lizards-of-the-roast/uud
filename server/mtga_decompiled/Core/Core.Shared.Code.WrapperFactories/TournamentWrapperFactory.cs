using Core.Meta.MainNavigation.Tournaments;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.Network;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public static class TournamentWrapperFactory
{
	public static ITournamentServiceWrapper CreateServiceWrapper()
	{
		return new AwsTournamentServiceWrapper(Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS);
	}

	public static ITournamentDataProvider CreateDataProvider()
	{
		return new TournamentDataProvider(Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS, Pantry.Get<ITournamentServiceWrapper>());
	}

	public static ITournamentController CreateController()
	{
		return new TournamentController(Pantry.Get<ITournamentDataProvider>(), Pantry.Get<ISocialManager>(), Pantry.Get<IBILogger>());
	}
}
