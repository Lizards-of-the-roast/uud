using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class PayCostHeuristicHandler : BaseUserRequestHandler<PayCostsRequest>
{
	private readonly MtgGameState _gameState;

	private readonly Random _rng;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly BaseUserRequestHandler<PayCostsRequest> _defaultHandler;

	public PayCostHeuristicHandler(PayCostsRequest request, MtgGameState gameState, Random rng, ICardDatabaseAdapter cardDatabase, BaseUserRequestHandler<PayCostsRequest> defaultHandler)
		: base(request)
	{
		_gameState = gameState;
		_rng = rng;
		_cardDatabase = cardDatabase;
		_defaultHandler = defaultHandler;
	}

	public override void HandleRequest()
	{
		if (_request.PaymentSelection != null && _request.PaymentSelection.Ids.Count > 0)
		{
			_defaultHandler.HandleRequest();
			return;
		}
		if (_request.PaymentActions != null && _request.PaymentActions.Actions.Count > 0)
		{
			_defaultHandler.HandleRequest();
			return;
		}
		EffectCostRequest effectCost = _request.EffectCost;
		if (effectCost != null && effectCost.EffectCostType == EffectCostType.Select)
		{
			SelectNRequest costSelection = _request.EffectCost.CostSelection;
			if (_request.EffectCost.CostSelection.Weights.Count == 0)
			{
				_defaultHandler.HandleRequest();
				return;
			}
			List<uint> list = new List<uint>(costSelection.Ids);
			List<uint> list2 = new List<uint>();
			Dictionary<uint, int> dictionary = new Dictionary<uint, int>();
			for (int i = 0; i < costSelection.Weights.Count; i++)
			{
				dictionary[costSelection.Ids[i]] = costSelection.Weights[i];
			}
			int num = 0;
			while (list.Count > 0)
			{
				List<uint> leastConvertedManaCosts = GetLeastConvertedManaCosts(list);
				int index = _rng.Next(0, leastConvertedManaCosts.Count);
				uint num2 = leastConvertedManaCosts[index];
				list2.Add(num2);
				list.RemoveAt(index);
				num += dictionary[num2];
				if (num >= costSelection.MinSel)
				{
					break;
				}
			}
			costSelection.SubmitSelection(list2);
		}
		else if (_request.CanCancel)
		{
			_request.Cancel();
		}
		else
		{
			_request.AutoRespond();
		}
	}

	private int GetConvertedManaCost(uint cardInstanceId)
	{
		return ((int?)_cardDatabase.CardDataProvider.GetCardPrintingById(_gameState.GetCardById(cardInstanceId).GrpId)?.ConvertedManaCost) ?? (-1);
	}

	private List<uint> GetLeastConvertedManaCosts(List<uint> cardInstanceIds)
	{
		List<uint> list = new List<uint>();
		foreach (uint cardInstanceId in cardInstanceIds)
		{
			if (list.Count == 0)
			{
				list.Add(cardInstanceId);
			}
			else if (GetConvertedManaCost(cardInstanceId) < GetConvertedManaCost(list[0]))
			{
				list.Clear();
				list.Add(cardInstanceId);
			}
			else if (GetConvertedManaCost(cardInstanceId) == GetConvertedManaCost(list[0]))
			{
				list.Add(cardInstanceId);
			}
		}
		return list;
	}
}
