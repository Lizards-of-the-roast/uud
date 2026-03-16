using System;
using System.Collections.Generic;
using MovementSystem;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

public interface IBattlefieldCardHolder : ICardHolder
{
	bool LayoutLocked { get; set; }

	List<CardLayoutData> PreviousLayoutData { get; }

	Transform Transform { get; }

	event System.Action OnCardHolderUpdated;

	Transform GetRegionTransformForCardType(CardType cardType, GREPlayerNum playerNum);

	IdealPoint GetLayoutEndpoint(CardLayoutData data);

	new IdealPoint GetLayoutEndpoint(DuelScene_CDC cdc);

	IBattlefieldStack GetStackForCard(DuelScene_CDC card);

	IBattlefieldStack GetStackForInstanceId(uint id);

	void UpdateForPhase(Phase phase, Step step);

	bool CardIsStackParent(DuelScene_CDC card);

	void RefreshAbilitiesForCardsStack(DuelScene_CDC card);

	void SetOpponentFocus(params uint[] playerIds)
	{
	}
}
