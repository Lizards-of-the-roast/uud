using Core.Meta.MainNavigation.SystemMessage;
using SharedClientCore.SharedClientCore.Code.PVPChallenge;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.PrivateGame;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Core.Meta.MainNavigation.Challenge;

public class PVPChallengeControllerFactory
{
	public static PVPChallengeController Create()
	{
		ChallengeDataProvider challengeDataProvider = Pantry.Get<ChallengeDataProvider>();
		IChallengeCommunicationWrapper challengeCommunicationWrapper = Pantry.Get<IChallengeCommunicationWrapper>();
		DeckDataProvider deckDataProvider = Pantry.Get<DeckDataProvider>();
		Pantry.Get<IFrontDoorConnectionServiceWrapper>();
		return new PVPChallengeController(cosmeticsProvider: Pantry.Get<CosmeticsProvider>(), challengeDeckValidation: Pantry.Get<IChallengeDeckValidation>(), connectionManager: Pantry.Get<ConnectionManager>(), accountClient: Pantry.Get<IAccountClient>(), challengeDataProvider: challengeDataProvider, challengeCommunicationWrapper: challengeCommunicationWrapper, deckDataProvider: deckDataProvider, systemMessageManager: Pantry.Get<ISystemMessageManager>(), clientLocProvider: Pantry.Get<IClientLocProvider>(), biLogger: Pantry.Get<IBILogger>());
	}
}
