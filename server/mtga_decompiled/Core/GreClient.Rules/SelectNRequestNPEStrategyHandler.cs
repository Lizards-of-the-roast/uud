using System;
using System.Collections.Generic;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.Rules;

public class SelectNRequestNPEStrategyHandler : BaseUserRequestHandler<SelectNRequest>
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly Random _random;

	private readonly List<uint> _topPicksChooseActions;

	private readonly MtgGameState _gameState;

	private readonly DeckHeuristic _aiConfig;

	public SelectNRequestNPEStrategyHandler(SelectNRequest request, ICardDatabaseAdapter cardDatabase, List<uint> topPicksChooseActions, MtgGameState gameState, DeckHeuristic aiConfig)
		: base(request)
	{
		_cardDatabase = cardDatabase;
		_topPicksChooseActions = topPicksChooseActions;
		_random = new Random();
		_gameState = gameState;
		_aiConfig = aiConfig;
	}

	public override void HandleRequest()
	{
		SelectNRequestRandomHandler selectNRequestRandomHandler = new SelectNRequestRandomHandler(_request, _random, _cardDatabase);
		if (_request.OptionContext == OptionContext.Stacking)
		{
			selectNRequestRandomHandler.HandleRequest();
			return;
		}
		if (_request.ListType == SelectionListType.Static && _request.StaticList == StaticList.Colors)
		{
			selectNRequestRandomHandler.HandleRequest();
			return;
		}
		if (_request.ListType == SelectionListType.Static && _request.StaticList == StaticList.WishCards)
		{
			selectNRequestRandomHandler.HandleRequest();
			return;
		}
		if (_request.Weights.Count > 0)
		{
			selectNRequestRandomHandler.HandleRequest();
			return;
		}
		List<uint> ids = new List<uint>(_request.Ids);
		List<uint> selections = NPEStrategyHelpers.PickWell(_gameState, _aiConfig, ids, _request.MinSel, _request.MaxSel, _topPicksChooseActions, _random);
		_request.SubmitSelection(selections);
	}
}
