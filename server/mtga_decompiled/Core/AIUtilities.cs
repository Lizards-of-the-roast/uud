using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;

public class AIUtilities
{
	public enum AbilityIds
	{
		Deathtouch = 1,
		DoubleStrike = 3,
		FirstStrike = 6,
		Flash = 7,
		Flying = 8,
		Hexproof = 10,
		Lifelink = 12,
		Reach = 13,
		Trample = 14,
		Vigilance = 15,
		Menace = 142,
		TapAddG = 1005,
		EtbRaiseDead = 1157,
		SpellgorgerWeird = 1208,
		DiesDrawACard = 17519,
		Unblockable = 62969,
		EtbDraw = 86788,
		AddCostDiscard = 87929,
		EtbGain4Life = 88604,
		EtBDeal1Dmg = 92894,
		EtBCreateTwoGoblins = 96140,
		DiesReturn = 98465,
		DiesMinusOne = 102036,
		SaniSkeleton = 102887,
		OtherVampiresYouControlGetPlus1Plus1 = 117100,
		OtherCreaturesYouControlGetPlus1Plus0 = 118813,
		EtbBounce = 119222,
		ImpassionedOrator = 122104,
		BrinebornCutthroat = 133509,
		PackMastiff = 133545
	}

	private static Dictionary<uint, int> WeightedAbilities = new Dictionary<uint, int>
	{
		{ 8u, 1 },
		{ 6u, 1 },
		{ 7u, 0 },
		{ 1u, 2 },
		{ 3u, 2 },
		{ 12u, 2 },
		{ 10u, 2 },
		{ 14u, 1 },
		{ 15u, 2 },
		{ 1005u, 2 },
		{ 1157u, 0 },
		{ 118813u, 2 },
		{ 1208u, 1 },
		{ 86788u, 0 },
		{ 87929u, 0 },
		{ 88604u, 0 },
		{ 92894u, 0 },
		{ 96140u, 0 },
		{ 98465u, 0 },
		{ 102036u, 0 },
		{ 102887u, 0 },
		{ 119222u, 0 },
		{ 122104u, 1 }
	};

	public static bool HasDeathtouch(MtgCardInstance card)
	{
		return card.Abilities.Exists((AbilityPrintingData x) => x.Id == 1);
	}

	public static bool HasLifelink(MtgCardInstance card)
	{
		return card.Abilities.Exists((AbilityPrintingData x) => x.Id == 12);
	}

	public static bool HasFlying(MtgCardInstance card)
	{
		return card.Abilities.Exists((AbilityPrintingData x) => x.Id == 8);
	}

	public static bool HasUnblockable(MtgCardInstance card)
	{
		return card.Abilities.Exists((AbilityPrintingData x) => x.Id == 62969);
	}

	public static bool HasReach(MtgCardInstance card)
	{
		return card.Abilities.Exists((AbilityPrintingData x) => x.Id == 13);
	}

	public static bool HasMenace(MtgCardInstance card)
	{
		return card.Abilities.Exists((AbilityPrintingData x) => x.Id == 142);
	}

	public static int GetAbilityScoreScaledToPowerToughness(AbilityPrintingData ability, MtgCardInstance cardObj)
	{
		int num = Math.Max(0, cardObj.Power.Value);
		int num2 = Math.Max(0, cardObj.Toughness.Value);
		int value = 0;
		if (WeightedAbilities.TryGetValue(ability.Id, out value))
		{
			if (ability.Id == 1)
			{
				return value;
			}
			return value * (num + num2) / 2;
		}
		return value;
	}
}
