using System.Collections.Generic;
using AssetLookupTree;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ZoneTransferGrouper : IUXEventGrouper
{
	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly IVfxProvider _vfxProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly GameManager _gameManager;

	public ZoneTransferGrouper(ICardHolderProvider cardHolderProvider, IVfxProvider vfxProvider, AssetLookupSystem assetLookupSystem, GameManager gameManager)
	{
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
		_vfxProvider = vfxProvider ?? NullVfxProvider.Default;
		_assetLookupSystem = assetLookupSystem;
		_gameManager = gameManager;
	}

	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		int num = 0;
		while (num < events.Count)
		{
			ZoneTransferGroup zoneTransferGroup = events[num] as ZoneTransferGroup;
			if (events[num] is ZoneTransferUXEvent zoneTransferUXEvent)
			{
				ZoneTransferGroup item = new ZoneTransferGroup(zoneTransferUXEvent, _vfxProvider, _assetLookupSystem, _gameManager);
				events.Remove(zoneTransferUXEvent);
				events.Insert(num, item);
				continue;
			}
			if (num + 1 < events.Count && zoneTransferGroup != null && events[num + 1] is ZoneTransferUXEvent zoneTransferUXEvent2 && AbleToGroupWith(zoneTransferUXEvent2, zoneTransferGroup))
			{
				events.Remove(zoneTransferUXEvent2);
				zoneTransferGroup.Enqueue(zoneTransferUXEvent2);
				continue;
			}
			if (zoneTransferGroup != null)
			{
				foreach (ZoneTransferUXEvent zoneTransfer in zoneTransferGroup._zoneTransfers)
				{
					if (!IsFromOpponentsHand(zoneTransfer))
					{
						continue;
					}
					if (CardNoLongerVisibleAfterReveal(zoneTransfer))
					{
						List<CardRevealedEvent> revealEvents = zoneTransfer.RevealEvents.FindAll((CardRevealedEvent x) => x.EventType == RevealEventType.Delete);
						zoneTransfer.RevealEvents.RemoveAll((CardRevealedEvent x) => x.EventType == RevealEventType.Delete);
						events.Insert(num, new RevealCardsUXEvent(revealEvents, _gameManager.Context));
						num++;
					}
					else if (!IsKnownDigitalHandToVisibleZone(zoneTransfer, num, events))
					{
						events.Insert(num, new HandShuffleUxEvent(_cardHolderProvider));
						num++;
					}
					break;
				}
			}
			num++;
		}
	}

	private static bool AbleToGroupWith(ZoneTransferUXEvent potentialTransferEvt, ZoneTransferGroup zoneTransferGroup)
	{
		foreach (ZoneTransferUXEvent zoneTransfer in zoneTransferGroup._zoneTransfers)
		{
			if (!CanGroupZoneTransfers(zoneTransfer, potentialTransferEvt))
			{
				return false;
			}
		}
		return true;
	}

	public static bool CanGroupZoneTransfers(ZoneTransferUXEvent lhs, ZoneTransferUXEvent rhs)
	{
		if (lhs == null || rhs == null)
		{
			return false;
		}
		return CanGroupZoneTransfers(ZoneTransferDetails.Translate(lhs), ZoneTransferDetails.Translate(rhs));
	}

	public static bool CanGroupZoneTransfers(ZoneTransferDetails lhs, ZoneTransferDetails rhs)
	{
		if (HasReasonConflict(lhs, rhs))
		{
			return false;
		}
		if (HasIdConflict(lhs, rhs))
		{
			return false;
		}
		return true;
	}

	public static bool HasIdConflict(ZoneTransferDetails lhs, ZoneTransferDetails rhs)
	{
		if (lhs.NewId == rhs.NewId)
		{
			return true;
		}
		if (lhs.HasIdChange && lhs.OldId == rhs.NewId)
		{
			return true;
		}
		if (rhs.HasIdChange && rhs.OldId == lhs.NewId)
		{
			return true;
		}
		return false;
	}

	public static bool HasReasonConflict(ZoneTransferDetails lhs, ZoneTransferDetails rhs)
	{
		if (lhs.Reason == rhs.Reason)
		{
			return false;
		}
		if (lhs.IsPendingCreation || rhs.IsPendingCreation)
		{
			return false;
		}
		if (lhs.ReasonIsSBA && rhs.ReasonIsSBA)
		{
			return false;
		}
		return true;
	}

	private static bool IsFromOpponentsHand(ZoneTransferUXEvent zte)
	{
		if (zte != null && zte.FromZone != null && zte.FromZone.Owner != null && !zte.FromZone.Owner.IsLocalPlayer)
		{
			return zte.FromZone.Type == ZoneType.Hand;
		}
		return false;
	}

	private static bool IsKnownDigitalHandToVisibleZone(ZoneTransferUXEvent zte, int index, List<UXEvent> events)
	{
		if (zte.ToZone == null || zte.ToZone.Visibility != Visibility.Public)
		{
			return false;
		}
		int index2 = index;
		while (index2-- > 0)
		{
			if (!(events[index2] is ZoneTransferGroup zoneTransferGroup))
			{
				continue;
			}
			foreach (ZoneTransferUXEvent zoneTransfer in zoneTransferGroup._zoneTransfers)
			{
				if (zoneTransfer.NewId == zte.OldId && (zoneTransfer.Reason == ZoneTransferReason.Draft || zoneTransfer.Reason == ZoneTransferReason.Seek))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool CardNoLongerVisibleAfterReveal(ZoneTransferUXEvent zte)
	{
		if (zte == null)
		{
			return false;
		}
		if (!HasDeletedRevealEvent(zte.RevealEvents))
		{
			return false;
		}
		if (zte.NewInstance != null && (zte.NewInstance.FaceDownState.IsFaceDown || zte.NewInstance.Visibility != Visibility.Public))
		{
			return true;
		}
		return false;
	}

	private static bool HasDeletedRevealEvent(IEnumerable<CardRevealedEvent> revealEvents)
	{
		if (revealEvents == null)
		{
			return false;
		}
		foreach (CardRevealedEvent revealEvent in revealEvents)
		{
			if (revealEvent.EventType == RevealEventType.Delete)
			{
				return true;
			}
		}
		return false;
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
