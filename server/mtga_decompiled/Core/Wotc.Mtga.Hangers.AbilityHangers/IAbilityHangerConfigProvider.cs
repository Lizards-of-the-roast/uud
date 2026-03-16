using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.CardParts;

namespace Wotc.Mtga.Hangers.AbilityHangers;

public interface IAbilityHangerConfigProvider
{
	IEnumerable<HangerConfig> GetHangerConfigsForCard(ICardDataAdapter cardData, CardHolderType cardHolder, CDCViewMetadata metaData);

	IEnumerable<HangerConfig> GetHangerConfigsForAbility(ICardDataAdapter cardData, CardHolderType cardHolder, CDCViewMetadata metaData, AbilityPrintingData ability, AbilityState state = AbilityState.Normal);

	void Cleanup();
}
