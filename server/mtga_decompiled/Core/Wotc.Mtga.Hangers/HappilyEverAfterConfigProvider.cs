using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class HappilyEverAfterConfigProvider : IHangerConfigProvider
{
	private const uint AbilityId = 136094u;

	private readonly IObjectPool _objectPool;

	private readonly IClientLocProvider _clientLocProvider;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly string _checkedIconPath;

	private readonly string _uncheckedIconPath;

	public HappilyEverAfterConfigProvider(IObjectPool objectPool, IClientLocProvider clientLocProvider, IGameStateProvider gameStateProvider, IPathProvider<string> iconPathProvider)
	{
		_objectPool = objectPool;
		_clientLocProvider = clientLocProvider;
		_gameStateProvider = gameStateProvider;
		_checkedIconPath = iconPathProvider.GetPath("CheckmarkInBox");
		_uncheckedIconPath = iconPathProvider.GetPath("EmptyBox");
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (!ShouldCreateHangers(model))
		{
			yield break;
		}
		MtgGameState gameState = _gameStateProvider.CurrentGameState;
		HashSet<CardColor> colors = _objectPool.PopObject<HashSet<CardColor>>();
		HashSet<CardType> cardTypes = _objectPool.PopObject<HashSet<CardType>>();
		foreach (MtgCardInstance visibleCard in gameState.Battlefield.VisibleCards)
		{
			if (visibleCard.Controller.InstanceId != model.Controller.InstanceId)
			{
				continue;
			}
			foreach (CardColor color in visibleCard.Colors)
			{
				colors.Add(color);
			}
			foreach (CardType cardType in visibleCard.CardTypes)
			{
				cardTypes.Add(cardType);
			}
		}
		MtgZone zoneForPlayer = gameState.GetZoneForPlayer(model.Controller.InstanceId, ZoneType.Graveyard);
		if (zoneForPlayer != null)
		{
			foreach (MtgCardInstance visibleCard2 in zoneForPlayer.VisibleCards)
			{
				foreach (CardType cardType2 in visibleCard2.CardTypes)
				{
					cardTypes.Add(cardType2);
				}
			}
		}
		string countConditionNotMetString = "<color=red>{0}</color>";
		string countConditionMetString = "<style=\"Title\"><color=green>{0}</color></style>";
		bool flag = (long)colors.Count >= 5L;
		string format = (flag ? countConditionMetString : countConditionNotMetString);
		format = string.Format(format, colors.Count);
		string localizedText = _clientLocProvider.GetLocalizedText((colors.Count == 1) ? "AbilityHanger/SpecialHangers/HappilyEverAfter_Color_Body" : "AbilityHanger/SpecialHangers/HappilyEverAfter_Colors_Body", ("count", format));
		yield return new HangerConfig(string.Empty, localizedText, null, flag ? _checkedIconPath : _uncheckedIconPath);
		bool flag2 = (long)cardTypes.Count >= 6L;
		string format2 = (flag2 ? countConditionMetString : countConditionNotMetString);
		format2 = string.Format(format2, cardTypes.Count);
		string localizedText2 = _clientLocProvider.GetLocalizedText((cardTypes.Count == 1) ? "AbilityHanger/SpecialHangers/HappilyEverAfter_Type_Body" : "AbilityHanger/SpecialHangers/HappilyEverAfter_Types_Body", ("count", format2));
		yield return new HangerConfig(string.Empty, localizedText2, null, flag2 ? _checkedIconPath : _uncheckedIconPath);
		MtgPlayer playerById = gameState.GetPlayerById(model.Controller.InstanceId);
		bool flag3 = playerById.LifeTotal >= playerById.StartingLifeTotal;
		string localizedText3 = _clientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/HappilyEverAfter_Life_Body");
		yield return new HangerConfig(string.Empty, localizedText3, null, flag3 ? _checkedIconPath : _uncheckedIconPath);
		_objectPool.PushObject(colors);
		_objectPool.PushObject(cardTypes);
	}

	private static bool ShouldCreateHangers(ICardDataAdapter model)
	{
		if (model != null)
		{
			return ShouldCreateHangers(model.Instance);
		}
		return false;
	}

	private static bool ShouldCreateHangers(MtgCardInstance cardInstance)
	{
		if (cardInstance == null)
		{
			return false;
		}
		foreach (AbilityPrintingData ability in cardInstance.Abilities)
		{
			if (ability.Id == 136094)
			{
				return true;
			}
		}
		return false;
	}
}
