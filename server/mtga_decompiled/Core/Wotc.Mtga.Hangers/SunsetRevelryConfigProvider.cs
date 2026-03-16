using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class SunsetRevelryConfigProvider : IHangerConfigProvider
{
	private const uint SunsetRevelryTitleId = 529825u;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IClientLocProvider _clientLocProvider;

	private readonly string _checkedIconPath;

	private readonly string _crossedOutIconPath;

	public SunsetRevelryConfigProvider(IClientLocProvider clientLocProvider, IGameStateProvider gameStateProvider, IPathProvider<string> iconPathProvider)
	{
		_clientLocProvider = clientLocProvider;
		_gameStateProvider = gameStateProvider;
		_checkedIconPath = iconPathProvider.GetPath("CheckmarkInBox");
		_crossedOutIconPath = iconPathProvider.GetPath("CrossedOutBox");
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (ShowHanger(model))
		{
			MtgGameState gameState = _gameStateProvider.CurrentGameState;
			MtgCardInstance instance = model.Instance;
			uint controllerId = instance.Controller.InstanceId;
			string spritePath = (OpponentHasMoreLife(controllerId, gameState) ? _checkedIconPath : _crossedOutIconPath);
			string localizedText = _clientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/SunsetRevelry_Life_Body");
			yield return new HangerConfig(string.Empty, localizedText, null, spritePath);
			string spritePath2 = (OpponentHasMoreCreatures(controllerId, gameState) ? _checkedIconPath : _crossedOutIconPath);
			string localizedText2 = _clientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/SunsetRevelry_Creatures_Body");
			yield return new HangerConfig(string.Empty, localizedText2, null, spritePath2);
			string spritePath3 = (OpponentHasMoreCardsInHand(controllerId, gameState) ? _checkedIconPath : _crossedOutIconPath);
			string localizedText3 = _clientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/SunsetRevelry_Hand_Body");
			yield return new HangerConfig(string.Empty, localizedText3, null, spritePath3);
		}
	}

	private static bool ShowHanger(ICardDataAdapter cardData)
	{
		if (cardData != null)
		{
			return ShowHanger(cardData.Instance);
		}
		return false;
	}

	private static bool ShowHanger(MtgCardInstance instance)
	{
		if (instance != null && instance.TitleId == 529825 && ControlledByLocalPlayer(instance))
		{
			return IsInHand(instance);
		}
		return false;
	}

	private static bool ControlledByLocalPlayer(MtgCardInstance instance)
	{
		return instance.Controller.IsLocalPlayer;
	}

	private static bool IsInHand(MtgCardInstance instance)
	{
		MtgZone zone = instance.Zone;
		if (zone != null)
		{
			return zone.Type == ZoneType.Hand;
		}
		return false;
	}

	private static bool OpponentHasMoreLife(uint controllerId, MtgGameState gameState)
	{
		if (!gameState.TryGetPlayer(controllerId, out var player))
		{
			return false;
		}
		int lifeTotal = player.LifeTotal;
		foreach (MtgPlayer player2 in gameState.Players)
		{
			if (player2.LifeTotal > lifeTotal)
			{
				return true;
			}
		}
		return false;
	}

	private static bool OpponentHasMoreCreatures(uint controllerId, MtgGameState gameState)
	{
		if (!gameState.TryGetPlayer(controllerId, out var _))
		{
			return false;
		}
		MtgZone battlefield = gameState.Battlefield;
		uint creatureCount = GetCreatureCount(battlefield, controllerId);
		foreach (MtgPlayer player2 in gameState.Players)
		{
			if (GetCreatureCount(battlefield, player2.InstanceId) > creatureCount)
			{
				return true;
			}
		}
		return false;
	}

	private static uint GetCreatureCount(MtgZone battlefield, uint controllerId)
	{
		uint num = 0u;
		foreach (MtgCardInstance visibleCard in battlefield.VisibleCards)
		{
			if (visibleCard.CardTypes.Contains(CardType.Creature) && visibleCard.Controller.InstanceId == controllerId)
			{
				num++;
			}
		}
		return num;
	}

	private static bool OpponentHasMoreCardsInHand(uint controllerId, MtgGameState gameState)
	{
		uint num = CardsInHandForPlayer(controllerId, gameState);
		foreach (MtgPlayer player in gameState.Players)
		{
			if (CardsInHandForPlayer(player.InstanceId, gameState) > num)
			{
				return true;
			}
		}
		return false;
	}

	private static uint CardsInHandForPlayer(uint controllerId, MtgGameState gameState)
	{
		return gameState.GetZoneForPlayer(controllerId, ZoneType.Hand)?.TotalCardCount ?? 0;
	}
}
