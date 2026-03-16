using System.Collections.Generic;
using System.Linq;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class CombatFramePostProcess : IUXEventGrouper
{
	private readonly GameManager _gameManager;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly ZoneTransferGrouper _zoneTransferGrouper;

	public CombatFramePostProcess(GameManager gameManager, ICardViewProvider cardViewProvider, ZoneTransferGrouper zoneTransferGrouper)
	{
		_gameManager = gameManager;
		_cardViewProvider = cardViewProvider;
		_zoneTransferGrouper = zoneTransferGrouper;
	}

	public void GroupEvents(in int index, ref List<UXEvent> events)
	{
		int num = index;
		while (num < events.Count)
		{
			if (events[num] is UXEventDamageDealt)
			{
				CreateCombatFrame(num, ref events);
			}
			else
			{
				num++;
			}
		}
	}

	private void CreateCombatFrame(int index, ref List<UXEvent> events)
	{
		List<UXEvent> list = new List<UXEvent>();
		List<UXEventDamageDealt> list2 = new List<UXEventDamageDealt>();
		List<UpdateCardModelUXEvent> list3 = new List<UpdateCardModelUXEvent>();
		List<LifeTotalUpdateUXEvent> list4 = new List<LifeTotalUpdateUXEvent>();
		DamageType damageType = DamageType.None;
		while (events.Count > index && events[index] is UXEventDamageDealt uXEventDamageDealt)
		{
			if (damageType == DamageType.None)
			{
				damageType = uXEventDamageDealt.DamageType;
			}
			else if (damageType != uXEventDamageDealt.DamageType)
			{
				break;
			}
			events.RemoveAt(index);
			list2.Add(uXEventDamageDealt);
			while (events.Count > index)
			{
				if (events[index] is UpdateCardModelUXEvent updateCardModelUXEvent && updateCardModelUXEvent.AffectorId == uXEventDamageDealt.Source.InstanceId)
				{
					list3.Add(updateCardModelUXEvent);
					events.RemoveAt(index);
					continue;
				}
				if (events[index] is LifeTotalUpdateUXEvent lifeTotalUpdateUXEvent && lifeTotalUpdateUXEvent.AffectorId == uXEventDamageDealt.Source.InstanceId)
				{
					list4.Add(lifeTotalUpdateUXEvent);
					events.RemoveAt(index);
					continue;
				}
				if (!(events[index] is ZoneTransferUXEvent { Reason: ZoneTransferReason.CardCreated } zoneTransferUXEvent) || zoneTransferUXEvent.NewInstance.ObjectType != GameObjectType.Ability || zoneTransferUXEvent.NewInstance.Parent == null || (zoneTransferUXEvent.NewInstance.ParentId != uXEventDamageDealt.Target.InstanceId && zoneTransferUXEvent.NewInstance.ParentId != uXEventDamageDealt.Source.InstanceId))
				{
					break;
				}
				list.Add(zoneTransferUXEvent);
				events.RemoveAt(index);
			}
		}
		List<SyntheticEventUXEvent> list5 = new List<SyntheticEventUXEvent>();
		if (list2.Count > 0)
		{
			while (events.Count > index)
			{
				SyntheticEventUXEvent syntheticEvent = events[index] as SyntheticEventUXEvent;
				if (syntheticEvent == null || !list2.Exists((UXEventDamageDealt x) => x.Source.InstanceId == syntheticEvent.AffectorId))
				{
					break;
				}
				list5.Add(syntheticEvent);
				events.RemoveAt(index);
			}
		}
		List<CountersChangedUXEvent> list6 = new List<CountersChangedUXEvent>();
		if (list2.Count > 0)
		{
			while (events.Count > index)
			{
				CountersChangedUXEvent counterRemovedEvt = events[index] as CountersChangedUXEvent;
				if (counterRemovedEvt == null || counterRemovedEvt.CounterType != CounterType.Loyalty || !list2.Exists((UXEventDamageDealt x) => x.Target.InstanceId == counterRemovedEvt.AffectedId) || counterRemovedEvt.ChangeAmount >= 0)
				{
					break;
				}
				list6.Add(counterRemovedEvt);
				events.RemoveAt(index);
			}
		}
		List<ZoneTransferUXEvent> list7 = new List<ZoneTransferUXEvent>();
		if (list2.Count > 0)
		{
			int num = index;
			while (num < events.Count)
			{
				ZoneTransferUXEvent zoneTransfer = events[num] as ZoneTransferUXEvent;
				if (zoneTransfer == null)
				{
					break;
				}
				ZoneTransferReason reason = zoneTransfer.Reason;
				if (reason == ZoneTransferReason.Resolve)
				{
					if (!list2.Exists((UXEventDamageDealt x) => x.GetInvolvedIds().Contains(zoneTransfer.OldId)))
					{
						break;
					}
					num++;
				}
				else if (reason == ZoneTransferReason.Delete)
				{
					MtgCardInstance resolvingCard = zoneTransfer.OldInstance;
					if (resolvingCard == null || resolvingCard.ObjectType != GameObjectType.Ability)
					{
						break;
					}
					if (list2.Exists((UXEventDamageDealt x) => x.GetInvolvedIds().Contains(resolvingCard.InstanceId)))
					{
						num++;
						continue;
					}
					if (resolvingCard.Parent == null || !list2.Exists((UXEventDamageDealt x) => x.GetInvolvedIds().Contains(resolvingCard.Parent.InstanceId)))
					{
						break;
					}
					num++;
				}
				else if (reason == ZoneTransferReason.Damage && list2.Exists((UXEventDamageDealt x) => x.GetInvolvedIds().Contains(zoneTransfer.OldId)))
				{
					list7.Add(zoneTransfer);
					events.Remove(zoneTransfer);
				}
				else
				{
					if (reason != ZoneTransferReason.ZeroLoyalty || !list6.Exists((CountersChangedUXEvent x) => x.AffectedId == zoneTransfer.OldId))
					{
						break;
					}
					list7.Add(zoneTransfer);
					events.Remove(zoneTransfer);
				}
			}
		}
		List<UXEvent> list8 = new List<UXEvent>(list2);
		list8.AddRange(list6);
		list8.AddRange(list7);
		list8.AddRange(list3);
		list8.AddRange(list4);
		events.Insert(index, new CombatFrame(list8, _gameManager, _cardViewProvider, _zoneTransferGrouper));
		for (int num2 = 1; num2 <= list.Count; num2++)
		{
			events.Insert(index + num2, list[num2 - 1]);
		}
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
