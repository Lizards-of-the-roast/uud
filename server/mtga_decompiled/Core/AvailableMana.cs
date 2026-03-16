using System;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

[Serializable]
[CreateAssetMenu(fileName = "AvailableMana", menuName = "Heuristic/Boardstate/AvailableMana", order = 4)]
public class AvailableMana : BoardstateHeuristic
{
	[Serializable]
	public enum Color
	{
		ANY,
		White,
		Blue,
		Black,
		Red,
		Green,
		Void,
		Snow
	}

	[SerializeField]
	private uint _minimumAmountAvailable;

	[SerializeField]
	private Color _typeOfMana;

	public uint MinimumAmountAvailable => _minimumAmountAvailable;

	public Color TypeOfMana => _typeOfMana;

	public override bool IsMet(MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
	{
		int num = 0;
		foreach (MtgCardInstance localPlayerBattlefieldCard in gameState.LocalPlayerBattlefieldCards)
		{
			if (localPlayerBattlefieldCard.IsTapped)
			{
				continue;
			}
			foreach (AbilityPrintingData ability in localPlayerBattlefieldCard.Abilities)
			{
				if (ability.SubCategory == AbilitySubCategory.Mana)
				{
					num++;
					if (num >= _minimumAmountAvailable)
					{
						return true;
					}
					break;
				}
			}
		}
		return false;
	}
}
