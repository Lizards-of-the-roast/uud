using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Universal;

public class UniversalBattlefieldGroupSpacer : IUniversalBattlefieldGroup
{
	public Vector3 Position { get; private set; }

	public Vector2 Dimensions { get; private set; }

	public IEnumerable<UniversalBattlefieldStack> VisibleStacks => Array.Empty<UniversalBattlefieldStack>();

	public IEnumerable<UniversalBattlefieldStack> AllStacks => Array.Empty<UniversalBattlefieldStack>();

	public IEnumerable<CardLayoutData> CardLayoutDatas => Array.Empty<CardLayoutData>();

	public UniversalBattlefieldGroup.Configuration Config { get; private set; }

	public bool IgnoreRegionSpacing => true;

	public UniversalBattlefieldGroupSpacer(UniversalBattlefieldGroup.Configuration config)
	{
		Config = config;
	}

	public void ApplyPositionalDelta(Vector3 posDelta)
	{
		Position += posDelta;
	}

	public void Clear(IObjectPool pool)
	{
	}

	public void GenerateLayoutDatas(MtgGameState gameState, WorkflowBase workflow, IReadOnlyCollection<CardLayoutData> solvedLayoutDatas, bool collapse, uint expandedStackParentId)
	{
		Dimensions = Config.Scale;
		Position = CdcVector3.right * Dimensions.x / 2f;
	}

	public bool TryAddCard(DuelScene_CDC cardView, GameManager gameManager, bool isFocusPlayer)
	{
		return false;
	}

	public bool TryAddCard(ICardDataAdapter cardModel, GameManager gameManager, bool isFocusPlayer)
	{
		return false;
	}
}
