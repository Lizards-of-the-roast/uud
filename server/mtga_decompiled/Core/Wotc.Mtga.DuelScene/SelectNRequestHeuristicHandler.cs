using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class SelectNRequestHeuristicHandler : BaseUserRequestHandler<SelectNRequest>
{
	private readonly DeckHeuristic _heuristic;

	private readonly MtgGameState _gameState;

	private readonly Random _rng;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly BaseUserRequestHandler<SelectNRequest> _fallback;

	public SelectNRequestHeuristicHandler(SelectNRequest request, DeckHeuristic heuristic, MtgGameState gameState, ICardDatabaseAdapter cardDatabase, Random rng, BaseUserRequestHandler<SelectNRequest> fallback)
		: base(request)
	{
		_heuristic = heuristic;
		_gameState = gameState;
		_cardDatabase = cardDatabase;
		_rng = rng;
		_fallback = fallback;
	}

	public override void HandleRequest()
	{
		if (_request.StaticList == StaticList.Colors && ShouldUseHeuristicOptions(out var result))
		{
			List<uint> list = new List<uint>();
			foreach (AcceptableChoiceContainer.InnerListOfCriteria item in result)
			{
				foreach (AcceptableChoiceContainer.AcceptableChoice item2 in item.criteria.FindAll((AcceptableChoiceContainer.AcceptableChoice acceptableTarget) => acceptableTarget.targetFilter == TargetFilter.Color))
				{
					if (item2.filterParameter.Contains("White", caseIndependent: true))
					{
						list.Add(1u);
						continue;
					}
					if (item2.filterParameter.Contains("Blue", caseIndependent: true))
					{
						list.Add(2u);
						continue;
					}
					if (item2.filterParameter.Contains("Black", caseIndependent: true))
					{
						list.Add(3u);
						continue;
					}
					if (item2.filterParameter.Contains("Red", caseIndependent: true))
					{
						list.Add(4u);
						continue;
					}
					if (item2.filterParameter.Contains("Green", caseIndependent: true))
					{
						list.Add(5u);
						continue;
					}
					list = new List<uint> { 1u, 2u, 3u, 4u, 5u };
				}
			}
			if (list.Count > 0)
			{
				List<uint> list2 = new List<uint>();
				int num = _rng.Next(_request.MinSel, (int)(_request.MaxSel + 1));
				while (list2.Count < num)
				{
					list2.Add(list[_rng.Next(0, list.Count)]);
				}
				_request.SubmitSelection(list2);
			}
			else
			{
				_fallback.HandleRequest();
			}
		}
		else
		{
			_fallback.HandleRequest();
		}
	}

	private bool ShouldUseHeuristicOptions(out IReadOnlyList<AcceptableChoiceContainer.InnerListOfCriteria> result)
	{
		result = Array.Empty<AcceptableChoiceContainer.InnerListOfCriteria>();
		MtgCardInstance topCardOnStack = _gameState.GetTopCardOnStack();
		if (topCardOnStack == null)
		{
			return false;
		}
		uint id = ((topCardOnStack.ObjectSourceGrpId != 0) ? topCardOnStack.ObjectSourceGrpId : topCardOnStack.BaseGrpId);
		if (!_cardDatabase.CardDataProvider.TryGetCardPrintingById(id, out var card))
		{
			return false;
		}
		if (!_heuristic.CardHeuristicExists(card.TitleId, _cardDatabase))
		{
			return false;
		}
		result = _heuristic.GetCardHeuristicAcceptableChoices(card.TitleId, _cardDatabase);
		return result.Count > 0;
	}
}
