using System.Collections.Generic;
using System.Linq;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.Rules;

public class SelectTargetsRequestNPEHandler : BaseUserRequestHandler<SelectTargetsRequest>
{
	private class PreferredTargetsComparer : IComparer<(Target target, MtgEntity entity)>
	{
		private List<uint> _preferenceList;

		private DeckHeuristic _deckHeuristic;

		private NPE_Game _npeGame;

		public void SetParams(List<uint> preferenceList, DeckHeuristic deckHeuristic, NPE_Game npeGame)
		{
			_preferenceList = preferenceList;
			_deckHeuristic = deckHeuristic;
			_npeGame = npeGame;
		}

		public void ClearParams()
		{
			_preferenceList = null;
			_deckHeuristic = null;
			_npeGame = null;
		}

		public int Compare((Target target, MtgEntity entity) x, (Target target, MtgEntity entity) y)
		{
			if (_preferenceList == null || _deckHeuristic == null || _npeGame == null)
			{
				return 0;
			}
			return GetScoreForEntity(y.entity).CompareTo(GetScoreForEntity(x.entity));
		}

		private float GetScoreForEntity(MtgEntity entity)
		{
			if (!(entity is MtgPlayer))
			{
				if (entity is MtgCardInstance mtgCardInstance)
				{
					int num = _preferenceList.IndexOf(mtgCardInstance.GrpId);
					if (num >= 0)
					{
						return (float)((_preferenceList.Count - num) * 10000) + _deckHeuristic.ScoreCard(mtgCardInstance, _npeGame.InjectedNpeDirector);
					}
					return _deckHeuristic.ScoreCard(mtgCardInstance);
				}
				return 1f;
			}
			return 1f;
		}
	}

	private readonly NPE_Game _npeGame;

	private readonly MtgGameState _gameState;

	private readonly DeckHeuristic _aiConfig;

	private int _targetIdx;

	private readonly List<(Target target, MtgEntity entity)> _preferredTargets = new List<(Target, MtgEntity)>();

	private readonly PreferredTargetsComparer _preferredTargetsComparer = new PreferredTargetsComparer();

	public SelectTargetsRequestNPEHandler(SelectTargetsRequest request, NPE_Game npeGame, MtgGameState gameState, DeckHeuristic aiConfig, int targetIdx)
		: base(request)
	{
		_npeGame = npeGame;
		_gameState = gameState;
		_aiConfig = aiConfig;
		_targetIdx = targetIdx;
	}

	public override void HandleRequest()
	{
		HandleRequest(_targetIdx);
	}

	private void HandleRequest(int targetIdx)
	{
		_targetIdx = targetIdx;
		TargetSelection targetSelection = _request.TargetSelections[targetIdx];
		MtgCardInstance card;
		Target target;
		if (targetSelection.IsAtCapacity() && targetSelection.CanSubmit())
		{
			if (targetIdx == _request.TargetSelections.Count - 1)
			{
				_request.SubmitTargets();
			}
			else
			{
				HandleRequest(targetIdx + 1);
			}
		}
		else if (_gameState.TryGetCard(_gameState.Stack.CardIds.First(), out card) && TryGetTarget(card.GrpId, targetSelection, out target))
		{
			_request.UpdateTarget(target, targetSelection.TargetIdx);
		}
	}

	private bool TryGetTarget(uint grpId, TargetSelection targetSelection, out Target target)
	{
		if (TryGetPreferredTarget(grpId, targetSelection, out target))
		{
			return target != null;
		}
		foreach (Target target2 in targetSelection.Targets)
		{
			if (target2.LegalAction == SelectAction.Select)
			{
				target = target2;
				return true;
			}
		}
		target = null;
		return false;
	}

	private bool TryGetPreferredTarget(uint grpId, TargetSelection targetSelection, out Target preferredTarget)
	{
		_preferredTargets.Clear();
		_preferredTargetsComparer.ClearParams();
		if (_npeGame._targetPreferences.TryGetValue(grpId, out var value))
		{
			foreach (Target target in targetSelection.Targets)
			{
				if (_gameState.TryGetEntity(target.TargetInstanceId, out var mtgEntity) && value.IsPreferredEntity(mtgEntity))
				{
					_preferredTargets.Add((target, mtgEntity));
				}
			}
			_preferredTargetsComparer.SetParams(value.PreferredTargets, _aiConfig, _npeGame);
			_preferredTargets.Sort(_preferredTargetsComparer);
			preferredTarget = ((_preferredTargets.Count > 0) ? _preferredTargets[0].target : null);
			_preferredTargets.Clear();
			_preferredTargetsComparer.ClearParams();
			return preferredTarget != null;
		}
		preferredTarget = null;
		return false;
	}
}
