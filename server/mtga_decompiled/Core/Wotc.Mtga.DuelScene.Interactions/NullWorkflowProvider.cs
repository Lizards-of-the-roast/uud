namespace Wotc.Mtga.DuelScene.Interactions;

public class NullWorkflowProvider : IWorkflowProvider
{
	public static readonly IWorkflowProvider Default = new NullWorkflowProvider();

	public WorkflowBase GetCurrentWorkflow()
	{
		return null;
	}

	public WorkflowBase GetPendingWorkflow()
	{
		return null;
	}
}
