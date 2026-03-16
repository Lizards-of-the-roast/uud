using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class ActionsAvailableHeuristicHandler : BaseUserRequestHandler<ActionsAvailableRequest>
{
	private readonly DeckHeuristic _heuristic;

	private readonly MtgGameState _gameState;

	private readonly TurnInformation _turnInformation;

	private readonly ICardDatabaseAdapter _cardDatabase;

	public ActionsAvailableHeuristicHandler(ActionsAvailableRequest request, DeckHeuristic heuristic, MtgGameState gameState, TurnInformation turnInformation, ICardDatabaseAdapter cardDatabase)
		: base(request)
	{
		_heuristic = heuristic;
		_gameState = gameState;
		_turnInformation = turnInformation;
		_cardDatabase = cardDatabase;
	}

	public override void HandleRequest()
	{
		Action action = _request.Actions.Find((Action action2) => action2.ActionType == ActionType.Play);
		if (action != null)
		{
			_request.SubmitAction(action);
			return;
		}
		List<Action> list = new List<Action>(_request.Actions);
		if (_request.CanPass)
		{
			foreach (Action item in _turnInformation.ActionsTaken)
			{
				int num = 0;
				while (num < list.Count)
				{
					if (item.PropertiesMatch(list[num]))
					{
						list.RemoveAt(num);
					}
					else
					{
						num++;
					}
				}
			}
		}
		int num2 = 0;
		while (num2 < list.Count)
		{
			if (list[num2].CanAffordToCast())
			{
				num2++;
			}
			else
			{
				list.RemoveAt(num2);
			}
		}
		List<DeckHeuristic.WeightedAction> list2 = new List<DeckHeuristic.WeightedAction>();
		foreach (DeckHeuristic.WeightedAction reaction in _heuristic.GetReactions(_gameState, _cardDatabase))
		{
			if (reaction._action.CanAffordToCast())
			{
				list2.Add(reaction);
			}
		}
		List<DeckHeuristic.WeightedAction> acceptableActions = _heuristic.GetAcceptableActions(list, _gameState, _turnInformation, _cardDatabase);
		if (acceptableActions.Count == 0)
		{
			_request.SubmitPass();
			return;
		}
		DeckHeuristic.WeightedAction highestWeightedAction = GetHighestWeightedAction(acceptableActions);
		if (GetHighestWeightedAction(list2)._weight > highestWeightedAction._weight)
		{
			_request.SubmitPass();
			return;
		}
		_turnInformation.ActionsTaken.Add(highestWeightedAction._action);
		_request.SubmitAction(highestWeightedAction._action);
	}

	private DeckHeuristic.WeightedAction GetHighestWeightedAction(List<DeckHeuristic.WeightedAction> acceptableActions)
	{
		DeckHeuristic.WeightedAction result = default(DeckHeuristic.WeightedAction);
		float num = float.MinValue;
		foreach (DeckHeuristic.WeightedAction acceptableAction in acceptableActions)
		{
			if (acceptableAction._weight > num)
			{
				num = acceptableAction._weight;
				result = acceptableAction;
			}
		}
		return result;
	}
}
