using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions;

public class NullWorkflowTranslation<T> : IWorkflowTranslation<T> where T : BaseUserRequest
{
	public static readonly IWorkflowTranslation<T> Default = new NullWorkflowTranslation<T>();

	public WorkflowBase Translate(T req)
	{
		return null;
	}
}
public class NullWorkflowTranslation : IWorkflowTranslation<BaseUserRequest>
{
	public static readonly IWorkflowTranslation<BaseUserRequest> Default = new NullWorkflowTranslation();

	public WorkflowBase Translate(BaseUserRequest req)
	{
		return new NullWorkflow(req);
	}
}
