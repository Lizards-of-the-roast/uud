using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions;

public interface IWorkflowTranslator
{
	WorkflowBase Translate(BaseUserRequest req);
}
