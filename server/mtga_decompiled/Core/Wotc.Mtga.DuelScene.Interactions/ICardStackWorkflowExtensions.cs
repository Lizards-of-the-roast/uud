using GreClient.CardData;

namespace Wotc.Mtga.DuelScene.Interactions;

public static class ICardStackWorkflowExtensions
{
	public static bool CanStack(this WorkflowBase workflow, ICardDataAdapter lhs, ICardDataAdapter rhs)
	{
		if (workflow is ICardStackWorkflow cardStackWorkflow && !cardStackWorkflow.CanStack(lhs, rhs))
		{
			return false;
		}
		if (workflow is IParentWorkflow parentWorkflow)
		{
			foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
			{
				if (!childWorkflow.CanStack(lhs, rhs))
				{
					return false;
				}
			}
		}
		return true;
	}
}
