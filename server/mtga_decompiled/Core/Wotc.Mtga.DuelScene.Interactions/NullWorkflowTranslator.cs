using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions;

public class NullWorkflowTranslator : IWorkflowTranslator
{
	public static readonly IWorkflowTranslator Default = new NullWorkflowTranslator();

	public WorkflowBase Translate(BaseUserRequest req)
	{
		return null;
	}
}
