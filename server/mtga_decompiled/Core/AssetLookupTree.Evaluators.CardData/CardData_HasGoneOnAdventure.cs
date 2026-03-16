using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasGoneOnAdventure : EvaluatorBase_Boolean
{
	private bool hasGoneOnAdventure(ICardDataAdapter model)
	{
		if (model == null)
		{
			return false;
		}
		if (model.ZoneType != ZoneType.Exile)
		{
			return false;
		}
		if (!model.IsAdventureCard())
		{
			return false;
		}
		foreach (ActionInfo action in model.Actions)
		{
			if (action != null && action.Action?.ActionType == ActionType.CastAdventure)
			{
				return false;
			}
		}
		return true;
	}

	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, hasGoneOnAdventure(bb.CardData));
	}
}
