using System.Collections.Generic;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Indirectors.Interaction.ActionsAvailable;

public class Interaction_ActionsAvailable_AllActiveActions : ActionIndirector
{
	public override IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (!(bb.Interaction is ActionsAvailableWorkflow actionsAvailableWorkflow))
		{
			yield break;
		}
		IReadOnlyList<GreInteraction> allActions = actionsAvailableWorkflow.GetAllActions();
		if (allActions == null || allActions.Count <= 0)
		{
			yield break;
		}
		foreach (GreInteraction item in allActions)
		{
			if (item.IsActive)
			{
				bb.GreAction = item.GreAction;
				bb.GreActionType = item.Type;
				yield return bb;
			}
		}
	}
}
