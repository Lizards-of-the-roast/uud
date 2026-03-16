using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class EnsureTargetVisualizationPostProcess : IUXEventGrouper
{
	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		if (TryGetInsertIndex(events, out var insertIdx))
		{
			events.Insert(insertIdx, new WaitForSecondsUXEvent(0.333f));
		}
	}

	private static bool TryGetInsertIndex(IReadOnlyList<UXEvent> events, out int insertIdx)
	{
		if (events.Exists((UXEvent x) => x.IsBlocking))
		{
			insertIdx = -1;
			return false;
		}
		insertIdx = -1;
		for (int num = events.Count - 1; num >= 0; num--)
		{
			if (IsOpponentAbilityWithUpdatedTarget(events[num] as UpdateCardModelUXEvent))
			{
				insertIdx = num + 1;
				return true;
			}
		}
		insertIdx = -1;
		return false;
	}

	private static bool IsOpponentAbilityWithUpdatedTarget(UpdateCardModelUXEvent updateEvt)
	{
		if (updateEvt == null)
		{
			return false;
		}
		if (updateEvt.Property != PropertyType.Target)
		{
			return false;
		}
		MtgCardInstance newInstance = updateEvt.NewInstance;
		if (newInstance == null)
		{
			return false;
		}
		if (newInstance.ObjectType != GameObjectType.Ability)
		{
			return false;
		}
		MtgPlayer controller = newInstance.Controller;
		if (controller == null || controller.IsLocalPlayer)
		{
			return false;
		}
		return true;
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
