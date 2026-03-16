using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class HangerUtilities
{
	public static bool ShowLinkedFaceAbilityHangers(ICardDataAdapter cardData)
	{
		switch (cardData.LinkedFaceType)
		{
		case LinkedFace.DfcFront:
		case LinkedFace.DfcBack:
			if (cardData != null && cardData.Instance?.OthersideGrpId == 0)
			{
				return false;
			}
			break;
		case LinkedFace.MeldCard:
		case LinkedFace.MeldedPermanent:
		case LinkedFace.SplitCard:
		case LinkedFace.SpecializeParent:
		case LinkedFace.SpecializeChild:
			return false;
		}
		return true;
	}

	public static bool ShowLinkedFaceFaceHangers(LinkedFace linkedFaceType)
	{
		if (linkedFaceType == LinkedFace.None || (uint)(linkedFaceType - 11) <= 1u)
		{
			return false;
		}
		return true;
	}

	public static IEnumerable<(AbilityPrintingData Ability, AbilityState State)> GetAllAbilities(ICardDataAdapter cardData, ICardDataProvider cardDataProvider, bool includeLinkedFaces = true)
	{
		foreach (KeyValuePair<AbilityPrintingData, AbilityState> allAbility in cardData.AllAbilities)
		{
			AbilityPrintingData key = allAbility.Key;
			if (allAbility.Value != AbilityState.Removed)
			{
				yield return (Ability: key, State: allAbility.Value);
			}
		}
		if (cardData.Printing != null && (cardData.Instance == null || !cardData.Instance.AbilityModifications.Exists(AbilityModification.ModType.RemoveAll, (AbilityModification mod, AbilityModification.ModType allType) => mod.Type == allType)))
		{
			foreach (AbilityPrintingData hiddenAbility in cardData.Printing.HiddenAbilities)
			{
				yield return (Ability: hiddenAbility, State: AbilityState.Normal);
			}
		}
		if (!includeLinkedFaces || !ShowLinkedFaceAbilityHangers(cardData))
		{
			yield break;
		}
		for (int i = 0; i < cardData.LinkedFaceGrpIds.Count; i++)
		{
			ICardDataAdapter linkedFaceAtIndex = cardData.GetLinkedFaceAtIndex(i, ignoreInstance: false, cardDataProvider);
			foreach (KeyValuePair<AbilityPrintingData, AbilityState> allAbility2 in linkedFaceAtIndex.AllAbilities)
			{
				AbilityPrintingData key2 = allAbility2.Key;
				if (allAbility2.Value != AbilityState.Removed)
				{
					yield return (Ability: key2, State: allAbility2.Value);
				}
			}
		}
	}
}
