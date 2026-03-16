using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Store;
using UnityEngine;
using Wizards.Mtga.FrontDoorModels;

namespace Core.Rewards;

[Serializable]
public class EventTicketReward : ItemReward<TicketStack, RewardDisplay>
{
	protected override RewardType _rewardType => RewardType.EventTicket;

	public void AddStack(TicketStack ticketToAdd)
	{
		TicketStack ticketStack = ToAdd.FirstOrDefault((TicketStack t) => t.ticket == ticketToAdd.ticket);
		if (ticketStack != null)
		{
			ticketStack.count += ticketToAdd.count;
			return;
		}
		TicketStack item = new TicketStack
		{
			ticket = ticketToAdd.ticket,
			count = ticketToAdd.count
		};
		ToAdd.Enqueue(item);
	}

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
		IEnumerable<TicketStack> tickets = inventoryUpdate.delta.tickets;
		foreach (TicketStack item in tickets ?? Enumerable.Empty<TicketStack>())
		{
			AddStack(item);
		}
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (TicketStack ticketStack in ToAdd)
		{
			yield return (RewardDisplayContext ctxt) => ShowEventTicketReward(ccr, ticketStack, ctxt.ChildIndex);
		}
	}

	private IEnumerator ShowEventTicketReward(ContentControllerRewards ccr, TicketStack ticketStack, int childIndex)
	{
		RewardDisplay rewardDisplay = Instantiate(ccr, childIndex);
		rewardDisplay.GetComponent<Animator>().SetTrigger(ContentControllerRewards.QuantityUpdate);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_reward_rollover_reveal_generic, rewardDisplay.gameObject);
		yield return null;
	}
}
