using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Duel;

public static class TargetingColorer
{
	public static readonly string EmptyFormat = "{0}";

	public static bool IsMultiTargetAbility(uint abilityGrpId, IAbilityDataProvider abilityProvider, IList<TargetSpec> targetSpecs)
	{
		if (abilityProvider.TryGetAbilityPrintingById(abilityGrpId, out var ability) && !ability.IsModalAbility() && !ability.IsModalAbilityChild())
		{
			return false;
		}
		foreach (TargetSpec targetSpec in targetSpecs)
		{
			if (targetSpec.AbilityId != abilityGrpId)
			{
				return true;
			}
		}
		return false;
	}

	public static string GetFormatDueToTargeting(uint abilityGrpId, IAbilityDataProvider abilityProvider, IList<TargetSpec> targetSpecs, AssetLookupSystem assetLookupSystem, ICardDataAdapter cardData)
	{
		string result = EmptyFormat;
		if (IsMultiTargetAbility(abilityGrpId, abilityProvider, targetSpecs))
		{
			for (int i = 0; i < targetSpecs.Count; i++)
			{
				if (targetSpecs[i].AbilityId == abilityGrpId)
				{
					result = GetFormatDueToTargetingIndex(i, assetLookupSystem, cardData);
				}
			}
		}
		return result;
	}

	private static string GetFormatDueToTargetingIndex(int targetIndex, AssetLookupSystem assetLookupSystem, ICardDataAdapter cardData)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.TargetIndex = targetIndex;
		assetLookupSystem.Blackboard.SetCardDataExtensive(cardData);
		return GetFormatFromALT(assetLookupSystem);
	}

	public static string GetHangerTextTargetingFormat(int targetIndex, AssetLookupSystem assetLookupSystem, ICardDataAdapter cardData)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.TargetIndex = targetIndex;
		assetLookupSystem.Blackboard.SetCardDataExtensive(cardData);
		assetLookupSystem.Blackboard.IsHangerText = true;
		return GetFormatFromALT(assetLookupSystem);
	}

	public static string GetFormatForIndex(int targetIndex, AssetLookupSystem assetLookupSystem, bool useDarkHangerColors = false)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.TargetIndex = targetIndex;
		assetLookupSystem.Blackboard.IsHangerText = useDarkHangerColors;
		return GetFormatFromALT(assetLookupSystem);
	}

	private static string GetFormatFromALT(AssetLookupSystem assetLookupSystem)
	{
		string result = string.Empty;
		TargetingColorPayload payload = assetLookupSystem.TreeLoader.LoadTree<TargetingColorPayload>().GetPayload(assetLookupSystem.Blackboard);
		if (payload != null)
		{
			result = "<#" + ColorUtility.ToHtmlStringRGB(payload.Color) + ">{0}</color>";
		}
		return result;
	}
}
