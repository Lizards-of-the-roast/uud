using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class StartingZoneIdCalculator
{
	public uint GetStartingZoneId(uint sourceId, MtgGameState latestGameState, ICardDatabaseAdapter cardDatabase, ICollection<uint> zoneIds)
	{
		List<MetaDataTag> list = new List<MetaDataTag>();
		if (latestGameState.TryGetCard(sourceId, out var card))
		{
			ICardDataAdapter cardDataAdapter = card.ToCardData(cardDatabase);
			if (cardDataAdapter != null)
			{
				if (card.ObjectType == GameObjectType.Ability && cardDatabase.AbilityDataProvider.TryGetAbilityPrintingById(card.GrpId, out var ability))
				{
					list.AddRange(ability.Tags);
				}
				list.AddRange(cardDataAdapter.Tags);
				foreach (AbilityPrintingData ability2 in cardDataAdapter.Abilities)
				{
					list.AddRange(ability2.Tags);
				}
			}
		}
		return StartingZoneId(list, latestGameState, zoneIds);
	}

	public uint StartingZoneId(IReadOnlyCollection<MetaDataTag> sourceCardTags, MtgGameState gameState, ICollection<uint> zoneIds)
	{
		if (sourceCardTags.Contains(MetaDataTag.DefaultToLocalGraveyard) && zoneIds.Contains(gameState.LocalGraveyard.Id))
		{
			return gameState.LocalGraveyard.Id;
		}
		if (zoneIds.Contains(gameState.LocalSideboard.Id))
		{
			return gameState.LocalSideboard.Id;
		}
		if (zoneIds.Contains(gameState.OpponentHand.Id))
		{
			return gameState.OpponentHand.Id;
		}
		if (zoneIds.Contains(gameState.OpponentLibrary.Id))
		{
			return gameState.OpponentLibrary.Id;
		}
		if (zoneIds.Contains(gameState.OpponentGraveyard.Id))
		{
			return gameState.OpponentGraveyard.Id;
		}
		if (zoneIds.Contains(gameState.LocalGraveyard.Id))
		{
			return gameState.LocalGraveyard.Id;
		}
		if (zoneIds.Contains(gameState.LocalLibrary.Id))
		{
			return gameState.LocalLibrary.Id;
		}
		if (zoneIds.Contains(gameState.LocalHand.Id))
		{
			return gameState.LocalHand.Id;
		}
		if (zoneIds.Contains(gameState.Stack.Id))
		{
			return gameState.Stack.Id;
		}
		if (zoneIds.Contains(gameState.Battlefield.Id))
		{
			return gameState.Battlefield.Id;
		}
		if (zoneIds.Contains(gameState.Exile.Id))
		{
			return gameState.Exile.Id;
		}
		using (IEnumerator<uint> enumerator = zoneIds.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return uint.MaxValue;
	}
}
