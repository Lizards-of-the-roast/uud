using System;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

public class NPEStrategyRequestHandlerFactory : RequestHandlerFactory<BaseUserRequest>, IDisposable
{
	private readonly Random _random = new Random();

	private readonly RequestHandlerFactory<BaseUserRequest> _requestHandlerFactory;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly NPE_Game _npeGame;

	private readonly DeckHeuristic _aiconfig;

	private MtgGameState _gameState;

	public NPEStrategyRequestHandlerFactory(RequestHandlerFactory<BaseUserRequest> requestHandlerFactory, ICardDatabaseAdapter cardDatabase, NPE_Game game, DeckHeuristic aiconfig)
	{
		_requestHandlerFactory = requestHandlerFactory;
		_npeGame = game;
		_aiconfig = aiconfig;
		_cardDatabase = cardDatabase;
	}

	public override BaseUserRequestHandler GetHandlerForRequest(BaseUserRequest request)
	{
		if (!(request is ActionsAvailableRequest request2))
		{
			if (!(request is ChooseStartingPlayerRequest request3))
			{
				if (!(request is OptionalActionMessageRequest request4))
				{
					if (!(request is CastingTimeOptionRequest request5))
					{
						if (!(request is SelectNRequest request6))
						{
							if (request is PayCostsRequest decision)
							{
								return new PayCostsRequestNPEStrategyHandler(decision, _gameState, _aiconfig, _npeGame._topPicksForPayingCosts, _random);
							}
							return _requestHandlerFactory.GetHandlerForRequest(request);
						}
						return new SelectNRequestNPEStrategyHandler(request6, _cardDatabase, _npeGame._topPicksChooseActions, _gameState, _aiconfig);
					}
					return new CastingTimeOptionRandomHandler(request5, _random, 4);
				}
				return new OptionalActionMessageRequestNPEStrategyHandler(request4);
			}
			return new ChooseLocalPlayerRequestHandler(request3, _gameState);
		}
		return new ActionsAvailableRequestNPEHandler(request2, _cardDatabase, _npeGame, _gameState, _random);
	}

	public override void SetGameState(MtgGameState state)
	{
		_gameState = state;
		_requestHandlerFactory.SetGameState(state);
	}

	public void Dispose()
	{
		_gameState = null;
		if (_requestHandlerFactory is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}
}
