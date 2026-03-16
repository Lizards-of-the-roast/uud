using System;
using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

[Serializable]
[CreateAssetMenu(fileName = "NumCardsOfTypeInZone", menuName = "Heuristic/Boardstate/NumCardsOfTypeInZone", order = 2)]
public class NumCardsOfTypeInZone : BoardstateHeuristic
{
	[SerializeField]
	private NumCardsOfTypeInZoneConstraintContainer _numCardsOfTypeInZoneContainer;

	public NumCardsOfTypeInZoneConstraintContainer NumCardsOfTypeInZoneConstraintContainer => _numCardsOfTypeInZoneContainer;

	private bool doComparison(NumCardsOfTypeInZoneConstraintContainer.NumCardsOfTypeInZoneConstraint constraint, int count)
	{
		return constraint.comparison switch
		{
			Comparison.EqualTo => count == constraint.number, 
			Comparison.GreaterThan => count > constraint.number, 
			Comparison.GreaterThanOrEqualTo => count >= constraint.number, 
			Comparison.LessThan => count < constraint.number, 
			Comparison.LessThanOrEqualTo => count <= constraint.number, 
			_ => false, 
		};
	}

	public override bool IsMet(MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
	{
		foreach (NumCardsOfTypeInZoneConstraintContainer.InnerListOfNumCardsOfTypeInZoneConstraints constraint in _numCardsOfTypeInZoneContainer.constraints)
		{
			bool flag = true;
			foreach (NumCardsOfTypeInZoneConstraintContainer.NumCardsOfTypeInZoneConstraint constraint2 in constraint.constraints)
			{
				List<MtgCardInstance> list = new List<MtgCardInstance>();
				switch (constraint2.controller)
				{
				case Controller.AI:
					list.AddRange(SpecificNamedCardInZone.getLocalZoneVisibleCards(gameState, constraint2.zone));
					break;
				case Controller.Opponent:
					list.AddRange(SpecificNamedCardInZone.getOpponentZoneVisibleCards(gameState, constraint2.zone));
					break;
				case Controller.ANY:
					list.AddRange(SpecificNamedCardInZone.getLocalZoneVisibleCards(gameState, constraint2.zone));
					list.AddRange(SpecificNamedCardInZone.getOpponentZoneVisibleCards(gameState, constraint2.zone));
					break;
				}
				List<MtgCardInstance> list2 = new List<MtgCardInstance>();
				foreach (MtgCardInstance item in list)
				{
					if (!item.CardTypes.Contains(constraint2.cardType))
					{
						list2.Add(item);
					}
				}
				foreach (MtgCardInstance item2 in list2)
				{
					list.Remove(item2);
				}
				if (!doComparison(constraint2, list.Count))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}
}
