using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Ability;
using AssetLookupTree.Payloads.Ability.Metadata;
using GreClient.CardData;
using Pooling;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers.AbilityHangers;

public class ALTQueryProvider : IHangerLookupProvider
{
	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly IObjectPool _genericPool;

	public ALTQueryProvider(AssetLookupSystem assetLookupSystem, IObjectPool genericPool)
	{
		_assetLookupSystem = assetLookupSystem;
		_genericPool = genericPool;
	}

	public void FillCardBlackboard(ICardDataAdapter model, CardHolderType cardHolder, CDCViewMetadata metaData)
	{
		IBlackboard blackboard = _assetLookupSystem.Blackboard;
		blackboard.Clear();
		blackboard.SetCardDataExtensive(model);
		blackboard.SetCdcViewMetadata(metaData);
		blackboard.CardHolderType = cardHolder;
	}

	public AbilityBadgeData GetBadgeData(HangerEntryData hangerEntryData, IReadOnlyCollection<string> layers, AbilityPrintingData ability, ICardDataAdapter cardData)
	{
		AbilityBadgeData abilityBadgeData = new AbilityBadgeData
		{
			Ability = ability,
			LocTitle = hangerEntryData.Title,
			LocTerm = hangerEntryData.Term,
			LocAddendum = hangerEntryData.Addendum
		};
		AssetLookupTree<BadgeEntry> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<BadgeEntry>();
		if (assetLookupTree != null)
		{
			HashSet<BadgeEntry> hashSet = _genericPool.PopObject<HashSet<BadgeEntry>>();
			if (assetLookupTree.GetPayloadLayered(_assetLookupSystem.Blackboard, hashSet))
			{
				BadgeEntry badgeEntry = null;
				foreach (BadgeEntry item in hashSet)
				{
					if (item.Layers.SetEquals(layers))
					{
						badgeEntry = item;
						break;
					}
				}
				BadgeEntryData badgeEntryData = null;
				if (badgeEntry != null)
				{
					badgeEntryData = badgeEntry.Data;
				}
				if (badgeEntryData != null && badgeEntryData.ValidOnHanger)
				{
					abilityBadgeData.IconSpritePath = badgeEntryData.SpriteRef.RelativePath;
					abilityBadgeData.ActivationWord = badgeEntryData.GetActivationWord();
					if (badgeEntryData.NumberCalculator.GetNumber(new NumericBadgeCalculatorInput
					{
						Ability = ability,
						CardData = cardData
					}, out var number, out var modifier))
					{
						abilityBadgeData.ActivationWordCount = $"{number}{modifier}";
					}
				}
				hashSet.Clear();
				_genericPool.PushObject(hashSet, tryClear: false);
			}
		}
		return abilityBadgeData;
	}

	public bool ShouldShowAbilityHanger(AbilityPrintingData ability)
	{
		return AbilityBadgeUtil.ShouldShowAbilityHanger(ability);
	}

	public bool ShouldShowAbilityHanger(AbilityType referenceType)
	{
		return AbilityBadgeUtil.ShouldShowAbilityHanger(referenceType);
	}

	public IEnumerable<HangerPayload> QueryHangers(AbilityPrintingData ability, AbilityType abilityType)
	{
		AssetLookupTree<HangerEntry> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<HangerEntry>();
		if (assetLookupTree == null)
		{
			yield break;
		}
		IBlackboard blackboard = _assetLookupSystem.Blackboard;
		blackboard.Ability = ability;
		blackboard.AbilityType = abilityType;
		HashSet<HangerEntry> foundPayloads = _genericPool.PopObject<HashSet<HangerEntry>>();
		if (!assetLookupTree.GetPayloadLayered(_assetLookupSystem.Blackboard, foundPayloads))
		{
			yield break;
		}
		foreach (HangerEntry item in foundPayloads)
		{
			yield return new HangerPayload(item.Data, item.Layers);
		}
		foundPayloads.Clear();
		_genericPool.PushObject(foundPayloads, tryClear: false);
	}
}
