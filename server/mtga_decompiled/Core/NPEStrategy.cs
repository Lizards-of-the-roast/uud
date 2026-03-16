using System;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.NPE;

public class NPEStrategy
{
	private const float BLOCKER_DELAY_IN_MS = 500f;

	public static RequestHandlerFactory<BaseUserRequest> CreateHandlers(int gameNumber, NPE_Game game, DeckHeuristic aiConfig, ICardDatabaseAdapter cardDatabase)
	{
		return CreateHandlers(gameNumber, new ObjectPool(), new Random(), game, aiConfig, cardDatabase);
	}

	public static RequestHandlerFactory<BaseUserRequest> CreateHandlers(int gameNumber, IObjectPool objectPool, Random random, NPE_Game game, DeckHeuristic aiConfig, ICardDatabaseAdapter cardDatabase)
	{
		return gameNumber switch
		{
			0 => new RequestHandlerFactory_NPE_Game1(RandomRequestHandlers.Create(cardDatabase, random, objectPool)), 
			1 => new RequestHandlerFactory_NPE_Game2(RandomRequestHandlers.Create(cardDatabase, random, objectPool), objectPool, 500f), 
			_ => new RoundTripRequestHandlerFactory(new SelectTargetsRequestNPEHandlerFactory(game, aiConfig), new DeclareAttackerRequestNPEHandlerFactory(cardDatabase, aiConfig, game), new DeclareBlockersRequestNPEHandlerFactory(aiConfig, cardDatabase, game, 0.75f), new NPEStrategyRequestHandlerFactory(RandomRequestHandlers.Create(cardDatabase, random, objectPool), cardDatabase, game, aiConfig)), 
		};
	}
}
