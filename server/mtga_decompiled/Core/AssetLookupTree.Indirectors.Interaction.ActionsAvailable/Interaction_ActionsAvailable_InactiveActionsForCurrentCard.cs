using System.Collections.Generic;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Indirectors.Interaction.ActionsAvailable;

public class Interaction_ActionsAvailable_InactiveActionsForCurrentCard : ActionIndirector
{
	public override IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.CardData == null || bb.CardData.InstanceId == 0 || !(bb.Interaction is ActionsAvailableWorkflow actionsAvailableWorkflow))
		{
			yield break;
		}
		IReadOnlyList<GreInteraction> actionsForId = actionsAvailableWorkflow.GetActionsForId(bb.CardData.InstanceId);
		if (actionsForId == null || actionsForId.Count <= 0)
		{
			yield break;
		}
		foreach (GreInteraction item in actionsForId)
		{
			if (!item.IsActive)
			{
				bb.GreAction = item.GreAction;
				bb.GreActionType = item.Type;
				yield return bb;
			}
		}
	}
}
