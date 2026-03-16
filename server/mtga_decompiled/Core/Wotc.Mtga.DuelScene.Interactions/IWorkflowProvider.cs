namespace Wotc.Mtga.DuelScene.Interactions;

public interface IWorkflowProvider
{
	WorkflowBase GetCurrentWorkflow();

	WorkflowBase GetPendingWorkflow();
}
