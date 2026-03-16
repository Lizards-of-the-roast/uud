using System;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene;

public class SelectFromGroupsHeuristicHandler : BaseUserRequestHandler<SelectFromGroupsRequest>
{
	private readonly DeckHeuristic _heuristic;

	private readonly MtgGameState _gameState;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly Random _rng;

	public SelectFromGroupsHeuristicHandler(SelectFromGroupsRequest request, DeckHeuristic heuristic, MtgGameState gameState, ICardDatabaseAdapter cardDatabase, Random rng)
		: base(request)
	{
		_heuristic = heuristic;
		_gameState = gameState;
		_cardDatabase = cardDatabase;
		_rng = rng;
	}

	public override void HandleRequest()
	{
		GroupNode groupNode = new GroupNode(_request);
		int num = 0;
		while (!groupNode.CanSubmit)
		{
			uint id;
			if (HasHeuristicForTopCardOnStack())
			{
				id = groupNode.SelectableIds[num];
				num++;
			}
			else
			{
				num = _rng.Next(0, groupNode.SelectableIds.Count);
				id = groupNode.SelectableIds[num];
			}
			groupNode.Branch(id);
		}
		_request.Submit(groupNode.Submit());
	}

	private bool HasHeuristicForTopCardOnStack()
	{
		MtgCardInstance topCardOnStack = _gameState.GetTopCardOnStack();
		if (topCardOnStack == null)
		{
			return false;
		}
		uint id = ((topCardOnStack.ObjectSourceGrpId != 0) ? topCardOnStack.ObjectSourceGrpId : topCardOnStack.BaseGrpId);
		if (_cardDatabase.CardDataProvider.TryGetCardPrintingById(id, out var card))
		{
			return _heuristic.CardHeuristicExists(card.TitleId, _cardDatabase);
		}
		return false;
	}
}
