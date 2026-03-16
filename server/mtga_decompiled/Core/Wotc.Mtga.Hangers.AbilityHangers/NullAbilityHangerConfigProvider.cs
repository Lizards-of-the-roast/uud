using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.CardParts;

namespace Wotc.Mtga.Hangers.AbilityHangers;

public class NullAbilityHangerConfigProvider : IAbilityHangerConfigProvider
{
	public IEnumerable<HangerConfig> GetHangerConfigsForAbility(ICardDataAdapter cardData, CardHolderType cardHolder, CDCViewMetadata metaData, AbilityPrintingData ability, AbilityState state = AbilityState.Normal)
	{
		yield break;
	}

	public IEnumerable<HangerConfig> GetHangerConfigsForCard(ICardDataAdapter cardData, CardHolderType cardHolder, CDCViewMetadata metaData)
	{
		yield break;
	}

	public void Cleanup()
	{
	}
}
