using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Universal;

public interface IUniversalBattlefieldGroup
{
	Vector3 Position { get; }

	Vector2 Dimensions { get; }

	IEnumerable<UniversalBattlefieldStack> VisibleStacks { get; }

	IEnumerable<UniversalBattlefieldStack> AllStacks { get; }

	IEnumerable<CardLayoutData> CardLayoutDatas { get; }

	UniversalBattlefieldGroup.Configuration Config { get; }

	bool IgnoreRegionSpacing { get; }

	bool TryAddCard(DuelScene_CDC cardView, GameManager gameManager, bool isFocusPlayer);

	bool TryAddCard(ICardDataAdapter cardModel, GameManager gameManager, bool isFocusPlayer);

	void Clear(IObjectPool pool);

	void GenerateLayoutDatas(MtgGameState gameState, WorkflowBase workflow, IReadOnlyCollection<CardLayoutData> solvedLayoutDatas, bool collapse, uint expandedStackParentId);

	void ApplyPositionalDelta(Vector3 posDelta);
}
