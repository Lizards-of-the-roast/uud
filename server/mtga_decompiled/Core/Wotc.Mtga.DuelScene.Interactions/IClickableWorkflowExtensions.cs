namespace Wotc.Mtga.DuelScene.Interactions;

public static class IClickableWorkflowExtensions
{
	public static bool CanClick(this WorkflowBase workflow, IEntityView entity, SimpleInteractionType clickType)
	{
		if (workflow is IClickableWorkflow clickableWorkflow)
		{
			return clickableWorkflow.CanClick(entity, clickType);
		}
		if (workflow is IParentWorkflow parentWorkflow)
		{
			foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
			{
				if (childWorkflow.CanClick(entity, clickType))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void OnClick(this WorkflowBase workflow, IEntityView entity, SimpleInteractionType clickType)
	{
		if (workflow is IClickableWorkflow clickableWorkflow)
		{
			clickableWorkflow.OnClick(entity, clickType);
		}
		else
		{
			if (!(workflow is IParentWorkflow parentWorkflow))
			{
				return;
			}
			foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
			{
				if (childWorkflow.CanClick(entity, clickType))
				{
					childWorkflow.OnClick(entity, clickType);
					break;
				}
			}
		}
	}

	public static bool CanClickStack(this WorkflowBase workflow, CdcStackCounterView entity, SimpleInteractionType clickType)
	{
		if (workflow is IClickableWorkflow clickableWorkflow)
		{
			return clickableWorkflow.CanClickStack(entity, clickType);
		}
		if (workflow is IParentWorkflow parentWorkflow)
		{
			foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
			{
				if (childWorkflow.CanClickStack(entity, clickType))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void OnClickStack(this WorkflowBase workflow, CdcStackCounterView entity, SimpleInteractionType clickType)
	{
		if (workflow is IClickableWorkflow clickableWorkflow)
		{
			clickableWorkflow.OnClickStack(entity);
		}
		else
		{
			if (!(workflow is IParentWorkflow parentWorkflow))
			{
				return;
			}
			foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
			{
				if (childWorkflow.CanClickStack(entity, clickType))
				{
					childWorkflow.OnClickStack(entity, clickType);
					break;
				}
			}
		}
	}

	public static void OnBattlefieldClick(this WorkflowBase workflow)
	{
		if (workflow is IClickableWorkflow clickableWorkflow)
		{
			clickableWorkflow.OnBattlefieldClick();
		}
		if (!(workflow is IParentWorkflow parentWorkflow))
		{
			return;
		}
		foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
		{
			childWorkflow.OnBattlefieldClick();
		}
	}
}
