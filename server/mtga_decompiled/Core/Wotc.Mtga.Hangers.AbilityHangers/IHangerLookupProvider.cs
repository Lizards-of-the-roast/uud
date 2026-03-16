using System.Collections.Generic;
using AssetLookupTree.Payloads.Ability.Metadata;
using GreClient.CardData;
using Wotc.Mtga.CardParts;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers.AbilityHangers;

public interface IHangerLookupProvider
{
	void FillCardBlackboard(ICardDataAdapter model, CardHolderType cardHolder, CDCViewMetadata metaData);

	IEnumerable<HangerPayload> QueryHangers(AbilityPrintingData ability, AbilityType abilityType);

	AbilityBadgeData GetBadgeData(HangerEntryData hangerEntryData, IReadOnlyCollection<string> layers, AbilityPrintingData ability, ICardDataAdapter cardData);

	bool ShouldShowAbilityHanger(AbilityPrintingData ability);

	bool ShouldShowAbilityHanger(AbilityType referenceType);
}
