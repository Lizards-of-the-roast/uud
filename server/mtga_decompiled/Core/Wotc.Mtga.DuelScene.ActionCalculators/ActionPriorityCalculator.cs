using System;
using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.ActionCalculators;

public class ActionPriorityCalculator : IActionPriorityCalculator
{
	private readonly HashSet<Wotc.Mtgo.Gre.External.Messaging.Action> _actionCache = new HashSet<Wotc.Mtgo.Gre.External.Messaging.Action>();

	public Wotc.Mtgo.Gre.External.Messaging.Action GetPrioritizedAction(IAbilityDataProvider abilityDataProvider, IReadOnlyCollection<GreInteraction> interactions, IReadOnlyCollection<ActionInfo> gsActions, IListFilter<Wotc.Mtgo.Gre.External.Messaging.Action> typeFilter)
	{
		_actionCache.Clear();
		foreach (GreInteraction item in (IEnumerable<GreInteraction>)(((object)interactions) ?? ((object)Array.Empty<GreInteraction>())))
		{
			if (item != null && item.GreAction != null && item.IsActive)
			{
				_actionCache.Add(item.GreAction);
			}
		}
		foreach (ActionInfo item2 in (IEnumerable<ActionInfo>)(((object)gsActions) ?? ((object)Array.Empty<ActionInfo>())))
		{
			if (item2 != null && item2.Action != null)
			{
				_actionCache.Add(item2.Action);
			}
		}
		return ManaUtilities.GetActionWithLowestCost(abilityDataProvider, _actionCache, typeFilter, NullListFilter<Wotc.Mtgo.Gre.External.Messaging.Action>.Default);
	}
}
