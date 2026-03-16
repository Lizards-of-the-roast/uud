using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class OwnershipHangerConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _locProvider;

	private readonly IEntityNameProvider<uint> _entityNameProvider;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly IGameStateProvider _gameStateProvider;

	public OwnershipHangerConfigProvider(IClientLocProvider locProvider, IEntityNameProvider<uint> entityNameProvider, ICardViewProvider cardViewProvider, IGameStateProvider gameStateProvider)
	{
		_locProvider = locProvider ?? NullLocProvider.Default;
		_entityNameProvider = entityNameProvider ?? NullIdNameProvider.Default;
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (ShouldCreate(model))
		{
			string localizedText = _locProvider.GetLocalizedText("AbilityHanger/SpecialHangers/Ownership/Header");
			string name = _entityNameProvider.GetName(model.Owner?.InstanceId ?? 0);
			yield return new HangerConfig(localizedText, name, null, null, convertSymbols: false);
		}
	}

	private bool ShouldCreate(ICardDataAdapter cardData)
	{
		MtgCardInstance instance = cardData.Instance;
		if (instance == null)
		{
			return false;
		}
		if (instance.ObjectType == GameObjectType.Ability)
		{
			return false;
		}
		MtgPlayer owner = instance.Owner;
		MtgPlayer controller = instance.Controller;
		if (owner == null || controller == null)
		{
			return false;
		}
		if (owner.InstanceId != controller.InstanceId)
		{
			return true;
		}
		MtgZone zone = instance.Zone;
		if (zone == null)
		{
			return false;
		}
		if (!_cardViewProvider.TryGetCardView(instance.InstanceId, out var cardView))
		{
			return false;
		}
		if (!(cardView.CurrentCardHolder is ZoneCardHolderBase { GetZone: var getZone }))
		{
			return false;
		}
		if (getZone == null)
		{
			return false;
		}
		if (!IsInAnotherPlayersZone(zone, getZone))
		{
			return IsExileAttachment(zone.Type, _gameStateProvider.CurrentGameState, instance.AttachedToId, owner.InstanceId);
		}
		return true;
	}

	private static bool IsInAnotherPlayersZone(MtgZone instanceZone, MtgZone cardHolderZone)
	{
		if (instanceZone.Owner != null && cardHolderZone.Owner != null)
		{
			return instanceZone.OwnerId != cardHolderZone.OwnerId;
		}
		return false;
	}

	private static bool IsExileAttachment(ZoneType zoneType, MtgGameState gameState, uint attachedToId, uint ownerId)
	{
		if (zoneType != ZoneType.Exile)
		{
			return false;
		}
		if (!gameState.TryGetCard(attachedToId, out var card))
		{
			return false;
		}
		MtgPlayer controller = card.Controller;
		if (controller == null)
		{
			return false;
		}
		return ownerId != controller.InstanceId;
	}
}
