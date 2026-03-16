using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.ActionCalculators;

public interface IActionPriorityCalculator
{
	Action GetPrioritizedAction(IAbilityDataProvider abilityDataProvider, IReadOnlyCollection<GreInteraction> interactions, IReadOnlyCollection<ActionInfo> gsActions, IListFilter<Action> typeFilter);
}
