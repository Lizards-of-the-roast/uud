using Core.Meta.Social;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.Network;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.WrapperFactories;

public static class LobbyWrapperFactory
{
	public static ILobbyServiceWrapper CreateServiceWrapper()
	{
		return new AwsLobbyServiceWrapper(Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS);
	}

	public static ILobbyDataProvider CreateDataProvider()
	{
		return new LobbyDataProvider(Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS, Pantry.Get<ILobbyServiceWrapper>());
	}

	public static ILobbyController CreateController()
	{
		return new LobbyController(Pantry.Get<ILobbyDataProvider>(), Pantry.Get<ISocialManager>(), Pantry.Get<IBILogger>());
	}
}
