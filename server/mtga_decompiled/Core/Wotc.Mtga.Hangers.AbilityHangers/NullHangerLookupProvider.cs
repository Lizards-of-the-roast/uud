using System.Collections.Generic;
using AssetLookupTree.Payloads.Ability.Metadata;
using GreClient.CardData;
using Wotc.Mtga.CardParts;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers.AbilityHangers;

public class NullHangerLookupProvider : IHangerLookupProvider
{
	public static NullHangerLookupProvider Default = new NullHangerLookupProvider();

	public void FillCardBlackboard(ICardDataAdapter model, CardHolderType cardHolder, CDCViewMetadata metaData)
	{
	}

	public IEnumerable<HangerPayload> QueryHangers(AbilityPrintingData ability, AbilityType abilityType)
	{
		yield break;
	}

	public AbilityBadgeData GetBadgeData(HangerEntryData hangerEntryData, IReadOnlyCollection<string> layers, AbilityPrintingData ability, ICardDataAdapter cardData)
	{
		return new AbilityBadgeData();
	}

	public bool ShouldShowAbilityHanger(AbilityPrintingData ability)
	{
		return false;
	}

	public bool ShouldShowAbilityHanger(AbilityType referenceType)
	{
		return false;
	}
}
