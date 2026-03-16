using System;
using System.Collections.Generic;

namespace GreClient.Rules;

public class PayCostsRequestNPEStrategyHandler : BaseUserRequestHandler<PayCostsRequest>
{
	private readonly Random _random;

	private readonly MtgGameState _gameState;

	private readonly DeckHeuristic _aiConfig;

	private readonly List<uint> _topPicksForPayingCosts;

	public PayCostsRequestNPEStrategyHandler(PayCostsRequest decision, MtgGameState gameState, DeckHeuristic aiConfig, List<uint> topPicksForPayingCosts, Random random)
		: base(decision)
	{
		_gameState = gameState;
		_aiConfig = aiConfig;
		_topPicksForPayingCosts = topPicksForPayingCosts;
		_random = random ?? new Random();
	}

	public override void HandleRequest()
	{
		EffectCostRequest effectCost = _request.EffectCost;
		if (effectCost != null && effectCost.CostSelection != null)
		{
			SelectNRequest costSelection = _request.EffectCost.CostSelection;
			List<uint> ids = new List<uint>(costSelection.Ids);
			int minPicks = ((costSelection.MinSel != int.MinValue) ? costSelection.MinSel : 0);
			List<uint> selections = NPEStrategyHelpers.PickWell(_gameState, _aiConfig, ids, minPicks, costSelection.MaxSel, _topPicksForPayingCosts, _random);
			costSelection.SubmitSelection(selections);
		}
		else
		{
			new PayCostsRequestRandomHandler(_request, _random)?.HandleRequest();
		}
	}
}
