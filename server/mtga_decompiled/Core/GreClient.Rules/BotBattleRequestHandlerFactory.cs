using System;
using Wotc.Mtga.Cards.Database;

namespace GreClient.Rules;

public class BotBattleRequestHandlerFactory : RequestHandlerFactory<BaseUserRequest>
{
	private readonly RequestHandlerFactory<BaseUserRequest> _requestHandlerFactory;

	private readonly Random _random;

	private readonly SetTest _setTest;

	private readonly ICardDatabaseAdapter _cardDatabase;

	public BotBattleRequestHandlerFactory(SetTest setTest, ICardDatabaseAdapter cardDatabase, Random random, RequestHandlerFactory<BaseUserRequest> requestHandlerFactory)
	{
		_setTest = setTest;
		_cardDatabase = cardDatabase;
		_random = random;
		_requestHandlerFactory = requestHandlerFactory;
	}

	public override BaseUserRequestHandler GetHandlerForRequest(BaseUserRequest request)
	{
		if (!(request is ActionsAvailableRequest request2))
		{
			if (!(request is SelectNRequest request3))
			{
				if (request is PayCostsRequest decision)
				{
					return new PayCostsRequestBotBattleStrategyHandler(decision, _random);
				}
				return _requestHandlerFactory.GetHandlerForRequest(request);
			}
			return new SelectNRequestBotBattleStrategyHandler(request3, _setTest, _random, _cardDatabase);
		}
		return new ActionsAvailableRequestBotBattleStrategyHandler(request2, _random, _setTest);
	}

	public override void SetGameState(MtgGameState state)
	{
		_requestHandlerFactory.SetGameState(state);
	}
}
