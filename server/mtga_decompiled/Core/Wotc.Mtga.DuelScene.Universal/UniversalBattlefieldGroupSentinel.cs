using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Universal;

public class UniversalBattlefieldGroupSentinel : IUniversalBattlefieldGroup
{
	public Vector3 Position => Vector3.zero;

	public Vector2 Dimensions => Vector2.zero;

	public IEnumerable<UniversalBattlefieldStack> VisibleStacks => Array.Empty<UniversalBattlefieldStack>();

	public IEnumerable<UniversalBattlefieldStack> AllStacks => Array.Empty<UniversalBattlefieldStack>();

	public IEnumerable<CardLayoutData> CardLayoutDatas => Array.Empty<CardLayoutData>();

	public UniversalBattlefieldGroup.Configuration Config => null;

	public bool IgnoreRegionSpacing => true;

	public void GenerateLayoutDatas(MtgGameState gameState, WorkflowBase workflow, IReadOnlyCollection<CardLayoutData> solvedLayoutDatas, bool collapse, uint expandedStackParentId)
	{
	}

	public void ApplyPositionalDelta(Vector3 posDelta)
	{
	}

	public bool TryAddCard(DuelScene_CDC cardView, GameManager gameManager, bool isFocusPlayer)
	{
		return false;
	}

	public bool TryAddCard(ICardDataAdapter cardModel, GameManager gameManager, bool isFocusPlayer)
	{
		return false;
	}

	public void Clear(IObjectPool pool)
	{
	}
}
