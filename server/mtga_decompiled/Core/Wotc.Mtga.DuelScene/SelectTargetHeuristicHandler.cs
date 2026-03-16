using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions.SelectTargets;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class SelectTargetHeuristicHandler : BaseUserRequestHandler<SelectTargetsRequest>
{
	private readonly DeckHeuristic _heuristic;

	private readonly MtgGameState _gameState;

	private readonly SelectTargetsState _targetState;

	private readonly TurnInformation _turnInformation;

	private readonly Random _rng;

	private readonly ICardDatabaseAdapter _cardDatabase;

	public SelectTargetHeuristicHandler(SelectTargetsRequest request, DeckHeuristic heuristic, MtgGameState gameState, SelectTargetsState targetsState, TurnInformation turnInformation, Random rng, ICardDatabaseAdapter cardDatabase)
		: base(request)
	{
		_heuristic = heuristic;
		_gameState = gameState;
		_targetState = targetsState;
		_turnInformation = turnInformation;
		_rng = rng;
		_cardDatabase = cardDatabase;
	}

	public override void HandleRequest()
	{
		if (_turnInformation.IsOverMaxRequestCounter(_request))
		{
			_targetState.TargetSelectIdx = 0;
			_request.AutoRespond();
			return;
		}
		int i;
		for (i = _targetState.TargetSelectIdx; TargetSubmission.DiaochanAutoAdvanceIndex(i, _request.TargetSelections, _gameState); i++)
		{
		}
		HandleSelectTargetRequest(_request, i);
	}

	private void HandleSelectTargetRequest(SelectTargetsRequest selectTargets, int selectionIdx)
	{
		_targetState.TargetSelectIdx = selectionIdx;
		TargetSelection targetSelection = selectTargets.TargetSelections[selectionIdx];
		if (targetSelection.IsAtCapacity() && targetSelection.CanSubmit())
		{
			if (TargetSubmission.SubmitAtThisIndex(selectionIdx, _request.TargetSelections, _gameState))
			{
				_targetState.TargetSelectIdx = 0;
				_turnInformation.ClearRequest();
				selectTargets.SubmitTargets();
			}
			else
			{
				HandleSelectTargetRequest(selectTargets, selectionIdx + 1);
			}
			return;
		}
		int index = (int)(_gameState.Stack.TotalCardCount - 1);
		uint id = ((_gameState.Stack.VisibleCards[index].ObjectSourceGrpId != 0) ? _gameState.Stack.VisibleCards[index].ObjectSourceGrpId : _gameState.Stack.VisibleCards[index].BaseGrpId);
		uint titleId = _cardDatabase.CardDataProvider.GetCardPrintingById(id).TitleId;
		CardHeuristic chosenHeuristic;
		List<Target> acceptableTargets = _heuristic.GetAcceptableTargets(titleId, targetSelection, _gameState, _cardDatabase, out chosenHeuristic);
		if (_rng.Next(targetSelection.CanSubmit() ? (-1) : 0, acceptableTargets.Count) != -1)
		{
			Target heuristicTarget = GetHeuristicTarget(chosenHeuristic, acceptableTargets);
			selectTargets.UpdateTarget(heuristicTarget, targetSelection.TargetIdx);
		}
		else if (TargetSubmission.SubmitAtThisIndex(selectionIdx, _request.TargetSelections, _gameState))
		{
			_turnInformation.ClearRequest();
			_targetState.TargetSelectIdx = 0;
			selectTargets.SubmitTargets();
		}
		else
		{
			HandleSelectTargetRequest(selectTargets, selectionIdx + 1);
		}
	}

	private Target GetHeuristicTarget(CardHeuristic heuristic, List<Target> selectableTargets)
	{
		if (selectableTargets == null || selectableTargets.Count == 0)
		{
			return null;
		}
		if (heuristic == null)
		{
			return selectableTargets[_rng.Next(0, selectableTargets.Count)];
		}
		return heuristic.Archetype switch
		{
			Archetype.Removal => GetBestRemovalTarget(selectableTargets), 
			Archetype.CombatTrick => GetBestCombatTrickTarget(selectableTargets), 
			_ => selectableTargets[_rng.Next(0, selectableTargets.Count)], 
		};
	}

	private Target GetBestRemovalTarget(List<Target> selectableTargets)
	{
		if (selectableTargets.Count == 0)
		{
			return null;
		}
		List<Target> list = new List<Target>();
		if (selectableTargets.Count > 1)
		{
			foreach (Target selectableTarget in selectableTargets)
			{
				if (_gameState.TryGetEntity(selectableTarget.TargetInstanceId, out var mtgEntity) && mtgEntity is MtgPlayer)
				{
					list.Add(selectableTarget);
				}
			}
			selectableTargets.RemoveAll(list.Contains);
		}
		if (selectableTargets.Count == 0)
		{
			return null;
		}
		List<Target> list2 = new List<Target> { selectableTargets[0] };
		for (int i = 1; i < selectableTargets.Count; i++)
		{
			if (_gameState.GetCardById(selectableTargets[i].TargetInstanceId).Power.Value >= _gameState.GetCardById(list2[0].TargetInstanceId).Power.Value)
			{
				if (_gameState.GetCardById(selectableTargets[i].TargetInstanceId).Power.Value > _gameState.GetCardById(list2[0].TargetInstanceId).Power.Value)
				{
					list2.Clear();
				}
				list2.Add(selectableTargets[i]);
			}
		}
		List<Target> list3 = new List<Target> { list2[0] };
		for (int j = 1; j < list2.Count; j++)
		{
			if (_gameState.GetCardById(list2[j].TargetInstanceId).Toughness.Value >= _gameState.GetCardById(list3[0].TargetInstanceId).Toughness.Value)
			{
				if (_gameState.GetCardById(list2[j].TargetInstanceId).Toughness.Value > _gameState.GetCardById(list3[0].TargetInstanceId).Toughness.Value)
				{
					list3.Clear();
				}
				list3.Add(list2[j]);
			}
		}
		return list3[_rng.Next(0, list3.Count)];
	}

	private Target GetBestCombatTrickTarget(List<Target> selectableTargets)
	{
		if (selectableTargets.Count == 0)
		{
			return null;
		}
		bool flag = false;
		List<Target> list = new List<Target>();
		foreach (Target selectableTarget in selectableTargets)
		{
			MtgCardInstance cardById = _gameState.GetCardById(selectableTarget.TargetInstanceId);
			if (cardById.BlockingIds.Count == 0 || cardById.BlockedByIds.Count == 0)
			{
				if (flag)
				{
					list.Clear();
				}
				list.Add(selectableTarget);
				flag = true;
			}
			else
			{
				if (flag)
				{
					continue;
				}
				if (list.Count == 0)
				{
					list.Add(selectableTarget);
					continue;
				}
				int value = _gameState.GetCardById(list[0].TargetInstanceId).Power.Value;
				if (cardById.Power.Value >= value)
				{
					if (cardById.Power.Value > value)
					{
						list.Clear();
					}
					list.Add(selectableTarget);
				}
			}
		}
		List<Target> list2 = new List<Target>();
		foreach (Target item in list)
		{
			if (list2.Count == 0)
			{
				list2.Add(item);
			}
			MtgCardInstance cardById2 = _gameState.GetCardById(item.TargetInstanceId);
			if (flag)
			{
				int value2 = _gameState.GetCardById(list2[0].TargetInstanceId).Power.Value;
				if (cardById2.Power.Value >= value2)
				{
					if (cardById2.Power.Value > value2)
					{
						list2.Clear();
					}
					list2.Add(item);
				}
				list2.Add(item);
				continue;
			}
			int value3 = _gameState.GetCardById(list2[0].TargetInstanceId).Toughness.Value;
			if (cardById2.Toughness.Value >= value3)
			{
				if (cardById2.Toughness.Value > value3)
				{
					list2.Clear();
				}
				list2.Add(item);
			}
			list2.Add(item);
		}
		return list2[_rng.Next(0, list2.Count)];
	}
}
