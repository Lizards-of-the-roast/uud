using System;
using System.Collections.Generic;
using GreClient.Rules;
using WorkflowVisuals;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectTargets;

public class SelectTargetsHighlightsGenerator : IHighlightsGenerator
{
	private static HashSet<ZoneType> _alwaysHotZoneTypes = new HashSet<ZoneType>
	{
		ZoneType.Graveyard,
		ZoneType.Exile
	};

	private readonly Func<int> _getCurrentTargetSelectionIdx;

	private readonly Func<IReadOnlyList<TargetSelection>> _getTargetSelections;

	private readonly IGameStateProvider _gameStateProvider;

	public SelectTargetsHighlightsGenerator(Func<int> getCurrentTargetSelectionIdx, Func<IReadOnlyList<TargetSelection>> getTargetSelections, IGameStateProvider gameStateProvider)
	{
		_getCurrentTargetSelectionIdx = getCurrentTargetSelectionIdx;
		_getTargetSelections = getTargetSelections;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
	}

	public Highlights GetHighlights()
	{
		int num = _getCurrentTargetSelectionIdx?.Invoke() ?? 0;
		IReadOnlyList<TargetSelection> readOnlyList = _getTargetSelections();
		Highlights highlights = new Highlights();
		MtgGameState gameState = _gameStateProvider.LatestGameState;
		for (int i = 0; i < readOnlyList[num].Targets.Count; i++)
		{
			Target target = readOnlyList[num].Targets[i];
			uint targetInstanceId = target.TargetInstanceId;
			switch (target.LegalAction)
			{
			case SelectAction.Unselect:
				highlights.IdToHighlightType_Workflow[targetInstanceId] = HighlightType.Selected;
				break;
			case SelectAction.Select:
				highlights.IdToHighlightType_Workflow[targetInstanceId] = GreHighlightToClientHighlight(i, readOnlyList[num], gameState);
				break;
			}
		}
		for (int j = 0; j < num; j++)
		{
			foreach (Target target2 in readOnlyList[j].Targets)
			{
				if (target2.LegalAction == SelectAction.Unselect)
				{
					uint targetInstanceId2 = target2.TargetInstanceId;
					highlights.IdToHighlightType_Workflow[targetInstanceId2] = HighlightType.Selected;
				}
			}
		}
		return highlights;
	}

	public static HighlightType GreHighlightToClientHighlight(int targetIndex, TargetSelection targetSelection, MtgGameState gameState)
	{
		if (targetSelection.Targets.Count == 0)
		{
			return HighlightType.None;
		}
		Target target = targetSelection.Targets[targetIndex];
		switch ((gameState != null) ? HighlightTypeForTarget(targetIndex, targetSelection, gameState) : target.Highlight)
		{
		case Wotc.Mtgo.Gre.External.Messaging.HighlightType.Tepid:
		case Wotc.Mtgo.Gre.External.Messaging.HighlightType.Hot:
			return HighlightType.Hot;
		case Wotc.Mtgo.Gre.External.Messaging.HighlightType.Cold:
		case Wotc.Mtgo.Gre.External.Messaging.HighlightType.ReplaceRole:
			return HighlightType.Cold;
		default:
			return HighlightType.None;
		}
	}

	private static Wotc.Mtgo.Gre.External.Messaging.HighlightType HighlightTypeForTarget(int targetIndex, TargetSelection targetSelection, MtgGameState gameState)
	{
		if (targetSelection.Targets.Count == 0)
		{
			return Wotc.Mtgo.Gre.External.Messaging.HighlightType.None;
		}
		Target target = targetSelection.Targets[targetIndex];
		if (target != null && gameState.TryGetCard(target.TargetInstanceId, out var card) && card.Zone != null && _alwaysHotZoneTypes.Contains(card.Zone.Type) && targetSelection.TargetingAbilityGrpId != 190943)
		{
			return Wotc.Mtgo.Gre.External.Messaging.HighlightType.Hot;
		}
		return targetSelection.Targets[targetIndex].Highlight;
	}
}
