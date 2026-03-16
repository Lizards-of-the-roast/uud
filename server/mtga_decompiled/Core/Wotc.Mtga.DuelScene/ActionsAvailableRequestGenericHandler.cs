using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class ActionsAvailableRequestGenericHandler : BaseUserRequestHandler<ActionsAvailableRequest>
{
	private readonly TurnInformation _turnInformation;

	private readonly ICardDatabaseAdapter _cardDatabase;

	public ActionsAvailableRequestGenericHandler(ActionsAvailableRequest request, TurnInformation turnInformation, ICardDatabaseAdapter cardDatabase)
		: base(request)
	{
		_turnInformation = turnInformation;
		_cardDatabase = cardDatabase;
	}

	public override void HandleRequest()
	{
		Action action = _request.Actions.Find((Action action5) => action5.ActionType == ActionType.Play);
		if (action != null)
		{
			_request.SubmitAction(action);
			return;
		}
		List<Action> affordableActions = GetAffordableActions(_request.Actions);
		if (_request.CanPass)
		{
			foreach (Action item in _turnInformation.ActionsTaken)
			{
				int num = 0;
				while (num < affordableActions.Count)
				{
					if (item.PropertiesMatch(affordableActions[num]))
					{
						affordableActions.RemoveAt(num);
					}
					else
					{
						num++;
					}
				}
			}
		}
		if (affordableActions.Count > 0)
		{
			if (_turnInformation.numberOfCreaturesCasted == 0)
			{
				List<Action> castableCreatureActions = GetCastableCreatureActions(affordableActions);
				if (castableCreatureActions.Count > 0)
				{
					Action action2 = castableCreatureActions[0];
					for (int num2 = 1; num2 < castableCreatureActions.Count; num2++)
					{
						if (GetConvertedManaCostOfCastAction(castableCreatureActions[num2]) < GetConvertedManaCostOfCastAction(action2))
						{
							action2 = castableCreatureActions[num2];
						}
					}
					_request.SubmitAction(action2);
					_turnInformation.numberOfCreaturesCasted++;
					_turnInformation.ActionsTaken.Add(action2);
					return;
				}
			}
			if (_turnInformation.numberOfSorceryCasted == 0)
			{
				List<Action> castableSorceryActions = GetCastableSorceryActions(affordableActions);
				if (castableSorceryActions.Count > 0)
				{
					Action action3 = castableSorceryActions[0];
					for (int num3 = 1; num3 < castableSorceryActions.Count; num3++)
					{
						if (GetConvertedManaCostOfCastAction(castableSorceryActions[num3]) < GetConvertedManaCostOfCastAction(action3))
						{
							action3 = castableSorceryActions[num3];
						}
					}
					_request.SubmitAction(action3);
					_turnInformation.numberOfSorceryCasted++;
					_turnInformation.ActionsTaken.Add(action3);
					return;
				}
			}
			if (_turnInformation.numberOfInstantCasted == 0 && (_turnInformation.phase == Phase.Combat || (_turnInformation.phase == Phase.Ending && _turnInformation.step == Step.End)))
			{
				List<Action> castableInstantActions = GetCastableInstantActions(affordableActions);
				if (castableInstantActions.Count > 0)
				{
					Action action4 = castableInstantActions[0];
					for (int num4 = 1; num4 < castableInstantActions.Count; num4++)
					{
						if (GetConvertedManaCostOfCastAction(castableInstantActions[num4]) < GetConvertedManaCostOfCastAction(action4))
						{
							action4 = castableInstantActions[num4];
						}
					}
					_request.SubmitAction(action4);
					_turnInformation.numberOfInstantCasted++;
					_turnInformation.ActionsTaken.Add(action4);
					return;
				}
			}
		}
		_request.SubmitPass();
	}

	private List<Action> GetAffordableActions(List<Action> availableActions)
	{
		List<Action> list = new List<Action>();
		foreach (Action availableAction in availableActions)
		{
			if (availableAction.CanAffordToCast())
			{
				list.Add(availableAction);
			}
		}
		return list;
	}

	private List<Action> GetCastableCreatureActions(List<Action> availableActions)
	{
		List<Action> list = new List<Action>();
		foreach (Action availableAction in availableActions)
		{
			if (availableAction.ActionType == ActionType.Cast)
			{
				CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(availableAction.GrpId);
				if (cardPrintingById != null && cardPrintingById.Types.Contains(CardType.Creature))
				{
					list.Add(availableAction);
				}
			}
		}
		return list;
	}

	private List<Action> GetCastableSorceryActions(List<Action> availableActions)
	{
		List<Action> list = new List<Action>();
		foreach (Action availableAction in availableActions)
		{
			if (availableAction.ActionType == ActionType.Cast)
			{
				CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(availableAction.GrpId);
				if (cardPrintingById != null && cardPrintingById.Types.Contains(CardType.Sorcery))
				{
					list.Add(availableAction);
				}
			}
		}
		return list;
	}

	private List<Action> GetCastableInstantActions(List<Action> availableActions)
	{
		List<Action> list = new List<Action>();
		foreach (Action availableAction in availableActions)
		{
			if (availableAction.ActionType == ActionType.Cast)
			{
				CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(availableAction.GrpId);
				if (cardPrintingById != null && cardPrintingById.Types.Contains(CardType.Instant))
				{
					list.Add(availableAction);
				}
			}
		}
		return list;
	}

	private int GetConvertedManaCostOfCastAction(Action action)
	{
		if (action.ActionType != ActionType.Cast)
		{
			return int.MaxValue;
		}
		int num = 0;
		foreach (ManaRequirement item in action.ManaCost)
		{
			num += item.Count;
		}
		return num;
	}
}
