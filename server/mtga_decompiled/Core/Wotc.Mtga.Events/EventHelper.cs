using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Shared.Code;
using Core.Shared.Code.ClientModels;
using Wizards.MDN;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Unification.Models.Events;
using Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

namespace Wotc.Mtga.Events;

public static class EventHelper
{
	public const string EVENT_DESC_PREPEND = "Events/Event_Desc_";

	public const string EVENT_TITLE_PREPEND = "Events/Event_Title_";

	public static bool PreconWithInvalidDeck(IPlayerEvent playerEvent)
	{
		if (playerEvent == null)
		{
			return false;
		}
		IEventInfo eventInfo = playerEvent.EventInfo;
		if (eventInfo != null && !eventInfo.IsPreconEvent)
		{
			return false;
		}
		if (playerEvent.PacketsChosen != null && playerEvent.PacketsChosen.Count > 0)
		{
			return false;
		}
		Guid? guid = playerEvent.CourseData?.CourseDeck?.Id;
		if (!guid.HasValue)
		{
			return false;
		}
		List<Guid> list = playerEvent.EventUXInfo?.EventComponentData?.InspectPreconDecksWidget?.DeckIds;
		if (list != null)
		{
			return !list.Contains(guid.Value);
		}
		return false;
	}

	public static string GetLocalizedCategory(this IPlayerEvent playerEvent)
	{
		return "Events/Event_Cat_" + playerEvent.EventUXInfo.PublicEventName;
	}

	public static string GetTitleKeyForEvent(IEventUXInfo eventUxInfo)
	{
		return GetKeyOrFallBack(eventUxInfo?.EventComponentData?.TitleRankText?.LocKey, "Events/Event_Title_" + eventUxInfo.PublicEventName);
	}

	public static string GetDescriptionKeyForEvent(IEventUXInfo eventUxInfo)
	{
		return GetKeyOrFallBack(eventUxInfo?.EventComponentData?.DescriptionText?.LocKey, "Events/Event_Desc_" + eventUxInfo.PublicEventName);
	}

	private static string GetKeyOrFallBack(string locKey, string backup)
	{
		if (string.IsNullOrEmpty(locKey))
		{
			return backup;
		}
		return locKey;
	}

	public static bool IsOmniscience(this IPlayerEvent playerEvent)
	{
		return OmniscienceUtil.EventHasEmblem(playerEvent);
	}

	public static EventTimerState GetTimerState(this IPlayerEvent playerEvent)
	{
		bool flag = playerEvent.CourseData.CurrentModule != PlayerEventModule.Join;
		if (playerEvent.EventInfo.EventState == MDNEventState.ForceActive)
		{
			if (!flag)
			{
				return EventTimerState.Unjoined;
			}
			return EventTimerState.Joined;
		}
		DateTime gameTime = ServerGameTime.GameTime;
		DateTime lockedTime = playerEvent.EventInfo.LockedTime;
		DateTime closedTime = playerEvent.EventInfo.ClosedTime;
		if (gameTime < playerEvent.EventInfo.StartTime)
		{
			return EventTimerState.Preview;
		}
		if (flag)
		{
			if (gameTime < closedTime)
			{
				if (!((closedTime - gameTime).TotalHours < 48.0))
				{
					return EventTimerState.Joined;
				}
				return EventTimerState.Joined_ClosingSoon;
			}
			return EventTimerState.ClosedAndCompleted;
		}
		if (gameTime < lockedTime)
		{
			if (!((lockedTime - gameTime).TotalHours < 48.0))
			{
				return EventTimerState.Unjoined;
			}
			return EventTimerState.Unjoined_LockingSoon;
		}
		return EventTimerState.Unjoined_Locked;
	}

	public static int PrioritizeBannerIfPlayerHasToken(bool shouldPrioritize, List<EventEntryFeeInfo> entryFees, List<Client_CustomTokenDefinitionWithQty> tokenDefinitions)
	{
		if (shouldPrioritize)
		{
			if (!HasTokenForEvents(entryFees, tokenDefinitions))
			{
				return 1;
			}
			return 0;
		}
		return 1;
	}

	private static int PrioritizeBannerIfPlayerHasToken(IPlayerEvent playerEvent, List<Client_CustomTokenDefinitionWithQty> tokenDefinitions)
	{
		return PrioritizeBannerIfPlayerHasToken(playerEvent.EventUXInfo.PrioritizeBannerIfPlayerHasToken, playerEvent.EventInfo.EntryFees, tokenDefinitions);
	}

	private static bool HasTokenForEvents(List<EventEntryFeeInfo> entryFees, List<Client_CustomTokenDefinitionWithQty> tokenDefinitions)
	{
		foreach (EventEntryFeeInfo entryFee in entryFees)
		{
			if (HasTokenForEvent(entryFee, tokenDefinitions))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasTokenForEvent(EventEntryFeeInfo info, List<Client_CustomTokenDefinitionWithQty> tokenDefinitions)
	{
		if (!info.UsesRemaining)
		{
			return false;
		}
		string text = TokenIdForEvent(info);
		Client_CustomTokenDefinitionWithQty client_CustomTokenDefinitionWithQty = TokenDefinitionForId(tokenDefinitions, text);
		if (text != null && client_CustomTokenDefinitionWithQty != null && client_CustomTokenDefinitionWithQty.Quantity >= info.Quantity)
		{
			return true;
		}
		return false;
	}

	public static string TokenIdForEvent(EventEntryFeeInfo entryFeeInfo)
	{
		return entryFeeInfo.CurrencyType switch
		{
			EventEntryCurrencyType.DraftToken => "DraftToken", 
			EventEntryCurrencyType.SealedToken => "SealedToken", 
			EventEntryCurrencyType.EventToken => entryFeeInfo.ReferenceId, 
			_ => null, 
		};
	}

	public static Client_CustomTokenDefinitionWithQty TokenDefinitionForId(List<Client_CustomTokenDefinitionWithQty> tokenDefinitions, string tokenId)
	{
		if (string.IsNullOrEmpty(tokenId))
		{
			return null;
		}
		foreach (Client_CustomTokenDefinitionWithQty tokenDefinition in tokenDefinitions)
		{
			if (tokenDefinition.TokenId == tokenId)
			{
				return tokenDefinition;
			}
		}
		return null;
	}

	public static List<BillboardData> GetPriorityBillboards(List<EventContext> orderedEventContexts, List<ClientDynamicFilterTag> orderedFilterTags, List<Client_CustomTokenDefinitionWithQty> tokenDefinitions, int count)
	{
		if (count <= 0)
		{
			return new List<BillboardData>(0);
		}
		return (from data in orderedEventContexts.Select((EventContext context) => new BillboardData(EBillboardType.Event, context)).Concat(orderedFilterTags.Select((ClientDynamicFilterTag filter) => new BillboardData(EBillboardType.Filter, GetHighestPriorityEventWithTag(orderedEventContexts, filter), filter)))
			orderby PrioritizeBannerIfPlayerHasToken(data.BillboardEvent.PlayerEvent, tokenDefinitions)
			select data).ThenBy(delegate(BillboardData data)
		{
			int displayPriority = data.BillboardEvent.PlayerEvent.EventUXInfo.DisplayPriority;
			int num = data.BillboardDynamicFilterTag?.BillboardPriority ?? displayPriority;
			return (displayPriority > num) ? num : displayPriority;
		}).Take(count).ToList();
	}

	private static EventContext GetHighestPriorityEventWithTag(List<EventContext> eventContexts, ClientDynamicFilterTag dynamicFilterTag)
	{
		return eventContexts.FirstOrDefault((EventContext x) => x.PlayerEvent.EventUXInfo.DynamicFilterTagIds != null && x.PlayerEvent.EventUXInfo.DynamicFilterTagIds.Contains(dynamicFilterTag.TagId));
	}
}
