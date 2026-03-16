using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public static class ReplacementEffectDataUtil
{
	public static bool HasInvalidAbilityId(this ReplacementEffectData data)
	{
		uint abilityId = data.AbilityId;
		if (abilityId == 0 || abilityId == 133355)
		{
			return true;
		}
		return false;
	}

	public static bool AffectedIdIsPlayer(this ReplacementEffectData data, MtgGameState gameState)
	{
		MtgPlayer player;
		return gameState.TryGetPlayer(data.AffectedId, out player);
	}

	public static bool RecipientIsPlayer(this ReplacementEffectData data, MtgGameState gameState)
	{
		if (data.RecipientIds == null)
		{
			return false;
		}
		foreach (uint recipientId in data.RecipientIds)
		{
			if (gameState.TryGetPlayer(recipientId, out var _))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AbilityIsNotStaticDamagePrevention(this ReplacementEffectData data, AbilityPrintingData abilityData)
	{
		return !data.SpawnerType.Contains(ReplacementEffectSpawnerType.PreventDamage) || abilityData == null || abilityData.Category != AbilityCategory.Static;
	}

	public static bool EffectIsGeneralDamageRedirectionToCard(this ReplacementEffectData data, MtgGameState gameState)
	{
		MtgCardInstance card;
		if (data.SpawnerType.Contains(ReplacementEffectSpawnerType.DamageRedirection) && data.SourceIds.Count == 0 && data.RecipientIds.Count == 1)
		{
			return gameState.TryGetCard(data.RecipientIds[0], out card);
		}
		return false;
	}

	public static bool AffectorAndAffectedAreSame(this ReplacementEffectData data, MtgGameState gameState)
	{
		if (gameState != null)
		{
			MtgCardInstance cardById = gameState.GetCardById(data.AffectorId);
			if (cardById != null)
			{
				MtgCardInstance cardById2 = gameState.GetCardById(data.AffectedId);
				if (cardById2 != null)
				{
					if (cardById2.InstanceId != cardById.InstanceId && cardById2.ParentId != cardById.InstanceId)
					{
						return cardById2.InstanceId == cardById.ParentId;
					}
					return true;
				}
			}
		}
		return false;
	}

	public static bool AffectorIsBattlefieldButAffectedDoesntExist(this ReplacementEffectData data, MtgGameState gameState)
	{
		if (gameState != null && data.AffectorId < data.AffectedId)
		{
			MtgCardInstance cardById = gameState.GetCardById(data.AffectorId);
			if (cardById != null && gameState.GetCardById(data.AffectedId) == null)
			{
				MtgZone zone = cardById.Zone;
				if (zone == null)
				{
					return false;
				}
				return zone.Type == ZoneType.Battlefield;
			}
		}
		return false;
	}
}
