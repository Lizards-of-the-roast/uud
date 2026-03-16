using System;
using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

[Serializable]
[CreateAssetMenu(fileName = "SpecificNamedCardInZone", menuName = "Heuristic/Boardstate/SpecificNamedCardInZone", order = 1)]
public class SpecificNamedCardInZone : BoardstateHeuristic
{
	[SerializeField]
	private string _cardName;

	[SerializeField]
	private Zone _zone;

	[SerializeField]
	private Controller _controller;

	public string CardName => _cardName;

	public Zone Zone => _zone;

	public Controller Controller => _controller;

	public static IReadOnlyList<MtgCardInstance> getLocalZoneVisibleCards(MtgGameState gameState, Zone zone)
	{
		return zone switch
		{
			Zone.Battlefield => gameState.LocalPlayerBattlefieldCards, 
			Zone.Exile => gameState.LocalPlayerExileCards, 
			Zone.Graveyard => gameState.LocalGraveyard.VisibleCards, 
			Zone.Hand => gameState.LocalHand.VisibleCards, 
			_ => null, 
		};
	}

	public static IReadOnlyList<MtgCardInstance> getOpponentZoneVisibleCards(MtgGameState gameState, Zone zone)
	{
		return zone switch
		{
			Zone.Battlefield => gameState.OpponentBattlefieldCards, 
			Zone.Exile => null, 
			Zone.Graveyard => gameState.OpponentGraveyard.VisibleCards, 
			Zone.Hand => gameState.OpponentHand.VisibleCards, 
			_ => null, 
		};
	}

	public override bool IsMet(MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
	{
		foreach (MtgCardInstance item in (Controller == Controller.AI) ? getLocalZoneVisibleCards(gameState, Zone) : getOpponentZoneVisibleCards(gameState, Zone))
		{
			if (cardDatabase.GreLocProvider.GetLocalizedText(item.TitleId).Equals(CardName))
			{
				return true;
			}
		}
		return false;
	}
}
