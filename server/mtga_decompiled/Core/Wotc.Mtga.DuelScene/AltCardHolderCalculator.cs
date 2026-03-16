using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class AltCardHolderCalculator : IAltCardHolderCalculator
{
	private readonly IObjectPool _objectPool;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly ICardHolderProvider _cardHolderProvider;

	public AltCardHolderCalculator(IObjectPool objectPool, IGameStateProvider gameStateProvider, ICardHolderProvider cardHolderProvider)
	{
		_objectPool = objectPool ?? NullObjectPool.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
	}

	public bool TryGetAltCardHolder(ICardDataAdapter cardData, ICardHolder originalDest, out ICardHolder altDest)
	{
		altDest = originalDest;
		MtgGameState gameState = _gameStateProvider.CurrentGameState;
		if (cardData?.Zone == null)
		{
			return false;
		}
		if (originalDest.CardHolderType == CardHolderType.CardBrowserDefault || originalDest.CardHolderType == CardHolderType.CardBrowserViewDismiss || originalDest.CardHolderType == CardHolderType.Stack)
		{
			return false;
		}
		switch (cardData.ZoneType)
		{
		case ZoneType.PhasedOut:
			altDest = _cardHolderProvider.GetCardHolderByZoneId(gameState.Battlefield.Id);
			return true;
		case ZoneType.Command:
			if (cardData.Instance.Designations.Exists((DesignationData x) => x.Type == Designation.Commander) && cardData.Controller != null)
			{
				MtgZone zoneForPlayer3 = gameState.GetZoneForPlayer(cardData.Controller.InstanceId, ZoneType.Hand);
				altDest = _cardHolderProvider.GetCardHolderByZoneId(zoneForPlayer3.Id);
				return true;
			}
			break;
		case ZoneType.Sideboard:
			if (cardData.Instance.Designations.Exists((DesignationData x) => x.Type == Designation.Companion) && cardData.Controller != null)
			{
				MtgZone zoneForPlayer4 = gameState.GetZoneForPlayer(cardData.Controller.InstanceId, ZoneType.Hand);
				altDest = _cardHolderProvider.GetCardHolderByZoneId(zoneForPlayer4.Id);
				return true;
			}
			break;
		case ZoneType.Library:
			if (ShowLibraryCardAsNHC(cardData, gameState))
			{
				MtgZone zoneForPlayer2 = gameState.GetZoneForPlayer(gameState.LocalPlayer.InstanceId, ZoneType.Hand);
				altDest = _cardHolderProvider.GetCardHolderByZoneId(zoneForPlayer2.Id);
				return true;
			}
			break;
		case ZoneType.Graveyard:
		case ZoneType.Exile:
		{
			HashSet<uint> hashSet = _objectPool.PopObject<HashSet<uint>>();
			List<ActionInfo> list = _objectPool.PopObject<List<ActionInfo>>();
			GatherInstanceIdsRelevantToGraveyardNHC(cardData.Instance, hashSet);
			GatherActionsRelevantToGraveyardNHC(gameState.Actions, hashSet, list);
			ActionInfo actionInfo = null;
			if (list.Count == 1)
			{
				actionInfo = list[0];
			}
			else if (list.Count > 1)
			{
				actionInfo = list.Find((ActionInfo ai) => gameState.GetPlayerById(ai.SeatId).IsLocalPlayer);
				if (actionInfo == null)
				{
					actionInfo = list[0];
				}
			}
			hashSet.Clear();
			list.Clear();
			_objectPool.PushObject(hashSet, tryClear: false);
			_objectPool.PushObject(list, tryClear: false);
			if (actionInfo != null)
			{
				MtgPlayer playerById = gameState.GetPlayerById(actionInfo.SeatId);
				MtgZone zoneForPlayer = gameState.GetZoneForPlayer(playerById.InstanceId, ZoneType.Hand);
				altDest = _cardHolderProvider.GetCardHolderByZoneId(zoneForPlayer.Id);
				return true;
			}
			break;
		}
		}
		if (cardData.ZoneType == ZoneType.Exile && cardData.IsDisplayedFaceDown && cardData.Instance.CatalogId == WellKnownCatalogId.StandardCardBack && cardData.Instance.GrpId == 3 && cardData.Instance.AttachedToId == 0)
		{
			return false;
		}
		if (cardData.ZoneType == ZoneType.Exile && cardData.Instance.AttachedToId != 0 && gameState.TryGetCard(cardData.Instance.AttachedToId, out var card) && card.Zone.Type == ZoneType.Battlefield)
		{
			altDest = _cardHolderProvider.GetCardHolderByZoneId(gameState.Battlefield.Id);
			return true;
		}
		return false;
	}

	public static void GatherInstanceIdsRelevantToGraveyardNHC(MtgCardInstance card, HashSet<uint> resultInstanceIds)
	{
		resultInstanceIds.Clear();
		resultInstanceIds.Add(card.InstanceId);
		foreach (MtgCardInstance linkedFaceInstance in card.LinkedFaceInstances)
		{
			resultInstanceIds.Add(linkedFaceInstance.InstanceId);
		}
		foreach (MtgCardInstance child in card.Children)
		{
			resultInstanceIds.Add(child.InstanceId);
		}
		resultInstanceIds.Remove(0u);
	}

	public static void GatherActionsRelevantToGraveyardNHC(IReadOnlyCollection<ActionInfo> actionsInState, IReadOnlyCollection<uint> cardInstanceIds, List<ActionInfo> resultActions)
	{
		resultActions.Clear();
		foreach (ActionInfo item in actionsInState)
		{
			if (item.Action != null && (cardInstanceIds.Contains(item.Action.InstanceId) || cardInstanceIds.Contains(item.Action.SourceId)))
			{
				resultActions.Add(item);
			}
		}
	}

	public static bool ShowLibraryCardAsNHC(ICardDataAdapter cardData, MtgGameState gameState)
	{
		if (cardData == null || gameState == null)
		{
			return false;
		}
		if (cardData.OwnerNum == GREPlayerNum.LocalPlayer)
		{
			return false;
		}
		foreach (ActionInfo action in cardData.Actions)
		{
			if (ActionRelatesToLocalPlayer(action, gameState))
			{
				return true;
			}
		}
		return false;
	}

	private static bool ActionRelatesToLocalPlayer(ActionInfo actionInfo, MtgGameState gameState)
	{
		if (actionInfo == null || gameState == null)
		{
			return false;
		}
		return gameState.GetPlayerById(actionInfo.SeatId)?.IsLocalPlayer ?? false;
	}
}
