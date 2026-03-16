using Core.Shared.Code.PVPChallenge;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.PVPChallenge;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Core.Shared.Code.WrapperFactories;

public class ChallengeCommunicationWrapperFactory
{
	public static IChallengeCommunicationWrapper Create()
	{
		ISocialManager socialManager = Pantry.Get<ISocialManager>();
		Matchmaking matchmaking = Pantry.Get<Matchmaking>();
		IAccountClient accountClient = Pantry.Get<IAccountClient>();
		IChallengeServiceWrapper challengeServiceWrapper = Pantry.Get<IChallengeServiceWrapper>();
		CosmeticsProvider cosmeticsProvider = Pantry.Get<CosmeticsProvider>();
		return new ChallengeCommunicationWrapper(matchmaking, socialManager, accountClient, challengeServiceWrapper, cosmeticsProvider);
	}
}
