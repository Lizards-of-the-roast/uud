using System;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;

public class BotBattle_SetTestStrategy : IHeadlessClientStrategy
{
	private readonly SetTest _setTest;

	private readonly RequestHandlerFactory<BaseUserRequest> _requestHandlerFactory;

	public BotBattle_SetTestStrategy(SetTest setTest, RequestHandlerFactory<BaseUserRequest> requestHandlerFactory)
	{
		_setTest = setTest;
		_requestHandlerFactory = requestHandlerFactory;
	}

	public void SetGameState(MtgGameState state)
	{
		_setTest.UpdateGameState(state);
		_requestHandlerFactory.SetGameState(state);
	}

	public void HandleRequest(BaseUserRequest request)
	{
		_setTest.LogRequest(request);
		_requestHandlerFactory.GetHandlerForRequest(request)?.HandleRequest();
	}

	public static RequestHandlerFactory<BaseUserRequest> CreateHandlers(SetTest setTest, ICardDatabaseAdapter database, Random random = null, IObjectPool objectPool = null)
	{
		if (random == null)
		{
			random = new Random();
		}
		if (objectPool == null)
		{
			objectPool = new ObjectPool();
		}
		return new RoundTripRequestHandlerFactory(new SelectTargetsRequestRandomHandlerFactory(random, objectPool), new DeclareAttackerRequestRandomHandlerFactory(random, objectPool), new DeclareBlockersRequestRandomHandlerFactory(random), new BotBattleRequestHandlerFactory(setTest, database, random, RandomRequestHandlers.Create(database, random, objectPool)));
	}
}
