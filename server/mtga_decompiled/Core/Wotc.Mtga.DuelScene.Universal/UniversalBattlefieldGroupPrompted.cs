using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Universal;

public class UniversalBattlefieldGroupPrompted : IUniversalBattlefieldGroup
{
	public Vector3 Position { get; private set; }

	public Vector2 Dimensions { get; private set; }

	public IEnumerable<UniversalBattlefieldStack> VisibleStacks => Array.Empty<UniversalBattlefieldStack>();

	public IEnumerable<UniversalBattlefieldStack> AllStacks => Array.Empty<UniversalBattlefieldStack>();

	public IEnumerable<CardLayoutData> CardLayoutDatas => Array.Empty<CardLayoutData>();

	public UniversalBattlefieldGroup.Configuration Config { get; private set; }

	public bool IgnoreRegionSpacing => true;

	public UniversalBattlefieldGroupPrompted(UniversalBattlefieldGroup.Configuration config)
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
		int? num = workflow?.Buttons?.WorkflowButtons?.Count;
		Vector2 dimensions;
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			if (valueOrDefault > 1 || (valueOrDefault == 1 && workflow?.Buttons?.CancelData != null))
			{
				dimensions = Config.Scale;
				goto IL_008d;
			}
		}
		dimensions = Vector2.zero;
		goto IL_008d;
		IL_008d:
		Dimensions = dimensions;
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
