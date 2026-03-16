using AssetLookupTree;
using AssetLookupTree.Payloads.Card.Badge;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;
using Wotc.Mtgo.Gre.External.Messaging;

public static class AbilityBadgeUtil
{
	public static bool ShouldShowAbilityHanger(AbilityPrintingData ability)
	{
		if (ability == null)
		{
			return false;
		}
		if (!MDNPlayerPrefs.ShowEvergreenKeywordReminders)
		{
			return !ability.IsEvergreen();
		}
		return true;
	}

	public static bool ShouldShowAbilityHanger(AbilityType abilityType)
	{
		if (abilityType == AbilityType.None)
		{
			return false;
		}
		if (!MDNPlayerPrefs.ShowEvergreenKeywordReminders)
		{
			return !abilityType.IsEvergreen();
		}
		return true;
	}

	public static AbilityBadgeData GetBadgeDataForCondition(AssetLookupSystem altSystem, ConditionType condition, ICardDataAdapter cardData = null)
	{
		altSystem.Blackboard.Clear();
		altSystem.Blackboard.Condition = condition;
		altSystem.Blackboard.SetCardDataExtensive(cardData);
		AbilityBadgeData genericBadge = GetGenericBadge<Card_Badge_Condition>(altSystem);
		altSystem.Blackboard.Clear();
		return genericBadge;
	}

	public static AbilityBadgeData GetBadgeDataForDesignation(AssetLookupSystem altSystem, Designation designation, ICardDataAdapter cardData = null)
	{
		altSystem.Blackboard.Clear();
		altSystem.Blackboard.Designation = designation;
		altSystem.Blackboard.SetCardDataExtensive(cardData);
		AbilityBadgeData genericBadge = GetGenericBadge<Card_Badge_Designation>(altSystem);
		altSystem.Blackboard.Clear();
		return genericBadge;
	}

	public static AbilityBadgeData GetLayeredEffectLocKeys(AssetLookupSystem altSystem, string layeredEffectType, ICardDataAdapter cardData = null)
	{
		altSystem.Blackboard.Clear();
		altSystem.Blackboard.LayeredEffectType = layeredEffectType;
		altSystem.Blackboard.SetCardDataExtensive(cardData);
		AbilityBadgeData genericBadge = GetGenericBadge<Card_Badge_LayeredEffect>(altSystem);
		altSystem.Blackboard.Clear();
		return genericBadge;
	}

	public static AbilityBadgeData GetGenericBadge<T>(AssetLookupSystem altSystem) where T : Card_Badge_Base
	{
		if (altSystem.TreeLoader.TryLoadTree(out AssetLookupTree<T> loadedTree))
		{
			T payload = loadedTree.GetPayload(altSystem.Blackboard);
			if (payload != null)
			{
				return new AbilityBadgeData
				{
					BadgePrefabPath = payload.PrefabRef.RelativePath,
					IconSpritePath = payload.IconRef.RelativePath,
					LocTitle = payload.LocTitleKey,
					LocTerm = payload.LocTermKey,
					LocAddendum = payload.LocAddendumKey
				};
			}
		}
		return null;
	}

	public static ViewAbilityBadge CreateAbilityBadge(AbilityBadgeData badgeData, ICardDataAdapter model, int layer)
	{
		if (string.IsNullOrEmpty(badgeData.BadgePrefabPath) || string.IsNullOrEmpty(badgeData.IconSpritePath))
		{
			return null;
		}
		bool highlight = badgeData.ForceHighlight;
		string text = string.Empty;
		if (badgeData.Ability != null && !string.IsNullOrEmpty(badgeData.ActivationWord))
		{
			foreach (AbilityWordData activeAbilityWord in model.ActiveAbilityWords)
			{
				if (activeAbilityWord.AbilityGrpId != 0 && activeAbilityWord.AbilityGrpId != badgeData.Ability.Id)
				{
					continue;
				}
				if (activeAbilityWord.IsActive && activeAbilityWord.AbilityWord == "PartySize")
				{
					text = activeAbilityWord.AdditionalDetail;
					highlight = true;
					break;
				}
				if (activeAbilityWord.AbilityWord == badgeData.ActivationWord)
				{
					if (!string.IsNullOrEmpty(activeAbilityWord.AdditionalDetail))
					{
						text = activeAbilityWord.AdditionalDetail;
					}
					else
					{
						highlight = true;
					}
					break;
				}
			}
		}
		ViewAbilityBadge viewAbilityBadge;
		if (Application.isPlaying)
		{
			viewAbilityBadge = Pantry.Get<IUnityObjectPool>().PopObject(badgeData.BadgePrefabPath).GetComponent<ViewAbilityBadge>();
			viewAbilityBadge.Init();
		}
		else
		{
			viewAbilityBadge = AssetLoader.Instantiate<ViewAbilityBadge>(badgeData.BadgePrefabPath);
		}
		viewAbilityBadge.SetSprite(badgeData.IconSpritePath);
		viewAbilityBadge.SetHighlight(highlight);
		viewAbilityBadge.SetText(text);
		viewAbilityBadge.gameObject.SetLayer(layer);
		return viewAbilityBadge;
	}
}
